using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Manisero.DataflowPoc.Core.Extensions
{
    public static class TaskExtensions
    {
        public static Task ContinueWithStatusPropagation(this Task task, Action<Task> continuationAction)
        {
            var completionSource = new TaskCompletionSource<object>();

            task.ContinueWith(x =>
            {
                var exceptions = new List<Exception>(2);

                if (x.IsFaulted)
                {
                    exceptions.Add(x.Exception);
                }

                try
                {
                    continuationAction(x);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }

                if (exceptions.Count != 0)
                {
                    completionSource.SetException(new AggregateException(exceptions));
                }
                else if (x.IsCanceled)
                {
                    completionSource.SetCanceled();
                }
                else
                {
                    completionSource.SetResult(null);
                }
            });

            return completionSource.Task;
        }

        public static Task CreateGlobalCompletion(IEnumerable<Task> tasks, CancellationTokenSource cancellationSource)
        {
            var faultHandlers = tasks.Select(x => x.ContinueWithStatusPropagation(
                t =>
                {
                    if (t.IsFaulted && !cancellationSource.IsCancellationRequested)
                    {
                        cancellationSource.Cancel();
                    }
                }));

            return Task.WhenAll(faultHandlers);
        }
    }
}

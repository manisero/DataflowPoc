using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dataflow.Extensions
{
    public static class TaskExtensions
    {
        public static Task ContinueWithStatusPropagation(this Task task, Action<Task> continuationAction)
        {
            var completionSource = new TaskCompletionSource<object>();

            task.ContinueWith(x =>
                                  {
                                      continuationAction(x);

                                      if (x.IsFaulted)
                                      {
                                          try
                                          {
                                              throw x.Exception;
                                          }
                                          catch (Exception e)
                                          {
                                              completionSource.SetException(e);
                                          }
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

        public static Task CreateGlobalCompletion(CancellationTokenSource cancellationSource, params Task[] tasks)
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

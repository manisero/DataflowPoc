using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Manisero.DataflowPoc.Extensions
{
    public static class DataflowExtensions
    {
        public static IDisposable LinkWithCompletion<TData>(this ISourceBlock<TData> source, ITargetBlock<TData> target, Predicate<TData> predicate = null)
        {
            return predicate != null
                       ? source.LinkTo(target, new DataflowLinkOptions { PropagateCompletion = true }, predicate)
                       : source.LinkTo(target, new DataflowLinkOptions { PropagateCompletion = true });
        }

        public static void DeriveCompletionOrFaultFrom<TData>(this ITargetBlock<TData> target, params ISourceBlock<TData>[] sources) => target.DeriveCompletionOrFaultFrom((IEnumerable<ISourceBlock<TData>>)sources);

        public static void DeriveCompletionOrFaultFrom<TData>(this ITargetBlock<TData> target, IEnumerable<ISourceBlock<TData>> sources)
        {
            Task.WhenAll(sources.Select(x => x.Completion))
                .ContinueWith(x =>
                                  {
                                      if (x.IsFaulted)
                                      {
                                          target.Fault(x.Exception);
                                      }
                                      else
                                      {
                                          target.Complete();
                                      }
                                  });
        }

        public static IDisposable IgnoreOutput<TData>(this ISourceBlock<TData> source)
        {
            var terminateBlock = DataflowBlock.NullTarget<TData>(); // Never completes

            return source.LinkWithCompletion(terminateBlock);
        }
    }
}

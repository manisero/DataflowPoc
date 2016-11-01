using System;
using System.Threading.Tasks.Dataflow;

namespace Dataflow.Extensions
{
    public static class DataflowExtensions
    {
        public static IDisposable LinkWithCompletion<TData>(this ISourceBlock<TData> source, ITargetBlock<TData> target, Predicate<TData> predicate = null)
        {
            return predicate != null
                       ? source.LinkTo(target, new DataflowLinkOptions { PropagateCompletion = true }, predicate)
                       : source.LinkTo(target, new DataflowLinkOptions { PropagateCompletion = true });
        }

        public static IDisposable IgnoreOutput<TData>(this ISourceBlock<TData> source)
        {
            var terminateBlock = DataflowBlock.NullTarget<TData>(); // Never completes

            return source.LinkWithCompletion(terminateBlock);
        }
    }
}

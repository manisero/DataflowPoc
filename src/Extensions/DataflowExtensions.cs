using System;
using System.Threading.Tasks.Dataflow;

namespace Dataflow.Extensions
{
    public static class DataflowExtensions
    {
        public static IDisposable LinkWithCompletion<TData>(this ISourceBlock<TData> source, ITargetBlock<TData> target)
        {
            return source.LinkTo(target, new DataflowLinkOptions { PropagateCompletion = true });
        }
    }
}

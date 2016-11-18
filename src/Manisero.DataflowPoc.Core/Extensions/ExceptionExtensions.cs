using System;

namespace Manisero.DataflowPoc.Core.Extensions
{
    public static class ExceptionExtensions
    {
        public static Exception FlattenIfAggregate(this Exception exception) => (exception as AggregateException)?.Flatten() ?? exception;
    }
}

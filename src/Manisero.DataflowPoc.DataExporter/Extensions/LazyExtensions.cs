using System;

namespace Manisero.DataflowPoc.DataExporter.Extensions
{
    public static class LazyExtensions
    {
        public static TValue ValueIfCreated<TValue>(this Lazy<TValue> lazy)
            where TValue : class
            => lazy.IsValueCreated ? lazy.Value : null;
    }
}

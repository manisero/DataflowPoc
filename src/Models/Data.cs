using System;
using System.Collections.Generic;
using System.Threading;
using Dataflow.Etw;
using Dataflow.Extensions;

namespace Dataflow.Models
{
    public class Data : IDisposable
    {
        private static int _instancesCount;
        private static int _livingInstancesCount;

        public static readonly Func<Data, int> IdGetter = x => x.Id;

        public Data()
        {
            Id = Interlocked.Increment(ref _instancesCount);
            var living = Interlocked.Increment(ref _livingInstancesCount);
            Events.Write.DataCreation(Id, living);
        }

        public void Dispose()
        {
            var living = Interlocked.Decrement(ref _livingInstancesCount);
            Events.Write.DataDisposal(Id, living);
        }

        public int Id { get; }

        public string PersonJson { get; set; }

        public Person Person { get; set; }

        public bool IsValid => Error == null;

        public string Error { get; set; }

        public override string ToString()
        {
            return Error ?? Person.ToString();
        }
    }

    public static class DataExtensions
    {
        public static DataBatch ToBatch(this ICollection<Data> data) => new DataBatch { Data = data };
    }

    public class DataBatch : IDisposable
    {
        private static int _instancesCount;
        public static readonly Func<DataBatch, int> IdGetter = x => x.Id;

        public DataBatch()
        {
            Id = Interlocked.Increment(ref _instancesCount);
        }

        public void Dispose()
        {
            Data.ForEach(x => x.Dispose());
            Data.Clear();
        }

        public int Id { get; }

        public int IntendedSize { get; set; }

        public ICollection<Data> Data { get; set; }
    }
}

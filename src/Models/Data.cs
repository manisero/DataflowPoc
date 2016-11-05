using System;
using System.Collections.Generic;
using System.Threading;

namespace Dataflow.Models
{
    public class Data
    {
        private static int _instancesCount;
        public static readonly Func<Data, int> IdGetter = x => x.Id;

        public Data()
        {
            Id = Interlocked.Increment(ref _instancesCount);
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

    public class DataBatch
    {
        private static int _instancesCount;
        public static readonly Func<DataBatch, int> IdGetter = x => x.Id;

        public DataBatch()
        {
            Id = Interlocked.Increment(ref _instancesCount);
        }

        public int Id { get; }

        public ICollection<Data> Data { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using Manisero.DataflowPoc.Core.Etw;

namespace Manisero.DataflowPoc.Playground.Models
{
    public class Data
    {
        private static int _instancesCount;

        public static readonly Func<Data, int> IdGetter = x => x.Id;

        public Data()
        {
            Id = Interlocked.Increment(ref _instancesCount);
            Events.Write.DataCreation(Id);
        }

        public int Id { get; }

        public string PersonJson { get; set; }

        public Person Person { get; set; } = new Person();

        public bool IsValid => Error == null;

        public string Error { get; set; }

        public override string ToString()
        {
            return Error ?? Person.ToString();
        }
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

        public int IntendedSize { get; set; }

        public ICollection<Data> Data { get; set; }
    }
}

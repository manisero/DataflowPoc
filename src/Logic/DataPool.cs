using System.Collections.Generic;
using Dataflow.Models;

namespace Dataflow.Logic
{
    public class DataPool
    {
        public static DataPool Current { get; private set; }

        public DataPool()
        {
            Current = this;
        }

        private readonly Queue<Data> _pool = new Queue<Data>(50000);

        public Data Rent()
        {
            return _pool.Count != 0
                       ? _pool.Dequeue()
                       : new Data();
        }

        public void Return(Data data)
        {
            data.PersonJson = null;
            data.Person = null;
            data.Error = null;

            _pool.Enqueue(data);
        }

        public int GetPooledCound() => _pool.Count;
    }
}

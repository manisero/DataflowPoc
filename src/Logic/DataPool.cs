using System.Collections.Concurrent;
using Manisero.DataflowPoc.Models;

namespace Manisero.DataflowPoc.Logic
{
    public class DataPool
    {
        public static DataPool Current { get; private set; }

        public DataPool()
        {
            Current = this;
        }

        private readonly ConcurrentBag<Data> _pool = new ConcurrentBag<Data>();

        public Data Rent()
        {
            Data data;

            return _pool.TryTake(out data)
                       ? data
                       : new Data();
        }

        public void Return(Data data)
        {
            if (!Settings.EnableDataPooling)
            {
                return;
            }

            data.PersonJson = null;
            data.Error = null;

            _pool.Add(data);
        }

        public int GetPooledCound() => _pool.Count;
    }
}

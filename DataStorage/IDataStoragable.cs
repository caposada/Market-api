using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStorage
{
    public interface IDataStoragable<T> where T : new()
    {
        public DataStorage<T>? Store { get; set; }

        public abstract void Destroy();

    }
}

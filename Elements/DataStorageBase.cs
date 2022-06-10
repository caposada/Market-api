using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements
{
    public class DataStorageBase<T> where T : StoreBase
    {
        public string? FileName { get; set; }
        public string? FolderName { get; set; }
        public string? PathPrefix { get; set; }

        public DataStorageBase(T store)
        {
            this.FileName = store.GetFilename();
            this.FolderName = store.GetFolderName();
            this.PathPrefix = store.GetPathPrefix();
        }
    }
}

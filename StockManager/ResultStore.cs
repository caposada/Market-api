using Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManager
{
    public class ResultStore<T> : StoreBase
    {
        public T? Result { get; set; }

        private string? folderName;

        public ResultStore()
        {
        }

        public ResultStore(string? folderName)
        {
            this.folderName = folderName;
        }

        public override string GetFilename()
        {
            return "Result";
        }

        public override string? GetFolderName()
        {
            return folderName;
        }

        public override string GetPathPrefix()
        {
            return Constants.MARKETDATA_REQUEST_FOLDER_NAME;
        }
    }
}

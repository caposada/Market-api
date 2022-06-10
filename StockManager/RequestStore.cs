using DataStorage;
using Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManager
{
    public class RequestStore<T> : StoreBase
    {
        public T? Result { get; set; }
        public string? Symbol { get; set; }
        public DateTime ValidUntil { get; set; }
        public DateTime TimeStamp { get; set; }

        private string? folderName;

        public RequestStore()
        {
        }

        public RequestStore(string folderName)
        {
            this.folderName = folderName;
        }

        public override string GetFilename()
        {
            return "Request";
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

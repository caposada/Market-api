using Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManager
{
    public class MarketDataLoggerStore : StoreBase
    {
        public class RequestRecord
        {
            public DateTime Date { get; set; }
            public int Count { get; set; }

        }

        public List<RequestRecord> RequestRecords { get; set; }

        public MarketDataLoggerStore()
        {
            this.RequestRecords = new List<RequestRecord>();
        }

        public override string GetFilename()
        {
            return "Logger_MarketData";
        }

        public override string? GetFolderName()
        {
            return null;
        }

        public override string GetPathPrefix()
        {
            return Constants.APP_SETTINGS_FOLDER_NAME;
        }
    }
}

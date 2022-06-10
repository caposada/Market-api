using Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StockManager
{
    public class MarketDataLogger : IDataStoragable<MarketDataLoggerStore>
    {
        public int TodaysCount
        {
            get
            {
                return GetTodaysRequestRecord().Count;
            }
        }

        [JsonIgnore]
        public DataStorage<MarketDataLoggerStore>? Store { get; set; }

        public MarketDataLogger()
        {
            this.Store = new DataStorage<MarketDataLoggerStore>(new MarketDataLoggerStore());
            this.Store.Load();
        }

        public void IncrementCount()
        {
            var requestRecord = GetTodaysRequestRecord();
            requestRecord.Count++;
            Store.Save();
        }

        private MarketDataLoggerStore.RequestRecord GetTodaysRequestRecord()
        {
            DateTime today = DateTime.Today;
            var requestRecord = Store.Data.RequestRecords.Find(x => x.Date == today);
            if (requestRecord == null)
            {
                requestRecord = new MarketDataLoggerStore.RequestRecord()
                {
                    Count = 0,
                    Date = today
                };
                Store.Data.RequestRecords.Add(requestRecord);
                Store.Save();
            }
            return requestRecord;
        }

        public void Destroy()
        {
            throw new NotImplementedException();
        }
    }
}

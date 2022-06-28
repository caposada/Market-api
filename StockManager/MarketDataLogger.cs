namespace StockManager
{
    public class MarketDataLogger
    {
        public int TodaysCount
        {
            get
            {
                return GetTodaysRequestRecord().Count;
            }
        }

        public MarketDataLogger()
        {
        }

        public void IncrementCount()
        {
            using (StockMarketContext context = new StockMarketContext())
            {
                DateTime today = DateTime.Today;
                var requestRecord = context.RequestRecords.Where(x => x.Date == today).FirstOrDefault();
                if (requestRecord == null)
                {
                    requestRecord = new RequestRecord()
                    {
                        Count = 0,
                        Date = today
                    };
                    context.RequestRecords.Add(requestRecord);
                }
                requestRecord.Count++;
                context.SaveChanges();
            }
        }

        private RequestRecord GetTodaysRequestRecord()
        {
            using (StockMarketContext context = new StockMarketContext())
            {
                DateTime today = DateTime.Today;
                var requestRecord = context.RequestRecords.Where(x => x.Date == today).FirstOrDefault();
                if (requestRecord == null)
                {
                    requestRecord = new RequestRecord()
                    {
                        Count = 0,
                        Date = today
                    };
                    context.RequestRecords.Add(requestRecord);
                    context.SaveChanges();
                }
                return requestRecord;
            }
        }

    }
}

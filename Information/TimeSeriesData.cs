using Elements;

namespace Information
{

    public class TimeSeriesData
    {
        public class Store : StoreBase
        {
            public List<StockTimeSeriesResult>? StockTimeSeriesResults { get; set; }

            private string folderName;

            public Store()
            {
            }

            public Store(string folderName)
            {
                this.folderName = folderName;
            }

            public override string GetFilename()
            {
                return "TimeSerieses";
            }

            public override string GetFolderName()
            {
                return folderName;
            }

            public override string GetPathPrefix()
            {
                return Constants.GATHERER_FOLDER_NAME;
            }
        }

        public Guid Id { get; set; }
        public List<TimeSeries> TimeSerieses
        {
            get
            {
                if (timeSerieses != null)
                    return timeSerieses;

                if (store.Data.StockTimeSeriesResults == null)
                    store.Load();

                if (store.Data.StockTimeSeriesResults == null)
                    store.Data.StockTimeSeriesResults = new List<StockTimeSeriesResult>();

                timeSerieses = new List<TimeSeries>();
                foreach (var stockTimeSeriesResult in store.Data.StockTimeSeriesResults)
                {
                    AddTimeSeries(stockTimeSeriesResult);
                }
                return timeSerieses;
            }
        }
        public bool Exists
        {
            get
            {
                return store.Exists();
            }
        }
        public DateTimeOffset PublishedDate { get; private set; }

        private DataStorage<Store> store;
        private List<TimeSeries> timeSerieses;

        public TimeSeriesData(Guid id, DateTimeOffset publishedDate)
        {
            this.PublishedDate = publishedDate;
            this.Id = id;
            store = new DataStorage<Store>(new Store(this.Id.ToString()));
        }

        public void Add(StockTimeSeriesResult stockTimeSeriesResult)
        {
            if (store.Data.StockTimeSeriesResults == null)
                store.Load();

            if (store.Data.StockTimeSeriesResults == null)
                store.Data.StockTimeSeriesResults = new List<StockTimeSeriesResult>();

            store.Data.StockTimeSeriesResults.Add(stockTimeSeriesResult);
            store.Save();

            AddTimeSeries(stockTimeSeriesResult);
        }

        private void AddTimeSeries(StockTimeSeriesResult stockTimeSeriesResult)
        {
            TimeSeries timeSeries = new TimeSeries(PublishedDate, stockTimeSeriesResult);
            timeSeries.ParseData();
            timeSerieses.Add(timeSeries);

        }
    }
}

using DataStorage;
using Elements;
using System.Text.Json.Serialization;

namespace Information
{    

    public class TimeSeriesData : IDataStoragable<TimeSeriesesStore>
    {
        public Guid Id { get; set; }
        public List<TimeSeries> TimeSerieses
        {
            get
            {
                if (timeSerieses != null)
                    return timeSerieses;

                if (Store.Data.StockTimeSeriesResults == null)
                    Store.Load();

                if (Store.Data.StockTimeSeriesResults == null)
                    Store.Data.StockTimeSeriesResults = new List<StockTimeSeriesResult>();

                timeSerieses = new List<TimeSeries>();
                foreach (var stockTimeSeriesResult in Store.Data.StockTimeSeriesResults)
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
                return Store.Exists();
            }
        }
        public DateTimeOffset PublishedDate { get; private set; }

        [JsonIgnore]
        public DataStorage<TimeSeriesesStore>? Store { get; set; }

        private List<TimeSeries> timeSerieses;

        public TimeSeriesData(Guid id, DateTimeOffset publishedDate)
        {
            this.PublishedDate = publishedDate;
            this.Id = id;
            Store = new DataStorage<TimeSeriesesStore>(new TimeSeriesesStore(this.Id.ToString()));
        }

        public void Add(StockTimeSeriesResult stockTimeSeriesResult)
        {
            if (Store.Data.StockTimeSeriesResults == null)
                Store.Load();

            if (Store.Data.StockTimeSeriesResults == null)
                Store.Data.StockTimeSeriesResults = new List<StockTimeSeriesResult>();

            Store.Data.StockTimeSeriesResults.Add(stockTimeSeriesResult);
            Store.Save();

            AddTimeSeries(stockTimeSeriesResult);
        }

        private void AddTimeSeries(StockTimeSeriesResult stockTimeSeriesResult)
        {
            TimeSeries timeSeries = new TimeSeries(PublishedDate, stockTimeSeriesResult);
            timeSeries.ParseData();
            timeSerieses.Add(timeSeries);

        }

        public void Destroy()
        {
            throw new NotImplementedException();
        }
    }
}

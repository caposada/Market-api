using AlphaVantage.Net.Common.Intervals;
using AlphaVantage.Net.Stocks;

namespace Elements
{
    public class TimeSeries // Wrapper for StockTimeSeriesResult
    {
        public DateTimeOffset PublishedDate { get; private set; }
        public string? Symbol
        {
            get
            {
                return stockTimeSeriesResult.Symbol;
            }
        }
        public DateTime ValidUntil
        {
            get
            {
                return stockTimeSeriesResult.ValidUntil;
            }
        }
        public DateTime TimeStamp
        {
            get
            {
                return stockTimeSeriesResult.TimeStamp;
            }
        }
        public Interval Interval
        {
            get
            {
                return stockTimeSeriesResult.Interval;
            }
        }
        public List<DataPoint>? DataPoints { get; private set; }
        public decimal Min { get; private set; }
        public decimal Max { get; private set; }
        public DateTime From { get; private set; }
        public DateTime To { get; private set; }
        public decimal MinChangePercent { get; private set; }
        public decimal MaxChangePercent { get; private set; }
        public PriceChangeVolatility LowVolatility { get; private set; }
        public PriceChangeVolatility HighVolatility { get; private set; }

        private StockTimeSeriesResult stockTimeSeriesResult;

        public TimeSeries(DateTimeOffset publishedDate, StockTimeSeriesResult stockTimeSeriesResult)
        {
            this.stockTimeSeriesResult = stockTimeSeriesResult;
            this.PublishedDate = publishedDate;
        }

        public void ParseData()
        {
            Task.Run(() =>
            {
                this.DataPoints = new List<DataPoint>();
                this.From = DateTime.MaxValue;
                this.To = DateTime.MinValue;
                this.Min = int.MaxValue;
                this.Max = int.MinValue;
                this.MinChangePercent = int.MaxValue;
                this.MaxChangePercent = int.MinValue;

                List<StockDataPoint>? stockDataPoints = stockTimeSeriesResult?.Result?.DataPoints.ToList();
                if (stockDataPoints != null)
                {
                    foreach (var stockDataPoint in stockDataPoints)
                    {
                        DataPoint dataPoint = new DataPoint(stockDataPoint);
                        this.DataPoints.Add(dataPoint);
                        if (stockDataPoint.ClosingPrice < this.Min)
                            this.Min = stockDataPoint.ClosingPrice;
                        if (stockDataPoint.ClosingPrice > this.Max)
                            this.Max = stockDataPoint.ClosingPrice;
                        if (stockDataPoint.Time < this.From)
                            this.From = stockDataPoint.Time;
                        if (stockDataPoint.Time > this.To)
                            this.To = stockDataPoint.Time;
                        if (dataPoint.ChangePercent < this.MinChangePercent)
                            this.MinChangePercent = dataPoint.ChangePercent;
                        if (dataPoint.ChangePercent > this.MaxChangePercent)
                            this.MaxChangePercent = dataPoint.ChangePercent;
                    }

                    RateVolatility();
                    GetVectors();
                }
            });
        }

        public void GetVectors()
        {
            // Get points before publish data
            var fiveHourBeforeDataPoint = DataPoints.FirstOrDefault(x => x.Time < this.PublishedDate.AddHours(-5), DataPoints.Last());
            var twoHourBeforeDataPoint = DataPoints.FirstOrDefault(x => x.Time < this.PublishedDate.AddHours(-2), DataPoints.Last());
            var oneHourBeforeDataPoint = DataPoints.FirstOrDefault(x => x.Time < this.PublishedDate.AddHours(-1), DataPoints.Last());

            // Get point at publish date
            var publishedDataPoint = DataPoints.FirstOrDefault(x => x.Time < this.PublishedDate, null);

            // Get points after publish date
            var oneHourAfterDataPoint = DataPoints.FirstOrDefault(x => x.Time < this.PublishedDate.AddHours(1), DataPoints.First());
            var twoHourAfterDataPoint = DataPoints.FirstOrDefault(x => x.Time < this.PublishedDate.AddHours(2), DataPoints.First());
            var fiveHourAfterDataPoint = DataPoints.FirstOrDefault(x => x.Time < this.PublishedDate.AddHours(5), DataPoints.First());

        }

        private void RateVolatility()
        {
            switch (this.MaxChangePercent)
            {
                case > 10: HighVolatility = PriceChangeVolatility.EXTREME; break;
                case > 5: HighVolatility = PriceChangeVolatility.SIGNIFICANT; break;
                case > 2: HighVolatility = PriceChangeVolatility.HIGH; break;
                case > 1: HighVolatility = PriceChangeVolatility.NORMAL; break;
                default: HighVolatility = PriceChangeVolatility.LOW; break;
            }
            switch (this.MinChangePercent)
            {
                case < -10: LowVolatility = PriceChangeVolatility.EXTREME; break;
                case < -5: LowVolatility = PriceChangeVolatility.SIGNIFICANT; break;
                case < -2: LowVolatility = PriceChangeVolatility.HIGH; break;
                case < -1: LowVolatility = PriceChangeVolatility.NORMAL; break;
                default: LowVolatility = PriceChangeVolatility.LOW; break;
            }
        }
    }
}

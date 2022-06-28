using AlphaVantage.Net.Common.Intervals;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Elements
{
    public class TimeSeries // Wrapper for StockTimeSeriesResult
    {
        [Key]
        public Guid Id { get; set; }
        public Guid NewsItemId { get; set; }
        public DateTimeOffset PublishedDate { get; set; }
        public string Symbol { get; set; }
        public DateTime ValidUntil { get; set; }
        public DateTime TimeStamp { get; set; }
        public Interval Interval { get; set; }
        public string DataPointsJson { get; set; }
        [NotMapped]
        public decimal Min { get; set; }
        [NotMapped]
        public decimal Max { get; set; }
        [NotMapped]
        public DateTime From { get; set; }
        [NotMapped]
        public DateTime To { get; set; }
        [NotMapped]
        public decimal MinChangePercent { get; set; }
        [NotMapped]
        public decimal MaxChangePercent { get; set; }
        [NotMapped]
        public PriceChangeVolatility LowVolatility { get; set; }
        [NotMapped]
        public PriceChangeVolatility HighVolatility { get; set; }
        [NotMapped]
        public List<DataPoint> DataPoints
        {
            get
            {
                if (dataPoint == null)
                    dataPoint = JsonSerializer.Deserialize<List<DataPoint>>(DataPointsJson);
                return dataPoint;
            }
        }

        private List<DataPoint> dataPoint;

        public TimeSeries(
            Guid id,
            Guid newsItemId,
            DateTimeOffset publishedDate,
            string symbol,
            DateTime validUntil,
            DateTime timeStamp,
            Interval interval,
            string dataPointsJson)
        {
            Id = id;
            NewsItemId = newsItemId;
            PublishedDate = publishedDate;
            Symbol = symbol;
            ValidUntil = validUntil;
            TimeStamp = timeStamp;
            Interval = interval;
            DataPointsJson = dataPointsJson;

            ParseData();
        }

        public TimeSeries(Guid newsItemId, DateTimeOffset publishedDate, StockTimeSeriesResult stockTimeSeriesResult)
        {
            Id = Guid.NewGuid();
            NewsItemId = newsItemId;
            PublishedDate = publishedDate;
            Symbol = stockTimeSeriesResult.Symbol;
            ValidUntil = stockTimeSeriesResult.ValidUntil;
            TimeStamp = stockTimeSeriesResult.TimeStamp;
            Interval = stockTimeSeriesResult.Interval;

            dataPoint = new List<DataPoint>();
            foreach (var stockDataPoint in stockTimeSeriesResult?.Result?.DataPoints.ToList())
            {
                DataPoint dataPoint = new DataPoint(stockDataPoint);
                this.DataPoints.Add(dataPoint);
            }

            DataPointsJson = JsonSerializer.Serialize<List<DataPoint>>(dataPoint);

            ParseData();
        }

        private void ParseData()
        {
            Task.Run(() =>
            {
                this.From = DateTime.MaxValue;
                this.To = DateTime.MinValue;
                this.Min = int.MaxValue;
                this.Max = int.MinValue;
                this.MinChangePercent = int.MaxValue;
                this.MaxChangePercent = int.MinValue;

                foreach (var dataPoint in DataPoints)
                {
                    if (dataPoint.ClosingPrice < this.Min)
                        this.Min = dataPoint.ClosingPrice;
                    if (dataPoint.ClosingPrice > this.Max)
                        this.Max = dataPoint.ClosingPrice;
                    if (dataPoint.Time < this.From)
                        this.From = dataPoint.Time;
                    if (dataPoint.Time > this.To)
                        this.To = dataPoint.Time;
                    if (dataPoint.ChangePercent < this.MinChangePercent)
                        this.MinChangePercent = dataPoint.ChangePercent;
                    if (dataPoint.ChangePercent > this.MaxChangePercent)
                        this.MaxChangePercent = dataPoint.ChangePercent;
                }

                RateVolatility();
                GetVectors();

            });
        }

        private void GetVectors()
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

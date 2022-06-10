using AlphaVantage.Net.Common.Intervals;
using AlphaVantage.Net.Stocks;
using System.Text.Json.Serialization;

namespace Elements
{
    public class StockTimeSeriesResult
    {
        public string? Symbol { get; set; }
        public DateTime ValidUntil { get; set; }
        public DateTime TimeStamp { get; set; }
        public Interval Interval { get; set; }
        public StockTimeSeries? Result { get; set; }

        public StockTimeSeriesResult(string symbol, DateTime validUntil, DateTime timestamp, Interval interval, StockTimeSeries result)
        {
            this.Symbol = symbol;
            this.ValidUntil = validUntil;
            this.TimeStamp = timestamp;
            this.Interval = interval;
            this.Result = result;
        }

        [JsonConstructor]
        public StockTimeSeriesResult()
        {
        }

        public static DateTime CalculatedValidUntilDateTime(Interval interval)
        {
            DateTime validUntil = DateTime.Now;
            switch (interval)
            {
                case Interval.Min1:
                    validUntil = validUntil.AddMinutes(1);
                    break;
                case Interval.Min5:
                    validUntil = validUntil.AddMinutes(5);
                    break;
                case Interval.Min15:
                    validUntil = validUntil.AddMinutes(15);
                    break;
                case Interval.Min30:
                    validUntil = validUntil.AddMinutes(30);
                    break;
                case Interval.Min60:
                    validUntil = validUntil.AddHours(1);
                    break;
                case Interval.Daily:
                    validUntil = validUntil.AddDays(1);
                    break;
                case Interval.Weekly:
                    validUntil = validUntil.AddDays(7);
                    break;
                case Interval.Monthly:
                    validUntil = validUntil.AddMonths(1);
                    break;
            }
            return validUntil;
        }

    }
}

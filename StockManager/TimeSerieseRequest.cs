using AlphaVantage.Net.Common.Intervals;

namespace StockManager
{

    public class TimeSerieseRequest
    {
        public string Symbol { get; set; }
        public Interval Interval { get; set; }

        public TimeSerieseRequest(string symbol, Interval interval)
        {
            Symbol = symbol;
            Interval = interval;
        }
    }
}

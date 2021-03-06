using AlphaVantage.Net.Stocks;

namespace Elements
{
    public class DataPoint
    {
        public DateTime Time { get; set; }
        public decimal OpeningPrice { get; set; }
        public decimal ClosingPrice { get; set; }
        public decimal HighestPrice { get; set; }
        public decimal LowestPrice { get; set; }
        public long Volume { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercent { get; set; }

        public DataPoint()
        {
        }

        public DataPoint(DateTime time, decimal openingPrice, decimal closingPrice, decimal highestPrice, decimal lowestPrice, long volume, decimal change, decimal changePercent)
        {
            Time = time;
            OpeningPrice = openingPrice;
            ClosingPrice = closingPrice;
            HighestPrice = highestPrice;
            LowestPrice = lowestPrice;
            Volume = volume;
            Change = change;
            ChangePercent = changePercent;
        }

        public DataPoint(StockDataPoint stockDataPoint)
        {
            this.Time = stockDataPoint.Time;
            this.OpeningPrice = stockDataPoint.OpeningPrice;
            this.ClosingPrice = stockDataPoint.ClosingPrice;
            this.HighestPrice = stockDataPoint.HighestPrice;
            this.LowestPrice = stockDataPoint.LowestPrice;
            this.Volume = stockDataPoint.Volume;

            this.Change = this.ClosingPrice - this.OpeningPrice;
            this.ChangePercent = this.Change / this.OpeningPrice * 100; // (Close - Open) / Open * 100
        }
    }
}

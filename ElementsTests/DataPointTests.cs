using AlphaVantage.Net.Stocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Elements.Tests
{
    [TestClass()]
    public class DataPointTests
    {
        [TestMethod()]
        public void DataPointTest()
        {
            StockDataPoint stockDataPoint = new StockDataPoint()
            {
                OpeningPrice = 102,
                ClosingPrice = 104,
                HighestPrice = 106,
                LowestPrice = 100,
                Time = new DateTime(2022, 01, 01, 10, 0, 0)
            };
            DataPoint dataPoint = new DataPoint(stockDataPoint);

            double change = ((double)dataPoint.Change);
            double changePercent = ((double)dataPoint.ChangePercent);

            Assert.AreEqual(2, change);

            Assert.AreEqual(1.96078431372549, changePercent, 0.001);
        }
    }
}
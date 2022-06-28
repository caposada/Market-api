using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StockManager.Tests
{
    [TestClass()]
    public class MarketDataRequestTests
    {
        static Guid guid = Guid.NewGuid();
        static string description = "This is a test";

        [TestMethod()]
        public void TheConstructor()
        {
            MarketDataRequest marketDataRequest = new MarketDataRequest(description, guid);

            Assert.AreEqual(guid, marketDataRequest.RecordId, "The RecordId should be the same as the one passed into the constructor");
            Assert.AreEqual(description, marketDataRequest.Description, "The Description should be the same as the one passed into the constructor");
        }

        [TestMethod()]
        public void ErrorSwitch()
        {
            MarketDataRequest marketDataRequest = new MarketDataRequest(description);

            Assert.AreEqual(MarketDataRequestStatus.PENDING, marketDataRequest.MarketDataRequestStatus, "The error count is 0 so this should be PENDING");

            marketDataRequest.ErrorCount++;
            Assert.AreEqual(MarketDataRequestStatus.PENDING, marketDataRequest.MarketDataRequestStatus, "The error count is 1 so this should be PENDING");

            for (int i = 1; i < MarketDataRequest.MAX_ERRORS - 1; i++)
            {
                marketDataRequest.ErrorCount++;
            }
            Assert.AreEqual(MarketDataRequestStatus.PENDING, marketDataRequest.MarketDataRequestStatus, "The error count is MAX_ERRORS - 1 so this should be PENDING");

            marketDataRequest.ErrorCount++;
            Assert.AreEqual(MarketDataRequestStatus.ERROR, marketDataRequest.MarketDataRequestStatus, "The error count is MAX_ERRORS so this should be ERROR");
        }
    }
}
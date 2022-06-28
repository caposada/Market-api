using AlphaVantage.Net.Common.Intervals;
using AlphaVantage.Net.Stocks;
using Elements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StockManager.Tests
{
    [TestClass()]
    public class MarketRequestorTests
    {
        static MarketData sharedMarketData = new MarketData();
        static MarketRequestor marketRequestor = new MarketRequestor(sharedMarketData);
        static QueuePriority queuePriority = QueuePriority.HIGH;
        static string symbol = "AAPL";
        static Interval interval = Interval.Daily;

        [TestMethod()]
        public async Task GetQuoteTestAsync()
        {
            // May have to wait because of Pause caused by other Tests
            await sharedMarketData.PauseRequest();

            await sharedMarketData.ClearPreviousQuoteRetrievals(); // Need to clear any previous Retrievals (request/result)

            Guid recordId = Guid.NewGuid();

            Assert.IsNull(marketRequestor.GetQuote(recordId), "Should be null - the request, with that id, shouldn't exist yet");

            RequestResult expected = new RequestResult(recordId, MarketDataRequestStatus.PENDING);
            RequestResult rexult = marketRequestor.GetQuote(symbol, queuePriority, recordId);

            Assert.AreEqual(expected.Id, rexult.Id, "Should have the same Id");
            Assert.AreEqual(expected.Status, rexult.Status, "Should have the same Status (PENDING)");
        }

        [TestMethod()]
        public async Task GetQuoteTest_PostDataRetrievalAsync()
        {
            // May have to wait because of Pause caused by other Tests
            await sharedMarketData.PauseRequest();

            await sharedMarketData.ClearPreviousQuoteRetrievals(); // Need to clear any previous Retrievals (request/result)

            Guid recordId = Guid.NewGuid();
            var request = new QuoteRequest(symbol);
            string description = $"Quote (Symbol:{symbol})";
            var marketDataRequest = new MarketDataRequest<QuoteRequest, GlobalQuote>(
                request,
                description,
                recordId);
            await sharedMarketData.GetQuote(marketDataRequest);
            GlobalQuote quote = marketDataRequest.Resulting;

            Assert.AreEqual(MarketDataRequestStatus.SUCCESS, marketDataRequest.MarketDataRequestStatus, "Should be SUCCESS");

            Assert.IsNotNull(marketRequestor.GetQuote(recordId), "Should NOT be null - the request, with that id, should have an entry");
        }

        [TestMethod()]
        public async Task GetTimeSeriesTestAsync()
        {
            // May have to wait because of Pause caused by other Tests
            await sharedMarketData.PauseRequest();

            await sharedMarketData.ClearPreviousTimeSeriesRetrievals(); // Need to clear any previous Retrievals (request/result)

            Guid recordId = Guid.NewGuid();

            Assert.IsNull(marketRequestor.GetTimeSeries(recordId), "Should be null - the request, with that id, shouldn't exist yet");

            RequestResult expected = new RequestResult(recordId, MarketDataRequestStatus.PENDING);
            RequestResult rexult = marketRequestor.GetTimeSeries(symbol, interval, queuePriority, recordId);

            Assert.AreEqual(expected.Id, rexult.Id, "Should have the same Id");
            Assert.AreEqual(expected.Status, rexult.Status, "Should have the same Status (PENDING)");
        }

        [TestMethod()]
        public async Task GetTimeSeriesTest_PostDataRetrievalAsync()
        {
            // May have to wait because of Pause caused by other Tests
            await sharedMarketData.PauseRequest();

            await sharedMarketData.ClearPreviousQuoteRetrievals(); // Need to clear any previous Retrievals (request/result)

            Guid recordId = Guid.NewGuid();
            TimeSerieseRequest request = new TimeSerieseRequest(symbol, interval);
            string description = $"Time Series (Symbol:{symbol};Interval:{interval})";
            var marketDataRequest = new MarketDataRequest<TimeSerieseRequest, StockTimeSeries>(
                request,
                description,
                recordId);
            await sharedMarketData.GetTimeSeries(marketDataRequest);
            StockTimeSeries timeSeries = marketDataRequest.Resulting;

            Assert.AreEqual(MarketDataRequestStatus.SUCCESS, marketDataRequest.MarketDataRequestStatus, "Should be SUCCESS");

            Assert.IsNotNull(marketRequestor.GetTimeSeries(recordId), "Should NOT be null - the request, with that id, should have an entry");
        }
    }
}
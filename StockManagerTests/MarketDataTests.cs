using AlphaVantage.Net.Common.Intervals;
using AlphaVantage.Net.Stocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManager.Tests
{
    [TestClass()]
    public class MarketDataTests
    {
        static MarketData sharedMarketData = new MarketData();
        static string symbol = "AAPL";
        static Interval interval = Interval.Daily;

        [TestMethod()]
        public void AAA1_MarketDataTest() // AAA1_ ensures this test is done first
        {
            Assert.AreEqual(0, sharedMarketData.ActiveCallsCount, "Should initially be zero");
            Assert.AreEqual(sharedMarketData.MaxHitsPerMinute, sharedMarketData.AvailableCalls, $"Should initially be {sharedMarketData.MaxHitsPerMinute}");
            Assert.AreEqual(0, sharedMarketData.CallCountInMinute, $"Should initially be zero");
            Assert.AreEqual(0, sharedMarketData.ErrorCount, "Should initially be zero");
            Assert.AreEqual(DateTime.MinValue, sharedMarketData.NextReady, $"Should initially be {DateTime.MinValue}");
            Assert.AreEqual(MarketDataStatus.OKAY, sharedMarketData.Status, "Should initially be OKAY");
        }
        
        [TestMethod()]
        public void Settings()
        {
            Assert.IsTrue(sharedMarketData.ApiKey.Length > 0, "Should be a string with content");
            Assert.IsTrue(sharedMarketData.MaxHitsPerMinute > 0, "Should be a number above zero");
            Assert.IsTrue(sharedMarketData.MaxHitsPerDay > 0, "Should be a number above zero");
        }

        [TestMethod()]
        public async Task Logger_TodaysCount()
        {
            // May have to wait because of Pause caused by other Tests
            await sharedMarketData.PauseRequest();

            int? todaysCount = sharedMarketData.TodaysCount;

            var marketDataRequest = new MarketDataRequest<CompanyOverviewRequest, string>(
                new CompanyOverviewRequest(symbol),
                $"Company Overview (Symbol:{symbol})");
            await sharedMarketData.GetCompanyOverview(marketDataRequest);

            Assert.AreEqual(MarketDataRequestStatus.SUCCESS, marketDataRequest.MarketDataRequestStatus, "Should be SUCCESS");
            Assert.AreEqual(todaysCount + 1, sharedMarketData.TodaysCount, "Should be that TodaysCount has increased by 1 after call");
        }

        [TestMethod()]
        public async Task CompanyListingsTest()
        {
            // May have to wait because of Pause caused by other Tests
            await sharedMarketData.PauseRequest();

            var marketDataRequest = new MarketDataRequest<CompanyListingsRequest, string>(
                new CompanyListingsRequest(),
                $"Company Listings");
            await sharedMarketData.GetCompanyListings(marketDataRequest);

            Assert.AreEqual(MarketDataRequestStatus.SUCCESS, marketDataRequest.MarketDataRequestStatus, "Should be SUCCESS");
            Assert.IsTrue(marketDataRequest.Resulting.Length > 10000, "Should be a large sized string");
        }

        [TestMethod()]
        public async Task CompanyOverviewTest()
        {
            // May have to wait because of Pause caused by other Tests
            await sharedMarketData.PauseRequest();

            var marketDataRequest = new MarketDataRequest<CompanyOverviewRequest, string>(
                new CompanyOverviewRequest(symbol),
                $"Company Overview (Symbol:{symbol})");
            await sharedMarketData.GetCompanyOverview(marketDataRequest);

            Assert.AreEqual(MarketDataRequestStatus.SUCCESS, marketDataRequest.MarketDataRequestStatus, "Should be SUCCESS");
            Assert.IsTrue(marketDataRequest.Resulting.Length > 100, "Should be a medium sized string");
            Assert.IsTrue(marketDataRequest.Resulting.Contains("MarketCapitalization"), "Should be a string containing 'MarketCapitalization'");
        }

        [TestMethod()]
        public async Task SymbolSearchMatchTest()
        {
            // May have to wait because of Pause caused by other Tests
            await sharedMarketData.PauseRequest();

            string keyWords = "Procter & Gamble";
            var marketDataRequest = new MarketDataRequest<SymbolSearchMatchRequest, ICollection<SymbolSearchMatch>>(
                new SymbolSearchMatchRequest(keyWords),
                $"Symbol Search Match (keyWords:{keyWords})");
            await sharedMarketData.GetSymbolSearchMatch(marketDataRequest);

            Assert.AreEqual(MarketDataRequestStatus.SUCCESS, marketDataRequest.MarketDataRequestStatus, "Should be SUCCESS");
            Assert.IsTrue(marketDataRequest.Resulting.Count == 5, "Should be a list with 5 items");
            Assert.IsTrue(marketDataRequest.Resulting.ToArray()[0].Name == "Procter & Gamble Co.", "Should be a string containing 'Procter & Gamble Co.'");
        }

        [TestMethod()]
        public async Task QuoteTest()
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
            Assert.IsTrue(quote.Symbol == symbol, "Should be the relevant company Symbol");

            Retrieval<QuoteRequest, GlobalQuote> foundRetrieval = sharedMarketData.FindPreviousMatchingRetrieval(request);
            Assert.IsTrue(foundRetrieval.Requesting.Symbol == symbol, "Should be the same Symbol");
            Assert.IsTrue(foundRetrieval.RecordId == recordId, "Should be the same RecordId");
            Assert.IsTrue(foundRetrieval.ValidUntil > DateTime.Now, "Should be valid for at least over a minute");
            Assert.AreEqual(quote, foundRetrieval.GetResult(), "Should be the same quote object as before");

            Retrieval<QuoteRequest, GlobalQuote> retrieval = sharedMarketData.GetQuoteRetrieval(recordId);
            Assert.AreEqual(quote, foundRetrieval.GetResult(), "Should be the same GlobalQuote object as before");
        }

        [TestMethod()]
        public async Task TimeSeriesTest()
        {
            // May have to wait because of Pause caused by other Tests
            await sharedMarketData.PauseRequest();

            await sharedMarketData.ClearPreviousTimeSeriesRetrievals(); // Need to clear any previous Retrievals (request/result)

            Guid recordId = Guid.NewGuid();
            TimeSerieseRequest request = new TimeSerieseRequest(symbol, interval);
            string description = $"Time Series (Symbol:{symbol};Interval:{interval})";
            var marketDataRequest = new MarketDataRequest<TimeSerieseRequest, StockTimeSeries>(
                request,
                description,
                recordId);
            await sharedMarketData.GetTimeSeries(marketDataRequest);
            StockTimeSeries timeSeries = marketDataRequest.Resulting;


            // Get result with call (this should not return a previously stored result)
            Assert.AreEqual(MarketDataRequestStatus.SUCCESS, marketDataRequest.MarketDataRequestStatus, "Should be SUCCESS");
            Assert.IsTrue(timeSeries.DataPoints.Count > 0, "Should be a number of DataPoints");
            Assert.IsTrue(timeSeries.Interval == interval, "Should be a the same interval");


            Retrieval<TimeSerieseRequest, StockTimeSeries> foundRetrieval = sharedMarketData.FindPreviousMatchingRetrieval(request);
            Assert.IsTrue(foundRetrieval.Requesting.Symbol == symbol, "Should be the same Symbol");
            Assert.IsTrue(foundRetrieval.Requesting.Interval == interval, "Should be the same Interval");
            Assert.IsTrue(foundRetrieval.RecordId == recordId, "Should be the same RecordId");
            Assert.IsTrue(foundRetrieval.ValidUntil > DateTime.Now, "Should be valid for at least over a minute");
            Assert.AreEqual(timeSeries, foundRetrieval.GetResult(), "Should be the same StockTimeSeries object as before");


            Retrieval<TimeSerieseRequest, StockTimeSeries> retrieval = sharedMarketData.GetTimeSeriesRetrieval(recordId);
            Assert.AreEqual(timeSeries, foundRetrieval.GetResult(), "Should be the same StockTimeSeries object as before");


            var secondCallMarketDataRequest = new MarketDataRequest<TimeSerieseRequest, StockTimeSeries>(
                request,
                description,
                recordId);
            await sharedMarketData.GetTimeSeries(secondCallMarketDataRequest);
            StockTimeSeries secondCallTimeSeries = secondCallMarketDataRequest.Resulting;
            // Get result with call (this should return a previously stored result)
            Assert.AreEqual(MarketDataRequestStatus.SUCCESS, marketDataRequest.MarketDataRequestStatus, "Should be SUCCESS");
            Assert.IsTrue(marketDataRequest.RecordId == recordId, "Should be the same RecordId as the first call as this was retrieved from PreviousTimeSerieseRetrievals list");

        }

        [TestMethod()]
        public void GetWhenNextReadyTest()
        {
            MarketData isolatedMarketData = new MarketData();

            DateTime now = DateTime.Now;
            DateTime oneMinuteAhead = now.AddMinutes(1);
            isolatedMarketData.NextReady = oneMinuteAhead;
            int milliseconds = isolatedMarketData.GetWhenNextReady();

            Assert.IsTrue(milliseconds > 55000 && milliseconds <= 60000, "Should be just just under 60000 milliseconds (one minute)");
        }

        [TestMethod()]
        public async Task PauseRequestTest()
        {
            MarketData isolatedMarketData = new MarketData();

            DateTime currentTime = DateTime.Now;
            DateTime future = currentTime.AddSeconds(3);
            isolatedMarketData.NextReady = future;
            isolatedMarketData.Status = MarketDataStatus.DELAYED;

            await isolatedMarketData.PauseRequest();

            DateTime now = DateTime.Now; // Small adjustment is needed here because the Task.Delay is not perfect
            Assert.IsTrue(now >= future, "Should have pause for a few seconds");
        }

        [TestMethod()]
        public async Task PauseRequestTest_WithParameters()
        {
            MarketData isolatedMarketData = new MarketData();

            DateTime currentTime = DateTime.Now;
            DateTime future = currentTime.AddSeconds(3);
            isolatedMarketData.NextReady = future;
            isolatedMarketData.Status = MarketDataStatus.DELAYED;

            MarketDataRequest marketDataRequest = new MarketDataRequest("Pause test");

            _ = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait a second
                Assert.AreEqual(MarketDataRequestStatus.DELAYED, marketDataRequest.MarketDataRequestStatus, "Should be DELAYED as the pause should be active a second ago");
            });

            await isolatedMarketData.PauseRequest(marketDataRequest);

            DateTime now = DateTime.Now; // Small adjustment is needed here because the Task.Delay is not perfect
            Assert.AreEqual(MarketDataRequestStatus.PENDING, marketDataRequest.MarketDataRequestStatus, "Should be back to PENDING after the paue");
        }

        [TestMethod()]
        public async Task IncrementActiveCallCount()
        {
            MarketData isolatedMarketData = new MarketData();

            Assert.AreEqual(0, isolatedMarketData.ActiveCallsCount, "Should be zero");
            Assert.AreEqual(MarketDataStatus.OKAY, isolatedMarketData.Status, "Should be OKAY");

            isolatedMarketData.IncrementActiveCallCount();

            Assert.AreEqual(MarketDataStatus.LOADING, isolatedMarketData.Status, "Should be LOADING");
            Assert.AreEqual(1, isolatedMarketData.ActiveCallsCount, "Should be 1");
        }

        [TestMethod()]
        public async Task DecrementActiveCallCount()
        {
            MarketData isolatedMarketData = new MarketData();
        }

        [TestMethod()]
        public async Task IncrementMinuteCallCount()
        {
            MarketData isolatedMarketData = new MarketData();
        }

        [TestMethod()]
        public async Task IncrementErrorCount()
        {
            MarketData isolatedMarketData = new MarketData();
        }

        [TestMethod()]
        public async Task GetValidUntil()
        {
            MarketData isolatedMarketData = new MarketData();
        }

        [TestMethod()]
        public async Task CalculatedValidUntilDateTime()
        {
            MarketData isolatedMarketData = new MarketData();
        }

    }
}
using AlphaVantage.Net.Common.Intervals;
using AlphaVantage.Net.Stocks;
using Elements;
using Microsoft.AspNetCore.Mvc;
using StockManager;

namespace MarketWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketController : Controller
    {
        public class MarketDetails
        {
            public MarketDataStatus Status { get; set; }
            public DateTime? NextReady { get; set; }
            public int AvailableCalls { get; set; }
            public int CallCountInMinute { get; set; }
            public int ActiveCallsCount { get; set; }

        }

        public class Quote
        {
            public string Symbol { get; set; }
            public decimal OpeningPrice { get; set; }
            public decimal PreviousClosingPrice { get; set; }
            public decimal HighestPrice { get; set; }
            public decimal LowestPrice { get; set; }
            public decimal Price { get; set; }
            public long Volume { get; set; }
            public decimal Change { get; set; }
            public decimal ChangePercent { get; set; }
            public DateTime LatestTradingDay { get; set; }

            public Quote(GlobalQuote globalQuote)
            {
                this.Symbol = globalQuote.Symbol;
                this.OpeningPrice = globalQuote.OpeningPrice;
                this.PreviousClosingPrice = globalQuote.PreviousClosingPrice;
                this.HighestPrice = globalQuote.HighestPrice;
                this.LowestPrice = globalQuote.LowestPrice;
                this.Price = globalQuote.Price;
                this.Volume = globalQuote.Volume;
                this.Change = globalQuote.Change;
                this.ChangePercent = globalQuote.ChangePercent;
                this.LatestTradingDay = globalQuote.LatestTradingDay.ToDateTimeUnspecified();
            }

        }

        const int CACHE_DURATION_30MINS = 60 * 30;          // 30 Minutes
        const int CACHE_DURATION_FULLDAY = 60 * 60 * 24;    // 1 DAY

        private readonly Market.App marketApp;

        public MarketController(Market.App marketApp)
        {
            this.marketApp = marketApp;
        }

        [HttpGet("Details")]
        public ActionResult<MarketDetails> GetMarketDetails()
        {
            MarketDetails details = new MarketDetails()
            {
                Status = marketApp.MarketData.Status,
                NextReady = marketApp.MarketData.NextReady,
                AvailableCalls = marketApp.MarketData.AvailableCalls.Value,
                CallCountInMinute = marketApp.MarketData.CallCountInMinute,
                ActiveCallsCount = marketApp.MarketData.ActiveCallsCount
            };
            return details;
        }


        [HttpGet("{symbol}/Quote")]
        public ActionResult<RequestResult> GetQuote(string symbol)
        {
            return marketApp.MarketRequestor.GetQuote(symbol, QueuePriority.HIGH);
        }

        [HttpGet("{symbol}/TimeSeries")]
        public ActionResult<RequestResult> GetTimeSeries(string symbol)
        {
            return marketApp.MarketRequestor.GetTimeSeries(symbol, Interval.Daily, QueuePriority.HIGH);
        }

        [HttpGet("{symbol}/TimeSeries/{interval}")]
        public ActionResult<RequestResult> GetTimeSeries(string symbol, Interval interval)
        {
            // TODO - set ResponseCache.Duration programically so we can reduce calls to stock market server
            // for month/weeek calls
            return marketApp.MarketRequestor.GetTimeSeries(symbol, interval, QueuePriority.HIGH);
        }




        [HttpGet("Quote/Request/{id}")]
        public ActionResult<Quote> GetQuoteRequest(string id)
        {
            GlobalQuote globalQuote = marketApp.MarketRequestor.GetQuote(Guid.Parse(id));
            Quote quote = new Quote(globalQuote);
            return quote;
        }

        [HttpGet("TimeSeries/Request/{id}")]
        public ActionResult<StockTimeSeries> GetTimeSeriesRequest(string id)
        {
            return marketApp.MarketRequestor.GetTimeSeries(Guid.Parse(id));
        }




        [HttpGet("Test/TimeSeries")]
        public ActionResult<RequestResult> TestRequestorTimeSeries()
        {
            return marketApp.MarketRequestor.GetTimeSeries("AAPL", Interval.Min60, QueuePriority.HIGH);
        }
    }
}

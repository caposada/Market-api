using AlphaVantage.Net.Stocks;
using DataStorage;
using Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StockManager
{
    public class MarketDataPreviousRetrievalsStore : StoreBase
    {
        public List<Retrieval<TimeSerieseRequest, StockTimeSeries>> PreviousTimeSerieseRetrievals { get; set; }
        public List<Retrieval<QuoteRequest, GlobalQuote>> PreviousQuoteRetrievals { get; set; }

        [JsonConstructor]
        public MarketDataPreviousRetrievalsStore()
        {
            PreviousTimeSerieseRetrievals = new List<Retrieval<TimeSerieseRequest, StockTimeSeries>>();
            PreviousQuoteRetrievals = new List<Retrieval<QuoteRequest, GlobalQuote>>();
        }

        public override string GetFilename()
        {
            return "PreviousRequests";
        }

        public override string? GetFolderName()
        {
            return null;
        }

        public override string GetPathPrefix()
        {
            return Constants.MARKETDATA_REQUEST_FOLDER_NAME;
        }

    }
}

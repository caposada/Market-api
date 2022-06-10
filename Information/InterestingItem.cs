using AlphaVantage.Net.Common.Intervals;
using AlphaVantage.Net.Stocks;
using Elements;
using StockManager;
using System.Text.Json.Serialization;
using TextAnalysis;

namespace Information
{
    public delegate void InterestingItemsNotify(InterestingItem interestingItem);                               // delegate

    public class InterestingItem : GathererInformationItem
    {
        private const int NUMBER_OF_DAYS_IN_THE_PAST = 10;

        public event InterestingItemsNotify? AllTimeSeriesDataProcessed;             // event

        public class Store : StoreBase
        {
            public string? Text { get; set; }
            public DateTimeOffset PublishDate { get; set; }
            public ExchangeState StockExchangeState { get; set; }
            public List<AnalysisFinding>? Findings { get; set; }

            private string? folderName;

            public Store()
            {
            }

            public Store(string folderName)
            {
                this.folderName = folderName;
            }

            public override string GetFilename()
            {
                return "Findings";
            }

            public override string? GetFolderName()
            {
                return folderName;
            }

            public override string GetPathPrefix()
            {
                return Constants.GATHERER_FOLDER_NAME;
            }
        }

        [JsonIgnore]
        public string? Text
        {
            get
            {
                return store.Data.Text;
            }
        }
        [JsonIgnore]
        public DateTimeOffset PublishDate
        {
            get
            {
                return store.Data.PublishDate;
            }
        }
        [JsonIgnore]
        public List<AnalysisFinding>? Findings
        {
            get
            {
                return store.Data.Findings;
            }
        }
        [JsonIgnore]
        public AnalysisBreakDown? AnalysisBreakDown
        {
            get
            {
                if (breakdown == null)
                    breakdown = new BreakDown(this.Id);
                return breakdown.AnalysisBreakDown;
            }
        }
        [JsonIgnore]
        public List<TimeSeries> TimeSerieses
        {
            get
            {
                if (timeSeriesData == null)
                    timeSeriesData = new TimeSeriesData(this.Id, this.PublishDate);
                return timeSeriesData.TimeSerieses;
            }
        }
        [JsonIgnore]
        public bool StockTimeSeriesExists
        {
            get
            {
                if (timeSeriesData == null)
                    timeSeriesData = new TimeSeriesData(this.Id, this.PublishDate);
                return timeSeriesData.Exists;
            }
        }
        [JsonIgnore]
        public bool NeedToRetrieveTimeSeriesData
        {
            get
            {
                return
                    TimeSeriesStatus == TimeSeriesStatus.NOT_SEEN
                    || TimeSeriesStatus == TimeSeriesStatus.READY
                    || TimeSeriesStatus == TimeSeriesStatus.PARTIAL
                    || TimeSeriesStatus == TimeSeriesStatus.PRE_DATE;
            }
        }
        public TimeSeriesStatus TimeSeriesStatus { get; set; }

        private DataStorage<Store> store;
        private BreakDown? breakdown;
        private TimeSeriesData? timeSeriesData;
        private static StockExchange stockExchange = new NYSE();
        private List<MarketDataRequestAction>? marketDataRequestActions;

        [JsonConstructorAttribute]
        public InterestingItem(Guid id, Guid sourceId, DateTime timestamp)
            : base(id, sourceId, timestamp)
        {
            store = new DataStorage<Store>(new Store(this.Id.ToString()));
            store.Load();
        }

        public InterestingItem(AnalysisInfo info)
            : base(info.NewsItem.Id, info.NewsItem.SourceId, DateTime.Now)
        {
            store = new DataStorage<Store>(new Store(this.Id.ToString()));
            store.Data.Text = info.Text;
            store.Data.PublishDate = info.NewsItem.PublishDate;
            store.Data.Findings = info.Findings;
            store.Save();

            breakdown = new BreakDown(this.Id);
            breakdown.AnalysisBreakDown = info.BreakDown;

            this.TimeSeriesStatus = TimeSeriesStatus.NOT_SEEN;
        }

        public void Destroy()
        {

        }

        public List<MarketDataRequestAction>? GetMarketDataRequestActions(MarketData marketData)
        {
            if (marketDataRequestActions == null)
            {
                marketDataRequestActions = new List<MarketDataRequestAction>();

                if (CanProceed())
                    FillMarketDataRequestActions(marketData);
            }

            return marketDataRequestActions;
        }

        private void FillMarketDataRequestActions(MarketData marketData)
        {
            Interval interval = Interval.Min15;
            DateTime validUntil = StockTimeSeriesResult.CalculatedValidUntilDateTime(interval);
            int actionProcessed = 0;
            int actionsComplete = 0;
            if (Findings != null)
            {
                foreach (var finding in Findings)
                {
                    if (finding.Confidence >= AnalysisConfidence.HIGH)
                    {
                        string? symbol = finding.Company.Symbol;

                        // Make sure we haven't already found this data for this symbol
                        if (symbol != null && !TimeSerieses.Any(x => x.Symbol == symbol))
                        {
                            //MarketDataRequestAction? foundMarketDataRequestAction = marketDataRequestActions.Find(x => ((MarketDataRequest<TimeSerieseRequest, StockTimeSeries>)x.MarketDataRequest).Requesting.Symbol == symbol);
                            //if (foundMarketDataRequestAction != null) {
                            //}


                            var requesting = new TimeSerieseRequest(symbol, interval);
                            var marketDataRequest = new MarketDataRequest<TimeSerieseRequest, StockTimeSeries>(requesting, $"Time Series (Symbol:{symbol};Interval:{interval})");

                            Action action = new Action(async () =>
                            {
                                await marketData.GetTimeSeries(marketDataRequest);
                                if (marketDataRequest.MarketDataRequestStatus == MarketDataRequestStatus.SUCCESS && marketDataRequest.Resulting != null)
                                {
                                    if (timeSeriesData == null)
                                        timeSeriesData = new TimeSeriesData(this.Id, this.PublishDate);

                                    StockTimeSeriesResult stockTimeSeriesResult = new StockTimeSeriesResult(symbol, validUntil, DateTime.Now, interval, marketDataRequest.Resulting);
                                    timeSeriesData.Add(stockTimeSeriesResult);

                                    actionsComplete++;
                                }
                                else
                                {
                                    if (marketDataRequest.MarketDataRequestStatus == MarketDataRequestStatus.ERROR)
                                    {
                                        // This request keeps failing (multipple times as it has been readded to the queue),
                                        // so set in motion it's removal from the queue, and future queuing
                                        actionsComplete++;
                                    }

                                    this.TimeSeriesStatus = TimeSeriesStatus.PARTIAL;
                                }

                                if (actionsComplete == marketDataRequestActions.Count)
                                {
                                    // All data has been collected successfully 
                                    this.TimeSeriesStatus = TimeSeriesStatus.SUCCESS;
                                }
                                else
                                {
                                    this.TimeSeriesStatus = TimeSeriesStatus.PARTIAL;
                                }

                                actionProcessed++;
                                if (actionProcessed >= marketDataRequestActions.Count)
                                {
                                    // All actions completed
                                    AllTimeSeriesDataProcessed?.Invoke(this);
                                }
                            });

                            MarketDataRequestAction MarketDataRequestAction = new MarketDataRequestAction(marketDataRequest, action);
                            marketDataRequestActions.Add(MarketDataRequestAction);
                        }
                    }
                }
            }

            if (marketDataRequestActions.Count == 0)
            {
                this.TimeSeriesStatus = TimeSeriesStatus.SUCCESS;
            }
        }

        private bool CanProceed()
        {
            // Check if stock exchange was open at the time of publiocation
            // We don't want to gather time series data for items when the NewsItem was published out of hours
            // finding.Company.Exchange // We only use NYSE and Nasdaq, both of which are in New York
            var stockExchangeState = stockExchange.IsOpen(PublishDate.DateTime);
            if (stockExchangeState != ExchangeState.OPEN)
            {
                this.TimeSeriesStatus = TimeSeriesStatus.OUT_OF_HOURS;
                return false; // NewsItem published Out of hours, weekend or holiday!
            }

            // Check if it's too late to collect data
            // We can only get about a number of days worth of time series data if we use the Min15 interval,
            // so check that the publishDate is within 20 days
            DateTime numberOfDaysPast = DateTime.Today.AddDays(-NUMBER_OF_DAYS_IN_THE_PAST);
            if (PublishDate.Date < numberOfDaysPast)
            {
                this.TimeSeriesStatus = TimeSeriesStatus.PAST_DATE;
                return false; // Date to far back // CAP get full data instead of 100 datapoints?
            }

            // Check if it's too early to collect data
            // We want the time series data after the published date of the NewsItem
            // in order to see if there are any changes in the price based on the news information.
            // Mon 12:34pm  | Thu 10:20am   | Fri 3:55pm
            DateTime publishDateEvening = PublishDate.Date.AddHours(20);                                            // Mon 8pm      | Thu 8pm       | Fri 8pm
            DateTime nextOpening = stockExchange.CalculateNextOpenDateTime(publishDateEvening);                     // Tue 2:30pm   | Fri 2:30pm    | Mon 2:30pm
            DateTime dayAfterOpening = stockExchange.CalculateNextOpenDateTime(nextOpening); // and do it again =>  // Wed 2:30pm   | Mon 2:30pm    | Tue 2:30pm
            if (DateTime.Now < dayAfterOpening)
            {
                // Now is too early to collect at least a days worth of time series data
                this.TimeSeriesStatus = TimeSeriesStatus.PRE_DATE;
                return false; // Date to far back
            }

            return true; // All is fine
        }

    }

}

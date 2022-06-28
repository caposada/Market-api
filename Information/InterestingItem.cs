using AlphaVantage.Net.Common.Intervals;
using AlphaVantage.Net.Stocks;
using Elements;
using StockManager;
using System.ComponentModel.DataAnnotations.Schema;
using TextAnalysis;

namespace Information
{
    public delegate void InterestingItemsNotify(InterestingItem interestingItem);                               // delegate

    public class InterestingItem : GathererInformationItem
    {
        private const int NUMBER_OF_DAYS_IN_THE_PAST = 10;

        public event InterestingItemsNotify? AllTimeSeriesDataProcessed;             // event

        [NotMapped]
        public List<AnalysisFinding>? Findings
        {
            get
            {
                using (InformationContext context = new InformationContext())
                {
                    return context.AnalysisFindings.Where(x => x.NewsItemId == this.NewsItemId).ToList();
                }
            }
        }
        [NotMapped]
        public AnalysisBreakDown? AnalysisBreakDown
        {
            get
            {
                using (InformationContext context = new InformationContext())
                {
                    return context.AnalysisBreakDowns.Where(x => x.NewsItemId == this.NewsItemId).FirstOrDefault();
                }
            }
        }
        [NotMapped]
        public List<TimeSeries> TimeSerieses
        {
            get
            {
                using (InformationContext context = new InformationContext())
                {
                    return context.TimeSerieses.Where(x => x.NewsItemId == NewsItemId).ToList();
                }
            }
        }
        [NotMapped]
        public bool StockTimeSeriesExists
        {
            get
            {
                return TimeSerieses.Count > 0;
            }
        }
        [NotMapped]
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



        private static StockExchange stockExchange = new NYSE();
        private List<MarketDataRequestAction>? marketDataRequestActions;



        public InterestingItem(Guid newsItemId, Guid sourceId, DateTime timestamp, string text, DateTimeOffset publishDate)
            : base(newsItemId, sourceId, timestamp, text, publishDate)
        {
            this.TimeSeriesStatus = TimeSeriesStatus.NOT_SEEN;
        }

        public async Task SetInfoAsync(AnalysisInfo info)
        {
            try
            {
                using (InformationContext context = new InformationContext())
                {
                    foreach (var finding in info.Findings)
                    {
                        context.AnalysisFindings.Add(
                            new AnalysisFinding(this.NewsItemId, finding.Company, finding.Confidence, finding.Rationale, finding.Tokens));
                    }

                    context.AnalysisBreakDowns.Add(
                        new AnalysisBreakDown(this.NewsItemId, info.BreakDown.Spans));

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {

            }
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

                                    using (InformationContext context = new InformationContext())
                                    {
                                        StockTimeSeriesResult stockTimeSeriesResult = new StockTimeSeriesResult(symbol, validUntil, DateTime.Now, interval, marketDataRequest.Resulting);
                                        TimeSeries timeSeries = new TimeSeries(this.NewsItemId, this.PublishDate, stockTimeSeriesResult);
                                        context.TimeSerieses.Add(timeSeries);
                                        context.SaveChanges();
                                    }

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

        public override string ToString()
        {
            return $"ID: {this.NewsItemId}";
        }

    }

}

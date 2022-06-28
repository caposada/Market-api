using Elements;
using StockManager;
using TextAnalysis;

namespace Information
{
    public delegate void GathererInterestingItemNotify(GathererInformationItem interestingItem);    // delegate
    public delegate void InformationNotify();                                                       // delegate

    public class GathererInformation
    {
        public event GathererInterestingItemNotify? InterestedItemAdded;                            // event
        public event GathererInterestingItemNotify? InterestedItemRemoved;                          // event
        public event GathererInterestingItemNotify? NoninterestedItemAdded;                         // event
        public event InformationNotify? InformationChanged;                                         // event

        private const int MIN_AVAILABILITY = 0; // 0 to 4

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private AsyncronousQueueProcessor requestQueueProcessor;
        private Task? requestsTask;
        private CancellationToken periodicCheckCancellationToken = new CancellationToken();
        private MarketData marketData;

        public List<InterestingItem> InterestingItems
        {
            get
            {
                using (InformationContext context = new InformationContext())
                {
                    return context.InterestingItems.OrderByDescending(x => x.PublishDate).ToList();
                }
            }
        }
        public List<NonInterestingItem> NonInterestingItems
        {
            get
            {
                using (InformationContext context = new InformationContext())
                {
                    return context.NonInterestingItems.OrderByDescending(x => x.PublishDate).ToList();
                }
            }
        }
        public DateTime LatestDate { get; set; }
        public int RequestQueueProcessorCount
        {
            get
            {
                return this.requestQueueProcessor.Count;
            }
        }


        public GathererInformation(MarketData marketData)
        {

            this.marketData = marketData;
            this.requestQueueProcessor = new AsyncronousQueueProcessor();

            CheckWeHaveAllMarketData();
            _ = PeriodicCheckWeHaveAllMarketData();

        }

        //public async Task CleanAsync(List<NewsItem> allNewsItems)
        //{
        //    // Find all InterestingItem and NonInterestingItem which are of now value anymore and remove them
        //    DateTime deemedOld = DateTime.Today - Constants.DEFAULT_CULL_PERIOD;

        //    // Find all old NonInterestingItem
        //    var recentNonInterestingItems = Store.Data.NonInterestingItems.FindAll(x => x.Timestamp > deemedOld);
        //    var stillGoodNonInterestingItems = recentNonInterestingItems.FindAll(x => allNewsItems.Any(y => y.Id == x.NewsItemId));
        //    Store.Data.NonInterestingItems = stillGoodNonInterestingItems;

        //    // Find all old NonInterestingItem
        //    var recentInterestingItems = Store.Data.InterestingItems.FindAll(x => x.Timestamp > deemedOld);
        //    var stillGoodInterestingItems = recentInterestingItems.FindAll(x => allNewsItems.Any(y => y.Id == x.NewsItemId));
        //    Store.Data.InterestingItems = stillGoodInterestingItems;

        //    Store.Save();

        //    List<Guid> currentIds = InterestingItems.Select(x => x.NewsItemId).ToList();
        //    await Store.CleanAsync(currentIds);
        //}

        public AnalysisBreakDown? GetBreakdown(Guid newsItemId)
        {
            using (InformationContext context = new InformationContext())
            {
                return context.AnalysisBreakDowns.Where(x => x.NewsItemId == newsItemId).FirstOrDefault();
            }
        }

        public bool MarkNotInteresting(Guid newsItemId)
        {
            using (InformationContext context = new InformationContext())
            {
                var found = context.InterestingItems.Where(x => x.NewsItemId == newsItemId).FirstOrDefault();
                if (found != null)
                {
                    var nonInterestingItem = new NonInterestingItem(found.NewsItemId, found.SourceId, found.Timestamp, found.Text, found.PublishDate);

                    context.NonInterestingItems.Add(nonInterestingItem);
                    NoninterestedItemAdded?.Invoke(nonInterestingItem);


                    context.InterestingItems.Remove(found);
                    InterestedItemRemoved?.Invoke(found);

                    context.SaveChanges();

                    return true;
                }
            }
            return false;
        }

        public async Task AddInteresingItemAsync(AnalysisInfo info)
        {
            var item = new InterestingItem(info.NewsItem.Id, info.NewsItem.SourceId, DateTime.Now, info.NewsItem.Text, info.NewsItem.PublishDate);
            _ = item.SetInfoAsync(info);
            item.AllTimeSeriesDataProcessed += InterestingItem_AllTimeSeriesDataProcessed;

            using (InformationContext context = new InformationContext())
            {
                context.InterestingItems.Add(item);
                await context.SaveChangesAsync();
            }

            InterestedItemAdded?.Invoke(item);

            _ = QueueMarketDataRequests(item);
        }

        public async Task AddNoninteresingItemAsync(AnalysisInfo info)
        {
            var item = new NonInterestingItem(info.NewsItem.Id, info.NewsItem.SourceId, DateTime.Now, info.NewsItem.Text, info.NewsItem.PublishDate);
            using (InformationContext context = new InformationContext())
            {
                context.NonInterestingItems.Add(item);
                await context.SaveChangesAsync();
            }
            NoninterestedItemAdded?.Invoke(item);
        }

        private void CheckWeHaveAllMarketData()
        {
            bool atLeastOne = false;
            using (InformationContext context = new InformationContext())
            {
                foreach (var interestingItem in context.InterestingItems)
                {
                    if (interestingItem.NeedToRetrieveTimeSeriesData)
                    {
                        atLeastOne = true;
                        interestingItem.AllTimeSeriesDataProcessed += InterestingItem_AllTimeSeriesDataProcessed;

                        _ = QueueMarketDataRequests(interestingItem);
                    }
                }
                //if (atLeastOne)
                //    Store.Save();
            }
        }

        private void InterestingItem_AllTimeSeriesDataProcessed(InterestingItem interestingItem)
        {
            if (interestingItem.NeedToRetrieveTimeSeriesData)
            {
                _ = QueueMarketDataRequests(interestingItem);
            }
            else
            {
                interestingItem.AllTimeSeriesDataProcessed -= InterestingItem_AllTimeSeriesDataProcessed;

                //// The InterestingItem has all it's TimeSeriesData now, so inform
                //// the Gatherer to save the Information
                //Store.Save();
            }
        }

        private async Task QueueMarketDataRequests(InterestingItem interestingItem)
        {
            try
            {
                await _semaphore.WaitAsync();

                // Get a list of actions to request market data for each symbol in each finding
                List<MarketDataRequestAction>? marketDataRequestActions = interestingItem.GetMarketDataRequestActions(marketData);
                if (marketDataRequestActions != null && marketDataRequestActions.Count > 0)
                {
                    foreach (var marketDataRequestAction in marketDataRequestActions)
                    {
                        _ = requestQueueProcessor.Add(marketDataRequestAction.Action);
                        ColourConsole.WriteLine($"GathererInformation - action added [{requestQueueProcessor.Count}].", ConsoleColor.DarkBlue, ConsoleColor.White);
                    }

                    // Start processing the queue if it isn't already up and running
                    if (requestQueueProcessor.Count > 0 && (requestsTask == null || requestsTask.IsCompleted))
                    {
                        requestsTask = Task.Run(async delegate
                        {
                            while (requestQueueProcessor.Count > 0)
                            {
                                // Only call market if there is capacity
                                if (marketData.AvailableCalls > MIN_AVAILABILITY)
                                {
                                    requestQueueProcessor.Pulse();
                                    ColourConsole.WriteLine($"GathererInformation - pulse [{requestQueueProcessor.Count}].", ConsoleColor.DarkBlue, ConsoleColor.White);
                                    InformationChanged?.Invoke();
                                }

                                await marketData.PauseRequest();
                            }
                        });
                    }
                }
                else
                {

                }
            }
            finally
            {
                _semaphore.Release();
            }

        }

        private async Task PeriodicCheckWeHaveAllMarketData()
        {
            // This should fire off a CheckWeHaveAllMarketData call every evening at 11pm

            while (true)
            {
                // Get evening's 11pm
                //DateTime eleven = DateTime.Today.AddHours(23);
                DateTime nextOpening = new StockExchange().CalculateNextOpenDateTime();
                await Task.Delay(Utils.GetWhen(nextOpening), periodicCheckCancellationToken);
                CheckWeHaveAllMarketData();
            }
        }

    }

}

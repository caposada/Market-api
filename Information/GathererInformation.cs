﻿using Elements;
using StockManager;
using System.Text.Json.Serialization;
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

        public class Store : StoreBase
        {
            public List<InterestingItem> InterestingItems { get; set; }
            public List<NonInterestingItem> NonInterestingItems { get; set; }
            public DateTime LatestDate { get; set; }

            public Store()
            {
                this.InterestingItems = new List<InterestingItem>();
                this.NonInterestingItems = new List<NonInterestingItem>();
                this.LatestDate = DateTime.MinValue;
            }

            public override string GetFilename()
            {
                return "Information";
            }

            public override string? GetFolderName()
            {
                return null;
            }

            public override string GetPathPrefix()
            {
                return Constants.GATHERER_FOLDER_NAME;
            }
        }

        private const int MIN_AVAILABILITY = 0; // 0 to 4

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private AsyncronousQueueProcessor requestQueueProcessor;
        private Task? requestsTask;
        private DataStorage<Store> store;
        private CancellationToken periodicCheckCancellationToken = new CancellationToken();

        public List<InterestingItem> InterestingItems
        {
            get
            {
                return store.Data.InterestingItems;
            }
        }
        public List<NonInterestingItem> NonInterestingItems
        {
            get
            {
                return store.Data.NonInterestingItems;
            }
        }
        public DateTime LatestDate
        {
            get
            {
                return store.Data.LatestDate;
            }
            set
            {
                store.Data.LatestDate = value;
                store.Save();
            }
        }

        [JsonIgnore]
        public MarketData MarketData { get; set; }

        [JsonConstructorAttribute]
        public GathererInformation(MarketData marketData)
        {
            store = new DataStorage<Store>(new Store());
            store.Load();

            this.MarketData = marketData;
            this.requestQueueProcessor = new AsyncronousQueueProcessor();

            CheckWeHaveAllMarketData();
            _ = PeriodicCheckWeHaveAllMarketData();

            //_ = HouseKeeping();
        }

        // Temp - shouldn't be needed (keep an eye on the feeds/Store folder for GUID folders that don't belong anymore)
        public async Task CleanAsync()
        {
            List<Guid> currentIds = InterestingItems.Select(x => x.Id).ToList();
            await store.CleanAsync(currentIds);
        }

        public async Task HouseKeeping()
        {
            // Find all InterestingItem and NonInterestingItem which are of now value anymore and remove them
            DateTime deemedOld = DateTime.Today - Constants.DEFAULT_CULL_PERIOD;

            // Find all old NonInterestingItem
            var oldNonInterestingItems = NonInterestingItems.FindAll(x => x.Timestamp < deemedOld);

            // Find all old NonInterestingItem
            var oldInterestingItems = InterestingItems.FindAll(x => x.Timestamp < deemedOld);
        }

        public AnalysisBreakDown? GetBreakdown(Guid id)
        {
            var found = InterestingItems.Find(x => x.Id == id);
            if (found != null)
            {
                return found.AnalysisBreakDown;
            }
            return null;
        }

        public bool MarkNotInteresting(Guid id)
        {
            var found = InterestingItems.Find(x => x.Id == id);
            if (found != null)
            {
                var nonInterestingItem = new NonInterestingItem(found.Id, found.SourceId, found.Timestamp);

                NonInterestingItems.Add(nonInterestingItem);
                NoninterestedItemAdded?.Invoke(nonInterestingItem);


                InterestingItems.Remove(found);
                InterestedItemRemoved?.Invoke(found);

                store.Save();

                return true;
            }
            return false;
        }

        public void AddInteresingItem(AnalysisInfo info)
        {
            var item = new InterestingItem(info);
            item.AllTimeSeriesDataProcessed += InterestingItem_AllTimeSeriesDataProcessed;
            InterestingItems.Add(item);
            store.Save();
            InterestedItemAdded?.Invoke(item);

            _ = QueueMarketDataRequests(item);
        }

        public void AddNoninteresingItem(AnalysisInfo info)
        {
            var item = new NonInterestingItem(info);
            NonInterestingItems.Add(item);
            store.Save();
            NoninterestedItemAdded?.Invoke(item);
        }

        private void CheckWeHaveAllMarketData()
        {
            bool atLeastOne = false;
            foreach (var interestingItem in InterestingItems)
            {
                if (interestingItem.NeedToRetrieveTimeSeriesData)
                {
                    atLeastOne = true;
                    interestingItem.AllTimeSeriesDataProcessed += InterestingItem_AllTimeSeriesDataProcessed;

                    _ = QueueMarketDataRequests(interestingItem);
                }
            }
            if (atLeastOne)
                store.Save();
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

                // The InterestingItem has all it's TimeSeriesData now, so inform
                // the Gatherer to save the Information
                InformationChanged?.Invoke();
                store.Save();
            }
        }

        private async Task QueueMarketDataRequests(InterestingItem interestingItem)
        {
            try
            {
                await _semaphore.WaitAsync();

                // Get a list of actions to request market data for each symbol in each finding
                List<MarketDataRequestAction>? marketDataRequestActions = interestingItem.GetMarketDataRequestActions(MarketData);
                if (marketDataRequestActions != null && marketDataRequestActions.Count > 0)
                {
                    foreach (var marketDataRequestAction in marketDataRequestActions)
                    {
                        _ = requestQueueProcessor.Add(marketDataRequestAction.Action);
                        ColourConsole.WriteLine($"Gatherer.Information - action added [{requestQueueProcessor.Count}].", ConsoleColor.DarkBlue, ConsoleColor.White);
                    }

                    // Start processing the queue if it isn't already up and running
                    if (requestQueueProcessor.Count > 0 && (requestsTask == null || requestsTask.IsCompleted))
                    {
                        requestsTask = Task.Run(async delegate
                        {
                            while (requestQueueProcessor.Count > 0)
                            {
                                // Only call market if there is capacity
                                if (MarketData.AvailableCalls > MIN_AVAILABILITY)
                                {
                                    requestQueueProcessor.Pulse();
                                    ColourConsole.WriteLine($"Gatherer.Information - pulse [{requestQueueProcessor.Count}].", ConsoleColor.DarkBlue, ConsoleColor.White);
                                }

                                await MarketData.PauseRequest();
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

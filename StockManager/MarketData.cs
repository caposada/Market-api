using AlphaVantage.Net.Common;
using AlphaVantage.Net.Common.Intervals;
using AlphaVantage.Net.Common.Size;
using AlphaVantage.Net.Core.Client;
using AlphaVantage.Net.Stocks;
using AlphaVantage.Net.Stocks.Client;
using Elements;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

[assembly: InternalsVisibleTo("StockManagerTests")]
namespace StockManager
{
    public delegate void MarketDataNotify();                    // delegate

    public class MarketData
    {
        public event MarketDataNotify? StateChange;              // event

        public class Settings
        {
            public class Store : StoreBase
            {
                public string? AlphaVantage_ApiKey { get; set; }
                public int? AlphaVantage_MaxHitsPerMinute { get; set; }
                public int? AlphaVantage_MaxHitsPerDay { get; set; }

                public Store()
                {
                }

                public override string GetFilename()
                {
                    return "Settings_MarketData";
                }

                public override string? GetFolderName()
                {
                    return null;
                }

                public override string GetPathPrefix()
                {
                    return Constants.APP_SETTINGS_FOLDER_NAME;
                }
            }

            public string? ApiKey
            {
                get
                {
                    return store.Data.AlphaVantage_ApiKey;
                }
            }
            public int? MaxHitsPerMinute
            {
                get
                {
                    return store.Data.AlphaVantage_MaxHitsPerMinute;
                }
            }
            public int? MaxHitsPerDay
            {
                get
                {
                    return store.Data.AlphaVantage_MaxHitsPerDay;
                }
            }

            private DataStorage<Store> store;

            public Settings()
            {
                this.store = new DataStorage<Store>(new Store());
                this.store.Load();
            }

        }

        public class Logger
        {
            public class RequestRecord
            {
                public DateTime Date { get; set; }
                public int Count { get; set; }

            }

            public class Store : StoreBase
            {
                public List<RequestRecord> RequestRecords { get; set; }

                public Store()
                {
                    this.RequestRecords = new List<RequestRecord>();
                }

                public override string GetFilename()
                {
                    return "Logger_MarketData";
                }

                public override string? GetFolderName()
                {
                    return null;
                }

                public override string GetPathPrefix()
                {
                    return Constants.APP_SETTINGS_FOLDER_NAME;
                }
            }

            public int TodaysCount
            {
                get
                {
                    return GetTodaysRequestRecord().Count;
                }
            }

            private DataStorage<Store> store;

            public Logger()
            {
                this.store = new DataStorage<Store>(new Store());
                this.store.Load();
            }

            public void IncrementCount()
            {
                var requestRecord = GetTodaysRequestRecord();
                requestRecord.Count++;
                store.Save();
            }

            private RequestRecord GetTodaysRequestRecord()
            {
                DateTime today = DateTime.Today;
                var requestRecord = store.Data.RequestRecords.Find(x => x.Date == today);
                if (requestRecord == null)
                {
                    requestRecord = new RequestRecord()
                    {
                        Count = 0,
                        Date = today
                    };
                    store.Data.RequestRecords.Add(requestRecord);
                    store.Save();
                }
                return requestRecord;
            }

        }

        public class Store : StoreBase
        {
            public List<Retrieval<TimeSerieseRequest, StockTimeSeries>> PreviousTimeSerieseRetrievals { get; set; }
            public List<Retrieval<QuoteRequest, GlobalQuote>> PreviousQuoteRetrievals { get; set; }

            [JsonConstructor]
            public Store()
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

        private const int MAX_ERRORS = 20;                      // This will change in the future if we buy a better package

        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private static Task? minuteResetTask;
        private static Task? dailyResetTask;
        private Settings settings;
        private Logger logger;
        private DataStorage<Store> store;

        public int ActiveCallsCount { get; private set; }
        public int CallCountInMinute { get; private set; }
        public int ErrorCount { get; private set; }
        public MarketDataStatus Status { get; internal set; }
        public DateTime NextReady { get; internal set; }
        public int? AvailableCalls
        {
            get
            {
                return settings.MaxHitsPerMinute - CallCountInMinute;
            }
        }
        public int? MaxHitsPerMinute
        {
            get
            {
                return settings.MaxHitsPerMinute;
            }
        }
        public int? MaxHitsPerDay
        {
            get
            {
                return settings.MaxHitsPerDay;
            }
        }
        public int? TodaysCount
        {
            get
            {
                return logger.TodaysCount;
            }
        }
        public string? ApiKey
        {
            get
            {
                return settings.ApiKey;
            }
        }

        public MarketData()
        {
            this.settings = new Settings();
            this.logger = new Logger();

            this.store = new DataStorage<Store>(new Store());
            this.store.Load();
        }

        public async Task GetCompanyListings(MarketDataRequest<CompanyListingsRequest, string> marketDataRequest)
        {
            if (ApiKey != null)
            {
                var taskAction = new Func<Task>(async () =>
                {
                    var query = new Dictionary<string, string>()
                    {
                    };
                    using var client = new AlphaVantageClient(ApiKey);
                    marketDataRequest.SetResult(await client.RequestPureJsonAsync(
                            ApiFunction.LISTING_STATUS,
                            query));
                });
                await DoRequest(marketDataRequest, taskAction);
            }
        }

        public async Task GetCompanyOverview(MarketDataRequest<CompanyOverviewRequest, string> marketDataRequest)
        {
            if (ApiKey != null)
            {
                var taskAction = new Func<Task>(async () =>
                {
                    var query = new Dictionary<string, string>()
                    {
                        {"symbol", marketDataRequest.Requesting.Symbol}
                    };
                    using var client = new AlphaVantageClient(ApiKey);
                    marketDataRequest.SetResult(await client.RequestPureJsonAsync(ApiFunction.OVERVIEW, query));
                });
                await DoRequest(marketDataRequest, taskAction);
            }
        }

        public async Task GetSymbolSearchMatch(MarketDataRequest<SymbolSearchMatchRequest, ICollection<SymbolSearchMatch>> marketDataRequest)
        {
            if (ApiKey != null)
            {
                var taskAction = new Func<Task>(async () =>
                {
                    using var client = new AlphaVantageClient(ApiKey);
                    using var stocksClient = client.Stocks();
                    marketDataRequest.SetResult(await stocksClient.SearchSymbolAsync(marketDataRequest.Requesting.KeyWords));
                });
                await DoRequest(marketDataRequest, taskAction);
            }
        }

        public async Task GetQuote(MarketDataRequest<QuoteRequest, GlobalQuote> marketDataRequest)
        {
            if (ApiKey != null)
            {
                var taskAction = new Func<Task>(async () =>
                {
                    using var client = new AlphaVantageClient(ApiKey);
                    using var stocksClient = client.Stocks();
                    var result = await stocksClient.GetGlobalQuoteAsync(marketDataRequest.Requesting.Symbol);
                    marketDataRequest.SetResult(result);

                    // Store this result for future recalls
                    var retrieval = new Retrieval<QuoteRequest, GlobalQuote>(
                        marketDataRequest.Requesting,
                        marketDataRequest.Resulting,
                        GetValidUntil(),
                        marketDataRequest.RecordId);
                    store.Data.PreviousQuoteRetrievals.Add(retrieval);
                    store.Save();
                });
                await DoRequest(marketDataRequest, taskAction);
            }
        }

        public Retrieval<QuoteRequest, GlobalQuote>? FindPreviousMatchingRetrieval(QuoteRequest request)
        {
            // 13:15 Previous ValidUntil
            // 13:07 Now
            DateTime now = DateTime.Now;

            var retrieval = store.Data.PreviousQuoteRetrievals.Find(x =>
                x.Requesting?.Symbol == request.Symbol &&
                x.ValidUntil > now);

            return retrieval;
        }

        public Retrieval<QuoteRequest, GlobalQuote>? GetQuoteRetrieval(Guid recordId)
        {
            return (Retrieval<QuoteRequest, GlobalQuote>?)store.Data.PreviousQuoteRetrievals.Find(x => x.RecordId == recordId);
        }

        public async Task GetTimeSeries(MarketDataRequest<TimeSerieseRequest, StockTimeSeries> marketDataRequest)
        {
            // Check if we have recently done this call before
            var previousRetrieval = FindPreviousMatchingRetrieval(marketDataRequest.Requesting);
            if (previousRetrieval != null)
            {
                // Found a matching previous request, so mock the request
                marketDataRequest.SetResult(previousRetrieval.GetResult());

                ColourConsole.WriteLine($"MarketData - {marketDataRequest.Description} previous data found.", ConsoleColor.DarkGreen, ConsoleColor.White);

            }
            else
            {
                if (ApiKey != null)
                {
                    var taskAction = new Func<Task>(async () =>
                    {
                        using var client = new AlphaVantageClient(ApiKey);
                        using var stocksClient = client.Stocks();
                        var result = await stocksClient.GetTimeSeriesAsync(
                            marketDataRequest.Requesting.Symbol,
                            marketDataRequest.Requesting.Interval,
                            OutputSize.Full,
                            isAdjusted: false);
                        marketDataRequest.SetResult(result);

                        // Store this result for future recalls
                        var retrieval = new Retrieval<TimeSerieseRequest, StockTimeSeries>(
                            marketDataRequest.Requesting,
                            marketDataRequest.Resulting,
                            GetValidUntil(marketDataRequest.Requesting.Interval),
                            marketDataRequest.RecordId);
                        store.Data.PreviousTimeSerieseRetrievals.Add(retrieval);
                        store.Save();
                    });
                    await DoRequest(marketDataRequest, taskAction);
                }
            }
        }

        public Retrieval<TimeSerieseRequest, StockTimeSeries>? FindPreviousMatchingRetrieval(TimeSerieseRequest request)
        {
            // 13:15 Previous ValidUntil
            // 13:07 Now
            DateTime now = DateTime.Now;

            var retrieval = store.Data.PreviousTimeSerieseRetrievals.Find(x =>
                x.Requesting?.Symbol == request.Symbol &&
                x.Requesting.Interval == request.Interval &&
                x.ValidUntil > now);

            return retrieval;
        }

        public Retrieval<TimeSerieseRequest, StockTimeSeries>? GetTimeSeriesRetrieval(Guid recordId)
        {
            return (Retrieval<TimeSerieseRequest, StockTimeSeries>?)store.Data.PreviousTimeSerieseRetrievals.Find(x => x.RecordId == recordId);
        }

        public int GetWhenNextReady()
        {
            int milliseconds = Utils.GetWhen(NextReady);
            return milliseconds >= 0 ? milliseconds : 0;
        }
        
        public async Task ClearAllPreviousRetrievals()
        {
            await store.CleanAsync(new List<Guid>());

            store.Data.PreviousQuoteRetrievals.Clear();
            store.Data.PreviousTimeSerieseRetrievals.Clear();
            store.Save();
        }

        public async Task ClearPreviousQuoteRetrievals()
        {
            List<Guid> currentIds = store.Data.PreviousTimeSerieseRetrievals.Select(x => x.RecordId.Value).ToList(); // These are the ones we will keep
            await store.CleanAsync(currentIds);

            store.Data.PreviousQuoteRetrievals.Clear();
            store.Save();
        }

        public async Task ClearPreviousTimeSeriesRetrievals()
        {
            List<Guid> currentIds = store.Data.PreviousQuoteRetrievals.Select(x => x.RecordId.Value).ToList(); // These are the ones we will keep
            await store.CleanAsync(currentIds);

            store.Data.PreviousTimeSerieseRetrievals.Clear();
            store.Save();
        }

        public async Task PauseRequest()
        {
            if (Status == MarketDataStatus.DELAYED || Status == MarketDataStatus.OFFLINE)
            {
                int milliseconds = GetWhenNextReady();
                while (milliseconds > 0)
                {
                    await Task.Delay(milliseconds);
                    milliseconds = GetWhenNextReady();
                }
            }
        }

        internal async Task PauseRequest(MarketDataRequest marketDataRequest)
        {
            if (Status == MarketDataStatus.DELAYED || Status == MarketDataStatus.OFFLINE)
            {
                if (marketDataRequest != null)
                    marketDataRequest.MarketDataRequestStatus = MarketDataRequestStatus.DELAYED;
                await PauseRequest();
                if (marketDataRequest != null)
                    marketDataRequest.MarketDataRequestStatus = MarketDataRequestStatus.PENDING;
            }
        }

        internal async Task IncrementActiveCallCount()
        {
            try
            {
                await _semaphore.WaitAsync();

                ActiveCallsCount++;
                OnStateChange(true);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        internal async Task DecrementActiveCallCount()
        {
            try
            {
                await _semaphore.WaitAsync();

                ActiveCallsCount--;
                OnStateChange(true);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        internal async Task IncrementMinuteCallCount(MarketDataRequest marketDataRequest)
        {
            try
            {
                await _semaphore.WaitAsync();

                CallCountInMinute++;
                OnStateChange(true);

                if (CallCountInMinute >= MaxHitsPerMinute && Status != MarketDataStatus.OFFLINE)
                {
                    // Minute quota of calls has been reach

                    if (marketDataRequest != null)
                        marketDataRequest.MarketDataRequestStatus = MarketDataRequestStatus.DELAYED;

                    NextReady = DateTime.Now.AddMinutes(1);
                    OnStateChange(true);
                }

                if (minuteResetTask == null || minuteResetTask.IsCompleted)
                {
                    // CallCountInMinute is above 1, but will reset to 0 in a minute
                    minuteResetTask = Task.Run((Func<Task?>)async delegate
                    {
                        await Task.Delay(TimeSpan.FromMinutes(1));
                        CallCountInMinute = 0;
                        OnStateChange(true);
                    });
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        internal async Task IncrementErrorCount(MarketDataRequest marketDataRequest)
        {
            try
            {
                await _semaphore.WaitAsync();

                ErrorCount++;

                if (ErrorCount >= MAX_ERRORS)
                {
                    if (dailyResetTask == null || dailyResetTask.IsCompleted)
                    {
                        // Minute quota of calls has been reach
                        NextReady = DateTime.Now.AddHours(1);
                        OnStateChange(true);

                        dailyResetTask = Task.Run((Func<Task?>)async delegate
                        {
                            if (marketDataRequest != null)
                                marketDataRequest.MarketDataRequestStatus = MarketDataRequestStatus.DELAYED;
                            ColourConsole.WriteLine($"MarketData - delayed by 1 hour.", ConsoleColor.White, ConsoleColor.DarkRed);
                            await Task.Delay(GetWhenNextReady());
                            ColourConsole.WriteLine($"MarketData - resuming.", ConsoleColor.White, ConsoleColor.DarkRed);
                            CallCountInMinute = 0;
                            ErrorCount = 0;
                            OnStateChange(true);
                            if (marketDataRequest != null)
                                marketDataRequest.MarketDataRequestStatus = MarketDataRequestStatus.PENDING;
                        });
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        internal DateTime GetValidUntil(Interval? interval = null)
        {
            return interval != null ? CalculatedValidUntilDateTime(interval.Value) : DateTime.Now.AddMinutes(15);
        }

        internal static DateTime CalculatedValidUntilDateTime(Interval interval)
        {
            DateTime validUntil = DateTime.Now;
            switch (interval)
            {
                case Interval.Min1:
                    validUntil = validUntil.AddMinutes(1);
                    break;
                case Interval.Min5:
                    validUntil = validUntil.AddMinutes(5);
                    break;
                case Interval.Min15:
                    validUntil = validUntil.AddMinutes(15);
                    break;
                case Interval.Min30:
                    validUntil = validUntil.AddMinutes(30);
                    break;
                case Interval.Min60:
                    validUntil = validUntil.AddHours(1);
                    break;
                case Interval.Daily:
                    validUntil = validUntil.AddDays(1);
                    break;
                case Interval.Weekly:
                    validUntil = validUntil.AddDays(7);
                    break;
                case Interval.Monthly:
                    validUntil = validUntil.AddMonths(1);
                    break;
            }
            return validUntil;
        }

        private async Task DoRequest(MarketDataRequest marketDataRequest, Func<Task> taskAction)
        {
            try
            {

                await IncrementActiveCallCount();

                await PauseRequest(marketDataRequest); // It may be OFFLINE

                await IncrementMinuteCallCount(marketDataRequest);

                await PauseRequest(marketDataRequest); // It may be DELAYED

                await taskAction();

                ErrorCount = 0;
                logger.IncrementCount();

                ColourConsole.WriteLine($"MarketData - {marketDataRequest.Description} success.", ConsoleColor.DarkGreen, ConsoleColor.White);
            }
            catch (Exception ex)
            {
                marketDataRequest.ErrorCount++;
                ColourConsole.WriteLine($"MarketData - {marketDataRequest.Description} failed (Error count: {marketDataRequest.ErrorCount}).", ConsoleColor.DarkRed, ConsoleColor.White);
                await IncrementErrorCount(marketDataRequest);
            }
            finally
            {
                await DecrementActiveCallCount();
            }
        }

        private void OnStateChange(bool force = false)
        {
            MarketDataStatus newRequestStatus = MarketDataStatus.OKAY;

            if (ErrorCount >= MAX_ERRORS)
                newRequestStatus = MarketDataStatus.OFFLINE;
            else if (CallCountInMinute >= MaxHitsPerMinute && newRequestStatus != MarketDataStatus.OFFLINE)
                newRequestStatus = MarketDataStatus.DELAYED;
            else if (ActiveCallsCount > 0 && newRequestStatus == MarketDataStatus.OKAY)
                newRequestStatus = MarketDataStatus.LOADING;

            if (force)
            {
                Status = newRequestStatus;
                StateChange?.Invoke();
            }
            else if (newRequestStatus != Status)
            {
                // Only change and fire event if the Status actually changes
                Status = newRequestStatus;
                StateChange?.Invoke();
            }
        }


    }
}

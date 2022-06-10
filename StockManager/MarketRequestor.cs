using AlphaVantage.Net.Common.Intervals;
using AlphaVantage.Net.Stocks;
using Elements;

namespace StockManager
{

    public delegate void MarketRequestorsNotify(Guid id, string name);      // delegate

    public class MarketRequestor
    {

        public event MarketRequestorsNotify? ResultReady;                   // event

        private const int DELAY_BEFORE_STARTING_QUEUE = 1;  // In seconds

        private PriorityAsyncronousQueueProcessor requestQueueProcessor = new PriorityAsyncronousQueueProcessor();
        private MarketData marketData;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource queueStartTokenSource = new CancellationTokenSource();

        public MarketRequestor(MarketData marketData)
        {
            this.marketData = marketData;

            //this.store = new DataStorage<Store>(new Store());
            //store.Load();

            requestQueueProcessor.Added += (Action action) =>
            {

            };
            requestQueueProcessor.Processing += (Action action) =>
            {

            };
            requestQueueProcessor.Finished += (Action action) =>
            {

            };
            requestQueueProcessor.Started += () =>
            {

            };
            requestQueueProcessor.AllFinished += () =>
            {

            };
        }

        public RequestResult GetQuote(string symbol, QueuePriority priority, Guid? recordId = null)
        {
            QuoteRequest request = new QuoteRequest(symbol);
            string description = $"Quote (Symbol:{symbol})";

            var previousRetrieval = marketData.FindPreviousMatchingRetrieval(request);
            if (previousRetrieval != null)
            {
                ColourConsole.WriteLine($"MarketRequestor - {description} previous data found.", ConsoleColor.DarkGreen, ConsoleColor.White);

                // Found a matching previous request, so mock the request
                if (previousRetrieval.RecordId != null)
                {
                    return new RequestResult(previousRetrieval.RecordId.Value, MarketDataRequestStatus.SUCCESS);
                }
            }

            recordId = recordId ?? Guid.NewGuid(); // Transaction id
            Action action = new Action(async () =>
            {
                var marketDataRequest = new MarketDataRequest<QuoteRequest, GlobalQuote>(
                    request,
                    description,
                    recordId);

                await marketData.GetQuote(marketDataRequest);

                if (marketDataRequest.MarketDataRequestStatus == MarketDataRequestStatus.SUCCESS && marketDataRequest.Resulting != null)
                {
                    if (marketDataRequest.RecordId != null)
                        ResultReady?.Invoke(marketDataRequest.RecordId.Value, "TimeSeries");
                }
            });

            _ = requestQueueProcessor.Add(action, priority);
            SetDelayedReaction();

            return new RequestResult(recordId, MarketDataRequestStatus.PENDING);
        }

        public GlobalQuote? GetQuote(Guid id)
        {
            var result = marketData.GetQuoteRetrieval(id);
            if (result != null)
            {
                return result.GetResult();
            }
            return null;
        }

        public RequestResult GetTimeSeries(string symbol, Interval interval, QueuePriority priority, Guid? recordId = null)
        {
            TimeSerieseRequest request = new TimeSerieseRequest(symbol, interval);
            string description = $"Time Series (Symbol:{symbol};Interval:{interval})";

            var previousRetrieval = marketData.FindPreviousMatchingRetrieval(request);
            if (previousRetrieval != null)
            {
                ColourConsole.WriteLine($"MarketRequestor - {description} previous data found.", ConsoleColor.DarkGreen, ConsoleColor.White);

                // Found a matching previous request, so mock the request
                if (previousRetrieval.RecordId != null)
                {
                    return new RequestResult(previousRetrieval.RecordId.Value, MarketDataRequestStatus.SUCCESS);
                }
            }

            recordId = recordId ?? Guid.NewGuid(); // Transaction id
            Action action = new Action(async () =>
            {
                var marketDataRequest = new MarketDataRequest<TimeSerieseRequest, StockTimeSeries>(
                    request,
                    description,
                    recordId);

                await marketData.GetTimeSeries(marketDataRequest);

                if (marketDataRequest.MarketDataRequestStatus == MarketDataRequestStatus.SUCCESS && marketDataRequest.Resulting != null)
                {
                    if (marketDataRequest.RecordId != null)
                        ResultReady?.Invoke(marketDataRequest.RecordId.Value, "TimeSeries");
                }
            });

            _ = requestQueueProcessor.Add(action, priority);
            SetDelayedReaction();

            return new RequestResult(recordId, MarketDataRequestStatus.PENDING);
        }

        public StockTimeSeries? GetTimeSeries(Guid id)
        {
            var retrieval = marketData.GetTimeSeriesRetrieval(id);
            if (retrieval != null)
            {
                return retrieval.GetResult();
            }
            return null;
        }

        private void SetDelayedReaction()
        {
            // A bunch of gaherer actions have been added to the queue, but more may come in very soon
            // thus setting of the queue processing again after the first lot have bee process.
            // This function should stop this by adding a delay to the processing start to allow a bigger queue
            // before we go

            // Cancel any previous task
            queueStartTokenSource.Cancel();
            //ColourConsole.WriteWarning($"Gatherer(SetDelayedReaction) - cancelling task.");            

            //ColourConsole.WriteWarning($"Gatherer(SetDelayedReaction) - running (delayed) task.");
            queueStartTokenSource = new CancellationTokenSource();
            var task = Task.Run(async delegate
            {
                // Start after a number of seconds unless cancelled
                await Task.Delay(TimeSpan.FromSeconds(DELAY_BEFORE_STARTING_QUEUE), queueStartTokenSource.Token);
                await requestQueueProcessor.Run();
            }, queueStartTokenSource.Token);
        }


    }
}

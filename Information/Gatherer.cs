using Companies;
using Elements;
using News;
using TextAnalysis;

namespace Information
{

    public delegate void GathererInterestingItemsNotify(int numberOfInterestingItems);                          // delegate

    public class Gatherer
    {
        public event GathererInterestingItemsNotify? InterestedItemsChanged;                                       // event

        private const AnalysisConfidence MINIMUM_CONFIDENCE = AnalysisConfidence.HIGH;

        /*
         * This will get all the newsitems from NewsManager (Souces/feeds)
         * into one big list of NewsItems.
         * It will check if it has all of them against its own list of
         * interesting and non-interesting items.
         * Anything new, will be added to a queue to process.
         * After, it will react to events from the NewsManager regarding new NewsUtems,
         * and add them to the queue.
         * The queue will be processed for interesting news.
         * Anything interesting will be recorded, and non-interesting will also be recorded.
         * If something IS interesting, an event will be fired for processing up the chain.
         * 
         */

        private AsyncronousQueueProcessor newsItemsQueueProcessor;
        private CancellationTokenSource queueStartTokenSource = new CancellationTokenSource();
        private int interestingItemCount = 0;
        private CompanyDataStore companyDataStore;
        private NewsManager newsManager;
        private GathererInformation gathererInformation;

        public Gatherer(CompanyDataStore companyDataStore, NewsManager newsManager, GathererInformation gathererInformation)
        {
            this.companyDataStore = companyDataStore;
            this.gathererInformation = gathererInformation;
            this.newsManager = newsManager;

            newsManager.FreshArrivals += NewsManager_FreshArrivals;

            newsItemsQueueProcessor = new AsyncronousQueueProcessor();
            newsItemsQueueProcessor.Added += (Action action) =>
            {
                ColourConsole.WriteInfo($"Gatherer - queue action added.");
            };
            newsItemsQueueProcessor.Processing += (Action action) =>
            {
                //ColourConsole.WriteInfo($"Gatherer - queue action processing...");
            };
            newsItemsQueueProcessor.Finished += (Action action) =>
            {
                //ColourConsole.WriteInfo($"Gatherer - ...queue action finished.");
            };
            newsItemsQueueProcessor.Started += () =>
            {
                ColourConsole.WriteInfo($"Gatherer - queue started.");
            };
            newsItemsQueueProcessor.AllFinished += () =>
            {
                ColourConsole.WriteInfo($"Gatherer - queue actions complete.");
                UpdateInformation();
            };

            GatherAllNews();
        }

        public void MarkNotInteresting(Guid id)
        {
            if (gathererInformation.MarkNotInteresting(id))
            {
                InterestedItemsChanged?.Invoke(-1);
            }
        }

        private void UpdateInformation()
        {
            gathererInformation.LatestDate = DateTime.Now;
            if (interestingItemCount > 0)
                ColourConsole.WriteLine($"Gatherer - Found " + interestingItemCount + " interesting item(s).", ConsoleColor.Yellow, ConsoleColor.DarkGreen);
            else
                ColourConsole.WriteLine($"Gatherer - Found no interesting items.", ConsoleColor.Yellow, ConsoleColor.DarkRed);
            InterestedItemsChanged?.Invoke(interestingItemCount);
            interestingItemCount = 0;
        }

        private void GatherAllNews()
        {
            ColourConsole.WriteInfo($"Gatherer - all new NewsItems, to be processed, started ...");

            List<NewsItem> allNewsItems = new List<NewsItem>();
            foreach (var source in newsManager.Sources)
            {
                List<NewsItem>? newsItems = source?.NewsItems;
                if (newsItems != null)
                    allNewsItems.AddRange(newsItems);
            }


            // Now check against our two (Interesting and Noninteresting) lists
            // and if there is something new, we can investigate it
            List<GathererInformationItem> combinedTwoList = new List<GathererInformationItem>();
            combinedTwoList.AddRange(gathererInformation.InterestingItems);
            combinedTwoList.AddRange(gathererInformation.NonInterestingItems);
            List<GathererInformationItem> newItemsList = new List<GathererInformationItem>();
            allNewsItems.RemoveAll(x => combinedTwoList.Find(y => y.Id == x.Id) != null);

            // Now all everything we have never seen before to a queue for processing
            if (allNewsItems.Count > 0)
            {
                _ = EnqueueNewsItems(allNewsItems);
            }
            else
            {
                ColourConsole.WriteInfo($"Gatherer - ...no new items to process.");
            }
        }

        private async Task ProcessQueuedAction(NewsItem newsItem)
        {
            //ColourConsole.WriteInfo($"Gatherer - now looking at NewsItem [{newsItem.Id}].");

            Analyser analyser = new Analyser(companyDataStore);

            await analyser.Analyse(newsItem);

            AnalysisInfo info = analyser.Info;

            if (info.Interesting)
            {
                AnalysisConfidence highestConfidence = info.HighestConfidence;
                if (highestConfidence >= MINIMUM_CONFIDENCE)
                {

                    ColourConsole.WriteInfo(
                        $"--------------- Start Analysis --------------------------------------------------------------- ");

                    ColourConsole.WriteInfo(
                        $"Gatherer - found something interesting in NewsItem [{newsItem.Id}] " +
                        $"with Text:");

                    ColourConsole.WriteNormal(info.Text);
                    string dateString = newsItem.PublishDate.ToString("dddd, dd MMMM yyyy hh:mm tt");
                    ColourConsole.WriteInfo(
                        $"Published at [{dateString}]");

                    foreach (var finding in info.Findings)
                    {
                        string companyInfo = $"{finding.Company.Name} ({finding.Company.Symbol}:{finding.Company.Exchange})";
                        string message =
                                $"AnalysisInfo - Company " +
                                $"[{companyInfo}] " +
                                $"with Confidence [{finding.Confidence}] " +
                                $"and with Rationale [{finding.Rationale}] " +
                                $"has been found in a news item.";

                        switch (finding.Confidence)
                        {
                            case AnalysisConfidence.LOW:
                                ColourConsole.WriteDanger(message);
                                break;
                            case AnalysisConfidence.MEDIUM:
                                ColourConsole.WriteWarning(message);
                                break;
                            case AnalysisConfidence.HIGH:
                                ColourConsole.WriteSuccess(message);
                                break;
                        }
                    }

                    // We have least one finding with a confidence high enough
                    gathererInformation.AddInteresingItem(info);
                    interestingItemCount++;

                    ColourConsole.WriteInfo(
                        $"--------------- End Analysis ----------------------------------------------------------------- ");
                }
                else
                {
                    gathererInformation.AddNoninteresingItem(info);
                }
            }
            else
            {
                gathererInformation.AddNoninteresingItem(info);
            }
        }

        private void NewsManager_FreshArrivals(Guid id, List<NewsItem> freshNewsItems)
        {
            string? FeedTitle = newsManager.GetSourceFeedTitle(id);
            ColourConsole.WriteLine($"Gatherer - " + freshNewsItems.Count + " fresh NewsItem(s) have arrived from '" + FeedTitle + "'.", ConsoleColor.White, ConsoleColor.DarkGreen);
            _ = EnqueueNewsItems(freshNewsItems);
        }

        private async Task EnqueueNewsItems(List<NewsItem> newsItems)
        {
            foreach (var newsItem in newsItems)
            {
                await EnqueueNewsItemAction(newsItem);
            };
            ColourConsole.WriteInfo($"Gatherer - all fresh NewsItems, to be processed, have been added to the queue.");
            SetDelayedReaction();
        }

        private async Task EnqueueNewsItemAction(NewsItem newsItem)
        {
            Action action = new Action(() =>
            {
                _ = ProcessQueuedAction(newsItem);
            });
            await newsItemsQueueProcessor.Add(action);
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
                // Start after 5 seconds unless cancelled
                await Task.Delay(TimeSpan.FromSeconds(5), queueStartTokenSource.Token);
                await newsItemsQueueProcessor.Run();
                //ColourConsole.WriteWarning($"Gatherer(SetDelayedReaction) - (delayed) task is running.");
            }, queueStartTokenSource.Token);

        }

    }

}

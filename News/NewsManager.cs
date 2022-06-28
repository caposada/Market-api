using Configuration;
using Elements;

namespace News
{

    // This runs on its own, plodding away
    public class NewsManager
    {
        public delegate void NewsManagerNotify(Guid id);                                // delegate
        public delegate void FeedFreshArrival(Guid id, List<NewsItem> freshNewsItems);  // delegate
        public delegate void SourceMonitorNotify(Guid id, string eventName);            // delegate

        public event NewsManagerNotify? Added;                           // event
        public event NewsManagerNotify? Removed;                         // event
        public event FeedFreshArrival? FreshArrivals;                    // event
        public event SourceMonitorNotify? SourceMonitorChanged;          // event

        private NewsData newsData;

        public List<Source> Sources { get; set; }
        public List<NewsItem> NewsItems
        {
            get
            {
                return this.newsData.NewsItems;
            }
        }

        public NewsManager()
        {
            this.newsData = new NewsData();

            using (NewsContext context = new NewsContext())
            {
                Sources = context.Sources.ToList();
                foreach (Source source in Sources)
                {
                    source.Setup();
                    source.SourceMonitor.Changed += Source_SourceMonitorChanged; // Watch for general changes (event)
                    source.SourceMonitor.FreshArrivals += Source_FreshArrivalsAsync;
                    source.Start();
                }
            }

        }

        public List<NewsItem> GetNewsItems(Guid id)
        {
            return newsData.NewsItems.Where(x => x.SourceId == id).OrderByDescending(x => x.PublishDate).ToList();
        }

        public int GetNewsItemsCount(Guid id)
        {
            return GetNewsItems(id).Count;
        }

        public DateTimeOffset GetNewsItemsLastPublish(Guid id)
        {
            var newsItemsInOrder = newsData.NewsItems.Where(x => x.SourceId == id).OrderByDescending(x => x.PublishDate).ToList();
            return newsItemsInOrder.Count > 0 ? newsItemsInOrder.First().PublishDate : DateTimeOffset.MinValue;
        }

        public string? GetSourceFeedTitle(Guid id)
        {
            var source = Sources.Where(x => x.Id == id).First();
            if (source != null)
                return source.FeedTitle;
            return null;
        }

        public Source? GetSource(Guid id)
        {
            return Sources.Where(x => x.Id == id).First();
        }

        public void SetTimezone(Guid id, string timezone)
        {
            using (NewsContext context = new NewsContext())
            {
                var source = Sources.Where(x => x.Id == id).First();
                source.Timezone = timezone;

                context.Sources.Where(x => x.Id == id).First().Timezone = timezone;
                context.SaveChanges();
                SourceMonitorChanged?.Invoke(id, "TIMEZONE");
            }
        }

        public void SetPollingTimespan(Guid id, TimeSpan pollingTimespan)
        {
            using (NewsContext context = new NewsContext())
            {
                var source = Sources.Where(x => x.Id == id).First();
                source.SetPollingTimespan(pollingTimespan);

                context.Sources.Where(x => x.Id == id).First().PollingTimespan = source.PollingTimespan;
                context.SaveChanges();
                SourceMonitorChanged?.Invoke(id, "ISPOLLING");
            }
        }

        public void SetPolling(Guid id, bool polling)
        {
            var source = Sources.Where(x => x.Id == id).First();
            source.SetPolling(polling);
            SourceMonitorChanged?.Invoke(id, "POLLINGTIMESPAN");
        }

        public void AddFeed(FeedType feedType, string title, string url, string timezone)
        {
            using (NewsContext context = new NewsContext())
            {
                if (!context.Sources.Any(x => x.FeedTitle == title || x.FeedUrl == url))
                {
                    Source source = new Source(
                        feedType,
                        title,
                        url,
                        timezone,
                        Constants.DEFAULT_POLLING_PERIOD);
                    source.Setup();
                    source.SourceMonitor.FreshArrivals += Source_FreshArrivalsAsync;     //Watch for new NewsItems(event)
                    source.SourceMonitor.Changed += Source_SourceMonitorChanged;    // Watch for general changes (event)
                    source.Start();

                    Sources.Add(source);
                    context.Sources.Add(source);
                    context.SaveChanges();

                    Added?.Invoke(source.Id);
                }
                else
                {
                    ColourConsole.WriteWarning($"NewsManager - Source (RssFeed) can not add [{title}] already exists!");
                }
            }
        }

        public void Remove(Source source)
        {
            source.SourceMonitor.Changed -= Source_SourceMonitorChanged;        // Stop watching for general changes (event)
            source.SourceMonitor.FreshArrivals -= Source_FreshArrivalsAsync;         // Stop watching for new NewsItems (event)

            source.SourceMonitor.ShutDown();

            using (NewsContext context = new NewsContext())
            {
                Sources.Remove(source);
                context.Sources.Remove(source);
                context.SaveChanges();
            }

            Removed?.Invoke(source.Id);
        }

        public void Remove(Guid id)
        {
            Source? source = Sources.Find(x => x.Id == id);
            if (source != null)
            {
                Remove(source);
            }
        }

        //// Temp - shouldn't be needed (keep an eye on the feeds/NewsManagerSettingsStore folder for GUID folders that don't belong anymore)
        //public async Task CleanAsync()
        //{
        //    List<Guid> currentIds = Sources.Select(x => x.Id).ToList();
        //    await Store.CleanAsync(currentIds);

        //    foreach (var source in Sources)
        //    {
        //        source.Clean();
        //    }
        //}

        //public void Clean()
        //{
        //    _ = newsData.CleanAsync();
        //}

        public void CheckMissingFeeds()
        {
            using (ConfigurationContext context = new ConfigurationContext())
            {
                var standardSources = context.StandardSources.ToList();
                foreach (var standardSource in standardSources)
                {
                    bool found = Sources.Any(x =>
                        x.FeedType == standardSource.FeedType
                        && x.FeedTitle == standardSource.FeedTitle
                        && x.FeedUrl == standardSource.FeedUrl);
                    if (!found)
                    {
                        // This feed is missing!
                        AddFeed(standardSource.FeedType, standardSource.FeedTitle, standardSource.FeedUrl, standardSource.Timezone);
                    }
                }
            }
        }

        public void ExpungeAllNewsItemsForEverySource()
        {
            _ = newsData.ExpungeAllNewsItemsAsync();
        }

        private void Source_SourceMonitorChanged(Guid id, string eventName)
        {
            SourceMonitorChanged?.Invoke(id, eventName);
        }

        private void Source_FreshArrivalsAsync(Guid id, List<NewsItem> freshNewsItems)
        {
            List<NewsItem> brandNewNewsItems = newsData.ProcessFreshItemsAsync(freshNewsItems);
            if (brandNewNewsItems.Count > 0)
            {
                // Fire event!!!!
                FreshArrivals?.Invoke(id, brandNewNewsItems);
            }
        }

    }

}

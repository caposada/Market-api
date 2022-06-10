using Elements;
using static News.Source;

namespace News
{

    // This runs on its own, plodding away
    public class NewsManager
    {
        public delegate void NewsManagerNotify(Guid id);                  // delegate

        public event NewsManagerNotify? Added;                           // event
        public event NewsManagerNotify? Removed;                         // event
        public event FeedFreshArrival? FreshArrivals;                    // event
        public event SourceMonitorNotify? SourceMonitorChanged;          // event

        public class Settings
        {
            public class StandardNewsSource
            {
                public string? Timezone { get; set; }
                public FeedType FeedType { get; set; }
                public string FeedTitle { get; set; }
                public string FeedUrl { get; set; }
            }

            public class Store : StoreBase
            {
                public List<StandardNewsSource> StandardNewsSources { get; set; }

                public Store()
                {
                }

                public override string GetFilename()
                {
                    return "Settings_NewsManager";
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

            public List<StandardNewsSource> StandardNewsSources
            {
                get
                {
                    return store.Data.StandardNewsSources;
                }
            }

            private DataStorage<Store> store;

            public Settings()
            {
                this.store = new DataStorage<Store>(new Store());
                this.store.Load();
            }

        }

        public class Store : StoreBase
        {

            public List<Source> Sources { get; set; }

            public Store()
            {
                this.Sources = new List<Source>();
            }

            public override string GetFilename()
            {
                return "Sources";
            }

            public override string? GetFolderName()
            {
                return null;
            }

            public override string GetPathPrefix()
            {
                return Constants.FEED_FOLDER_NAME;
            }
        }

        public List<Source> Sources
        {
            get
            {
                return store.Data.Sources;
            }
        }

        private DataStorage<Store> store;
        private Settings settings;

        public NewsManager()
        {
            this.store = new DataStorage<Store>(new Store());
            this.store.Load();

            // Add event watching for every feed
            foreach (Source source in this.store.Data.Sources)
            {
                source.Load();
                source.SourceMonitorChanged += Source_SourceMonitorChanged; // Watch for general changes (event)
                source.FreshArrivals += Source_FreshArrivals;
            }
        }

        public void SaveChanges()
        {
            store.Save();
        }

        public string? GetSourceFeedTitle(Guid id)
        {
            var sourced = Sources.Find(x => x.Id == id);
            if (sourced != null)
                return sourced.FeedTitle;
            return null;
        }

        public Source? GetSource(Guid id)
        {
            return Sources.Find(x => x.Id == id);
        }

        public void AddFeed(FeedType feedType, string title, string url, string timezone)
        {
            if (!store.Data.Sources.Any(x => x.FeedTitle == title || x.FeedUrl == url))
            {
                Source source = new Source(
                    feedType,
                    title,
                    url,
                    timezone);
                source.SourceMonitorChanged += Source_SourceMonitorChanged; // Watch for general changes (event)
                source.FreshArrivals += Source_FreshArrivals;               //Watch for new NewsItems(event)
                Sources.Add(source);
                store.Save();

                Added?.Invoke(source.Id);
            }
            else
            {
                ColourConsole.WriteWarning($"Source (RssFeed) - Can not add [{title}] already exists!");
            }
        }

        public void Remove(Source source)
        {
            source.SourceMonitorChanged -= Source_SourceMonitorChanged;     // Stop watching for general changes (event)
            source.FreshArrivals -= Source_FreshArrivals;                   // Stop watching for new NewsItems (event)

            source.SourceMonitor.ShutDown();
            source.Destroy();

            Sources.Remove(source);
            store.Save();

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

        // Temp - shouldn't be needed (keep an eye on the feeds/Store folder for GUID folders that don't belong anymore)
        public async Task CleanAsync()
        {
            List<Guid> currentIds = Sources.Select(x => x.Id).ToList();
            await store.CleanAsync(currentIds);
        }

        public void CheckMissingFeeds()
        {
            if (settings == null)
                settings = new Settings();

            foreach (var standardNewsSource in settings.StandardNewsSources)
            {
                bool found = Sources.Any(x =>
                    x.FeedType == standardNewsSource.FeedType
                    && x.FeedTitle == standardNewsSource.FeedTitle
                    && x.FeedUrl == standardNewsSource.FeedUrl);
                if (!found)
                {
                    // This feed is missing!
                    AddFeed(standardNewsSource.FeedType, standardNewsSource.FeedTitle, standardNewsSource.FeedUrl, standardNewsSource.Timezone);
                }
            }
        }

        public void ExpungeAllNewsItemsForEverySource()
        {
            foreach (Source source in Sources)
            {
                source.ExpungeAllNewsItems();
            }
        }

        private void Source_SourceMonitorChanged(Guid id, string eventName)
        {
            SourceMonitorChanged?.Invoke(id, eventName);
        }

        private void Source_FreshArrivals(Guid id, List<NewsItem> freshNewsItems)
        {
            FreshArrivals?.Invoke(id, freshNewsItems);
        }

    }

}

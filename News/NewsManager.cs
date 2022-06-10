using Elements;
using System.Text.Json.Serialization;
using static News.Source;

namespace News
{

    // This runs on its own, plodding away
    public class NewsManager : IDataStoragable<SourcesStore>
    {
        public delegate void NewsManagerNotify(Guid id);                  // delegate

        public event NewsManagerNotify? Added;                           // event
        public event NewsManagerNotify? Removed;                         // event
        public event FeedFreshArrival? FreshArrivals;                    // event
        public event SourceMonitorNotify? SourceMonitorChanged;          // event

        public class Settings : IDataStoragable<NewsManagerSettingsStore>
        {            

            public List<NewsManagerSettingsStore.StandardNewsSource> StandardNewsSources
            {
                get
                {
                    return Store.Data.StandardNewsSources;
                }
            }

            [JsonIgnore]
            public DataStorage<NewsManagerSettingsStore>? Store { get; set; }

            public Settings()
            {
                this.Store = new DataStorage<NewsManagerSettingsStore>(new NewsManagerSettingsStore());
                this.Store.Load();
            }

            public void Destroy()
            {
                throw new NotImplementedException();
            }
        }
            
        [JsonIgnore]
        public DataStorage<SourcesStore>? Store { get; set; }

        public List<Source> Sources
        {
            get
            {
                return Store.Data.Sources;
            }
        }

        private Settings settings;

        public NewsManager()
        {
            this.Store = new DataStorage<SourcesStore>(new SourcesStore());
            this.Store.Load();

            // Add event watching for every feed
            foreach (Source source in this.Store.Data.Sources)
            {
                source.Load();
                source.SourceMonitorChanged += Source_SourceMonitorChanged; // Watch for general changes (event)
                source.FreshArrivals += Source_FreshArrivals;
            }
        }

        public void SaveChanges()
        {
            Store.Save();
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
            if (!Store.Data.Sources.Any(x => x.FeedTitle == title || x.FeedUrl == url))
            {
                Source source = new Source(
                    feedType,
                    title,
                    url,
                    timezone);
                source.SourceMonitorChanged += Source_SourceMonitorChanged; // Watch for general changes (event)
                source.FreshArrivals += Source_FreshArrivals;               //Watch for new NewsItems(event)
                Sources.Add(source);
                Store.Save();

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
            Store.Save();

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

        // Temp - shouldn't be needed (keep an eye on the feeds/NewsManagerSettingsStore folder for GUID folders that don't belong anymore)
        public async Task CleanAsync()
        {
            List<Guid> currentIds = Sources.Select(x => x.Id).ToList();
            await Store.CleanAsync(currentIds);
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

        public void Destroy()
        {
            throw new NotImplementedException();
        }
    }

}

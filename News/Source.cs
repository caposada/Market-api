using Elements;
using RssFeedReader;
using System.Text.Json.Serialization;
using TwitterFeedReader;

namespace News
{
    
    public class Source
    {
        public delegate void SourceMonitorNotify(Guid id, string eventName);            // delegate
        public delegate void FeedFreshArrival(Guid id, List<NewsItem> freshNewsItems);  // delegate

        public event SourceMonitorNotify? SourceMonitorChanged;                         // event
        public event FeedFreshArrival? FreshArrivals;                                   // event
               
        private NewsData newsData;

        [JsonIgnore]
        public SourceMonitor? SourceMonitor { get; set; }
        [JsonIgnore]
        public List<NewsItem> NewsItems
        {
            get
            {
                return this.newsData.NewsItems;
            }
        }
        [JsonIgnore]
        public DateTimeOffset? LastPublished
        {
            get
            {
                return this.newsData.LastPublished;
            }
        }

        public Guid Id { get; set; }
        public string? Timezone { get; set; }
        public FeedType FeedType { get; set; }
        public string FeedTitle { get; set; }
        public string FeedUrl { get; set; }

        [JsonConstructorAttribute]
        public Source()
        {
            // This constructor is used when sources/feeds are loaded at startup           
        }

        public Source(FeedType feedType, string title, string url, string timezone)
        {
            // This constructor is used when added a new source/feed
            this.Id = Guid.NewGuid();
            this.FeedType = feedType;
            this.FeedTitle = title;
            this.FeedUrl = url;
            this.Timezone = timezone;

            this.newsData = new NewsData(this.Id);

            NewsFeed? feed = MakeFeed();

            this.SourceMonitor = new SourceMonitor(this.Id, feed);
            this.SourceMonitor.Changed += SourceMonitor_Changed;
            this.SourceMonitor.FreshArrivals += SourceMonitor_FreshArrivals;
            this.SourceMonitor.Save();
            this.SourceMonitor.Run();    // Do a run
            this.SourceMonitor.Start();  // Start the periodic timer
        }

        public void Destroy()
        {
            newsData.Destroy();
            SourceMonitor.Destroy();
        }

        public void Load()
        {
            this.newsData = new NewsData(this.Id);

            NewsFeed? feed = MakeFeed();

            this.SourceMonitor = new SourceMonitor(this.Id, feed);
            this.SourceMonitor.Changed += SourceMonitor_Changed;
            this.SourceMonitor.FreshArrivals += SourceMonitor_FreshArrivals;
            this.SourceMonitor.Load();
            this.SourceMonitor.Run();    // Do a run
            this.SourceMonitor.Start();  // Start the periodic timer <-- Maybe we add a flag stored in datafile for on/off 
        }

        public void ExpungeAllNewsItems()
        {
            newsData.ExpungeAllNewsItems();
        }

        private void SourceMonitor_Changed(string eventName)
        {
            SourceMonitorChanged?.Invoke(this.Id, eventName);
        }

        private void SourceMonitor_FreshArrivals(List<NewsItem> freshNewsItems)
        {
            List<NewsItem> brandNewNewsItems = newsData.ProcessFreshItems(freshNewsItems);
            if (brandNewNewsItems.Count > 0)
            {
                // Fire event!!!!
                FreshArrivals(this.Id, brandNewNewsItems);

            }
        }

        public NewsFeed MakeFeed()
        {
            NewsFeed? feed = null;

            if (this.FeedType == FeedType.RssFeed)
            {
                feed = new RssFeed(this.Id, this.FeedTitle, this.FeedUrl);
            }
            else if (this.FeedType == FeedType.TwitterFeed)
            {
                feed = new TwitterFeed(this.Id, this.FeedTitle, this.FeedUrl);
            }

            return feed;
        }
    }
}

using Elements;
using RssFeedReader;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TwitterFeedReader;

namespace News
{

    public class Source
    {

        [NotMapped]
        public SourceMonitor? SourceMonitor { get; set; }

        [Key]
        public Guid Id { get; set; }
        public string Timezone { get; set; }
        public FeedType FeedType { get; set; }
        public string FeedTitle { get; set; }
        public string FeedUrl { get; set; }
        public TimeSpan PollingTimespan { get; set; }

        public Source(FeedType feedType, string feedTitle, string feedUrl, string timezone, TimeSpan pollingTimespan)
        {
            // This constructor is used when added a new source/feed
            this.Id = Guid.NewGuid();
            this.FeedType = feedType;
            this.FeedTitle = feedTitle;
            this.FeedUrl = feedUrl;
            this.Timezone = timezone;
            this.PollingTimespan = pollingTimespan;
        }

        public void SetPolling(bool polling)
        {
            SourceMonitor.IsPolling = polling;
        }

        public void SetPollingTimespan(TimeSpan pollingTimespan)
        {
            PollingTimespan = PollingTimespan.Add(pollingTimespan);
            SourceMonitor.PollingTimespan = PollingTimespan;
        }

        public void Setup()
        {
            Func<NewsFeed> NewsFeedFunc = MakeFeed;
            this.SourceMonitor = new SourceMonitor(this.Id, NewsFeedFunc, this.PollingTimespan);
        }

        public void Start()
        {
            this.SourceMonitor.Run();    // Do a run
            this.SourceMonitor.Start();  // Start the periodic timer
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

using Elements;
using Tweetinvi;
using Tweetinvi.Models;

namespace TwitterFeedReader
{
    public class TwitterFeed : NewsFeed
    {
        public class Store : StoreBase
        {
            public string? Twitter_ApiKey { get; set; }
            public string? Twitter_ApiKeySecret { get; set; }
            public string? Twitter_AccessToken { get; set; }
            public string? Twitter_AccessTokenSecret { get; set; }

            public Store()
            {
            }

            public override string GetFilename()
            {
                return "Settings_Twitter";
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

        private static TwitterClient? userClient;
        private IUser? user = null;

        public TwitterFeed(Guid id, string title, string url)
            : base(id, title, url)
        {
        }

        protected async override Task<List<NewsItem>> GetNews()
        {
            Setup();

            if (user == null && userClient != null)
                user = await userClient.Users.GetUserAsync(this.Url);

            List<NewsItem> newsItems = new List<NewsItem>();
            try
            {
                if (user != null)
                {
                    ITweet[] tweets = await user.GetUserTimelineAsync();
                    foreach (var tweet in tweets)
                    {
                        DateTimeOffset correctDateTime = GetCorrectDateTimeOffset(tweet.CreatedAt);
                        NewsItem newsItem = new NewsItem(tweet.IdStr, correctDateTime, tweet.FullText, this);
                        newsItems.Add(newsItem);
                    }
                }
            }
            catch (Exception ex)
            {
                ColourConsole.WriteError($"NewsFeed (TwitterFeed) [{this.Title}] Exception: " + ex.Message);
            }
            return newsItems;
        }

        private void Setup()
        {
            if (userClient == null)
            {
                DataStorage<Store> store = new DataStorage<Store>(new Store());
                store.Load();
                userClient = new TwitterClient(
                    store.Data.Twitter_ApiKey,
                    store.Data.Twitter_ApiKeySecret,
                    store.Data.Twitter_AccessToken,
                    store.Data.Twitter_AccessTokenSecret);
            }
        }
    }
}
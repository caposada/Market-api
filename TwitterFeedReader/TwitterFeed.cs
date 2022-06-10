using Elements;
using System.Text.Json.Serialization;
using Tweetinvi;
using Tweetinvi.Models;

namespace TwitterFeedReader
{   
    public class TwitterFeed : NewsFeed, IDataStoragable<TwitterSettingsStore>
    {        

        [JsonIgnore]
        public DataStorage<TwitterSettingsStore>? Store { get; set; }

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
                Store = new DataStorage<TwitterSettingsStore>(new TwitterSettingsStore());
                Store.Load();
                userClient = new TwitterClient(
                    Store.Data.Twitter_ApiKey,
                    Store.Data.Twitter_ApiKeySecret,
                    Store.Data.Twitter_AccessToken,
                    Store.Data.Twitter_AccessTokenSecret);
            }
        }

        public void Destroy()
        {
            throw new NotImplementedException();
        }
    }
}
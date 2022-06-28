using Configuration;
using Elements;
using Tweetinvi;
using Tweetinvi.Models;

namespace TwitterFeedReader
{
    public class TwitterFeed : NewsFeed
    {

        private static TwitterClient? userClient;
        private IUser? user = null;
        protected static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 5);

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
                    ITweet[] tweets;

                    try
                    {
                        await _semaphore.WaitAsync();

                        ColourConsole.WriteInfo($"NewsFeed (TwitterFeed) - getting news for [{this.Title}]: ");
                        tweets = await user.GetUserTimelineAsync();
                    }
                    finally
                    {
                        _semaphore.Release();
                    }

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
                ColourConsole.WriteError($"NewsFeed (TwitterFeed) - [{this.Title}] Exception: " + ex.Message);
            }
            return newsItems;
        }

        private void Setup()
        {
            if (userClient == null)
            {
                using (ConfigurationContext context = new ConfigurationContext())
                {
                    userClient = new TwitterClient(
                        context.Settings.Where(x => x.Name == "Twitter_ApiKey").First().Value,
                        context.Settings.Where(x => x.Name == "Twitter_ApiKeySecret").First().Value,
                        context.Settings.Where(x => x.Name == "Twitter_AccessToken").First().Value,
                        context.Settings.Where(x => x.Name == "Twitter_AccessTokenSecret").First().Value);
                }
            }
        }

    }
}
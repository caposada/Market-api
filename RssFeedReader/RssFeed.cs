using Elements;
using System.ServiceModel.Syndication;
using System.Xml;

namespace RssFeedReader
{
    public class RssFeed : NewsFeed
    {

        public RssFeed(Guid id, string title, string url)
            : base(id, title, url)
        {
        }

        protected async override Task<List<NewsItem>> GetNews()
        {
            List<NewsItem> newsItems = new List<NewsItem>();
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "SO/1.0"); // < ---Huh ? Nasdaq won't work without it

                ColourConsole.WriteInfo($"NewsFeed (RssFeed) - getting news for [{this.Title}]: ");
                HttpResponseMessage response = await client.GetAsync(this.Url);
                response.EnsureSuccessStatusCode();

                string rss = await response.Content.ReadAsStringAsync();
                XmlReader reader = XmlReader.Create(new StringReader(rss));
                var feed = SyndicationFeed.Load(reader);

                foreach (var item in feed.Items.Reverse())
                {
                    if (item.Title != null)
                    {
                        string? refId = item.Id != null ? item.Id : null;
                        DateTimeOffset correctDateTime = GetCorrectDateTimeOffset(item.PublishDate);
                        NewsItem newsItem = new NewsItem(refId, correctDateTime, item.Title.Text, this);
                        newsItems.Add(newsItem);
                    }
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                ColourConsole.WriteError($"NewsFeed (RssFeed) - [{this.Title}] Exception: " + ex.Message);
            }
            return newsItems;
        }

    }
}

using DataStorage;
using Elements;
using System.Text.Json.Serialization;

namespace News
{
    public class NewsDataStore : StoreBase
    {

        public List<NewsItem> NewsItems
        {
            get
            {
                return newsItems.OrderByDescending(x => x.PublishDate).ToList();
            }
            set
            {
                newsItems = value;
            }
        }

        protected List<NewsItem> newsItems;
        protected string? folderName;

        [JsonConstructor]
        public NewsDataStore()
        {
            this.newsItems = new List<NewsItem>();
        }

        public NewsDataStore(string folderName)
        {
            this.folderName = folderName;
            this.newsItems = new List<NewsItem>();
        }

        public override string GetFilename()
        {
            return "NewsData";
        }

        public override string? GetFolderName()
        {
            return folderName;
        }

        public override string GetPathPrefix()
        {
            return Constants.FEED_FOLDER_NAME;
        }

        public void Add(NewsItem newNewsItem)
        {
            newsItems.Add(newNewsItem);
        }

        public void Add(List<NewsItem> newNewsItems)
        {
            newsItems.AddRange(newNewsItems);
        }

        public void ExpungeAll()
        {
            newsItems = new List<NewsItem>();
        }

    }
}

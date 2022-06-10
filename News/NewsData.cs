using Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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

    public class NewsData : IDataStoragable<NewsDataStore>
    {

        public List<NewsItem> NewsItems
        {
            get
            {
                return this.Store.Data.NewsItems;
            }
        }
        public DateTimeOffset LastPublished
        {
            get
            {
                return NewsItems.Count > 0 ? NewsItems[0].PublishDate : DateTimeOffset.MinValue;
            }
        }

        public DataStorage<NewsDataStore>? Store { get; set; }

        public NewsData(Guid id)
        {
            this.Store = new DataStorage<NewsDataStore>(new NewsDataStore(id.ToString()), "Store");
            this.Store.Load();
        }

        public void Destroy()
        {
            Store.Destroy();
        }

        public void Add(NewsItem newNewsItem)
        {
            Store.Data.Add(newNewsItem);
            Store.Save();
        }

        public void Add(List<NewsItem> newNewsItems)
        {
            Store.Data.Add(newNewsItems);
            Store.Save();
        }

        public List<NewsItem> ProcessFreshItems(List<NewsItem> freshNewsItems)
        {
            List<NewsItem> brandNewNewsItems = new List<NewsItem>();

            foreach (var freshNewsItem in freshNewsItems)
            {
                bool hasIdAlready = NewsItems.Any(x => x.Id == freshNewsItem.Id);
                bool hasTextAlready = NewsItems.Any(x => x.Text == freshNewsItem.Text);

                if (!hasIdAlready && !hasTextAlready)
                {
                    brandNewNewsItems.Add(freshNewsItem);
                }
            }

            if (brandNewNewsItems.Count > 0)
            {
                Add(brandNewNewsItems);
            }

            return brandNewNewsItems;
        }

        public void ExpungeAllNewsItems()
        {
            Store.Data.ExpungeAll();
            Store.Save();
        }
    }
}

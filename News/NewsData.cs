using Elements;

namespace News
{
    public class NewsData
    {

        public List<NewsItem> NewsItems
        {
            get
            {
                using (NewsContext context = new NewsContext())
                {
                    return context.NewsItems.ToList();
                }
            }
        }
        public DateTimeOffset LastPublished
        {
            get
            {
                using (NewsContext context = new NewsContext())
                {
                    var newsItems = context.NewsItems.ToList();
                    return newsItems.Count > 0 ? newsItems[0].PublishDate : DateTimeOffset.MinValue;
                }
            }
        }

        public NewsData()
        {
        }

        //public async Task CleanAsync()
        //{
        //    using (NewsContext context = new NewsContext())
        //    {
        //        DateTime oldesetDate = DateTime.Today - Constants.DEFAULT_CULL_PERIOD;
        //        var old = context.NewsItems.Where(x => x.SourceId == Id && x.PublishDate <= oldesetDate).ToList();
        //        if (old.Count > 0)
        //        {
        //            context.NewsItems.RemoveRange(old);
        //            await context.SaveChangesAsync();
        //        }
        //    }
        //}

        public async Task AddAsync(List<NewsItem> newNewsItems)
        {
            using (NewsContext context = new NewsContext())
            {
                context.NewsItems.AddRange(newNewsItems);
                await context.SaveChangesAsync();
            }
        }

        public List<NewsItem> ProcessFreshItemsAsync(List<NewsItem> freshNewsItems)
        {
            List<NewsItem> brandNewNewsItems = new List<NewsItem>();

            using (NewsContext context = new NewsContext())
            {
                foreach (var freshNewsItem in freshNewsItems)
                {
                    bool hasIdAlready = context.NewsItems.Any(x => x.Id == freshNewsItem.Id);
                    bool hasTextAlready = context.NewsItems.Any(x => x.Text == freshNewsItem.Text);

                    if (!hasIdAlready && !hasTextAlready)
                    {
                        brandNewNewsItems.Add(freshNewsItem);
                    }
                }
            }

            if (brandNewNewsItems.Count > 0)
            {
                _ = AddAsync(brandNewNewsItems);
            }

            return brandNewNewsItems;
        }

        public async Task ExpungeAllNewsItemsAsync()
        {
            using (NewsContext context = new NewsContext())
            {
                context.NewsItems.RemoveRange(context.NewsItems);
                await context.SaveChangesAsync();
            }
        }
    }
}

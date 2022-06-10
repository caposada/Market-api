using Microsoft.AspNetCore.Mvc;
using News;

namespace MarketWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        public class NewsDetails
        {
            public int TotalNumberOfNewsItems { get; set; }
            public List<string>? SourceIds { get; set; }
            public DateTimeOffset Latest { get; set; }
            public DateTimeOffset Earliest { get; set; }
            public int NumberOfRssFeeds { get; set; }
            public int NumberOfTwitterFeeds { get; set; }
        }

        public class NewsItem
        {
            public Guid Id { get; set; }
            public string? RefId { get; set; }
            public DateTimeOffset PublishDate { get; set; }
            public DateTime Timestamp { get; set; }
            public string Text { get; set; }
            public Guid SourceId { get; set; }
        }

        public class NewsSourceDetails
        {
            public string? SourceId { get; set; }
            public string? Title { get; set; }
            public string? Url { get; set; }
            public FeedType? FeedType { get; set; }
            public string? Timezone { get; set; }
            public bool? IsPolling { get; set; }
            public DateTime? LastPoll { get; set; }
            public TimeSpan? PollingTimespan { get; set; }
            public int? NewsItems_Count { get; set; }
            public DateTimeOffset? NewsItems_LastPublished { get; set; }
        }

        public class UpdateFeedDetails
        {
            public string? Timezone { get; set; }
        }

        public class AddSourceDetails
        {
            public FeedType FeedType { get; set; }
            public string Title { get; set; }
            public string Url { get; set; }
            public string Timezone { get; set; }

            public AddSourceDetails(FeedType feedType, string title, string url, string timezone)
            {
                this.FeedType = feedType;
                this.Title = title;
                this.Url = url;
                this.Timezone = timezone;
            }
        }

        private readonly Market.App marketApp;

        public NewsController(Market.App marketApp)
        {
            this.marketApp = marketApp;
        }

        [HttpPut("Clean")]
        public void Clean()
        {
            marketApp.NewsManager.CleanAsync();
        }

        [HttpPut("CheckMissingFeeds")]
        public void CheckMissingFeeds()
        {
            marketApp.NewsManager.CheckMissingFeeds();
        }

        [HttpPut("Expunge")]
        public void Expunge()
        {
            marketApp.NewsManager.ExpungeAllNewsItemsForEverySource();
        }

        [HttpGet("Details")]
        public ActionResult<NewsDetails> GetNewsManagerDetails()
        {
            var allNewsItems = marketApp.NewsManager.Sources.SelectMany(x => x.NewsItems).ToList();
            NewsDetails details = new NewsDetails()
            {
                SourceIds = marketApp.NewsManager.Sources.OrderBy(x => x.FeedTitle).Select(x => x.Id.ToString()).ToList(),
                TotalNumberOfNewsItems = allNewsItems.Count,
                Earliest = allNewsItems.Count > 0 ? allNewsItems.Min(x => x.PublishDate) : DateTimeOffset.MinValue,
                Latest = allNewsItems.Count > 0 ? allNewsItems.Max(x => x.PublishDate) : DateTimeOffset.MaxValue,
                NumberOfRssFeeds = marketApp.NewsManager.Sources.FindAll(x => x.FeedType == FeedType.RssFeed).Count,
                NumberOfTwitterFeeds = marketApp.NewsManager.Sources.FindAll(x => x.FeedType == FeedType.TwitterFeed).Count
            };
            return details;
        }

        [HttpGet("{id}/Details")]
        public ActionResult<NewsSourceDetails> GetNewsSourceDetails(string id)
        {
            var source = marketApp.NewsManager.GetSource(Guid.Parse(id));
            var sourceMonitor = source?.SourceMonitor;
            NewsSourceDetails details = new NewsSourceDetails()
            {
                SourceId = id,
                Title = source?.FeedTitle,
                Url = source?.FeedUrl,
                FeedType = source?.FeedType,
                Timezone = source?.Timezone,
                NewsItems_Count = source?.NewsItems?.Count,
                NewsItems_LastPublished = source?.LastPublished,
                IsPolling = sourceMonitor?.IsPolling,
                LastPoll = sourceMonitor?.LastPoll,
                PollingTimespan = sourceMonitor?.PollingTimespan
            };
            return details;
        }

        [HttpGet("{id}/NewsItems")]
        public ActionResult<List<NewsItem>?> GetNewsItems(string id)
        {
            var source = marketApp.NewsManager.GetSource(Guid.Parse(id));
            List<NewsItem> newsItems = new List<NewsItem>();
            foreach (var newsItem in source?.NewsItems)
            {
                newsItems.Add(new NewsItem()
                {
                    Id = newsItem.Id,
                    RefId = newsItem.RefId,
                    PublishDate = newsItem.PublishDate,
                    Timestamp = newsItem.Timestamp,
                    Text = newsItem.Text,
                    SourceId = newsItem.SourceId,
                });
            }
            return newsItems;
        }

        [HttpPut("{id}/SourceMonitor")]
        public ActionResult<NewsSourceDetails>? UpdateSourceMonitorSettings(string id, [FromBody] NewsSourceDetails details)
        {
            try
            {
                var source = marketApp.NewsManager.GetSource(Guid.Parse(id));
                var sourceMonitor = source?.SourceMonitor;

                if (sourceMonitor != null)
                {
                    if (details.IsPolling != null)
                        sourceMonitor.IsPolling = (bool)details.IsPolling;

                    if (details.PollingTimespan != null)
                        sourceMonitor.PollingTimespan = sourceMonitor.PollingTimespan.Add((TimeSpan)details.PollingTimespan);
                }

                NewsSourceDetails newDetails = new NewsSourceDetails()
                {
                    SourceId = id,
                    Title = source?.FeedTitle,
                    Url = source?.FeedUrl,
                    FeedType = source?.FeedType,
                    Timezone = source?.Timezone,
                    NewsItems_Count = source?.NewsItems?.Count,
                    NewsItems_LastPublished = source?.LastPublished,
                    IsPolling = sourceMonitor?.IsPolling,
                    LastPoll = sourceMonitor?.LastPoll,
                    PollingTimespan = sourceMonitor?.PollingTimespan
                };
                return newDetails;
            }
            catch (Exception)
            {
                return null;
            }
        }

        [HttpPut("{id}/Feed")]
        public ActionResult<NewsSourceDetails>? UpdateFeedSettings(string id, [FromBody] UpdateFeedDetails details)
        {
            try
            {
                var source = marketApp.NewsManager.GetSource(Guid.Parse(id));
                var sourceMonitor = source?.SourceMonitor;

                if (sourceMonitor != null)
                {
                    if (details.Timezone != null)
                    {
                        source.Timezone = details.Timezone;
                        marketApp.NewsManager.SaveChanges();
                    }
                }

                NewsSourceDetails newDetails = new NewsSourceDetails()
                {
                    SourceId = id,
                    Title = source?.FeedTitle,
                    Url = source?.FeedUrl,
                    FeedType = source?.FeedType,
                    Timezone = source?.Timezone,
                    NewsItems_Count = source?.NewsItems?.Count,
                    NewsItems_LastPublished = source?.LastPublished,
                    IsPolling = sourceMonitor?.IsPolling,
                    LastPoll = sourceMonitor?.LastPoll,
                    PollingTimespan = sourceMonitor?.PollingTimespan
                };
                return newDetails;
            }
            catch (Exception)
            {
                return null;
            }
        }

        [HttpGet("{id}/ForceUpdate")]
        public void ForceUpdate(string id)
        {
            var source = marketApp.NewsManager.GetSource(Guid.Parse(id));
            var sourceMonitor = source?.SourceMonitor;
            if (sourceMonitor != null)
                _ = sourceMonitor.ForceUpdate();
        }

        [HttpGet("ForceUpdateAll")]
        public void ForceUpdateAll()
        {
            foreach (var source in marketApp.NewsManager.Sources)
            {
                var sourceMonitor = source.SourceMonitor;
                if (sourceMonitor != null)
                    _ = sourceMonitor.ForceUpdate();
            }
        }

        [HttpPost()]
        public ActionResult<List<string>> AddSource([FromBody] AddSourceDetails sourceDetails)
        {
            marketApp.NewsManager.AddFeed(sourceDetails.FeedType, sourceDetails.Title, sourceDetails.Url, sourceDetails.Timezone);

            // Return a list of NewsSource ids
            return marketApp.NewsManager.Sources.OrderBy(x => x.FeedTitle).Select(x => x.Id.ToString()).ToList();
        }

        [HttpDelete("{id}")]
        public ActionResult<List<string>> RemoveSource(string id)
        {
            marketApp.NewsManager.Remove(Guid.Parse(id));

            // Return a list of NewsSource ids
            return marketApp.NewsManager.Sources.OrderBy(x => x.FeedTitle).Select(x => x.Id.ToString()).ToList();
        }

    }
}

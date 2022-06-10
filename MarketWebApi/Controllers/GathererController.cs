using Elements;
using Information;
using Microsoft.AspNetCore.Mvc;
using News;
using TextAnalysis;

namespace MarketWebApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class GathererController : Controller
    {
        const int MAX_INTERESTINGITEMS = 100;

        public class SourceItem
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public FeedType FeedType { get; set; }
        }

        public class GathererDetails
        {
            public int TotalNonInterestingItemsCount { get; set; }
            public int TotalInterestingItemsCount { get; set; }
            public List<SourceItem> SourceItems { get; set; }
        }

        public class InterestingItemsDetails
        {
            public class InterestingItem
            {
                public Guid Id { get; set; }
                public string Text { get; set; }
                public DateTimeOffset PublishDate { get; set; }
                public List<AnalysisFinding> Findings { get; set; }
                public bool HasTimeSeries { get; set; }

                public InterestingItem(Information.InterestingItem iterestingItem)
                {
                    Id = iterestingItem.Id;
                    Text = iterestingItem.Text;
                    PublishDate = iterestingItem.PublishDate;
                    Findings = iterestingItem.Findings;
                    HasTimeSeries = iterestingItem.StockTimeSeriesExists;
                }
            }

            public int InterestingItemsCount { get; private set; }
            public List<InterestingItem> InterestingItems { get; private set; }

            public InterestingItemsDetails(List<Information.InterestingItem> interestingItems, Filter filter)
            {
                interestingItems = interestingItems.OrderByDescending(x => x.PublishDate).ToList();

                if (filter.HasTimeSeries == true)
                    interestingItems = interestingItems.FindAll(x => x.StockTimeSeriesExists);

                this.InterestingItemsCount = interestingItems.Count;
                this.InterestingItems = new List<InterestingItem>();

                // Only get a certain number of items (MAX_INTERESTINGITEMS)
                foreach (var interestingItem in interestingItems.Take(MAX_INTERESTINGITEMS))
                {
                    this.InterestingItems.Add(new InterestingItem(interestingItem));
                }
            }
        }

        public class Filter
        {
            public bool? HasTimeSeries { get; set; }
            public DateTime? DateFrom { get; set; }
            public DateTime? DateTo { get; set; }
        }

        public class InterestingItemDetails
        {
            public string Id { get; set; }
            public string Text { get; set; }
            public DateTimeOffset PublishDate { get; set; }
            public DateTime Timestamp { get; set; }
            public List<AnalysisFinding> Findings { get; set; }
            public List<TimeSeries> TimeSerieses { get; set; }
            public DateTime LastPoll { get; set; }
            public FeedType FeedType { get; set; }
            public string Title { get; set; }
            public string Url { get; set; }

        }

        private readonly Market.App marketApp;

        public GathererController(Market.App marketApp)
        {
            this.marketApp = marketApp;
        }

        [HttpPut("Clean")]
        public void Clean()
        {
            marketApp.GathererInformation.CleanAsync();
        }

        [HttpGet("Details")]
        public ActionResult<GathererDetails> GetDetails()
        {
            List<SourceItem> sourceItems = new List<SourceItem>();
            foreach (var item in marketApp.NewsManager.Sources.OrderBy(x => x.FeedTitle))
            {
                sourceItems.Add(new SourceItem()
                {
                    Id = item.Id,
                    Name = item.FeedTitle,
                    FeedType = item.FeedType
                });
            }
            GathererDetails details = new GathererDetails()
            {
                TotalNonInterestingItemsCount = marketApp.GathererInformation.NonInterestingItems.Count,
                TotalInterestingItemsCount = marketApp.GathererInformation.InterestingItems.Count,
                SourceItems = sourceItems
            };
            return details;
        }

        [HttpPost("InterestingItems")]
        public ActionResult<InterestingItemsDetails> GetInterestingItems([FromBody] Filter filter)
        {
            InterestingItemsDetails details = new InterestingItemsDetails(marketApp.GathererInformation.InterestingItems, filter);
            return details;
        }

        [HttpGet("InterestingItems/{id}/Details")]
        public ActionResult<InterestingItemDetails> GetInterestingItemDetails(string id)
        {
            Guid guid = Guid.Parse(id);
            var interestingItem = marketApp.GathererInformation.InterestingItems.Find(x => x.Id == guid);
            var source = marketApp.NewsManager.GetSource(interestingItem.SourceId);
            InterestingItemDetails details = new InterestingItemDetails()
            {
                Id = id,
                Text = interestingItem.Text,
                PublishDate = interestingItem.PublishDate.ToLocalTime(),
                Timestamp = interestingItem.Timestamp,
                Findings = interestingItem.Findings,
                TimeSerieses = interestingItem.TimeSerieses,
                LastPoll = source != null ? source.SourceMonitor.LastPoll : DateTime.MinValue,
                FeedType = source != null ? source.FeedType : FeedType.TwitterFeed,
                Title = source != null ? source.FeedTitle : "",
                Url = source != null ? source.FeedUrl : ""
            };
            return details;
        }

        [HttpGet("InterestingItems/{id}/Breakdown")]
        public ActionResult<AnalysisBreakDown> GetInterestingItemBreakdown(string id)
        {
            return marketApp.GathererInformation.GetBreakdown(Guid.Parse(id));
        }

        [HttpPost("InterestingItems/CompanyName/{startsWith}")]
        public async Task<InterestingItemsDetails> GetInterestingItemsByName(string startsWith, [FromBody] Filter filter)
        {
            List<InterestingItem> allInterestingItems = marketApp.GathererInformation.InterestingItems.OrderByDescending(x => x.PublishDate).ToList();
            List<InterestingItem> interestingItems = new List<InterestingItem>();

            foreach (var interestingItem in allInterestingItems)
            {
                foreach (var finding in interestingItem.Findings)
                {
                    if (finding.Company.Name.StartsWith(startsWith, true, null))
                    {
                        interestingItems.Add(interestingItem);
                    }
                    else
                    {
                        var company = await marketApp.CompanyDataStore.GetCompanyBySymbol(finding.Company.Symbol);
                        if (company != null && company.Aliases.Any(x => x.StartsWith(startsWith, true, null)))
                        {
                            interestingItems.Add(interestingItem);
                        }
                    }

                }
            }

            InterestingItemsDetails details = new InterestingItemsDetails(interestingItems, filter);
            return details;
        }

        [HttpPost("InterestingItems/CompanySymbol/{startsWith}")]
        public ActionResult<InterestingItemsDetails> GetInterestingItemsBySymbol(string startsWith, [FromBody] Filter filter)
        {
            List<InterestingItem> allInterestingItems = marketApp.GathererInformation.InterestingItems.OrderByDescending(x => x.PublishDate).ToList();
            List<InterestingItem> interestingItems = new List<InterestingItem>();

            foreach (var interestingItem in allInterestingItems)
            {
                foreach (var finding in interestingItem.Findings)
                {
                    if (finding.Company.Symbol.StartsWith(startsWith))
                    {
                        interestingItems.Add(interestingItem);
                    }

                }
            }

            InterestingItemsDetails details = new InterestingItemsDetails(interestingItems, filter);
            return details;
        }

        [HttpPost("InterestingItems/Source/{sourceId}")]
        public ActionResult<InterestingItemsDetails> GetInterestingItemsBySource(string sourceId, [FromBody] Filter filter)
        {
            List<InterestingItem> allInterestingItems = marketApp.GathererInformation.InterestingItems.OrderByDescending(x => x.PublishDate).ToList();
            Guid guid = Guid.Parse(sourceId);
            List<InterestingItem> interestingItems = allInterestingItems.FindAll(x => x.SourceId == guid);
            InterestingItemsDetails details = new InterestingItemsDetails(interestingItems, filter);
            return details;
        }

        [HttpDelete("InterestingItems/{id}/MarkNotInteresting")]
        public void MarkNotInteresting(string id)
        {
            marketApp.Gatherer.MarkNotInteresting(Guid.Parse(id));
        }

    }
}

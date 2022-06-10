using System.Text.Json.Serialization;
using TextAnalysis;

namespace Information
{
    public class NonInterestingItem : GathererInformationItem
    {

        [JsonConstructorAttribute]
        public NonInterestingItem(Guid id, Guid sourceId, DateTime timestamp)
            : base(id, sourceId, timestamp)
        {
        }

        public NonInterestingItem(AnalysisInfo info)
            : base(info.NewsItem.Id, info.NewsItem.SourceId, DateTime.Now)
        {
        }
    }
}

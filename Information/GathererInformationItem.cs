using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Information
{
    public abstract class GathererInformationItem
    {
        [Key]
        public Guid NewsItemId { get; set; }
        public Guid SourceId
        {
            get;
            set;
        }
        public DateTime Timestamp { get; set; }
        public string Text { get; set; }
        public DateTimeOffset PublishDate { get; set; }

        [JsonConstructorAttribute]
        public GathererInformationItem(Guid newsItemId, Guid sourceId, DateTime timestamp, string text, DateTimeOffset publishDate)
        {
            NewsItemId = newsItemId;
            SourceId = sourceId;
            Timestamp = timestamp;
            Text = text;
            PublishDate = publishDate;
        }
    }

}

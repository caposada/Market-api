using System.Text.Json.Serialization;

namespace Information
{
    public abstract class GathererInformationItem
    {
        public Guid Id { get; set; }
        public Guid SourceId
        {
            get;
            set;
        }
        public DateTime Timestamp { get; set; }

        [JsonConstructorAttribute]
        public GathererInformationItem(Guid id, Guid sourceId, DateTime timestamp)
        {
            this.Id = id;
            this.SourceId = sourceId;
            this.Timestamp = timestamp;
        }
    }

}

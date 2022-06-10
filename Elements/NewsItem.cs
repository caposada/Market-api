using System.Text.Json.Serialization;

namespace Elements
{

    public class NewsItem
    {
        public Guid Id { get; set; }
        public string? RefId { get; set; }
        public DateTimeOffset PublishDate { get; set; }
        public DateTime Timestamp { get; set; }
        public string Text { get; set; }
        public Guid SourceId { get; set; }

        [JsonConstructorAttribute]
        public NewsItem(Guid id, string refId, DateTimeOffset publishDate, DateTime timestamp, string text, Guid sourceId)
        {
            this.Id = id;
            this.RefId = refId;
            this.PublishDate = publishDate;
            this.Timestamp = timestamp;
            this.Text = text;
            this.SourceId = sourceId;
        }

        public NewsItem(string? refId, DateTimeOffset publishDate, string text, NewsFeed feed)
        {
            this.Id = Guid.NewGuid();
            this.RefId = refId;
            this.PublishDate = publishDate;
            this.Timestamp = DateTime.Now;
            this.Text = text;
            this.SourceId = feed.Id;
        }

        public NewsItem(string refId, DateTimeOffset publishDate, List<string> textList, NewsFeed feed)
        {
            this.Id = Guid.NewGuid();
            this.RefId = refId;
            this.Timestamp = DateTime.Now;
            this.PublishDate = publishDate;
            this.Text = ExtractTextFromTextList(textList);
            this.SourceId = feed.Id;
        }

        private string ExtractTextFromTextList(List<string> textList)
        {
            string text = "";
            foreach (string textItem in textList)
            {
                text += textItem;
            }
            return text;
        }


    }


}

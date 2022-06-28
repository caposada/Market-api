using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using static TextAnalysis.AnalysisBreakDown;

namespace Information
{
    public class AnalysisBreakDown
    {
        [Key]
        public Guid Id { get; set; }
        public Guid NewsItemId { get; set; }
        public string SpanJson { get; set; }

        [NotMapped]
        public List<Token> AllTokens
        {
            get
            {
                var spans = JsonSerializer.Deserialize<List<Span>>(SpanJson);
                return spans.SelectMany(span => span).ToList<Token>();
            }
        }

        public AnalysisBreakDown(
            Guid id,
            Guid newsItemId,
            string spanJson)
        {
            this.Id = id;
            this.NewsItemId = newsItemId;
            this.SpanJson = spanJson;
        }

        public AnalysisBreakDown(
            Guid newsItemId,
            List<Span> spans)
        {
            this.Id = Guid.NewGuid();
            this.NewsItemId = newsItemId;
            this.SpanJson = JsonSerializer.Serialize<List<Span>>(spans);
        }
    }
}

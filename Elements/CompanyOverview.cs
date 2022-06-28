using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Elements
{
    public class CompanyOverview
    {
        [Key]
        public Guid Id { get; set; }
        [ForeignKey("Symbol")]
        public string Symbol { get; set; }
        public DateTime LastUpdated { get; set; }
        public string JsonData { get; set; }

        [NotMapped]
        public Dictionary<string, string> OverviewDictionary
        {
            get
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(JsonData);
            }
        }

        public CompanyOverview(string symbol, string jsonData)
        {
            this.Id = Guid.NewGuid();
            this.Symbol = symbol;
            this.JsonData = jsonData;
        }

        public bool IsValid()
        {
            return this.OverviewDictionary != null && this.OverviewDictionary.Count > 3;
        }
    }
}

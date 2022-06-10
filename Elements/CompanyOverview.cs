using System.Text.Json.Serialization;

namespace Elements
{
    public class CompanyOverview
    {
        public string Symbol { get; set; }
        public DateTime LastUpdated { get; set; }
        public Dictionary<string, string> OverviewDictionary { get; set; }

        [JsonConstructorAttribute]
        public CompanyOverview()
        {
            this.OverviewDictionary = new Dictionary<string, string>();
        }

        public CompanyOverview(SimpleCompany company, Dictionary<string, string> dataDictionary)
        {
            this.Symbol = company.Symbol;
            this.OverviewDictionary = dataDictionary;
        }

        public bool IsValid()
        {
            return this.OverviewDictionary != null && this.OverviewDictionary.Count > 3;
        }
    }
}

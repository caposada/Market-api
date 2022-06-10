using System.Text.Json.Serialization;

namespace Elements
{
    public class CompanyAlias
    {
        public string Symbol { get; set; }
        public List<string> Names { get; set; }

        public CompanyAlias()
        {
            this.Names = new List<string>();
        }

        public CompanyAlias(string symbol, string name)
        {
            this.Symbol = symbol;
            this.Names = new List<string>();
            this.Names.Add(name);
        }

        [JsonConstructorAttribute]
        public CompanyAlias(string symbol, List<string> names)
        {
            this.Symbol = symbol;
            this.Names = names;
        }
    }
}

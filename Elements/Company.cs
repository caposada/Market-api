using System.Text.Json.Serialization;

namespace Elements
{
    public class BasicCompany
    {
        public string? Symbol { get; set; }
        public string? Name { get; set; }
        public Exchanges Exchange { get; set; }

        [JsonConstructorAttribute]
        public BasicCompany(
               string symbol,
               string name,
               Exchanges exchange)
        {
            this.Symbol = symbol;
            this.Name = name;
            this.Exchange = exchange;
        }

        public BasicCompany()
        {
        }

    }

    public class SimpleCompany : BasicCompany
    {

        public AssetTypes AssetType { get; set; }
        public DateTime IpoDate { get; set; }
        public string Status { get; set; }
        public string LongName { get; set; }
        [JsonIgnore]
        public List<string> Aliases { get; set; }
        [JsonIgnore]
        public bool HasOverview { get; set; }

        [JsonConstructorAttribute]
        public SimpleCompany(
            string symbol,
            string longName,
            string name,
            Exchanges exchange,
            AssetTypes assetType,
            DateTime ipoDate,
            string status,
            List<string> aliases)
        {
            this.Symbol = symbol;
            this.LongName = longName;
            this.Name = name;
            this.Exchange = exchange;
            this.AssetType = assetType;
            this.IpoDate = ipoDate;
            this.Status = status;
            this.Aliases = new List<string>();
            this.HasOverview = false;
        }

        public SimpleCompany(
            string symbol,
            string name,
            string exchange,
            string assetType,
            string ipoDate,
            string status)
        {
            exchange = exchange.Replace(' ', '_');

            this.Symbol = symbol;
            this.LongName = Utils.Clean(name);
            this.Name = Utils.Reduce(this.LongName);
            this.Exchange = (Exchanges)Enum.Parse<Exchanges>(exchange);
            this.AssetType = (AssetTypes)Enum.Parse<AssetTypes>(assetType.ToUpper());
            this.IpoDate = DateTime.Parse(ipoDate);
            this.Status = status;
            this.Aliases = new List<string>();
            this.HasOverview = false;
        }

        public override string ToString()
        {
            return $"{Name} ({Symbol}:{Exchange})";
        }

    }

    public class DetailedCompany : SimpleCompany
    {
        public DetailedCompany(
            string symbol,
            string name,
            string exchange,
            string assetType,
            string ipoDate,
            string status)
            : base(symbol, name, exchange, assetType, ipoDate, status)
        {
        }


    }


}

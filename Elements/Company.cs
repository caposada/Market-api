using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Elements
{
    public class BasicCompany
    {
        [Key]
        public Guid Id { get; set; }
        [ForeignKey("Symbol")]
        public string Symbol { get; set; }
        public string Name { get; set; }
        public Exchanges Exchange { get; set; }
    }

    public class SimpleCompany : BasicCompany
    {

        public AssetTypes AssetType { get; set; }
        public DateTime IpoDate { get; set; }
        public string Status { get; set; }
        public string LongName { get; set; }
        [JsonIgnore]
        [NotMapped]
        public List<string> Aliases { get; set; }
        [JsonIgnore]
        [NotMapped]
        public bool HasOverview { get; set; }

        public SimpleCompany(
            string symbol,
            string name,
            Exchanges exchange,
            AssetTypes assetType,
            DateTime ipoDate,
            string status)
        {

            this.Id = Guid.NewGuid();
            this.Symbol = symbol;
            this.LongName = Utils.Clean(name);
            this.Name = Utils.Reduce(this.LongName);
            this.Exchange = exchange;
            this.AssetType = assetType;
            this.IpoDate = ipoDate;
            this.Status = status;
            this.Aliases = new List<string>();
            this.HasOverview = false;
        }

        public override string ToString()
        {
            return $"{Name} ({Symbol}:{Exchange})";
        }

    }

    //public class DetailedCompany : SimpleCompany
    //{
    //    public DetailedCompany(
    //        string symbol,
    //        string name,
    //        string exchange,
    //        AssetTypes assetType,
    //        string ipoDate,
    //        string status)
    //        : base(symbol, name, exchange, assetType, ipoDate, status)
    //    {
    //    }


    //}


}

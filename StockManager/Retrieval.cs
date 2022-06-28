using AlphaVantage.Net.Common.Intervals;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace StockManager
{
    public class Retrieval<Result>
    {
        [Key]
        public Guid RecordId { get; set; }
        public string Symbol { get; set; }
        public Interval? Interval { get; set; }
        public DateTime ValidUntil { get; set; }
        public string ResultingJson { get; set; }

        [NotMapped]
        public Result? Resulting
        {
            get
            {
                return JsonSerializer.Deserialize<Result>(ResultingJson);
            }
        }

        public Retrieval(Guid recordId, string symbol, Interval? interval, DateTime validUntil, string resultingJson)
        {
            this.RecordId = recordId;
            this.Symbol = symbol;
            this.Interval = interval;
            this.ValidUntil = validUntil;
            this.ResultingJson = resultingJson;
        }

        public Retrieval(string symbol, DateTime validUntil, Result resulting, Guid? recordId = null)
        {
            this.RecordId = recordId ?? Guid.NewGuid();
            this.Symbol = symbol;
            this.ValidUntil = validUntil;

            this.ResultingJson = JsonSerializer.Serialize<Result>(resulting);
        }

        public Retrieval(string symbol, Interval interval, DateTime validUntil, Result resulting, Guid? recordId = null)
        {
            this.RecordId = recordId ?? Guid.NewGuid();
            this.Symbol = symbol;
            this.Interval = interval;
            this.ValidUntil = validUntil;

            this.ResultingJson = JsonSerializer.Serialize<Result>(resulting);
        }

        public Result? GetResult()
        {
            return JsonSerializer.Deserialize<Result>(ResultingJson);
        }

    }
}

using System.Text.Json.Serialization;

namespace StockManager
{
    public class Request
    {
        public Guid Id { get; set; }
        public DateTime TimeStamp { get; private set; }

        public Request(Guid id)
        {
            this.Id = id;
            this.TimeStamp = DateTime.Now;
        }

        [JsonConstructor]
        public Request()
        {
            this.Id = Guid.NewGuid();
            this.TimeStamp = DateTime.Now;
        }

    }
}

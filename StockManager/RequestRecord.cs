using System.ComponentModel.DataAnnotations;

namespace StockManager
{
    public class RequestRecord
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public int Count { get; set; }

    }
}

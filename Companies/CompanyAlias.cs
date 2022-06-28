using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Elements
{
    public class CompanyAlias
    {
        [Key]
        public Guid Id { get; set; }
        [ForeignKey("Symbol")]
        public string Symbol { get; set; }
        public string Name { get; set; }

        public CompanyAlias(string symbol, string name)
        {
            this.Id = Guid.NewGuid();
            this.Symbol = symbol;
            this.Name = name;
        }
    }
}

using AlphaVantage.Net.Stocks;
using Microsoft.EntityFrameworkCore;

namespace StockManager
{
    internal class StockMarketContext : DbContext
    {
        public DbSet<RequestRecord> RequestRecords { get; set; }
        public DbSet<Retrieval<GlobalQuote>> PreviousQuoteRetrievals { get; set; }
        public DbSet<Retrieval<StockTimeSeries>> PreviousTimeSerieseRetrievals { get; set; }

        public StockMarketContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=.\SQLEXPRESS;Database=StockMarketDB;Integrated Security=true;");
        }
    }
}

using Elements;
using Microsoft.EntityFrameworkCore;

namespace News
{
    public class NewsContext : DbContext
    {
        public DbSet<Source> Sources { get; set; }
        public DbSet<NewsItem> NewsItems { get; set; }

        public NewsContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=.\SQLEXPRESS;Database=NewsDB;Integrated Security=true;");
        }
    }
}

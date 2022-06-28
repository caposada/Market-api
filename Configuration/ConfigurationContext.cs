using Microsoft.EntityFrameworkCore;

namespace Configuration
{
    public class ConfigurationContext : DbContext
    {
        public DbSet<Setting> Settings { get; set; }
        public DbSet<StandardSource> StandardSources { get; set; }

        public ConfigurationContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=.\SQLEXPRESS;Database=ConfigurationDB;Integrated Security=true;");
        }
    }
}
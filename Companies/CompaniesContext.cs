using Elements;
using Microsoft.EntityFrameworkCore;

namespace Companies
{
    public class CompaniesContext : DbContext
    {
        public DbSet<SimpleCompany> Companies { get; set; }
        public DbSet<CompanyAlias> Aliases { get; set; }
        public DbSet<CompanyOverview> Overviews { get; set; }

        public CompaniesContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=.\SQLEXPRESS;Database=CompaniesDB;Integrated Security=true;");
        }
    }
}

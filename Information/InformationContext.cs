using Elements;
using Microsoft.EntityFrameworkCore;

namespace Information
{
    public class InformationContext : DbContext
    {
        public DbSet<InterestingItem> InterestingItems { get; set; }
        public DbSet<NonInterestingItem> NonInterestingItems { get; set; }
        public DbSet<AnalysisBreakDown> AnalysisBreakDowns { get; set; }
        public DbSet<AnalysisFinding> AnalysisFindings { get; set; }
        public DbSet<TimeSeries> TimeSerieses { get; set; }

        public InformationContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=.\SQLEXPRESS;Database=InformationDB;Integrated Security=true;");
        }
    }

}

using Elements;

namespace Companies
{
    public class CompaniesAliasList
    {

        public CompaniesAliasList()
        {
        }

        public void SetAlias(string symbol, List<string> aliases)
        {
            using (CompaniesContext context = new CompaniesContext())
            {
                // Remove deleted entries
                var old = context.Aliases.Where(x => x.Symbol == symbol && !aliases.Contains(x.Name));
                context.Aliases.RemoveRange(old);

                // Add any new
                foreach (string newAlias in aliases)
                {
                    if (!context.Aliases.Any(x => x.Symbol == symbol && x.Name == newAlias))
                    {
                        context.Add(new CompanyAlias(symbol, newAlias));
                    }
                }

                context.SaveChanges();
            }
        }

        public List<CompanyAlias> GetAliases()
        {
            using (CompaniesContext context = new CompaniesContext())
            {
                return context.Aliases.ToList();
            }
        }

        public List<string> GetNames(string symbol)
        {
            using (CompaniesContext context = new CompaniesContext())
            {
                return context.Aliases.Where(x => x.Symbol == symbol).Select(x => x.Name).ToList();
            }
        }

        public List<string> GetAllAliasNames()
        {
            using (CompaniesContext context = new CompaniesContext())
            {
                return context.Aliases.Select(x => x.Name).ToList();
            }
        }

    }
}

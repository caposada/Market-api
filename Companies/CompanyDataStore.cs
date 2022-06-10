using Elements;
using StockManager;
using System.Text.RegularExpressions;

namespace Companies
{
    public delegate void CompanyDataStoreNotify(string symbol);                  // delegate

    public class CompanyDataStore
    {
        public event CompanyDataStoreNotify? CompanyChanged;                      // event

        private static CompaniesList? companiesList = null;
        private static CompaniesAliasList? companiesAliasList = null;
        private static CompanyOverviewList? companyOverviewList = null;
        private static bool loaded = false;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private MarketData marketData;

        public CompanyDataStore(MarketData marketData)
        {
            this.marketData = marketData;
        }

        public async Task<SimpleCompany?> SetAliases(string symbol, List<string> aliases)
        {
            await Load();
            SimpleCompany? company = companiesList?.Companies.Find(x => x.Symbol == symbol);
            if (company != null)
            {
                var alias = companiesAliasList?.SetAlias(symbol, aliases);
                if (alias != null)
                {
                    company.Aliases = alias.Names;
                    CompanyChanged?.Invoke(symbol);
                }
            }
            return company;
        }

        public async Task<CompanyOverview?> GetOverview(string symbol)
        {
            await Load();
            var company = companiesList?.GetCompanyBySymbol(symbol);
            if (company != null)
            {
                if (companyOverviewList != null)
                {
                    CompanyOverview? overview = await companyOverviewList.GetOverview(company);
                    company.HasOverview = overview != null;
                    return overview;
                }
            }
            return null;
        }

        public async Task<List<SimpleCompany>?> GetCompanies()
        {
            await Load();
            return companiesList?.GetCompanies();
        }

        public async Task<List<string>?> GetFlatList()
        {
            await Load();
            return companiesList?.GetFlatList();
        }

        public async Task<SimpleCompany?> GetCompany(string text)
        {
            await Load();
            return companiesList?.GetCompany(text);
        }

        public async Task<List<SimpleCompany>?> GetCompaniesFromFragment(string text)
        {
            await Load();
            return companiesList?.GetCompaniesFromFragment(text);
        }

        public async Task<SimpleCompany?> GetCompanyBySymbol(string symbol)
        {
            await Load();
            var company = companiesList?.GetCompanyBySymbol(symbol);
            if (company != null)
            {
                if (companyOverviewList != null)
                    company.HasOverview = companyOverviewList.HasOverview(symbol);
            }
            return company;
        }

        public async Task<SimpleCompany?> GetCompanyByName(string text)
        {
            await Load();
            return companiesList?.GetCompanyByName(text);
        }

        public async Task<List<SimpleCompany>?> GetCompanies(Regex pattern)
        {
            await Load();
            return companiesList?.GetCompanies(pattern);
        }

        private async Task Load()
        {
            try
            {
                await _semaphore.WaitAsync();
                if (!loaded)
                    await LoadAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task LoadAsync()
        {
            companiesList = new CompaniesList(marketData);
            await companiesList.LoadAsync();

            // Associate the list of 'Company Aliases' with each company
            companiesAliasList = new CompaniesAliasList();
            foreach (var alias in companiesAliasList.Aliases)
            {
                var company = companiesList?.Companies?.Find(x => x.Symbol == alias.Symbol);
                if (company != null)
                {
                    company.Aliases = alias.Names;
                }
            }

            // Associate the list of 'Company Overviews' with each company
            companyOverviewList = new CompanyOverviewList(marketData);
            companyOverviewList.OverviewChanged += CompanyOverviewList_OverviewChanged;
            foreach (var overview in companyOverviewList.Overviews)
            {
                var company = companiesList.Companies?.Find(x => x.Symbol == overview.Symbol);
                if (company != null && overview.IsValid())
                {
                    company.HasOverview = true;
                }
            }

            loaded = true;
        }

        private void CompanyOverviewList_OverviewChanged(string symbol)
        {
            SimpleCompany? company = companiesList?.Companies?.Find(x => x.Symbol == symbol);
            if (company != null)
            {
                CompanyChanged?.Invoke(symbol);
            }
        }
    }
}

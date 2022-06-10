using Elements;
using Microsoft.AspNetCore.Mvc;

namespace MarketWebApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : Controller
    {
        public class SymbolAndName
        {
            public string Symbol { get; set; }
            public string Name { get; set; }
        }

        public class UpdateDetails
        {
            public List<string> Aliases { get; set; }
        }

        public class CompaniesDetails
        {
            public int NumberOfCompanies { get; set; }
        }

        public class CompanyDetails
        {
            public string Symbol { get; set; }
            public string Name { get; set; }
            public Exchanges Exchange { get; set; }
            public AssetTypes AssetType { get; set; }
            public DateTime IpoDate { get; set; }
            public string Status { get; set; }
            public string LongName { get; set; }
            public List<string> Aliases { get; set; }
            public bool HasOverview { get; set; }

            public CompanyDetails(SimpleCompany company)
            {
                this.Symbol = company.Symbol;
                this.Name = company.Name;
                this.Exchange = company.Exchange;
                this.AssetType = company.AssetType;
                this.IpoDate = company.IpoDate;
                this.Status = company.Status;
                this.LongName = company.LongName;
                this.Aliases = company.Aliases;
                this.HasOverview = company.HasOverview;
            }
        }

        private readonly Market.App marketApp;

        public CompanyController(Market.App marketApp)
        {
            this.marketApp = marketApp;
        }

        [HttpGet("Details")]
        public async Task<CompaniesDetails> GetDetails()
        {
            var allCompanies = await marketApp.CompanyDataStore.GetCompanies();
            CompaniesDetails details = new CompaniesDetails()
            {
                NumberOfCompanies = allCompanies.Count
            };
            return details;
        }

        [HttpGet("{symbol}/Details")]
        public async Task<CompanyDetails> GetCompanyDetails(string symbol)
        {
            return new CompanyDetails(await marketApp.CompanyDataStore.GetCompanyBySymbol(symbol));
        }

        [HttpGet("{symbol}/Overview")]
        public async Task<CompanyOverview?> GetCompanyOverview(string symbol)
        {
            return await marketApp.CompanyDataStore.GetOverview(symbol);
        }

        [HttpGet("Name/{startsWith}")]
        public async Task<List<CompanyDetails>> GetCompaniesByName(string startsWith)
        {
            var allCompanies = await marketApp.CompanyDataStore.GetCompanies();
            var companiesBeginingWith = allCompanies.FindAll(x => x.Name.StartsWith(startsWith, true, null));
            List<CompanyDetails> companies = new List<CompanyDetails>();
            foreach (var company in companiesBeginingWith)
            {
                companies.Add(new CompanyDetails(company));
            }
            return companies;
        }

        [HttpGet("Symbol/{startsWith}")]
        public async Task<List<CompanyDetails>> GetCompaniesBySymbol(string startsWith)
        {
            var allCompanies = await marketApp.CompanyDataStore.GetCompanies();
            var companiesBeginingWith = allCompanies.FindAll(x => x.Symbol.StartsWith(startsWith, true, null));
            List<CompanyDetails> companies = new List<CompanyDetails>();
            foreach (var company in companiesBeginingWith)
            {
                companies.Add(new CompanyDetails(company));
            }
            return companies;
        }

        [HttpPut("{symbol}")]
        public async Task<SimpleCompany?> UpdateCompany(string symbol, [FromBody] UpdateDetails details)
        {
            if (details.Aliases != null)
                return await marketApp.CompanyDataStore.SetAliases(symbol, details.Aliases);
            return null;
        }






    }
}

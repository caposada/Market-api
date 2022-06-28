using CsvHelper;
using Elements;
using StockManager;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Companies
{
    public class CompaniesList
    {
        internal class Csv
        {
            public string symbol { get; set; }
            public string name { get; set; }
            public string exchange { get; set; }
            public string assetType { get; set; }
            public string ipoDate { get; set; }
            public string delistingDate { get; set; }
            public string status { get; set; }

            public Csv(
                string symbol,
                string name,
                string exchange,
                string assetType,
                string ipoDate,
                string delistingDate,
                string status)
            {
                this.symbol = symbol;
                this.name = name;
                this.exchange = exchange;
                this.assetType = assetType;
                this.ipoDate = ipoDate;
                this.delistingDate = delistingDate;
                this.status = status;
            }
        }

        private const int MIN_COMPANY_NAME_SIZE = 4;

        public List<SimpleCompany> Companies
        {
            get
            {
                using (CompaniesContext context = new CompaniesContext())
                {
                    return context.Companies.ToList();
                }
            }
        }
        public bool Loaded
        {
            get
            {
                using (CompaniesContext context = new CompaniesContext())
                {
                    return context.Companies != null && context.Companies.ToList().Count > 0;
                }
            }
        }

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private MarketData marketData;

        public CompaniesList(MarketData marketData)
        {
            this.marketData = marketData;
        }

        public async Task LoadAsync()
        {
            ColourConsole.WriteInfo($"CompaniesList - Loading CompanyList ...");

            if (!Loaded)
            {
                // Data file doesn't exist!

                ColourConsole.WriteInfo($"CompaniesList - Getting fresh CompanyList from market ...");

                // Go get the data from the internet
                var marketDataRequest = new MarketDataRequest<CompanyListingsRequest, string>(
                    new CompanyListingsRequest(),
                    $"Company Listings");
                await marketData.GetCompanyListings(marketDataRequest);
                if (marketDataRequest.MarketDataRequestStatus == MarketDataRequestStatus.SUCCESS && marketDataRequest.Resulting != null)
                {
                    // Extract data from CSV data dowloaded 
                    if (marketDataRequest.Resulting != null)
                    {
                        using (CompaniesContext context = new CompaniesContext())
                        {
                            context.Companies.AddRange(ExtractDataFromDataString(marketDataRequest.Resulting));
                            context.SaveChanges();
                        }

                        ColourConsole.WriteInfo($"CompaniesList - ... CompanyList from market retrieved and saved to data file.");
                    }
                }
            }

            ColourConsole.WriteInfo($"CompaniesList - ... CompanyList loaded.");
        }

        public List<SimpleCompany>? GetCompanies()
        {
            using (CompaniesContext context = new CompaniesContext())
            {
                return context.Companies.ToList();
            }
        }

        public List<string> GetFlatList()
        {
            List<string> flatlist = new List<string>();
            using (CompaniesContext context = new CompaniesContext())
            {
                foreach (var company in context.Companies)
                {
                    flatlist.Add(company.LongName);

                    if (company.Name != null)
                        flatlist.Add(company.Name);

                    string? companyFirstWord = GetFirstWordOfName(company.LongName);
                    if (companyFirstWord != null)
                        flatlist.Add(companyFirstWord);

                    if (company.Aliases.Count > 0)
                        flatlist.AddRange(company.Aliases);
                }
            }
            return flatlist;
        }

        public SimpleCompany? GetCompany(string text)
        {
            using (CompaniesContext context = new CompaniesContext())
            {
                return context.Companies.ToList().Find(
                    x => (x.Name.Equals(text, StringComparison.CurrentCultureIgnoreCase)) || x.Aliases.Contains(text) || x.Symbol == text);
            }
        }

        public List<SimpleCompany> GetCompaniesFromFragment(string text)
        {
            using (CompaniesContext context = new CompaniesContext())
            {
                List<SimpleCompany> companies = new List<SimpleCompany>();
                var allCompanies = context.Companies.ToList();
                companies.AddRange(allCompanies.FindAll(x => x.Name != null && x.Name.Contains(text)));
                companies.AddRange(allCompanies.FindAll(x => x.Aliases.Contains(text)));
                return companies;
            }
        }

        public SimpleCompany? GetCompanyBySymbol(string symbol)
        {
            using (CompaniesContext context = new CompaniesContext())
            {
                return context.Companies.Where(x => x.Symbol == symbol).FirstOrDefault();
            }
        }

        public SimpleCompany? GetCompanyByName(string text)
        {
            using (CompaniesContext context = new CompaniesContext())
            {
                return context.Companies.Where(x => x.Name == text).FirstOrDefault();
            }
        }

        public List<SimpleCompany>? GetCompanies(Regex pattern)
        {
            using (CompaniesContext context = new CompaniesContext())
            {
                return context.Companies.Where(x => x.Name != null && pattern.IsMatch(x.Name)).ToList();
            }
        }

        private static List<SimpleCompany>? ExtractDataFromDataString(string marketDataString)
        {
            List<SimpleCompany>? companies = null;
            if (marketDataString != null)
            {
                companies = new List<SimpleCompany>();

                //// Extract data from CSV data string
                var stream = new MemoryStream();
                var streamWriter = new StreamWriter(stream);
                streamWriter.Write(marketDataString);
                streamWriter.Flush();
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var cswList = csv.GetRecords<Csv>();
                    foreach (var record in cswList)
                    {
                        if (record.symbol != "" && record.name != "")
                        {
                            string exchange = record.exchange.Replace(' ', '_');
                            SimpleCompany simpleCompany = new SimpleCompany(
                                record.symbol,
                                record.name,
                                (Exchanges)Enum.Parse<Exchanges>(exchange),
                                (AssetTypes)Enum.Parse<AssetTypes>(record.assetType.ToUpper()),
                                DateTime.Parse(record.ipoDate),
                                record.status);
                            companies.Add(simpleCompany);
                        }
                    }
                }
            }
            return companies;
        }

        private static string? GetFirstWordOfName(string name)
        {
            string? firstWord = null;

            string[] splitText = Utils.SplitText(name);
            if (splitText.Length > 0)
            {
                string text = splitText[0];
                if (text.Length >= MIN_COMPANY_NAME_SIZE)
                {
                    firstWord = text;
                }
                else if (Utils.IsAllUppercase(text))
                {
                    // e.g. like UBS, ATA, etc.
                    firstWord = text;
                }
                else
                {
                    // Ignore - used for debugging
                }
            }

            return firstWord;
        }

    }
}

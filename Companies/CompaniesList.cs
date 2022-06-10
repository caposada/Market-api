using CsvHelper;
using Elements;
using StockManager;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Companies
{
    public class CompaniesList : IDataStoragable<CompaniesListStore>
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

        public List<SimpleCompany>? Companies
        {
            get
            {
                return Store.Data.Companies;
            }
        }
        public bool Loaded
        {
            get
            {
                return Store != null && Store.Data.Companies != null && Store.Data.Companies.Count > 0;
            }
        }

        [JsonIgnore]
        public DataStorage<CompaniesListStore>? Store { get; set; }

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private MarketData marketData;

        public CompaniesList(MarketData marketData)
        {
            this.marketData = marketData;
            this.Store = new DataStorage<CompaniesListStore>(new CompaniesListStore());
        }

        public async Task LoadAsync()
        {
            ColourConsole.WriteInfo($"Loading CompanyList ...");

            // Load data file
            this.Store.Load();

            if (!Loaded)
            {
                // Data file doesn't exist!

                ColourConsole.WriteInfo($"Getting fresh CompanyList from market ...");

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
                        Store.Data.Companies = ExtractDataFromDataString(marketDataRequest.Resulting);

                        // Save to data file
                        Store.Save();

                        ColourConsole.WriteInfo($"... CompanyList from market retrieved and saved to data file.");
                    }
                }
            }

            ColourConsole.WriteInfo($"... CompanyList loaded.");
        }

        public List<SimpleCompany>? GetCompanies()
        {
            return Store.Data.Companies;
        }

        public List<string> GetFlatList()
        {
            List<string> flatlist = new List<string>();
            if (Store.Data.Companies != null)
            {
                foreach (var company in Store.Data.Companies)
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
            SimpleCompany? company = null;
            if (Store.Data.Companies != null)
            {
                company = Store.Data.Companies.Find(
                    x => (x.Name != null && x.Name.Equals(text, StringComparison.CurrentCultureIgnoreCase)) || x.Aliases.Contains(text) || x.Symbol == text);
            }
            return company;
        }

        public List<SimpleCompany> GetCompaniesFromFragment(string text)
        {
            if (Store.Data.Companies != null)
            {
                List<SimpleCompany> companies = new List<SimpleCompany>();
                companies.AddRange(Store.Data.Companies.FindAll(x => x.Name != null && x.Name.Contains(text)));
                companies.AddRange(Store.Data.Companies.FindAll(x => x.Aliases.Contains(text)));
                return companies;
            }
            return new List<SimpleCompany>();
        }

        public SimpleCompany? GetCompanyBySymbol(string symbol)
        {
            if (Store.Data.Companies != null)
                return Store.Data.Companies.Find(x => x.Symbol == symbol);
            return null;
        }

        public SimpleCompany? GetCompanyByName(string text)
        {
            if (Store.Data.Companies != null)
                return Store.Data.Companies.Find(x => x.Name == text);
            return null;
        }

        public List<SimpleCompany>? GetCompanies(Regex pattern)
        {
            if (Store.Data.Companies != null)
                return Store.Data.Companies.Where(x => x.Name != null && pattern.IsMatch(x.Name)).ToList();
            return null;
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
                            SimpleCompany simpleCompany = new SimpleCompany(
                                record.symbol,
                                record.name,
                                record.exchange,
                                record.assetType,
                                record.ipoDate,
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

        public void Destroy()
        {
            throw new NotImplementedException();
        }
    }
}

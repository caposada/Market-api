using Elements;
using StockManager;
using System.Text.Json;

namespace Companies
{
    public delegate void CompanyOverviewNotify(string symbol);  // delegate

    public class CompanyOverviewList
    {
        public event CompanyOverviewNotify? OverviewChanged;    // event

        private const int MONTHS_VALID = 6; // 6 months that the overview data will be valid

        public class Store : StoreBase
        {
            public List<CompanyOverview> Overviews { get; set; }

            public Store()
            {
                this.Overviews = new List<CompanyOverview>();
            }

            public override string GetFilename()
            {
                return "CompanyOverviewList";
            }

            public override string? GetFolderName()
            {
                return null;
            }

            public override string GetPathPrefix()
            {
                return Constants.COMPANIES_FOLDER_NAME;
            }
        }

        public List<CompanyOverview> Overviews
        {
            get
            {
                return store.Data.Overviews;
            }
        }

        private DataStorage<Store> store;
        private MarketData marketData;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public CompanyOverviewList(MarketData marketData)
        {
            this.marketData = marketData;
            this.store = new DataStorage<Store>(new Store());
            this.store.Load();
        }

        public bool HasOverview(string symbol)
        {
            var overview = store.Data.Overviews.Find(x => x.Symbol == symbol);
            return overview != null;
        }

        public async Task<CompanyOverview?> GetOverview(SimpleCompany company)
        {
            try
            {
                await _semaphore.WaitAsync();

                var overview = store.Data.Overviews.Find(x => x.Symbol == company.Symbol);
                if (overview != null)
                {
                    if (overview.LastUpdated < DateTime.Now.AddMonths(-MONTHS_VALID)) // Over a number of months old!
                    {
                        // Overview is a bit old, lets get some new data
                        // Destroy the current overview and remove it from the list
                        store.Data.Overviews.Remove(overview);
                        store.Save();
                        overview = null;
                    }
                    else if (!overview.IsValid())
                    {
                        // We don't have the proper data, so retreive it again
                        // Destroy the current overview and remove it from the list
                        store.Data.Overviews.Remove(overview);
                        store.Save();
                        overview = null;
                    }
                    else
                    {
                        // Found, valid and up to date!
                        return overview;
                    }
                }

                if (overview == null)
                    _ = GetNewOverviewData(company); // Go get some (new) data (in the background)

                return overview;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task GetNewOverviewData(SimpleCompany company)
        {
            var requesting = new CompanyOverviewRequest(company.Symbol);
            var marketDataRequest = new MarketDataRequest<CompanyOverviewRequest, string>(requesting, $"Company Overview (Symbol:{company.Symbol})");
            await marketData.GetCompanyOverview(marketDataRequest);
            if (marketDataRequest.MarketDataRequestStatus == MarketDataRequestStatus.SUCCESS && marketDataRequest.Resulting != null)
            {
                Dictionary<string, string>? overviewDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(marketDataRequest.Resulting);
                if (overviewDictionary != null)
                {
                    // Data
                    var overview = new CompanyOverview(company, overviewDictionary);
                    overview.LastUpdated = DateTime.Now;
                    if (overview.IsValid())
                    {
                        store.Data.Overviews.Add(overview);
                        store.Save();
                        company.HasOverview = true;
                        OverviewChanged?.Invoke(company.Symbol);
                    }
                }
            }
            else
            {

            }
        }

    }
}

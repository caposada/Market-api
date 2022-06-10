using DataStorage;
using Elements;
using StockManager;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Companies
{
    public delegate void CompanyOverviewNotify(string symbol);  // delegate
      
    public class CompanyOverviewList : IDataStoragable<CompanyOverviewStore>
    {
        public event CompanyOverviewNotify? OverviewChanged;    // event

        private const int MONTHS_VALID = 6; // 6 months that the overview data will be valid
        
        public List<CompanyOverview> Overviews
        {
            get
            {
                return Store.Data.Overviews;
            }
        }

        [JsonIgnore]
        public DataStorage<CompanyOverviewStore>? Store { get; set; }

        private MarketData marketData;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public CompanyOverviewList(MarketData marketData)
        {
            this.marketData = marketData;
            this.Store = new DataStorage<CompanyOverviewStore>(new CompanyOverviewStore());
            this.Store.Load();
        }

        public bool HasOverview(string symbol)
        {
            var overview = Store.Data.Overviews.Find(x => x.Symbol == symbol);
            return overview != null;
        }

        public async Task<CompanyOverview?> GetOverview(SimpleCompany company)
        {
            try
            {
                await _semaphore.WaitAsync();

                var overview = Store.Data.Overviews.Find(x => x.Symbol == company.Symbol);
                if (overview != null)
                {
                    if (overview.LastUpdated < DateTime.Now.AddMonths(-MONTHS_VALID)) // Over a number of months old!
                    {
                        // Overview is a bit old, lets get some new data
                        // Destroy the current overview and remove it from the list
                        Store.Data.Overviews.Remove(overview);
                        Store.Save();
                        overview = null;
                    }
                    else if (!overview.IsValid())
                    {
                        // We don't have the proper data, so retreive it again
                        // Destroy the current overview and remove it from the list
                        Store.Data.Overviews.Remove(overview);
                        Store.Save();
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
                        Store.Data.Overviews.Add(overview);
                        Store.Save();
                        company.HasOverview = true;
                        OverviewChanged?.Invoke(company.Symbol);
                    }
                }
            }
            else
            {

            }
        }

        public void Destroy()
        {
            throw new NotImplementedException();
        }
    }
}

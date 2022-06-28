using Elements;
using StockManager;

namespace Companies
{
    public delegate void CompanyOverviewNotify(string symbol);  // delegate

    public class CompanyOverviewList
    {
        public event CompanyOverviewNotify? OverviewChanged;    // event

        private const int MONTHS_VALID = 6; // 6 months that the overview data will be valid

        public List<CompanyOverview> Overviews
        {
            get
            {
                using (CompaniesContext context = new CompaniesContext())
                {
                    return context.Overviews.ToList();
                }
            }
        }

        private MarketData marketData;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public CompanyOverviewList(MarketData marketData)
        {
            this.marketData = marketData;
        }

        public bool HasOverview(string symbol)
        {
            using (CompaniesContext context = new CompaniesContext())
            {
                var overview = context.Overviews.Where(x => x.Symbol == symbol);
                return overview != null;
            }
        }

        public async Task<CompanyOverview?> GetOverview(SimpleCompany company)
        {
            try
            {
                await _semaphore.WaitAsync();

                using (CompaniesContext context = new CompaniesContext())
                {
                    var overview = context.Overviews.Where(x => x.Symbol == company.Symbol).First();
                    if (overview != null)
                    {
                        if (overview.LastUpdated < DateTime.Now.AddMonths(-MONTHS_VALID)) // Over a number of months old!
                        {
                            // Overview is a bit old, lets get some new data
                            // Destroy the current overview and remove it from the list
                            context.Overviews.Remove(overview);
                            context.SaveChanges();
                            overview = null;
                        }
                        else if (!overview.IsValid())
                        {
                            // We don't have the proper data, so retreive it again
                            // Destroy the current overview and remove it from the list
                            context.Overviews.Remove(overview);
                            context.SaveChanges();
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
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task GetNewOverviewData(SimpleCompany company)
        {
            using (CompaniesContext context = new CompaniesContext())
            {
                var requesting = new CompanyOverviewRequest(company.Symbol);
                var marketDataRequest = new MarketDataRequest<CompanyOverviewRequest, string>(requesting, $"Company Overview (Symbol:{company.Symbol})");
                await marketData.GetCompanyOverview(marketDataRequest);
                if (marketDataRequest.MarketDataRequestStatus == MarketDataRequestStatus.SUCCESS && marketDataRequest.Resulting != null)
                {
                    // Data
                    var overview = new CompanyOverview(company.Symbol, marketDataRequest.Resulting);
                    overview.LastUpdated = DateTime.Now;
                    if (overview.IsValid())
                    {
                        context.Overviews.Add(overview);
                        context.SaveChanges();
                        company.HasOverview = true;
                        OverviewChanged?.Invoke(company.Symbol);
                    }
                }
                else
                {

                }
            }
        }

    }
}

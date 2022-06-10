namespace StockManager
{
    public class CompanyOverviewRequest
    {
        public string Symbol { get; set; }

        public CompanyOverviewRequest(string symbol)
        {
            Symbol = symbol;
        }
    }
}

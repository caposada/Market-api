namespace StockManager
{
    public class QuoteRequest
    {
        public string Symbol { get; set; }

        public QuoteRequest(string symbol)
        {
            Symbol = symbol;
        }
    }
}

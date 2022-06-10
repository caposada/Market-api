namespace StockManager
{
    public class MarketDataRequestAction
    {
        public MarketDataRequest MarketDataRequest { get; set; }
        public Action Action { get; set; }

        public MarketDataRequestAction(MarketDataRequest marketDataRequest, Action action)
        {
            this.MarketDataRequest = marketDataRequest;
            this.Action = action;
        }
    }
}

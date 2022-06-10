namespace StockManager
{
    public class RequestResult
    {
        public Guid? Id { get; set; }
        public MarketDataRequestStatus Status { get; set; }

        public RequestResult(Guid? id, MarketDataRequestStatus status)
        {
            this.Id = id;
            this.Status = status;
        }
    }
}

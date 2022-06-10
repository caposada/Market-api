namespace StockManager
{
    public class MarketDataRequest
    {
        public const int MAX_ERRORS = 10;
        private int errorCount = 0;

        public MarketDataRequestStatus MarketDataRequestStatus { get; set; }
        public int ErrorCount
        {
            get
            {
                return this.errorCount;
            }
            set
            {
                this.errorCount = value;
                if (this.errorCount >= MAX_ERRORS)
                    MarketDataRequestStatus = MarketDataRequestStatus.ERROR;
            }
        }
        public string Description { get; set; }
        public Guid? RecordId { get; set; }

        public MarketDataRequest(string description, Guid? recordId = null)
        {
            this.Description = description;
            this.RecordId = recordId;
        }
    }

    public class MarketDataRequest<Request, Result> : MarketDataRequest
    {
        public Request Requesting { get; set; }
        public Result? Resulting { get; set; }

        public MarketDataRequest(Request requesting, string description, Guid? recordId = null)
            : base(description, recordId)
        {
            this.Requesting = requesting;
        }

        public void SetResult(Result? Resulting)
        {
            this.Resulting = Resulting;
            this.MarketDataRequestStatus = MarketDataRequestStatus.SUCCESS;
        }

    }
}

namespace Elements
{


    public abstract class NewsFeed
    {

        private TimeSpan? timeOffset = null;

        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public TimeSpan TimeOffset
        {
            get
            {
                if (timeOffset == null)
                {
                    WorldTimeAPI.TimeZone timeZone = new WorldTimeAPI.TimeZone("America", "New_York");
                    WorldTimeAPI.WorldTimeAPIClient client = new WorldTimeAPI.WorldTimeAPIClient();
                    var time = client.GetTime(timeZone);
                    TimeSpan offset = time.UtcOffset;
                    TimeSpan extra = time.Dst ? TimeSpan.FromHours(1) : TimeSpan.Zero;
                    timeOffset = offset.Subtract(extra);
                }
                return timeOffset.Value;
            }
        }

        public NewsFeed(Guid id, string title, string url)
        {
            this.Id = id;
            this.Title = title;
            this.Url = url;
        }

        public async Task<List<NewsItem>> ProcessFeedAsync()
        {
            return await GetNews();
        }

        protected abstract Task<List<NewsItem>> GetNews();

        protected DateTimeOffset GetCorrectDateTimeOffset(DateTimeOffset dateTime)
        {
            DateTimeOffset localDateTime = dateTime.LocalDateTime;
            DateTimeOffset correctedDateTime = localDateTime + TimeOffset;
            return correctedDateTime;
        }

    }

}

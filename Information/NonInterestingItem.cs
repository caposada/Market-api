namespace Information
{
    public class NonInterestingItem : GathererInformationItem
    {

        public NonInterestingItem(Guid newsItemId, Guid sourceId, DateTime timestamp, string text, DateTimeOffset publishDate)
            : base(newsItemId, sourceId, timestamp, text, publishDate)
        {
        }

    }
}

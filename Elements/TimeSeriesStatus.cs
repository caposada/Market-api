namespace Elements
{
    public enum TimeSeriesStatus
    {
        PRE_DATE,       // NeedToRetrieveTimeSeriesData = true = To early, needs to be the following evening at 8pm (or weekend/holiday)
        READY,          // NeedToRetrieveTimeSeriesData = true = Good to go - the initial setting <---- CAP get rid of this when no longer used
        PARTIAL,        // NeedToRetrieveTimeSeriesData = true = Some data collected, but not complete
        SUCCESS,        // NeedToRetrieveTimeSeriesData = false = Data retreived and stored
        PAST_DATE,      // NeedToRetrieveTimeSeriesData = false = The data is old (CAP we could get full TimeSeries and not just the 100 datapoints?)
        OUT_OF_HOURS,   // NeedToRetrieveTimeSeriesData = false = The NewsItem was published out of hours
        NOT_SEEN        // NeedToRetrieveTimeSeriesData = true = This item has not been seen yet 
    }
}

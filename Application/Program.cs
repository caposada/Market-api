// See https://aka.ms/new-console-template for more information
using Elements;
using StockManager;

Testing.Test_StockManager();
//Testing.Test_StockMarket();
//Testing.Test_MarketRequestor();
//Testing.Test_CompanyList();
//Testing.Test_Twitter();
//Testing.Test_Analysis();
//Testing.Test_WorldTimeAPI();


Console.WriteLine("Press any key to finish.");
ConsoleKeyInfo keyInfo = Console.ReadKey(true);

public static class Testing
{


    public async static void Test_StockManager()
    {
        MarketData marketData = new MarketData();
        string symbol = "AAPL";
        var requesting = new CompanyOverviewRequest(symbol);
        var marketDataRequest = new MarketDataRequest<CompanyOverviewRequest, string>(requesting, $"Company Overview (Symbol:{symbol})");
        await marketData.GetCompanyOverview(marketDataRequest);
        if (marketDataRequest.MarketDataRequestStatus == MarketDataRequestStatus.SUCCESS && marketDataRequest.Resulting != null)
        {
            var result = marketDataRequest.Resulting;
        }
    }

    public static void Test_StockMarket()
    {
        NYSE stockMarket = new NYSE();

        stockMarket.CalculateNextOpenDateTime();

        TimeSpan stockMarketLocalTime = stockMarket.GetTimeAtStockExchange();
        TimeSpan timeLeftUntilMarketOpens = stockMarket.TimeLeftUntilOpen();
        TimeSpan timeLeftUntilMarketCloses = stockMarket.TimeLeftUntilClose();
        bool isMarketOpen = stockMarket.IsOpenNow();

        var result = stockMarket.IsOpen(new DateTime(2022, 05, 15, 12, 0, 0));

    }

    public static void Test_Twitter()
    {
        //TwitterFeed twitterFeed = new TwitterFeed("MarketWatch", "marketwatch", "America/New_York");
        //twitterFeed.Test();

    }

    public static void Test_Analysis()
    {
        //string text1 = "Post season in any sport is a privilege to watch. $NBA $NHL";
        //string text2 = "Nice to see $SBUX Interim CEO Howard Schultz buying 137.5K shares.";
        //string text3 = "Why Berkshire Hathaway Didn’t Have to Disclose Earlier its 10% Stake in Paramount";
        ////              012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
        ////              0         1         2         3         4         5         6         7         8    

        //NewsItem newsItem = new NewsItem("", DateTimeOffset.Now, text3, new RssFeed(""));
        //CompanyDataStore companyDataStore = new CompanyDataStore();
        //Analyser analyser = new Analyser(companyDataStore, newsItem);

        //analyser.Analyse();
    }

    public static void Test_WorldTimeAPI()
    {

        WorldTimeAPI.TimeZone timeZone = new WorldTimeAPI.TimeZone("America", "New_York");


        WorldTimeAPI.WorldTimeAPIClient client = new WorldTimeAPI.WorldTimeAPIClient();
        var time = client.GetTime(timeZone);
        TimeSpan offset = time.UtcOffset;
    }


}

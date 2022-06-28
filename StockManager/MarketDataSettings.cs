using Configuration;

namespace StockManager
{
    public partial class MarketData
    {
        public class MarketDataSettings
        {
            public string ApiKey { get; set; }
            public int MaxHitsPerMinute { get; set; }
            public int MaxHitsPerDay { get; set; }

            public MarketDataSettings()
            {
                using (ConfigurationContext context = new ConfigurationContext())
                {
                    ApiKey = context.Settings.Where(x => x.Name == "AlphaVantage_ApiKey").First().Value;
                    MaxHitsPerMinute = int.Parse(context.Settings.Where(x => x.Name == "AlphaVantage_MaxHitsPerMinute").First().Value);
                    MaxHitsPerDay = int.Parse(context.Settings.Where(x => x.Name == "AlphaVantage_MaxHitsPerDay").First().Value);
                }
            }

        }
    }
}

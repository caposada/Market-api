using Elements;
using System.Text.Json.Serialization;

namespace StockManager
{

    public partial class MarketData
    {
        public class MarketDataSettings : IDataStoragable<MarketDataSettingsStore>
        {     
            public string? ApiKey
            {
                get
                {
                    return Store.Data.AlphaVantage_ApiKey;
                }
            }
            public int? MaxHitsPerMinute
            {
                get
                {
                    return Store.Data.AlphaVantage_MaxHitsPerMinute;
                }
            }
            public int? MaxHitsPerDay
            {
                get
                {
                    return Store.Data.AlphaVantage_MaxHitsPerDay;
                }
            }

            [JsonIgnore]
            public DataStorage<MarketDataSettingsStore>? Store { get; set; }

            public MarketDataSettings()
            {
                this.Store = new DataStorage<MarketDataSettingsStore>(new MarketDataSettingsStore());
                this.Store.Load();
            }

            public void Destroy()
            {
                throw new NotImplementedException();
            }
        }
    }
}

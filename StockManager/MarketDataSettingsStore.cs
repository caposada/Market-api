using Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManager
{
    public class MarketDataSettingsStore : StoreBase
    {
        public string? AlphaVantage_ApiKey { get; set; }
        public int? AlphaVantage_MaxHitsPerMinute { get; set; }
        public int? AlphaVantage_MaxHitsPerDay { get; set; }

        public MarketDataSettingsStore()
        {
        }

        public override string GetFilename()
        {
            return "Settings_MarketData";
        }

        public override string? GetFolderName()
        {
            return null;
        }

        public override string GetPathPrefix()
        {
            return Constants.APP_SETTINGS_FOLDER_NAME;
        }
    }
}

using DataStorage;
using Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News
{
    public class NewsManagerSettingsStore : StoreBase
    {
        public class StandardNewsSource
        {
            public string? Timezone { get; set; }
            public FeedType FeedType { get; set; }
            public string FeedTitle { get; set; }
            public string FeedUrl { get; set; }
        }

        public List<StandardNewsSource> StandardNewsSources { get; set; }

        public NewsManagerSettingsStore()
        {
        }

        public override string GetFilename()
        {
            return "Settings_NewsManager";
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

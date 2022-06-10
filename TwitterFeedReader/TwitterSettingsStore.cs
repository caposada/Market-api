using DataStorage;
using Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterFeedReader
{
    public class TwitterSettingsStore : StoreBase
    {
        public string? Twitter_ApiKey { get; set; }
        public string? Twitter_ApiKeySecret { get; set; }
        public string? Twitter_AccessToken { get; set; }
        public string? Twitter_AccessTokenSecret { get; set; }

        public TwitterSettingsStore()
        {
        }

        public override string GetFilename()
        {
            return "Settings_Twitter";
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

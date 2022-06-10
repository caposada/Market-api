using Elements;

namespace News
{
    public class SourceMonitorStore : StoreBase
    {
        public TimeSpan PollingTimespan { get; set; } = Constants.DEFAULT_POLLING_PERIOD;

        private string? folderName;

        public SourceMonitorStore()
        {
        }

        public SourceMonitorStore(string? folderName)
        {
            this.folderName = folderName;
        }

        public override string GetFilename()
        {
            return "Settings_SourceMonitor";
        }

        public override string? GetFolderName()
        {
            return folderName;
        }

        public override string GetPathPrefix()
        {
            return Constants.FEED_FOLDER_NAME;
        }
    }
}

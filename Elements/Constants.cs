namespace Elements
{
    public static class Constants
    {

        public const string USE_FILE_SYSTEM = "NORMAL"; // ISOSTORE OR NORMAL (i.e. 'ROOT_PATH')
        public const string ROOT_PATH = "C:/market/";
        public const string DATASTORE_PATH = "datastore/";
        public const string APP_SETTINGS_FOLDER_NAME = "app/";
        public const string JSON_DATA_FILENAME = "data.json";
        public const string CSV_DATA_FILENAME = "data.csv";
        public const string COMPANIES_FOLDER_NAME = "companies/";
        public const string FEED_FOLDER_NAME = "feeds/";
        public const string GATHERER_FOLDER_NAME = "gatherer/";
        public const string MARKETDATA_REQUEST_FOLDER_NAME = "marketdatarequests/";
        public const int MIN_POLLING_SECONDS = 60;
        public static readonly TimeSpan DEFAULT_POLLING_PERIOD = new TimeSpan(0, 30, 0); // £0 minutes
        public static readonly TimeSpan DEFAULT_CULL_PERIOD = new TimeSpan(20, 0, 0, 0); // 20 days, after which certain items are deemed old


    }
}

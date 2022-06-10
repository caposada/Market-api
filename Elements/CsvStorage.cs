namespace Elements
{
    public class CsvStorage : Storage
    {
        public static string LoadData(string path, string? datafileName)
        {
            string csvfilename = datafileName != null ? datafileName : Constants.CSV_DATA_FILENAME;
            return LoadDataFile(path, csvfilename);
        }

        public static void SaveData(string path, string dataString, string? datafileName)
        {
            string csvfilename = datafileName != null ? datafileName : Constants.CSV_DATA_FILENAME;
            SaveDataFile(path, dataString, csvfilename);
        }

    }
}

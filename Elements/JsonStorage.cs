using System.Text.Json;

namespace Elements
{
    public class JsonStorage<T> : Storage
    {

        public static T? LoadData(string path, string? datafileName)
        {
            string jsonfilename = datafileName != null ? datafileName : Constants.JSON_DATA_FILENAME;
            string? jsonString = LoadDataFile(path, jsonfilename);
            if (jsonString != null)
            {
                try
                {
                    T? t = JsonSerializer.Deserialize<T>(jsonString);
                    return t;
                }
                catch (Exception)
                {
                }
            }
            return default;
        }

        public static void SaveData(string path, T obj, string? datafileName)
        {
            string jsonfilename = datafileName != null ? datafileName : Constants.JSON_DATA_FILENAME;
            string jsonString = JsonSerializer.Serialize(obj);
            SaveDataFile(path, jsonString, jsonfilename);
        }

        public static void RemoveFolder(string path)
        {
            RemoveDataFolder(path);
        }

    }
}

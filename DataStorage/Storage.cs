using Elements;
using System.IO.IsolatedStorage;

namespace DataStorage
{

    public interface IFileStorage
    {
        abstract bool FileExists(string path);

        abstract void DeleteDirectory(string path);

        abstract List<string> GetDirectories(string path);

        abstract bool DirectoryExists(string path);

        abstract void CreateDirectory(string path);

        abstract Stream GetStream(string path, FileMode fileMode);

        abstract void DeleteFile(string path);

    }

    public class NormalFileStorage : IFileStorage
    {
        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(GetPrefixPath() + path);
        }

        public void DeleteDirectory(string path)
        {
            Directory.Delete(GetPrefixPath() + path, true);
        }

        public List<string> GetDirectories(string path)
        {
            return Directory.GetDirectories(GetPrefixPath() + path).ToList();
        }

        public void DeleteFile(string fullFilePath)
        {
            File.Delete(GetPrefixPath() + fullFilePath);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(GetPrefixPath() + path);
        }

        public bool FileExists(string fullFilePath)
        {
            return File.Exists(GetPrefixPath() + fullFilePath);
        }

        public Stream GetStream(string fullFilePath, FileMode fileMode)
        {
            return File.Open(
                GetPrefixPath() + fullFilePath,
                fileMode);
        }

        protected string GetPrefixPath()
        {
            return Constants.ROOT_PATH + Constants.DATASTORE_PATH;
        }
    }

    public class IsoFileStorage : IFileStorage
    {
        protected static IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(
            IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

        public void CreateDirectory(string path)
        {
            isoStore.CreateDirectory(path);
        }

        public void DeleteDirectory(string path)
        {
            isoStore.DeleteDirectory(path);
        }

        public List<string> GetDirectories(string path)
        {
            return isoStore.GetDirectoryNames().ToList();
        }

        public void DeleteFile(string fullFilePath)
        {
            isoStore.DeleteFile(fullFilePath);
        }

        public bool DirectoryExists(string path)
        {
            return isoStore.DirectoryExists(path);
        }

        public bool FileExists(string fullFilePath)
        {
            return isoStore.FileExists(fullFilePath);
        }

        public Stream GetStream(string fullFilePath, FileMode fileMode)
        {
            return new IsolatedStorageFileStream(
                fullFilePath,
                fileMode,
                isoStore);
        }
    }

    public class Storage
    {

        protected static IFileStorage fileStorage = GetStream();

        protected static IFileStorage GetStream()
        {
            if (Constants.USE_FILE_SYSTEM == "ISOSTORE")
            {
                return new IsoFileStorage();
            }
            else
            {
                return new NormalFileStorage();
            }
        }

        public static string? LoadDataFile(string path, string datafileName)
        {

            string? dataString = null;
            string fullFilePath = path + datafileName;
            if (fileStorage.FileExists(fullFilePath))
            {
                using (Stream stream = fileStorage.GetStream(
                    fullFilePath,
                    FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataString = reader.ReadToEnd();
                    }
                }
            }

            return dataString;
        }

        public static void SaveDataFile(string path, string dataString, string datafileName)
        {
            if (!fileStorage.DirectoryExists(path))
            {
                fileStorage.CreateDirectory(path);
            }

            string fullFilePath = path + datafileName;
            fileStorage.DeleteFile(fullFilePath);

            using (Stream stream = fileStorage.GetStream(
                fullFilePath,
                FileMode.CreateNew))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataString);
                }
            }
        }

        public static void RemoveDataFolder(string path)
        {
            fileStorage.DeleteDirectory(path);
        }

        public static List<string> GetDataFolders(string path)
        {
            return fileStorage.GetDirectories(path);
        }

        public static bool FileExists(string path, string datafileName)
        {
            string fullFilePath = path + datafileName;
            return fileStorage.FileExists(fullFilePath);
        }

        public static async Task CleanAsync(string path, List<Guid> validIds)
        {
            // This checks if the folder is associated with a valid id,
            // and if not, the folder is deleted
            await Task.Run(() =>
            {
                var folders = fileStorage.GetDirectories(path);
                foreach (string folder in folders)
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(folder);
                    string folderName = directoryInfo.Name;
                    Guid id;
                    bool isGuid = Guid.TryParse(folderName, out id);
                    if (isGuid && !validIds.Contains(Guid.Parse(folderName)))
                    {
                        directoryInfo.Delete(true);
                    }
                }
            });
        }
    }

}

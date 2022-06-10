namespace Elements
{    
    public class DataStorage<T> : DataStorageBase<StoreBase> where T : new()
    {
        public T Data { get; set; }

        private object lockObject = new object();
        private string? subFoldername = null;

        public DataStorage(T store, string? subFoldername = null)
            : base(store as StoreBase)
        {
            this.Data = store;
            this.subFoldername = subFoldername;
        }

        public bool Exists(string dataFilename)
        {
            return JsonStorage<T>.FileExists(GetPath(), dataFilename);
        }

        public bool Exists()
        {
            return Exists(GetFileName());
        }

        public void Load(string dataFilename)
        {
            lock (lockObject)
            {
                T? data = JsonStorage<T>.LoadData(GetPath(), dataFilename);
                if (data != null)
                    Data = data;
            }
        }

        public void Load()
        {
            Load(GetFileName());
        }

        public void Save(string dataFilename)
        {
            lock (lockObject)
            {
                JsonStorage<T>.SaveData(GetPath(), Data, dataFilename);
            }
        }

        public void Save()
        {
            Save(GetFileName());
        }

        public void Destroy()
        {
            string path = GetPath();
            JsonStorage<T>.RemoveFolder(path);
        }

        public async Task CleanAsync(List<Guid> validIds)
        {
            string path = GetPath();
            await JsonStorage<T>.CleanAsync(path, validIds);
        }

        protected string GetPath()
        {
            string subPathFoldername = subFoldername ?? "Store";
            return PathPrefix
                + subPathFoldername
                + "/"
                + (FolderName != null ? FolderName + "/" : "");
        }

        protected string GetFileName()
        {
            return FileName != null ? FileName + ".json" : Constants.JSON_DATA_FILENAME;
        }

    }

}

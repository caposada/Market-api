namespace DataStorage
{
    public abstract class StoreBase
    {
        public abstract string GetFilename();

        public abstract string? GetFolderName();

        public abstract string GetPathPrefix();
    }
}

using Elements;
using System.Text.Json.Serialization;

namespace StockManager
{
    public class Retrieval<Request, Result> : BaseRetrieval
    {

        public class Store : StoreBase
        {
            public Result? Result { get; set; }

            private string? folderName;

            public Store()
            {
            }

            public Store(string? folderName)
            {
                this.folderName = folderName;
            }

            public override string GetFilename()
            {
                return "Result";
            }

            public override string? GetFolderName()
            {
                return folderName;
            }

            public override string GetPathPrefix()
            {
                return Constants.MARKETDATA_REQUEST_FOLDER_NAME;
            }
        }

        private DataStorage<Store>? store;

        public Request? Requesting { get; set; }

        [JsonConstructor]
        public Retrieval()
        {

        }

        public Retrieval(Request requesting, Result? resulting, DateTime validUntil, Guid? recordId = null)
        {
            this.RecordId = recordId ?? Guid.NewGuid();
            this.Requesting = requesting;
            this.ValidUntil = validUntil;

            this.store = new DataStorage<Store>(new Store(RecordId.ToString()));
            this.store.Data.Result = resulting;
            this.store.Save();
        }

        public Result? GetResult()
        {
            if (store == null)
                store = new DataStorage<Store>(new Store(RecordId.ToString()));

            if (store.Data.Result == null)
                store.Load();

            return store.Data.Result;
        }

    }
}

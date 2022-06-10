using Elements;
using System.Text.Json.Serialization;

namespace StockManager
{
    public class Request
    {
        public Guid Id { get; set; }
        public DateTime TimeStamp { get; private set; }

        public Request(Guid id)
        {
            this.Id = id;
            this.TimeStamp = DateTime.Now;
        }

        [JsonConstructor]
        public Request()
        {
            this.Id = Guid.NewGuid();
            this.TimeStamp = DateTime.Now;
        }

    }

    public class GenericRequest<T> : Request
    {
        public class Store : StoreBase
        {
            public T? Result { get; set; }
            public string? Symbol { get; set; }
            public DateTime ValidUntil { get; set; }
            public DateTime TimeStamp { get; set; }

            private string? folderName;

            public Store()
            {
            }

            public Store(string folderName)
            {
                this.folderName = folderName;
            }

            public override string GetFilename()
            {
                return "Request";
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

        [JsonIgnore]
        public T? Result
        {
            get
            {
                return store.Data.Result;
            }
            set
            {
                store.Data.Result = value;
                store.Data.TimeStamp = DateTime.Now;
                store.Save();
            }
        }
        public string? Symbol
        {
            get
            {
                return store.Data.Symbol;
            }
            set
            {
                store.Data.Symbol = value;
            }
        }
        public DateTime ValidUntil
        {
            get
            {
                return store.Data.ValidUntil;
            }
            set
            {
                store.Data.ValidUntil = value;
            }
        }

        private DataStorage<Store> store;

        public GenericRequest(string symbol, DateTime validUntil)
            : base()
        {
            this.store = new DataStorage<Store>(new Store(this.Id.ToString()));
            this.Symbol = symbol;
            this.ValidUntil = validUntil;
        }
    }



}

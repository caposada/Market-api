using DataStorage;
using Elements;
using System.Text.Json.Serialization;

namespace StockManager
{   
    public class Retrieval<Request, Result> : BaseRetrieval, IDataStoragable<ResultStore<Result>>
    {        

        [JsonIgnore]
        public DataStorage<ResultStore<Result>>? Store { get; set; }

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

            this.Store = new DataStorage<ResultStore<Result>>(new ResultStore<Result>(RecordId.ToString()));
            this.Store.Data.Result = resulting;
            this.Store.Save();
        }

        public Result? GetResult()
        {
            if (Store == null)
                Store = new DataStorage<ResultStore<Result>>(new ResultStore<Result>(RecordId.ToString()));

            if (Store.Data.Result == null)
                Store.Load();

            return Store.Data.Result;
        }

        public void Destroy()
        {
            throw new NotImplementedException();
        }
    }
}

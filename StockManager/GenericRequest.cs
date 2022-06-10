using DataStorage;
using Elements;
using System.Text.Json.Serialization;

namespace StockManager
{
    public class GenericRequest<T> : Request, IDataStoragable<RequestStore<T>>
    {

        [JsonIgnore]
        public T? Result
        {
            get
            {
                return Store.Data.Result;
            }
            set
            {
                Store.Data.Result = value;
                Store.Data.TimeStamp = DateTime.Now;
                Store.Save();
            }
        }
        public string? Symbol
        {
            get
            {
                return Store.Data.Symbol;
            }
            set
            {
                Store.Data.Symbol = value;
            }
        }
        public DateTime ValidUntil
        {
            get
            {
                return Store.Data.ValidUntil;
            }
            set
            {
                Store.Data.ValidUntil = value;
            }
        }

        [JsonIgnore]
        public DataStorage<RequestStore<T>>? Store { get; set; }

        public GenericRequest(string symbol, DateTime validUntil)
            : base()
        {
            this.Store = new DataStorage<RequestStore<T>>(new RequestStore<T>(this.Id.ToString()));
            this.Symbol = symbol;
            this.ValidUntil = validUntil;
        }

        public void Destroy()
        {
            throw new NotImplementedException();
        }
    }

}

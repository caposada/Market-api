using Companies;
using Information;
using News;
using StockManager;

namespace Market
{
    public delegate void AppNotify(object obj);                     // delegate

    public class App
    {
        public event AppNotify? EchoMessage;                        // event

        public NewsManager NewsManager { get; }
        public MarketData MarketData { get; }
        public MarketRequestor MarketRequestor { get; }
        public CompanyDataStore CompanyDataStore { get; }
        public GathererInformation GathererInformation { get; }
        public Gatherer Gatherer { get; }


        public App()
        {
            this.MarketData = new MarketData();
            this.CompanyDataStore = new CompanyDataStore(this.MarketData);
            this.MarketRequestor = new MarketRequestor(this.MarketData);
            this.GathererInformation = new GathererInformation(this.MarketData);
            this.NewsManager = new NewsManager();
            this.Gatherer = new Gatherer(this.CompanyDataStore, this.NewsManager, this.GathererInformation);
        }

        public void WebsocketEcho(object obj)
        {
            EchoMessage?.Invoke(obj);
        }

    }
}

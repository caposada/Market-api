using Elements;
using System.Text.Json.Serialization;

namespace News
{

    public class SourceMonitor : IDataStoragable<SourceMonitorStore>
    {
        public delegate void SourceMonitorNotify(string eventName);             // delegate
        public delegate void FeedFreshArrival(List<NewsItem> freshNewsItems);   // delegate

        public event SourceMonitorNotify? Changed;                              // event
        public event FeedFreshArrival? FreshArrivals;                           // event

        public enum SourceMonitorStatus
        {
            IDLE,
            RUNNING
        }

        public SourceMonitorStatus Status { get; set; }
        public TimeSpan PollingTimespan
        {
            get
            {
                return Store.Data.PollingTimespan;
            }
            set
            {
                if (value.TotalSeconds < Constants.MIN_POLLING_SECONDS) // <--- *** make constant ***
                {
                    ColourConsole.WriteWarning(
                        $"Polling period can not be below {Constants.MIN_POLLING_SECONDS} seconds.");
                    TimeSpan newTimeSpan = new TimeSpan(0, 0, Constants.MIN_POLLING_SECONDS);
                    if (newTimeSpan.TotalSeconds != Store.Data.PollingTimespan.TotalSeconds)
                    {
                        if (timer != null)
                            timer.Interval = Store.Data.PollingTimespan.TotalMilliseconds;
                        Store.Data.PollingTimespan = new TimeSpan(0, 0, Constants.MIN_POLLING_SECONDS);
                        Store.Save();
                        Changed?.Invoke("POLLINGTIMESPAN");
                    }
                }
                else
                {
                    if (timer != null)
                        timer.Interval = value.TotalMilliseconds;
                    Store.Data.PollingTimespan = value;
                    Store.Save();
                    Changed?.Invoke("POLLINGTIMESPAN");
                }
            }
        }
        public NewsFeed Feed { get; private set; }
        public bool IsPolling
        {
            get
            {
                return timer != null && timer.Enabled;
            }
            set
            {
                bool current = timer != null && timer.Enabled;
                if (current == value)
                    return; // Nothing changed
                if (value == true)
                {
                    Start();
                }
                else
                {
                    Stop();
                }
                Changed?.Invoke("ISPOLLING");
            }
        }
        public DateTime LastPoll
        {
            get
            {
                return lastPoll;
            }
            set
            {
                lastPoll = value;
                Changed?.Invoke("LASTPOLL");
            }
        }

        [JsonIgnore]
        public DataStorage<SourceMonitorStore>? Store { get; set; }

        private System.Timers.Timer? timer;
        private DateTime lastPoll;

        public SourceMonitor(Guid id, NewsFeed feed)
        {
            this.Status = SourceMonitorStatus.IDLE;
            this.Feed = feed;
            this.Store = new DataStorage<SourceMonitorStore>(new SourceMonitorStore(id.ToString()));
            this.LastPoll = DateTime.MinValue;
        }

        public void Destroy()
        {
            Store.Destroy();
        }

        public void Load()
        {
            if (Store.Exists())
                Store.Load();
            else
                Store.Save();
        }

        public void Save()
        {
            this.Store.Save();
        }

        public void Run()
        {
            _ = RunFeed();
            if (timer == null)
            {
                timer = new System.Timers.Timer();
                timer.Interval = PollingTimespan.TotalMilliseconds;
                timer.Elapsed += Poll;
            }
        }

        public async Task ForceUpdate()
        {
            await RunFeed();
        }

        public void ShutDown()
        {
            if (timer != null)
            {
                timer.Elapsed -= Poll;
                timer.Stop();
                timer.Dispose();
                timer = null;
            }
        }

        public void Start()
        {
            if (timer != null)
            {
                timer.Start();
            }
        }

        public void Stop()
        {
            if (timer != null)
            {
                timer.Stop();
            }
        }

        private async Task RunFeed()
        {
            Status = SourceMonitorStatus.RUNNING;
            List<NewsItem> freshNewsItems = await Feed.ProcessFeedAsync(); //////// Run first feed grab
            LastPoll = DateTime.Now;
            Status = SourceMonitorStatus.IDLE;

            if (freshNewsItems.Count > 0)
            {
                FreshArrivals?.Invoke(freshNewsItems);
            }
        }

        private void Poll(object? sender, System.Timers.ElapsedEventArgs e)
        {
            _ = RunFeed();
        }

    }
}

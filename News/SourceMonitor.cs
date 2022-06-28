using Elements;

namespace News
{

    public class SourceMonitor
    {
        public delegate void SourceMonitorNotify(Guid id, string eventName);            // delegate
        public delegate void FeedFreshArrival(Guid id, List<NewsItem> freshNewsItems);  // delegate

        public event SourceMonitorNotify? Changed;                              // event
        public event FeedFreshArrival? FreshArrivals;                           // event

        public enum SourceMonitorStatus
        {
            IDLE,
            RUNNING
        }

        public Guid Id { get; set; }
        public SourceMonitorStatus Status { get; set; }
        public TimeSpan PollingTimespan
        {
            get
            {
                return pollingTimespan;
            }
            set
            {
                if (value.TotalSeconds < Constants.MIN_POLLING_SECONDS) // <--- *** make constant ***
                {
                    ColourConsole.WriteWarning($"SourceMonitor - Polling period can not be below {Constants.MIN_POLLING_SECONDS} seconds.");
                    TimeSpan newTimeSpan = new TimeSpan(0, 0, Constants.MIN_POLLING_SECONDS);
                    if (newTimeSpan.TotalSeconds != pollingTimespan.TotalSeconds)
                    {
                        if (timer != null)
                            timer.Interval = pollingTimespan.TotalMilliseconds;
                        pollingTimespan = new TimeSpan(0, 0, Constants.MIN_POLLING_SECONDS);
                    }
                }
                else
                {
                    if (timer != null)
                        timer.Interval = value.TotalMilliseconds;
                    pollingTimespan = value;
                }
            }
        }
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
                Changed?.Invoke(this.Id, "LASTPOLL");
            }
        }

        private TimeSpan pollingTimespan;
        private System.Timers.Timer? timer;
        private DateTime lastPoll;
        private Func<NewsFeed> newsFeedFunc;

        public SourceMonitor(Guid id, Func<NewsFeed> newsFeedFunc, TimeSpan pollingTimespan)
        {
            this.Id = id;
            this.pollingTimespan = pollingTimespan;
            this.newsFeedFunc = newsFeedFunc;
            this.LastPoll = DateTime.MinValue;
            this.Status = SourceMonitorStatus.IDLE;
        }

        public void Run()
        {
            _ = RunFeed();
            if (timer == null)
            {
                timer = new System.Timers.Timer();
                timer.Interval = pollingTimespan.TotalMilliseconds;
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

            NewsFeed feed = newsFeedFunc();

            List<NewsItem> freshNewsItems = await feed.ProcessFeedAsync();
            LastPoll = DateTime.Now;
            Status = SourceMonitorStatus.IDLE;

            if (freshNewsItems.Count > 0)
            {
                // Only get items that are relevant (relatively recent)
                DateTime oldesetDate = DateTime.Today - Constants.DEFAULT_CULL_PERIOD;
                var recentNewsItems = freshNewsItems.FindAll(x => x.PublishDate > oldesetDate);

                FreshArrivals?.Invoke(this.Id, recentNewsItems);
            }
        }

        private void Poll(object? sender, System.Timers.ElapsedEventArgs e)
        {
            _ = RunFeed();
        }

    }
}

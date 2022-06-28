namespace Elements
{


    public delegate void QueueProcessorActionNotify<Action>(Action action);     // delegate
    public delegate void QueueProcessorStatusNotify();                          // delegate

    public class AsyncronousQueueProcessor
    {

        public event QueueProcessorActionNotify<Action>? Added;          // event
        public event QueueProcessorActionNotify<Action>? Processing;     // event
        public event QueueProcessorActionNotify<Action>? Finished;       // event
        public event QueueProcessorStatusNotify? Started;                // event
        public event QueueProcessorStatusNotify? AllFinished;            // event

        public bool IsRunning
        {
            get
            {
                return queueProcessingTask?.Status == TaskStatus.Running;
            }
        }
        public int Count
        {
            get
            {
                return actionsQueue.Count;
            }
        }
        public bool RunAutomatically { get; set; }

        private Queue<Action> actionsQueue = new Queue<Action>();
        protected Task? queueProcessingTask;
        protected readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public AsyncronousQueueProcessor()
        {
        }

        public async Task Add(Action action)
        {
            try
            {
                await _semaphore.WaitAsync();

                actionsQueue.Enqueue(action);

                OnActionAdded(action);
            }
            finally
            {
                _semaphore.Release();
            }

            if (RunAutomatically)
                await Run();
        }

        public virtual async Task Run()
        {
            try
            {
                await _semaphore.WaitAsync();

                if (queueProcessingTask == null || queueProcessingTask.Status != TaskStatus.Running)
                {
                    queueProcessingTask = new Task(() =>
                    {
                        OnStarted();
                        while (actionsQueue.Count > 0)
                        {
                            Pulse();
                        }
                        OnAllFinished();
                    });
                    queueProcessingTask.Start();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public virtual void Pulse()
        {
            Action action = actionsQueue.Dequeue();
            if (action != null)
            {
                OnActionProcessing(action);
                action.Invoke();
                OnActionFinished(action);
            }
        }

        protected virtual void OnActionAdded(Action action)
        {
            Added?.Invoke(action);
        }

        protected virtual void OnActionProcessing(Action action)
        {
            Processing?.Invoke(action);
        }

        protected virtual void OnActionFinished(Action action)
        {
            Finished?.Invoke(action);
        }

        protected virtual void OnStarted()
        {
            Started?.Invoke();
        }

        protected virtual void OnAllFinished()
        {
            AllFinished?.Invoke();
        }

    }

}

namespace Elements
{

    public class PriorityAsyncronousQueueProcessor : AsyncronousQueueProcessor
    {

        private Queue<Action> lowPriorityActionsQueue = new Queue<Action>();
        private Queue<Action> mediumPriorityActionsQueue = new Queue<Action>();
        private Queue<Action> highPriorityActionsQueue = new Queue<Action>();

        public PriorityAsyncronousQueueProcessor()
        {
        }

        public async Task Add(Action action, QueuePriority priority)
        {
            try
            {
                await _semaphore.WaitAsync();

                switch (priority)
                {
                    case QueuePriority.LOW: lowPriorityActionsQueue.Enqueue(action); break;
                    case QueuePriority.MEDIUM: mediumPriorityActionsQueue.Enqueue(action); break;
                    case QueuePriority.HIGH: highPriorityActionsQueue.Enqueue(action); break;
                }

                base.OnActionAdded(action);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override async Task Run()
        {
            try
            {
                await _semaphore.WaitAsync();

                if (queueProcessingTask == null || queueProcessingTask.Status != TaskStatus.Running)
                {
                    queueProcessingTask = new Task(() =>
                    {
                        base.OnStarted();

                        while (
                            highPriorityActionsQueue.Count > 0 ||
                            mediumPriorityActionsQueue.Count > 0 ||
                            lowPriorityActionsQueue.Count > 0)
                        {
                            Pulse();
                        }

                        base.OnAllFinished();
                    });
                    queueProcessingTask.Start();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override void Pulse()
        {
            Action? action = null;
            if (highPriorityActionsQueue.Count > 0)
                action = highPriorityActionsQueue.Dequeue();
            else if (mediumPriorityActionsQueue.Count > 0)
                action = mediumPriorityActionsQueue.Dequeue();
            else if (lowPriorityActionsQueue.Count > 0)
                action = lowPriorityActionsQueue.Dequeue();
            if (action != null)
            {
                base.OnActionProcessing(action);
                action.Invoke();
                //Thread.Sleep(1000);
                base.OnActionFinished(action);
            }
        }

    }
}

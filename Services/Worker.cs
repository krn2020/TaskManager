using TaskManager.Models;

namespace TaskManager.Services
{
    public class Worker
    {
        private readonly int _id;
        private readonly TaskQueue _queue;
        private readonly HistoryService _history;
        private readonly TaskDispatcher _dispatcher;
        private Thread? _thread;
        private volatile bool _shouldStop;

        public Worker(int id, TaskQueue queue, HistoryService history, TaskDispatcher dispatcher)
        {
            _id = id;
            _queue = queue;
            _history = history;
            _dispatcher = dispatcher;
        }

        public void Start()
        {
            _shouldStop = false;
            _thread = new Thread(Run);
            _thread.Start();
        }

        public void Stop()
        {
            _shouldStop = true;
            _thread?.Join(TimeSpan.FromSeconds(10));
        }

        private async void Run()
        {
            while (!_shouldStop)
            {
                if (!_dispatcher.IsRunning)
                {
                    if (_queue.Count == 0) break;
                    await Task.Delay(500);
                    continue;
                }

                if (_queue.TryDequeue(out WorkTask task, 500))
                {
                    _dispatcher.RegisterRunningTask(_id, task);
                    task.Start();
                    bool completed = await task.ExecuteWithGifAsync(_id, _dispatcher.GlobalCancellationToken);
                    if (completed)
                    {
                        task.Complete();
                        _history.Add(task);
                    }
                    else
                    {
                        _queue.Enqueue(task);
                    }
                    _dispatcher.UnregisterRunningTask(_id);
                }
            }
        }
    }
}
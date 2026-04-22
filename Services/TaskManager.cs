using TaskManager.Models;

namespace TaskManager.Services
{
    public class TaskManager
    {
        private readonly TaskQueue _queue = new();
        private readonly HistoryService _history = new();
        private readonly List<Worker> _workers = new();
        private readonly Dictionary<int, WorkTask> _runningTasks = new(); // workerId -> task
        private readonly object _lock = new();
        private bool _isRunning = false;
        private int _nextTaskId = 1;

        public bool IsRunning
        {
            get { lock (_lock) { return _isRunning; } }
        }

        public void Start(int workerCount)
        {
            lock (_lock)
            {
                if (_isRunning) return;

                // Создаём и запускаем воркеров, если их ещё нет
                if (_workers.Count == 0)
                {
                    for (int i = 0; i < workerCount; i++)
                    {
                        var worker = new Worker(i, _queue, _history, this);
                        _workers.Add(worker);
                    }
                }

                foreach (var worker in _workers)
                    worker.Start();

                _isRunning = true;
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                if (!_isRunning) return;
                _isRunning = false;
            }

            // Останавливаем всех воркеров (дожидаемся завершения текущих задач)
            foreach (var worker in _workers)
                worker.Stop();
        }

        public void AddTask(string name, int priority)
        {
            var task = new WorkTask(_nextTaskId++, name, priority);
            _queue.Enqueue(task);
        }

        public void RegisterRunningTask(int workerId, WorkTask task)
        {
            lock (_lock)
            {
                _runningTasks[workerId] = task;
            }
        }

        public void UnregisterRunningTask(int workerId)
        {
            lock (_lock)
            {
                _runningTasks.Remove(workerId);
            }
        }

        public (List<WorkTask> queue, Dictionary<int, WorkTask> running) GetStateSnapshot()
        {
            var queueSnapshot = _queue.GetSnapshot();
            Dictionary<int, WorkTask> runningCopy;
            lock (_lock)
            {
                runningCopy = new Dictionary<int, WorkTask>(_runningTasks);
            }
            return (queueSnapshot, runningCopy);
        }

        public IReadOnlyList<WorkTask> GetHistory()
        {
            return _history.GetAll();
        }
    }
}

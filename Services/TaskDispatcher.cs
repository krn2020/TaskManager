using TaskManager.Models;
namespace TaskManager.Services
{
    public class TaskDispatcher
    {
        private readonly TaskQueue _queue = new();
        private readonly HistoryService _history = new();
        private readonly List<Worker> _workers = new();
        private readonly Dictionary<int, WorkTask> _runningTasks = new();
        private readonly object _lock = new();
        private bool _isRunning = false;
        private int _nextTaskId = 1;

        public bool IsRunning { get { lock (_lock) { return _isRunning; } } }
        public event Action? StateChanged;
        public void Start(int workerCount)
        {
            lock (_lock)
            {
                if (_isRunning) return;
                if (_workers.Count == 0)
                {
                    for (int i = 0; i < workerCount; i++)
                        _workers.Add(new Worker(i, _queue, _history, this));
                }
                foreach (var w in _workers) w.Start();
                _isRunning = true;
            }
        }

        public void Stop()
        {
            lock (_lock) { _isRunning = false; }
            foreach (var w in _workers) w.Stop();
        }

        public void AddTask(string name, int priority)
        {
            var task = new WorkTask(_nextTaskId++, name, priority);
            _queue.Enqueue(task);
            StateChanged?.Invoke();
        }

        public void RegisterRunningTask(int workerId, WorkTask task)
        {
            lock (_lock) { _runningTasks[workerId] = task; }
            StateChanged?.Invoke();
        }

        public void UnregisterRunningTask(int workerId)
        {
            lock (_lock) { _runningTasks.Remove(workerId); }
            StateChanged?.Invoke();
        }

        public (List<WorkTask> queue, Dictionary<int, WorkTask> running) GetStateSnapshot()
        {
            var q = _queue.GetSnapshot();
            Dictionary<int, WorkTask> r;
            lock (_lock) { r = new Dictionary<int, WorkTask>(_runningTasks); }
            return (q, r);
        }

        public IReadOnlyList<WorkTask> GetHistory() => _history.GetAll();
    }
}
using TaskManager.Models;

namespace TaskManager.Services
{
    public class TaskQueue
    {
        private readonly PriorityQueue<WorkTask, (int Priority, long Sequence)> _queue = new();
        private long _sequence = 0;
        private readonly object _lock = new();

        public void Enqueue(WorkTask task)
        {
            lock (_lock)
            {
                _queue.Enqueue(task, (task.Priority, _sequence++));
            }
        }

        public bool TryDequeue(out WorkTask task, int timeoutMilliseconds = Timeout.Infinite)
        {
            // Используем таймаут для ожидания, чтобы можно было периодически проверять флаг остановки
            const int checkInterval = 200;
            int elapsed = 0;
            while (true)
            {
                lock (_lock)
                {
                    if (_queue.Count > 0)
                    {
                        task = _queue.Dequeue();
                        return true;
                    }
                }

                if (timeoutMilliseconds != Timeout.Infinite && elapsed >= timeoutMilliseconds)
                {
                    task = null;
                    return false;
                }

                Thread.Sleep(checkInterval);
                elapsed += checkInterval;
            }
        }

        public List<WorkTask> GetSnapshot()
        {
            lock (_lock)
            {
                // PriorityQueue не предоставляет прямой доступ к элементам, поэтому создаём копию
                var temp = new PriorityQueue<WorkTask, (int, long)>();
                var list = new List<WorkTask>();
                while (_queue.Count > 0)
                {
                    var task = _queue.Dequeue();
                    list.Add(task);
                    temp.Enqueue(task, (task.Priority, _sequence));
                }
                while (temp.Count > 0)
                {
                    var task = temp.Dequeue();
                    _queue.Enqueue(task, (task.Priority, _sequence));
                }
                return list.OrderBy(t => t.Priority).ThenBy(t => t.AddedTime).ToList();
            }
        }

        public int Count
        {
            get
            {
                lock (_lock) { return _queue.Count; }
            }
        }
    }
}

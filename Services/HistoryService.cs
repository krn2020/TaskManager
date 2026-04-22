
using TaskManager.Models;

namespace TaskManager.Services
{
    public class HistoryService
    {
        private readonly List<WorkTask> _completedTasks = new();
        private readonly object _lock = new();

        public void Add(WorkTask task)
        {
            lock (_lock)
            {
                _completedTasks.Add(task);
            }
        }

        public IReadOnlyList<WorkTask> GetAll()
        {
            lock (_lock)
            {
                return _completedTasks.ToList();
            }
        }
    }
}

namespace TaskManager.Models
{
    public class WorkTask
    {
        private readonly object _lock = new object();

        public int Id { get; }
        public string Name { get; }
        public int Priority { get; }
        public TaskStatus Status { get; private set; }
        public DateTime AddedTime { get; }
        public DateTime? StartTime { get; private set; }
        public DateTime? EndTime { get; private set; }
        public double Duration => (EndTime - StartTime)?.TotalSeconds ?? 0;

        public WorkTask(int id, string name, int priority)
        {
            Id = id;
            Name = name;
            Priority = priority;
            Status = TaskStatus.Pending;
            AddedTime = DateTime.Now;
        }

        public void Start()
        {
            lock (_lock)
            {
                if (Status == TaskStatus.Pending)
                {
                    Status = TaskStatus.Running;
                    StartTime = DateTime.Now;
                }
            }
        }

        public void Complete()
        {
            lock (_lock)
            {
                if (Status == TaskStatus.Running)
                {
                    Status = TaskStatus.Completed;
                    EndTime = DateTime.Now;
                }
            }
        }

        public void Execute()
        {
            Thread.Sleep(5000);
        }
    }
}

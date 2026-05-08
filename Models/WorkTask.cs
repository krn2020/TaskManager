using Avalonia.Controls;
using Avalonia.Threading;
using TaskManager.Services;
using TaskManager.Views;

namespace TaskManager.Models
{
    public class WorkTask
    {
        private readonly object _lock = new();
        public int Id { get; }
        public string Name { get; }
        public int Priority { get; }
        public TaskStatus Status { get; private set; }
        public DateTime AddedTime { get; }
        public DateTime? StartTime { get; private set; }
        public DateTime? EndTime { get; private set; }
        public double Duration => (EndTime - StartTime)?.TotalSeconds ?? 0;
        public Window? WorkerWindow { get; set; }
        public string? CurrentGifUrl { get; set; }
        public WorkTask(int id, string name, int priority)
        {
            Id = id;
            Name = name;
            Priority = priority;
            Status = TaskStatus.Ожидает;
            AddedTime = DateTime.Now;
        }

        public void Start()
        {
            lock (_lock)
            {
                if (Status == TaskStatus.Ожидает)
                {
                    Status = TaskStatus.Выполняется;
                    StartTime = DateTime.Now;
                }
            }
        }

        public void Complete()
        {
            lock (_lock)
            {
                if (Status == TaskStatus.Выполняется)
                {
                    Status = TaskStatus.Завершена;
                    EndTime = DateTime.Now;
                }
            }
        }

        public async Task ExecuteWithGifAsync(WorkTask task, int workerId)
        {
            var random = new Random();
            var durationInSeconds = random.Next(10, 61);
            task.CurrentGifUrl = await GetRandomGifUrlAsync();
    
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                task.WorkerWindow = new GifWindow(workerId.ToString(), task.CurrentGifUrl);
                task.WorkerWindow.Show();
            });
    
            await Task.Delay(durationInSeconds * 1000);
    
            await Dispatcher.UIThread.InvokeAsync(() => task.WorkerWindow?.Close());
        }
        
        private static readonly GiphyService _giphyService = new GiphyService("mBdJrrGnIFRWKZb82oVsvVRXQ8QSWGCa");

        private async Task<string> GetRandomGifUrlAsync()
        {
            return await _giphyService.GetRandomGifUrlAsync();
        }
    }
}
using Avalonia.Controls;
using Avalonia.Threading;
using TaskManager.Services;
using TaskManager.Views;

namespace TaskManager.Models
{
    public class WorkTask
    {
        private readonly object _lock = new();
        private CancellationTokenSource? _cts;
        private Window? _workerWindow;
        private static readonly GiphyService _giphyService = new GiphyService("mBdJrrGnIFRWKZb82oVsvVRXQ8QSWGCa");

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
        public int InterruptCount { get; private set; }

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
        
        public async Task CancelAsync()
        {
            lock (_lock)
            {
                if (Status != TaskStatus.Выполняется) return;
                Status = TaskStatus.Ожидает;
                InterruptCount++;          
                _cts?.Cancel();  
            }
            
            if (_workerWindow != null)
            {
                await Dispatcher.UIThread.InvokeAsync(() => _workerWindow.Close());
                _workerWindow = null;
            }
        }

        public async Task<bool> ExecuteWithGifAsync(int workerId, CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var random = new Random();
            var durationInSeconds = random.Next(10, 61);
            var gifUrl = await GetRandomGifUrlAsync();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _workerWindow = new GifWindow(workerId.ToString(), gifUrl);
                _workerWindow.Show();
                _workerWindow.Closed += (s, e) => _cts?.Cancel();
            });

            try
            {
                await Task.Delay(durationInSeconds * 1000, _cts.Token);
                return true; 
            }
            catch (OperationCanceledException)
            {
                return false; 
            }
            finally
            {
                await Dispatcher.UIThread.InvokeAsync(() => _workerWindow?.Close());
                _workerWindow = null;
                _cts.Dispose();
                _cts = null;
            }
        }


        
        private async Task<string> GetRandomGifUrlAsync()
        {
            return await _giphyService.GetRandomGifUrlAsync();
        }
    }
}
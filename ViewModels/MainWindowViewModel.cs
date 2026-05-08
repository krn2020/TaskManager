using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Threading;
using ReactiveUI;
using TaskManager.Models;
using TaskManager.Services;
using TaskManager.Configuration;

namespace TaskManager.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly TaskDispatcher _dispatcher;
        private readonly int _workerCount;
        private string _newTaskName = "";
        private int _newTaskPriority = 5;
        private bool _isSystemRunning;
        private string _statusText = "Остановлена";

        public ObservableCollection<WorkTask> QueueTasks { get; } = new();
        public ObservableDictionary<int, WorkTask> RunningTasks { get; } = new();
        public ObservableCollection<WorkTask> HistoryTasks { get; } = new();

        public string NewTaskName { get => _newTaskName; set => this.RaiseAndSetIfChanged(ref _newTaskName, value); }
        public int NewTaskPriority { get => _newTaskPriority; set => this.RaiseAndSetIfChanged(ref _newTaskPriority, value); }
        public bool IsSystemRunning { get => _isSystemRunning; set => this.RaiseAndSetIfChanged(ref _isSystemRunning, value); }
        public string StatusText { get => _statusText; set => this.RaiseAndSetIfChanged(ref _statusText, value); }

        public ReactiveCommand<Unit, Unit> StartCommand { get; }
        public ReactiveCommand<Unit, Unit> StopCommand { get; }
        public ReactiveCommand<Unit, Unit> AddTaskCommand { get; }
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowHistoryCommand { get; }

        public MainWindowViewModel()
        {
            _workerCount = ConfigLoader.LoadWorkerCount("config.xml");
            _dispatcher = new TaskDispatcher();

            _dispatcher.StateChanged += OnStateChanged;

            StartCommand = ReactiveCommand.Create(StartSystem);
            StopCommand = ReactiveCommand.Create(StopSystem);
            AddTaskCommand = ReactiveCommand.Create(AddTask);
            RefreshCommand = ReactiveCommand.Create(RefreshState);
            ShowHistoryCommand = ReactiveCommand.Create(ShowHistory);

            RefreshState();
        }

        private void OnStateChanged()
        {
            Dispatcher.UIThread.Invoke(RefreshState);
        }

        private void StartSystem() { _dispatcher.Start(_workerCount); IsSystemRunning = true; StatusText = "Работает"; RefreshState(); }
        private void StopSystem() { _dispatcher.Stop(); IsSystemRunning = false; StatusText = "Остановлена"; RefreshState(); }
        private void AddTask()
        {
            Console.WriteLine($"[UI] AddTask called. Name: {NewTaskName}, Priority: {NewTaskPriority}");
            if (string.IsNullOrWhiteSpace(NewTaskName)) 
            {
                Console.WriteLine("[UI] Task name is empty, ignoring.");
                return;
            }
            _dispatcher.AddTask(NewTaskName, NewTaskPriority);
            Console.WriteLine("[UI] Task added to dispatcher, calling RefreshState");
            NewTaskName = "";
            RefreshState();
        }

        private void RefreshState()
        {
            Console.WriteLine("[UI] RefreshState called");
            var snapshot = _dispatcher.GetStateSnapshot();
            var queue = snapshot.queue;
            var running = snapshot.running;
            Console.WriteLine($"[UI] Queue count: {queue.Count}, Running count: {running.Count}");
    
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Console.WriteLine("[UI] Updating UI collections...");
                QueueTasks.Clear();
                foreach (var task in queue) QueueTasks.Add(task);
                RunningTasks.Clear();
                foreach (var kvp in running) RunningTasks.Add(kvp.Key, kvp.Value);
                Console.WriteLine($"[UI] UI updated. QueueTasks count: {QueueTasks.Count}, RunningTasks count: {RunningTasks.Count}");
            });
        }
        private void ShowHistory()
        {
            var history = _dispatcher.GetHistory();
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                HistoryTasks.Clear();
                foreach (var task in history) HistoryTasks.Add(task);
            });
        }
    }

    public class ObservableDictionary<TKey, TValue> : ObservableCollection<KeyValuePair<TKey, TValue>>
    {
        public void Add(TKey key, TValue value) => Add(new KeyValuePair<TKey, TValue>(key, value));
        public void Remove(TKey key)
        {
            var item = this.FirstOrDefault(kvp => Equals(kvp.Key, key));
            if (!Equals(item.Key, null)) Remove(item);
        }
        public new void ClearItems() => base.ClearItems();
    }
}
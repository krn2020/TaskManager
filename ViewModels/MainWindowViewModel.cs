using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using TaskManager.Configuration;
using TaskManager.Models;

public class MainWindowViewModel : ReactiveObject
{
    private readonly TaskManager _taskManager;
    private readonly int _workerCount;
    private string _newTaskName = "";
    private int _newTaskPriority = 5;
    private bool _isSystemRunning;
    private string _statusText = "Остановлена";

    public ObservableCollection<WorkTask> QueueTasks { get; } = new();
    public ObservableDictionary<int, WorkTask> RunningTasks { get; } = new(); 
    public ObservableCollection<WorkTask> HistoryTasks { get; } = new();

    public string NewTaskName
    {
        get => _newTaskName;
        set => this.RaiseAndSetIfChanged(ref _newTaskName, value);
    }

    public int NewTaskPriority
    {
        get => _newTaskPriority;
        set => this.RaiseAndSetIfChanged(ref _newTaskPriority, value);
    }

    public bool IsSystemRunning
    {
        get => _isSystemRunning;
        set => this.RaiseAndSetIfChanged(ref _isSystemRunning, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => this.RaiseAndSetIfChanged(ref _statusText, value);
    }

    public ReactiveCommand<Unit, Unit> StartCommand { get; }
    public ReactiveCommand<Unit, Unit> StopCommand { get; }
    public ReactiveCommand<Unit, Unit> AddTaskCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowHistoryCommand { get; }

    public MainWindowViewModel()
    {
        int workerCount = ConfigLoader.LoadWorkerCount("config.xml");
        _workerCount = workerCount;
        _taskManager = new TaskManager();

        StartCommand = ReactiveCommand.Create(StartSystem);
        StopCommand = ReactiveCommand.Create(StopSystem);
        AddTaskCommand = ReactiveCommand.Create(AddTask);
        RefreshCommand = ReactiveCommand.Create(RefreshState);
        ShowHistoryCommand = ReactiveCommand.Create(ShowHistory);

        var timer = new Timer(_ => RefreshState(), null, 0, 500);
    }

    private void StartSystem()
    {
        _taskManager.Start(_workerCount);
        IsSystemRunning = true;
        StatusText = "Работает";
        RefreshState();
    }

    private void StopSystem()
    {
        _taskManager.Stop();
        IsSystemRunning = false;
        StatusText = "Остановлена";
        RefreshState();
    }

    private void AddTask()
    {
        if (string.IsNullOrWhiteSpace(NewTaskName))
            return;
        _taskManager.AddTask(NewTaskName, NewTaskPriority);
        NewTaskName = "";
        RefreshState();
    }

    private void RefreshState()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var (queue, running) = _taskManager.GetStateSnapshot();
            QueueTasks.Clear();
            foreach (var task in queue)
                QueueTasks.Add(task);

            RunningTasks.Clear();
            foreach (var kvp in running)
                RunningTasks[kvp.Key] = kvp.Value;
        });
    }

    private void ShowHistory()
    {
        var history = _taskManager.GetHistory();
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            HistoryTasks.Clear();
            foreach (var task in history)
                HistoryTasks.Add(task);
        });
    }
}

public class ObservableDictionary<TKey, TValue> : ObservableCollection<KeyValuePair<TKey, TValue>>
{
    public void Add(TKey key, TValue value) => Add(new KeyValuePair<TKey, TValue>(key, value));
    public void ClearItems() => base.ClearItems();
    public void Remove(TKey key)
    {
        var item = this.FirstOrDefault(kvp => kvp.Key.Equals(key));
        if (item.Key != null) Remove(item);
    }
    public new void Add(KeyValuePair<TKey, TValue> item) => base.Add(item);
}
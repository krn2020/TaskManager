
using TaskManager.Models;

namespace TaskManager.Services
{
    public class Worker
    {
        private readonly int _id;
        private readonly TaskQueue _queue;
        private readonly HistoryService _history;
        private readonly TaskManager _manager;
        private Thread _thread;
        private volatile bool _shouldStop;

        public Worker(int id, TaskQueue queue, HistoryService history, TaskManager manager)
        {
            _id = id;
            _queue = queue;
            _history = history;
            _manager = manager;
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

        private void Run()
        {
            while (!_shouldStop)
            {
                // Проверяем, запущена ли система (разрешено ли брать новые задачи)
                if (!_manager.IsRunning)
                {
                    // Если система остановлена и очередь пуста – выходим
                    if (_queue.Count == 0)
                        break;
                    // Иначе даём время на завершение уже взятых задач (текущая задача уже выполняется)
                    Thread.Sleep(500);
                    continue;
                }

                // Ожидаем задачу с таймаутом, чтобы периодически проверять _shouldStop
                if (_queue.TryDequeue(out WorkTask task, 500))
                {
                    // Регистрируем задачу как выполняемую
                    _manager.RegisterRunningTask(_id, task);
                    task.Start();

                    // Выполняем задачу
                    task.Execute();

                    // Завершаем задачу
                    task.Complete();
                    _history.Add(task);
                    _manager.UnregisterRunningTask(_id);
                }
            }
        }
    }
}

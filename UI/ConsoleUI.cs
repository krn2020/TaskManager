namespace TaskManager.UI
{
    public class ConsoleUI
    {
        private readonly Services.TaskDispatcher _taskManager;
        private readonly int _workerCount;

        public ConsoleUI(Services.TaskDispatcher taskManager, int workerCount)
        {
            _taskManager = taskManager;
            _workerCount = workerCount;
        }

        public void Run()
        {
            Console.WriteLine("Система обработки задач с приоритетами");
            Console.WriteLine("Команды: start, stop, add, list, history, exit");
            Console.WriteLine("---------------------------------------------");

            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine()?.Trim().ToLower();
                switch (input)
                {
                    case "start":
                        _taskManager.Start(_workerCount);
                        Console.WriteLine("Система запущена.");
                        break;
                    case "stop":
                        _taskManager.Stop();
                        Console.WriteLine("Система остановлена (текущие задачи будут завершены).");
                        break;
                    case "add":
                        AddTaskDialog();
                        break;
                    case "list":
                        ShowState();
                        break;
                    case "history":
                        ShowHistory();
                        break;
                    case "exit":
                        _taskManager.Stop();
                        Console.WriteLine("Выход...");
                        return;
                    default:
                        Console.WriteLine("Неизвестная команда.");
                        break;
                }
            }
        }

        private void AddTaskDialog()
        {
            Console.Write("Имя задачи: ");
            string name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Имя не может быть пустым.");
                return;
            }

            Console.Write("Приоритет (1 - самый высокий, 10 - самый низкий): ");
            if (!int.TryParse(Console.ReadLine(), out int priority) || priority < 1 || priority > 10)
            {
                Console.WriteLine("Приоритет должен быть числом от 1 до 10.");
                return;
            }

            _taskManager.AddTask(name, priority);
            Console.WriteLine($"Задача '{name}' с приоритетом {priority} добавлена.");
        }

        private void ShowState()
        {
            var (queue, running) = _taskManager.GetStateSnapshot();

            Console.WriteLine("\n=== ТЕКУЩЕЕ СОСТОЯНИЕ ===");
            Console.WriteLine($"Всего задач в очереди: {queue.Count}");
            Console.WriteLine($"Выполняется задач: {running.Count}\n");

            // Выполняющиеся задачи
            if (running.Any())
            {
                Console.WriteLine("--- ВЫПОЛНЯЮТСЯ (воркер -> задача) ---");
                foreach (var kvp in running)
                {
                    var task = kvp.Value;
                    Console.WriteLine($"Воркер {kvp.Key}: Задача #{task.Id} \"{task.Name}\" (приор.{task.Priority})");
                }
                Console.WriteLine();
            }

            // Очередь задач (в порядке приоритета)
            if (queue.Any())
            {
                Console.WriteLine("--- ОЧЕРЕДЬ (по приоритету) ---");
                Console.WriteLine("ID | Имя                | Приор. | Статус   | Время добавления");
                foreach (var task in queue)
                {
                    Console.WriteLine($"{task.Id,3} | {task.Name,-18} | {task.Priority,6} | {task.Status,-8} | {task.AddedTime:HH:mm:ss}");
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Очередь пуста.\n");
            }
        }

        private void ShowHistory()
        {
            var history = _taskManager.GetHistory();
            Console.WriteLine("\n=== ИСТОРИЯ ВЫПОЛНЕННЫХ ЗАДАЧ ===");
            if (!history.Any())
            {
                Console.WriteLine("Нет выполненных задач.");
                return;
            }

            Console.WriteLine("ID | Имя                | Приор. | Время добавления | Старт       | Окончание   | Длительность (сек)");
            foreach (var task in history)
            {
                var duration = task.EndTime.HasValue && task.StartTime.HasValue
                    ? (task.EndTime.Value - task.StartTime.Value).TotalSeconds
                    : 0;
                Console.WriteLine($"{task.Id,3} | {task.Name,-18} | {task.Priority,6} | {task.AddedTime:HH:mm:ss} | {task.StartTime?.ToString("HH:mm:ss") ?? "N/A"} | {task.EndTime?.ToString("HH:mm:ss") ?? "N/A"} | {duration,5:F1}");
            }
            Console.WriteLine();
        }
    }
}

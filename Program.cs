// Program.cs
using TaskManager.Configuration;
using TaskManager.UI;

class Program
{
    static void Main(string[] args)
    {
        // Загружаем конфигурацию
        int workerCount = ConfigLoader.LoadWorkerCount("config.xml");
        Console.WriteLine($"Конфигурация: {workerCount} воркер(ов)");

        // Создаём диспетчер и UI
        var taskManager = new TaskManager.Services.TaskManager();
        var ui = new ConsoleUI(taskManager, workerCount);

        // Запускаем интерфейс
        ui.Run();
    }
}
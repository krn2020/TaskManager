using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Reflection.PortableExecutable;

namespace TaskProcessorApp;

class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
}

//using TaskManager.Configuration;
//using TaskManager.UI;

//class Program
//{
//    static void Main(string[] args)
//    {
//        int workerCount = ConfigLoader.LoadWorkerCount("config.xml");
//        Console.WriteLine($"Конфигурация: {workerCount} воркер(ов)");

//        var taskManager = new TaskManager.Services.TaskManager();
//        var ui = new ConsoleUI(taskManager, workerCount);

//        ui.Run();
//    }
//}
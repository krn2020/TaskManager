using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TaskManager.Services;

namespace TaskManager.Views;
public partial class GifWindow : Window
{
    public GifWindow(string workerId, string gifUrl)
    {
        InitializeComponent();
        Title = $"Воркер {workerId}";
        DataContext = new { GifUrl = gifUrl };

        Width = WindowPositionManager.WindowWidth;
        Height = WindowPositionManager.WindowHeight;
        WindowStartupLocation = WindowStartupLocation.Manual;

        var position = WindowPositionManager.RequestNextPosition();
        Position = position;
        WindowPositionManager.RegisterWindow(this, position);
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
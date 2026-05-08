using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TaskManager.Views;
public partial class GifWindow : Window
{
    private static int _windowCounter = 0;
    private static readonly int OffsetStep = 40;

    public GifWindow(string workerId, string gifUrl)
    {
        InitializeComponent();
        Title = $"Воркер {workerId}";
        DataContext = new { GifUrl = gifUrl };
            
        var offset = _windowCounter++ * OffsetStep;
        Position = new PixelPoint(100 + offset, 100 + offset);
        Width = 400;
        Height = 400;
        WindowStartupLocation = WindowStartupLocation.Manual;
        
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
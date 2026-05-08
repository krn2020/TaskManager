using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TaskManager.ViewModels;

namespace TaskManager.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
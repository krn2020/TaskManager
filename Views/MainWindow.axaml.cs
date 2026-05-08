using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TaskManager.Services;
using TaskManager.ViewModels;

namespace TaskManager.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        var tabControl = this.FindControl<TabControl>("MainTabControl");
        if (tabControl != null)
            tabControl.SelectionChanged += OnTabSelectionChanged;
    }

    private void OnTabSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is TabItem selectedTab)
        {
            if (selectedTab.Header?.ToString()?.Contains("История") == true)
            {
                ((MainWindowViewModel)DataContext).OnHistoryTabSelected();
            }
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        WindowPositionManager.UpdateWorkingArea(this);
    }
}
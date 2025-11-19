using Avalonia.Controls;
using RazerController.ViewModels;

namespace RazerController.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Track window activation state for polling
        Activated += (s, e) =>
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.IsWindowActive = true;
            }
        };
        
        Deactivated += (s, e) =>
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.IsWindowActive = false;
            }
        };
    }
}
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using RazerController.ViewModels;
using System;
using System.Threading.Tasks;

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
    
    private async void ColorPreview_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm) return;
        
        var dialog = new ColorPickerDialog
        {
            RedValue = vm.RedValue,
            GreenValue = vm.GreenValue,
            BlueValue = vm.BlueValue
        };
        
        var result = await dialog.ShowDialog<bool>(this);
        
        if (result)
        {
            vm.RedValue = dialog.RedValue;
            vm.GreenValue = dialog.GreenValue;
            vm.BlueValue = dialog.BlueValue;
        }
    }
}
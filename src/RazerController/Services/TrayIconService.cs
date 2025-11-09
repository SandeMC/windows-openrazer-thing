using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using RazerController.Views;

namespace RazerController.Services;

public class TrayIconService
{
    private TrayIcon? _trayIcon;
    private readonly IClassicDesktopStyleApplicationLifetime? _desktop;

    public TrayIconService()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _desktop = desktop;
        }
    }

    public void Initialize()
    {
        if (_desktop == null) return;

        _trayIcon = new TrayIcon
        {
            Icon = new WindowIcon("/Assets/avalonia-logo.ico"),
            ToolTipText = "Razer Controller"
        };

        var menu = new NativeMenu();

        var showItem = new NativeMenuItem("Show");
        showItem.Click += (s, e) => ShowMainWindow();
        menu.Items.Add(showItem);

        menu.Items.Add(new NativeMenuItemSeparator());

        var exitItem = new NativeMenuItem("Exit");
        exitItem.Click += (s, e) => _desktop.Shutdown();
        menu.Items.Add(exitItem);

        _trayIcon.Menu = menu;
        _trayIcon.Clicked += (s, e) => ShowMainWindow();

        _trayIcon.IsVisible = true;
    }

    private void ShowMainWindow()
    {
        if (_desktop?.MainWindow != null)
        {
            _desktop.MainWindow.Show();
            _desktop.MainWindow.WindowState = WindowState.Normal;
            _desktop.MainWindow.Activate();
        }
    }

    public void Dispose()
    {
        if (_trayIcon != null)
        {
            _trayIcon.IsVisible = false;
            _trayIcon.Dispose();
        }
    }
}

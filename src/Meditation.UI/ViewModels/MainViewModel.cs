using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;
using Meditation.UI.Windows;
using System;

namespace Meditation.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Meditation!";

    [RelayCommand]
    public void DisplayAttachProcessWindow()
    {
        var applicationLifetime = (IClassicDesktopStyleApplicationLifetime?)(Application.Current?.ApplicationLifetime);
        var ownerWindow = applicationLifetime?.MainWindow
                          ?? throw new InvalidOperationException("Cannot open modal, as parent window could not be resolved.");

        var window = new AttachToProcessWindow();
        window.ShowDialog(ownerWindow);
    }
}

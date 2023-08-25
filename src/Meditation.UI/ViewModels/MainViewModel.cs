using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Meditation.UI.Windows;
using System;
using System.Threading.Tasks;

namespace Meditation.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Meditation!";
    private readonly IAvaloniaStorageProvider _storageProvider;
    private readonly IUserInterfaceEventsRaiser _eventsRaiser;

    public MainViewModel(IAvaloniaStorageProvider storageProvider, IUserInterfaceEventsRaiser eventsRaiser)
    {
        _storageProvider = storageProvider;
        _eventsRaiser = eventsRaiser;
    }

    [RelayCommand]
    public void DisplayAttachProcessWindow()
    {
        var applicationLifetime = (IClassicDesktopStyleApplicationLifetime?)(Application.Current?.ApplicationLifetime);
        var ownerWindow = applicationLifetime?.MainWindow
                          ?? throw new InvalidOperationException("Cannot open modal, as parent window could not be resolved.");

        var window = new AttachToProcessWindow();
        window.ShowDialog(ownerWindow);
    }

    [RelayCommand]
    public async Task OpenAssembly()
    {
        var storage = _storageProvider.GetStorageProvider();
        
        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Assembly",
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("DLLs") { Patterns = new[] { "*.dll" } } }
        });

        if (files.Count != 0)
            _eventsRaiser.RaiseAssemblyLoadRequested(files[0].Path.AbsolutePath);
    }
}

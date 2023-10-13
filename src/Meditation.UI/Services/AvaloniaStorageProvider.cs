using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System.ComponentModel;

namespace Meditation.UI.Services
{
    internal class AvaloniaStorageProvider : IAvaloniaStorageProvider
    {
        public IStorageProvider GetStorageProvider()
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                throw new InvalidAsynchronousStateException("Could not resolve main window");

            if (desktop?.MainWindow?.StorageProvider is not { } storageProvider)
                throw new InvalidAsynchronousStateException("Could not resolve main window's storage provider");

            return storageProvider;
        }
    }
}

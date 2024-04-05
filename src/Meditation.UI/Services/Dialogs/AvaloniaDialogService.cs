using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace Meditation.UI.Services.Dialogs
{
    internal class AvaloniaDialogService : IAvaloniaDialogService
    {
        private readonly IServiceProvider _serviceProvider;

        public AvaloniaDialogService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public DialogLifetime CreateDialog<TWindow>() where TWindow : Window
        {
            try
            {
                var owner = GetRootWindow();
                return new DialogLifetime(typeof(TWindow), owner, _serviceProvider);
            }
            catch (Exception ex)
            {
                // FIXME [#16]: add logging
                throw new InvalidOperationException("Could not open dialog.", ex);
            }
        }

        public async Task<IStorageFile?> ShowSaveFileDialog(string title, string extension, string folder, string fileName)
        {
            try
            {
                var owner = GetRootWindow();
                return await owner.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
                {
                    Title = title,
                    DefaultExtension = extension,
                    ShowOverwritePrompt = true,
                    SuggestedFileName = fileName,
                    SuggestedStartLocation = await owner.StorageProvider.TryGetFolderFromPathAsync(folder)
                });
            }
            catch (Exception ex)
            {
                // FIXME [#16]: add logging
                throw new InvalidOperationException("Could not open dialog.", ex);
            }
        }

        private static Window GetRootWindow()
        {
            if (Application.Current is not { } application)
                throw new InvalidOperationException("Cannot obtain application instance.");

            if (application.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktopLifetime)
                throw new InvalidOperationException($"Application lifetime is not {nameof(IClassicDesktopStyleApplicationLifetime)}.");

            if (desktopLifetime.MainWindow is not { } rootWindow)
                throw new InvalidOperationException("Parent window could not be resolved.");
            
            return rootWindow;
        }
    }
}

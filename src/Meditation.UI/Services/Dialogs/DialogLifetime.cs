using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Meditation.UI.Services.Dialogs
{
    public class DialogLifetime : IDisposable
    {
        private readonly Window _ownerDialog;
        private readonly Window _dialogWindow;
        private TaskCompletionSource? _taskCompletionSource;
        private bool _isShown;
        private bool _isDisposed;

        public object? DataContext => _dialogWindow?.DataContext;

        public DialogLifetime(Type dialogType, Window ownerDialog, IServiceProvider serviceProvider)
        {
            var dialogWindow = CreateDialog(dialogType, serviceProvider);
            _dialogWindow = dialogWindow;
            _ownerDialog = ownerDialog;
        }

        public Task Show()
        {
            _dialogWindow.Show(_ownerDialog);
            _taskCompletionSource = new TaskCompletionSource();
            _isShown = true;
            return _taskCompletionSource.Task;
        }

        public void Close()
        {
            if (!_isShown)
                return;

            _dialogWindow.Close();
            _taskCompletionSource?.SetResult();
            _isShown = false;
        }

        public void SetDataContext(object? dataContext)
        {
            _dialogWindow.DataContext = dataContext;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _isShown = false;
            _dialogWindow.Close();
        }

        private static Window CreateDialog(Type dialogType, IServiceProvider serviceProvider)
        {
            if (!typeof(Window).IsAssignableFrom(dialogType))
                throw new ArgumentException($"Supplied type {dialogType} must be a Window");

            if (ActivatorUtilities.CreateInstance(serviceProvider, dialogType) is not Window dialogWindow)
                throw new ArgumentException($"Could not construct dialog {dialogType}");

            return dialogWindow;
        }
    }
}
using System;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Meditation.UI.Services.Dialogs
{
    public class DialogLifetime : IDisposable
    {
        private readonly Window _ownerDialog;
        private readonly Window _dialogWindow;
        private bool _isShown;
        private bool _isDisposed;

        public DialogLifetime(Type dialogType, Window ownerDialog, IServiceProvider serviceProvider)
        {
            if (!typeof(Window).IsAssignableFrom(dialogType))
                throw new ArgumentException($"Supplied type {dialogType} must be a Window");

            if (ActivatorUtilities.CreateInstance(serviceProvider, dialogType) is not Window dialogWindow)
                throw new ArgumentException($"Could not construct dialog {dialogType}");

            _dialogWindow = dialogWindow;
            _ownerDialog = ownerDialog;
        }

        public void Show()
        {
            _dialogWindow.Show(_ownerDialog);
            _isShown = true;
        }

        public void Close()
        {
            if (!_isShown)
                throw new InvalidOperationException("Dialog is not opened");

            _dialogWindow.Close();
            _isShown = false;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _isShown = false;
            _dialogWindow.Close();
        }
    }
}
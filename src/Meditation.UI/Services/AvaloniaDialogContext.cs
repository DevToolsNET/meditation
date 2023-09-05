using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meditation.UI.ViewModels;

namespace Meditation.UI.Services
{
    internal class AvaloniaDialogContext : IAvaloniaDialogsContext
    {
        private readonly Stack<(Window Dialog, Task LifeTime)> _dialogs;
        private readonly object _syncObject;
        private readonly IServiceProvider _serviceProvider;

        public AvaloniaDialogContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _dialogs = new Stack<(Window Dialog, Task LifeTime)>();
            _syncObject = new object();
        }

        public void DisplayDialog<TWindow>() where TWindow : Window
        {
            lock (_syncObject)
            {
                var dialog = ActivatorUtilities.CreateInstance<TWindow>(_serviceProvider);
                var owner = (_dialogs.Count > 0) ? _dialogs.Peek().Dialog : GetRootWindow();
                var lifetime = dialog.ShowDialog(owner);
                _dialogs.Push((dialog, lifetime));
            }
        }

        public Task CloseDialogAsync<TWindow>() where TWindow : Window
        {
            lock (_syncObject)
            {
                if (_dialogs.Count == 0)
                    throw new InvalidOperationException("No dialog is currently open.");

                if (_dialogs.Peek().Dialog.GetType() != typeof(TWindow))
                    throw new InvalidOperationException($"Dialog of type {typeof(TWindow)} is not the top-most one.");

                var (dialog, lifetime) = _dialogs.Pop();
                if (!lifetime.IsCompleted)
                    dialog.Close();

                return lifetime;
            }
        }

        private static Window GetRootWindow()
        {
            var applicationLifetime = (IClassicDesktopStyleApplicationLifetime?)(Application.Current?.ApplicationLifetime);
            var ownerWindow = applicationLifetime?.MainWindow
                              ?? throw new InvalidOperationException("Cannot open modal, as parent window could not be resolved.");
            return ownerWindow;
        }
    }
}

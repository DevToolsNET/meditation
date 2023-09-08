using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Meditation.UI.Services
{
    internal class AvaloniaDialogService : IAvaloniaDialogService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, Window> _dialogs;

        public AvaloniaDialogService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _dialogs = new Dictionary<Type, Window>();
        }

        public void DisplayDialog<TWindow>() where TWindow : Window
        {
            var owner = GetRootWindow();
            var dialog = ActivatorUtilities.CreateInstance<TWindow>(_serviceProvider);
            dialog.Closed += HandleDialogClosed;
            
            lock (_dialogs)
            {
                if (_dialogs.TryGetValue(typeof(TWindow), out var existing))
                    existing.Close();

                _dialogs.Add(dialog.GetType(), dialog);
                dialog.ShowDialog(owner);
            }
        }

        public void CloseDialog<TWindow>() where TWindow : Window
        {
            lock (_dialogs)
            {
                if (_dialogs.TryGetValue(typeof(TWindow), out var existing))
                    existing.Close();
            }
        }

        private void HandleDialogClosed(object? sender, EventArgs args)
        {
            if (sender == null)
                return;

            lock (_dialogs)
            {
                if (_dialogs.Remove(sender.GetType(), out var dialog))
                    dialog.Closed -= HandleDialogClosed;
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

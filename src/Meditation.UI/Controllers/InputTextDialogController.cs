using Meditation.UI.Controllers.Utils;
using Meditation.UI.Services.Dialogs;
using Meditation.UI.ViewModels;
using Meditation.UI.Windows;
using System;
using System.Threading.Tasks;
using MsBox.Avalonia.Enums;

namespace Meditation.UI.Controllers
{
    public class InputTextDialogController
    {
        private readonly IAvaloniaDialogService _dialogService;

        public InputTextDialogController(IAvaloniaDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public async Task<DialogLifetime?> CreateDialog(string message)
        {
            try
            {
                var dialog = _dialogService.CreateDialog<InputTextDialogWindow>();
                dialog.SetDataContext(new InputTextDialogViewModel(message, () => CloseDialog(dialog)));
                return dialog;
            }
            catch (Exception exception)
            {
                await DialogUtilities.ShowUnhandledExceptionMessageBox(exception);
                return null;
            }
        }

        public async Task<InputTextDialogViewModel?> GetDataContext(DialogLifetime dialog)
        {
            if (dialog.DataContext is not InputTextDialogViewModel dataContext)
            {
                await DialogUtilities.ShowUnhandledErrorMessageBox($"Unexpected DataContext of type \"{dialog.DataContext?.GetType().FullName ?? "null"}\".");
                return null;
            }

            return dataContext;
        }

        public Task ShowDialog(DialogLifetime dialog)
        {
            try
            {
                return dialog.Show();
            }
            catch (Exception exception)
            {
                return DialogUtilities.ShowUnhandledExceptionMessageBox(exception);
            }
        }

        public Task CloseDialog(DialogLifetime dialog)
        {
            try
            {
                dialog.Close();
                dialog.Dispose();
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                return DialogUtilities.ShowUnhandledExceptionMessageBox(exception);
            }
        }
    }
}

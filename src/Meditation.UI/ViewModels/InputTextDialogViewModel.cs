using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Meditation.UI.ViewModels
{
    public partial class InputTextDialogViewModel : ViewModelBase
    {
        [ObservableProperty] private string _message;
        [ObservableProperty] private string? _text;
        private readonly Action _closeAction;

        public InputTextDialogViewModel(string message, Action closeAction)
        {
            Message = message;
            _closeAction = closeAction;
        }

        [RelayCommand]
        public void ConfirmInput()
        {
            _closeAction();
        }

        [RelayCommand]
        public void CancelInput()
        {
            Text = null; 
            _closeAction();
        }
    }
}

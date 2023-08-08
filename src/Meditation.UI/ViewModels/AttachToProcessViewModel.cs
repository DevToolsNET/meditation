using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;

namespace Meditation.UI.ViewModels
{
    public partial class AttachToProcessViewModel : ViewModelBase
    {
        [RelayCommand]
        public void AttachProcess()
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(
                title: "Missing feature",
                text: "This functionality is not yet implemented",
                @enum: ButtonEnum.Ok);
            messageBox.ShowAsync();
        }
    }
}

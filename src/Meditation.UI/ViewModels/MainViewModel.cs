using CommunityToolkit.Mvvm.Input;
using Meditation.UI.Windows;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Threading.Tasks;

namespace Meditation.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Meditation!";
    private readonly IAvaloniaDialogService _dialogService;
    private readonly IAttachedProcessProvider _attachedProcessProvider;
    private readonly IAttachedProcessController _attachedProcessController;

    public MainViewModel(IAvaloniaDialogService dialogService, IAttachedProcessProvider attachedProcessProvider, IAttachedProcessController attachedProcessController)
    {
        _dialogService = dialogService;
        _attachedProcessProvider = attachedProcessProvider;
        _attachedProcessController = attachedProcessController;
    }

    [RelayCommand]
    public async Task DisplayAttachProcessWindow()
    {
        if (_attachedProcessProvider.ProcessSnapshot is { } snapshot)
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(
                title: "Cannot attach to multiple processes",
                text: $"To attach another process, detach first from the currently attached process ({snapshot.ProcessId.Value}).",
                @enum: ButtonEnum.Ok);
            await messageBox.ShowAsync();
            return;
        }

        _dialogService.DisplayDialog<AttachToProcessWindow>();
    }

    [RelayCommand]
    public async Task DetachProcess()
    {
        if (_attachedProcessProvider.ProcessSnapshot is { } snapshot)
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(
                title: "Preparing to detach process",
                text: $"Do you want to detach from process ({snapshot.ProcessId.Value})?",
                @enum: ButtonEnum.YesNo);

            if (await messageBox.ShowAsync() == ButtonResult.Yes)
                _attachedProcessController.Detach();
        }
        else
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(
                title: "Nothing to detach",
                text: "Currently, there is no attached process",
                @enum: ButtonEnum.Ok);
            await messageBox.ShowAsync();
        }
    }
}

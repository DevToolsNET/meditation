using CommunityToolkit.Mvvm.Input;
using Meditation.UI.Windows;

namespace Meditation.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Meditation!";
    private readonly IAvaloniaDialogService _dialogService;

    public MainViewModel(IAvaloniaDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    [RelayCommand]
    public void DisplayAttachProcessWindow()
    {
        _dialogService.DisplayDialog<AttachToProcessWindow>();
    }
}

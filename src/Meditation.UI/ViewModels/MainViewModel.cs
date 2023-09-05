using CommunityToolkit.Mvvm.Input;
using Meditation.UI.Windows;

namespace Meditation.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Meditation!";
    private readonly IAvaloniaDialogsContext _dialogContext;

    public MainViewModel(IAvaloniaDialogsContext dialogContext)
    {
        _dialogContext = dialogContext;
    }

    [RelayCommand]
    public void DisplayAttachProcessWindow()
    {
        _dialogContext.DisplayDialog<AttachToProcessWindow>();
    }
}

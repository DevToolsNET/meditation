using Meditation.UI.Controllers;

namespace Meditation.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public AttachToProcessController AttachToProcessController { get; }
    public string Greeting => "Welcome to Meditation!";

    public MainViewModel(AttachToProcessController attachToProcessDialogController)
    {
        AttachToProcessController = attachToProcessDialogController;
    }
}

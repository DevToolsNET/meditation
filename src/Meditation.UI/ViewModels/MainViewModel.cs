using Meditation.UI.Controllers;

namespace Meditation.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public AttachToProcessController AttachToProcessController { get; }
    public DevelopmentEnvironmentController DevelopmentEnvironmentController { get; }

    public MainViewModel(
        AttachToProcessController attachToProcessDialogController, 
        DevelopmentEnvironmentController developmentEnvironmentController)
    {
        AttachToProcessController = attachToProcessDialogController;
        DevelopmentEnvironmentController = developmentEnvironmentController;
    }
}

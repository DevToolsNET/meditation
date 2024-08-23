using CommunityToolkit.Mvvm.ComponentModel;
using Meditation.UI.Controllers;

namespace Meditation.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public AttachToProcessController AttachToProcessController { get; }
    public DevelopmentEnvironmentController DevelopmentEnvironmentController { get; }
    [ObservableProperty] private StatusBarViewModel _statusBarViewModel;
    private readonly IAttachedProcessContext _attachedProcessContext;

    public MainViewModel(
        IAttachedProcessContext attachedProcessContext,
        AttachToProcessController attachToProcessDialogController, 
        DevelopmentEnvironmentController developmentEnvironmentController)
    {
        _attachedProcessContext = attachedProcessContext;
        AttachToProcessController = attachToProcessDialogController;
        DevelopmentEnvironmentController = developmentEnvironmentController;
        StatusBarViewModel = new StatusBarViewModel();
        Initialize();
    }

    private void Initialize()
    {
        const string noProcessAttachedMessage = "No process attached.";
        StatusBarViewModel.SetInformationStatus(noProcessAttachedMessage);
        _attachedProcessContext.ProcessAttached += pid => StatusBarViewModel.SetSuccessStatus($"Attached to process with PID = {pid.Value}.");
        _attachedProcessContext.ProcessAttaching += _ => StatusBarViewModel.SetWarningStatus("Attaching...");
        _attachedProcessContext.ProcessDetached += _ => StatusBarViewModel.SetInformationStatus(noProcessAttachedMessage);
    }
}

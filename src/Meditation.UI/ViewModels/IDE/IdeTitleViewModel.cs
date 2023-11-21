using CommunityToolkit.Mvvm.ComponentModel;
using Meditation.MetadataLoaderService.Models;
using Meditation.UI.Services;

namespace Meditation.UI.ViewModels.IDE
{
    public partial class IdeTitleViewModel : ViewModelBase
    {
        [ObservableProperty] private string? _title;
        [ObservableProperty] private bool _isDirty;
        private readonly IWorkspaceContext _workspaceContext;
        private const string DefaultTitle = "Meditation Editor";

        public IdeTitleViewModel(IWorkspaceContext workspaceContext)
        {
            Title = DefaultTitle;
            _workspaceContext = workspaceContext;
            RegisterEventHandlers();
        }

        private void RegisterEventHandlers()
        {
            _workspaceContext.WorkspaceCreated += OnWorkspaceCreated;
            _workspaceContext.WorkspaceDestroyed += OnWorkspaceDestroyed;
        }

        private void OnWorkspaceCreated(MethodMetadataEntry method) => Title = method.ToFullDisplayString();
        private void OnWorkspaceDestroyed(MethodMetadataEntry method) => Title = DefaultTitle;
    }
}

using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using Meditation.CompilationService;

namespace Meditation.UI.ViewModels.IDE
{
    public partial class DevelopmentEnvironmentViewModel : ViewModelBase
    {
        [ObservableProperty] private IdeTitleViewModel _titleViewModel;
        [ObservableProperty] private IdeTextEditorViewModel _textEditorViewModel;
        [ObservableProperty] private IdeDiagnosticsListViewModel _diagnosticsViewModel;

        public DevelopmentEnvironmentViewModel(
            IWorkspaceContext workspaceContext,
            ICompilationService compilationService)
        {
            TitleViewModel = new IdeTitleViewModel(workspaceContext);
            TextEditorViewModel = new IdeTextEditorViewModel(workspaceContext, compilationService);
            DiagnosticsViewModel = new IdeDiagnosticsListViewModel
            {
                Diagnostics = new[]
                {
                    new IdeDiagnosticsViewModel("Error", "TEST", 123, "Test error message 1"),
                    new IdeDiagnosticsViewModel("Warning", "TEST", 321, "Test error message 2")
                }.ToImmutableArray()
            };
        }
    }
}

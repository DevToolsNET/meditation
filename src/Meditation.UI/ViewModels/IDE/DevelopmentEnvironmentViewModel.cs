using System;
using System.Collections.Immutable;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Meditation.CompilationService;

namespace Meditation.UI.ViewModels.IDE
{
    public partial class DevelopmentEnvironmentViewModel : ViewModelBase
    {
        [ObservableProperty] private IdeTitleViewModel _titleViewModel;
        [ObservableProperty] private IdeTextEditorViewModel _textEditorViewModel;
        [ObservableProperty] private IdeDiagnosticsSummaryViewModel _diagnosticsSummaryViewModel;
        [ObservableProperty] private IdeStatusBarViewModel _statusBarViewModel;

        public DevelopmentEnvironmentViewModel(
            IWorkspaceContext workspaceContext,
            ICodeTemplateProvider codeTemplateProvider)
        {
            TitleViewModel = new IdeTitleViewModel(workspaceContext);
            TextEditorViewModel = new IdeTextEditorViewModel(workspaceContext, codeTemplateProvider);
            DiagnosticsSummaryViewModel = new IdeDiagnosticsSummaryViewModel
            {
                DiagnosticEntries = new(),
                Output = string.Empty
            };
            StatusBarViewModel = new IdeStatusBarViewModel();
        }
    }
}

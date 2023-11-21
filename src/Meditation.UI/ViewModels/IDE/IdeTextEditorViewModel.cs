using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Meditation.CompilationService;
using Meditation.MetadataLoaderService.Models;

namespace Meditation.UI.ViewModels.IDE
{
    public partial class IdeTextEditorViewModel : ViewModelBase
    {
        public event Action<string?>? TextChanged;
        [ObservableProperty] private string? _text;
        private readonly IWorkspaceContext _workspaceContext;
        private readonly ICompilationService _compilationService;

        public IdeTextEditorViewModel(
            IWorkspaceContext workspaceContext,
            ICompilationService compilationService)
        {
            Text = CreateDefaultText();
            _workspaceContext = workspaceContext;
            _compilationService = compilationService;
            RegisterEventHandlers();
        }

        private void RegisterEventHandlers()
        {
            _workspaceContext.WorkspaceCreated += OnWorkspaceCreated;
            _workspaceContext.WorkspaceDestroyed += OnWorkspaceDestroyed;
        }

        private string CreateDefaultText()
            => "Hello World!";

        private void OnWorkspaceCreated(MethodMetadataEntry method) => Text = _compilationService.GenerateCodeTemplateForPatch(method);
        private void OnWorkspaceDestroyed(MethodMetadataEntry _) => Text = CreateDefaultText();
        partial void OnTextChanged(string? value) => TextChanged?.Invoke(value);
    }
}

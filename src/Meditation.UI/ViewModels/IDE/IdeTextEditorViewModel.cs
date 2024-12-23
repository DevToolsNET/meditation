﻿using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Meditation.CompilationService;
using Meditation.MetadataLoaderService.Models;

namespace Meditation.UI.ViewModels.IDE
{
    public partial class IdeTextEditorViewModel : ViewModelBase
    {
        public event Action<string?>? TextChanged;
        public event Action<bool>? EnabledChanged;
        [ObservableProperty] private string? _text;
        [ObservableProperty] private bool _enabled;
        [ObservableProperty] private bool _isInitializingWorkspace;
        private readonly IWorkspaceContext _workspaceContext;
        private readonly ICodeTemplateProvider _codeTemplateProvider;

        public IdeTextEditorViewModel(
            IWorkspaceContext workspaceContext,
            ICodeTemplateProvider codeTemplateProvider)
        {
            Text = CreateDefaultText();
            _workspaceContext = workspaceContext;
            _codeTemplateProvider = codeTemplateProvider;
            RegisterEventHandlers();
        }

        private void RegisterEventHandlers()
        {
            _workspaceContext.WorkspaceCreating += OnWorkspaceCreating;
            _workspaceContext.WorkspaceCreated += OnWorkspaceCreated;
            _workspaceContext.WorkspaceDestroyed += OnWorkspaceDestroyed;
        }

        private string CreateDefaultText()
        {
            return """
                   Welcome to Meditation!
                   
                   If you want to create a patch:
                   * Attach to a process
                   * Select a method from the metadata viewer
                   * Create a workspace (Workspace -> Create)
                   * Implement your patch
                   * Build the workspace (Compilation -> Build)
                   
                   If you want to apply a patch:
                   * Attach to a process
                   * Select a patch from the patcher viewer
                   * Ensure that there are no errors (target process must contain patched assemblies)
                   * Right click the patch and press "Apply"
                   """;
        }

        private void OnWorkspaceCreating(MethodMetadataEntry obj)
        {
            Text = string.Empty;
            IsInitializingWorkspace = true;
        }

        private void OnWorkspaceCreated(MethodMetadataEntry method)
        {
            Enabled = true;
            IsInitializingWorkspace = false;
            Text = _codeTemplateProvider.GenerateCodeTemplateForPatch(method);
        }

        private void OnWorkspaceDestroyed(MethodMetadataEntry _)
        {
            Enabled = false;
            Text = CreateDefaultText();
        }

        partial void OnTextChanged(string? value) => TextChanged?.Invoke(value);
        partial void OnEnabledChanged(bool value) => EnabledChanged?.Invoke(value);
    }
}

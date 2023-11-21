using System.Linq;
using System.Text;
using System.Threading;
using CommunityToolkit.Mvvm.Input;
using Meditation.MetadataLoaderService.Models;
using Meditation.UI.ViewModels;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Threading.Tasks;
using Meditation.UI.ViewModels.IDE;
using System;
using System.IO;
using Meditation.CompilationService;
using Microsoft.CodeAnalysis;

namespace Meditation.UI.Controllers
{
    public partial class DevelopmentEnvironmentController
    {
        private readonly IAvaloniaDialogService _dialogService;
        private readonly IWorkspaceContext _compilationContext;

        public DevelopmentEnvironmentController(
            IWorkspaceContext compilationContext, 
            IAvaloniaDialogService dialogService)
        {
            _dialogService = dialogService;
            _compilationContext = compilationContext;
        }

        [RelayCommand]
        public async Task CreateWorkspace(MetadataBrowserViewModel metadataBrowserViewModel)
        {
            try
            {
                if (metadataBrowserViewModel.SelectedItem is not MethodMetadataEntry method)
                {
                    var messageBox = MessageBoxManager.GetMessageBoxStandard(
                        title: "No method was selected",
                        text: "Unable to create a new workspace because no method was selected.",
                        @enum: ButtonEnum.Ok);
                    await messageBox.ShowAsync();
                    return;
                }

                _compilationContext.CreateWorkspace(method, "MeditationTemporaryProject", "MeditationPatch");
            }
            catch (Exception exception)
            {
                await ShowUnhandledExceptionMessageBox(exception);
            }
        }

        [RelayCommand]
        public async Task BuildWorkspace(DevelopmentEnvironmentViewModel ideViewModel, CancellationToken ct)
        {
            try
            {
                // Build project
                _compilationContext.AddDocument(ideViewModel.TextEditorViewModel.Text ?? string.Empty, Encoding.UTF8);
                var compilationResult = await _compilationContext.Build(ct);
                UpdateUserInterfaceAfterBuild(ideViewModel, compilationResult);

                if (compilationResult.Success)
                {
                    var messageBox = MessageBoxManager.GetMessageBoxStandard(
                        title: "Patch prepared", 
                        text: "Would you like to save it?", 
                        @enum: ButtonEnum.YesNo);
                    var result = await messageBox.ShowAsync();
                    if (result == ButtonResult.Yes)
                    {
                        const string filename = "TestPatch.dll";
                        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Meditation");
                        if (!Directory.Exists(folder))
                            Directory.CreateDirectory(folder);
                        var fullPath = Path.Combine(folder, filename);

                        var data = _compilationContext.GetProjectAssembly();
                        await File.WriteAllBytesAsync(fullPath, data, ct);
                        ideViewModel.StatusBarViewModel.SetSuccessStatus($"Saved as {fullPath}.");
                    }
                }
            }
            catch (Exception exception)
            {
                await ShowUnhandledExceptionMessageBox(exception);
            }
        }

        [RelayCommand]
        public async Task DestroyWorkspace(DevelopmentEnvironmentViewModel ideViewModel)
        {
            try
            {
                if (_compilationContext.Method is not { } method)
                {
                    var messageBox = MessageBoxManager.GetMessageBoxStandard(
                        title: "No workspace is active",
                        text: "Unable to destroy workspace as there is no editor active.",
                        @enum: ButtonEnum.Ok);
                    await messageBox.ShowAsync();
                    return;
                }

                _compilationContext.DestroyWorkspace();
                ideViewModel.StatusBarViewModel.SetInformationStatus("Ready.");
            }
            catch (Exception exception)
            {
                await ShowUnhandledExceptionMessageBox(exception);
            }
        }

        private void UpdateUserInterfaceAfterBuild(DevelopmentEnvironmentViewModel ideViewModel, CompilationResult compilationResult)
        {
            // Set errors list and output view
            var diagnosticEntries = compilationResult.Result.First().Value.Diagnostics;
            ideViewModel.DiagnosticsSummaryViewModel.DiagnosticEntries.Clear();
            ideViewModel.DiagnosticsSummaryViewModel.Output = compilationResult.OutputLog;
            foreach (var item in diagnosticEntries)
            {
                var entry = new IdeDiagnosticsEntryViewModel(
                    severity: item.Severity.ToString(),
                    code: item.Id,
                    location: item.Location.ToString(),
                    message: item.GetMessage());
                ideViewModel.DiagnosticsSummaryViewModel.DiagnosticEntries.Add(entry);
            }

            // Set status bar
            var errorsCount = diagnosticEntries.Count(e => e.Severity == DiagnosticSeverity.Error);
            var warningsCount = diagnosticEntries.Count(e => e.Severity == DiagnosticSeverity.Warning);
            if (errorsCount > 0)
                ideViewModel.StatusBarViewModel.SetErrorStatus($"Build failed due to {errorsCount} error(s).");
            else if (warningsCount > 0)
                ideViewModel.StatusBarViewModel.SetWarningStatus($"Build succeeded with {warningsCount} warning(s).");
            else
                ideViewModel.StatusBarViewModel.SetSuccessStatus("Build succeeded.");
        }

        private static Task<ButtonResult> ShowMessageBox(string title, string content, ButtonEnum buttonEnum = ButtonEnum.Ok)
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(title: title, text: content, @enum: buttonEnum);
            return messageBox.ShowAsync();
        }

        private static Task ShowUnhandledExceptionMessageBox(Exception exception)
        {
            return ShowMessageBox(title: "Unhandled exception", content: exception.ToString());
        }
    }
}

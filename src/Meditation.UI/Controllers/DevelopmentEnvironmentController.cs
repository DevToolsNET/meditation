using CommunityToolkit.Mvvm.Input;
using Meditation.AttachProcessService;
using Meditation.AttachProcessService.Models;
using Meditation.CompilationService;
using Meditation.MetadataLoaderService.Models;
using Meditation.PatchingService;
using Meditation.PatchingService.Models;
using Meditation.UI.Configuration;
using Meditation.UI.Controllers.Utils;
using Meditation.UI.ViewModels;
using Meditation.UI.ViewModels.IDE;
using Microsoft.CodeAnalysis;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.UI.Controllers
{
    public partial class DevelopmentEnvironmentController
    {
        private readonly IWorkspaceContext _compilationContext;
        private readonly IPatchStorage _patchStorage;
        private readonly IAvaloniaDialogService _dialogService;
        private readonly InputTextDialogController _inputTextDialogController;

        public DevelopmentEnvironmentController(
            IWorkspaceContext compilationContext, 
            IPatchStorage patchStorage,
            IAvaloniaDialogService dialogService,
            InputTextDialogController inputTextDialogController)
        {
            _compilationContext = compilationContext;
            _patchStorage = patchStorage;
            _dialogService = dialogService;
            _inputTextDialogController = inputTextDialogController;
        }

        [RelayCommand]
        public async Task CreateWorkspace(MetadataBrowserViewModel metadataBrowserViewModel, CancellationToken ct)
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

                // Let user select project (assembly) name for the new patch
                var dialog = await _inputTextDialogController.CreateDialog("Enter patch name for the new workspace:");
                if (dialog is null || await _inputTextDialogController.GetDataContext(dialog) is not {} inputTextDialogDataContext)
                    return;
                await _inputTextDialogController.ShowDialog(dialog);

                var projectName = inputTextDialogDataContext.Text ?? "MyPatch";
                await _compilationContext.CreateWorkspaceAsync(method, projectName, projectName, ct);
            }
            catch (Exception exception)
            {
                await DialogUtilities.ShowUnhandledExceptionMessageBox(exception);
            }
        }

        [RelayCommand]
        public async Task BuildWorkspace(DevelopmentEnvironmentViewModel ideViewModel, CancellationToken ct)
        {
            try
            {
                // Build project
                _compilationContext.AddDocument(ideViewModel.TextEditorViewModel.Text ?? string.Empty, Encoding.UTF8);
                var compilationResult = await _compilationContext.BuildAsync(ct);
                UpdateUserInterfaceAfterBuild(ideViewModel, compilationResult);

                if (compilationResult.Success)
                    await UpdateUserInterfaceAfterSuccessfulBuildAsync(ideViewModel, ct);
            }
            catch (Exception exception)
            {
                await DialogUtilities.ShowUnhandledExceptionMessageBox(exception);
            }
        }

        [RelayCommand]
        public async Task DestroyWorkspace(DevelopmentEnvironmentViewModel ideViewModel)
        {
            try
            {
                if (_compilationContext.Method is null)
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
                await DialogUtilities.ShowUnhandledExceptionMessageBox(exception);
            }
        }

        private async Task UpdateUserInterfaceAfterSuccessfulBuildAsync(DevelopmentEnvironmentViewModel ideViewModel, CancellationToken ct)
        {
            // Ask user if we want to save patch
            var messageBox = MessageBoxManager.GetMessageBoxStandard(
                title: "Patch was successfully built",
                text: "Do you want to save it?",
                @enum: ButtonEnum.YesNo);
            var result = await messageBox.ShowAsync();
            if (result != ButtonResult.Yes)
                return;

            // Let user choose save directory / filename
            var fileInfo = await _dialogService.ShowSaveFileDialog(
                title: "Save patch as...",
                extension: "dll",
                folder: _patchStorage.GetRootFolderForPatches(),
                fileName: "Patch");
            if (fileInfo == null)
                return;

            // Store patch
            var fullPath = fileInfo.Path.AbsolutePath;
            var data = _compilationContext.GetProjectAssembly();
            await _patchStorage.StorePatch(fullPath, data, overwriteExistingFile: true, ct);
            ideViewModel.StatusBarViewModel.SetSuccessStatus($"Saved as {fullPath}.");
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
    }
}

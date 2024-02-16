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
using Meditation.PatchingService;
using Microsoft.CodeAnalysis;

namespace Meditation.UI.Controllers
{
    public partial class DevelopmentEnvironmentController
    {
        private readonly IWorkspaceContext _compilationContext;
        private readonly IAttachedProcessContext _attachedProcessContext;
        private readonly IPatchStorage _patchStorage;
        private readonly IPatchListProvider _patchListProvider;
        private readonly IPatchApplier _patchApplier;

        public DevelopmentEnvironmentController(
            IWorkspaceContext compilationContext, 
            IAttachedProcessContext attachedProcessContext, 
            IPatchListProvider patchListProvider, 
            IPatchStorage patchStorage,
            IPatchApplier patchApplier)
        {
            _compilationContext = compilationContext;
            _attachedProcessContext = attachedProcessContext;
            _patchListProvider = patchListProvider;
            _patchStorage = patchStorage;
            _patchApplier = patchApplier;
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
                    await UpdateUserInterfaceAfterSuccessfulBuild(ideViewModel, ct);
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

        private async Task UpdateUserInterfaceAfterSuccessfulBuild(DevelopmentEnvironmentViewModel ideViewModel, CancellationToken ct)
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(
                title: "Patch was successfully built",
                text: "Do you want to save it?",
                @enum: ButtonEnum.YesNo);
            var result = await messageBox.ShowAsync();
            if (result != ButtonResult.Yes)
                return;

            // TODO: let user choose name
            const string filename = "TestPatch.dll";

            var data = _compilationContext.GetProjectAssembly();
            await _patchStorage.StorePatch(filename, data, overwriteExistingFile: true, ct);
            var fullName = Path.Combine(_patchStorage.GetRootFolderForPatches(), filename);
            ideViewModel.StatusBarViewModel.SetSuccessStatus($"Saved as {fullName}.");

            messageBox = MessageBoxManager.GetMessageBoxStandard(
                title: "Patch was successfully stored",
                text: "Do you want to apply it?",
                @enum: ButtonEnum.YesNo);
            result = await messageBox.ShowAsync();
            if (result != ButtonResult.Yes)
                return;

            var pid = _attachedProcessContext.ProcessSnapshot!.ProcessId.Value;
            var patch = _patchListProvider.GetAllPatches().Single(p => p.Path == fullName);
            _patchApplier.ApplyPatch(pid, patch);
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

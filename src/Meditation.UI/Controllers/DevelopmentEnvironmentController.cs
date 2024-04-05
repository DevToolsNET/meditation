using CommunityToolkit.Mvvm.Input;
using Meditation.AttachProcessService;
using Meditation.AttachProcessService.Models;
using Meditation.CompilationService;
using Meditation.MetadataLoaderService.Models;
using Meditation.PatchingService;
using Meditation.PatchingService.Models;
using Meditation.UI.Configuration;
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
        private readonly ApplicationConfiguration _configuration;
        private readonly IWorkspaceContext _compilationContext;
        private readonly IAttachedProcessContext _attachedProcessContext;
        private readonly IProcessListProvider _processListProvider;
        private readonly IPatchStorage _patchStorage;
        private readonly IPatchListProvider _patchListProvider;
        private readonly IPatchApplier _patchApplier;
        private readonly IAvaloniaDialogService _dialogService;

        public DevelopmentEnvironmentController(
            ApplicationConfiguration configuration, 
            IWorkspaceContext compilationContext, 
            IAttachedProcessContext attachedProcessContext,
            IProcessListProvider processListProvider,
            IPatchListProvider patchListProvider, 
            IPatchStorage patchStorage,
            IPatchApplier patchApplier,
            IAvaloniaDialogService dialogService)
        {
            _configuration = configuration;
            _compilationContext = compilationContext;
            _attachedProcessContext = attachedProcessContext;
            _processListProvider = processListProvider;
            _patchListProvider = patchListProvider;
            _patchStorage = patchStorage;
            _patchApplier = patchApplier;
            _dialogService = dialogService;
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

                await _compilationContext.CreateWorkspace(method, "MeditationTemporaryProject", "MeditationPatch", ct);
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

            // TODO: code is temporary and will be refactored in milestone 4
            messageBox = MessageBoxManager.GetMessageBoxStandard(
                title: "Patch was successfully stored",
                text: "Do you want to apply it?",
                @enum: ButtonEnum.YesNo);
            result = await messageBox.ShowAsync();
            if (result != ButtonResult.Yes)
                return;

            var patch = _patchListProvider.GetAllPatches().Single(p => string.Equals(
                Path.GetFullPath(p.Path), 
                Path.GetFullPath(fileInfo.Path.AbsolutePath), 
                StringComparison.InvariantCultureIgnoreCase));

            await ApplyPatch(patch, ct);
        }

        private async Task ApplyPatch(PatchInfo patch, CancellationToken ct)
        {
            // TODO: this code is temporary and will be refactored in milestone 4
            var processId = new ProcessId(_attachedProcessContext.ProcessSnapshot!.ProcessId.Value);
            var processInfo = _processListProvider.GetProcessById(processId);
            await processInfo.Initialize(ct);

            if (processInfo.Architecture is null)
                throw new Exception($"Could not determine architecture of the target process {processId}.");

            var bootstrapLibrary = _configuration.NativeExecutables
                .FirstOrDefault(e => e.Architecture == processInfo.Architecture);

            if (bootstrapLibrary == null)
                throw new Exception($"Application configuration did not specify native executable for architecture {processInfo.Architecture}.");

            var patchConfiguration = new PatchingConfiguration(
                PatchInfo: patch,
                NativeBootstrapLibraryPath: bootstrapLibrary.Path,
                NativeExportedEntryPointSymbol: _configuration.Hooking.NativeExportedEntryPointSymbol,
                ManagedBootstrapEntryPointTypeFullName: _configuration.Hooking.ManagedBootstrapEntryPointTypeFullName,
                ManagedBootstrapEntryPointMethod: _configuration.Hooking.ManagedBootstrapEntryPointMethod,
                CompanyUniqueIdentifier: _configuration.UniquePatchingIdentifierScope,
                NativeBootstrapLibraryLoggingPath: _configuration.Logging.BootstrapNativeFileName,
                ManagedBootstrapLibraryLoggingPath: _configuration.Logging.BootstrapManagedFileName);

            await Task.Run(() => _patchApplier.ApplyPatch(processId.Value, patchConfiguration), ct);
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

using Meditation.AttachProcessService.Models;
using Meditation.AttachProcessService;
using Meditation.PatchingService;
using Meditation.UI.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.IO;
using CommunityToolkit.Mvvm.Input;
using Meditation.UI.Controllers.Utils;
using Microsoft.Extensions.Logging;

namespace Meditation.UI.Controllers
{
    public partial class PatchProcessController
    {
        private readonly ApplicationConfiguration _configuration;
        private readonly IAttachedProcessContext _attachedProcessContext;
        private readonly IProcessListProvider _processListProvider;
        private readonly IPatchApplier _patchApplier;
        private readonly IPatchReverser _patchReverser;
        private readonly ILogger _logger;

        public PatchProcessController(
            ApplicationConfiguration configuration,
            IProcessListProvider processListProvider,
            IAttachedProcessContext attachedProcessContext,
            IPatchApplier patchApplier,
            IPatchReverser patchReverser,
            ILogger<PatchProcessController> logger)
        {
            _configuration = configuration;
            _processListProvider = processListProvider;
            _attachedProcessContext = attachedProcessContext;
            _patchApplier = patchApplier;
            _patchReverser = patchReverser;
            _logger = logger;
        }

        [RelayCommand]
        public async Task ApplyPatch(string patchAssemblyPath, CancellationToken ct)
        {
            try
            {
                _logger.LogDebug("Command {cmd} started.", nameof(ApplyPatch));
                await ApplyPatchImpl(patchAssemblyPath, ct);
                _logger.LogDebug("Command {cmd} finished.", nameof(ApplyPatch));
            }
            catch (Exception exception)
            {
                _logger.LogError("Command {cmd} failed.", nameof(ApplyPatch));
                await DialogUtilities.ShowUnhandledExceptionMessageBox(exception);
            }
        }

        [RelayCommand]
        public async Task ReversePatch(string patchAssemblyPath, CancellationToken ct)
        {
            try
            {
                _logger.LogDebug("Command {cmd} started.", nameof(ReversePatch));
                await ReversePatchImpl(patchAssemblyPath, ct);
                _logger.LogDebug("Command {cmd} finished.", nameof(ReversePatch));
            }
            catch (Exception exception)
            {
                _logger.LogError("Command {cmd} failed.", nameof(ReversePatch));
                await DialogUtilities.ShowUnhandledExceptionMessageBox(exception);
            }
        }

        private async Task ApplyPatchImpl(string patchAssemblyPath, CancellationToken ct)
        {
            await ApplyPatchAsync(patchAssemblyPath, ct);
            await DialogUtilities.ShowMessageBox(
                title: "Success",
                content: $"Applied patch \"{patchAssemblyPath}\" into target process.");
        }

        private async Task ReversePatchImpl(string patchAssemblyPath, CancellationToken ct)
        {
            await ReversePatchAsync(patchAssemblyPath, ct);
            await DialogUtilities.ShowMessageBox(
                title: "Success",
                content: $"Reversed patch \"{patchAssemblyPath}\" into target process.");
        }

        private async Task ApplyPatchAsync(string patchAssemblyPath, CancellationToken ct)
        {
            var processInfo = await GetAttachedProcessInfo(ct);
            var bootstrapLibraryPath = GetNativeBootstrapLibraryPath(processInfo);

            var patchConfiguration = new PatchingConfiguration(
                PatchAssemblyPath: patchAssemblyPath,
                NativeBootstrapLibraryPath: bootstrapLibraryPath,
                NativeExportedEntryPointSymbol: _configuration.Hooking.NativeExportedEntryPointSymbol,
                ManagedBootstrapEntryPointTypeFullName: _configuration.Hooking.ManagedBootstrapEntryPointTypeFullName,
                ManagedBootstrapEntryPointMethod: _configuration.Hooking.ManagedBootstrapEntryPointHookMethod,
                CompanyUniqueIdentifier: _configuration.UniquePatchingIdentifierScope,
                NativeBootstrapLibraryLoggingPath: _configuration.Logging.BootstrapNativeFileName,
                ManagedBootstrapLibraryLoggingPath: _configuration.Logging.BootstrapManagedFileName);

            await Task.Run(() => _patchApplier.ApplyPatch(processInfo.Id.Value, patchConfiguration), ct);
        }

        private async Task ReversePatchAsync(string patchAssemblyPath, CancellationToken ct)
        {
            var processInfo = await GetAttachedProcessInfo(ct);
            var bootstrapLibraryPath = GetNativeBootstrapLibraryPath(processInfo);

            var patchConfiguration = new PatchingConfiguration(
                PatchAssemblyPath: patchAssemblyPath,
                NativeBootstrapLibraryPath: bootstrapLibraryPath,
                NativeExportedEntryPointSymbol: _configuration.Hooking.NativeExportedEntryPointSymbol,
                ManagedBootstrapEntryPointTypeFullName: _configuration.Hooking.ManagedBootstrapEntryPointTypeFullName,
                ManagedBootstrapEntryPointMethod: _configuration.Hooking.ManagedBootstrapEntryPointUnhookMethod,
                CompanyUniqueIdentifier: _configuration.UniquePatchingIdentifierScope,
                NativeBootstrapLibraryLoggingPath: _configuration.Logging.BootstrapNativeFileName,
                ManagedBootstrapLibraryLoggingPath: _configuration.Logging.BootstrapManagedFileName);

            await Task.Run(() => _patchReverser.ReversePatch(processInfo.Id.Value, patchConfiguration), ct);
        }

        private async Task<ProcessInfo> GetAttachedProcessInfo(CancellationToken ct)
        {
            var processId = new ProcessId(_attachedProcessContext.ProcessSnapshot!.ProcessId.Value);
            var processInfo = _processListProvider.GetProcessById(processId);
            await processInfo.Initialize(ct);
            return processInfo;
        }

        private string GetNativeBootstrapLibraryPath(ProcessInfo processInfo)
        {
            if (processInfo.Architecture is null)
                throw new Exception($"Could not determine architecture of the target process {processInfo.Id.Value}.");

            var bootstrapLibrary = _configuration.NativeExecutables
                .FirstOrDefault(e => e.Architecture == processInfo.Architecture);

            if (bootstrapLibrary == null)
                throw new Exception($"Application configuration did not specify native executable for architecture {processInfo.Architecture}.");

            if (!File.Exists(bootstrapLibrary.Path))
                throw new Exception($"Could not find \"{bootstrapLibrary.Path}\". Search directory: \"{Directory.GetCurrentDirectory()}\".");

            return bootstrapLibrary.Path;
        }
    }
}

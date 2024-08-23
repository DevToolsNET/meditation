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

namespace Meditation.UI.Controllers
{
    public partial class PatchProcessController
    {
        private readonly ApplicationConfiguration _configuration;
        private readonly IAttachedProcessContext _attachedProcessContext;
        private readonly IProcessListProvider _processListProvider;
        private readonly IPatchApplier _patchApplier;

        public PatchProcessController(
            ApplicationConfiguration configuration,
            IProcessListProvider processListProvider,
            IAttachedProcessContext attachedProcessContext,
            IPatchApplier patchApplier)
        {
            _configuration = configuration;
            _processListProvider = processListProvider;
            _attachedProcessContext = attachedProcessContext;
            _patchApplier = patchApplier;
        }

        [RelayCommand]
        public async Task ApplyPatch(string patchAssemblyPath, CancellationToken ct)
        {
            try
            {
                await ApplyPatchAsync(patchAssemblyPath, ct);
                await DialogUtilities.ShowMessageBox(
                    title: "Success",
                    content: $"Applied patch \"{patchAssemblyPath}\" into target process.");
            }
            catch (Exception exception)
            {
                await DialogUtilities.ShowUnhandledExceptionMessageBox(exception);
            }
        }

        [RelayCommand]
        public Task ReversePatch(string patchAssemblyPath, CancellationToken ct)
        {
            try
            {
                // TODO: implement patch reverse
                throw new NotImplementedException("Reversing has not been implemented yet.");
            }
            catch (Exception exception)
            {
                return DialogUtilities.ShowUnhandledExceptionMessageBox(exception);
            }
        }

        private async Task ApplyPatchAsync(string patchAssemblyPath, CancellationToken ct)
        {
            var processId = new ProcessId(_attachedProcessContext.ProcessSnapshot!.ProcessId.Value);
            var processInfo = _processListProvider.GetProcessById(processId);
            await processInfo.Initialize(ct);

            if (processInfo.Architecture is null)
                throw new Exception($"Could not determine architecture of the target process {processId}.");

            var bootstrapLibrary = _configuration.NativeExecutables
                .FirstOrDefault(e => e.Architecture == processInfo.Architecture);

            if (bootstrapLibrary == null)
                throw new Exception($"Application configuration did not specify native executable for architecture {processInfo.Architecture}.");

            if (!File.Exists(bootstrapLibrary.Path))
                throw new Exception($"Could not find \"{bootstrapLibrary.Path}\". Search directory: \"{Directory.GetCurrentDirectory()}\".");

            var patchConfiguration = new PatchingConfiguration(
                PatchAssemblyPath: patchAssemblyPath,
                NativeBootstrapLibraryPath: bootstrapLibrary.Path,
                NativeExportedEntryPointSymbol: _configuration.Hooking.NativeExportedEntryPointSymbol,
                ManagedBootstrapEntryPointTypeFullName: _configuration.Hooking.ManagedBootstrapEntryPointTypeFullName,
                ManagedBootstrapEntryPointMethod: _configuration.Hooking.ManagedBootstrapEntryPointMethod,
                CompanyUniqueIdentifier: _configuration.UniquePatchingIdentifierScope,
                NativeBootstrapLibraryLoggingPath: _configuration.Logging.BootstrapNativeFileName,
                ManagedBootstrapLibraryLoggingPath: _configuration.Logging.BootstrapManagedFileName);

            await Task.Run(() => _patchApplier.ApplyPatch(processId.Value, patchConfiguration), ct);
        }
    }
}

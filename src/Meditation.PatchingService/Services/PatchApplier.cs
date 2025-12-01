using Meditation.Bootstrap.Managed;
using Meditation.InjectorService;
using System;
using System.Threading.Tasks;

namespace Meditation.PatchingService.Services
{
    internal class PatchApplier : IPatchApplier
    {
        private readonly IProcessInjector _processInjector;
        private readonly IProcessInjecteeExecutor _processInjecteeExecutor;

        public PatchApplier(IProcessInjector processInjector, IProcessInjecteeExecutor processInjecteeExecutor)
        {
            _processInjector = processInjector;
            _processInjecteeExecutor = processInjecteeExecutor;
        }

        public async Task ApplyPatch(int pid, PatchingConfiguration configuration)
        {
            var hookArguments = PatchingConfiguration.ConstructArgs(typeof(EntryPoint).Assembly, configuration);
            var remoteModuleHandle = await _processInjector.TryInjectModule(pid: pid, assemblyPath: configuration.NativeBootstrapLibraryPath);

            if (remoteModuleHandle.IsInvalid)
                throw new Exception("Could not inject patch!");

            var returnCode = _processInjecteeExecutor.ExecuteExportedMethod(
                pid: pid,
                modulePath: configuration.NativeBootstrapLibraryPath,
                injectedModuleHandle: remoteModuleHandle,
                exportedMethodName: configuration.NativeExportedEntryPointSymbol,
                argument: hookArguments);

            if (returnCode != 0)
                throw new Exception($"Patch returned error code that does not indicate success: {returnCode:X}.");
        }
    }
}

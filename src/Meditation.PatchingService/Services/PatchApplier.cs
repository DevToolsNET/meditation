using Meditation.Bootstrap.Managed;
using Meditation.InjectorService;
using System;

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

        public void ApplyPatch(int pid, PatchingConfiguration configuration)
        {
            var hookArguments = PatchingConfiguration.ConstructArgs(typeof(EntryPoint).Assembly, configuration);

            if (!_processInjector.TryInjectModule(pid: pid, assemblyPath: configuration.NativeBootstrapLibraryPath, out var remoteMeditationBootstrapNativeModuleHandle))
                throw new Exception("Could not inject patch!");

            var returnCode = _processInjecteeExecutor.ExecuteExportedMethod(
                pid: pid,
                modulePath: configuration.NativeBootstrapLibraryPath,
                injectedModuleHandle: remoteMeditationBootstrapNativeModuleHandle,
                exportedMethodName: configuration.NativeExportedEntryPointSymbol,
                argument: hookArguments);

            if (returnCode != 0)
                throw new Exception($"Patch returned error code that does not indicate success: {returnCode:X}.");
        }
    }
}

using Meditation.InjectorService;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Meditation.Bootstrap.Managed;
using Meditation.Interop;

namespace Meditation.PatchingService.Services
{
    internal class PatchReverser : IPatchReverser
    {
        private readonly IProcessInjecteeExecutor _processInjecteeExecutor;

        public PatchReverser(IProcessInjecteeExecutor processInjecteeExecutor)
        {
            _processInjecteeExecutor = processInjecteeExecutor;
        }

        public void ReversePatch(int pid, PatchingConfiguration configuration)
        {
            var nativeBootstrapModuleName = Path.GetFileName(configuration.NativeBootstrapLibraryPath);
            var nativeBootstrapModule = Process.GetProcessById(pid)
                .Modules.Cast<ProcessModule>()
                .SingleOrDefault(m => m.ModuleName.Equals(nativeBootstrapModuleName, StringComparison.Ordinal));
            if (nativeBootstrapModule == null)
                throw new InvalidOperationException($"Target process does not have module {nativeBootstrapModuleName}. It has not been injected yet.");

            var remoteMeditationBootstrapNativeModuleHandle = new GenericSafeHandle(
                acquireDelegate: () => nativeBootstrapModule.BaseAddress, 
                releaseDelegate: static _ => true, 
                ownsHandle: false);

            var args = PatchingConfiguration.ConstructArgs(typeof(EntryPoint).Assembly, configuration);
            var returnCode = _processInjecteeExecutor.ExecuteExportedMethod(
                pid: pid,
                modulePath: configuration.NativeBootstrapLibraryPath,
                injectedModuleHandle: remoteMeditationBootstrapNativeModuleHandle,
                exportedMethodName: configuration.NativeExportedEntryPointSymbol,
                argument: args);

            if (returnCode != 0)
                throw new Exception($"Patch returned error code that does not indicate success: {returnCode:X}.");
        }
    }
}

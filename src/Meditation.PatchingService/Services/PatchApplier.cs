using Meditation.InjectorService;
using Meditation.PatchingService.Models;
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

        public void ApplyPatch(int pid, PatchInfo patch)
        {
            const string nativeHookEntryPoint = "MeditationInitialize";
            const string nativeHookLibrary = @"F:\Workspace\meditation\src\Meditation.Bootstrap.Native\bin\Debug\net7.0\win-x64\native\Meditation.Bootstrap.Native.dll";
            var managedHookLibrary = typeof(Bootstrap.Managed.EntryPoint).Assembly.Location;
            var hookArguments = $"{managedHookLibrary}#Meditation.Bootstrap.Managed.EntryPoint#Hook#{patch.Path}";

            if (!_processInjector.TryInjectModule(pid: pid, assemblyPath: nativeHookLibrary, out var remoteMeditationBootstrapNativeModuleHandle))
                throw new Exception("Could not inject patch!");

            if (!_processInjecteeExecutor.TryExecuteExportedMethod(
                    pid: pid,
                    modulePath: nativeHookLibrary,
                    injectedModuleHandle: remoteMeditationBootstrapNativeModuleHandle,
                    exportedMethodName: nativeHookEntryPoint,
                    argument: hookArguments,
                    returnCode: out var returnCode))
            {
                throw new Exception("Could not initialize patch!");
            }

            if (returnCode != 0)
                throw new Exception($"Patch returned error code that does not indicate success: {returnCode.Value:X}");
        }
    }
}

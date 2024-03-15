using Meditation.Bootstrap.Managed;
using Meditation.InjectorService;
using System;

namespace Meditation.PatchingService.Services
{
    internal class PatchApplier : IPatchApplier
    {
        private const string NativeHookEntryPointSymbol = "MeditationInitialize";
        private const string ManagedHookEntryPointTypeFullName = "Meditation.Bootstrap.Managed.EntryPoint";
        private const string ManagedHookEntryPointMethod = "Hook";
        private readonly IProcessInjector _processInjector;
        private readonly IProcessInjecteeExecutor _processInjecteeExecutor;

        public PatchApplier(IProcessInjector processInjector, IProcessInjecteeExecutor processInjecteeExecutor)
        {
            _processInjector = processInjector;
            _processInjecteeExecutor = processInjecteeExecutor;
        }

        public void ApplyPatch(int pid, PatchingConfiguration configuration)
        {
            var hookArguments = ConstructHookArgs(configuration);

            if (!_processInjector.TryInjectModule(pid: pid, assemblyPath: configuration.NativeBootstrapLibraryPath, out var remoteMeditationBootstrapNativeModuleHandle))
                throw new Exception("Could not inject patch!");

            if (!_processInjecteeExecutor.TryExecuteExportedMethod(
                    pid: pid,
                    modulePath: configuration.NativeBootstrapLibraryPath,
                    injectedModuleHandle: remoteMeditationBootstrapNativeModuleHandle,
                    exportedMethodName: NativeHookEntryPointSymbol,
                    argument: hookArguments,
                    returnCode: out var returnCode))
            {
                throw new Exception("Could not initialize patch!");
            }

            if (returnCode != 0)
                throw new Exception($"Patch returned error code that does not indicate success: {returnCode.Value:X}.");
        }

        private static string ConstructHookArgs(PatchingConfiguration configuration)
        {
            return string.Join('#',
                configuration.NativeBootstrapLibraryLoggingPath,
                configuration.ManagedBootstrapLibraryLoggingPath,
                configuration.CompanyUniqueIdentifier,
                typeof(EntryPoint).Assembly.Location,
                ManagedHookEntryPointTypeFullName,
                ManagedHookEntryPointMethod,
                configuration.PatchInfo.Path);
        }
    }
}

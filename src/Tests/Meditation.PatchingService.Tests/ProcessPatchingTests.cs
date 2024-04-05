using Meditation.Bootstrap.Managed;
using Meditation.InjectorService;
using Meditation.PatchingService.Models;
using Meditation.TestUtils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Meditation.PatchingService.Tests
{
    public class ProcessPatchingTests : TestsBase
    {
        private const string ipcTerminationSignaller = "/meditation/tests-ipc-signal";

        [Theory]
        [InlineData("net8.0")]
#if WINDOWS
        [InlineData("net472")]
#endif
        public async Task PatchingService_PatchOtherProcess(string netSdkIdentifier)
        {
            // Prepare
            var patchInfo = new PatchInfo(
                Path: typeof(TestPatch.Harmony.Patch).Assembly.Location,
                TargetFullAssemblyName: typeof(TestPatch.Harmony.Patch).Assembly.FullName ?? "test-assembly",
                Method: null! /* Not used by patch applier */);

            var patchingConfiguration = new PatchingConfiguration(
                PatchInfo: patchInfo,
                NativeBootstrapLibraryPath: BootstrapNativeHelpers.GetMeditationNativeModulePath(),
                NativeExportedEntryPointSymbol: "MeditationInitialize",
                ManagedBootstrapEntryPointTypeFullName: "Meditation.Bootstrap.Managed.EntryPoint",
                ManagedBootstrapEntryPointMethod: "Hook",
                CompanyUniqueIdentifier: "net.devtools.meditation.unittests",
                NativeBootstrapLibraryLoggingPath: "./native-tests.log",
                ManagedBootstrapLibraryLoggingPath: "./managed.tests.log");
            var hookArgs = PatchingConfiguration.ConstructHookArgs(typeof(EntryPoint).Assembly, patchingConfiguration);

            var processInjector = ServiceProvider.GetRequiredService<IProcessInjector>();
            var processInjecteeExecutor = ServiceProvider.GetRequiredService<IProcessInjecteeExecutor>();
            using var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, ipcTerminationSignaller);

            // Act (module injection)
            using var executionTask = TestSubjectHelpers.GetTestSubjectExecutionCommand(netSdkIdentifier).ExecuteAsync();
            var processId = executionTask.ProcessId;
            var moduleInjectionResult = processInjector.TryInjectModule(processId, patchingConfiguration.NativeBootstrapLibraryPath, out var remoteModuleHandle);
            // Act (code execution)
            bool? executionResult = null;
            uint? returnCode = null;
            if (moduleInjectionResult)
            {
                // Ensure module had time to load properly
                await Task.Delay(TimeSpan.FromSeconds(value: 1));
                // Execute managed hook entrypoint
                using var moduleHandle = remoteModuleHandle;
                executionResult = processInjecteeExecutor.TryExecuteExportedMethod(
                    processId,
                    patchingConfiguration.NativeBootstrapLibraryPath,
                    moduleHandle!,
                    patchingConfiguration.NativeExportedEntryPointSymbol,
                    hookArgs,
                    out returnCode);
            }
            // Wait for target process to exit gracefully
            waitHandle.Set();
            await executionTask;

            // Assert
            Assert.True(moduleInjectionResult);
            Assert.True(executionResult);
            Assert.True(returnCode.HasValue);
            Assert.True(returnCode.Value == 0);
        }
    }
}
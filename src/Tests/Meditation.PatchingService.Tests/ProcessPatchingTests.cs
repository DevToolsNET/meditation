using Meditation.Bootstrap.Managed;
using Meditation.InjectorService;
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
        public async Task PatchingService_PatchOtherProcess(string netSdkIdentifier)
        {
            // Prepare
            var patchingConfiguration = new PatchingConfiguration(
                PatchAssemblyPath: typeof(TestPatch.Harmony.Patch).Assembly.Location,
                NativeBootstrapLibraryPath: BootstrapNativeHelpers.GetMeditationNativeModulePath(),
                NativeExportedEntryPointSymbol: "MeditationInitialize",
                ManagedBootstrapEntryPointTypeFullName: "Meditation.Bootstrap.Managed.EntryPoint",
                ManagedBootstrapEntryPointMethod: "Hook",
                CompanyUniqueIdentifier: "net.devtools.meditation.unittests",
                NativeBootstrapLibraryLoggingPath: "./native-tests.log",
                ManagedBootstrapLibraryLoggingPath: "./managed.tests.log");
            var hookArgs = PatchingConfiguration.ConstructArgs(typeof(EntryPoint).Assembly, patchingConfiguration);

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

        [Theory]
        [InlineData("net8.0")]
        public async Task PatchingService_ReversePatchOtherProcess(string netSdkIdentifier)
        {
            // Prepare
            var hookingConfiguration = new PatchingConfiguration(
                PatchAssemblyPath: typeof(TestPatch.Harmony.Patch).Assembly.Location,
                NativeBootstrapLibraryPath: BootstrapNativeHelpers.GetMeditationNativeModulePath(),
                NativeExportedEntryPointSymbol: "MeditationInitialize",
                ManagedBootstrapEntryPointTypeFullName: "Meditation.Bootstrap.Managed.EntryPoint",
                ManagedBootstrapEntryPointMethod: "Hook",
                CompanyUniqueIdentifier: "net.devtools.meditation.unittests",
                NativeBootstrapLibraryLoggingPath: "./native-tests.log",
                ManagedBootstrapLibraryLoggingPath: "./managed.tests.log");
            var unhookingConfiguration = hookingConfiguration with { ManagedBootstrapEntryPointMethod = "Unhook" };
            var hookArgs = PatchingConfiguration.ConstructArgs(typeof(EntryPoint).Assembly, hookingConfiguration);
            var unhookArgs = PatchingConfiguration.ConstructArgs(typeof(EntryPoint).Assembly, unhookingConfiguration);
            var processInjector = ServiceProvider.GetRequiredService<IProcessInjector>();
            var processInjecteeExecutor = ServiceProvider.GetRequiredService<IProcessInjecteeExecutor>();
            using var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, ipcTerminationSignaller);

            // Act (module injection)
            using var executionTask = TestSubjectHelpers.GetTestSubjectExecutionCommand(netSdkIdentifier).ExecuteAsync();
            var processId = executionTask.ProcessId;
            var moduleInjectionResult = processInjector.TryInjectModule(processId, hookingConfiguration.NativeBootstrapLibraryPath, out var remoteModuleHandle);
            // Act (code execution - apply patch)
            bool? applyExecutionResult = null;
            uint? applyReturnCode = null;
            if (moduleInjectionResult)
            {
                // Ensure module had time to load properly
                await Task.Delay(TimeSpan.FromSeconds(value: 1));
                // Execute managed hook entrypoint
                using var moduleHandle = remoteModuleHandle;
                applyExecutionResult = processInjecteeExecutor.TryExecuteExportedMethod(
                    processId,
                    hookingConfiguration.NativeBootstrapLibraryPath,
                    moduleHandle!,
                    hookingConfiguration.NativeExportedEntryPointSymbol,
                    hookArgs,
                    out applyReturnCode);
            }
            // Act (code execution - reverse patch)
            bool? reverseExecutionResult = null;
            uint? reverseReturnCode = null;
            if (applyExecutionResult.HasValue && applyExecutionResult.Value)
            {
                // Execute managed hook entrypoint
                using var moduleHandle = remoteModuleHandle;
                reverseExecutionResult = processInjecteeExecutor.TryExecuteExportedMethod(
                    processId,
                    hookingConfiguration.NativeBootstrapLibraryPath,
                    moduleHandle!,
                    hookingConfiguration.NativeExportedEntryPointSymbol,
                    unhookArgs,
                    out reverseReturnCode);
            }
            // Wait for target process to exit gracefully
            waitHandle.Set();
            await executionTask;

            // Assert
            Assert.True(moduleInjectionResult);
            Assert.True(applyExecutionResult);
            Assert.True(applyReturnCode.HasValue);
            Assert.True(applyReturnCode.Value == 0);
            Assert.True(reverseExecutionResult);
            Assert.True(reverseReturnCode.HasValue);
            Assert.True(reverseReturnCode.Value == 0);
        }
    }
}
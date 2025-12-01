using CliWrap;
using Meditation.Bootstrap.Managed;
using Meditation.InjectorService;
using Meditation.TestUtils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;

namespace Meditation.PatchingService.Tests
{
    public class ProcessPatchingTests : TestsBase
    {
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

            // Act
            CommandTask<CommandResult> execution;
            SafeHandle remoteModuleHandle;
            bool? executionResult = null;
            uint? returnCode = null;
            using (var executionController = TestSubjectHelpers.GetTestSubjectExecutionController(netSdkIdentifier))
            {
                // Act (module injection)
                execution = executionController.ExecuteAsync();
                var processId = execution.ProcessId;
                remoteModuleHandle = await processInjector.TryInjectModule(processId, patchingConfiguration.NativeBootstrapLibraryPath);

                // Act (code execution)
                if (!remoteModuleHandle.IsInvalid)
                {
                    // Ensure module had time to load properly
                    await Task.Delay(TimeSpan.FromSeconds(value: 1));
                    // Execute managed hook entrypoint
                    using var moduleHandle = remoteModuleHandle;
                    executionResult = processInjecteeExecutor.TryExecuteExportedMethod(
                        processId,
                        patchingConfiguration.NativeBootstrapLibraryPath,
                        moduleHandle,
                        patchingConfiguration.NativeExportedEntryPointSymbol,
                        hookArgs,
                        out returnCode);
                }
            }
            await TestSubjectHelpers.KillTestSubject(execution);

            // Assert
            Assert.False(remoteModuleHandle.IsInvalid);
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

            // Act
            CommandTask<CommandResult> execution;
            SafeHandle remoteModuleHandle;
            bool? applyExecutionResult = null;
            uint? applyReturnCode = null;
            bool? reverseExecutionResult = null;
            uint? reverseReturnCode = null;
            using (var executionController = TestSubjectHelpers.GetTestSubjectExecutionController(netSdkIdentifier))
            {
                // Act (module injection)
                execution = executionController.ExecuteAsync();
                var processId = execution.ProcessId;
                remoteModuleHandle = await processInjector.TryInjectModule(processId, hookingConfiguration.NativeBootstrapLibraryPath);

                // Act (code execution - apply patch)
                if (!remoteModuleHandle.IsInvalid)
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
            }
            await TestSubjectHelpers.KillTestSubject(execution);

            // Assert
            Assert.False(remoteModuleHandle.IsInvalid);
            Assert.True(applyExecutionResult);
            Assert.True(applyReturnCode.HasValue);
            Assert.True(applyReturnCode.Value == 0);
            Assert.True(reverseExecutionResult);
            Assert.True(reverseReturnCode.HasValue);
            Assert.True(reverseReturnCode.Value == 0);
        }
    }
}
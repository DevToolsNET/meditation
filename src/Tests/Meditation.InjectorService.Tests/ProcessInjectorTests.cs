using Meditation.Interop;
using Meditation.PatchingService;
using Meditation.TestBootstrapManaged;
using Meditation.TestUtils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CliWrap;
using Xunit;

namespace Meditation.InjectorService.Tests
{
    public class ProcessInjectorTests : TestsBase
    {
        [Theory]
        [InlineData("net8.0")]
        public async Task ProcessInjector_InjectSelfToOtherProcess(string netSdkIdentifier)
        {
            // Prepare
            var injectedAssemblyPath = typeof(ProcessInjectorTests).Assembly.Location;
            var processInjector = ServiceProvider.GetRequiredService<IProcessInjector>();

            // Act
            CommandTask<CommandResult> execution;
            SafeHandle moduleHandle;
            using (var executionController = TestSubjectHelpers.GetTestSubjectExecutionController(netSdkIdentifier))
            {
                execution = executionController.ExecuteAsync();
                var processId = execution.ProcessId;
                moduleHandle = await processInjector.TryInjectModule(processId, injectedAssemblyPath);
            }
            await TestSubjectHelpers.KillTestSubject(execution);

            // Assert
            Assert.False(moduleHandle.IsInvalid);
            Assert.False(moduleHandle.IsClosed);
        }

        [Theory]
        [InlineData("net8.0")]
        public async Task ProcessInjecteeExecutor_ExecuteCodeFromNativeModule(string netSdkIdentifier)
        {
            // Prepare
            const string exportedMethod = "MeditationSanityCheck";
            var hookArgument = string.Empty;
            var injectedModulePath = BootstrapNativeHelpers.GetMeditationNativeModulePath();
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
                remoteModuleHandle = await processInjector.TryInjectModule(processId, injectedModulePath);

                // Act (code execution)
                if (!remoteModuleHandle.IsInvalid)
                {
                    using var moduleHandle = remoteModuleHandle;
                    executionResult = processInjecteeExecutor.TryExecuteExportedMethod(
                        processId,
                        injectedModulePath,
                        moduleHandle!,
                        exportedMethod,
                        hookArgument,
                        out returnCode);
                }
            }
            await TestSubjectHelpers.KillTestSubject(execution);

            // Assert
            Assert.False(remoteModuleHandle.IsInvalid);
            Assert.True(executionResult);
            Assert.True(returnCode.HasValue);
            Assert.Equal(0xABCD_EF98, returnCode);
        }

        [Theory]
        [InlineData("net8.0")]
        public async Task ProcessInjecteeExecutor_ExecuteCodeFromManagedAssembly(string netSdkIdentifier)
        {
            // Prepare
            var patchingConfiguration = new PatchingConfiguration(
                PatchAssemblyPath: null!,
                NativeBootstrapLibraryPath: BootstrapNativeHelpers.GetMeditationNativeModulePath(),
                NativeExportedEntryPointSymbol: "MeditationInitialize",
                ManagedBootstrapEntryPointTypeFullName: "Meditation.TestBootstrapManaged.EntryPoint",
                ManagedBootstrapEntryPointMethod: "TestHook",
                CompanyUniqueIdentifier: "net.devtools.meditation.unittests",
                NativeBootstrapLibraryLoggingPath: "native-tests.log",
                ManagedBootstrapLibraryLoggingPath: "managed.tests.log");
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
                        moduleHandle!,
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
            Assert.Equal((uint)NativeHookErrorCode.Ok, returnCode);
        }
    }
}

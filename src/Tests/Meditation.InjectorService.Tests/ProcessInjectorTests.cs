using Meditation.Interop;
using Meditation.PatchingService;
using Meditation.PatchingService.Models;
using Meditation.TestBootstrapManaged;
using Meditation.TestUtils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Meditation.InjectorService.Tests
{
    public class ProcessInjectorTests : TestsBase
    {
        private const string ipcTerminationSignaller = "/meditation/tests-ipc-signal";

        [Theory]
        [InlineData("net8.0")]
#if WINDOWS
        [InlineData("net472")]
#endif
        public async Task ProcessInjector_InjectSelfToOtherProcess(string netSdkIdentifier)
        {
            // Prepare
            var injectedAssemblyPath = typeof(ProcessInjectorTests).Assembly.Location;
            var processInjector = ServiceProvider.GetRequiredService<IProcessInjector>();
            using var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, ipcTerminationSignaller);

            // Act
            using var executionTask = TestSubjectHelpers.GetTestSubjectExecutionCommand(netSdkIdentifier).ExecuteAsync();
            var processId = executionTask.ProcessId;
            var result = processInjector.TryInjectModule(processId, injectedAssemblyPath, out var remoteModuleHandle);
            using var moduleHandle = remoteModuleHandle;
            // Wait for target process to exit gracefully
            waitHandle.Set();
            await executionTask;

            // Assert
            Assert.True(result);
            Assert.False(moduleHandle?.IsInvalid);
            Assert.False(moduleHandle?.IsClosed);
        }

        [Theory]
        [InlineData("net8.0")]
#if WINDOWS
        [InlineData("net472")]
#endif
        public async Task ProcessInjecteeExecutor_ExecuteCodeFromNativeModule(string netSdkIdentifier)
        {
            // Prepare
            const string exportedMethod = "MeditationSanityCheck";
            var hookArgument = string.Empty;
            var injectedModulePath = BootstrapNativeHelpers.GetMeditationNativeModulePath();
            var processInjector = ServiceProvider.GetRequiredService<IProcessInjector>();
            var processInjecteeExecutor = ServiceProvider.GetRequiredService<IProcessInjecteeExecutor>();
            using var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, ipcTerminationSignaller);

            // Act (module injection)
            using var executionTask = TestSubjectHelpers.GetTestSubjectExecutionCommand(netSdkIdentifier).ExecuteAsync();
            var processId = executionTask.ProcessId;
            var moduleInjectionResult = processInjector.TryInjectModule(processId, injectedModulePath, out var remoteModuleHandle);
            // Act (code execution)
            bool? executionResult = null; 
            uint? returnCode = null;
            if (moduleInjectionResult)
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
            // Wait for target process to exit gracefully
            waitHandle.Set();
            await executionTask;

            // Assert
            Assert.True(moduleInjectionResult);
            Assert.True(executionResult);
            Assert.True(returnCode.HasValue);
            Assert.Equal(0xABCD_EF98, returnCode);
        }

        [Theory]
        [InlineData("net8.0")]
#if WINDOWS
        [InlineData("net472")]
#endif
        public async Task ProcessInjecteeExecutor_ExecuteCodeFromManagedAssembly(string netSdkIdentifier)
        {
            // Prepare
            var patchingConfiguration = new PatchingConfiguration(
                PatchInfo: new PatchInfo(
                    Path: string.Empty,
                    TargetFullAssemblyName: string.Empty,
                    Method: null!),
                NativeBootstrapLibraryPath: BootstrapNativeHelpers.GetMeditationNativeModulePath(),
                NativeExportedEntryPointSymbol: "MeditationInitialize",
                ManagedBootstrapEntryPointTypeFullName: "Meditation.TestBootstrapManaged.EntryPoint",
                ManagedBootstrapEntryPointMethod: "TestHook",
                CompanyUniqueIdentifier: "net.devtools.meditation.unittests",
                NativeBootstrapLibraryLoggingPath: "native-tests.log",
                ManagedBootstrapLibraryLoggingPath: "managed.tests.log");
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
            Assert.Equal((uint)NativeHookErrorCode.Ok, returnCode);
        }
    }
}

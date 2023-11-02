using CliWrap;
using Meditation.Interop;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Meditation.InjectorService.Tests
{
    public class ProcessInjectorTests : TestsBase
    {
        [Theory]
        [InlineData("net7.0")]
#if WINDOWS
        [InlineData("net472")]
#endif
        public async Task ProcessInjector_InjectSelfToOtherProcess(string netSdkIdentifier)
        {
            // Prepare
            var injectedAssemblyPath = typeof(ProcessInjectorTests).Assembly.Location;
            var processInjector = ServiceProvider.GetRequiredService<IProcessInjector>();
            var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, TestSubject.TestSubject.SynchronizationHandleName);
            waitHandle.Reset();

            // Act
            var executionTask = GetTestSubjectExecutionCommand(netSdkIdentifier).ExecuteAsync();
            var processId = executionTask.ProcessId;
            var result = processInjector.TryInjectModule(processId, injectedAssemblyPath, out var remoteModuleHandle);
            // Wait for target process to exit gracefully
            waitHandle.Set();
            await executionTask;

            // Assert
            Assert.True(result);
            Assert.False(remoteModuleHandle?.IsInvalid);
            Assert.False(remoteModuleHandle?.IsClosed);
        }

        [Theory]
        [InlineData("net7.0")]
#if WINDOWS
        [InlineData("net472")]
#endif
        public async Task ProcessInjecteeExecutor_ExecuteCodeFromNativeModule(string netSdkIdentifier)
        {
            // Prepare
            const string exportedMethod = "MeditationSanityCheck";
            var hookArgument = string.Empty;
            var injectedModulePath = GetMeditationBootstrapNativeModulePath();
            var processInjector = ServiceProvider.GetRequiredService<IProcessInjector>();
            var processInjecteeExecutor = ServiceProvider.GetRequiredService<IProcessInjecteeExecutor>();
            var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, TestSubject.TestSubject.SynchronizationHandleName);
            waitHandle.Reset();

            // Act (module injection)
            var executionTask = GetTestSubjectExecutionCommand(netSdkIdentifier).ExecuteAsync();
            var processId = executionTask.ProcessId;
            var moduleInjectionResult = processInjector.TryInjectModule(processId, injectedModulePath, out var remoteModuleHandle);
            // Act (code execution)
            bool? executionResult = null; 
            uint? returnCode = null;
            if (moduleInjectionResult)
            {
                executionResult = processInjecteeExecutor.TryExecuteExportedMethod(
                    processId, 
                    injectedModulePath, 
                    remoteModuleHandle!, 
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
            Assert.Equal(0xABCD_EF98, returnCode);
        }

        [Theory]
        [InlineData("net7.0")]
#if WINDOWS
        [InlineData("net472")]
#endif
        public async Task ProcessInjecteeExecutor_ExecuteCodeFromManagedAssembly(string netSdkIdentifier)
        {
            // Prepare
            const string exportedMethod = "MeditationInitialize";
            var hookArgument = $"{GetMeditationBootstrapManagedAssemblyPath()}#Meditation.Bootstrap.Managed.EntryPoint#Hook#calc.exe";
            var injectedModulePath = GetMeditationBootstrapNativeModulePath();
            var processInjector = ServiceProvider.GetRequiredService<IProcessInjector>();
            var processInjecteeExecutor = ServiceProvider.GetRequiredService<IProcessInjecteeExecutor>();
            var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, TestSubject.TestSubject.SynchronizationHandleName);
            waitHandle.Reset();

            // Act (module injection)
            var executionTask = GetTestSubjectExecutionCommand(netSdkIdentifier).ExecuteAsync();
            var processId = executionTask.ProcessId;
            var moduleInjectionResult = processInjector.TryInjectModule(processId, injectedModulePath, out var remoteModuleHandle);
            // Act (code execution)
            bool? executionResult = null;
            uint? returnCode = null;
            if (moduleInjectionResult)
            {
                // Ensure module had time to load properly
                await Task.Delay(TimeSpan.FromSeconds(value: 1));
                // Execute managed hook entrypoint
                executionResult = processInjecteeExecutor.TryExecuteExportedMethod(
                    processId, 
                    injectedModulePath, 
                    remoteModuleHandle!, 
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
            Assert.Equal((uint)ErrorCode.Ok, returnCode);
        }

        private static string GetMeditationBootstrapNativeModulePath()
        {
            const string netSdkIdentifier = "net7.0";
            string runtimeIdentifier;
            string moduleExtension;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.OSArchitecture == Architecture.X64)
            {
                runtimeIdentifier = "win-x64";
                moduleExtension = "dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            return Path.GetFullPath(
                Path.Combine(
                    "../../../../../",
                    "Meditation.Bootstrap.Native",
                    "bin",
                    "Debug",
                    netSdkIdentifier,
                    runtimeIdentifier,
                    "publish",
                    $"Meditation.Bootstrap.Native.{moduleExtension}"));
        }

        private static Command GetTestSubjectExecutionCommand(string netSdkIdentifier)
        {
            var isNetFramework = netSdkIdentifier.StartsWith("net4");
            var extension = isNetFramework ? "exe" : "dll";
            var assemblyPath =  Path.GetFullPath(
                Path.Combine(
                    "../../../../",
                    "Meditation.TestSubject",
                    "bin",
                    "Debug",
                    netSdkIdentifier,
                    $"Meditation.TestSubject.{extension}"));
            var executable = isNetFramework ? assemblyPath : "dotnet";
            var argument = isNetFramework ? string.Empty : assemblyPath;
            return Cli.Wrap(executable).WithArguments(argument);
        }

        private static string GetMeditationBootstrapManagedAssemblyPath()
        {
            const string netVersion = "netstandard2.0";

            return Path.GetFullPath(
                Path.Combine(
                    "../../../../../",
                    "Meditation.Bootstrap.Managed",
                    "bin",
                    "Debug",
                    netVersion,
                    "Meditation.Bootstrap.Managed.dll"));
        }
    }
}

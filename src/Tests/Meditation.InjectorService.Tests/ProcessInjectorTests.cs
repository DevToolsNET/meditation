using CliWrap;
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
        [Fact]
        public async Task ProcessInjector_InjectSelfToOtherProcess()
        {
            // Prepare
            var targetExecutable = typeof(TestSubject.TestSubject).Assembly.Location;
            var assemblyPath = typeof(ProcessInjectorTests).Assembly.Location;
            var processInjector = ServiceProvider.GetRequiredService<IProcessInjector>();
            var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, TestSubject.TestSubject.SynchronizationHandleName);

            // Act
            var executionTask = Cli.Wrap("dotnet")
                .WithArguments(targetExecutable)
                .ExecuteAsync();
            var processId = executionTask.ProcessId;
            var result = processInjector.TryInjectModule(processId, assemblyPath, out var remoteModuleHandle);
            // Wait for target process to exit gracefully
            waitHandle.Set();
            await executionTask;

            // Assert
            Assert.True(result);
            Assert.False(remoteModuleHandle?.IsInvalid);
            Assert.False(remoteModuleHandle?.IsClosed);
        }

        [Fact]
        public async Task ProcessInjecteeExecutor_ExecuteInjectedCode()
        {
            // Prepare
            const string exportedMethod = "MeditationSanityCheck";
            var targetExecutable = typeof(TestSubject.TestSubject).Assembly.Location;
            var injectedModulePath = GetMeditationBootstrapNativeModulePath();
            var processInjector = ServiceProvider.GetRequiredService<IProcessInjector>();
            var processInjecteeExecutor = ServiceProvider.GetRequiredService<IProcessInjecteeExecutor>();
            var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, TestSubject.TestSubject.SynchronizationHandleName);

            // Act (module injection)
            var executionTask = Cli.Wrap("dotnet")
                .WithArguments(targetExecutable)
                .ExecuteAsync();
            var processId = executionTask.ProcessId;
            var moduleInjectionResult = processInjector.TryInjectModule(processId, injectedModulePath, out var remoteModuleHandle);
            // Act (code execution)
            bool? executionResult = null;
            uint? returnCode = null;
            if (moduleInjectionResult)
                executionResult = processInjecteeExecutor.TryExecuteExportedMethod(processId, injectedModulePath, remoteModuleHandle!, exportedMethod, out returnCode);
            // Wait for target process to exit gracefully
            waitHandle.Set();
            await executionTask;

            // Assert
            Assert.True(moduleInjectionResult);
            Assert.True(executionResult);
            Assert.Equal(0xDEAD_C0DE, returnCode);
        }

        private static string GetMeditationBootstrapNativeModulePath()
        {
            const string netVersion = "net7.0";
            string runtimeIdentifier;
            string moduleExtension;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.OSArchitecture == Architecture.X64)
            {
                runtimeIdentifier = "win-x64";
                moduleExtension = "dll";
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            return Path.GetFullPath(
                Path.Combine(
                    "../../../../../",
                    "Meditation.Bootstrap",
                    "bin",
                    "Debug",
                    netVersion,
                    runtimeIdentifier,
                    "publish",
                    $"Meditation.Bootstrap.{moduleExtension}"));
        }
    }
}

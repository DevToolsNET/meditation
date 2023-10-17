using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using Xunit;

namespace Meditation.InjectorService.Tests
{
    public class ProcessInjectorTests : TestsBase
    {
        [Fact]
        public void ProcessInjector_InjectSelfToOtherProcess()
        {
            // Prepare
            const string targetExecutable = "notepad.exe";
            var assemblyPath = typeof(ProcessInjectorTests).Assembly.Location;
            var targetProcess = Process.Start(targetExecutable);
            var processInjector = ServiceProvider.GetRequiredService<IProcessInjector>();

            // Act
            var result = processInjector.TryInjectModuleToProcess(targetProcess.Id, assemblyPath, out var remoteModuleHandle);

            // Assert
            Assert.True(result);
            Assert.False(remoteModuleHandle?.IsInvalid);
            Assert.False(remoteModuleHandle?.IsClosed);
        }
    }
}

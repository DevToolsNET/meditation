using CliWrap;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
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
            var result = processInjector.TryInjectModuleToProcess(processId, assemblyPath, out var remoteModuleHandle);
            waitHandle.Set();
            await executionTask;

            // Assert
            Assert.True(result);
            Assert.False(remoteModuleHandle?.IsInvalid);
            Assert.False(remoteModuleHandle?.IsClosed);
        }
    }
}

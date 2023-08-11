using System;
using System.Linq;
using Meditation.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Meditation.UnitTests.AttachToProcess
{
    public class CommandLineArgumentsProviderTests : TestsBase
    {
        [Fact]
        public void CommandLineArgumentsProvider_ReturnsNonEmptyArgumentsForThisProcess()
        {
            // Prepare
            var pid = Environment.ProcessId;
            var attachableProcessListProvider = ServiceProvider.GetRequiredService<IAttachableProcessListProvider>();
            var commandLineArgumentsProvider = ServiceProvider.GetRequiredService<ICommandLineArgumentsProvider>();

            // Act
            var process = attachableProcessListProvider.GetAllAttachableProcesses().Single(p => p.Id == pid).Internal;
            var result = commandLineArgumentsProvider.TryGetCommandLineArguments(process, out var commandLineArguments);

            // Assert
            Assert.True(result);
            Assert.NotNull(commandLineArguments);
            Assert.NotEmpty(commandLineArguments);
        }
    }
}

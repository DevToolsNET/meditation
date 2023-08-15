using System;
using System.Linq;
using System.Threading.Tasks;
using Meditation.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Meditation.UnitTests.AttachToProcess
{
    public class ProcessCommandLineProviderTests : TestsBase
    {
        [Fact]
        public async Task ProcessCommandLineProvider_ReturnsNonEmptyArgumentsForThisProcess()
        {
            // Prepare
            var pid = Environment.ProcessId;
            var attachableProcessListProvider = ServiceProvider.GetRequiredService<IAttachableProcessListProvider>();
            var commandLineArgumentsProvider = ServiceProvider.GetRequiredService<IProcessCommandLineProvider>();

            // Act
            var process = (await attachableProcessListProvider.GetAllAttachableProcessesAsync()).Single(p => p.Id == pid).Internal;
            var result = commandLineArgumentsProvider.TryGetCommandLineArguments(process, out var commandLineArguments);

            // Assert
            Assert.True(result);
            Assert.NotNull(commandLineArguments);
            Assert.NotEmpty(commandLineArguments);
        }
    }
}

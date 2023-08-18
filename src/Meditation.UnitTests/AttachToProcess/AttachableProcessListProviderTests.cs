using Meditation.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Meditation.UnitTests.AttachToProcess
{
    public class AttachableProcessListProviderTests : TestsBase
    {
        [Fact]
        public async Task AttachableProcessListProvider_ReturnsNonEmptyList()
        {
            // Prepare
            var attachableProcessListProvider = ServiceProvider.GetRequiredService<IAttachableProcessListProvider>();

            // Act
            var processes = await attachableProcessListProvider.GetAllAttachableProcessesAsync(CancellationToken.None);

            // Assert
            Assert.NotEmpty(processes);
            Assert.True(processes.Any(p => p.Id == Environment.ProcessId));
        }
    }
}

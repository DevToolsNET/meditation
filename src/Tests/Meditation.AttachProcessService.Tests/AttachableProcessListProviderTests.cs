using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Meditation.AttachProcessService.Models;
using Xunit;

namespace Meditation.AttachProcessService.Tests
{
    public class AttachableProcessListProviderTests : TestsBase
    {
        [Fact]
        public async Task AttachableProcessListProvider_ReturnsNonEmptyList()
        {
            // Prepare
            var pid = new ProcessId(Environment.ProcessId);
            var attachableProcessListProvider = ServiceProvider.GetRequiredService<IAttachableProcessesAggregator>();

            // Act
            var processes = await attachableProcessListProvider.GetAttachableProcessesAsync(CancellationToken.None);

            // Assert
            Assert.NotEmpty(processes);
            Assert.True(processes.Any(p => p.Id == pid));
        }
    }
}

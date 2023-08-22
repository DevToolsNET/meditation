using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Meditation.AttachProcessService.Tests
{
    public class AttachableProcessListProviderTests : TestsBase
    {
        [Fact]
        public async Task AttachableProcessListProvider_ReturnsNonEmptyList()
        {
            // Prepare
            var attachableProcessListProvider = ServiceProvider.GetRequiredService<IAttachableProcessListProvider>();

            // Act
            var processes = await attachableProcessListProvider.GetAttachableProcessesAsync(CancellationToken.None);

            // Assert
            Assert.NotEmpty(processes);
            Assert.True(processes.Any(p => p.Id == Environment.ProcessId));
        }
    }
}

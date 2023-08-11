using Meditation.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;

namespace Meditation.UnitTests.AttachToProcess
{
    public class AttachableProcessListProviderTests : TestsBase
    {
        [Fact]
        public void AttachableProcessListProvider_ReturnsNonEmptyList()
        {
            // Prepare
            var attachableProcessListProvider = ServiceProvider.GetRequiredService<IAttachableProcessListProvider>();

            // Act
            var processes = attachableProcessListProvider.GetAllAttachableProcesses();

            // Assert
            Assert.NotEmpty(processes);
            Assert.True(processes.Any(p => p.Id == Environment.ProcessId));
        }
    }
}

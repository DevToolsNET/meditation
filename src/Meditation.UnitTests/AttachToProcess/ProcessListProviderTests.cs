using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Meditation.Common.Services;
using Xunit;

namespace Meditation.UnitTests.AttachToProcess
{
    public class ProcessListProviderTests : TestsBase
    {
        [Fact]
        public void ProcessListProvider_ReturnsNonEmptyList()
        {
            // Prepare
            var provider = ServiceProvider.GetRequiredService<IProcessListProvider>();

            // Act
            var processes = provider.GetAllProcesses();

            // Assert
            Assert.NotEmpty(processes);
            Assert.True(processes.Any(p => p.Id == Environment.ProcessId));
        }
    }
}
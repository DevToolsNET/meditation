using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Meditation.AttachProcessService.Models;
using Xunit;

namespace Meditation.AttachProcessService.Tests
{
    public class ProcessListProviderTests : TestsBase
    {
        [Fact]
        public void ProcessListProvider_ReturnsNonEmptyList()
        {
            // Prepare
            var pid = new ProcessId(Environment.ProcessId);
            var provider = ServiceProvider.GetRequiredService<IProcessListProvider>();

            // Act
            var processes = provider.GetAllProcesses();

            // Assert
            Assert.NotEmpty(processes);
            Assert.True(processes.Any(p => p.Id == pid));
        }
    }
}
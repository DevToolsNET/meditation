using Meditation.Core.Services;
using Xunit;

namespace Meditation.UnitTests
{
    public class ProcessListProviderTests
    {
        [Fact]
        public void ProcessListProvider_ReturnsNonEmptyList()
        {
            // Prepare
            var provider = new ProcessListProvider();

            // Act
            var processes = provider.GetAllProcesses();

            // Assert
            Assert.NotEmpty(processes);
            Assert.True(processes.Any(p => p.Id == Environment.ProcessId));
        }
    }
}
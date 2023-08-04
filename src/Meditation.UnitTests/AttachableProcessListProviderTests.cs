using Meditation.Core.Services;
using Xunit;

namespace Meditation.UnitTests
{
    public class AttachableProcessListProviderTests
    {
        [Fact]
        public void AttachableProcessListProvider_ReturnsNonEmptyList()
        {
            // Prepare
            var processListProvider = new ProcessListProvider();
            var attachableProcessListProvider = new AttachableProcessListProvider(processListProvider);

            // Act
            var processes = attachableProcessListProvider.GetAllAttachableProcesses();

            // Assert
            Assert.NotEmpty(processes);
            Assert.True(processes.Any(p => p.Id == Environment.ProcessId));
        }
    }
}

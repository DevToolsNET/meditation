using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Meditation.AttachProcessService.Tests
{
    public class ProcessSnapshotCreatorTests : TestsBase
    {
        [Fact]
        public async Task ProcessSnapshotCreator_ReturnsSnapshotForDotNetProcess()
        {
            // Prepare
            var processId = Environment.ProcessId;
            var snapshotCreator = ServiceProvider.GetRequiredService<IProcessSnapshotCreator>();

            // Act
            using var snapshot = await snapshotCreator.CreateProcessSnapshotAsync(processId, CancellationToken.None);

            // Assert
            Assert.NotNull(snapshot);
            Assert.Equal(processId, snapshot.ProcessId);
            Assert.NotEmpty(snapshot.GetModules());
        }
    }
}

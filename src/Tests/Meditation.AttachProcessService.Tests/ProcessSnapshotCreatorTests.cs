using System;
using System.Threading;
using System.Threading.Tasks;
using Meditation.AttachProcessService.Models;
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
            var processId = new ProcessId(Environment.ProcessId);
            var snapshotCreator = ServiceProvider.GetRequiredService<IProcessSnapshotCreator>();

            // Act
            using var snapshot = await snapshotCreator.CreateProcessSnapshotAsync(processId, CancellationToken.None);

            // Assert
            Assert.NotNull(snapshot);
            Assert.Equal(processId, snapshot.ProcessId);
            Assert.NotEmpty(snapshot.EnumerateModules());
        }
    }
}

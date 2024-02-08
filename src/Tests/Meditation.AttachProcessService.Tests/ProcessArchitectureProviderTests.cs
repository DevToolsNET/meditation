using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Meditation.AttachProcessService.Models;
using Xunit;

namespace Meditation.AttachProcessService.Tests
{
    public class ProcessArchitectureProviderTests : TestsBase
    {
        [Fact]
        public async Task ProcessArchitectureProvider_ReturnsCorrectArchitectureForCurrentProcess()
        {
            // Prepare
            var pid = new ProcessId(Environment.ProcessId);
            var currentArchitecture = RuntimeInformation.ProcessArchitecture;
            var attachableProcessListProvider = ServiceProvider.GetRequiredService<IAttachableProcessesAggregator>();
            var architectureProvider = ServiceProvider.GetRequiredService<IProcessArchitectureProvider>();

            // Act
            var process = (await attachableProcessListProvider.GetAttachableProcessesAsync(CancellationToken.None)).Single(p => p.Id == pid).InternalProcess;
            var result = await architectureProvider.TryGetProcessArchitectureAsync(process, out var architecture, CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Equal(currentArchitecture, architecture);
        }
    }
}

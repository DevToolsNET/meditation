using Meditation.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;

namespace Meditation.UnitTests.AttachToProcess
{
    public class ProcessArchitectureProviderTests : TestsBase
    {
        [Fact]
        public async Task ProcessArchitectureProvider_ReturnsCorrectArchitectureForCurrentProcess()
        {
            // Prepare
            var pid = Environment.ProcessId;
            var currentArchitecture = RuntimeInformation.ProcessArchitecture;
            var attachableProcessListProvider = ServiceProvider.GetRequiredService<IAttachableProcessListProvider>();
            var architectureProvider = ServiceProvider.GetRequiredService<IProcessArchitectureProvider>();

            // Act
            var process = (await attachableProcessListProvider.GetAllAttachableProcessesAsync()).Single(p => p.Id == pid).Internal;
            var result = architectureProvider.TryGetProcessArchitecture(process, out var architecture);

            // Assert
            Assert.True(result);
            Assert.Equal(currentArchitecture, architecture);
        }
    }
}

using Meditation.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;

namespace Meditation.UnitTests.AttachToProcess
{
    public class ProcessArchitectureProviderTests : TestsBase
    {
        [Fact]
        public void ProcessArchitectureProvider_ReturnsCorrectArchitectureForCurrentProcess()
        {
            // Prepare
            var pid = Environment.ProcessId;
            var currentArchitecture = RuntimeInformation.ProcessArchitecture;
            var attachableProcessListProvider = ServiceProvider.GetRequiredService<IAttachableProcessListProvider>();
            var architectureProvider = ServiceProvider.GetRequiredService<IProcessArchitectureProvider>();

            // Act
            var process = attachableProcessListProvider.GetAllAttachableProcesses().Single(p => p.Id == pid).Internal;
            var result = architectureProvider.TryGetProcessArchitecture(process, out var architecture);

            // Assert
            Assert.True(result);
            Assert.Equal(currentArchitecture, architecture);
        }
    }
}

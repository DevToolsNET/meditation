﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Meditation.AttachProcessService.Models;
using Xunit;

namespace Meditation.AttachProcessService.Tests
{
    public class ProcessCommandLineProviderTests : TestsBase
    {
        [Fact]
        public async Task ProcessCommandLineProvider_ReturnsNonEmptyArgumentsForThisProcess()
        {
            // Prepare
            var pid = new ProcessId(Environment.ProcessId);
            var attachableProcessListProvider = ServiceProvider.GetRequiredService<IAttachableProcessesAggregator>();
            var commandLineArgumentsProvider = ServiceProvider.GetRequiredService<IProcessCommandLineProvider>();

            // Act
            var process = (await attachableProcessListProvider.GetAttachableProcessesAsync(CancellationToken.None)).Single(p => p.Id == pid).InternalProcess;
            var result = await commandLineArgumentsProvider.TryGetCommandLineArgumentsAsync(process, out var commandLineArguments, CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.NotNull(commandLineArguments);
            Assert.NotEmpty(commandLineArguments);
        }
    }
}

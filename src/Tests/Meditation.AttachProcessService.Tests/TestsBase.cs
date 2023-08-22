using Meditation.AttachProcessService.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Meditation.AttachProcessService.Tests
{
    public abstract class TestsBase
    {
        protected readonly IServiceProvider ServiceProvider;

        protected TestsBase()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMeditationAttachProcessService();
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}

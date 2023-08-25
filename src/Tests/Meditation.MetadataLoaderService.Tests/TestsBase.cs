using Microsoft.Extensions.DependencyInjection;
using System;
using Meditation.MetadataLoaderService.Configuration;

namespace Meditation.MetadataLoaderService.Tests
{
    public abstract class TestsBase
    {
        protected readonly IServiceProvider ServiceProvider;

        protected TestsBase()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMeditationMetadataLoaderServices();
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}

using Meditation.InjectorService.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Meditation.PatchingService.Configuration;

namespace Meditation.PatchingService.Tests
{
    public abstract class TestsBase
    {
        protected readonly IServiceProvider ServiceProvider;

        protected TestsBase()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMeditationInjectorServices();
            serviceCollection.AddMeditationPatchingServices();
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}

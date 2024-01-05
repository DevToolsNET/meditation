using Meditation.InjectorService.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Meditation.InjectorService.Tests
{
    public abstract class TestsBase
    {
        protected readonly IServiceProvider ServiceProvider;

        protected TestsBase()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMeditationInjectorServices();
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}

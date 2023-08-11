using System;
using Meditation.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Meditation.UnitTests
{
    public abstract class TestsBase
    {
        protected readonly IServiceProvider ServiceProvider;

        protected TestsBase()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMeditationCore();
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Meditation.UI.Utilities
{
    public static class ServiceProviderExtensions
    {
        public static T CreateInstance<T>(this IServiceProvider serviceProvider, params object[] additionalArgs)
        {
            return ActivatorUtilities.CreateInstance<T>(serviceProvider, additionalArgs);
        }
    }
}

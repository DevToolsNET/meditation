using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;

namespace Meditation.UI.Utilities
{
    public static class ResourceHostExtensions
    {
        public static IServiceProvider GetServiceProvider(this IResourceHost control)
        {
            // FIXME [#16]: there should be a nicer way to get this
            var serviceProvider = Application.Current?.Resources[typeof(IServiceProvider)] as IServiceProvider;
            return serviceProvider ?? throw new InvalidAsynchronousStateException($"Could not find a registered instance of {nameof(IServiceProvider)} in resources");
        }
    }
}

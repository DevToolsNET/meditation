using Meditation.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Meditation.UI.Configuration
{
    public static class ConfigurationExtensions
    {
        public static void AddMeditationUserInterfaceServices(this IServiceCollection services)
        {
            services.AddSingleton<IAvaloniaStorageProvider, AvaloniaStorageProvider>();

            var eventsHub = new UserInterfaceEventsHub();
            services.AddSingleton<IUserInterfaceEventsConsumer>(eventsHub);
            services.AddSingleton<IUserInterfaceEventsRaiser>(eventsHub);
        }
    }
}

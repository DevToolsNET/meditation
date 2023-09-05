using Meditation.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Meditation.UI.Configuration
{
    public static class ConfigurationExtensions
    {
        public static void AddMeditationUserInterfaceServices(this IServiceCollection services)
        {
            services.AddSingleton<IAvaloniaStorageProvider, AvaloniaStorageProvider>();
            services.AddSingleton<IAvaloniaDialogsContext, AvaloniaDialogContext>();

            var attachableProcessContext = new AttachedProcessContext();
            services.AddSingleton<IAttachedProcessProvider>(attachableProcessContext);
            services.AddSingleton<IAttachedProcessController>(attachableProcessContext);
        }
    }
}

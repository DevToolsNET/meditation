using Meditation.MetadataLoaderService.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Meditation.MetadataLoaderService.Configuration
{
    public static class ConfigurationExtensions
    {
        public static void AddMeditationMetadataLoaderServices(this IServiceCollection services)
        {
            services.AddScoped<IMetadataLoader, MetadataLoader>();
        }
    }
}

using Meditation.MetadataLoaderService.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Meditation.MetadataLoaderService.Configuration
{
    public static class ConfigurationExtensions
    {
        public static void AddMeditationMetadataLoaderServices(this IServiceCollection services)
        {
            services.AddScoped<IMetadataLoaderInternal, MetadataLoader>();
            services.AddScoped<IMetadataLoader>(p => p.GetRequiredService<IMetadataLoaderInternal>());
            services.AddScoped<IDependencyResolver, DependencyResolver>();
        }
    }
}

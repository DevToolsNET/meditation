using Meditation.PatchingService.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Meditation.PatchingService.Configuration
{
    public static class ConfigurationExtensions
    {
        public static void AddMeditationPatchingServices(this IServiceCollection services)
        {
            services.AddSingleton<IPatchListProvider, PatchListProvider>();
            services.AddSingleton<IPatchStorage, PatchStorage>();
            services.AddSingleton<IPatchApplier, PatchApplier>();
            services.AddSingleton<IPatchReverser, PatchReverser>();
        }
    }
}

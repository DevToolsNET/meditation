using Microsoft.Extensions.DependencyInjection;

namespace Meditation.CompilationService.Configuration
{
    public static class ConfigurationExtensions
    {
        public static void AddMeditationCompilationServices(this IServiceCollection services)
        {
            services.AddSingleton<ICompilationService, Services.CompilationService>();
        }
    }
}

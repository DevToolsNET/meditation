using Meditation.CompilationService.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Meditation.CompilationService.Configuration
{
    public static class ConfigurationExtensions
    {
        public static void AddMeditationCompilationServices(this IServiceCollection services)
        {
            services.AddTransient<ICompilationService, Services.CompilationService>();
            services.AddSingleton<ICodeTemplateProvider, CodeTemplateProvider>();
        }
    }
}

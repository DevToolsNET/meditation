using Meditation.Common.Services;
using Meditation.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Meditation.Core.Configuration
{
    public static class ConfigurationExtensions
    {
        public static void AddMeditationCore(this IServiceCollection services)
        {
            services.AddTransient<IProcessListProvider, ProcessListProvider>();
            services.AddTransient<IAttachableProcessListProvider, AttachableProcessListProvider>();
        }
    }
}

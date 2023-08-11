using System;
using System.Runtime.InteropServices;
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
            services.AddPlatformSpecificServices();
        }

        private static void AddPlatformSpecificServices(this IServiceCollection services)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                services.AddWindowsServices();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                services.AddLinuxServices();
            else
                throw new PlatformNotSupportedException(RuntimeInformation.OSDescription);
        }

        private static void AddWindowsServices(this IServiceCollection services)
        {
            services.AddSingleton<ICommandLineArgumentsProvider, WindowsCommandLineArgumentsProvider>();
        }

        private static void AddLinuxServices(this IServiceCollection services)
        {
            throw new NotImplementedException(OSPlatform.Linux.ToString());
        }
    }
}

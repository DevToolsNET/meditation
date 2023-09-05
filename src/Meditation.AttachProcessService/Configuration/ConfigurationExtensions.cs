using Meditation.AttachProcessService.Services;
using Meditation.AttachProcessService.Services.Windows;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.InteropServices;

namespace Meditation.AttachProcessService.Configuration
{
    public static class ConfigurationExtensions
    {
        public static void AddMeditationAttachProcessService(this IServiceCollection services)
        {
            services.AddSingleton<IProcessListProvider, ProcessListProvider>();
            services.AddTransient<IAttachableProcessListProvider, AttachableNetCoreProcessListProvider>();
            services.AddTransient<IAttachableProcessesAggregator, AttachableProcessListAggregator>();
            services.AddTransient<IProcessSnapshotCreator, ProcessSnapshotCreator>();
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
            services.AddSingleton<IProcessCommandLineProvider, WindowsProcessCommandLineProvider>();
            services.AddSingleton<IProcessArchitectureProvider, WindowsProcessArchitectureProvider>();
            services.AddTransient<IAttachableProcessListProvider, AttachableNetFrameworkProcessListProvider>();
        }

        private static void AddLinuxServices(this IServiceCollection services)
        {
            throw new NotImplementedException(OSPlatform.Linux.ToString());
        }
    }
}

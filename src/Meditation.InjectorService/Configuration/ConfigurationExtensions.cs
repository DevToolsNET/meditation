using Meditation.InjectorService.Services.Windows;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.InteropServices;
using Meditation.InjectorService.Services.Linux;

namespace Meditation.InjectorService.Configuration
{
    public static class ConfigurationExtensions
    {
        public static void AddMeditationInjectorServices(this IServiceCollection services)
        {
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
            services.AddTransient<IProcessInjector, WindowsProcessInjector>();
            services.AddTransient<IProcessInjecteeExecutor, WindowsProcessInjecteeExecutor>();
        }

        private static void AddLinuxServices(this IServiceCollection services)
        {
            services.AddTransient<IProcessInjector, LinuxProcessInjector>();
        }
    }
}
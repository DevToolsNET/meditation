using Meditation.UI.Controllers;
using Meditation.UI.Services;
using Meditation.UI.Services.Dialogs;
using Meditation.UI.Services.Patches;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Meditation.UI.Services.Windows;

namespace Meditation.UI.Configuration
{
    public static class ConfigurationExtensions
    {
        public static void AddMeditationUserInterfaceServices(this IServiceCollection services)
        {
            services.ConfigureServices();
            services.AddSingleton<IAvaloniaStorageProvider, AvaloniaStorageProvider>();
            services.AddSingleton<IAvaloniaDialogService, AvaloniaDialogService>();
            services.AddSingleton<IAttachedProcessContext, AttachedProcessContext>();
            services.AddSingleton<IWorkspaceContext, WorkspaceContext>();
            services.AddSingleton<IPatchViewModelBuilder, PatchViewModelBuilder>();
            services.AddSingleton<AttachToProcessController>();
            services.AddSingleton<DevelopmentEnvironmentController>();
            services.AddSingleton<PatchProcessController>();
            services.AddSingleton<InputTextDialogController>();
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

        [SupportedOSPlatform("windows")]
        private static void AddWindowsServices(this IServiceCollection services)
        {
            services.AddSingleton<IPrivilegeElevatorService, WindowsPrivilegeElevatorService>();
        }

        private static void AddLinuxServices(this IServiceCollection services)
        {
            throw new NotImplementedException(OSPlatform.Linux.ToString());
        }

        private static void ConfigureServices(this IServiceCollection services)
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddYamlFile("appsettings.yml");
            var configuration = configurationBuilder.Build();
            var appConfiguration = configuration.Get<ApplicationConfiguration>() 
                                   ?? throw new ArgumentException("File appsettings.yml is malformed.");
            appConfiguration.Validate();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton(appConfiguration);
        }
    }
}

using Meditation.UI.Controllers;
using Meditation.UI.Services;
using Meditation.UI.Services.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

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
            services.AddSingleton<AttachToProcessController>();
            services.AddSingleton<DevelopmentEnvironmentController>();
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

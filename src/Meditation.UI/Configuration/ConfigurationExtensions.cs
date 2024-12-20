using Meditation.UI.Controllers;
using Meditation.UI.Services;
using Meditation.UI.Services.Dialogs;
using Meditation.UI.Services.Patches;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Meditation.UI.Configuration
{
    public static class ConfigurationExtensions
    {
        private const string AppConfigurationFile = "appsettings.yml";

        public static void AddMeditationUserInterfaceServices(this IServiceCollection services)
        {
            var appConfiguration = services.ConfigureServices();
            services.ConfigureLogging(appConfiguration);
            services.AddSingleton<IAvaloniaStorageProvider, AvaloniaStorageProvider>();
            services.AddSingleton<IAvaloniaDialogService, AvaloniaDialogService>();
            services.AddSingleton<IAttachedProcessContext, AttachedProcessContext>();
            services.AddSingleton<IWorkspaceContext, WorkspaceContext>();
            services.AddSingleton<IPatchViewModelBuilder, PatchViewModelBuilder>();
            services.AddSingleton<AttachToProcessController>();
            services.AddSingleton<DevelopmentEnvironmentController>();
            services.AddSingleton<PatchProcessController>();
            services.AddSingleton<InputTextDialogController>();
        }

        private static ApplicationConfiguration ConfigureServices(this IServiceCollection services)
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddYamlFile(AppConfigurationFile);
            var configuration = configurationBuilder.Build();
            var appConfiguration = configuration.Get<ApplicationConfiguration>() 
                                   ?? throw new ArgumentException($"File {AppConfigurationFile} is malformed.");
            appConfiguration.Validate();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton(appConfiguration);
            return appConfiguration;
        }

        private static void ConfigureLogging(this IServiceCollection services, ApplicationConfiguration configuration)
        {
            services.ConfigureServices();
            services.AddLogging(builder =>
            {
                var mainLogFileName = Path.GetFileNameWithoutExtension(configuration.Logging.MainApplicationFileName);
                var mainLogDirectory = Path.GetDirectoryName(configuration.Logging.MainApplicationFileName);
                builder.AddFile(options =>
                {
                    options.FileName = mainLogFileName;
                    options.LogDirectory = mainLogDirectory;
                    options.Extension = "log";
                    options.FileSizeLimit = 10 * 1024 * 1024; /* 10 MB */
                    options.RetainedFileCountLimit = 10;
                });
                builder.SetMinimumLevel(LogLevel.Debug);
            });
        }
    }
}

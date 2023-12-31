﻿using Meditation.UI.Controllers;
using Meditation.UI.Services;
using Meditation.UI.Services.Dialogs;
using Meditation.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Meditation.UI.Configuration
{
    public static class ConfigurationExtensions
    {
        public static void AddMeditationUserInterfaceServices(this IServiceCollection services)
        {
            services.AddSingleton<IAvaloniaStorageProvider, AvaloniaStorageProvider>();
            services.AddSingleton<IAvaloniaDialogService, AvaloniaDialogService>();
            services.AddSingleton<IAttachedProcessContext, AttachedProcessContext>();
            services.AddSingleton<AttachToProcessController>();
        }
    }
}

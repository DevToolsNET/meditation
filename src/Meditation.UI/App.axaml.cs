using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Meditation.AttachProcessService.Configuration;
using Meditation.MetadataLoaderService.Configuration;
using Meditation.UI.Configuration;
using Meditation.UI.Utilities;
using Meditation.UI.ViewModels;
using Meditation.UI.Windows;
using Microsoft.Extensions.DependencyInjection;
using System;
using Meditation.CompilationService.Configuration;

namespace Meditation.UI
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            InitializeDependencyInjection();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            UseCommunityToolkitValidation();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = this.GetServiceProvider().CreateInstance<MainViewModel>()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void InitializeDependencyInjection()
        {
            // In order to make services container visible to all views, store it in resources
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMeditationAttachProcessService();
            serviceCollection.AddMeditationMetadataLoaderServices();
            serviceCollection.AddMeditationCompilationServices();
            serviceCollection.AddMeditationUserInterfaceServices();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            Resources[typeof(IServiceProvider)] = serviceProvider;
        }

        private static void UseCommunityToolkitValidation()
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            // Note: Avalonia registers its data validators first, thus the magic value zero

            BindingPlugins.DataValidators.RemoveAt(0);
        }
    }
}

using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Meditation.Core.Configuration;
using Meditation.UI.ViewModels;
using Meditation.UI.Views;
using Meditation.UI.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Meditation.UI
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            InitializeDependencyInjection();
        }

        private void InitializeDependencyInjection()
        {
            // In order to make services container visible to all views, store it in resources
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMeditationCore();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            Resources[typeof(IServiceProvider)] = serviceProvider;
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainViewModel()
                };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new MainView
                {
                    DataContext = new MainViewModel()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}

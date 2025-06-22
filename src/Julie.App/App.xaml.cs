using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Julie.Core.API;
using Julie.Core.Audio;
using Julie.Core.Screen;
using Julie.Core.Utils;
using Julie.Services;
using Julie.UI.ViewModels;
using Julie.UI.Views;

namespace Julie.App
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                ConfigureServices();
                
                // Create and show main window
                var mainViewModel = GetService<MainViewModel>();
                var mainWindow = new MainWindow(mainViewModel);
                mainWindow.Show();
                
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application startup error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        /// <summary>
        /// Configures dependency injection services
        /// </summary>
        private void ConfigureServices()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Core services
                    services.AddSingleton<SettingsManager>();
                    services.AddSingleton<LiveApiClient>();
                    services.AddTransient<AudioCapture>();
                    services.AddTransient<AudioPlayer>();
                    services.AddTransient<ScreenCapture>();

                    // Application services
                    services.AddSingleton<IJulieService, JulieService>();
                    services.AddSingleton<IAudioService, AudioService>();
                    services.AddSingleton<IScreenService, ScreenService>();

                    // ViewModels
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<ChatViewModel>();
                    services.AddTransient<SettingsViewModel>();

                    // Logging
                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                        builder.AddDebug();
                        builder.SetMinimumLevel(LogLevel.Information);
                    });
                })
                .Build();

            // Start the host
            _host.Start();
        }

        /// <summary>
        /// Gets a service from the dependency injection container
        /// </summary>
        public T GetService<T>() where T : class
        {
            try
            {
                return _host?.Services.GetRequiredService<T>() ?? throw new InvalidOperationException("Service not found");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get service {typeof(T).Name}: {ex.Message}", ex);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host?.Dispose();
            base.OnExit(e);
        }
    }
}

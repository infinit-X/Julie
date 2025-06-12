using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
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

        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            ConfigureServices();
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

                    // Views
                    services.AddTransient<MainWindow>();

                    // Logging
                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                        builder.AddDebug();
                        builder.SetMinimumLevel(LogLevel.Information);
                    });
                })
                .Build();
        }

        /// <summary>
        /// Gets a service from the dependency injection container
        /// </summary>
        public static T GetService<T>() where T : class
        {
            var app = (App)Current;
            return app._host?.Services.GetRequiredService<T>() ?? throw new InvalidOperationException("Service not found");
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var mainWindow = GetService<MainWindow>();
            mainWindow.Activate();
        }

        /// <summary>
        /// Invoked when the application is about to exit
        /// </summary>
        protected override void OnClosed(object sender, Microsoft.UI.Xaml.WindowEventArgs args)
        {
            _host?.Dispose();
            base.OnClosed(sender, args);
        }
    }
}

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Julie.Core.Models;
using Julie.Services;
using Microsoft.Extensions.Logging;

namespace Julie.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the main Julie window
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly IJulieService _julieService;
        private readonly ILogger<MainViewModel> _logger;

        [ObservableProperty]
        private bool _isConnected;

        [ObservableProperty]
        private bool _isSpeaking;

        [ObservableProperty]
        private bool _isListening;

        [ObservableProperty]
        private string _connectionStatus = "Disconnected";

        [ObservableProperty]
        private ChatViewModel _chatViewModel;

        [ObservableProperty]
        private SettingsViewModel _settingsViewModel;

        [ObservableProperty]
        private bool _isSettingsVisible;

        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand ToggleListeningCommand { get; }
        public ICommand ShowSettingsCommand { get; }
        public ICommand HideSettingsCommand { get; }
        public ICommand NewConversationCommand { get; }        public MainViewModel(
            IJulieService julieService,
            ChatViewModel chatViewModel,
            SettingsViewModel settingsViewModel,
            ILogger<MainViewModel> logger)
        {
            _julieService = julieService;
            _chatViewModel = chatViewModel;
            _settingsViewModel = settingsViewModel;
            _logger = logger;

            // Initialize commands
            ConnectCommand = new AsyncRelayCommand(ConnectAsync);
            DisconnectCommand = new AsyncRelayCommand(DisconnectAsync);
            ToggleListeningCommand = new AsyncRelayCommand(ToggleListeningAsync);
            ShowSettingsCommand = new RelayCommand(() => IsSettingsVisible = true);
            HideSettingsCommand = new RelayCommand(() => IsSettingsVisible = false);
            NewConversationCommand = new AsyncRelayCommand(StartNewConversationAsync);

            // Subscribe to Julie service events
            _julieService.ConnectionStatusChanged += OnConnectionStatusChanged;
            _julieService.IsSpeakingChanged += OnIsSpeakingChanged;
            _julieService.MessageReceived += OnMessageReceived;

            // Subscribe to settings events
            _settingsViewModel.OnSettingsSaved += OnSettingsSaved;

            // Initialize with settings
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing MainViewModel");
                  // Load settings and initialize Julie service
                await SettingsViewModel.LoadSettingsAsync();
                var settings = SettingsViewModel.Settings;
                
                if (settings != null)
                {
                    await _julieService.InitializeAsync(settings);
                }

                UpdateConnectionStatus();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize MainViewModel");
            }
        }

        private async Task ConnectAsync()
        {
            try
            {
                _logger.LogInformation("Connecting to Julie service");
                
                var settings = SettingsViewModel.Settings;
                if (settings == null || string.IsNullOrEmpty(settings.ApiKey))
                {
                    _logger.LogWarning("No API key configured");
                    IsSettingsVisible = true;
                    return;
                }

                await _julieService.InitializeAsync(settings);
                UpdateConnectionStatus();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to Julie service");
                ConnectionStatus = $"Connection failed: {ex.Message}";
            }
        }

        private async Task DisconnectAsync()
        {
            try
            {
                _logger.LogInformation("Disconnecting from Julie service");
                await _julieService.ShutdownAsync();
                UpdateConnectionStatus();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to disconnect from Julie service");
            }
        }

        private async Task ToggleListeningAsync()
        {
            try
            {
                if (IsListening)
                {
                    // Stop listening logic would go here
                    IsListening = false;
                    _logger.LogInformation("Stopped listening");
                }
                else
                {
                    // Start listening logic would go here
                    IsListening = true;
                    _logger.LogInformation("Started listening");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle listening state");
            }
        }

        private async Task StartNewConversationAsync()
        {
            try
            {
                _logger.LogInformation("Starting new conversation");
                var conversation = await _julieService.StartNewConversationAsync();
                ChatViewModel.LoadConversation(conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start new conversation");
            }
        }

        private void OnConnectionStatusChanged(object? sender, bool isConnected)
        {
            IsConnected = isConnected;
            UpdateConnectionStatus();
        }

        private void OnIsSpeakingChanged(object? sender, bool isSpeaking)
        {
            IsSpeaking = isSpeaking;
        }

        private void OnMessageReceived(object? sender, Message message)
        {
            ChatViewModel.AddMessage(message);
        }

        private void UpdateConnectionStatus()
        {
            ConnectionStatus = IsConnected ? "Connected" : "Disconnected";
        }        private async void OnSettingsSaved()
        {
            try
            {
                // Check if we have a valid API key and try to connect
                if (!string.IsNullOrEmpty(SettingsViewModel.ApiKey))
                {
                    ConnectionStatus = "Connecting...";
                    await ConnectAsync();
                }
                else
                {
                    ConnectionStatus = "No API Key";
                    IsConnected = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect after settings saved");
                ConnectionStatus = "Connection Failed";
                IsConnected = false;
            }
        }

        public void Cleanup()
        {
            try
            {
                _julieService.ConnectionStatusChanged -= OnConnectionStatusChanged;
                _julieService.IsSpeakingChanged -= OnIsSpeakingChanged;
                _julieService.MessageReceived -= OnMessageReceived;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup");
            }
        }
    }
}

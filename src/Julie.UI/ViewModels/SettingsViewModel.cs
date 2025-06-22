using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Julie.Core.Models;
using Julie.Core.Utils;
using Microsoft.Extensions.Logging;

namespace Julie.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the settings dialog
    /// </summary>
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly SettingsManager _settingsManager;
        private readonly ILogger<SettingsViewModel> _logger;

        [ObservableProperty]
        private UserSettings? _settings;

        [ObservableProperty]
        private bool _isDirty;

        [ObservableProperty]
        private string _apiKey = string.Empty;

        [ObservableProperty]
        private string _selectedVoice = "Aoede";

        [ObservableProperty]
        private string _selectedLanguage = "en-US";

        [ObservableProperty]
        private double _speechRate = 1.0;

        [ObservableProperty]
        private double _volume = 0.8;

        [ObservableProperty]
        private bool _voiceActivationEnabled = true;

        [ObservableProperty]
        private bool _screenContextEnabled = false;

        [ObservableProperty]
        private string _selectedTheme = "System";

        [ObservableProperty]
        private bool _startWithWindows = false;

        [ObservableProperty]
        private bool _startMinimized = false;

        [ObservableProperty]
        private bool _saveConversationHistory = true;

        [ObservableProperty]
        private bool _allowDataCollection = false;

        [ObservableProperty]
        private int _conversationRetentionDays = 30;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ResetToDefaultsCommand { get; }
        public ICommand TestVoiceCommand { get; }

        public string[] AvailableVoices { get; } = { "Puck", "Charon", "Kore", "Fenrir", "Aoede", "Leda", "Orus", "Zephyr" };
        public string[] AvailableLanguages { get; } = { "en-US", "en-GB", "en-AU", "es-US", "fr-FR", "de-DE", "ja-JP", "zh-CN" };
        public string[] AvailableThemes { get; } = { "Light", "Dark", "System" };

        public SettingsViewModel(SettingsManager settingsManager, ILogger<SettingsViewModel> logger)
        {
            _settingsManager = settingsManager;
            _logger = logger;

            // Initialize commands
            SaveCommand = new AsyncRelayCommand(SaveSettingsAsync);
            CancelCommand = new AsyncRelayCommand(CancelChangesAsync);
            ResetToDefaultsCommand = new AsyncRelayCommand(ResetToDefaultsAsync);
            TestVoiceCommand = new AsyncRelayCommand(TestVoiceAsync);

            // Subscribe to property changes to track dirty state
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Skip certain properties that shouldn't mark as dirty
            if (e.PropertyName == nameof(IsDirty) || e.PropertyName == nameof(Settings))
                return;

            IsDirty = true;
        }

        public async Task LoadSettingsAsync()
        {
            try
            {
                _logger.LogInformation("Loading settings");
                
                Settings = await _settingsManager.LoadSettingsAsync();
                await _settingsManager.ValidateAndFixSettingsAsync(Settings);

                // Update UI properties
                UpdateUIFromSettings();
                IsDirty = false;

                _logger.LogInformation("Settings loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load settings");
                
                // Use defaults if loading fails
                Settings = new UserSettings();
                UpdateUIFromSettings();
            }
        }        private async Task SaveSettingsAsync()
        {
            try
            {
                if (Settings == null)
                {
                    Settings = new UserSettings();
                }

                _logger.LogInformation("Saving settings");

                // Update settings from UI
                UpdateSettingsFromUI();

                // Validate and save
                Settings = await _settingsManager.ValidateAndFixSettingsAsync(Settings);
                await _settingsManager.SaveSettingsAsync(Settings);

                IsDirty = false;
                _logger.LogInformation("Settings saved successfully");

                // Notify that settings were saved (for connection status update)
                OnSettingsSaved?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save settings");
                throw;
            }
        }

        // Event to notify when settings are saved
        public event Action? OnSettingsSaved;

        private async Task CancelChangesAsync()
        {
            try
            {
                if (Settings != null)
                {
                    UpdateUIFromSettings();
                    IsDirty = false;
                    _logger.LogInformation("Settings changes cancelled");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel settings changes");
            }
        }

        private async Task ResetToDefaultsAsync()
        {
            try
            {
                _logger.LogInformation("Resetting settings to defaults");
                
                await _settingsManager.ResetToDefaultsAsync();
                Settings = new UserSettings();
                UpdateUIFromSettings();
                IsDirty = true;

                _logger.LogInformation("Settings reset to defaults");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reset settings to defaults");
            }
        }

        private async Task TestVoiceAsync()
        {
            try
            {
                _logger.LogInformation("Testing voice: {Voice}", SelectedVoice);
                
                // Voice testing logic would be implemented here
                // This would use the audio service to play a sample message
                
                _logger.LogInformation("Voice test completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to test voice");
            }
        }

        private void UpdateUIFromSettings()
        {
            if (Settings == null) return;

            try
            {
                // Temporarily disable property change tracking
                PropertyChanged -= OnPropertyChanged;

                ApiKey = JulieUtils.MaskApiKey(Settings.ApiKey ?? "");
                SelectedVoice = Settings.VoiceName;
                SelectedLanguage = Settings.LanguageCode;
                SpeechRate = Settings.SpeechRate;
                Volume = Settings.Volume;
                VoiceActivationEnabled = Settings.VoiceActivationEnabled;
                ScreenContextEnabled = Settings.ScreenContextEnabled;
                SelectedTheme = Settings.Theme;
                StartWithWindows = Settings.StartWithWindows;
                StartMinimized = Settings.StartMinimized;
                SaveConversationHistory = Settings.Privacy.SaveConversationHistory;
                AllowDataCollection = Settings.Privacy.AllowDataCollection;
                ConversationRetentionDays = Settings.Privacy.ConversationRetentionDays;

                // Re-enable property change tracking
                PropertyChanged += OnPropertyChanged;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update UI from settings");
                PropertyChanged += OnPropertyChanged; // Ensure we re-enable tracking
            }
        }

        private void UpdateSettingsFromUI()
        {
            if (Settings == null) return;

            try
            {
                // Only update API key if it's not masked
                if (!ApiKey.Contains("*"))
                {
                    Settings.ApiKey = ApiKey;
                }

                Settings.VoiceName = SelectedVoice;
                Settings.LanguageCode = SelectedLanguage;
                Settings.SpeechRate = SpeechRate;
                Settings.Volume = Volume;
                Settings.VoiceActivationEnabled = VoiceActivationEnabled;
                Settings.ScreenContextEnabled = ScreenContextEnabled;
                Settings.Theme = SelectedTheme;
                Settings.StartWithWindows = StartWithWindows;
                Settings.StartMinimized = StartMinimized;
                Settings.Privacy.SaveConversationHistory = SaveConversationHistory;
                Settings.Privacy.AllowDataCollection = AllowDataCollection;
                Settings.Privacy.ConversationRetentionDays = ConversationRetentionDays;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update settings from UI");
            }
        }

        public void SetApiKey(string apiKey)
        {
            try
            {
                if (Settings == null)
                {
                    Settings = new UserSettings();
                }

                Settings.ApiKey = apiKey;
                ApiKey = JulieUtils.MaskApiKey(apiKey);
                IsDirty = true;

                _logger.LogInformation("API key updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set API key");
            }
        }

        public bool ValidateSettings()
        {
            try
            {
                if (Settings == null) return false;

                // Validate API key
                if (string.IsNullOrWhiteSpace(Settings.ApiKey))
                {
                    _logger.LogWarning("API key is required");
                    return false;
                }

                if (!JulieUtils.IsValidApiKey(Settings.ApiKey))
                {
                    _logger.LogWarning("API key format is invalid");
                    return false;
                }

                // Validate other settings
                if (SpeechRate < 0.25 || SpeechRate > 4.0)
                {
                    _logger.LogWarning("Speech rate must be between 0.25 and 4.0");
                    return false;
                }

                if (Volume < 0.0 || Volume > 1.0)
                {
                    _logger.LogWarning("Volume must be between 0.0 and 1.0");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate settings");
                return false;
            }
        }
    }
}

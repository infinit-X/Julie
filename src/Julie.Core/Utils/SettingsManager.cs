using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Julie.Core.Models;
using Microsoft.Extensions.Logging;

namespace Julie.Core.Utils
{
    /// <summary>
    /// Manages user settings persistence
    /// </summary>
    public class SettingsManager
    {
        private readonly ILogger<SettingsManager> _logger;
        private readonly string _settingsPath;
        private UserSettings? _cachedSettings;
        private readonly object _lock = new object();

        public SettingsManager(ILogger<SettingsManager> logger)
        {
            _logger = logger;
            
            // Store settings in user's AppData folder
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var julieFolder = Path.Combine(appDataPath, "Julie");
            
            if (!Directory.Exists(julieFolder))
            {
                Directory.CreateDirectory(julieFolder);
            }
            
            _settingsPath = Path.Combine(julieFolder, "settings.json");
        }

        /// <summary>
        /// Loads user settings from disk
        /// </summary>
        public async Task<UserSettings> LoadSettingsAsync()
        {
            try
            {
                lock (_lock)
                {
                    if (_cachedSettings != null)
                    {
                        return _cachedSettings;
                    }
                }

                if (!File.Exists(_settingsPath))
                {
                    _logger.LogInformation("Settings file not found, creating default settings");
                    var defaultSettings = new UserSettings();
                    await SaveSettingsAsync(defaultSettings);
                    return defaultSettings;
                }

                var json = await File.ReadAllTextAsync(_settingsPath);
                var settings = JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();

                lock (_lock)
                {
                    _cachedSettings = settings;
                }

                _logger.LogInformation("Settings loaded successfully");
                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load settings, using defaults");
                return new UserSettings();
            }
        }

        /// <summary>
        /// Saves user settings to disk
        /// </summary>
        public async Task SaveSettingsAsync(UserSettings settings)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(settings, options);
                await File.WriteAllTextAsync(_settingsPath, json);

                lock (_lock)
                {
                    _cachedSettings = settings;
                }

                _logger.LogInformation("Settings saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save settings");
                throw;
            }
        }

        /// <summary>
        /// Updates a specific setting without loading all settings
        /// </summary>
        public async Task UpdateSettingAsync<T>(string propertyName, T value)
        {
            var settings = await LoadSettingsAsync();
            
            var property = typeof(UserSettings).GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(settings, value);
                await SaveSettingsAsync(settings);
            }
            else
            {
                throw new ArgumentException($"Property '{propertyName}' not found or not writable");
            }
        }

        /// <summary>
        /// Gets a specific setting value
        /// </summary>
        public async Task<T?> GetSettingAsync<T>(string propertyName)
        {
            var settings = await LoadSettingsAsync();
            
            var property = typeof(UserSettings).GetProperty(propertyName);
            if (property != null && property.CanRead)
            {
                return (T?)property.GetValue(settings);
            }
            
            return default;
        }

        /// <summary>
        /// Resets settings to defaults
        /// </summary>
        public async Task ResetToDefaultsAsync()
        {
            var defaultSettings = new UserSettings();
            await SaveSettingsAsync(defaultSettings);
            _logger.LogInformation("Settings reset to defaults");
        }

        /// <summary>
        /// Backs up current settings
        /// </summary>
        public async Task BackupSettingsAsync()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var backupPath = $"{_settingsPath}.backup.{DateTime.Now:yyyyMMdd_HHmmss}";
                    File.Copy(_settingsPath, backupPath);
                    _logger.LogInformation("Settings backed up to: {BackupPath}", backupPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to backup settings");
                throw;
            }
        }

        /// <summary>        /// <summary>
        /// Validates settings and fixes any issues
        /// </summary>
        public async Task<UserSettings> ValidateAndFixSettingsAsync(UserSettings settings)
        {
            var wasFixed = false;

            // Validate and fix volume
            if (settings.Volume < 0 || settings.Volume > 1)
            {
                settings.Volume = Math.Clamp(settings.Volume, 0.0, 1.0);
                wasFixed = true;
            }
            
            // Validate and fix speech rate
            if (settings.SpeechRate < 0.25 || settings.SpeechRate > 4.0)
            {
                settings.SpeechRate = Math.Clamp(settings.SpeechRate, 0.25, 4.0);
                wasFixed = true;
            }

            // Validate voice name
            var validVoices = new[] { "Puck", "Charon", "Kore", "Fenrir", "Aoede", "Leda", "Orus", "Zephyr" };
            if (!validVoices.Any(v => v.Equals(settings.VoiceName, StringComparison.OrdinalIgnoreCase)))
            {
                settings.VoiceName = "Aoede";
                wasFixed = true;
            }

            // Validate theme
            var validThemes = new[] { "Light", "Dark", "System" };
            if (!validThemes.Any(t => t.Equals(settings.Theme, StringComparison.OrdinalIgnoreCase)))
            {
                settings.Theme = "System";
                wasFixed = true;
            }

            if (wasFixed)
            {
                await SaveSettingsAsync(settings);
                _logger.LogInformation("Settings validated and fixed");
            }

            return settings;
        }

        /// <summary>
        /// Checks if this is the first run of the application
        /// </summary>
        public bool IsFirstRun()
        {
            return !File.Exists(_settingsPath);
        }
    }
}

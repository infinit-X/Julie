using System.Collections.Generic;

namespace Julie.Core.Models
{
    /// <summary>
    /// User settings and preferences for Julie
    /// </summary>
    public class UserSettings
    {
        /// <summary>
        /// Google API key for Live API
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Selected voice for speech synthesis
        /// </summary>
        public string VoiceName { get; set; } = "Aoede";

        /// <summary>
        /// Language code for speech
        /// </summary>
        public string LanguageCode { get; set; } = "en-US";

        /// <summary>
        /// Speech rate (0.25 to 4.0)
        /// </summary>
        public double SpeechRate { get; set; } = 1.0;

        /// <summary>
        /// Volume level (0.0 to 1.0)
        /// </summary>
        public double Volume { get; set; } = 0.8;

        /// <summary>
        /// Whether voice activation is enabled
        /// </summary>
        public bool VoiceActivationEnabled { get; set; } = true;

        /// <summary>
        /// Whether screen context awareness is enabled
        /// </summary>
        public bool ScreenContextEnabled { get; set; } = false;

        /// <summary>
        /// Theme preference (Light, Dark, System)
        /// </summary>
        public string Theme { get; set; } = "System";

        /// <summary>
        /// Whether to start Julie with Windows
        /// </summary>
        public bool StartWithWindows { get; set; } = false;

        /// <summary>
        /// Whether to start minimized
        /// </summary>
        public bool StartMinimized { get; set; } = false;

        /// <summary>
        /// Window position and size settings
        /// </summary>
        public WindowSettings WindowSettings { get; set; } = new WindowSettings();

        /// <summary>
        /// Privacy settings
        /// </summary>
        public PrivacySettings Privacy { get; set; } = new PrivacySettings();

        /// <summary>
        /// Accessibility settings
        /// </summary>
        public AccessibilitySettings Accessibility { get; set; } = new AccessibilitySettings();

        /// <summary>
        /// Custom hotkeys
        /// </summary>
        public Dictionary<string, string> Hotkeys { get; set; } = new Dictionary<string, string>
        {
            { "ActivateVoice", "Ctrl+Shift+J" },
            { "ToggleWindow", "Ctrl+Shift+H" },
            { "NewConversation", "Ctrl+N" }
        };
    }

    public class WindowSettings
    {
        public double Width { get; set; } = 800;
        public double Height { get; set; } = 600;
        public double Left { get; set; } = -1; // -1 means center
        public double Top { get; set; } = -1; // -1 means center
        public bool IsMaximized { get; set; } = false;
    }

    public class PrivacySettings
    {
        public bool SaveConversationHistory { get; set; } = true;
        public bool AllowDataCollection { get; set; } = false;
        public bool AllowScreenCapture { get; set; } = false;
        public int ConversationRetentionDays { get; set; } = 30;
    }

    public class AccessibilitySettings
    {
        public double FontSize { get; set; } = 14;
        public bool HighContrast { get; set; } = false;
        public bool ScreenReaderSupport { get; set; } = false;
        public bool ReduceAnimations { get; set; } = false;
    }
}

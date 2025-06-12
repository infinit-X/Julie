using System;
using System.Security.Cryptography;
using System.Text;

namespace Julie.Core.Utils
{
    /// <summary>
    /// Utility methods for the Julie application
    /// </summary>
    public static class JulieUtils
    {
        /// <summary>
        /// Converts audio samples to the format expected by Live API
        /// </summary>
        public static byte[] ConvertToLiveApiFormat(byte[] audioData, int sourceSampleRate, int targetSampleRate = 16000)
        {
            // This is a simplified conversion - in a real implementation you'd use proper audio resampling
            if (sourceSampleRate == targetSampleRate)
            {
                return audioData;
            }

            // For now, just return the original data
            // TODO: Implement proper audio resampling
            return audioData;
        }

        /// <summary>
        /// Encodes binary data to base64 string
        /// </summary>
        public static string ToBase64(byte[] data)
        {
            return Convert.ToBase64String(data);
        }

        /// <summary>
        /// Decodes base64 string to binary data
        /// </summary>
        public static byte[] FromBase64(string base64)
        {
            return Convert.FromBase64String(base64);
        }

        /// <summary>
        /// Generates a unique session ID
        /// </summary>
        public static string GenerateSessionId()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Gets a human-readable timestamp
        /// </summary>
        public static string GetTimestamp(DateTime? dateTime = null)
        {
            var dt = dateTime ?? DateTime.Now;
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Gets a relative time string (e.g., "2 minutes ago")
        /// </summary>
        public static string GetRelativeTime(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays >= 2 ? "s" : "")} ago";
            
            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours >= 2 ? "s" : "")} ago";
            
            if (timeSpan.TotalMinutes >= 1)
                return $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes >= 2 ? "s" : "")} ago";
            
            return "Just now";
        }

        /// <summary>
        /// Truncates text to a specified length
        /// </summary>
        public static string TruncateText(string text, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength - suffix.Length) + suffix;
        }

        /// <summary>
        /// Sanitizes text for display purposes
        /// </summary>
        public static string SanitizeText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Remove control characters except common whitespace
            var sb = new StringBuilder();
            foreach (char c in text)
            {
                if (c == '\n' || c == '\r' || c == '\t' || !char.IsControl(c))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Encrypts sensitive data using machine-specific entropy
        /// </summary>
        public static string EncryptString(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = ProtectedData.Protect(plainTextBytes, null, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encryptedBytes);
            }
            catch
            {
                // If encryption fails, return empty string for security
                return string.Empty;
            }
        }

        /// <summary>
        /// Decrypts sensitive data
        /// </summary>
        public static string DecryptString(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] plainTextBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainTextBytes);
            }
            catch
            {
                // If decryption fails, return empty string
                return string.Empty;
            }
        }

        /// <summary>
        /// Validates if a string is a valid API key format
        /// </summary>
        public static bool IsValidApiKey(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return false;

            // Basic validation - Google API keys typically start with specific prefixes
            return apiKey.StartsWith("AIza") && apiKey.Length >= 35;
        }

        /// <summary>
        /// Masks an API key for display purposes
        /// </summary>
        public static string MaskApiKey(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey) || apiKey.Length < 8)
                return "****";

            return apiKey.Substring(0, 4) + new string('*', Math.Max(4, apiKey.Length - 8)) + apiKey.Substring(apiKey.Length - 4);
        }

        /// <summary>
        /// Calculates the audio duration in seconds
        /// </summary>
        public static double CalculateAudioDuration(int byteCount, int sampleRate, int channels, int bitsPerSample)
        {
            var bytesPerSample = bitsPerSample / 8;
            var totalSamples = byteCount / (bytesPerSample * channels);
            return (double)totalSamples / sampleRate;
        }

        /// <summary>
        /// Formats a file size in human-readable format
        /// </summary>
        public static string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int suffixIndex = 0;
            double size = bytes;

            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }

            return $"{size:F1} {suffixes[suffixIndex]}";
        }

        /// <summary>
        /// Validates a hotkey string format
        /// </summary>
        public static bool IsValidHotkey(string hotkey)
        {
            if (string.IsNullOrWhiteSpace(hotkey))
                return false;

            // Basic validation for hotkey format (e.g., "Ctrl+Shift+J")
            var parts = hotkey.Split('+');
            return parts.Length >= 2 && parts.Length <= 4;
        }

        /// <summary>
        /// Gets the MIME type for an image based on its format
        /// </summary>
        public static string GetImageMimeType(System.Drawing.Imaging.ImageFormat format)
        {
            if (format.Equals(System.Drawing.Imaging.ImageFormat.Png))
                return "image/png";
            if (format.Equals(System.Drawing.Imaging.ImageFormat.Jpeg))
                return "image/jpeg";
            if (format.Equals(System.Drawing.Imaging.ImageFormat.Gif))
                return "image/gif";
            if (format.Equals(System.Drawing.Imaging.ImageFormat.Bmp))
                return "image/bmp";
            
            return "image/png"; // Default to PNG
        }
    }
}

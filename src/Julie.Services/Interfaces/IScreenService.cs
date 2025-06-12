using System;
using System.Threading.Tasks;
using Julie.Core.Screen;

namespace Julie.Services
{
    /// <summary>
    /// Service for managing screen capture and context awareness
    /// </summary>
    public interface IScreenService
    {
        /// <summary>
        /// Event fired when screen content is captured
        /// </summary>
        event EventHandler<byte[]> ScreenCaptured;

        /// <summary>
        /// Event fired when screen monitoring starts or stops
        /// </summary>
        event EventHandler<bool> MonitoringStatusChanged;

        /// <summary>
        /// Gets whether screen monitoring is active
        /// </summary>
        bool IsMonitoring { get; }

        /// <summary>
        /// Gets whether screen capture permission is granted
        /// </summary>
        bool HasPermission { get; }

        /// <summary>
        /// Initializes the screen service
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Requests permission for screen capture
        /// </summary>
        Task<bool> RequestPermissionAsync();

        /// <summary>
        /// Captures the current screen content
        /// </summary>
        Task<byte[]> CaptureScreenAsync();

        /// <summary>
        /// Starts monitoring screen for changes
        /// </summary>
        Task StartMonitoringAsync(TimeSpan interval);

        /// <summary>
        /// Stops monitoring screen
        /// </summary>
        Task StopMonitoringAsync();

        /// <summary>
        /// Gets information about available screens
        /// </summary>
        ScreenInfo[] GetAvailableScreens();

        /// <summary>
        /// Sets which screen to monitor
        /// </summary>
        Task SetActiveScreenAsync(int screenIndex);

        /// <summary>
        /// Captures a specific window
        /// </summary>
        Task<byte[]> CaptureWindowAsync(IntPtr windowHandle);

        /// <summary>
        /// Cleans up screen capture resources
        /// </summary>
        Task DisposeAsync();
    }
}

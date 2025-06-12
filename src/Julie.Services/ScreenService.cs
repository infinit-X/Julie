using System;
using System.Drawing;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Julie.Core.Screen;

namespace Julie.Services
{
    /// <summary>
    /// Screen service implementation for capturing screen content
    /// </summary>
    public class ScreenService : IScreenService
    {
        private readonly ILogger<ScreenService> _logger;
        private readonly ScreenCapture _screenCapture;
        private bool _isMonitoring;
        private bool _hasPermission = true; // Assume we have permission on Windows

        public event EventHandler<byte[]>? ScreenCaptured;
        public event EventHandler<bool>? MonitoringStatusChanged;

        public bool IsMonitoring => _isMonitoring;
        public bool HasPermission => _hasPermission;

        public ScreenService(ILogger<ScreenService> logger)
        {
            _logger = logger;
            _screenCapture = new ScreenCapture(logger);

            // Wire up events
            _screenCapture.ScreenCaptured += OnScreenCaptured;
        }

        private void OnScreenCaptured(object? sender, Core.Screen.ScreenCaptureEventArgs e)
        {
            ScreenCaptured?.Invoke(this, e.ImageData);
        }

        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing screen service");
                // Check if we have screen capture permissions
                _hasPermission = await CheckPermissionsAsync();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize screen service");
                throw;
            }
        }

        public async Task<bool> RequestPermissionAsync()
        {
            try
            {
                _logger.LogInformation("Requesting screen capture permission");
                
                // On Windows, we typically have permission by default
                // In a real implementation, this might show a dialog or check system settings
                _hasPermission = true;
                
                return _hasPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to request screen permission");
                return false;
            }
        }

        public async Task<byte[]> CaptureScreenAsync()
        {
            if (!_hasPermission)
            {
                throw new UnauthorizedAccessException("Screen capture permission not granted");
            }

            try
            {
                _logger.LogInformation("Capturing full screen");
                var imageData = await _screenCapture.CaptureFullScreenAsync();
                return imageData ?? Array.Empty<byte>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to capture screen");
                throw;
            }
        }

        public async Task StartMonitoringAsync(TimeSpan interval)
        {
            if (!_hasPermission)
            {
                throw new UnauthorizedAccessException("Screen capture permission not granted");
            }

            try
            {
                _logger.LogInformation("Starting screen monitoring (interval: {Interval})", interval);
                
                var intervalMs = (int)interval.TotalMilliseconds;
                var result = await _screenCapture.StartContinuousCaptureAsync(intervalMs);
                
                _isMonitoring = result;
                MonitoringStatusChanged?.Invoke(this, _isMonitoring);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start screen monitoring");
                throw;
            }
        }

        public async Task StopMonitoringAsync()
        {
            try
            {
                _logger.LogInformation("Stopping screen monitoring");
                
                var result = await _screenCapture.StopContinuousCaptureAsync();
                _isMonitoring = !result;
                MonitoringStatusChanged?.Invoke(this, _isMonitoring);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop screen monitoring");
                throw;
            }
        }

        public ScreenInfo[] GetAvailableScreens()
        {
            try
            {
                _logger.LogInformation("Getting available screens");
                return ScreenCapture.GetAvailableScreens();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get available screens");
                return Array.Empty<ScreenInfo>();
            }
        }

        public async Task SetActiveScreenAsync(int screenIndex)
        {
            try
            {
                _logger.LogInformation("Setting active screen to index: {ScreenIndex}", screenIndex);
                
                // Stop monitoring if running
                if (_isMonitoring)
                {
                    await StopMonitoringAsync();
                }
                
                // Update screen capture to use specific screen
                // Note: This would require changes to ScreenCapture to support screen selection
                // For now, just log the action
                _logger.LogInformation("Active screen set to: {ScreenIndex}", screenIndex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set active screen");
                throw;
            }
        }

        public async Task<byte[]> CaptureWindowAsync(IntPtr windowHandle)
        {
            if (!_hasPermission)
            {
                throw new UnauthorizedAccessException("Screen capture permission not granted");
            }

            try
            {
                _logger.LogInformation("Capturing window: {WindowHandle}", windowHandle);
                
                // For now, we'll capture the full screen since our ScreenCapture doesn't support specific windows
                // In a real implementation, this would capture just the specified window
                var imageData = await _screenCapture.CaptureFullScreenAsync();
                return imageData ?? Array.Empty<byte>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to capture window");
                throw;
            }
        }

        private async Task<bool> CheckPermissionsAsync()
        {
            try
            {
                // Try to capture a small portion of the screen to check permissions
                var testCapture = await _screenCapture.CaptureRegionAsync(0, 0, 1, 1);
                return testCapture != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task DisposeAsync()
        {
            try
            {
                if (_isMonitoring)
                {
                    await StopMonitoringAsync();
                }
                
                _screenCapture?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during screen service disposal");
            }
        }

        public void Dispose()
        {
            DisposeAsync().Wait(TimeSpan.FromSeconds(2));
        }
    }
}

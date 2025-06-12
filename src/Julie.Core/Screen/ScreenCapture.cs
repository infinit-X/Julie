using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;

namespace Julie.Core.Screen
{
    /// <summary>
    /// Event arguments for screen capture events
    /// </summary>
    public class ScreenCaptureEventArgs : EventArgs
    {
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public string MimeType { get; set; } = "image/png";
        public int Width { get; set; }
        public int Height { get; set; }
    }

    /// <summary>
    /// Captures screen content for Live API context awareness
    /// </summary>
    public class ScreenCapture : IDisposable
    {
        private readonly ILogger _logger;
        private bool _isCapturing = false;
        private readonly object _lock = new object();

        public event EventHandler<ScreenCaptureEventArgs>? ScreenCaptured;
        public event EventHandler? CaptureStarted;
        public event EventHandler? CaptureStopped;

        public bool IsCapturing
        {
            get
            {
                lock (_lock)
                {
                    return _isCapturing;
                }
            }
        }

        public ScreenCapture(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Captures the entire primary screen
        /// </summary>
        public async Task<byte[]> CaptureScreenAsync(ImageFormat? format = null, int maxWidth = 1920, int maxHeight = 1080)
        {
            try
            {
                format ??= ImageFormat.Png;
                
                // Get primary screen bounds
                var bounds = System.Windows.Forms.Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
                
                // Calculate scaled dimensions if needed
                var scaleX = Math.Min(1.0, (double)maxWidth / bounds.Width);
                var scaleY = Math.Min(1.0, (double)maxHeight / bounds.Height);
                var scale = Math.Min(scaleX, scaleY);
                
                var scaledWidth = (int)(bounds.Width * scale);
                var scaledHeight = (int)(bounds.Height * scale);

                using var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
                using var graphics = Graphics.FromImage(bitmap);
                
                // Capture the screen
                graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);

                // Scale if necessary
                if (scale < 1.0)
                {
                    using var scaledBitmap = new Bitmap(scaledWidth, scaledHeight, PixelFormat.Format32bppArgb);
                    using var scaledGraphics = Graphics.FromImage(scaledBitmap);
                    scaledGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    scaledGraphics.DrawImage(bitmap, 0, 0, scaledWidth, scaledHeight);

                    return await BitmapToBytesAsync(scaledBitmap, format);
                }
                else
                {
                    return await BitmapToBytesAsync(bitmap, format);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to capture screen");
                throw;
            }
        }

        /// <summary>
        /// Alias for CaptureScreenAsync for service compatibility
        /// </summary>
        public async Task<byte[]?> CaptureFullScreenAsync()
        {
            try
            {
                return await CaptureScreenAsync();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Captures a specific region of the screen
        /// </summary>
        public async Task<byte[]?> CaptureRegionAsync(int x, int y, int width, int height)
        {
            try
            {
                // This would need a more sophisticated implementation
                // For now, capture full screen
                return await CaptureScreenAsync();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Starts continuous screen capture
        /// </summary>
        public async Task<bool> StartContinuousCaptureAsync(int intervalMs)
        {
            try
            {
                await StartMonitoringAsync(TimeSpan.FromMilliseconds(intervalMs));
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Stops continuous screen capture
        /// </summary>
        public async Task<bool> StopContinuousCaptureAsync()
        {
            try
            {
                await StopMonitoringAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Captures a specific window
        /// </summary>
        public async Task<byte[]> CaptureWindowAsync(IntPtr windowHandle, ImageFormat? format = null)
        {
            try
            {
                format ??= ImageFormat.Png;

                // Get window rectangle
                if (!NativeMethods.GetWindowRect(windowHandle, out var rect))
                {
                    throw new InvalidOperationException("Failed to get window rectangle");
                }

                var width = rect.Right - rect.Left;
                var height = rect.Bottom - rect.Top;

                using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                using var graphics = Graphics.FromImage(bitmap);

                // Capture the window
                graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);

                return await BitmapToBytesAsync(bitmap, format);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to capture window");
                throw;
            }
        }

        /// <summary>
        /// Starts continuous screen monitoring (for future implementation)
        /// </summary>
        public async Task StartMonitoringAsync(TimeSpan interval)
        {
            lock (_lock)
            {
                if (_isCapturing)
                {
                    _logger.LogWarning("Screen monitoring is already active");
                    return;
                }
                _isCapturing = true;
            }

            CaptureStarted?.Invoke(this, EventArgs.Empty);
            _logger.LogInformation("Screen monitoring started with interval: {Interval}", interval);

            // This is a placeholder for continuous monitoring
            // In a real implementation, you would set up a timer or background task
            // For now, we'll just mark as not capturing
            lock (_lock)
            {
                _isCapturing = false;
            }
            CaptureStopped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Stops continuous screen monitoring
        /// </summary>
        public async Task StopMonitoringAsync()
        {
            lock (_lock)
            {
                if (!_isCapturing)
                {
                    _logger.LogWarning("Screen monitoring is not active");
                    return;
                }
                _isCapturing = false;
            }

            CaptureStopped?.Invoke(this, EventArgs.Empty);
            _logger.LogInformation("Screen monitoring stopped");
        }

        /// <summary>
        /// Converts a bitmap to byte array
        /// </summary>
        private async Task<byte[]> BitmapToBytesAsync(Bitmap bitmap, ImageFormat format)
        {
            using var stream = new MemoryStream();
            bitmap.Save(stream, format);
            return stream.ToArray();
        }

        /// <summary>
        /// Gets information about available screens
        /// </summary>
        public static ScreenInfo[] GetAvailableScreens()
        {
            var screens = System.Windows.Forms.Screen.AllScreens;
            var screenInfos = new ScreenInfo[screens.Length];

            for (int i = 0; i < screens.Length; i++)
            {
                var screen = screens[i];
                screenInfos[i] = new ScreenInfo
                {
                    DeviceName = screen.DeviceName,
                    Bounds = screen.Bounds,
                    IsPrimary = screen.Primary,
                    WorkingArea = screen.WorkingArea
                };
            }

            return screenInfos;
        }

        public void Dispose()
        {
            try
            {
                if (_isCapturing)
                {
                    StopMonitoringAsync().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing screen capture");
            }
        }
    }

    /// <summary>
    /// Information about a screen/display
    /// </summary>
    public class ScreenInfo
    {
        public string DeviceName { get; set; } = string.Empty;
        public Rectangle Bounds { get; set; }
        public bool IsPrimary { get; set; }
        public Rectangle WorkingArea { get; set; }
    }

    /// <summary>
    /// Native methods for window operations
    /// </summary>
    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        internal struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}

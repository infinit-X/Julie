using System;
using System.Threading.Tasks;
using NAudio.Wave;
using Microsoft.Extensions.Logging;

namespace Julie.Core.Audio
{
    /// <summary>
    /// Event arguments for audio capture events
    /// </summary>
    public class AudioCaptureEventArgs : EventArgs
    {
        public byte[] AudioData { get; set; } = Array.Empty<byte>();
        public string MimeType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Captures audio from the microphone for Live API input
    /// </summary>
    public class AudioCapture : IDisposable
    {
        private readonly ILogger _logger;
        private WaveInEvent? _waveIn;
        private bool _isCapturing = false;
        private readonly object _lock = new object();

        public event EventHandler<AudioCaptureEventArgs>? DataAvailable;
        public event EventHandler<byte[]>? AudioDataAvailable
        {
            add => DataAvailable += (sender, e) => value?.Invoke(sender, e.AudioData);
            remove { }
        }
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

        public AudioCapture(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Initializes the audio capture with the specified format
        /// Live API expects 16-bit PCM, 16kHz, mono
        /// </summary>
        public void Initialize(int deviceNumber = 0, int sampleRate = 16000, int channels = 1)
        {
            try
            {
                _waveIn = new WaveInEvent
                {
                    DeviceNumber = deviceNumber,
                    WaveFormat = new WaveFormat(sampleRate, 16, channels),
                    BufferMilliseconds = 50 // Small buffer for low latency
                };

                _waveIn.DataAvailable += OnDataAvailable;
                _waveIn.RecordingStopped += OnRecordingStopped;

                _logger.LogInformation("Audio capture initialized: {SampleRate}Hz, {Channels} channel(s), {BitsPerSample}-bit", 
                    sampleRate, channels, 16);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize audio capture");
                throw;
            }
        }

        /// <summary>
        /// Starts capturing audio from the microphone
        /// </summary>
        public async Task StartCaptureAsync()
        {
            if (_waveIn == null)
            {
                throw new InvalidOperationException("Audio capture not initialized. Call Initialize() first.");
            }

            try
            {
                lock (_lock)
                {
                    if (_isCapturing)
                    {
                        _logger.LogWarning("Audio capture is already running");
                        return;
                    }
                    _isCapturing = true;
                }

                _waveIn.StartRecording();
                CaptureStarted?.Invoke(this, EventArgs.Empty);
                _logger.LogInformation("Audio capture started");
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    _isCapturing = false;
                }
                _logger.LogError(ex, "Failed to start audio capture");
                throw;
            }
        }

        /// <summary>
        /// Alias for StartCaptureAsync for service compatibility
        /// </summary>
        public async Task<bool> StartRecordingAsync()
        {
            try
            {
                await StartCaptureAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Stops capturing audio from the microphone
        /// </summary>
        public async Task StopCaptureAsync()
        {
            if (_waveIn == null) return;

            try
            {
                lock (_lock)
                {
                    if (!_isCapturing)
                    {
                        _logger.LogWarning("Audio capture is not running");
                        return;
                    }
                }

                _waveIn.StopRecording();
                _logger.LogInformation("Audio capture stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping audio capture");
                throw;
            }
        }

        /// <summary>
        /// Alias for StopCaptureAsync for service compatibility
        /// </summary>
        public async Task<bool> StopRecordingAsync()
        {
            try
            {
                await StopCaptureAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets available audio input devices
        /// </summary>
        public static string[] GetAvailableDevices()
        {
            var devices = new string[WaveIn.DeviceCount];
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                var deviceInfo = WaveIn.GetCapabilities(i);
                devices[i] = deviceInfo.ProductName;
            }
            return devices;
        }

        /// <summary>
        /// Handles audio data available event from NAudio
        /// </summary>
        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            try
            {
                if (e.BytesRecorded > 0)
                {
                    // Create a copy of the audio data
                    var audioData = new byte[e.BytesRecorded];
                    Array.Copy(e.Buffer, 0, audioData, 0, e.BytesRecorded);

                    // Notify listeners with the captured audio data
                    DataAvailable?.Invoke(this, new AudioCaptureEventArgs
                    {
                        AudioData = audioData,
                        MimeType = "audio/pcm;rate=16000"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing captured audio data");
            }
        }

        /// <summary>
        /// Handles recording stopped event from NAudio
        /// </summary>
        private void OnRecordingStopped(object? sender, StoppedEventArgs e)
        {
            lock (_lock)
            {
                _isCapturing = false;
            }

            if (e.Exception != null)
            {
                _logger.LogError(e.Exception, "Audio recording stopped due to error");
            }

            CaptureStopped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Disposes of the audio capture resources
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (_isCapturing)
                {
                    StopCaptureAsync().GetAwaiter().GetResult();
                }

                if (_waveIn != null)
                {
                    _waveIn.DataAvailable -= OnDataAvailable;
                    _waveIn.RecordingStopped -= OnRecordingStopped;
                    _waveIn.Dispose();
                    _waveIn = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing audio capture");
            }
        }
    }
}

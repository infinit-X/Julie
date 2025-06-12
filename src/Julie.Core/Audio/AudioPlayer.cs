using System;
using System.Threading.Tasks;
using NAudio.Wave;
using Microsoft.Extensions.Logging;

namespace Julie.Core.Audio
{
    /// <summary>
    /// Plays audio received from the Live API
    /// </summary>
    public class AudioPlayer : IDisposable
    {
        private readonly ILogger _logger;
        private IWavePlayer? _waveOut;
        private BufferedWaveProvider? _waveProvider;
        private readonly object _lock = new object();
        private bool _isInitialized = false;

        public event EventHandler? PlaybackStarted;
        public event EventHandler? PlaybackStopped;

        public bool IsPlaying
        {
            get
            {
                lock (_lock)
                {
                    return _waveOut?.PlaybackState == PlaybackState.Playing;
                }
            }
        }

        public double Volume
        {
            get
            {
                lock (_lock)
                {
                    return _waveOut?.Volume ?? 0.0;
                }
            }
            set
            {
                lock (_lock)
                {
                    if (_waveOut != null)
                    {
                        _waveOut.Volume = (float)Math.Clamp(value, 0.0, 1.0);
                    }
                }
            }
        }

        public AudioPlayer(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Initializes the audio player for Live API audio output
        /// Live API outputs 24kHz 16-bit PCM audio
        /// </summary>
        public void Initialize(int sampleRate = 24000, int channels = 1, int bitsPerSample = 16)
        {
            try
            {
                lock (_lock)
                {
                    if (_isInitialized)
                    {
                        _logger.LogWarning("Audio player is already initialized");
                        return;
                    }

                    // Create wave format for Live API audio
                    var waveFormat = new WaveFormat(sampleRate, bitsPerSample, channels);

                    // Create buffered wave provider
                    _waveProvider = new BufferedWaveProvider(waveFormat)
                    {
                        BufferDuration = TimeSpan.FromSeconds(5), // 5 second buffer
                        DiscardOnBufferOverflow = true
                    };

                    // Create wave output device
                    _waveOut = new WaveOutEvent
                    {
                        DesiredLatency = 100, // 100ms latency for responsiveness
                        NumberOfBuffers = 3
                    };

                    _waveOut.Init(_waveProvider);
                    _waveOut.PlaybackStopped += OnPlaybackStopped;

                    _isInitialized = true;
                    _logger.LogInformation("Audio player initialized: {SampleRate}Hz, {Channels} channel(s), {BitsPerSample}-bit", 
                        sampleRate, channels, bitsPerSample);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize audio player");
                throw;
            }
        }

        /// <summary>
        /// Adds PCM audio data to the playback buffer
        /// </summary>
        public async Task AddAudioDataAsync(byte[] audioData)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Audio player not initialized. Call Initialize() first.");
            }

            try
            {
                lock (_lock)
                {
                    _waveProvider?.AddSamples(audioData, 0, audioData.Length);

                    // Start playback if not already playing
                    if (_waveOut?.PlaybackState != PlaybackState.Playing)
                    {
                        _waveOut?.Play();
                        PlaybackStarted?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding audio data to player");
                throw;
            }
        }

        /// <summary>
        /// Plays audio data directly (compatibility method)
        /// </summary>
        public async Task PlayAudioAsync(byte[] audioData)
        {
            await AddAudioDataAsync(audioData);
        }

        /// <summary>
        /// Sets the playback volume asynchronously
        /// </summary>
        public async Task SetVolumeAsync(double volume)
        {
            await Task.Run(() => Volume = volume);
        }

        /// <summary>
        /// Clears the audio buffer and stops playback
        /// </summary>
        public async Task ClearBufferAsync()
        {
            try
            {
                lock (_lock)
                {
                    _waveOut?.Stop();
                    _waveProvider?.ClearBuffer();
                }
                _logger.LogDebug("Audio buffer cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing audio buffer");
                throw;
            }
        }

        /// <summary>
        /// Stops audio playback
        /// </summary>
        public async Task StopAsync()
        {
            try
            {
                lock (_lock)
                {
                    _waveOut?.Stop();
                }
                _logger.LogDebug("Audio playback stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping audio playback");
                throw;
            }
        }

        /// <summary>
        /// Pauses audio playback
        /// </summary>
        public async Task PauseAsync()
        {
            try
            {
                lock (_lock)
                {
                    _waveOut?.Pause();
                }
                _logger.LogDebug("Audio playback paused");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pausing audio playback");
                throw;
            }
        }

        /// <summary>
        /// Resumes audio playback
        /// </summary>
        public async Task ResumeAsync()
        {
            try
            {
                lock (_lock)
                {
                    _waveOut?.Play();
                }
                _logger.LogDebug("Audio playback resumed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming audio playback");
                throw;
            }
        }

        /// <summary>
        /// Gets available audio output devices
        /// </summary>
        public static string[] GetAvailableDevices()
        {
            var devices = new string[WaveOut.DeviceCount];
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var deviceInfo = WaveOut.GetCapabilities(i);
                devices[i] = deviceInfo.ProductName;
            }
            return devices;
        }

        /// <summary>
        /// Handles playback stopped event
        /// </summary>
        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                _logger.LogError(e.Exception, "Audio playback stopped due to error");
            }

            PlaybackStopped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Disposes of the audio player resources
        /// </summary>
        public void Dispose()
        {
            try
            {
                lock (_lock)
                {
                    if (_waveOut != null)
                    {
                        _waveOut.PlaybackStopped -= OnPlaybackStopped;
                        _waveOut.Stop();
                        _waveOut.Dispose();
                        _waveOut = null;
                    }

                    _waveProvider = null;
                    _isInitialized = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing audio player");
            }
        }
    }
}

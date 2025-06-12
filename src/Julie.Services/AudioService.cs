using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Julie.Core.Audio;
using NAudio.Wave;

namespace Julie.Services
{
    /// <summary>
    /// Audio service implementation for capturing and playing audio
    /// </summary>
    public class AudioService : IAudioService
    {
        private readonly ILogger<AudioService> _logger;
        private readonly AudioCapture _audioCapture;
        private readonly AudioPlayer _audioPlayer;
        private bool _isCapturing;
        private bool _isPlaying;
        private double _volume = 1.0;

        public event EventHandler<byte[]>? AudioCaptured;
        public event EventHandler<bool>? VoiceActivityDetected;
        public event EventHandler<double>? InputVolumeChanged;

        public bool IsCapturing => _isCapturing;
        public bool IsPlaying => _isPlaying;
        public double Volume 
        { 
            get => _volume; 
            set 
            { 
                _volume = Math.Clamp(value, 0.0, 1.0);
                _ = SetVolumeAsync(_volume);
            } 
        }

        public AudioService(ILogger<AudioService> logger)
        {
            _logger = logger;
            _audioCapture = new AudioCapture(logger);
            _audioPlayer = new AudioPlayer(logger);

            // Wire up events
            _audioCapture.AudioDataAvailable += OnAudioDataAvailable;
        }

        private void OnAudioDataAvailable(object? sender, byte[] audioData)
        {
            AudioCaptured?.Invoke(this, audioData);
            
            // Simple voice activity detection based on audio level
            var level = CalculateAudioLevel(audioData);
            InputVolumeChanged?.Invoke(this, level);
            
            // Trigger voice activity if level is above threshold
            const double voiceThreshold = 0.01;
            VoiceActivityDetected?.Invoke(this, level > voiceThreshold);
        }

        private double CalculateAudioLevel(byte[] audioData)
        {
            if (audioData.Length == 0) return 0.0;
            
            // Convert bytes to 16-bit samples and calculate RMS
            var samples = new short[audioData.Length / 2];
            Buffer.BlockCopy(audioData, 0, samples, 0, audioData.Length);
            
            double sum = 0;
            foreach (var sample in samples)
            {
                sum += sample * sample;
            }
            
            return Math.Sqrt(sum / samples.Length) / short.MaxValue;
        }

        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing audio service");
                // Initialization logic if needed
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize audio service");
                throw;
            }
        }

        public async Task StartCaptureAsync()
        {
            try
            {
                _logger.LogInformation("Starting audio capture");
                var result = await _audioCapture.StartRecordingAsync();
                _isCapturing = result;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start audio capture");
                throw;
            }
        }

        public async Task StopCaptureAsync()
        {
            try
            {
                _logger.LogInformation("Stopping audio capture");
                var result = await _audioCapture.StopRecordingAsync();
                _isCapturing = !result;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop audio capture");
                throw;
            }
        }

        public async Task PlayAudioAsync(byte[] audioData)
        {
            try
            {
                _logger.LogInformation("Playing audio data ({Length} bytes)", audioData.Length);
                _isPlaying = true;
                await _audioPlayer.PlayAudioAsync(audioData);
                _isPlaying = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to play audio");
                _isPlaying = false;
                throw;
            }
        }

        public async Task StopPlaybackAsync()
        {
            try
            {
                _logger.LogInformation("Stopping audio playback");
                await _audioPlayer.StopAsync();
                _isPlaying = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop audio playback");
                throw;
            }
        }

        public string[] GetInputDevices()
        {
            try
            {
                _logger.LogInformation("Getting input devices");
                var devices = new List<string>();
                
                for (int i = 0; i < WaveIn.DeviceCount; i++)
                {
                    var capabilities = WaveIn.GetCapabilities(i);
                    devices.Add(capabilities.ProductName);
                }
                
                return devices.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get input devices");
                return Array.Empty<string>();
            }
        }

        public string[] GetOutputDevices()
        {
            try
            {
                _logger.LogInformation("Getting output devices");
                var devices = new List<string>();
                
                for (int i = 0; i < WaveOut.DeviceCount; i++)
                {
                    var capabilities = WaveOut.GetCapabilities(i);
                    devices.Add(capabilities.ProductName);
                }
                
                return devices.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get output devices");
                return Array.Empty<string>();
            }
        }

        public async Task SetInputDeviceAsync(int deviceIndex)
        {
            try
            {
                _logger.LogInformation("Setting input device to index: {DeviceIndex}", deviceIndex);
                
                // Stop current capture if running
                if (_isCapturing)
                {
                    await StopCaptureAsync();
                }
                
                // Update audio capture device
                // Note: This would require changes to AudioCapture to support device selection
                // For now, just log the action
                _logger.LogInformation("Input device set to: {DeviceIndex}", deviceIndex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set input device");
                throw;
            }
        }

        public async Task SetOutputDeviceAsync(int deviceIndex)
        {
            try
            {
                _logger.LogInformation("Setting output device to index: {DeviceIndex}", deviceIndex);
                
                // Stop current playback if running
                if (_isPlaying)
                {
                    await StopPlaybackAsync();
                }
                
                // Update audio player device
                // Note: This would require changes to AudioPlayer to support device selection
                // For now, just log the action
                _logger.LogInformation("Output device set to: {DeviceIndex}", deviceIndex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set output device");
                throw;
            }
        }

        private async Task SetVolumeAsync(double volume)
        {
            try
            {
                await _audioPlayer.SetVolumeAsync(volume);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set volume");
            }
        }

        public async Task DisposeAsync()
        {
            try
            {
                if (_isCapturing)
                {
                    await StopCaptureAsync();
                }
                
                if (_isPlaying)
                {
                    await StopPlaybackAsync();
                }
                
                _audioCapture?.Dispose();
                _audioPlayer?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during audio service disposal");
            }
        }

        public void Dispose()
        {
            DisposeAsync().Wait(TimeSpan.FromSeconds(2));
        }
    }
}

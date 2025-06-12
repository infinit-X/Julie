using System;
using System.Threading.Tasks;

namespace Julie.Services
{
    /// <summary>
    /// Service for managing audio input and output
    /// </summary>
    public interface IAudioService
    {
        /// <summary>
        /// Event fired when audio data is captured from microphone
        /// </summary>
        event EventHandler<byte[]> AudioCaptured;

        /// <summary>
        /// Event fired when voice activity is detected
        /// </summary>
        event EventHandler<bool> VoiceActivityDetected;

        /// <summary>
        /// Event fired when audio input volume changes
        /// </summary>
        event EventHandler<double> InputVolumeChanged;

        /// <summary>
        /// Gets whether audio capture is active
        /// </summary>
        bool IsCapturing { get; }

        /// <summary>
        /// Gets whether audio is currently playing
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// Gets or sets the output volume (0.0 to 1.0)
        /// </summary>
        double Volume { get; set; }

        /// <summary>
        /// Initializes the audio service
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Starts capturing audio from the microphone
        /// </summary>
        Task StartCaptureAsync();

        /// <summary>
        /// Stops capturing audio from the microphone
        /// </summary>
        Task StopCaptureAsync();

        /// <summary>
        /// Plays audio data through speakers
        /// </summary>
        Task PlayAudioAsync(byte[] audioData);

        /// <summary>
        /// Stops audio playback
        /// </summary>
        Task StopPlaybackAsync();

        /// <summary>
        /// Gets available audio input devices
        /// </summary>
        string[] GetInputDevices();

        /// <summary>
        /// Gets available audio output devices
        /// </summary>
        string[] GetOutputDevices();

        /// <summary>
        /// Sets the audio input device
        /// </summary>
        Task SetInputDeviceAsync(int deviceIndex);

        /// <summary>
        /// Sets the audio output device
        /// </summary>
        Task SetOutputDeviceAsync(int deviceIndex);

        /// <summary>
        /// Cleans up audio resources
        /// </summary>
        Task DisposeAsync();
    }
}

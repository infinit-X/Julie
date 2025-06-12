using System;
using System.Threading.Tasks;
using Julie.Core.Models;

namespace Julie.Services
{
    /// <summary>
    /// Main service for Julie AI assistant functionality
    /// </summary>
    public interface IJulieService
    {
        /// <summary>
        /// Event fired when a new message is received from Julie
        /// </summary>
        event EventHandler<Message> MessageReceived;

        /// <summary>
        /// Event fired when Julie starts or stops speaking
        /// </summary>
        event EventHandler<bool> IsSpeakingChanged;

        /// <summary>
        /// Event fired when the connection status changes
        /// </summary>
        event EventHandler<bool> ConnectionStatusChanged;

        /// <summary>
        /// Gets the current conversation
        /// </summary>
        Conversation? CurrentConversation { get; }

        /// <summary>
        /// Gets whether Julie is currently connected to the Live API
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets whether Julie is currently speaking
        /// </summary>
        bool IsSpeaking { get; }

        /// <summary>
        /// Initializes the Julie service with user settings
        /// </summary>
        Task InitializeAsync(UserSettings settings);

        /// <summary>
        /// Starts a new conversation
        /// </summary>
        Task<Conversation> StartNewConversationAsync();

        /// <summary>
        /// Sends a text message to Julie
        /// </summary>
        Task SendTextMessageAsync(string message);

        /// <summary>
        /// Sends audio data to Julie
        /// </summary>
        Task SendAudioMessageAsync(byte[] audioData);

        /// <summary>
        /// Sends screen context to Julie
        /// </summary>
        Task SendScreenContextAsync(byte[] imageData);

        /// <summary>
        /// Stops Julie from speaking
        /// </summary>
        Task StopSpeakingAsync();

        /// <summary>
        /// Updates Julie's configuration
        /// </summary>
        Task UpdateConfigurationAsync(UserSettings settings);

        /// <summary>
        /// Gets conversation history
        /// </summary>
        Task<Conversation[]> GetConversationHistoryAsync();

        /// <summary>
        /// Loads a specific conversation
        /// </summary>
        Task LoadConversationAsync(string conversationId);

        /// <summary>
        /// Deletes a conversation
        /// </summary>
        Task DeleteConversationAsync(string conversationId);

        /// <summary>
        /// Cleans up resources
        /// </summary>
        Task ShutdownAsync();
    }
}

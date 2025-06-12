using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Julie.Core.API;
using Julie.Core.Models;

namespace Julie.Services
{
    /// <summary>
    /// Main service implementation for Julie AI assistant functionality
    /// </summary>
    public class JulieService : IJulieService
    {
        private readonly ILogger<JulieService> _logger;
        private readonly LiveApiClient _liveApiClient;
        private readonly List<Conversation> _conversations = new();
        private Conversation? _currentConversation;
        private bool _isInitialized;
        private bool _isConnected;
        private bool _isSpeaking;

        public event EventHandler<Message>? MessageReceived;
        public event EventHandler<bool>? IsSpeakingChanged;
        public event EventHandler<bool>? ConnectionStatusChanged;

        public Conversation? CurrentConversation => _currentConversation;
        public bool IsConnected => _isConnected;
        public bool IsSpeaking => _isSpeaking;

        public JulieService(ILogger<JulieService> logger)
        {
            _logger = logger;
            _liveApiClient = new LiveApiClient(logger);

            // Wire up Live API events
            _liveApiClient.MessageReceived += OnLiveApiMessageReceived;
            _liveApiClient.Connected += OnLiveApiConnected;
            _liveApiClient.Disconnected += OnLiveApiDisconnected;
            _liveApiClient.Error += OnLiveApiError;
        }

        public async Task InitializeAsync(UserSettings settings)
        {
            try
            {
                _logger.LogInformation("Initializing Julie service...");
                
                // Connect to Live API
                await _liveApiClient.ConnectAsync(settings.ApiKey);
                
                _isInitialized = true;
                _logger.LogInformation("Julie service initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Julie service");
                throw;
            }
        }

        public async Task<Conversation> StartNewConversationAsync()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Service not initialized. Call InitializeAsync first.");
            }

            try
            {
                _logger.LogInformation("Starting new conversation");
                
                var conversation = new Conversation
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = $"Conversation {DateTime.Now:MM/dd HH:mm}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _conversations.Add(conversation);
                _currentConversation = conversation;
                
                return conversation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start new conversation");
                throw;
            }
        }

        public async Task SendTextMessageAsync(string message)
        {
            if (!_isInitialized || !_isConnected)
            {
                throw new InvalidOperationException("Service not connected. Call InitializeAsync first.");
            }

            try
            {
                _logger.LogInformation("Sending text message: {Message}", message);
                
                // Add user message to current conversation
                if (_currentConversation != null)
                {
                    var userMessage = new Message
                    {
                        Id = Guid.NewGuid().ToString(),
                        Role = MessageRole.User,
                        Content = message,
                        Timestamp = DateTime.UtcNow
                    };
                    _currentConversation.AddMessage(userMessage);
                }

                // Send to Live API
                await _liveApiClient.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send text message");
                throw;
            }
        }

        public async Task SendAudioMessageAsync(byte[] audioData)
        {
            if (!_isInitialized || !_isConnected)
            {
                throw new InvalidOperationException("Service not connected.");
            }

            try
            {
                _logger.LogInformation("Sending audio message ({Length} bytes)", audioData.Length);
                await _liveApiClient.SendAudioDataAsync(audioData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send audio message");
                throw;
            }
        }

        public async Task SendScreenContextAsync(byte[] imageData)
        {
            if (!_isInitialized || !_isConnected)
            {
                throw new InvalidOperationException("Service not connected.");
            }

            try
            {
                _logger.LogInformation("Sending screen context ({Length} bytes)", imageData.Length);
                await _liveApiClient.SendImageDataAsync(imageData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send screen context");
                throw;
            }
        }

        public async Task StopSpeakingAsync()
        {
            try
            {
                _logger.LogInformation("Stopping speaking");
                await _liveApiClient.InterruptAsync();
                
                _isSpeaking = false;
                IsSpeakingChanged?.Invoke(this, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop speaking");
                throw;
            }
        }

        public async Task UpdateConfigurationAsync(UserSettings settings)
        {
            try
            {
                _logger.LogInformation("Updating configuration");
                
                // Update Live API configuration if needed
                // This would reconnect with new settings
                if (_isConnected)
                {
                    await _liveApiClient.DisconnectAsync();
                    await _liveApiClient.ConnectAsync(settings.ApiKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update configuration");
                throw;
            }
        }

        public async Task<Conversation[]> GetConversationHistoryAsync()
        {
            try
            {
                _logger.LogInformation("Getting conversation history");
                return _conversations.OrderByDescending(c => c.UpdatedAt).ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get conversation history");
                throw;
            }
        }

        public async Task LoadConversationAsync(string conversationId)
        {
            try
            {
                _logger.LogInformation("Loading conversation: {ConversationId}", conversationId);
                
                var conversation = _conversations.FirstOrDefault(c => c.Id == conversationId);
                if (conversation == null)
                {
                    throw new ArgumentException($"Conversation not found: {conversationId}");
                }

                _currentConversation = conversation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load conversation");
                throw;
            }
        }

        public async Task DeleteConversationAsync(string conversationId)
        {
            try
            {
                _logger.LogInformation("Deleting conversation: {ConversationId}", conversationId);
                
                var conversation = _conversations.FirstOrDefault(c => c.Id == conversationId);
                if (conversation != null)
                {
                    _conversations.Remove(conversation);
                    
                    if (_currentConversation?.Id == conversationId)
                    {
                        _currentConversation = null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete conversation");
                throw;
            }
        }

        public async Task ShutdownAsync()
        {
            try
            {
                _logger.LogInformation("Shutting down Julie service");
                
                if (_isConnected)
                {
                    await _liveApiClient.DisconnectAsync();
                }
                
                _isInitialized = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during shutdown");
                throw;
            }
        }

        private void OnLiveApiMessageReceived(object? sender, LiveApiMessageEventArgs e)
        {
            try
            {
                var message = new Message
                {
                    Id = Guid.NewGuid().ToString(),
                    Role = MessageRole.Assistant,
                    Content = e.Text ?? "",
                    Timestamp = DateTime.UtcNow
                };

                // Add to current conversation
                if (_currentConversation != null)
                {
                    _currentConversation.AddMessage(message);
                    _currentConversation.UpdatedAt = DateTime.UtcNow;
                }

                MessageReceived?.Invoke(this, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling message received");
            }
        }

        private void OnLiveApiConnected(object? sender, EventArgs e)
        {
            _isConnected = true;
            _logger.LogInformation("Connected to Live API");
            ConnectionStatusChanged?.Invoke(this, true);
        }

        private void OnLiveApiDisconnected(object? sender, EventArgs e)
        {
            _isConnected = false;
            _logger.LogInformation("Disconnected from Live API");
            ConnectionStatusChanged?.Invoke(this, false);
        }

        private void OnLiveApiError(object? sender, LiveApiErrorEventArgs e)
        {
            _logger.LogError("Live API error: {Error}", e.Error);
        }

        public void Dispose()
        {
            try
            {
                ShutdownAsync().Wait(TimeSpan.FromSeconds(5));
                _liveApiClient?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during disposal");
            }
        }
    }
}

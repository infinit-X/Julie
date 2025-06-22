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
    /// Updated Julie service implementation using simple Gemini API HTTP client
    /// </summary>
    public class JulieService : IJulieService
    {
        private readonly ILogger<JulieService> _logger;
        private readonly SimpleGeminiClient _geminiClient;
        private readonly List<Conversation> _conversations = new();
        private Conversation? _currentConversation;
        private bool _isInitialized;
        private bool _isSpeaking;

        public event EventHandler<Message>? MessageReceived;
        public event EventHandler<bool>? IsSpeakingChanged;
        public event EventHandler<bool>? ConnectionStatusChanged;

        public Conversation? CurrentConversation => _currentConversation;
        public bool IsConnected => _geminiClient?.IsConnected ?? false;
        public bool IsSpeaking => _isSpeaking;        public JulieService(ILogger<JulieService> logger)
        {
            _logger = logger;
            // Create a logger for SimpleGeminiClient using the same logger factory
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Information));
            var geminiLogger = loggerFactory.CreateLogger<SimpleGeminiClient>();
            _geminiClient = new SimpleGeminiClient(geminiLogger);

            // Wire up Gemini API events
            _geminiClient.MessageReceived += OnGeminiMessageReceived;
            _geminiClient.ConnectionStatusChanged += OnGeminiConnectionStatusChanged;
            _geminiClient.ErrorOccurred += OnGeminiErrorOccurred;
        }

        public async Task InitializeAsync(UserSettings settings)
        {
            try
            {
                _logger.LogInformation("Initializing Julie service with Gemini API...");
                
                if (string.IsNullOrEmpty(settings.ApiKey))
                {
                    throw new ArgumentException("API key is required", nameof(settings));
                }                // Connect to Gemini API
                var connected = await _geminiClient.ConnectAsync(settings.ApiKey);
                
                if (connected)
                {
                    _isInitialized = true;
                    _logger.LogInformation("Julie service initialized successfully with Gemini API");
                }
                else
                {
                    throw new Exception("Failed to connect to Gemini API");
                }
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

                _logger.LogInformation("New conversation started: {ConversationId}", conversation.Id);
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
            if (!_isInitialized || !IsConnected)
            {
                throw new InvalidOperationException("Service not initialized or not connected");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Message cannot be empty", nameof(message));
            }

            try
            {
                _logger.LogInformation("Sending text message: {Message}", message);

                // Add user message to current conversation
                if (_currentConversation != null)
                {
                    var userMessage = new Message
                    {
                        Role = MessageRole.User,
                        Text = message,
                        Timestamp = DateTime.Now,
                        IsComplete = true
                    };
                    
                    _currentConversation.AddMessage(userMessage);
                    MessageReceived?.Invoke(this, userMessage);
                }                // Send to Gemini API
                await _geminiClient.SendTextMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send text message");
                throw;
            }
        }

        public async Task SendTextMessageWithScreenContextAsync(string message, byte[] screenImageData)
        {
            if (!_isInitialized || !IsConnected)
            {
                throw new InvalidOperationException("Service not initialized or not connected");
            }

            try
            {
                _logger.LogInformation("Sending text message with screen context: {Message}", message);

                // Add user message to current conversation
                if (_currentConversation != null)
                {
                    var userMessage = new Message
                    {
                        Role = MessageRole.User,
                        Text = message + " [Screen context included]",
                        Timestamp = DateTime.Now,
                        IsComplete = true
                    };
                    
                    _currentConversation.AddMessage(userMessage);
                    MessageReceived?.Invoke(this, userMessage);
                }                // Send to Gemini API with screen context (placeholder for now)
                await _geminiClient.SendTextMessageAsync(message + " [Note: Screen context feature will be implemented with Live API]");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send text message with screen context");
                throw;
            }
        }

        public async Task SendVoiceMessageAsync(byte[] audioData)
        {
            // For now, this is a placeholder. Voice processing would need additional implementation
            // with the Live API for real-time audio processing
            _logger.LogInformation("Voice message received (placeholder implementation)");
            
            // Convert to text placeholder for now
            await SendTextMessageAsync("[Voice message - transcription not implemented yet]");
        }

        public async Task StartVoiceInputAsync()
        {
            _logger.LogInformation("Starting voice input (placeholder implementation)");
            // Placeholder for voice input functionality
            // This would integrate with the Live API for real-time voice processing
        }

        public async Task StopVoiceInputAsync()
        {
            _logger.LogInformation("Stopping voice input (placeholder implementation)");
            // Placeholder for stopping voice input
        }

        public async Task DisconnectAsync()
        {
            try
            {
                _logger.LogInformation("Disconnecting Julie service");
                  if (_geminiClient != null)
                {
                    await _geminiClient.DisconnectAsync();
                }
                
                _isInitialized = false;
                _currentConversation = null;
                
                _logger.LogInformation("Julie service disconnected");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during disconnect");
            }
        }

        public IEnumerable<Conversation> GetConversations()
        {
            return _conversations.AsReadOnly();
        }

        public Conversation? GetConversation(string id)
        {
            return _conversations.FirstOrDefault(c => c.Id == id);
        }

        public async Task DeleteConversationAsync(string id)
        {
            var conversation = _conversations.FirstOrDefault(c => c.Id == id);
            if (conversation != null)
            {
                _conversations.Remove(conversation);
                
                if (_currentConversation?.Id == id)
                {
                    _currentConversation = null;
                }
                
                _logger.LogInformation("Conversation deleted: {ConversationId}", id);
            }
        }

        private void OnGeminiMessageReceived(object? sender, Message message)
        {
            try
            {
                _logger.LogInformation("Received message from Gemini: {Message}", message.Text);
                
                // Add to current conversation
                if (_currentConversation != null)
                {
                    _currentConversation.AddMessage(message);
                    _currentConversation.UpdatedAt = DateTime.UtcNow;
                }
                
                // Forward to UI
                MessageReceived?.Invoke(this, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing received message from Gemini");
            }
        }

        private void OnGeminiConnectionStatusChanged(object? sender, bool isConnected)
        {
            _logger.LogInformation("Gemini connection status changed: {IsConnected}", isConnected);
            ConnectionStatusChanged?.Invoke(this, isConnected);
        }

        private void OnGeminiErrorOccurred(object? sender, Exception exception)
        {
            _logger.LogError(exception, "Gemini API error occurred");
            
            // Create error message for UI
            var errorMessage = new Message
            {
                Role = MessageRole.System,
                Text = $"Error: {exception.Message}",
                Timestamp = DateTime.Now,
                IsComplete = true
            };
            
            MessageReceived?.Invoke(this, errorMessage);
        }
        
        public async Task SendAudioMessageAsync(byte[] audioData)
        {
            // For now, this is a placeholder for audio processing
            _logger.LogInformation("Audio message received (placeholder implementation)");
            
            // Convert to text placeholder for now
            await SendTextMessageAsync("[Audio message - transcription not implemented yet]");
        }

        public async Task SendScreenContextAsync(byte[] screenImageData)
        {
            if (!_isInitialized || !IsConnected)
            {
                throw new InvalidOperationException("Service not initialized or not connected");
            }

            try
            {
                _logger.LogInformation("Processing screen context");

                // Add screen context message to current conversation
                if (_currentConversation != null)
                {
                    var screenMessage = new Message
                    {
                        Role = MessageRole.User,
                        Text = "[Screen capture provided for context analysis]",
                        Timestamp = DateTime.Now,
                        IsComplete = true
                    };
                    
                    _currentConversation.AddMessage(screenMessage);
                    MessageReceived?.Invoke(this, screenMessage);
                }

                // Send to Gemini API with screen context placeholder
                await _geminiClient.SendTextMessageAsync("Please analyze the screen content provided. [Note: Screen analysis feature will be implemented with multimodal capabilities]");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process screen context");
                throw;
            }
        }

        public async Task StopSpeakingAsync()
        {
            _logger.LogInformation("Stopping speech (placeholder implementation)");
            _isSpeaking = false;
            IsSpeakingChanged?.Invoke(this, false);
        }

        public async Task UpdateConfigurationAsync(UserSettings settings)
        {
            try
            {
                _logger.LogInformation("Updating configuration");
                
                // Reconnect with new settings if API key changed
                if (!string.IsNullOrEmpty(settings.ApiKey) && _isInitialized)
                {
                    await _geminiClient.DisconnectAsync();
                    await _geminiClient.ConnectAsync(settings.ApiKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update configuration");
                throw;
            }
        }        public async Task<Conversation[]> GetConversationHistoryAsync()
        {
            _logger.LogInformation("Getting conversation history");
            return _conversations.ToArray();
        }

        public async Task LoadConversationAsync(string conversationId)
        {
            try
            {
                _logger.LogInformation("Loading conversation: {ConversationId}", conversationId);
                
                var conversation = _conversations.FirstOrDefault(c => c.Id == conversationId);
                if (conversation != null)
                {
                    _currentConversation = conversation;
                    _logger.LogInformation("Conversation loaded successfully");
                }
                else
                {
                    throw new ArgumentException($"Conversation not found: {conversationId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load conversation");
                throw;
            }
        }

        public async Task ShutdownAsync()
        {
            try
            {
                _logger.LogInformation("Shutting down Julie service");
                await DisconnectAsync();
                _conversations.Clear();
                _currentConversation = null;
                _isInitialized = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during shutdown");
            }
        }

        public void Dispose()
        {            try
            {
                if (_geminiClient != null)
                {
                    _geminiClient.MessageReceived -= OnGeminiMessageReceived;
                    _geminiClient.ConnectionStatusChanged -= OnGeminiConnectionStatusChanged;
                    _geminiClient.ErrorOccurred -= OnGeminiErrorOccurred;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during disposal");
            }
        }
    }
}

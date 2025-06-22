using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Julie.Core.Models;
using Julie.Services;
using Microsoft.Extensions.Logging;

namespace Julie.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the chat interface
    /// </summary>
    public partial class ChatViewModel : ObservableObject
    {
        private readonly IJulieService _julieService;
        private readonly ILogger<ChatViewModel> _logger;

        [ObservableProperty]
        private ObservableCollection<Message> _messages = new();

        [ObservableProperty]
        private string _currentMessage = string.Empty;

        [ObservableProperty]
        private bool _isTyping;

        [ObservableProperty]
        private bool _isJulieThinking;

        [ObservableProperty]
        private Conversation? _currentConversation;

        public ICommand SendMessageCommand { get; }
        public ICommand ClearChatCommand { get; }
        public ICommand StartVoiceInputCommand { get; }
        public ICommand StopVoiceInputCommand { get; }        public ChatViewModel(IJulieService julieService, ILogger<ChatViewModel> logger)
        {
            _julieService = julieService;
            _logger = logger;

            // Initialize commands
            SendMessageCommand = new AsyncRelayCommand(SendMessageAsync, CanSendMessage);
            ClearChatCommand = new RelayCommand(ClearChat);
            StartVoiceInputCommand = new AsyncRelayCommand(StartVoiceInputAsync);
            StopVoiceInputCommand = new AsyncRelayCommand(StopVoiceInputAsync);

            // Subscribe to property changes to update command states
            PropertyChanged += OnPropertyChanged;

            // Add welcome message
            AddWelcomeMessage();
        }

        private void AddWelcomeMessage()
        {
            var welcomeMessage = new Message
            {
                Role = MessageRole.System,
                Text = "👋 Welcome to Julie AI Assistant! I'm ready to help you with any questions or tasks. Type a message or use the voice recording feature to get started.",
                Timestamp = DateTime.Now,
                IsComplete = true
            };
            
            Messages.Add(welcomeMessage);
        }

        private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentMessage))
            {
                ((AsyncRelayCommand)SendMessageCommand).NotifyCanExecuteChanged();
            }
        }

        private bool CanSendMessage()
        {
            return !string.IsNullOrWhiteSpace(CurrentMessage) && !IsJulieThinking;
        }

        private async Task SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentMessage))
                return;

            try
            {
                var messageText = CurrentMessage.Trim();
                CurrentMessage = string.Empty;                // Add user message to chat
                var userMessage = new Message
                {
                    Role = MessageRole.User,
                    Text = messageText,
                    Timestamp = DateTime.Now
                };

                AddMessage(userMessage);

                // Send to Julie service
                IsJulieThinking = true;
                await _julieService.SendTextMessageAsync(messageText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message");
                  // Add error message to chat
                var errorMessage = new Message
                {
                    Role = MessageRole.System,
                    Text = $"Error sending message: {ex.Message}",
                    Timestamp = DateTime.Now
                };
                AddMessage(errorMessage);
            }
            finally
            {
                IsJulieThinking = false;
            }
        }

        private void ClearChat()
        {
            try
            {
                Messages.Clear();
                _logger.LogInformation("Chat cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear chat");
            }
        }

        private async Task StartVoiceInputAsync()
        {
            try
            {
                _logger.LogInformation("Starting voice input");
                // Voice input logic would be implemented here
                // This would interact with the audio service
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start voice input");
            }
        }

        private async Task StopVoiceInputAsync()
        {
            try
            {
                _logger.LogInformation("Stopping voice input");
                // Voice input stop logic would be implemented here
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop voice input");
            }
        }

        public void AddMessage(Message message)
        {
            try
            {
                // Add to current conversation if available
                CurrentConversation?.AddMessage(message);

                // Add to UI collection on UI thread
                if (System.Threading.SynchronizationContext.Current != null)
                {
                    Messages.Add(message);
                }
                else
                {                    // If not on UI thread, marshal to UI thread
                    Application.Current?.Dispatcher.BeginInvoke(() =>
                    {
                        Messages.Add(message);
                    });
                }                // If this is a completed assistant message, stop thinking indicator
                if (message.Role == MessageRole.Assistant && message.IsComplete)
                {
                    IsJulieThinking = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add message to chat");
            }
        }

        public void LoadConversation(Conversation conversation)
        {
            try
            {
                CurrentConversation = conversation;
                Messages.Clear();

                foreach (var message in conversation.Messages)
                {
                    Messages.Add(message);
                }

                _logger.LogInformation("Loaded conversation: {ConversationId}", conversation.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load conversation");
            }
        }

        public void UpdateMessage(Message message)
        {
            try
            {
                var existingMessage = Messages.FirstOrDefault(m => m.Id == message.Id);
                if (existingMessage != null)
                {
                    var index = Messages.IndexOf(existingMessage);
                    Messages[index] = message;
                }
                else
                {
                    AddMessage(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update message");
            }
        }

        public void StartTypingIndicator()
        {
            IsTyping = true;
        }

        public void StopTypingIndicator()
        {
            IsTyping = false;
        }
    }
}

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Julie.Core.Models;

namespace Julie.Core.API
{
    /// <summary>
    /// Simple HTTP-based Gemini API client for basic chat functionality
    /// </summary>
    public class SimpleGeminiClient
    {
        private readonly ILogger<SimpleGeminiClient> _logger;
        private readonly HttpClient _httpClient;
        private string? _apiKey;
        private bool _isConnected;

        public event EventHandler<bool>? ConnectionStatusChanged;
        public event EventHandler<Message>? MessageReceived;
        public event EventHandler<Exception>? ErrorOccurred;

        public bool IsConnected => _isConnected;

        public SimpleGeminiClient(ILogger<SimpleGeminiClient> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<bool> ConnectAsync(string apiKey, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Testing connection to Gemini API...");
                
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));
                }

                _apiKey = apiKey;
                
                // Test the API with a simple request
                var testResponse = await SendRequestAsync("Hello, are you working?", cancellationToken);
                
                if (!string.IsNullOrEmpty(testResponse))
                {
                    _isConnected = true;
                    _logger.LogInformation("Successfully connected to Gemini API");
                    ConnectionStatusChanged?.Invoke(this, true);
                    return true;
                }
                
                throw new Exception("Connection test failed - no response from API");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to Gemini API");
                _isConnected = false;
                ConnectionStatusChanged?.Invoke(this, false);
                ErrorOccurred?.Invoke(this, ex);
                return false;
            }
        }

        public async Task<bool> SendTextMessageAsync(string message, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_isConnected || string.IsNullOrEmpty(_apiKey))
                {
                    throw new InvalidOperationException("Not connected to Gemini API");
                }

                if (string.IsNullOrEmpty(message))
                {
                    throw new ArgumentException("Message cannot be null or empty", nameof(message));
                }

                _logger.LogInformation("Sending message to Gemini: {Message}", message);

                var responseText = await SendRequestAsync(message, cancellationToken);
                
                if (!string.IsNullOrEmpty(responseText))
                {
                    var responseMessage = new Message
                    {
                        Role = MessageRole.Assistant,
                        Text = responseText,
                        Timestamp = DateTime.Now,
                        IsComplete = true
                    };

                    _logger.LogInformation("Received response from Gemini: {Response}", responseText);
                    MessageReceived?.Invoke(this, responseMessage);
                    return true;
                }
                
                throw new Exception("No response received from Gemini API");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message to Gemini API");
                ErrorOccurred?.Invoke(this, ex);
                return false;
            }
        }

        private async Task<string?> SendRequestAsync(string message, CancellationToken cancellationToken)
        {
            try
            {
                // Use Gemini 1.5 Flash for basic chat
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={_apiKey}";
                
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = message }
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogDebug("Sending request to Gemini API: {Url}", url);
                
                var response = await _httpClient.PostAsync(url, content, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogDebug("Received response: {Response}", responseJson);
                    
                    var responseData = JsonSerializer.Deserialize<JsonElement>(responseJson);
                    
                    if (responseData.TryGetProperty("candidates", out var candidates) && 
                        candidates.GetArrayLength() > 0)
                    {
                        var firstCandidate = candidates[0];
                        if (firstCandidate.TryGetProperty("content", out var content_) &&
                            content_.TryGetProperty("parts", out var parts) &&
                            parts.GetArrayLength() > 0)
                        {
                            var firstPart = parts[0];
                            if (firstPart.TryGetProperty("text", out var text))
                            {
                                return text.GetString();
                            }
                        }
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Gemini API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    throw new Exception($"API request failed: {response.StatusCode} - {errorContent}");
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending request to Gemini API");
                throw;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                _isConnected = false;
                _apiKey = null;
                ConnectionStatusChanged?.Invoke(this, false);
                _logger.LogInformation("Disconnected from Gemini API");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during disconnect");
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}

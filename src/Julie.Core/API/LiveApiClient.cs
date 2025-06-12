using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Julie.Core.Models;

namespace Julie.Core.API
{
    /// <summary>
    /// Event arguments for Live API events
    /// </summary>
    public class LiveApiEventArgs : EventArgs
    {
        public string EventType { get; set; } = string.Empty;
        public object? Data { get; set; }
    }

    /// <summary>
    /// Event arguments for Live API message events
    /// </summary>
    public class LiveApiMessageEventArgs : EventArgs
    {
        public string? Text { get; set; }
        public byte[]? AudioData { get; set; }
        public string? FunctionCall { get; set; }
        public object? Metadata { get; set; }
    }

    /// <summary>
    /// Event arguments for Live API error events
    /// </summary>
    public class LiveApiErrorEventArgs : EventArgs
    {
        public string Error { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
        public string? Code { get; set; }
    }

    /// <summary>
    /// Configuration for Live API connection
    /// </summary>
    public class LiveApiConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "models/gemini-2.0-flash-exp";
        public List<string> ResponseModalities { get; set; } = new List<string> { "TEXT" };
        public string? SystemInstruction { get; set; }
        public List<FunctionDeclaration> Tools { get; set; } = new List<FunctionDeclaration>();
        public SpeechConfig? SpeechConfig { get; set; }
        public bool EnableScreenContext { get; set; } = false;
    }

    /// <summary>
    /// Speech configuration for Live API
    /// </summary>
    public class SpeechConfig
    {
        public string VoiceName { get; set; } = "Aoede";
        public string LanguageCode { get; set; } = "en-US";
        public double SpeechRate { get; set; } = 1.0;
    }

    /// <summary>
    /// Client for interacting with Google's Live API
    /// </summary>
    public class LiveApiClient : IDisposable
    {
        private readonly ILogger _logger;
        private ClientWebSocket? _webSocket;
        private CancellationTokenSource? _cancellationTokenSource;
        private LiveApiConfig? _config;
        private bool _isConnected = false;
        private readonly object _lock = new object();        public event EventHandler<LiveApiMessageEventArgs>? MessageReceived;
        public event EventHandler? Connected;
        public event EventHandler? Disconnected;
        public event EventHandler<LiveApiErrorEventArgs>? Error;
        public event EventHandler<LiveApiEventArgs>? AudioReceived;
        public event EventHandler<LiveApiEventArgs>? ToolCallReceived;
        public event EventHandler<LiveApiEventArgs>? TurnComplete;

        public bool IsConnected
        {
            get
            {
                lock (_lock)
                {
                    return _isConnected && _webSocket?.State == WebSocketState.Open;
                }
            }
        }

        public LiveApiClient(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Connects to the Live API with the specified configuration
        /// </summary>
        public async Task ConnectAsync(LiveApiConfig config, CancellationToken cancellationToken = default)
        {
            if (IsConnected)
            {
                await DisconnectAsync();
            }

            _config = config;
            _cancellationTokenSource = new CancellationTokenSource();
            
            try
            {
                _webSocket = new ClientWebSocket();
                
                var uri = new Uri($"wss://generativelanguage.googleapis.com/v1beta/models/{config.Model}:streamGenerateContent?key={config.ApiKey}");
                
                _logger.LogInformation("Connecting to Live API: {Uri}", uri);
                
                await _webSocket.ConnectAsync(uri, cancellationToken);
                
                lock (_lock)
                {
                    _isConnected = true;
                }

                // Send setup message
                await SendSetupMessage();

                // Start listening for messages
                _ = Task.Run(() => ListenForMessages(_cancellationTokenSource.Token));

                Connected?.Invoke(this, EventArgs.Empty);
                _logger.LogInformation("Connected to Live API successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to Live API");
                Error?.Invoke(this, new LiveApiErrorEventArgs { Error = "Connection failed", Exception = ex });
                throw;
            }
        }

        /// <summary>
        /// Connects to the Live API with just an API key (compatibility method)
        /// </summary>
        public async Task ConnectAsync(string apiKey, CancellationToken cancellationToken = default)
        {
            var config = new LiveApiConfig
            {
                ApiKey = apiKey,
                Model = "models/gemini-2.0-flash-exp",
                ResponseModalities = new List<string> { "TEXT", "AUDIO" }
            };
            
            await ConnectAsync(config, cancellationToken);
        }

        /// <summary>
        /// Sends a text message to the Live API
        /// </summary>
        public async Task SendMessageAsync(string message)
        {
            await SendTextMessage(message);
        }

        /// <summary>
        /// Sends audio data to the Live API
        /// </summary>
        public async Task SendAudioDataAsync(byte[] audioData)
        {
            await SendAudioMessage(audioData);
        }

        /// <summary>
        /// Sends image data to the Live API
        /// </summary>
        public async Task SendImageDataAsync(byte[] imageData)
        {
            await SendImageMessage(imageData);
        }

        /// <summary>
        /// Interrupts the current Live API response
        /// </summary>
        public async Task InterruptAsync()
        {
            await SendInterruptMessage();
        }

        /// <summary>
        /// Sends a setup message to configure the Live API session
        /// </summary>
        private async Task SendSetupMessage()
        {
            if (_config == null || _webSocket == null)
                return;

            var setupMessage = new
            {
                setup = new
                {
                    model = _config.Model,
                    generation_config = new
                    {
                        response_modalities = _config.ResponseModalities,
                        speech_config = _config.SpeechConfig != null ? new
                        {
                            voice_config = new
                            {
                                prebuilt_voice_config = new
                                {
                                    voice_name = _config.SpeechConfig.VoiceName
                                }
                            },
                            language_code = _config.SpeechConfig.LanguageCode
                        } : null
                    },
                    system_instruction = !string.IsNullOrEmpty(_config.SystemInstruction) ? new
                    {
                        parts = new[] { new { text = _config.SystemInstruction } }
                    } : null,
                    tools = _config.Tools.Count > 0 ? new[]
                    {
                        new { function_declarations = _config.Tools }
                    } : null
                }
            };

            var json = JsonSerializer.Serialize(setupMessage, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            await SendMessage(json);
        }

        /// <summary>
        /// Sends a text message to the Live API
        /// </summary>
        public async Task SendTextMessage(string text, bool turnComplete = true)
        {
            var message = new
            {
                client_content = new
                {
                    turns = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[] { new { text = text } }
                        }
                    },
                    turn_complete = turnComplete
                }
            };

            var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            await SendMessage(json);
        }

        /// <summary>
        /// Sends audio data to the Live API
        /// </summary>
        public async Task SendAudioMessage(byte[] audioData)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Not connected to Live API");
            }

            try
            {
                var audioBase64 = Convert.ToBase64String(audioData);
                var message = new
                {
                    clientContent = new
                    {
                        turns = new[]
                        {
                            new
                            {
                                role = "user",
                                parts = new[]
                                {
                                    new
                                    {
                                        inlineData = new
                                        {
                                            mimeType = "audio/pcm",
                                            data = audioBase64
                                        }
                                    }
                                }
                            }
                        }                    }
                };

                var json = JsonSerializer.Serialize(message);
                await SendMessage(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send audio message");
                throw;
            }
        }

        /// <summary>
        /// Sends image data to the Live API
        /// </summary>
        public async Task SendImageMessage(byte[] imageData)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Not connected to Live API");
            }

            try
            {
                var imageBase64 = Convert.ToBase64String(imageData);
                var message = new
                {
                    clientContent = new
                    {
                        turns = new[]
                        {
                            new
                            {
                                role = "user",
                                parts = new[]
                                {
                                    new
                                    {
                                        inlineData = new
                                        {
                                            mimeType = "image/png",
                                            data = imageBase64
                                        }
                                    }
                                }
                            }
                        }                    }
                };

                var json = JsonSerializer.Serialize(message);
                await SendMessage(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send image message");
                throw;
            }
        }

        /// <summary>
        /// Sends an interrupt message to stop current response
        /// </summary>
        public async Task SendInterruptMessage()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Not connected to Live API");
            }

            try
            {
                var message = new
                {
                    clientContent = new
                    {
                        interrupt = true
                    }                };

                var json = JsonSerializer.Serialize(message);
                await SendMessage(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send interrupt message");
                throw;
            }
        }

        /// <summary>
        /// Sends a tool response back to the Live API
        /// </summary>
        public async Task SendToolResponse(List<FunctionResponse> responses)
        {
            var message = new
            {
                tool_response = new
                {
                    function_responses = responses.Select(r => new
                    {
                        id = r.Id,
                        name = r.Name,
                        response = r.Response
                    })
                }
            };

            var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            await SendMessage(json);
        }

        /// <summary>
        /// Sends a raw message to the WebSocket
        /// </summary>
        private async Task SendMessage(string message)
        {
            if (_webSocket?.State != WebSocketState.Open)
            {
                _logger.LogWarning("Cannot send message: WebSocket is not open");
                return;
            }

            try
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                _logger.LogDebug("Sent message: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message");
                Error?.Invoke(this, new LiveApiErrorEventArgs { Error = "Send error", Exception = ex });
            }
        }

        /// <summary>
        /// Listens for incoming messages from the WebSocket
        /// </summary>
        private async Task ListenForMessages(CancellationToken cancellationToken)
        {
            if (_webSocket == null) return;

            var buffer = new byte[4096];
            var messageBuilder = new StringBuilder();

            try
            {
                while (_webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var chunk = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        messageBuilder.Append(chunk);

                        if (result.EndOfMessage)
                        {
                            var message = messageBuilder.ToString();
                            messageBuilder.Clear();
                            
                            await ProcessIncomingMessage(message);
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation("WebSocket connection closed by server");
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Message listening cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while listening for messages");
                Error?.Invoke(this, new LiveApiErrorEventArgs { Error = "Receive error", Exception = ex });
            }
            finally
            {
                lock (_lock)
                {
                    _isConnected = false;
                }
                Disconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Processes incoming messages from the Live API
        /// </summary>
        private async Task ProcessIncomingMessage(string message)
        {
            try
            {
                _logger.LogDebug("Received message: {Message}", message);
                
                using var document = JsonDocument.Parse(message);
                var root = document.RootElement;

                // Handle different message types
                if (root.TryGetProperty("serverContent", out var serverContent))
                {
                    await ProcessServerContent(serverContent);
                }
                else if (root.TryGetProperty("toolCall", out var toolCall))
                {
                    ProcessToolCall(toolCall);                }
                else if (root.TryGetProperty("setupComplete", out var _))
                {
                    _logger.LogInformation("Setup complete");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process incoming message: {Message}", message);
                Error?.Invoke(this, new LiveApiErrorEventArgs { Error = "Message processing error", Exception = ex });
            }
        }

        /// <summary>
        /// Processes server content messages
        /// </summary>
        private async Task ProcessServerContent(JsonElement serverContent)
        {
            if (serverContent.TryGetProperty("modelTurn", out var modelTurn))
            {
                if (modelTurn.TryGetProperty("parts", out var parts))
                {
                    foreach (var part in parts.EnumerateArray())
                    {
                        if (part.TryGetProperty("text", out var textElement))
                        {                            var text = textElement.GetString();
                            MessageReceived?.Invoke(this, new LiveApiMessageEventArgs { Text = text });
                        }
                        
                        if (part.TryGetProperty("inlineData", out var inlineData))
                        {
                            if (inlineData.TryGetProperty("data", out var dataElement) &&
                                inlineData.TryGetProperty("mimeType", out var mimeTypeElement))
                            {
                                var base64Data = dataElement.GetString();
                                var mimeType = mimeTypeElement.GetString();
                                
                                if (!string.IsNullOrEmpty(base64Data) && mimeType?.StartsWith("audio/") == true)
                                {
                                    var audioData = Convert.FromBase64String(base64Data);
                                    AudioReceived?.Invoke(this, new LiveApiEventArgs { EventType = "audio", Data = audioData });
                                }
                            }
                        }
                    }
                }
            }

            if (serverContent.TryGetProperty("turnComplete", out var turnCompleteElement) && 
                turnCompleteElement.GetBoolean())
            {
                TurnComplete?.Invoke(this, new LiveApiEventArgs { EventType = "turn_complete" });
            }
        }

        /// <summary>
        /// Processes tool call messages
        /// </summary>
        private void ProcessToolCall(JsonElement toolCall)
        {
            if (toolCall.TryGetProperty("functionCalls", out var functionCalls))
            {
                var toolCalls = new List<ToolCall>();
                
                foreach (var fc in functionCalls.EnumerateArray())
                {
                    var toolCallObj = new ToolCall();
                    
                    if (fc.TryGetProperty("id", out var idElement))
                        toolCallObj.Id = idElement.GetString() ?? "";
                    
                    if (fc.TryGetProperty("name", out var nameElement))
                        toolCallObj.Name = nameElement.GetString() ?? "";
                    
                    if (fc.TryGetProperty("args", out var argsElement))
                    {
                        foreach (var prop in argsElement.EnumerateObject())
                        {
                            toolCallObj.Arguments[prop.Name] = prop.Value.GetRawText();
                        }
                    }
                    
                    toolCalls.Add(toolCallObj);
                }
                
                ToolCallReceived?.Invoke(this, new LiveApiEventArgs { EventType = "tool_call", Data = toolCalls });
            }
        }

        /// <summary>
        /// Disconnects from the Live API
        /// </summary>
        public async Task DisconnectAsync()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                
                if (_webSocket?.State == WebSocketState.Open)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnecting", CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during disconnect");
            }
            finally
            {
                lock (_lock)
                {
                    _isConnected = false;
                }
                
                _webSocket?.Dispose();
                _webSocket = null;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        public void Dispose()
        {
            DisconnectAsync().GetAwaiter().GetResult();
        }
    }
}

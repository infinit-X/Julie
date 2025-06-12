using System;
using System.Collections.Generic;

namespace Julie.Core.Models
{
    /// <summary>
    /// Enumeration of message roles
    /// </summary>
    public enum MessageRole
    {
        User,
        Assistant,
        System
    }

    /// <summary>
    /// Represents a message in a conversation between the user and Julie
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Unique identifier for the message
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Role of the message sender (user, assistant)
        /// </summary>
        public MessageRole Role { get; set; }

        /// <summary>
        /// Text content of the message
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// Content property for backward compatibility
        /// </summary>
        public string? Content 
        { 
            get => Text; 
            set => Text = value; 
        }

        /// <summary>
        /// Audio data if the message contains audio
        /// </summary>
        public byte[]? AudioData { get; set; }

        /// <summary>
        /// MIME type for audio data
        /// </summary>
        public string? AudioMimeType { get; set; }

        /// <summary>
        /// Image data if the message contains images
        /// </summary>
        public byte[]? ImageData { get; set; }

        /// <summary>
        /// MIME type for image data
        /// </summary>
        public string? ImageMimeType { get; set; }

        /// <summary>
        /// Timestamp when the message was created
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Whether the message is complete (for streaming messages)
        /// </summary>
        public bool IsComplete { get; set; } = true;

        /// <summary>
        /// Indicates if this message is currently being generated
        /// </summary>
        public bool IsGenerating { get; set; } = false;

        /// <summary>
        /// Tool calls associated with this message
        /// </summary>
        public List<ToolCall>? ToolCalls { get; set; }

        /// <summary>
        /// Function responses for tool calls
        /// </summary>
        public List<FunctionResponse>? FunctionResponses { get; set; }
    }
}

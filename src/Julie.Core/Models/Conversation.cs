using System;
using System.Collections.Generic;
using System.Linq;

namespace Julie.Core.Models
{
    /// <summary>
    /// Represents a conversation between the user and Julie
    /// </summary>
    public class Conversation
    {
        /// <summary>
        /// Unique identifier for the conversation
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Title of the conversation (derived from first message or manually set)
        /// </summary>
        public string Title { get; set; } = "New Conversation";

        /// <summary>
        /// List of messages in chronological order
        /// </summary>
        public List<Message> Messages { get; set; } = new List<Message>();

        /// <summary>
        /// Timestamp when the conversation was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Timestamp when the conversation was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Whether the conversation is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Metadata associated with the conversation
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Adds a message to the conversation
        /// </summary>
        public void AddMessage(Message message)
        {
            Messages.Add(message);
            UpdatedAt = DateTime.Now;            // Set title from first user message if not set
            if (Title == "New Conversation" && message.Role == MessageRole.User && !string.IsNullOrEmpty(message.Text))
            {
                Title = message.Text.Length > 50 ? message.Text.Substring(0, 50) + "..." : message.Text;
            }
        }

        /// <summary>
        /// Gets the latest message in the conversation
        /// </summary>
        public Message? GetLatestMessage()
        {
            return Messages.Count > 0 ? Messages[Messages.Count - 1] : null;
        }        /// <summary>
        /// Gets messages by role
        /// </summary>
        public IEnumerable<Message> GetMessagesByRole(MessageRole role)
        {
            return Messages.Where(m => m.Role == role);
        }
    }
}

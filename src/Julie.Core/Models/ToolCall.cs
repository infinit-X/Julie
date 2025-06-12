using System.Collections.Generic;

namespace Julie.Core.Models
{
    /// <summary>
    /// Represents a tool call made by the AI
    /// </summary>
    public class ToolCall
    {
        /// <summary>
        /// Unique identifier for the tool call
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Name of the function being called
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Arguments passed to the function
        /// </summary>
        public Dictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Type of tool call (function, search, code_execution)
        /// </summary>
        public string Type { get; set; } = "function";
    }

    /// <summary>
    /// Represents a response to a function call
    /// </summary>
    public class FunctionResponse
    {
        /// <summary>
        /// ID of the tool call this responds to
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Name of the function
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Response data from the function
        /// </summary>
        public Dictionary<string, object> Response { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Whether the function call was successful
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Error message if the function call failed
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Represents a function declaration for the AI
    /// </summary>
    public class FunctionDeclaration
    {
        /// <summary>
        /// Name of the function
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of what the function does
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Parameters the function accepts
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Whether the function can be called asynchronously
        /// </summary>
        public bool IsAsync { get; set; } = false;
    }
}

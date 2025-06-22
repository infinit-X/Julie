# Julie AI Assistant

> **A Modern, Real-Time AI Assistant with Voice and Visual Capabilities**

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Latest-green.svg)](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](#)

Julie is a sophisticated AI assistant application built with .NET 8 and WPF, featuring real-time voice interaction, screen sharing capabilities, and seamless integration with Google's Gemini API. Experience the future of AI assistance with a beautiful, modern interface and powerful multimodal capabilities.

## 📸 Screenshots

### Main Chat Interface
![Julie Main Interface](assets/Screenshot%202025-06-22%20200811.png)
*Beautiful chat interface with gradient backgrounds and modern UI design*

### Settings & Configuration  
![Julie Settings](assets/Screenshot%202025-06-22%20200829.png)
*Comprehensive settings panel for API configuration and preferences*

### Live Conversation
![Julie Live Chat](assets/Screenshot%202025-06-22%20200917.png)
*Real-time conversation with Gemini AI showing message history and status*

## ✨ Features

### 🤖 **AI-Powered Conversations**
- **Real Gemini Integration**: Direct integration with Google's Gemini 1.5 Flash API
- **Intelligent Responses**: Context-aware AI conversations with advanced reasoning
- **Message History**: Complete conversation tracking and management
- **Error Handling**: Robust error handling with user-friendly feedback

### 🎙️ **Voice Interaction** 
- Real-time audio capture and playback with NAudio
- Voice recording with privacy disclaimer
- High-quality voice processing capabilities
- Audio device selection and management

### 🖥️ **Screen Context & Sharing**
- Multi-monitor screen capture capabilities
- Screen context integration for visual assistance  
- Selective screen area capture
- Performance-optimized rendering

### 💬 **Modern Chat Experience**
- **Beautiful UI**: Professional gradient backgrounds and modern design
- **Message Bubbles**: Elegant message display with timestamps
- **Typing Indicators**: Real-time connection status and thinking indicators
- **Welcome Messages**: Guided user experience from first launch

### 🎨 **Beautiful Interface**
- **Modern WPF Design**: Clean, professional interface with smooth gradients
- **Responsive Layout**: Adaptive design that works on different screen sizes
- **Visual Feedback**: Interactive elements with hover effects and focus states
- **Status Indicators**: Real-time connection status with visual cues

### ⚙️ **Advanced Configuration**
- **API Key Management**: Secure storage and validation of Gemini API keys
- **Connection Testing**: Automatic API key validation and connection testing
- **Real-time Updates**: Instant settings application without restart
- **Device Preferences**: Audio device configuration and management

## 🏗️ Architecture

Julie follows a clean, modular architecture with clear separation of concerns:

```
📁 Julie/
├── 🧠 Julie.Core/          # Core business logic and models
├── ⚙️ Julie.Services/      # Service implementations and interfaces  
├── 🎨 Julie.UI/           # WPF views and ViewModels
└── 🚀 Julie.App/          # Application composition and startup
```

### 🧩 **Technology Stack**

| Component | Technology | Purpose |
|-----------|------------|---------|
| **Framework** | .NET 8 | Cross-platform runtime |
| **UI Framework** | WPF | Native Windows UI with MVVM |
| **MVVM** | CommunityToolkit.Mvvm | Data binding and commands |
| **Audio** | NAudio | Audio capture/playback |
| **AI API** | Google Gemini 1.5 Flash | AI conversation engine |
| **HTTP Client** | System.Net.Http | API communication |
| **JSON** | System.Text.Json | Data serialization |
| **DI Container** | Microsoft.Extensions.DependencyInjection | Service management |
| **Logging** | Microsoft.Extensions.Logging | Structured logging |

### 🔧 **Current Implementation**

- **✅ HTTP-based Gemini Client**: Direct REST API integration with Gemini 1.5 Flash
- **✅ Real-time Chat**: Working text conversations with AI responses
- **✅ Connection Management**: API key validation and connection status
- **✅ Modern UI**: Beautiful WPF interface with gradients and animations
- **✅ Message History**: Complete conversation tracking and storage
- **🔄 Live API Integration**: Planned for voice and multimodal features

## 🚀 Getting Started

### 📋 Prerequisites

- **Windows 10/11** (version 1903 or later)
- **.NET 8 SDK** or later
- **Visual Studio 2022** (recommended) or Visual Studio Code
- **Google Gemini API Key** from [Google AI Studio](https://aistudio.google.com/)

### 🔧 Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/infinit-X/Julie.git
   cd Julie
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure API Key**
   - Obtain a Google Gemini API key from [Google AI Studio](https://aistudio.google.com/)
   - Launch the application and enter your API key in the Settings panel
   - The app will automatically test and validate your key

4. **Build the solution**
   ```bash
   dotnet build
   ```

5. **Run the application**
   ```bash
   dotnet run --project src/Julie.App
   ```

### 🎮 Quick Start Guide

1. **Launch Julie** from the built executable or Visual Studio
2. **Enter API Key**: Open Settings and enter your Google Gemini API key
3. **Verify Connection**: Check that the status shows "Connected" with a green indicator
4. **Start Chatting**: Type a message and press Enter or click Send
5. **Explore Features**: Try voice recording (with privacy disclaimer) and screen context
6. **Enjoy**: Experience seamless AI assistance with beautiful UI!

### 🔐 API Key Setup

To use Julie, you need a Google Gemini API key:

1. Visit [Google AI Studio](https://aistudio.google.com/)
2. Sign in with your Google account
3. Navigate to "Get API Key" section
4. Create a new API key for your project
5. Copy the key and paste it into Julie's Settings
6. Click "Save Settings" to validate and connect

**Supported Models**:
- `gemini-1.5-flash-latest` (Currently used for chat)
- `gemini-2.0-flash-live-001` (Planned for voice features)
- `gemini-1.5-pro-latest` (Available for complex tasks)

## 📚 Documentation

| Document | Description |
|----------|-------------|
| [🚀 Development Progress](DEVELOPMENT_PROGRESS.md) | Current development status and milestones |
| [📋 Guidelines](guidelines.md) | Development guidelines and best practices |
| [🏗️ Project Structure](project-structure.md) | Detailed architecture documentation |
| [🎨 UI/UX Design Guide](ui-ux-design-guide.md) | Design principles and UI guidelines |
| [🔌 API Integration Guide](api-integration-guide.md) | Gemini API integration documentation |
| [🤝 Contributing Guidelines](CONTRIBUTING.md) | How to contribute to the project |

### 📖 Additional Resources
- [📄 Available Models](available_models.txt) - List of supported Gemini models
- [📡 Live API Documentation](LiveAPI_documentation.txt) - Gemini Live API reference
- [⚙️ Configuration Examples](docs/configuration/) - Sample configuration files

## 🏛️ Project Structure

```
Julie/
├── src/
│   ├── Julie.Core/              # 🧠 Core Models & Utilities
│   │   ├── Models/              # Data models and entities
│   │   ├── Audio/               # Audio processing utilities
│   │   ├── Screen/              # Screen capture functionality
│   │   └── API/                 # API client implementations
│   │
│   ├── Julie.Services/          # ⚙️ Service Layer
│   │   ├── Interfaces/          # Service contracts
│   │   ├── Implementations/     # Service implementations
│   │   └── Configuration/       # Service configuration
│   │
│   ├── Julie.UI/               # 🎨 User Interface
│   │   ├── Views/              # XAML views and code-behind
│   │   ├── ViewModels/         # MVVM ViewModels
│   │   └── Controls/           # Custom user controls
│   │
│   └── Julie.App/              # 🚀 Application Host
│       ├── Configuration/       # App configuration
│       ├── Services/           # DI container setup
│       └── Startup/            # Application initialization
│
├── docs/                       # 📚 Documentation
├── tests/                      # 🧪 Unit and integration tests
└── assets/                     # 🎨 Images and resources
```

## 🔧 Configuration

### API Configuration
```json
{
  "GoogleAI": {
    "ApiKey": "your-gemini-api-key-here",
    "Model": "gemini-1.5-flash-latest", 
    "ApiVersion": "v1beta",
    "BaseUrl": "https://generativelanguage.googleapis.com"
  }
}
```

### Current Features Status
- **✅ Text Chat**: Fully functional with Gemini 1.5 Flash
- **✅ API Integration**: HTTP-based REST API client
- **✅ Connection Management**: Real-time status updates
- **✅ Message History**: Complete conversation tracking
- **🔄 Voice Features**: In development with Live API
- **🔄 Screen Context**: Planned multimodal integration

### Audio Settings (Future Implementation)
```json
{
  "Audio": {
    "SampleRate": 16000,
    "BufferSize": 1024, 
    "InputDevice": "default",
    "OutputDevice": "default",
    "EnableVoiceRecording": true
  }
}
```

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### 🔀 Development Workflow

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** your changes (`git commit -m 'Add amazing feature'`)
4. **Push** to the branch (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

### 🐛 Bug Reports

Please use the [GitHub Issues](https://github.com/infinit-X/Julie/issues) page to report bugs.

## 📊 Development Status

- ✅ **Core Architecture**: Complete and stable
- ✅ **Service Layer**: Fully implemented with proper interfaces
- ✅ **Gemini API Integration**: Working HTTP client with real responses
- ✅ **Chat Functionality**: Text conversations fully operational
- ✅ **UI Implementation**: Modern WPF interface with beautiful design
- ✅ **Connection Management**: API key validation and status tracking
- ✅ **Message History**: Conversation storage and display
- ✅ **Error Handling**: Comprehensive error management and user feedback
- 🔄 **Voice Features**: Live API integration in progress
- 🔄 **Screen Context**: Multimodal capabilities planned
- ⏳ **Testing**: Unit tests and integration tests needed
- ⏳ **Packaging**: MSI installer and deployment automation

**Current Build Status**: ✅ All core features functional, 0 compilation errors

### 🚀 **Recent Updates (v1.0)**
- ✅ Implemented working Gemini 1.5 Flash integration
- ✅ Added real-time chat with AI responses  
- ✅ Created beautiful gradient UI with modern design
- ✅ Added API key validation and connection testing
- ✅ Implemented message history and conversation management
- ✅ Added voice recording disclaimer and privacy notices
- ✅ Enhanced error handling with user-friendly messages

## 🔮 Roadmap

### 🎯 **Phase 1: Core Functionality** ✅ Complete
- [x] Basic text chat with Gemini API
- [x] Modern WPF user interface
- [x] API key management and validation
- [x] Message history and conversation tracking
- [x] Connection status and error handling

### 🎯 **Phase 2: Advanced Features** 🔄 In Progress  
- [ ] **Live API Integration**: Real-time voice conversations with Gemini 2.0 Flash Live
- [ ] **Multimodal Capabilities**: Screen context analysis and image understanding
- [ ] **Voice Input/Output**: Audio recording and speech synthesis
- [ ] **Advanced Models**: Integration with Gemini 2.5 Pro for complex tasks
- [ ] **Screen Analysis**: Context-aware responses based on screen content

### 🎯 **Phase 3: Enhancement** 📋 Planned
- [ ] Multi-language support and localization
- [ ] Custom AI model integration and fine-tuning
- [ ] Plugin system for extensibility
- [ ] Cloud synchronization and backup
- [ ] Advanced voice commands and shortcuts
- [ ] Mobile companion app

### 🚀 **Performance Improvements**
- [ ] Audio processing optimization for real-time performance
- [ ] Memory usage reduction and garbage collection optimization
- [ ] Startup time improvement and lazy loading
- [ ] Background processing for non-blocking operations
- [ ] Caching strategies for improved response times

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Google Gemini Team** for the powerful AI API
- **Microsoft** for .NET 8 and WinUI 3
- **CommunityToolkit** for MVVM utilities
- **NAudio** for audio processing capabilities

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/infinit-X/Julie/issues)
- **Discussions**: [GitHub Discussions](https://github.com/infinit-X/Julie/discussions)
- **Documentation**: [Project Wiki](https://github.com/infinit-X/Julie/wiki)

---

<div align="center">

**⭐ If you find Julie helpful, please consider giving it a star! ⭐**

Made with ❤️ by [Infinit-X](https://github.com/infinit-X)

</div>

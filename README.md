# Julie AI Assistant

> **A Modern, Real-Time AI Assistant with Voice and Visual Capabilities**

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![WinUI 3](https://img.shields.io/badge/WinUI-3.0-green.svg)](https://docs.microsoft.com/en-us/windows/apps/winui/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](#)

Julie is a sophisticated AI assistant application built with .NET 8 and WinUI 3, featuring real-time voice interaction, screen sharing capabilities, and seamless integration with Google's Gemini Live API.

## ✨ Features

### 🎙️ **Voice Interaction**
- Real-time audio capture and playback
- High-quality voice processing with NAudio
- Continuous conversation support
- Audio device selection and management

### 🖥️ **Screen Sharing**
- Multi-monitor screen capture
- Real-time screen sharing with AI
- Selective screen area capture
- Performance-optimized rendering

### 💬 **Intelligent Conversations**
- Powered by Google Gemini Live API
- Context-aware responses
- Real-time message streaming
- Conversation history management

### 🎨 **Modern UI/UX**
- Native WinUI 3 interface
- Responsive design patterns
- Dark/Light theme support
- Intuitive navigation and controls

### ⚙️ **Advanced Configuration**
- Customizable audio settings
- API key management
- Device preferences
- Real-time settings updates

## 🏗️ Architecture

Julie follows a clean, modular architecture with clear separation of concerns:

```
📁 Julie/
├── 🧠 Julie.Core/          # Core business logic and models
├── ⚙️ Julie.Services/      # Service implementations and interfaces
├── 🎨 Julie.UI/           # WinUI 3 views and ViewModels
└── 🚀 Julie.App/          # Application composition and startup
```

### 🧩 **Technology Stack**

| Component | Technology | Purpose |
|-----------|------------|---------|
| **Framework** | .NET 8 | Cross-platform runtime |
| **UI Framework** | WinUI 3 | Native Windows UI |
| **MVVM** | CommunityToolkit.Mvvm | Data binding and commands |
| **Audio** | NAudio | Audio capture/playback |
| **API** | Google Gemini Live | AI conversation engine |
| **Networking** | System.Net.WebSockets | Real-time communication |
| **DI Container** | Microsoft.Extensions.DependencyInjection | Service management |
| **Logging** | Microsoft.Extensions.Logging | Structured logging |

## 🚀 Getting Started

### 📋 Prerequisites

- **Windows 10/11** (version 1903 or later)
- **.NET 8 SDK** or later
- **Visual Studio 2022** (recommended) or Visual Studio Code
- **Windows App SDK** 1.6 or later
- **Google Gemini API Key**

### 🔧 Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/Julie.git
   cd Julie
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure API Key**
   - Obtain a Google Gemini API key from [Google AI Studio](https://makersuite.google.com/)
   - Add your API key in the application settings

4. **Build the solution**
   ```bash
   dotnet build
   ```

5. **Run the application**
   ```bash
   dotnet run --project src/Julie.App
   ```

### 🎮 Quick Start Guide

1. **Launch Julie** from the built executable
2. **Configure Settings**: Set your API key and audio preferences
3. **Start Chatting**: Use text or voice to interact with Julie
4. **Share Screen**: Enable screen sharing for visual assistance
5. **Enjoy**: Experience seamless AI assistance!

## 📚 Documentation

| Document | Description |
|----------|-------------|
| [🚀 Development Progress](DEVELOPMENT_PROGRESS.md) | Current development status and milestones |
| [📋 Guidelines](guidelines.md) | Development guidelines and best practices |
| [🏗️ Project Structure](project-structure.md) | Detailed architecture documentation |
| [🎨 UI/UX Design Guide](ui-ux-design-guide.md) | Design principles and UI guidelines |

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
    "ApiKey": "your-gemini-api-key",
    "Model": "gemini-2.0-flash-exp",
    "ApiVersion": "v1beta"
  }
}
```

### Audio Settings
```json
{
  "Audio": {
    "SampleRate": 44100,
    "BufferSize": 1024,
    "InputDevice": "default",
    "OutputDevice": "default"
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

Please use the [GitHub Issues](https://github.com/yourusername/Julie/issues) page to report bugs.

## 📊 Development Status

- ✅ **Core Architecture**: Complete
- ✅ **Service Layer**: Complete  
- ✅ **Audio System**: Complete
- ✅ **Screen Capture**: Complete
- ✅ **UI Implementation**: Complete
- ✅ **API Integration**: Complete
- ⚠️ **Packaging**: Environment setup needed
- ⏳ **Testing**: In progress

**Build Status**: All code complete, 0 compilation errors

## 🔮 Roadmap

### 🎯 **Upcoming Features**
- [ ] Multi-language support
- [ ] Custom AI model integration
- [ ] Mobile companion app
- [ ] Plugin system
- [ ] Cloud synchronization
- [ ] Advanced voice commands

### 🚀 **Performance Improvements**
- [ ] Audio processing optimization
- [ ] Memory usage reduction
- [ ] Startup time improvement
- [ ] Background processing

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Google Gemini Team** for the powerful AI API
- **Microsoft** for .NET 8 and WinUI 3
- **CommunityToolkit** for MVVM utilities
- **NAudio** for audio processing capabilities

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/yourusername/Julie/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/Julie/discussions)
- **Documentation**: [Project Wiki](https://github.com/yourusername/Julie/wiki)

---

<div align="center">

**⭐ If you find Julie helpful, please consider giving it a star! ⭐**

Made with ❤️ by [Your Name](https://github.com/yourusername)

</div>

# Contributing to Julie AI Assistant

Thank you for your interest in contributing to Julie! We welcome contributions from the community and are grateful for any help you can provide.

## 🤝 Code of Conduct

This project adheres to a code of conduct that we expect all contributors to follow. Please be respectful and professional in all interactions.

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK or later
- Visual Studio 2022 or Visual Studio Code
- Windows 10/11 (version 1903 or later)
- Windows App SDK 1.6 or later
- Git for version control

### Development Setup

1. **Fork the repository**
   ```bash
   git clone https://github.com/yourusername/Julie.git
   cd Julie
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Verify the build**
   ```bash
   dotnet build
   ```

4. **Run tests**
   ```bash
   dotnet test
   ```

## 📝 How to Contribute

### 🐛 Reporting Bugs

When reporting bugs, please include:

- **Clear title** and description
- **Steps to reproduce** the issue
- **Expected behavior** vs actual behavior
- **System information** (OS version, .NET version, etc.)
- **Screenshots** or logs if applicable

Use the [Bug Report Template](.github/ISSUE_TEMPLATE/bug_report.md) when creating issues.

### 💡 Suggesting Features

For feature requests, please include:

- **Clear use case** description
- **Detailed feature specification**
- **Mockups or diagrams** if applicable
- **Implementation considerations**

Use the [Feature Request Template](.github/ISSUE_TEMPLATE/feature_request.md) when suggesting features.

### 🔧 Pull Requests

1. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Follow coding standards** (see below)

3. **Write or update tests** for your changes

4. **Update documentation** if needed

5. **Commit with clear messages**
   ```bash
   git commit -m "feat: add voice command recognition"
   ```

6. **Push your branch**
   ```bash
   git push origin feature/your-feature-name
   ```

7. **Create a Pull Request** with:
   - Clear title and description
   - Link to related issues
   - Screenshots/demos if applicable
   - Test coverage information

## 📋 Coding Standards

### C# Code Style

Follow the [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions):

```csharp
// ✅ Good
public class AudioService : IAudioService
{
    private readonly ILogger<AudioService> _logger;
    
    public async Task<bool> StartRecordingAsync()
    {
        try
        {
            // Implementation
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start recording");
            return false;
        }
    }
}

// ❌ Avoid
public class audioservice
{
    public bool startRecording()
    {
        // Implementation without error handling
    }
}
```

### XAML Style

```xml
<!-- ✅ Good -->
<Grid Margin="20">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>
    
    <TextBlock Grid.Row="0" 
               Text="Chat Messages"
               Style="{StaticResource HeaderTextBlockStyle}"/>
               
    <ListView Grid.Row="1"
              ItemsSource="{x:Bind ViewModel.Messages}"/>
</Grid>

<!-- ❌ Avoid -->
<grid margin="20"><textblock text="Chat Messages"/><listview itemssource="{x:bind ViewModel.Messages}"/></grid>
```

### Naming Conventions

| Type | Convention | Example |
|------|------------|---------|
| **Classes** | PascalCase | `AudioService` |
| **Interfaces** | IPascalCase | `IAudioService` |
| **Methods** | PascalCase | `StartRecordingAsync()` |
| **Properties** | PascalCase | `IsRecording` |
| **Fields** | _camelCase | `_audioDevice` |
| **Parameters** | camelCase | `deviceId` |
| **Local Variables** | camelCase | `recordingLevel` |

### Architecture Guidelines

- **Follow SOLID principles**
- **Use dependency injection** for service management
- **Implement proper async/await** patterns
- **Include comprehensive error handling**
- **Write unit tests** for business logic
- **Document public APIs** with XML comments

## 🧪 Testing

### Unit Tests
```csharp
[TestMethod]
public async Task StartRecordingAsync_ShouldReturnTrue_WhenDeviceAvailable()
{
    // Arrange
    var mockDevice = new Mock<IAudioDevice>();
    var service = new AudioService(mockDevice.Object);
    
    // Act
    var result = await service.StartRecordingAsync();
    
    // Assert
    Assert.IsTrue(result);
}
```

### Integration Tests
- Test end-to-end workflows
- Verify API integrations
- Test UI interactions

### Test Coverage
- Aim for **80%+ code coverage**
- Focus on **business logic** and **critical paths**
- Include **edge cases** and **error scenarios**

## 📁 Project Structure

When contributing, please maintain the established project structure:

```
src/
├── Julie.Core/           # Core models and utilities
├── Julie.Services/       # Business logic and services  
├── Julie.UI/            # WinUI 3 user interface
└── Julie.App/           # Application composition
```

## 🔄 Development Workflow

### Feature Development
1. **Create issue** for discussion
2. **Fork repository** and create feature branch
3. **Implement feature** following guidelines
4. **Write tests** and update documentation
5. **Submit pull request** for review

### Bug Fixes
1. **Reproduce issue** and understand root cause
2. **Create test** that demonstrates the bug
3. **Fix issue** with minimal changes
4. **Verify fix** resolves the problem
5. **Submit pull request** with test coverage

### Code Review Process

All contributions go through code review:

- **Automated checks** must pass (build, tests, linting)
- **Peer review** by maintainers or contributors
- **Documentation** updates if needed
- **Performance impact** assessment for significant changes

## 📚 Documentation

When contributing, please update relevant documentation:

- **README.md** for user-facing changes
- **API documentation** for new public interfaces
- **Code comments** for complex logic
- **DEVELOPMENT_PROGRESS.md** for milestone updates

## 🏷️ Commit Message Format

Use [Conventional Commits](https://www.conventionalcommits.org/):

```
type(scope): description

[optional body]

[optional footer]
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes
- `refactor`: Code refactoring
- `test`: Test additions/changes
- `chore`: Build/maintenance tasks

**Examples:**
```
feat(audio): add noise cancellation support
fix(ui): resolve chat scrolling issue
docs(readme): update installation instructions
```

## 🚀 Release Process

Releases follow semantic versioning (SemVer):

- **Major** (1.0.0): Breaking changes
- **Minor** (0.1.0): New features, backward compatible
- **Patch** (0.0.1): Bug fixes, backward compatible

## 💬 Community

- **GitHub Discussions**: For questions and ideas
- **GitHub Issues**: For bug reports and feature requests
- **Pull Requests**: For code contributions

## 📞 Getting Help

If you need help:

1. **Check existing issues** and documentation
2. **Search GitHub Discussions**
3. **Create a new discussion** for questions
4. **Join our community** channels (if available)

## 🎯 Areas for Contribution

We especially welcome contributions in:

- **🐛 Bug fixes** and stability improvements
- **🎨 UI/UX enhancements** and accessibility
- **🔊 Audio processing** optimizations
- **📱 Cross-platform** compatibility
- **🧪 Test coverage** expansion
- **📚 Documentation** improvements
- **🌐 Internationalization** support

Thank you for contributing to Julie AI Assistant! 🙏

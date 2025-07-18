# Contributing to Elite Dangerous Inara Overlay

Thank you for your interest in contributing to the Elite Dangerous Inara Overlay project! This guide will help you understand the unified repository structure and development workflow.

## 🏗️ Repository Structure

This is a **unified repository** containing all components of the Elite Dangerous Inara Overlay system. Previously, these components were maintained in separate repositories, but they have been consolidated to improve development workflow and dependency management.

### Project Components
|| Project | Purpose | Language/Tech |
|---------|---------|---------------|
| `ED_Inara_Overlay/` | Main WPF application with overlay UI | C# / .NET 8 / WPF |
| `InaraTools/` | Shared library for INARA API communication | C# / .NET 8 |
| `Logger/` | Centralized logging infrastructure | C# / .NET 8 |
| `MockTargetApp/` | Test application simulating Elite Dangerous | C# / .NET 8 |
| `Inara_Parser/` | Legacy parser (being phased out) | C# / .NET 8 |
| `OverlayTestHarness.csproj` | Integration testing framework | C# / .NET 8 |

## 🚀 Getting Started

### Prerequisites

- **Visual Studio 2022** or **Visual Studio Code** with C# extension
- **.NET 8.0 SDK** or later
- **Windows 10/11** (required for Windows API functionality)
- **Git** for version control

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/ED_Inara_Overlay.git
   cd ED_Inara_Overlay
   ```

2. **Build the solution**
   ```bash
   # Option 1: Build everything
   dotnet build ED_Inara_Overlay/ED_Inara_Overlay.sln
   
   # Option 2: Build individual projects
   dotnet build ED_Inara_Overlay/ED_Inara_Overlay.csproj
   dotnet build InaraTools/InaraTools.csproj
   dotnet build Logger/Logger.csproj
   ```

3. **Run the application**
   ```bash
   dotnet run --project ED_Inara_Overlay/ED_Inara_Overlay.csproj
   ```

## 🔧 Development Workflow

### Working with the Unified Repository

Since this is a unified repository, changes often affect multiple components. Here's how to work effectively:

1. **Understand Dependencies**
   - `ED_Inara_Overlay` depends on `InaraTools` and `Logger`
   - Changes to `InaraTools` or `Logger` may require updates to the main application
   - Always build and test the entire solution after making changes

2. **Cross-Component Development**
   - When adding new features, consider which component should own the functionality
   - Shared utilities belong in `InaraTools` or `Logger`
   - UI-specific code belongs in `ED_Inara_Overlay`

3. **Testing**
   - Use `MockTargetApp` to simulate Elite Dangerous during development
   - Run `OverlayTestHarness` for integration testing
   - Test changes across all affected components

### Branch Strategy

- **main**: Stable, production-ready code
- **develop**: Integration branch for new features
- **feature/**: Individual feature branches
- **hotfix/**: Critical bug fixes

### Making Changes

1. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes**
   - Follow the existing code style and patterns
   - Add appropriate logging using the `Logger` component
   - Update documentation if needed

3. **Test thoroughly**
   ```bash
   # Build and test all components
   dotnet build ED_Inara_Overlay/ED_Inara_Overlay.sln
   
   # Run integration tests
   dotnet run --project OverlayTestHarness.csproj
   ```

4. **Commit and push**
   ```bash
   git add .
   git commit -m "feat: add your feature description"
   git push origin feature/your-feature-name
   ```

5. **Create a pull request**
   - Describe your changes clearly
   - Include testing steps
   - Reference any related issues

## 📝 Code Style Guidelines

### General Principles

- **Consistency**: Follow existing patterns in the codebase
- **Clarity**: Write self-documenting code with clear variable names
- **Separation of Concerns**: Keep UI logic separate from business logic
- **Error Handling**: Use proper exception handling and logging

### Specific Guidelines

- Use **PascalCase** for public members, **camelCase** for private members
- Include XML documentation for public APIs
- Use the `Logger` component for all logging instead of `Console.WriteLine`
- Follow WPF MVVM patterns in the UI layer

### Example Code Structure

```csharp
// Good: Clear separation of concerns
public class TradeRouteService
{
    private readonly ILogger _logger;
    private readonly IInaraApi _inaraApi;
    
    public TradeRouteService(ILogger logger, IInaraApi inaraApi)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _inaraApi = inaraApi ?? throw new ArgumentNullException(nameof(inaraApi));
    }
    
    public async Task<List<TradeRoute>> GetTradeRoutesAsync(string fromSystem, string toSystem)
    {
        try
        {
            _logger.LogInfo($"Fetching trade routes from {fromSystem} to {toSystem}");
            return await _inaraApi.GetTradeRoutesAsync(fromSystem, toSystem);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to fetch trade routes: {ex.Message}");
            throw;
        }
    }
}
```

## 🧪 Testing

### Testing Strategy

1. **Unit Tests**: Test individual components in isolation
2. **Integration Tests**: Test component interactions
3. **UI Tests**: Test overlay behavior with mock applications
4. **Manual Testing**: Test with actual Elite Dangerous

### Test Applications

- **MockTargetApp**: Simulates Elite Dangerous window behavior
- **OverlayTestHarness**: Automated testing framework
- **Regression Tests**: PowerShell scripts for automated testing

### Running Tests

```bash
# Run mock target app
dotnet run --project MockTargetApp/MockTargetApp.csproj

# Run test harness
dotnet run --project OverlayTestHarness.csproj

# Run regression tests
powershell -ExecutionPolicy Bypass -File ED_Inara_Overlay/RegressionTest.ps1
```

## 📚 Documentation

### What to Document

- **API Changes**: Update XML documentation for public APIs
- **Architecture Changes**: Update README.md if structure changes
- **New Features**: Add usage examples and screenshots
- **Breaking Changes**: Document migration steps

### Documentation Files

- `README.md`: Main project documentation
- `CONTRIBUTING.md`: This file
- `CHANGELOG.md`: Version history and changes
- `BUILD_GUIDE.md`: Detailed build instructions

## 🐛 Reporting Issues

### Bug Reports

Include:
- **Steps to reproduce**
- **Expected behavior**
- **Actual behavior**
- **Environment** (OS, .NET version, Elite Dangerous version)
- **Logs** (if available)

### Feature Requests

Include:
- **Use case**: Why is this feature needed?
- **Proposed solution**: How should it work?
- **Alternatives**: What other approaches were considered?

## 🔄 Migration from Separate Repositories

### What Changed

- **Before**: Components in separate repositories with individual builds
- **After**: All components in unified repository with shared build system

### Benefits

- **Simplified Development**: All code in one place
- **Consistent Dependencies**: Shared package versions
- **Easier Testing**: Cross-component testing in single environment
- **Unified Releases**: Single version for all components

### Migration Impact

- **Build Scripts**: Updated to handle unified structure
- **Dependencies**: Consolidated package management
- **Testing**: Integrated test suite across all components
- **Documentation**: Updated to reflect new structure

## 🤝 Community

- **Issues**: Use GitHub Issues for bug reports and feature requests
- **Discussions**: Use GitHub Discussions for questions and ideas
- **Pull Requests**: Follow the PR template and guidelines

## 📜 License

This project is licensed under the MIT License. By contributing, you agree that your contributions will be licensed under the same terms.

---

**Thank you for contributing to the Elite Dangerous Inara Overlay project!** 🚀

Your contributions help make the Elite Dangerous experience better for all commanders.

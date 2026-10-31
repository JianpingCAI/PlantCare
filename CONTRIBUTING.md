# Contributing to PlantCare

Thank you for your interest in contributing to the PlantCare project! This document provides guidelines and instructions for contributing to the project.

## Code of Conduct

Please be respectful and constructive in all interactions with other contributors and maintainers.

## Getting Started

### Prerequisites

- .NET 8 SDK or later
- Visual Studio 2022 or Visual Studio Code with C# extension
- Git

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/JianpingCAI/PlantCare.git
   cd PlantCare
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run tests**
   ```bash
   dotnet test
   ```

## Development Workflow

### 1. Create a Feature Branch

```bash
git checkout -b feature/your-feature-name
```

Use descriptive names:
- `feature/add-oauth-login`
- `fix/notification-crash`
- `docs/update-readme`

### 2. Code Standards

- **Language**: C# (.NET 8)
- **Code Style**: Follow `.editorconfig` rules (automatically enforced)
- **Naming Conventions**:
  - Classes, methods, properties: `PascalCase`
  - Private fields: `_camelCase`
  - Local variables: `camelCase`
  - Constants: `PascalCase`
  - Async methods: End with `Async` suffix

### 3. Architecture Guidelines

The project follows MVVM pattern with these layers:

- **PlantCare.App**: UI layer (Views, ViewModels, Services)
- **PlantCare.Data**: Data access layer (Models, DbContext, Repositories)
- **PlantCare.App.Tests**: Unit tests

**Dependency Injection**: All services should be registered in `MauiProgram.cs`

### 4. Commit Messages

Write clear, descriptive commit messages:

```
[TYPE] Brief description

Detailed explanation if needed.

Fixes #123  (if applicable)
```

Types:
- `feat:` New feature
- `fix:` Bug fix
- `docs:` Documentation
- `style:` Code style changes (no functional change)
- `refactor:` Code refactoring
- `perf:` Performance improvement
- `test:` Test addition/modification
- `chore:` Build, dependency, or tooling changes

**Example**:
```
feat: Add encryption service for sensitive data

Implements AES encryption for user passwords and personal information.
Follows security requirements from design document.

Fixes #45
```

### 5. Documentation

- Add XML documentation to public classes and methods
- Update README.md for user-facing changes
- Update design document for architectural changes
- Add comments for complex logic

## Testing

### Writing Tests

- Create tests in `PlantCare.App.Tests`
- Use xUnit framework
- Use Moq for mocking dependencies
- Follow naming convention: `[MethodName]_[Scenario]_[ExpectedResult]`

**Example**:
```csharp
[Fact]
public void Encrypt_ValidPlainText_ReturnsEncryptedString()
{
    // Arrange
    var service = new EncryptionService();
    var plainText = "test@example.com";

    // Act
    var result = service.Encrypt(plainText);

    // Assert
    Assert.NotEmpty(result);
    Assert.NotEqual(plainText, result);
}
```

### Running Tests

```bash
dotnet test
```

### Code Coverage

Aim for at least 70% code coverage. Use:
```bash
dotnet test /p:CollectCoverage=true
```

## Pull Request Process

1. **Update your branch with latest changes**
   ```bash
   git fetch origin
   git rebase origin/master
   ```

2. **Push your changes**
   ```bash
   git push origin feature/your-feature-name
   ```

3. **Create a Pull Request**
   - Provide a clear title and description
   - Reference related issues
   - Include screenshots for UI changes
   - List breaking changes if any

4. **PR Template**
   ```
   ## Description
   Brief description of changes

   ## Related Issues
   Fixes #123

   ## Type of Change
   - [ ] Bug fix
   - [ ] New feature
   - [ ] Breaking change
   - [ ] Documentation update

   ## Testing
   Describe how you tested the changes

   ## Screenshots (if applicable)
   Add screenshots for UI changes

   ## Checklist
   - [ ] Code follows style guidelines
   - [ ] Added tests for new functionality
   - [ ] Updated documentation
   - [ ] No new warnings introduced
   ```

5. **Review Process**
   - Maintainers will review your PR
   - Address feedback and comments
   - Re-request review after making changes

## Project Structure

```
PlantCare/
├── PlantCare.App/              # MAUI Application (UI Layer)
│   ├── Views/                  # XAML views
│   ├── ViewModels/             # MVVM ViewModels
│   ├── Services/               # Application services
│   │   ├── Security/           # Encryption services
│   │   ├── Accessibility/      # Accessibility services
│   │   ├── DataExportImport/   # Data export/import
│   │   └── DBService/          # Database services
│   ├── Utils/                  # Utilities and helpers
│   ├── Resources/              # Images, fonts, localization
│   ├── Platforms/              # Platform-specific code
│   └── MauiProgram.cs          # DI configuration
├── PlantCare.Data/             # Data Access Layer
│   ├── Models/                 # Entity models
│   ├── Repositories/           # Repository pattern
│   └── ApplicationDbContext    # EF Core DbContext
├── PlantCare.App.Tests/        # Unit tests
├── .editorconfig               # Code style rules
└── README.md                   # Project documentation
```

## Feature Implementation Checklist

When implementing a new feature:

- [ ] Design discussed and approved
- [ ] Feature branch created
- [ ] Implementation code written
- [ ] Unit tests added
- [ ] Code review completed
- [ ] Tests passing
- [ ] Documentation updated
- [ ] No breaking changes or documented

## Performance Guidelines

- Implement lazy loading for lists and images
- Use async/await for long-running operations
- Cache frequently accessed data
- Optimize database queries
- Profile performance-critical sections

## Security Guidelines

- Use `IEncryptionService` for sensitive data (passwords, email)
- Never hardcode secrets or API keys
- Use HTTPS for API communications
- Validate all user inputs
- Use parameterized queries to prevent SQL injection
- Keep dependencies updated

## Accessibility Guidelines

- Use `IAccessibilityService` for font size adjustments
- Implement screen reader support with AutomationProperties
- Ensure sufficient color contrast ratios
- Support keyboard navigation
- Test with accessibility features enabled

## Documentation

### Code Documentation

Use XML documentation comments:
```csharp
/// <summary>
/// Encrypts sensitive data using AES encryption.
/// </summary>
/// <param name="plainText">The text to encrypt.</param>
/// <returns>The encrypted text in Base64 format.</returns>
/// <exception cref="InvalidOperationException">Thrown when encryption fails.</exception>
public string Encrypt(string plainText)
{
    // Implementation
}
```

### Markdown Documentation

- Use clear headings
- Include code examples
- Add links to related documentation
- Keep it up-to-date with code changes

## Common Issues and Solutions

### Issue: NuGet package restore fails
**Solution**: 
```bash
dotnet clean
dotnet restore --no-cache
```

### Issue: Database migration fails
**Solution**: Delete the local database and rebuild:
```bash
# Delete AppData/PlantCare.db (or similar name)
dotnet build
```

### Issue: Build errors after pulling changes
**Solution**:
```bash
git clean -fd
dotnet clean
dotnet build
```

## Reporting Bugs

Use GitHub Issues to report bugs. Include:

1. Clear description of the bug
2. Steps to reproduce
3. Expected vs actual behavior
4. Platform and .NET version
5. Error messages and stack traces
6. Screenshots if applicable

## Feature Requests

Submit feature requests as GitHub Discussions or Issues with:

1. Clear title and description
2. Use cases and benefits
3. Proposed implementation (if applicable)
4. Related to which requirement (if applicable)

## Version Management

This project uses semantic versioning (MAJOR.MINOR.PATCH):
- MAJOR: Breaking changes
- MINOR: New features (backward compatible)
- PATCH: Bug fixes

## Release Process

Releases are made by maintainers:

1. Update version in `PlantCare.App.csproj`
2. Update CHANGELOG.md
3. Create a git tag
4. Build release binaries
5. Publish to app stores

## Questions?

- Check existing GitHub Issues and Discussions
- Review design and requirement documents
- Ask in Pull Requests or Issues
- Contact maintainers directly

## License

By contributing, you agree that your contributions will be licensed under the same license as the project.

Thank you for contributing to PlantCare! 🌱

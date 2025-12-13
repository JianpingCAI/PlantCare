# Contributing to PlantCare

Thank you for your interest in contributing to PlantCare! This document provides guidelines and information for contributors.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Pull Request Process](#pull-request-process)
- [Reporting Bugs](#reporting-bugs)
- [Suggesting Features](#suggesting-features)
- [Documentation](#documentation)

## Code of Conduct

### Our Pledge

We are committed to providing a welcoming and inspiring community for everyone.

### Our Standards

**Positive Behavior:**
- Using welcoming and inclusive language
- Being respectful of differing viewpoints and experiences
- Gracefully accepting constructive criticism
- Focusing on what is best for the community
- Showing empathy towards other community members

**Unacceptable Behavior:**
- Trolling, insulting/derogatory comments, and personal or political attacks
- Public or private harassment
- Publishing others' private information without explicit permission
- Other conduct which could reasonably be considered inappropriate

## Getting Started

### Prerequisites

Before you begin, ensure you have:

1. **.NET 10 SDK** installed
2. **Visual Studio 2022** (v17.11+) or **Visual Studio Code**
3. **.NET MAUI workload** installed
4. **Git** for version control
5. Read the [SETUP_DEVELOPMENT.md](docs/SETUP_DEVELOPMENT.md)

### Fork and Clone

1. Fork the repository on GitHub
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/PlantCare.git
   cd PlantCare
   ```
3. Add the upstream repository:
   ```bash
   git remote add upstream https://github.com/JianpingCAI/PlantCare.git
   ```

### Setting Up Your Development Environment

1. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

2. Build the solution:
   ```bash
   dotnet build
   ```

3. Run tests to ensure everything works:
   ```bash
   dotnet test
   ```

4. Create a branch for your changes:
   ```bash
   git checkout -b feature/your-feature-name
   ```

## Development Workflow

### Branch Naming Convention

Use descriptive branch names with prefixes:

- `feature/` - New features
  - Example: `feature/add-plant-categories`
- `bugfix/` - Bug fixes
  - Example: `bugfix/fix-notification-crash`
- `hotfix/` - Urgent fixes for production
  - Example: `hotfix/critical-data-loss`
- `refactor/` - Code refactoring
  - Example: `refactor/improve-repository-pattern`
- `docs/` - Documentation updates
  - Example: `docs/update-setup-guide`
- `test/` - Adding or updating tests
  - Example: `test/add-service-tests`

### Commit Message Guidelines

Follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

**Format:**
```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

**Examples:**
```
feat(plant): add plant categorization feature

- Add category field to PlantDbModel
- Update UI to show categories
- Add migration for database schema

Closes #123
```

```
fix(notification): resolve crash when scheduling notifications

Fixed null reference exception when plant has no next care date.

Fixes #456
```

### Keeping Your Fork Updated

Regularly sync your fork with the upstream repository:

```bash
git fetch upstream
git checkout master
git merge upstream/master
git push origin master
```

## Coding Standards

### C# Coding Conventions

Follow [Microsoft C# Coding Conventions](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions):

**Naming:**
- Classes, methods, properties: `PascalCase`
- Local variables, parameters: `camelCase`
- Private fields: `_camelCase` (with underscore prefix)
- Constants: `PascalCase`
- Interfaces: `IPascalCase` (prefix with I)

**Example:**
```csharp
public class PlantService : IPlantService
{
    private readonly IPlantRepository _plantRepository;
    private const int MaxPlantAge = 1000;

    public async Task<Plant> GetPlantAsync(Guid plantId)
    {
        var plant = await _plantRepository.GetByIdAsync(plantId);
        return plant;
    }
}
```

### XAML Conventions

**Element Naming:**
- Use `x:Name` with descriptive names in `PascalCase`
- Prefix with control type for clarity

**Example:**
```xaml
<Entry x:Name="PlantNameEntry" />
<Button x:Name="SaveButton" Text="Save" />
<CollectionView x:Name="PlantsCollectionView" />
```

### Code Organization

**File Structure:**
- One class per file
- File name matches class name
- Group related classes in appropriate folders

**Using Statements:**
- Remove unnecessary usings
- Order: System namespaces first, then third-party, then project namespaces

**Whitespace:**
- Use 4 spaces for indentation (no tabs)
- One blank line between methods
- No trailing whitespace

### Comments and Documentation

**XML Documentation:**
```csharp
/// <summary>
/// Gets a plant by its unique identifier.
/// </summary>
/// <param name="plantId">The unique identifier of the plant.</param>
/// <returns>The plant if found; otherwise, null.</returns>
/// <exception cref="ArgumentException">Thrown when plantId is empty.</exception>
public async Task<Plant?> GetPlantByIdAsync(Guid plantId)
{
    // Implementation
}
```

**When to Comment:**
- ✅ Complex algorithms or business logic
- ✅ Non-obvious code decisions
- ✅ Workarounds for known issues
- ❌ Obvious code (let code be self-documenting)
- ❌ Redundant information

### MVVM Best Practices

**ViewModels:**
- Inherit from `ViewModelBase`
- Use `[ObservableProperty]` for bindable properties
- Use `[RelayCommand]` for commands
- Keep UI logic out of ViewModels
- Don't reference Views directly

**Views:**
- Bind to ViewModels, not code-behind
- Use data binding, not event handlers
- Keep code-behind minimal

**Example:**
```csharp
public partial class PlantListViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<Plant> _plants = new();

    [ObservableProperty]
    private Plant? _selectedPlant;

    [RelayCommand]
    private async Task LoadPlantsAsync()
    {
        Plants.Clear();
        var plants = await _plantService.GetAllPlantsAsync();
        foreach (var plant in plants)
        {
            Plants.Add(plant);
        }
    }
}
```

### Dependency Injection

- Use constructor injection
- Avoid service locator pattern
- Register services in `MauiProgram.cs`
- Prefer interfaces over concrete types

**Example:**
```csharp
public class PlantService : IPlantService
{
    private readonly IPlantRepository _repository;
    private readonly IMapper _mapper;
    
    public PlantService(IPlantRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }
}
```

### Error Handling

- Use try-catch blocks for expected errors
- Log errors using `IAppLogger<T>`
- Show user-friendly error messages
- Don't swallow exceptions silently

**Example:**
```csharp
try
{
    await _plantService.DeletePlantAsync(plantId);
}
catch (DbUpdateException ex)
{
    _logger.LogError(ex, "Failed to delete plant {PlantId}", plantId);
    await _dialogService.Notify("Error", "Failed to delete plant. Please try again.");
}
```

### Async/Await Best Practices

- Always await async methods
- Use `Task` not `void` for async methods (except event handlers)
- Don't use `.Result` or `.Wait()` - causes deadlocks
- Use `ConfigureAwait(false)` in library code (not UI code)

**Good:**
```csharp
public async Task LoadDataAsync()
{
    var data = await _service.GetDataAsync();
    // Process data
}
```

**Bad:**
```csharp
public void LoadData()
{
    var data = _service.GetDataAsync().Result; // DON'T DO THIS
}
```

## Pull Request Process

### Before Submitting

1. **Update your branch**:
   ```bash
   git fetch upstream
   git rebase upstream/master
   ```

2. **Run tests**:
   ```bash
   dotnet test
   ```

3. **Build solution**:
   ```bash
   dotnet build
   ```

4. **Check for warnings**: Ensure no build warnings

5. **Format code**: Use Visual Studio's format document (Ctrl+K, Ctrl+D)

### Creating a Pull Request

1. **Push your branch**:
   ```bash
   git push origin feature/your-feature-name
   ```

2. **Open Pull Request** on GitHub

3. **Fill in the PR template**:
   - Clear description of changes
   - Link related issues
   - Screenshots (if UI changes)
   - Testing instructions

4. **Request review** from maintainers

### PR Checklist

- [ ] Code follows the project's coding standards
- [ ] Self-review of code completed
- [ ] Comments added for complex code
- [ ] Documentation updated (if needed)
- [ ] No new warnings introduced
- [ ] Tests added/updated for new functionality
- [ ] All tests pass
- [ ] UI tested on target platforms
- [ ] Commit messages are clear and descriptive

### Review Process

1. Maintainers will review your PR
2. Address any feedback or requested changes
3. Push updates to your branch (PR auto-updates)
4. Once approved, PR will be merged

### After Merge

1. Delete your feature branch:
   ```bash
   git branch -d feature/your-feature-name
   git push origin --delete feature/your-feature-name
   ```

2. Update your master branch:
   ```bash
   git checkout master
   git pull upstream master
   ```

## Reporting Bugs

### Before Reporting

- Check if the bug has already been reported in [Issues](https://github.com/JianpingCAI/PlantCare/issues)
- Try to reproduce the bug on the latest version
- Gather relevant information (logs, screenshots, steps to reproduce)

### Bug Report Template

```markdown
## Description
[Clear description of the bug]

## Steps to Reproduce
1. Go to '...'
2. Click on '...'
3. See error

## Expected Behavior
[What you expected to happen]

## Actual Behavior
[What actually happened]

## Screenshots
[If applicable, add screenshots]

## Environment
- OS: [e.g., Android 13, iOS 16, Windows 11]
- App Version: [e.g., 0.7.0]
- Device: [e.g., Pixel 7, iPhone 14]

## Logs
[Attach relevant logs from app data directory]

## Additional Context
[Any other relevant information]
```

## Suggesting Features

### Feature Request Template

```markdown
## Feature Description
[Clear description of the feature]

## Problem it Solves
[What problem does this feature address?]

## Proposed Solution
[How should this feature work?]

## Alternatives Considered
[What other approaches have you thought about?]

## Additional Context
[Mockups, examples, or other relevant information]

## Priority
[Low / Medium / High]
```

## Documentation

### When to Update Documentation

Update documentation when:
- Adding new features
- Changing existing behavior
- Modifying APIs or services
- Updating database schema
- Changing configuration

### Documentation Locations

- **README.md** - Project overview and quick start
- **docs/ARCHITECTURE.md** - Technical architecture
- **docs/API_DOCUMENTATION.md** - Service and API docs
- **docs/DATABASE_SCHEMA.md** - Database schema
- **Code Comments** - Inline documentation

### Documentation Standards

- Use clear, concise language
- Include code examples
- Keep documentation up-to-date with code
- Use proper markdown formatting
- Add diagrams where helpful

## Testing

### Test Coverage

Aim for test coverage on:
- Business logic (services)
- Data access (repositories)
- ViewModels (critical paths)

### Writing Tests

Follow these guidelines:
- Use descriptive test names: `MethodName_Scenario_ExpectedResult`
- Follow Arrange-Act-Assert pattern
- One assertion per test (when possible)
- Keep tests independent
- Use test fixtures for shared setup

**Example:**
```csharp
[Fact]
public async Task GetPlantById_WithValidId_ReturnsPlant()
{
    // Arrange
    var plantId = Guid.NewGuid();
    var expectedPlant = new Plant { Id = plantId, Name = "Rose" };
    
    // Act
    var result = await _plantService.GetPlantByIdAsync(plantId);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(expectedPlant.Name, result.Name);
}
```

## Questions?

If you have questions not covered here:

1. Check existing documentation in `docs/`
2. Search existing [Issues](https://github.com/JianpingCAI/PlantCare/issues)
3. Create a new issue with your question

## Recognition

Contributors will be acknowledged in:
- Release notes
- CONTRIBUTORS.md file (to be created)
- GitHub contributors page

## Thank You!

Your contributions make PlantCare better for everyone. We appreciate your time and effort! 🌱

---

**Happy Contributing!**

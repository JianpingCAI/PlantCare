# PlantCare Development Team Guidelines

## 📖 Overview

This document provides development standards for the PlantCare project. All team members should follow these guidelines to ensure consistency, quality, and adherence to the design system.

---

## 🎯 Core Principles

1. **Design Consistency**: Every UI element must respect the design system
2. **User Experience**: Prioritize functionality and clarity
3. **Performance**: Keep the app responsive across all devices
4. **Maintainability**: Write code that's easy to understand and modify
5. **Testing**: Verify all changes on multiple platforms before committing

---

## 🎨 Design System

### Color Palette
Reference: `PlantCare.App\Resources\Styles\Colors.xaml`

**Primary Colors**:
- Primary: #2D5016 (Deep Sage Green)
- Secondary: #5EC383 (Vibrant Mint)
- Accent: #A8D5BA (Light Sage)

**Status Colors**:
- Success: #5EC383
- Warning: #F39C12
- Danger: #E74C3C
- Info: #3498DB

**Rule**: Always use `AppThemeBinding` for color references to support dark/light modes.

### Typography System
Reference: `PlantCare.App\Resources\Styles\Typography.xaml`

**Never** hardcode font sizes. Use these styles:

| Style | Size | Weight | Usage |
|-------|------|--------|-------|
| DisplayLargeStyle | 32px | Bold | Page titles |
| DisplayStyle | 28px | Bold | Main headers |
| DisplaySmallStyle | 24px | Bold | Section headers |
| HeadlineLargeStyle | 28px | Bold | Card titles |
| HeadlineStyle | 24px | Bold | Section headers |
| HeadlineSmallStyle | 20px | Bold | Subsection headers |
| TitleLargeStyle | 22px | Bold | Dialog titles |
| TitleStyle | 18px | Bold | Form titles |
| TitleSmallStyle | 16px | Bold | Group headers |
| BodyLargeStyle | 16px | Regular | Large content |
| BodyStyle | 14px | Regular | Default content |
| BodySmallStyle | 12px | Regular | Small content |
| LabelLargeStyle | 14px | Bold | Large labels |
| LabelStyle | 12px | Bold | Default labels |
| LabelSmallStyle | 11px | Bold | Small labels |
| CaptionStyle | 12px | Regular | Helper text |
| CaptionSmallStyle | 11px | Regular | Fine print |

### Component Standards

**Buttons**:
```xaml
<Button 
    CornerRadius="12" 
    HeightRequest="48" 
    MinimumWidthRequest="48"
    FontAttributes="Bold" />
```

**Cards**:
```xaml
<Border 
    Style="{StaticResource ModernCardStyle}"
    Padding="16">
```

**Forms**:
```xaml
<Entry 
    BackgroundColor="{AppThemeBinding Light={StaticResource Gray100}, Dark={StaticResource Gray900}}"
    TextColor="{AppThemeBinding Light={StaticResource DarkGray}, Dark={StaticResource Gray100}}"
    PlaceholderColor="{AppThemeBinding Light={StaticResource Gray400}, Dark={StaticResource Gray500}}" />
```

**Spacing**: Use multiples of 8px (8, 12, 16, 20, 24, 32)

---

## 📝 XAML Standards

### ✅ DO

```xaml
✅ Use typography styles
<Label Style="{StaticResource TitleSmallStyle}" Text="My Text" />

✅ Use AppThemeBinding for colors
<Label TextColor="{AppThemeBinding Light={StaticResource DarkGray}, Dark={StaticResource Gray100}}" />

✅ Use named styles for components
<Border Style="{StaticResource ModernCardStyle}">

✅ Include x:DataType for compile-time binding validation
<ContentPage x:DataType="vm:PlantListOverviewViewModel">

✅ Use meaningful names
Binding="{Binding PlantName}"
Command="{Binding SavePlantCommand}"

✅ Organize with comments
<!-- Photo Section -->
<Border>...

✅ Consistent indentation (4 spaces)
```

### ❌ DON'T

```xaml
❌ Hardcode font sizes
<Label FontSize="16" Text="My Text" />

❌ Use inline colors
<Label TextColor="#2F2F2F" Text="My Text" />

❌ Create new inline styles
<Label FontSize="14" FontAttributes="Bold">

❌ Forget dark mode
<Label TextColor="{StaticResource DarkGray}" />

❌ Use small buttons
<Button HeightRequest="32">

❌ Inconsistent spacing
Padding="5"
Margin="10,4,8,6"

❌ Magic numbers without comments
Grid RowDefinitions="auto,*,120,*"

❌ Forget x:DataType
<ContentPage>
```

---

## 💻 C# Code Standards

### ViewModels

```csharp
✅ CORRECT: MVVM Pattern
public class PlantListOverviewViewModel : BaseViewModel
{
    public ObservableCollection<PlantListItemViewModel> Plants { get; set; }
    
    public IAsyncRelayCommand SelectPlantCommand { get; private set; }
    
    public PlantListOverviewViewModel()
    {
        SelectPlantCommand = new AsyncRelayCommand<PlantListItemViewModel>(OnSelectPlant);
    }
    
    private async Task OnSelectPlant(PlantListItemViewModel plant)
    {
        // Logic here
    }
}

❌ WRONG: Logic in Code-Behind
// DON'T DO THIS
public partial class PlantOverviewView : ContentPage
{
    private void OnPlantSelected(object sender, SelectionChangedEventArgs e)
    {
        // Business logic here - WRONG!
    }
}
```

### Async/Await

```csharp
✅ CORRECT: Always use async/await
public async Task LoadData()
{
    var result = await _service.GetPlantsAsync();
    Plants = new ObservableCollection<Plant>(result);
}

❌ WRONG: Blocking calls
var result = _service.GetPlants().Result; // BLOCKS UI!
```

### Error Handling

```csharp
✅ CORRECT: Proper error handling
try
{
    var result = await _service.GetPlantsAsync();
}
catch (HttpRequestException ex)
{
    await Shell.Current.DisplayAlert("Error", "Failed to load plants", "OK");
    Debug.WriteLine(ex.Message);
}

❌ WRONG: Ignoring errors
var result = await _service.GetPlantsAsync(); // No error handling
```

---

## 🔄 MVVM Architecture

### File Structure
```
Views/
├── PlantOverviewView.xaml
├── PlantOverviewView.xaml.cs (Empty or minimal)

ViewModels/
├── PlantListOverviewViewModel.cs (Contains all logic)

Services/
├── PlantService.cs (Data access)
├── DialogService.cs (UI notifications)
```

### Data Binding Pattern
```xaml
<!-- View -->
<CollectionView ItemsSource="{Binding Plants}">
    <CollectionView.ItemTemplate>
        <DataTemplate>
            <Label Text="{Binding Name}" />
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

```csharp
// ViewModel
public ObservableCollection<Plant> Plants
{
    get => _plants;
    set => SetProperty(ref _plants, value);
}

public IAsyncRelayCommand LoadCommand { get; }

public PlantViewModel()
{
    LoadCommand = new AsyncRelayCommand(LoadPlants);
}

private async Task LoadPlants()
{
    var plants = await _service.GetPlantsAsync();
    Plants = new ObservableCollection<Plant>(plants);
}
```

---

## 🧪 Testing Before Commit

### Platform Testing
- [ ] Build succeeds on Android
- [ ] Build succeeds on iOS
- [ ] Build succeeds on Windows
- [ ] App runs without crashes

### Visual Testing
- [ ] Light mode looks correct
- [ ] Dark mode looks correct
- [ ] Text is readable (good contrast)
- [ ] Spacing is consistent
- [ ] Buttons are easily clickable (48px minimum)

### Responsive Testing
- [ ] Works on small phones (5.5")
- [ ] Works on large phones (6.5")
- [ ] Works on tablets (7-10")
- [ ] No layout breaks

### Functionality Testing
- [ ] Bindings work correctly
- [ ] Commands execute properly
- [ ] No UI blocking
- [ ] Proper error handling

---

## 📋 Git Workflow

### Commit Message Format
```
[TYPE] Brief description

Types: FEATURE, BUGFIX, REFACTOR, DOCS, CHORE

Examples:
[FEATURE] Apply typography styles to PlantOverviewView
[BUGFIX] Fix plant card layout on small screens
[REFACTOR] Extract common card styling
[DOCS] Update development guidelines
[CHORE] Update dependencies
```

### Before Pushing
1. Run full build (no errors)
2. Run all tests
3. Verify all platforms
4. Verify dark/light modes
5. Write clear commit message
6. Create descriptive PR if needed

---

## 🚀 Common Development Tasks

### Adding a New Label
```xaml
<Label 
    Style="{StaticResource BodyStyle}" 
    Text="{Binding PropertyName}"
    TextColor="{AppThemeBinding Light={StaticResource DarkGray}, Dark={StaticResource Gray100}}" />
```

### Creating a Card Section
```xaml
<Border Style="{StaticResource ModernCardStyle}">
    <StackLayout Spacing="12" Padding="16">
        <Label Style="{StaticResource TitleSmallStyle}" Text="Section Title" />
        <Label Style="{StaticResource BodyStyle}" Text="Content" />
    </StackLayout>
</Border>
```

### Adding a Form Field
```xaml
<StackLayout Spacing="8">
    <Label Style="{StaticResource LabelStyle}" Text="Field Name" />
    <Entry 
        Text="{Binding FieldValue}"
        Placeholder="Enter value"
        Style="{StaticResource BodyStyle}" />
    <Label Style="{StaticResource CaptionStyle}" Text="Helper text" />
</StackLayout>
```

### Adding a Button
```xaml
<Button
    Command="{Binding ActionCommand}"
    Text="Action"
    CornerRadius="12"
    HeightRequest="48"
    FontAttributes="Bold" />
```

---

## 📊 Code Review Checklist

When reviewing code, ensure:

- [ ] Design system followed (colors, typography, spacing)
- [ ] Dark/light mode supported
- [ ] MVVM pattern maintained
- [ ] No hardcoded values
- [ ] Async/await used properly
- [ ] Error handling present
- [ ] Tests passing
- [ ] All platforms tested
- [ ] No performance issues
- [ ] Commit message clear

---

## 🆘 Troubleshooting

| Problem | Solution |
|---------|----------|
| Build fails | Check XAML syntax, verify all closing tags, check file encoding |
| Binding error | Add `x:DataType`, verify property names, check ViewModel |
| Colors wrong | Check `AppThemeBinding`, verify Colors.xaml references |
| Text inconsistent | Use typography styles, never hardcode `FontSize` |
| Dark mode broken | Ensure all colors use `AppThemeBinding` |
| Layout issues | Check spacing (8px multiples), verify padding/margin |
| Performance slow | Check bindings, reduce UI complexity, use virtualization |

---

## 📚 Key Resources

- **Design System**: `PlantCare.App\Resources\Styles\`
- **Implementation Guides**: `PHASE_2_*.md` files
- **Project Status**: `PLANTCARE_PROJECT_OVERVIEW.md`
- **Copilot Prompt**: `.github/COPILOT_INSTRUCTIONS.md`

---

## ✨ Quality Standards

Every change must:
1. Follow the design system
2. Support all platforms equally
3. Work in light AND dark mode
4. Follow MVVM pattern
5. Have no hardcoded values
6. Be tested before commit
7. Include clear commit message

---

**Remember**: Consistency drives quality. Follow these guidelines, and PlantCare will remain beautiful and maintainable! 🌿✨


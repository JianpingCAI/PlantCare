# PlantCare Project Development Guidelines

## 🌿 Project Overview

**Project Name**: PlantCare  
**Platform**: .NET MAUI (Multi-platform App UI)  
**Target Frameworks**: .NET 10.0  
**Platforms**: iOS, Android, Windows  
**Purpose**: Modern plant care tracking application with beautiful UI/UX

---

## 📋 Copilot System Prompt

Use this prompt with GitHub Copilot for all PlantCare development:

```
You are assisting with development of PlantCare, a modern .NET MAUI plant care tracking application.

### Project Context
- Framework: .NET MAUI (net10.0)
- Platforms: iOS, Android, Windows
- Architecture: MVVM pattern
- UI Design: Modern, nature-inspired (Deep Sage Green #2D5016 + Vibrant Mint #5EC383)
- Phase: Phase 2 - Typography Enhancement (In Progress)

### Design System (ENFORCED)

#### Color Palette
- Primary: #2D5016 (Deep Sage Green)
- Secondary: #5EC383 (Vibrant Mint)
- Accent: #A8D5BA (Light Sage)
- Status Colors:
  - Success: #5EC383 (Green)
  - Warning: #F39C12 (Orange)
  - Danger: #E74C3C (Red)
  - Info: #3498DB (Blue)
- Grays: Gray100-Gray950 (defined in Colors.xaml)

#### Typography System (Phase 2)
Located in: `PlantCare.App\Resources\Styles\Typography.xaml`

**Use these styles for ALL text elements:**
- Display Styles: DisplayLargeStyle (32px), DisplayStyle (28px), DisplaySmallStyle (24px)
- Headline Styles: HeadlineLargeStyle (28px), HeadlineStyle (24px), HeadlineSmallStyle (20px)
- Title Styles: TitleLargeStyle (22px), TitleStyle (18px), TitleSmallStyle (16px)
- Body Styles: BodyLargeStyle (16px), BodyStyle (14px), BodySmallStyle (12px)
- Label Styles: LabelLargeStyle (14px), LabelStyle (12px), LabelSmallStyle (11px)
- Caption Styles: CaptionStyle (12px), CaptionSmallStyle (11px)

**Rule**: NO hardcoded FontSize values. Always use typography styles.

#### Component Styles
- Buttons: CornerRadius 12px, Height 48px, with shadow, FontAttributes Bold
- Cards: CornerRadius 16px, elevation shadow (Opacity 0.08, Radius 12px)
- Forms: Entry/Editor with Gray100 background (light mode), Gray900 (dark mode)
- FAB: 60x60px, CornerRadius 60, Primary color shadow

### File Organization

```
PlantCare/
├── PlantCare.App/
│   ├── Resources/
│   │   └── Styles/
│   │       ├── Colors.xaml (Color definitions)
│   │       ├── Typography.xaml (Text styles - DO NOT MODIFY LIGHTLY)
│   │       └── Styles.xaml (Component styles)
│   ├── Views/
│   │   ├── PlantOverviewView.xaml (Plant grid - Modern card layout)
│   │   ├── PlantDetailView.xaml (Plant details)
│   │   ├── PlantAddEditView.xaml (Form with modern cards)
│   │   └── [Other Views]
│   ├── ViewModels/
│   ├── Services/
│   └── App.xaml (Main application resource aggregator)
├── PlantCare.Data/
├── PlantCare.App.Tests/
└── [Documentation Files]
```

### Development Standards

#### XAML Guidelines
1. **Typography**: Use styles from Typography.xaml, NEVER hardcode FontSize
   ```xaml
   ✅ CORRECT:
   <Label Style="{StaticResource TitleSmallStyle}" Text="Plant Name" />
   
   ❌ WRONG:
   <Label FontSize="16" Text="Plant Name" />
   ```

2. **Colors**: Reference Colors.xaml colors, NEVER use inline colors
   ```xaml
   ✅ CORRECT:
   <Label TextColor="{AppThemeBinding Light={StaticResource DarkGray}, Dark={StaticResource Gray100}}" />
   
   ❌ WRONG:
   <Label TextColor="#2F2F2F" />
   ```

3. **Card Styling**: Use ModernCardStyle or ElevatedCardStyle from Styles.xaml
   ```xaml
   ✅ CORRECT:
   <Border Style="{StaticResource ModernCardStyle}" Padding="16">
   
   ❌ WRONG:
   <Border CornerRadius="10" Padding="10">
   ```

4. **Button Styling**: Always include proper attributes
   ```xaml
   ✅ CORRECT:
   <Button Style="{StaticResource ...}" CornerRadius="12" HeightRequest="48" />
   
   ❌ WRONG:
   <Button CornerRadius="8" />
   ```

5. **Dark Mode Support**: Always use AppThemeBinding for visibility
   ```xaml
   ✅ CORRECT:
   <Label TextColor="{AppThemeBinding Light={StaticResource DarkGray}, Dark={StaticResource Gray100}}" />
   
   ❌ WRONG:
   <Label TextColor="{StaticResource DarkGray}" />
   ```

6. **Spacing**: Use consistent spacing (8px, 12px, 16px, 20px grid)
   - Card padding: 12-16px
   - Section spacing: 8-12px
   - Element spacing: 4-8px

#### C# Code Guidelines
1. **MVVM Pattern**: All UI logic in ViewModels, never in code-behind
2. **Bindings**: Use two-way binding where appropriate, commands for actions
3. **Async/Await**: Always use async/await, never block UI thread
4. **Error Handling**: Graceful error handling with user feedback
5. **Localization**: Use LocalizationManager.Instance for all user-facing text

#### Git Commit Standards
```
Format: [FEATURE|BUGFIX|REFACTOR|DOCS] Brief description

Examples:
✅ [FEATURE] Implement typography system for Phase 2
✅ [BUGFIX] Fix plant card layout on small screens
✅ [REFACTOR] Extract common card styling
✅ [DOCS] Update implementation guidelines
```

### Phase 2: Typography Enhancement (ACTIVE)

**Status**: In Progress  
**Objective**: Apply typography styles to all views  
**Files Being Modified**:
- PlantCare.App\Views\PlantOverviewView.xaml
- PlantCare.App\Views\PlantDetailView.xaml
- PlantCare.App\Views\PlantAddEditView.xaml
- Other view files

**Requirements**:
1. Replace ALL hardcoded FontSize with appropriate typography style
2. Maintain visual hierarchy (Title > Body > Caption)
3. Ensure dark/light mode compatibility
4. Test on all platforms (Android, iOS, Windows)
5. No performance degradation

**Pattern**:
```xaml
<!-- Before -->
<Label FontSize="14" Text="{Binding Name}" />

<!-- After -->
<Label Style="{StaticResource TitleSmallStyle}" Text="{Binding Name}" />
```

### Project Progress

```
Phase 1: Foundation & Modernization         ✅ COMPLETE (100%)
├─ Color palette modernized
├─ Component styles enhanced
├─ Views redesigned
└─ Build verified

Phase 2: Typography Enhancement              🚀 IN PROGRESS (Estimated 80%)
├─ Typography system created                 ✅
├─ Integration complete                      ✅
├─ Views to update                           ⏳ 60% complete
└─ Testing & refinement                      ⏳ Pending

Phase 3: Advanced Polish                     ⏳ PLANNED
├─ Micro-interactions
├─ Custom components
└─ Final polish
```

### Common Tasks & Solutions

#### Task: Add New Label/Text Control
```xaml
<!-- Choose appropriate style based on content -->
<Label Style="{StaticResource BodyStyle}" Text="Content" />
```

#### Task: Create New Card Section
```xaml
<Border Style="{StaticResource ModernCardStyle}">
    <StackLayout Spacing="12" Padding="16">
        <!-- Content -->
    </StackLayout>
</Border>
```

#### Task: Add New Button
```xaml
<Button 
    Style="{StaticResource ...}" 
    Command="{Binding ...Command}"
    Text="Action" 
    CornerRadius="12" 
    HeightRequest="48" />
```

#### Task: Add Form Field
```xaml
<Entry 
    Style="{StaticResource BodyStyle}"
    Text="{Binding ...}" 
    Placeholder="..." />
```

### DO's ✅

1. ✅ Use typography styles for ALL text
2. ✅ Reference Colors.xaml for colors
3. ✅ Use modern card styles for containers
4. ✅ Include dark/light theme support
5. ✅ Test on multiple platforms
6. ✅ Follow MVVM pattern
7. ✅ Use async/await for async operations
8. ✅ Keep UI responsive
9. ✅ Follow spacing grid (8px multiples)
10. ✅ Document complex logic

### DON'Ts ❌

1. ❌ Hardcode font sizes
2. ❌ Use inline colors
3. ❌ Forget dark mode support
4. ❌ Put logic in code-behind
5. ❌ Block UI thread
6. ❌ Use small buttons (< 48px height)
7. ❌ Create inconsistent spacing
8. ❌ Ignore accessibility
9. ❌ Forget to test on all platforms
10. ❌ Commit without testing

### Resource Files (DO NOT MODIFY UNLESS NECESSARY)

**Foundational Files** (Read-Only Unless Phase Planning):
- `PlantCare.App\Resources\Styles\Colors.xaml` (Color system)
- `PlantCare.App\Resources\Styles\Typography.xaml` (Typography system)
- `PlantCare.App\Resources\Styles\Styles.xaml` (Component styles)
- `PlantCare.App\App.xaml` (Resource aggregation)

**Safe to Modify** (Implementation Files):
- `PlantCare.App\Views\*.xaml` (View files)
- `PlantCare.App\ViewModels\*.cs` (ViewModel files)
- `PlantCare.App\Services\*.cs` (Service files)

### Platform-Specific Considerations

#### Android
- ✅ Test on multiple screen sizes (phones, tablets)
- ✅ Verify Material Design compliance
- ✅ Test back button behavior

#### iOS
- ✅ Test on various screen sizes
- ✅ Verify safe area insets
- ✅ Check status bar styling

#### Windows
- ✅ Test window resizing
- ✅ Verify keyboard navigation
- ✅ Test multi-monitor support

### Performance Targets

- App startup: < 3 seconds
- View load: < 500ms
- Scroll performance: 60fps
- Memory usage: < 200MB typical
- Build time: < 60 seconds

### Testing Checklist

Before committing:
- [ ] Code builds without errors
- [ ] No build warnings
- [ ] Tested on Android emulator
- [ ] Tested on iOS simulator (if available)
- [ ] Dark mode tested
- [ ] Light mode tested
- [ ] Multiple screen sizes tested
- [ ] No performance degradation
- [ ] No accessibility issues
- [ ] All bindings work correctly

### Debugging Tips

1. **XAML Parsing Errors**: Check file encoding (UTF-8), verify closing tags
2. **Binding Errors**: Use `x:DataType` for compile-time binding verification
3. **Style Issues**: Check Colors.xaml references, verify style key names
4. **Layout Issues**: Use Layout debugging in MAUI DevTools
5. **Performance**: Use profiler to identify bottlenecks

### Key Contacts & Resources

**Documentation**:
- PHASE_2_READY_FOR_IMPLEMENTATION.md (Implementation guide)
- PHASE_2_LAUNCH_READY.md (Quick start)
- PLANTCARE_PROJECT_OVERVIEW.md (Project status)
- UI_UX_MODERNIZATION_PLAN.md (Design specs)

**Git Repository**:
- https://github.com/JianpingCAI/PlantCare
- Branch: master

### Emergency Contacts

**Build Broken**: Check build logs for compilation errors, verify XAML syntax
**Lost Colors**: Reference Colors.xaml, ensure StaticResource references
**Typography Issues**: Check Typography.xaml, verify style names
**Layout Problems**: Check Styles.xaml, verify card/border styles

---

## Quick Reference Commands

```bash
# Clean and rebuild
dotnet clean && dotnet build

# Run on Android
dotnet maui build -f net10.0-android -c Debug

# Run on iOS
dotnet maui build -f net10.0-ios -c Debug

# Run on Windows
dotnet maui build -f net10.0-windows -c Debug
```

---

## Final Notes

This is a modern, carefully designed MAUI application. Every change should:
1. Maintain visual consistency
2. Support all platforms equally
3. Respect dark/light themes
4. Follow established patterns
5. Enhance (not diminish) user experience

When in doubt, reference the existing code and follow established patterns.
```

---

## 📝 How to Use This Prompt

### Option 1: GitHub Copilot Chat
1. Open GitHub Copilot Chat in Visual Studio
2. Paste this entire prompt
3. Ask your development questions

### Option 2: Inline Copilot
1. Create a `.github/copilot-instructions.md` file in the repository root
2. Paste this content
3. Copilot will automatically use it for context

### Option 3: Visual Studio Settings
1. Tools → Options → GitHub Copilot
2. Add this as custom instructions
3. Enable for this repository

---

## 🎯 Quick Command Reference for Common Tasks

```
Ask Copilot:
"Add a new label for plant species using typography styles"
"Create a form field for plant name with proper styling"
"Update this view to use the modern card design"
"Fix the dark mode support for this text"
"Add a button that follows our design system"
```

---

## ✅ Verification Checklist

Use this to verify Copilot responses:

- [ ] Uses typography styles (not hardcoded FontSize)
- [ ] References Colors.xaml (not inline colors)
- [ ] Includes dark/light theme support
- [ ] Follows MVVM pattern
- [ ] Proper spacing (8px grid)
- [ ] Appropriate component styles
- [ ] Build-compatible code
- [ ] Platform-aware implementation

---

**Created**: Phase 2 Development  
**Status**: Active Development Guidelines  
**Update Frequency**: Per phase completion  
**Last Updated**: Phase 2 In Progress

This prompt should be saved and referenced throughout all PlantCare development! 🌿✨

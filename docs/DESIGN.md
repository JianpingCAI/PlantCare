# PlantCare Design Document

## Document Information

**Version**: 1.0  
**Last Updated**: 2024-XX-XX  
**Author**: Jianping CAI  
**Status**: Living Document

---

## Table of Contents

1. [Design Philosophy](#1-design-philosophy)
2. [Visual Design](#2-visual-design)
3. [UI Components](#3-ui-components)
4. [Navigation Design](#4-navigation-design)
5. [Interaction Design](#5-interaction-design)
6. [Technical Design Decisions](#6-technical-design-decisions)
7. [Platform-Specific Design](#7-platform-specific-design)
8. [Accessibility Design](#8-accessibility-design)
9. [Design Patterns](#9-design-patterns)
10. [Future Design Considerations](#10-future-design-considerations)

---

## 1. Design Philosophy

### 1.1 Core Principles

**Simplicity First**
- Clean, uncluttered interface
- Focus on essential features
- Minimize cognitive load
- Clear visual hierarchy

**User-Centric**
- Intuitive navigation
- Predictable behavior
- Forgiving of errors
- Helpful feedback

**Delightful Experience**
- Smooth animations
- Responsive interactions
- Visual feedback
- Celebrate user actions (subtle)

**Consistency**
- Unified design language
- Predictable patterns
- Platform conventions
- Coherent color usage

### 1.2 Design Goals

1. **Ease of Use**: A first-time user should understand the app within 30 seconds
2. **Efficiency**: Common tasks completed in ≤ 3 taps
3. **Clarity**: Visual status always clear (which plants need care)
4. **Beauty**: Aesthetically pleasing while functional

---

## 2. Visual Design

### 2.1 Color Palette

**Primary Colors**
```
Primary Green: #48752C
├─ Light: #6FA84C
├─ Dark: #2A5018
└─ Contrast: #FFFFFF (white text)

Purpose: Main brand color, action buttons, active states
```

**Secondary Colors**
```
Water Blue: #2196F3
├─ Light: #64B5F6
└─ Dark: #1976D2

Fertilizer Brown: #795548
├─ Light: #A1887F
└─ Dark: #5D4037

Purpose: Differentiate watering (blue) and fertilization (brown)
```

**Status Colors**
```
Success: #4CAF50 (green)
Warning: #FF9800 (orange)
Error: #F44336 (red)
Info: #2196F3 (blue)

Purpose: Plant care status indicators
```

**Neutral Colors**
```
Background: #FAFAFA (off-white)
Surface: #FFFFFF (white)
Text Primary: #212121 (near-black)
Text Secondary: #757575 (gray)
Divider: #BDBDBD (light gray)
```

**State-Based Colors**
```
Overdue: #F44336 (red) - Plant needs care NOW
Due Soon: #FF9800 (orange) - Care due within 24 hours
Healthy: #4CAF50 (green) - Care not needed yet
```

### 2.2 Typography

**Font Family**
- **Primary**: OpenSans (default MAUI font)
  - Regular for body text
  - Semibold for headings and emphasis

**Type Scale**
```
Hero: 34sp, Semibold
H1: 24sp, Semibold
H2: 20sp, Semibold
H3: 18sp, Semibold
Body: 16sp, Regular
Caption: 14sp, Regular
Small: 12sp, Regular
```

**Line Height**
- Body text: 1.5x font size
- Headings: 1.2x font size

**Text Colors**
- Primary text: 87% opacity black (#212121)
- Secondary text: 60% opacity black (#757575)
- Disabled text: 38% opacity black

### 2.3 Spacing System

**8-Point Grid**
```
XS: 4dp
S: 8dp
M: 16dp
L: 24dp
XL: 32dp
XXL: 48dp
```

**Component Spacing**
- Between sections: 16dp
- Between list items: 8dp
- Card padding: 16dp
- Screen margins: 16dp
- Button padding: 12dp horizontal, 8dp vertical

### 2.4 Elevation & Shadows

**Material Design Elevation Levels**
```
Level 0: Flat (background)
Level 1: 2dp shadow (cards, list items)
Level 2: 4dp shadow (app bar)
Level 3: 8dp shadow (FAB, raised buttons)
Level 4: 16dp shadow (modal dialogs)
```

**Shadow Definition**
```xaml
<Shadow Brush="Black"
        Offset="0,2"
        Radius="4"
        Opacity="0.2" />
```

### 2.5 Shapes & Borders

**Corner Radius**
```
Small: 4dp (chips, tags)
Medium: 8dp (buttons, cards)
Large: 16dp (images, containers)
Circular: 50% (FAB, avatars)
```

**Border Thickness**
```
Thin: 1dp (dividers)
Medium: 2dp (focused inputs)
Thick: 4dp (emphasis)
```

---

## 3. UI Components

### 3.1 Plant Cards

**Design**
- Rectangle with rounded corners (8dp)
- Image thumbnail (120x120dp)
- Plant name (H3, primary text)
- Status indicators (colored dots/icons)
- Next care date (caption, secondary text)

**States**
- Normal: White background, subtle shadow
- Overdue: Red left border (4dp)
- Due Soon: Orange left border (4dp)
- Healthy: Green left border (4dp)

**Touch Target**: Minimum 48x48dp

### 3.2 Buttons

**Primary Button**
- Background: Primary green (#48752C)
- Text: White
- Height: 48dp
- Corner radius: 8dp
- Elevation: 2dp

**Secondary Button**
- Background: Transparent
- Text: Primary green
- Border: 1dp primary green
- Height: 48dp
- Corner radius: 8dp

**Icon Button**
- Size: 48x48dp (touch target)
- Icon size: 24x24dp
- No background (ripple on tap)
- Tint: Primary or secondary text color

**Floating Action Button (FAB)**
- Size: 56x56dp
- Icon size: 24x24dp
- Background: Primary green
- Elevation: 6dp
- Circular shape

### 3.3 Input Fields

**Text Input**
- Border: 1dp gray (normal), 2dp primary (focused)
- Height: 48dp
- Corner radius: 4dp
- Padding: 12dp
- Label: Caption size, secondary text
- Helper text: Small size, secondary text
- Error state: Red border, red helper text

**Date/Time Picker**
- Styled as text input
- Picker icon on right
- Tappable area: Full width

### 3.4 Navigation Bar

**Bottom Tab Bar**
- Height: 56dp
- Background: White
- Elevation: 8dp
- Icons: 24x24dp
- Labels: Small size

**Tabs**
- Home (plant list icon)
- Calendar (calendar icon)
- History (chart icon)
- Settings (gear icon)

**Active State**
- Icon and text: Primary green
- Inactive: Secondary text color

### 3.5 App Bar

**Top App Bar**
- Height: 56dp
- Background: White
- Elevation: 4dp
- Title: H2, primary text
- Icons: 24x24dp

**Actions**
- Search icon
- More menu (3-dot)
- Back arrow (on detail pages)

### 3.6 Lists

**Plant List**
- Grid layout on tablets (2-4 columns)
- Vertical list on phones
- Item height: 140dp
- Spacing: 8dp between items

**History List**
- Timeline style with divider line
- Date on left
- Icon indicator (water drop / fertilizer)
- Time and action on right

### 3.7 Dialogs

**Confirmation Dialog**
- Title: H2
- Message: Body text
- Actions: Two buttons (aligned right)
  - Positive: Primary button
  - Negative: Text button

**Alert Dialog**
- Similar to confirmation
- Single "OK" button

### 3.8 Calendar

**Month View**
- 7-column grid (week days)
- Dates with events: Colored dots
- Current date: Outlined
- Selected date: Filled background

**Event Indicators**
- Blue dot: Watering event
- Brown dot: Fertilization event
- Multiple dots: Multiple events

### 3.9 Charts

**Care History Chart**
- Line chart for trends
- Bar chart for frequency
- Color-coded: Blue (water), Brown (fertilize)
- Legend at bottom
- Touch to see details

### 3.10 Empty States

**Design**
- Centered content
- Icon or illustration
- Heading: H2
- Description: Body text
- Action button (if applicable)

**Example**
```
Icon: Plant illustration
Heading: "No plants yet"
Message: "Add your first plant to get started"
Button: "Add Plant"
```

---

## 4. Navigation Design

### 4.1 Navigation Pattern

**Shell-Based Navigation**
- Bottom tab navigation (primary)
- Hierarchical navigation (secondary)
- Modal navigation (add/edit)

**Navigation Levels**
```
Level 1: Tab Bar (Home, Calendar, History, Settings)
Level 2: Detail pages (Plant Detail)
Level 3: Edit/Add pages (Plant Add/Edit)
Level 4: Modals (Confirmation dialogs)
```

### 4.2 Navigation Flows

**Add Plant Flow**
```
Home → FAB (+) → Add Plant Form → Save → Home (with new plant)
                              → Cancel → Home
```

**View/Edit Plant Flow**
```
Home → Plant Card → Plant Detail → Edit → Edit Form → Save → Detail (updated)
                                                     → Cancel → Detail
                                → Delete → Confirm → Home
                                         → Cancel → Detail
```

**Mark Care Done Flow**
```
Home → Plant Card → Plant Detail → Water Button → Updated Detail
                                 → Fertilize Button → Updated Detail
```

**View History Flow**
```
History Tab → Select Plant → Plant History → Tap Event → Event Detail
                                           → Delete → Confirm → History
```

### 4.3 Back Button Behavior

**Android**
- Back button navigates up hierarchy
- On top-level page: Exit app (with confirmation)
- In forms: Show discard confirmation if changes made

**iOS**
- Swipe from left edge to go back
- Back button in navigation bar

### 4.4 Deep Linking

**Support deep links to**
- Specific plant: `plantcare://plant/{id}`
- Add plant: `plantcare://add`
- Calendar date: `plantcare://calendar/{date}`

---

## 5. Interaction Design

### 5.1 Touch Interactions

**Tap**
- Navigate to details
- Select item
- Activate button
- Minimum touch target: 48x48dp

**Long Press**
- Show context menu
- Quick actions (water/fertilize from list)
- Delete/edit shortcuts

**Swipe**
- Swipe to delete (list items)
- Swipe between tabs (optional)
- Pull to refresh (lists)

**Pinch**
- Zoom images (in plant detail)

### 5.2 Animations

**Transition Animations**
```
Page transitions: 300ms ease-in-out
Card expand: 200ms ease-out
Button press: 100ms ease-in-out
Fade in/out: 200ms
```

**Micro-interactions**
```
Button ripple: Material ripple effect
Loading spinner: Indeterminate progress
Success checkmark: 500ms animation
Error shake: 300ms horizontal shake
```

**Performance**
- Keep animations under 300ms
- 60fps for smooth experience
- Hardware acceleration where possible

### 5.3 Feedback

**Visual Feedback**
- Button press: Ripple effect
- Selection: Highlight/background change
- Success: Green checkmark
- Error: Red icon + shake
- Loading: Progress indicator

**Haptic Feedback** (where supported)
- Button press: Light impact
- Success: Medium impact
- Error: Heavy impact
- Long press: Medium impact

**Audio Feedback** (optional, off by default)
- Success chime
- Error beep
- Notification sound

### 5.4 Loading States

**Skeleton Screens**
- Show layout structure while loading
- Pulse animation
- Replace with actual content

**Progress Indicators**
- Indeterminate spinner: Unknown duration
- Progress bar: Known duration (file operations)
- Inline indicators: Small operations

**Optimistic Updates**
- Update UI immediately
- Rollback if operation fails
- Show subtle "syncing" indicator

---

## 6. Technical Design Decisions

### 6.1 MVVM Architecture

**Rationale**: Separation of concerns, testability, maintainability

**Structure**
```
View (XAML)
  ↓ Bindings
ViewModel (Logic)
  ↓ Services
Model (Data)
```

**Benefits**
- Testable business logic
- Reusable ViewModels
- Clear data flow
- Platform-agnostic logic

### 6.2 Repository Pattern

**Rationale**: Abstract data access, swappable data sources

**Structure**
```
ViewModel
  ↓
Service Layer
  ↓
Repository Interface
  ↓
Concrete Repository (SQLite)
```

**Benefits**
- Easy to test (mock repositories)
- Can swap data sources
- Centralized data logic

### 6.3 Dependency Injection

**Rationale**: Loose coupling, testability, configurability

**Implementation**: .NET built-in DI container

**Benefits**
- Constructor injection
- Lifetime management (Singleton, Transient, Scoped)
- Easy mocking for tests

### 6.4 Messaging Pattern

**Rationale**: Loosely-coupled communication between components

**Implementation**: WeakReferenceMessenger (CommunityToolkit.Mvvm)

**Benefits**
- No direct references between ViewModels
- Prevents memory leaks (weak references)
- Event-driven architecture

### 6.5 Local-First Architecture

**Rationale**: Privacy, offline-first, simplicity

**Design**
- All data stored locally (SQLite)
- No network calls required
- Export/import for backup
- Future: Optional cloud sync

**Benefits**
- Works offline
- Fast performance
- User owns their data
- No server costs

### 6.6 Image Optimization Strategy

**Problem**: Photos can be large (5-10MB)

**Solution**
- Resize to max 1920x1920 pixels
- Generate 200x200 thumbnails
- Use thumbnails in list views
- Lazy load full images

**Benefits**
- Reduced storage usage
- Faster list scrolling
- Better memory management

### 6.7 Notification Strategy

**Design**
- Local notifications only (Plugin.LocalNotification)
- Schedule based on care frequency
- Platform-specific permission handling
- Unique notification IDs per plant/care type

**ID Strategy**
```
Watering: plantId.GetHashCode()
Fertilizing: -plantId.GetHashCode()
```

**Benefits**
- Can cancel specific notifications
- Can reschedule independently
- Collision-free IDs

---

## 7. Platform-Specific Design

### 7.1 Android

**Material Design 3**
- Bottom navigation bar
- FAB for primary action
- Material ripple effects
- Material icons
- System back button handling

**Specific Considerations**
- Status bar color matches app bar
- Navigation bar transparency
- Handle Android lifecycle (pause/resume)
- Runtime permissions (camera, storage, notifications)

### 7.2 iOS

**iOS Human Interface Guidelines**
- Tab bar at bottom (similar to Android)
- SF Symbols where appropriate
- Swipe-back gesture
- iOS-style alerts and action sheets
- Follow safe area insets

**Specific Considerations**
- Handle notch/Dynamic Island
- iOS permission prompts
- App lifecycle events
- Haptic feedback patterns

### 7.3 Windows

**Windows App SDK Guidelines**
- Fluent Design principles
- Title bar with window controls
- Keyboard shortcuts
- Mouse hover states
- Responsive layout for desktop

**Specific Considerations**
- Handle window resizing
- Keyboard navigation
- Right-click context menus
- Larger touch targets acceptable

### 7.4 Responsive Design

**Breakpoints**
```
Small (Phone): < 600dp width
Medium (Phablet/Tablet): 600-1200dp
Large (Desktop): > 1200dp
```

**Adaptive Layouts**
- Phone: Single column, bottom nav
- Tablet: 2-3 column grid, bottom nav
- Desktop: Multi-column, side nav (optional)

---

## 8. Accessibility Design

### 8.1 Screen Reader Support

**Semantic Labels**
- All interactive elements have labels
- Images have alt text descriptions
- Status announced (e.g., "Needs watering")

**Navigation**
- Logical reading order
- Grouped related content
- Skip navigation links

### 8.2 Color Contrast

**WCAG 2.1 AA Compliance**
- Text contrast: ≥ 4.5:1
- Large text: ≥ 3:1
- Icons: ≥ 3:1 against background

**Tools**: Use color contrast checker

### 8.3 Touch Targets

**Minimum Size**
- All touch targets: ≥ 48x48dp
- Spacing between targets: ≥ 8dp

**Visual Targets**
- Can be smaller (e.g., icons)
- Touch area padded to minimum

### 8.4 Text Scaling

**Support Dynamic Type**
- Respect system font size settings
- Test at 100%, 150%, 200% scale
- Avoid fixed heights for text containers
- Allow wrapping

### 8.5 Motion & Animations

**Respect Reduced Motion**
- Check system settings
- Reduce or remove animations if preferred
- Keep essential animations only

---

## 9. Design Patterns

### 9.1 UI Design Patterns

**Pattern**: Pull-to-Refresh
- **Usage**: Refresh plant list
- **Implementation**: CollectionView RefreshView

**Pattern**: Empty State
- **Usage**: No plants, no history
- **Implementation**: Conditional rendering

**Pattern**: Loading State
- **Usage**: Data fetching
- **Implementation**: ActivityIndicator or skeleton screens

**Pattern**: Error State
- **Usage**: Failed operations
- **Implementation**: Error message with retry button

**Pattern**: Confirmation Dialogs
- **Usage**: Destructive actions (delete)
- **Implementation**: DisplayAlert with choices

**Pattern**: Search
- **Usage**: Find plants by name
- **Implementation**: SearchBar with filtered CollectionView

**Pattern**: Infinite Scroll
- **Usage**: Large history lists (future)
- **Implementation**: Pagination with "Load More"

### 9.2 Interaction Patterns

**Pattern**: Swipe Actions
- **Usage**: Quick delete from list
- **Implementation**: SwipeView with delete action

**Pattern**: Context Menu
- **Usage**: Long-press actions
- **Implementation**: FlyoutItem or custom context menu

**Pattern**: Inline Editing
- **Usage**: Quick updates
- **Implementation**: Toggle between view and edit mode

**Pattern**: Progressive Disclosure
- **Usage**: Show more details on demand
- **Implementation**: Expandable sections

---

## 10. Future Design Considerations

### 10.1 Dark Mode

**Planned for v0.9.0**

**Color Scheme**
```
Background: #121212 (dark gray, not black)
Surface: #1E1E1E
Primary: Lighter green (#6FA84C)
Text Primary: #FFFFFF (87% opacity)
Text Secondary: #FFFFFF (60% opacity)
```

**Considerations**
- Ensure sufficient contrast
- Reduce bright colors (less strain)
- Test in low-light environments

### 10.2 Themes

**Planned for v0.9.0**

**Theme Options**
- Light (default)
- Dark
- System default (follow OS)
- Custom colors (user-selectable)

### 10.3 Widgets

**Planned for v0.9.0**

**Home Screen Widget**
- Show plants needing care today
- Quick action to mark as watered
- Tap to open app

**Widget Sizes**
- Small: 2x2 (one plant)
- Medium: 4x2 (3-4 plants)
- Large: 4x4 (grid of plants)

### 10.4 Tablet Optimization

**Planned for v0.8.0**

**Landscape Layout**
- Two-pane layout (list + detail)
- Multi-column grid
- Side navigation (optional)
- Utilize extra space effectively

### 10.5 Desktop Optimization

**Planned for v0.8.0**

**Desktop-Specific**
- Keyboard shortcuts
- Menu bar
- Drag-and-drop for photos
- Resizable windows
- Multiple windows (future)

---

## Design Decision Records (DDRs)

### DDR-001: Why Shell Navigation?

**Decision**: Use .NET MAUI Shell for navigation

**Rationale**:
- Built-in tab navigation
- Deep linking support
- Consistent across platforms
- Route-based navigation
- Flyout menu option

**Alternatives Considered**:
- TabbedPage: Less flexible
- Custom navigation: Too much work

**Status**: Accepted

---

### DDR-002: Why Local-First?

**Decision**: Store all data locally, no cloud dependency

**Rationale**:
- User privacy
- Offline-first
- No server costs
- Simplicity for solo dev
- Fast performance

**Alternatives Considered**:
- Cloud-first: Requires backend, costs, complexity
- Hybrid: Still complex

**Status**: Accepted, cloud sync as optional future feature

---

### DDR-003: Why Material Design Icons?

**Decision**: Use Material Design icons for UI

**Rationale**:
- Free and open
- Wide variety
- Recognizable
- Well-documented
- Cross-platform

**Alternatives Considered**:
- Font Awesome: Similar, but Material Design is standard for Android
- Custom icons: Too much design work

**Status**: Accepted

---

### DDR-004: Why Bottom Tab Navigation?

**Decision**: Use bottom tab bar as primary navigation

**Rationale**:
- Thumb-friendly on phones
- Standard on both iOS and Android
- Persistent navigation
- Quick switching between sections

**Alternatives Considered**:
- Hamburger menu: Hidden, less discoverable
- Top tabs: Harder to reach on large phones

**Status**: Accepted

---

## Design Tools & Resources

### Tools Used

- **Figma** (optional): UI mockups and prototypes
- **Material Design Color Tool**: Color palette generation
- **Contrast Checker**: WCAG compliance verification
- **IconFinder**: Icon search and download

### Design References

- [Material Design 3](https://m3.material.io/)
- [iOS Human Interface Guidelines](https://developer.apple.com/design/human-interface-guidelines/)
- [Fluent Design System](https://www.microsoft.com/design/fluent/)
- [.NET MAUI Design Guidance](https://learn.microsoft.com/dotnet/maui/user-interface/design)

---

## Document Maintenance

### Review Schedule

- **Per Release**: Review design consistency
- **Quarterly**: Evaluate design patterns
- **Annually**: Major design refresh consideration

### Change Process

1. Identify design issue or improvement
2. Document proposed change
3. Create mockup or prototype (if needed)
4. Update design document
5. Implement in code
6. Validate with users (if possible)

---

**Last Updated**: 2024-XX-XX  
**Version**: 1.0  
**Maintainer**: Jianping CAI

---

**Design is never finished, only released! 🎨🌱**

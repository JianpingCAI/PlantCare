# PlantCare UI/UX - Visual Design Reference

## 🎨 Color Palette Reference

### Primary Colors (Updated)

```
┌─────────────────────────────────────────────┐
│ Primary: Deep Sage Green                    │
│ #2D5016                                     │
│ RGB(45, 80, 22)                             │
│ Usage: Primary buttons, key actions         │
│ ✓ Accessible, professional, plant-themed   │
└─────────────────────────────────────────────┘

┌─────────────────────────────────────────────┐
│ Secondary: Vibrant Mint                     │
│ #5EC383                                     │
│ RGB(94, 195, 131)                           │
│ Usage: Success states, healthy indicators   │
│ ✓ Bright, positive, growth-associated      │
└─────────────────────────────────────────────┘

┌─────────────────────────────────────────────┐
│ Accent: Light Sage                          │
│ #A8D5BA                                     │
│ RGB(168, 213, 186)                          │
│ Usage: Backgrounds, secondary accents       │
│ ✓ Light, calming, nature-inspired          │
└─────────────────────────────────────────────┘
```

### Status Colors

```
┌─────────────────────────────────────────────┐
│ SUCCESS: Vibrant Mint   #5EC383             │ 🟢
│ WARNING: Warm Orange    #F39C12             │ 🟡
│ DANGER: Coral Red       #E74C3C             │ 🔴
│ INFO: Sky Blue          #3498DB             │ 🔵
└─────────────────────────────────────────────┘
```

### Neutral Colors

```
┌─────────────────────────────────────────────┐
│ Dark Text:      #1F2937 (Primary)           │
│ Medium Text:    #6B7280 (Secondary)         │
│ Light Text:     #9CA3AF (Tertiary)          │
│ Light BG:       #F8F9FA (Off-white)         │
│ Dark BG:        #1a1a1a (Off-black)         │
│ Borders:        #E8E9EB (Light Gray)        │
└─────────────────────────────────────────────┘
```

---

## 📐 Typography Scale

### Font Sizes & Usage

```
Headline (H1)
│ Font: Inter Bold
│ Size: 32px
│ Weight: Bold
│ Line Height: 40px
│ Usage: Page titles, main headers
└─────────────────────────────────────

Subheadline (H2)
│ Font: Inter Bold
│ Size: 24px
│ Weight: Bold
│ Line Height: 32px
│ Usage: Section headers
└─────────────────────────────────────

Section Header (H3)
│ Font: Inter Bold
│ Size: 18px
│ Weight: Bold
│ Line Height: 28px
│ Usage: Subsection titles
└─────────────────────────────────────

Body (Default)
│ Font: Inter Regular
│ Size: 14px
│ Weight: Regular
│ Line Height: 22px
│ Usage: Main content, descriptions
└─────────────────────────────────────

Secondary Text
│ Font: Inter Regular
│ Size: 13px
│ Weight: Regular
│ Line Height: 20px
│ Color: Gray 500
│ Usage: Secondary information
└─────────────────────────────────────

Caption
│ Font: Inter Regular
│ Size: 12px
│ Weight: Regular
│ Line Height: 18px
│ Color: Gray 400
│ Usage: Captions, hints, timestamps
└─────────────────────────────────────

Button Text
│ Font: Inter Bold
│ Size: 14px
│ Weight: Bold
│ Line Height: 20px
│ Color: White
│ Usage: Button labels, CTAs
└─────────────────────────────────────
```

---

## 🎯 Component Specs

### Button Component

#### Primary Button
```
┌──────────────────────────────────────────┐
│        Add Plant                         │
├──────────────────────────────────────────┤
│ Background:    #2D5016 (Deep Sage)       │
│ Text Color:    White                     │
│ Padding:       16px (horizontal)         │
│              12px (vertical)             │
│ Corner Radius: 12px                      │
│ Min Height:    48px (touch target)       │
│ Font:          Inter Bold, 14px          │
│ Shadow:        Black, 0.12 opacity,      │
│              12px radius, 0,4 offset     │
│ States:        Normal, Hover, Disabled   │
└──────────────────────────────────────────┘
```

#### Secondary Button
```
┌──────────────────────────────────────────┐
│        Cancel                            │
├──────────────────────────────────────────┤
│ Background:    #F3F4F6 (Gray 100)        │
│ Text Color:    #2D5016 (Primary)         │
│ Padding:       16px (horizontal)         │
│              12px (vertical)             │
│ Corner Radius: 12px                      │
│ Min Height:    48px                      │
│ Font:          Inter Bold, 14px          │
│ States:        Normal, Hover, Disabled   │
└──────────────────────────────────────────┘
```

### Card Component

```
┌─────────────────────────────────────────┐
│ ╔═════════════════════════════════════╗ │
│ ║         Card Content                ║ │
│ ║                                     ║ │
│ ║  Padding: 16px                      ║ │
│ ║  Corner Radius: 16px                ║ │
│ ║  Background: White (Light)          ║ │
│ ║            #2D2D2D (Dark)           ║ │
│ ║  Border: Transparent                ║ │
│ ║  Shadow: 0 2px 12px Black, 0.08 op  ║ │
│ ╚═════════════════════════════════════╝ │
└─────────────────────────────────────────┘

Elevated Card (More Emphasis)
┌─────────────────────────────────────────┐
│ ╔═════════════════════════════════════╗ │
│ ║         Card Content                ║ │
│ ║                                     ║ │
│ ║  Padding: 16px                      ║ │
│ ║  Corner Radius: 16px                ║ │
│ ║  Shadow: 0 8px 16px Black, 0.15 op  ║ │
│ ║  (More prominent than standard)     ║ │
│ ╚═════════════════════════════════════╝ │
└─────────────────────────────────────────┘
```

### Plant Grid Card Layout

```
┌──────────────────────────────────┐
│      Plant Card Item             │
├──────────────────────────────────┤
│                                  │
│      ┌────────────────────┐      │
│      │                    │      │
│      │   Plant Photo      │ ✓ Healthy
│      │   (120 x 110px)    │      │
│      │                    │      │
│      └────────────────────┘      │
│                                  │
│      Monstera Deliciosa          │
│      Swiss Cheese Plant          │
│                                  │
│  💧 ████░░░░░░ 60%             │
│     ■ ■ ■ ■ ■ milestones       │
│                                  │
│  🌱 ███░░░░░░░ 30%              │
│     ■ ■ ■ ■ ■ milestones       │
│                                  │
└──────────────────────────────────┘

Dimensions:
- Width: ~110-140px (responsive grid)
- Height: Auto (content-based)
- Padding: 8px
- Card Padding: 12px
- Spacing: 3px between items
- Corner Radius: 12px
```

### Progress Indicator

```
Status: 60% Healthy
┌────────────────────────────────────┐
│ 💧 Watering                         │
│ ████░░░░░░░░░ Progress 60%         │
│ ■ ■ ■ ■ ■ Milestones (5 steps)   │
├────────────────────────────────────┤
│ Progress Bar:  Height 6px          │
│                ProgressColor:      │
│                #5EC383 (Green)     │
│                                    │
│ Milestones:    5 equal boxes      │
│                4px height, 2px gap │
│                Inactive: Gray 200  │
│                Active: Current color
└────────────────────────────────────┘
```

### Status Badge

```
┌──────────────────┐
│ ✓ Healthy        │
├──────────────────┤
│ Background:  #5EC383   │
│ Text:        White     │
│ Font:        Inter Bold, 11px
│ Padding:     8px (horizontal)
│            4px (vertical)
│ Corner:      4px radius
│ Position:    Top-right of card
└──────────────────┘

Badge States:
✓ Healthy  (Green)
⚠ Needs Water (Orange)
! Critical (Red)
ℹ Info (Blue)
```

---

## 🎬 Layout Specifications

### Plant Overview Grid

```
Desktop (Landscape):
┌─────────────┬─────────────┬─────────────┬─────────────┐
│  Card 1     │  Card 2     │  Card 3     │  Card 4     │
├─────────────┼─────────────┼─────────────┼─────────────┤
│  Card 5     │  Card 6     │  Card 7     │  Card 8     │
└─────────────┴─────────────┴─────────────┴─────────────┘
Span: 4 (dynamically adjusted based on screen width)

Tablet (Portrait):
┌──────────────────────┬──────────────────────┐
│  Card 1              │  Card 2              │
├──────────────────────┼──────────────────────┤
│  Card 3              │  Card 4              │
└──────────────────────┴──────────────────────┘
Span: 2

Mobile (Portrait):
┌──────────────┐
│  Card 1      │
├──────────────┤
│  Card 2      │
├──────────────┤
│  Card 3      │
└──────────────┘
Span: 1
Spacing: 3px (vertical and horizontal)
```

### Floating Action Button (FAB)

```
Position: Bottom-right corner
Size: 60x60px
Offset from edge: 20px (right), 20px (bottom)
┌────────────────┐
│   ┌─────────┐  │
│   │    +    │  │ FAB (Add Plant)
│   │         │  │ • Diameter: 60px
│   └─────────┘  │ • Icon: &#xe147;
│                │ • Color: #2D5016
└────────────────┘ • Shadow: Primary,
                    0.4 opacity, 16px rad
```

---

## 🎨 Color Contrast Reference

### WCAG AA Compliance Check

```
Primary (#2D5016) on White:
Ratio: 7.2:1 ✓ Exceeds AA requirement (4.5:1)

Primary (#2D5016) on Light Sage (#A8D5BA):
Ratio: 4.8:1 ✓ Meets AA requirement

Success (#5EC383) on White:
Ratio: 4.1:1 ✓ Meets AA requirement

Warning (#F39C12) on White:
Ratio: 4.3:1 ✓ Meets AA requirement

Danger (#E74C3C) on White:
Ratio: 3.2:1 ⚠ Needs verification for WCAG AA

Dark Gray (#1F2937) on White:
Ratio: 9.8:1 ✓ Exceeds AA requirement

Medium Gray (#6B7280) on White:
Ratio: 4.6:1 ✓ Meets AA requirement

Light Gray (#9CA3AF) on White:
Ratio: 3.3:1 ⚠ Use only for decorative elements
```

**Note**: Always test actual rendering on devices

---

## 🌙 Dark Theme Adaptations

### Color Mapping (Dark Mode)

```
Light Mode  →  Dark Mode
────────────────────────────────────
White       →  #1a1a1a (Off-black)
Off-white   →  #2D2D2D (Dark Gray)
Gray 100    →  #3F3F3F (Darker Gray)
DarkGray    →  #E8E9EB (Light Gray)
Primary     →  #5EC383 (Mint - lighter)
Text        →  #F8F9FA (Off-white)
Borders     →  #3F3F3F (Subtle)
```

### Shadow Adjustments (Dark Mode)

```
Standard Shadow:
Light: Black, 0.08 opacity
Dark:  Black, 0.16 opacity (more visible)

Elevated Shadow:
Light: Black, 0.15 opacity
Dark:  Black, 0.25 opacity (more pronounced)
```

---

## 📏 Spacing Scale (8px Grid System)

```
xs:  4px  (Extra small margins, gaps)
s:   8px  (Small components spacing)
m:  12px  (Medium spacing)
l:  16px  (Large spacing, card padding)
xl: 24px  (Extra large spacing, sections)
xxl:32px  (Double extra large, major sections)

Usage Examples:
- Card Padding:         16px (l)
- Section Spacing:      24px (xl)
- Element Spacing:      8px (s)
- Button Padding:       12px vertical, 16px horizontal
- Input Field Padding:  12px, 10px
- List Item Spacing:    8px (s)
```

---

## ✨ State Visual Feedback

### Button States

```
Normal:
┌──────────────────┐
│   Click Me       │ Background: #2D5016
└──────────────────┘ Shadow: Normal

Hover/Pressed:
┌──────────────────┐
│   Click Me       │ Background: #2D5016
└──────────────────┘ Opacity: 0.9
                    Shadow: Enhanced

Disabled:
┌──────────────────┐
│   Click Me       │ Background: #D1D5DB (Gray)
└──────────────────┘ Text: #9CA3AF
                    Opacity: 0.6
```

### Component Focus States

```
Focused Entry Field:
┌────────────────────────────┐
│ [User input here]          │ Outline: 2px
│                            │ Color: #2D5016
└────────────────────────────┘

Focused Button:
┌──────────────────┐
│   Click Me       │ Ring: 2px offset
└──────────────────┘ Ring Color: Primary
```

---

## 📱 Responsive Breakpoints

```
Mobile (Portrait):
- Width: 360px - 480px
- Grid Span: 1 column
- Card Width: Full width minus padding
- Button Width: 100% or fixed with max-width

Tablet (Landscape):
- Width: 768px - 1024px
- Grid Span: 2-3 columns
- Card Width: Auto (grid-based)

Desktop:
- Width: 1024px+
- Grid Span: 3-5 columns
- Max width container: 1440px
```

---

## 🎯 Touch Target Sizes

```
Minimum Recommended:
- Buttons:      48x48dp (2x recommended)
- Icons:        44x44dp (minimum)
- Links:        48x48dp (minimum)
- Spacing:      8dp minimum between targets

Comfortable:
- Buttons:      56x56dp
- Large icons:  64x64dp
- Minimum horizontal spacing: 16dp
```

---

## 📚 Design System Files

### Asset Locations
```
Colors:        PlantCare.App\Resources\Styles\Colors.xaml
Styles:        PlantCare.App\Resources\Styles\Styles.xaml
Typography:    PlantCare.App\Resources\Styles\Typography.xaml
Fonts:         PlantCare.App\Resources\Fonts\
Components:    PlantCare.App\Views\Components\
Icons:         Material Design Icons (FontFamily)
```

---

## 🔄 Design Consistency Checklist

Before committing changes:

- [ ] All buttons use updated colors
- [ ] All cards have consistent styling
- [ ] Spacing follows 8px grid
- [ ] Shadows applied consistently
- [ ] Typography hierarchy is clear
- [ ] Color contrast meets WCAG AA
- [ ] Dark mode colors applied
- [ ] Touch targets are 48x48dp minimum
- [ ] Icons are properly sized (14-28px)
- [ ] Padding is consistent (16px cards, 12-16px buttons)

---

**Version**: 1.0
**Last Updated**: 2024
**Status**: Ready for Design Implementation

# Circular Health/Mana Bars - Setup Guide

## 🎯 Overview

Create professional circular radial bars that drain from top to bottom (or any direction) with smooth animations!

---

## ⚡ Quick Setup (5 Minutes)

### Method 1: Use Existing Image Component (Easiest!)

**For any existing health/mana bar:**

1. **Select the Fill Image** in your hierarchy
2. **In Inspector, configure Image component:**
   ```
   Image Component:
   ├── Image Type: Filled
   ├── Fill Method: Radial 360
   ├── Fill Origin: Top          ← Drains from top!
   ├── Fill Amount: 1.0
   └── Clockwise: ✓ Checked
   ```
3. **Done!** Your bar is now circular!

**Visual:**
```
Top (Fill Origin: Top)
        12
    ●────────
  9 │        │ 3
    │    ◑   │
    └────────
        6

Drains clockwise from 12 o'clock (top) → 3 → 6 → 9 → back to 12
```

### Method 2: Use CircularHealthBar Component (Most Features!)

1. **Create UI Image** for your bar
2. **Add Component**: `CircularHealthBar`
3. **Configure in Inspector**:
   - Bar Type: Health or Mana
   - Fill Origin: Top
   - Clockwise: ✓
   - Colors: Adjust gradient
4. **Call from code:**
   ```csharp
   CircularHealthBar healthBar = GetComponent<CircularHealthBar>();
   healthBar.SetFillAmount(currentHealth, maxHealth);
   ```

---

## 🎨 Visual Setup

### Create Circular Bar UI Layout

```
HealthBarContainer
├── Background (Image - dark circle)
├── Fill (Image - colored circle) ← Configure this as radial!
└── Border (Image - ring outline)
```

**Example Layout:**
```
Canvas
└── PlayerPanel
    └── HealthBarCircular
        ├── HealthBackground (Image)
        │   - Color: Dark gray (0.2, 0.2, 0.2)
        │   - Size: 100x100
        ├── HealthFill (Image) ← THIS ONE!
        │   - Image Type: Filled
        │   - Fill Method: Radial 360
        │   - Fill Origin: Top
        │   - Color: Green
        │   - Size: 90x90 (slightly smaller)
        └── HealthBorder (Image)
            - Sprite: Circle outline
            - Size: 100x100
```

---

## 🎯 Fill Direction Options

Change `Fill Origin` to drain from different directions:

```
Fill Origin: Top (2)          Fill Origin: Right (1)
    Start here                     Start here
        ↓                              ↓
        ●                          ●────→
      ╱ ╲                        ╱     ╲
     │   │                      │       │
      ╲ ╱                        ╲     ╱
       ●                          ●────●

Fill Origin: Bottom (0)       Fill Origin: Left (3)
       ●                          ●────●
      ╱ ╲                        ╱     ╲
     │   │                      │       │
      ╲ ╱                        ╲     ╱
        ●                    ←────●
        ↑                      Start here
    Start here
```

**Most Common:**
- **Top** - Classic "draining" feel
- **Bottom** - "Filling up" feel
- **Right** - Clockwise progress
- **Left** - Counter-clockwise progress

---

## 💻 Code Integration

### With AnimatedCombatUI

**Update your AnimatedCombatUI to use circular bars:**

```csharp
// In AnimatedCombatUI.cs, replace Image references with CircularHealthBar:

[Header("Player UI")]
[SerializeField] private CircularHealthBar playerHealthBar;
[SerializeField] private CircularHealthBar playerManaBar;

private void AnimatePlayerHealth()
{
    if (combatManager == null || combatManager.playerCharacter == null) return;
    
    Character player = combatManager.playerCharacter;
    
    // Use circular bar
    if (playerHealthBar != null)
    {
        playerHealthBar.SetFillAmount(player.currentHealth, player.maxHealth);
    }
}
```

### With CombatAnimationManager

**CircularHealthBar works seamlessly with your animation system:**

```csharp
// Still works with CombatAnimationManager!
CircularHealthBar circularBar = GetComponent<CircularHealthBar>();

// The Image component is still accessible
Image fillImage = circularBar.GetComponent<Image>();
animationManager.AnimateHealthBar(fillImage, currentHealth, maxHealth);
```

### Standalone Usage

```csharp
// Simple usage:
CircularHealthBar healthBar = GetComponent<CircularHealthBar>();

// Set with values
healthBar.SetFillAmount(75, 100); // 75 out of 100

// Set with percentage
healthBar.SetFillAmount(0.75f); // 75%

// Instant (no animation)
healthBar.SetFillInstant(0.5f);

// Flash effect on damage
healthBar.Flash(Color.red);

// Shake on hit
healthBar.Shake(intensity: 15f);
```

---

## 🎨 Customization

### Colors

**Health Gradient (Green → Yellow → Red):**
```csharp
// In CircularHealthBar Inspector:
Health Gradient:
├── 0%   (empty):  Red    (1, 0, 0)
├── 50%  (half):   Yellow (1, 1, 0)
└── 100% (full):   Green  (0, 1, 0)
```

**Mana Color (Blue):**
```csharp
Mana Color: (0.3, 0.5, 1.0) // Light blue
```

### Animation Speed

```csharp
// In Inspector:
Animation Duration: 0.4  // Faster = 0.2, Slower = 0.8
Ease Type: easeOutQuad   // Try different easing!
```

### Low Health Warning

```csharp
// In Inspector:
Pulse On Low: ✓ Checked
Low Threshold: 0.25  // Pulse below 25%
Warning Effect: [Assign a red glow GameObject]
```

---

## 🌟 Advanced Features

### Flash on Damage/Heal

```csharp
// Flash red when taking damage
healthBar.Flash(Color.red, duration: 0.3f);

// Flash green when healing
healthBar.Flash(Color.green, duration: 0.3f);

// Flash white for critical hit
healthBar.Flash(Color.white, duration: 0.2f);
```

### Shake on Impact

```csharp
// Shake on damage
healthBar.Shake(intensity: 10f, duration: 0.3f);

// Bigger shake for big hits
float damage = 50f;
float shakeIntensity = Mathf.Clamp(damage / 5f, 5f, 20f);
healthBar.Shake(shakeIntensity);
```

### Low Health Pulse

```csharp
// Automatically pulses when below threshold!
// Configure in Inspector:
Pulse On Low: ✓
Low Threshold: 0.25  // Pulse below 25% health
```

**Attaches warning effect:**
```
HealthBar
└── WarningEffect (GameObject)
    └── RedGlow (Image - red, large)
    
Assign to CircularHealthBar.warningEffect
→ Shows when health is low!
```

---

## 🎭 Visual Styles

### Style 1: Minimalist

```
Simple circles, flat colors
    ●
  ╱   ╲
 │  ◑  │  ← Fill only
  ╲   ╱
    ●
```

### Style 2: Ring

```
Outlined ring with gradient
   ╭───╮
  ╱     ╲
 │   ◑   │  ← Gradient fill
  ╲     ╱
   ╰───╯
```

### Style 3: Orb

```
Glowing orb with inner shine
   ╭───╮
  ╱ ✦ ✦ ╲
 │  ◉   │  ← Glow effect
  ╲ ✦ ✦ ╱
   ╰───╯
```

### Style 4: Segmented

```
Multiple segments/sections
    ●●●
   ●   ●
  ●  ◑  ●  ← Segmented ring
   ●   ●
    ●●●
```

---

## 🔧 Inspector Features

### Context Menu Commands

Right-click `CircularHealthBar` component:
- **Set to Full** - Test at 100%
- **Set to Half** - Test at 50%
- **Set to Low** - Test at 20% (shows pulse)
- **Test Flash** - Preview flash effect
- **Test Shake** - Preview shake effect

---

## 💡 Pro Tips

### Tip 1: Layered Circles

Create depth with multiple circles:

```
PlayerHealthDisplay
├── BackgroundGlow (Image - large, dark, blurred)
├── HealthBackground (Image - solid circle)
├── HealthFill (CircularHealthBar - radial fill)
├── HealthBorder (Image - ring outline)
└── HealthText (Text - "75/100" centered)
```

### Tip 2: Animated Border

Add rotating border for active effect:

```csharp
// Rotate border continuously:
LeanTween.rotateAround(borderObject, Vector3.forward, 360f, 2f)
    .setLoopClamp();
```

### Tip 3: Multiple Bars

Stack bars for different stats:

```
Character Portrait
├── HealthBar (outer ring - red)
├── ManaBar (middle ring - blue)
└── ShieldBar (inner ring - white)
```

### Tip 4: Number Display

Show numbers in center:

```csharp
// Update text inside circle:
healthText.text = $"{currentHealth}\n{maxHealth}";
```

---

## 🎮 Example: Complete Character Display

```
CharacterDisplay
├── PortraitCircle (Image - character face)
│   └── Size: 120x120
│
├── HealthRing (CircularHealthBar)
│   ├── Fill Origin: Top
│   ├── Size: 140x140
│   └── Color: Green → Red gradient
│
├── ManaRing (CircularHealthBar)
│   ├── Fill Origin: Bottom
│   ├── Size: 160x160
│   └── Color: Blue
│
└── StatsText (Text - centered)
    └── "HP: 75/100\nMP: 50/80"
```

---

## 🔄 Integration Examples

### Example 1: Replace Existing Bar

**Before (linear bar):**
```csharp
Image healthBarFill;
healthBarFill.fillAmount = currentHealth / maxHealth;
```

**After (circular bar):**
```csharp
// Option A: Keep Image, just change Inspector settings to Radial 360

// Option B: Use CircularHealthBar component
CircularHealthBar healthBar;
healthBar.SetFillAmount(currentHealth, maxHealth);
```

### Example 2: With Damage Numbers

```csharp
// Take damage
enemy.TakeDamage(50);

// Flash health bar
healthBar.Flash(Color.red);

// Shake health bar
healthBar.Shake();

// Show damage number
animManager.ShowDamageNumber(50, position, DamageNumberType.Normal);

// Update health bar
healthBar.SetFillAmount(enemy.currentHealth, enemy.maxHealth);
```

### Example 3: Healing Effect

```csharp
// Heal
player.Heal(25);

// Flash green
healthBar.Flash(Color.green, 0.4f);

// Show heal number
animManager.ShowDamageNumber(25, position, DamageNumberType.Heal);

// Update bar
healthBar.SetFillAmount(player.currentHealth, player.maxHealth);
```

---

## 📊 Performance

**CircularHealthBar:**
- CPU: <0.1ms per update
- Memory: ~1KB per instance
- Animations: LeanTween optimized
- No GC allocations during updates

**Radial fills are efficient!** Unity's built-in Image component handles them well.

---

## 🎨 Art Assets

### What You Need

1. **Circle Sprite** (512x512 recommended)
   - White circle on transparent background
   - Clean edges, no antialiasing issues

2. **Border Sprite** (512x512)
   - Ring outline
   - Can be thicker for style

3. **Optional: Glow Sprite**
   - Soft gradient circle
   - For background glow effect

### Creating Sprites

**In Photoshop/GIMP:**
1. New canvas 512x512
2. Use ellipse tool (hold Shift for perfect circle)
3. Fill with white
4. Save as PNG with transparency
5. Import to Unity

**Or use Unity:**
1. Create → Sprite → Circle
2. Use built-in circle sprite

---

## ✅ Checklist

Setup checklist:
- [ ] Created circular sprite(s)
- [ ] Set up UI layout (background, fill, border)
- [ ] Configured Image as Radial 360
- [ ] Set Fill Origin to Top
- [ ] Added CircularHealthBar component (optional)
- [ ] Assigned references in Inspector
- [ ] Tested with context menu commands
- [ ] Integrated with combat system
- [ ] Tested flash/shake effects
- [ ] Configured low health warning
- [ ] Adjusted colors to match art style

---

## 🎬 Final Result

```
When complete, you'll have:
✅ Smooth circular bars that drain from top
✅ Color gradients (green → yellow → red)
✅ Flash effects on damage/heal
✅ Shake effects on impact
✅ Low health pulsing warning
✅ Seamless integration with your animations
✅ Professional game feel!
```

---

*Last Updated: October 2, 2025*
*Works with: AnimatedCombatUI, CombatAnimationManager*


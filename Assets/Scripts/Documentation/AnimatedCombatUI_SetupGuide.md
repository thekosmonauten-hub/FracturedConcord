# Animated Combat UI - Setup Guide

## 🎮 Overview

A modern, clean Combat UI system designed to work seamlessly with `CombatAnimationManager`. Replaces `SimpleCombatUI` with a production-ready solution that avoids scaling conflicts and provides smooth animations throughout.

---

## ✨ Key Features

✅ **Animation-First Design** - Built specifically for CombatAnimationManager
✅ **No Scaling Conflicts** - Clean, consistent UI approach
✅ **Card Pooling** - Performance-optimized card management
✅ **Smooth Transitions** - All UI updates are animated
✅ **Enemy Targeting** - Click-to-target with visual feedback
✅ **Real-time Updates** - Health/mana bars update automatically
✅ **Hover Effects** - Cards lift and scale on hover
✅ **Clean Architecture** - Modular, extensible, maintainable

---

## 📦 Components

| Script | Purpose |
|--------|---------|
| `AnimatedCombatUI.cs` | Main UI controller, manages all combat UI |
| `CardVisualizer.cs` | Displays card data on card prefabs |
| `CardHoverEffect.cs` | Handles card hover animations |

---

## 🚀 Quick Setup (10 Minutes)

### Step 1: Create UI GameObject

1. **Open your Combat Scene**
2. **Create Empty GameObject**: Hierarchy → Create Empty
3. **Name it**: `AnimatedCombatUI`
4. **Add Component**: `AnimatedCombatUI`

### Step 2: Create Player UI Panel

1. **Create Panel**: UI → Panel (name: `PlayerPanel`)
2. **Add these children**:
   - **Health Bar**:
     - `HealthBarBackground` (Image - dark gray)
     - `HealthBarFill` (Image - green, Fill Type: Filled)
   - **Mana Bar**:
     - `ManaBarBackground` (Image - dark gray)
     - `ManaBarFill` (Image - blue, Fill Type: Filled)
   - **Text Elements**:
     - `PlayerName` (Text)
     - `PlayerHealthText` (Text - "100/100")
     - `PlayerManaText` (Text - "10/10")

### Step 3: Create Enemy Panels (x3)

For each enemy (Enemy1, Enemy2, Enemy3):

1. **Create Panel**: UI → Panel (name: `Enemy1Panel`)
2. **Add children**:
   - `EnemyName` (Text)
   - `HealthBarBackground` (Image)
   - `HealthBarFill` (Image - red, Fill Type: Filled)
   - `HealthText` (Text - "50/50")
   - `IntentText` (Text - "Attack: 10")
   - `SelectionIndicator` (Image - yellow outline, initially hidden)
   - `Border` (Image - outline around panel)
   - `ClickArea` (Button - covers entire panel, no visual)

### Step 4: Create Card Hand Area

1. **Create Empty GameObject**: `CardHandParent`
2. **Position**: Bottom center of screen
3. **This is where cards will spawn**

### Step 5: Create Deck/Discard UI

1. **Deck Display**:
   - Position: Bottom left
   - `DeckCountText` (Text - "20")
   - Optional: Deck pile image
2. **Discard Display**:
   - Position: Bottom right
   - `DiscardCountText` (Text - "0")
   - Optional: Discard pile image

### Step 6: Create Turn UI

1. **Turn Indicator** (GameObject):
   - `TurnIndicatorText` (Text - "YOUR TURN")
   - Position: Top center
   - Large font, bold
2. **End Turn Button**:
   - Position: Bottom right
   - Text: "End Turn"

### Step 7: Create Combat Log (Optional)

1. **Create Text**: `CombatLog`
2. **Position**: Top left
3. **Shows recent actions**

### Step 8: Assign References in Inspector

Select `AnimatedCombatUI` GameObject and assign:

**References:**
- Combat Manager → Your `CombatManager`
- Animation Manager → Your `CombatAnimationManager`

**Player UI:**
- Player Health Bar Fill → `HealthBarFill` Image
- Player Health Text → Text component
- Player Mana Bar Fill → `ManaBarFill` Image
- Player Mana Text → Text component
- Player Name Text → Text component

**Enemy UI (expand Enemy Panels array to 3):**
For each panel:
- Panel Object → The parent panel GameObject
- Name Text → Enemy name text
- Health Bar Fill → Health fill image
- Health Text → Health text
- Intent Text → Intent text
- Selection Indicator → Selection highlight GameObject
- Border → Border image
- Click Area → Button component

**Card Hand UI:**
- Card Hand Parent → `CardHandParent` Transform
- Card Prefab → Your card prefab (create if needed)
- Card Spacing → 140
- Card Y Position → -300
- Card Scale → (1, 1, 1)

**Deck/Discard UI:**
- Deck Count Text
- Discard Count Text
- Deck Position → Empty GameObject at deck location
- Discard Position → Empty GameObject at discard location

**Turn UI:**
- Turn Indicator → GameObject
- Turn Indicator Text
- End Turn Button

**Combat Log:**
- Combat Log Text (optional)

### Step 9: Create Card Prefab

1. **Create UI GameObject**: UI → Panel (name: `CardPrefab`)
2. **Set Size**: 120x180
3. **Add children**:
   - `Background` (Image - for card background color)
   - `Border` (Image - for card border)
   - `CardName` (Text - top of card)
   - `Cost` (Text - top right corner)
   - `Type` (Text - below name)
   - `Damage` (Text - center, large)
   - `Description` (Text - bottom)
4. **Add Components**:
   - `CardVisualizer` (assigns references automatically)
   - `CardHoverEffect`
   - `Button` (for clicking)
5. **Save as Prefab**
6. **Assign to AnimatedCombatUI**

### Step 10: Test!

Press Play and test:
- ✅ Cards appear in hand
- ✅ Cards hover on mouse over
- ✅ Cards animate when played
- ✅ Health/mana bars animate
- ✅ Enemy panels show correctly
- ✅ Clicking enemies targets them

---

## 🎨 Customization

### Card Appearance

Edit `CardVisualizer.cs` to customize:
- Colors based on card type/rarity
- Font sizes and styles
- Layout and positioning
- Icon support

### Card Spacing

In `AnimatedCombatUI`:
- **Card Spacing** - Distance between cards (default: 140)
- **Card Y Position** - Vertical position of hand (default: -300)
- **Card Scale** - Size of cards (default: 1,1,1)

### Animation Speeds

In `CombatAnimationConfig`:
- Adjust all animation durations
- Change easing curves
- Modify hover effects

---

## 🔧 Advanced Configuration

### Enemy Panel Layout

Modify `EnemyPanel` class in `AnimatedCombatUI.cs` to add:
- Status effect icons
- Shield values
- Enemy type indicators
- More detailed intents

### Card Pool Size

Increase pool size for more cards:

```csharp
// In InitializeCardPool():
int poolSize = 15; // Increase from 10
```

### Custom Card Animations

Add custom animations in `CardVisualizer`:

```csharp
public void PlayCustomAnimation()
{
    // Your custom animation
    LeanTween.scale(gameObject, Vector3.one * 1.5f, 0.3f)
        .setEase(LeanTweenType.easeOutBack);
}
```

---

## 📊 Comparison: SimpleCombatUI vs AnimatedCombatUI

| Feature | SimpleCombatUI | AnimatedCombatUI |
|---------|----------------|-------------------|
| **Scaling System** | Complex, conflict-prone | Simple, consistent |
| **Animations** | ❌ None built-in | ✅ Fully integrated |
| **Card Pooling** | ❌ Instantiate/Destroy | ✅ Object pooling |
| **Enemy Targeting** | ❌ Not supported | ✅ Click-to-target |
| **Hover Effects** | ❌ No hover | ✅ Animated hover |
| **Health Bars** | ⚠️ Instant updates | ✅ Smooth animations |
| **Architecture** | ⚠️ Monolithic | ✅ Modular components |
| **Performance** | ⚠️ GC allocations | ✅ Pool-optimized |

---

## 🎯 Integration with CombatManager

The UI automatically integrates with `CombatManager`:

```csharp
// CombatManager triggers:
combatManager.OnCardPlayed += OnCardPlayed;
combatManager.OnTurnEnded += OnTurnEnded;
combatManager.OnCombatEnded += OnCombatEnded;

// UI automatically updates when:
- Health changes → Animated health bar
- Mana changes → Animated mana bar
- Cards drawn → Card draw animation
- Cards played → Card play animation
- Turn changes → Turn transition animation
```

---

## 🐛 Troubleshooting

### Cards Don't Appear

**Check:**
- ✅ Card Prefab is assigned
- ✅ Card Hand Parent exists
- ✅ Combat Manager has cards in deck
- ✅ Card pool initialized (check console)

**Fix:**
```csharp
// Verify in Start():
Debug.Log($"Card pool size: {cardPool.Count}");
Debug.Log($"Cards in hand: {combatManager.GetCurrentHand().Count}");
```

### Health Bars Don't Animate

**Check:**
- ✅ CombatAnimationManager exists in scene
- ✅ Animation Manager reference is assigned
- ✅ Health Bar Fill images are assigned
- ✅ Images have Fill Type: Filled

**Fix:**
- Select AnimatedCombatUI
- Verify all references are assigned (not null)

### Cards Don't Scale Properly

**Issue:** Cards appear too large/small

**Fix:**
1. Adjust `Card Scale` in Inspector (try 0.8 or 1.2)
2. Modify card prefab RectTransform size
3. Adjust `Card Spacing` to prevent overlap

### Hover Effect Not Working

**Check:**
- ✅ CardHoverEffect component on card prefab
- ✅ Card has EventSystem in scene
- ✅ Card has Raycast Target enabled on Image

**Fix:**
- Ensure Canvas has GraphicRaycaster
- Card prefab needs Image or Button with raycast enabled

### Enemy Panels Not Clickable

**Check:**
- ✅ Button component on ClickArea
- ✅ Button has Image with Raycast Target enabled
- ✅ Canvas has GraphicRaycaster

**Fix:**
```csharp
// In enemy panel setup:
button.image.raycastTarget = true;
```

---

## 🔌 Integration Examples

### Show Damage When Enemy Hit

```csharp
// In your damage application code:
public void DamageEnemy(int enemyIndex, float damage)
{
    Enemy enemy = combatManager.enemies[enemyIndex];
    enemy.TakeDamage(damage);
    
    // Show damage number
    Vector3 enemyPos = animatedCombatUI.GetEnemyScreenPosition(enemyIndex);
    CombatAnimationManager.Instance.ShowDamageNumber(
        damage, 
        enemyPos, 
        DamageNumberType.Normal
    );
    
    // Screen shake
    CombatAnimationManager.Instance.ShakeCamera(0.5f);
    
    // UI updates automatically via Update()
}
```

### Add Card to Hand Manually

```csharp
// Force refresh card display:
animatedCombatUI.UpdateCardHandUI();
```

### Custom Turn Transition

```csharp
// Override turn transition:
animatedCombatUI.LogMessage("Enemy attacks!");
```

---

## 📦 Migration from SimpleCombatUI

### Step 1: Backup

1. Duplicate your Combat Scene
2. Save as `CombatScene_Backup`

### Step 2: Replace Component

1. Remove `SimpleCombatUI` component
2. Add `AnimatedCombatUI` component
3. Follow setup guide above

### Step 3: Update References

Any scripts referencing `SimpleCombatUI`:

```csharp
// Old:
SimpleCombatUI simpleCombat = GetComponent<SimpleCombatUI>();

// New:
AnimatedCombatUI animatedCombat = GetComponent<AnimatedCombatUI>();
```

### Step 4: Test Thoroughly

- Test all card interactions
- Verify enemy targeting
- Check all animations
- Test turn flow

---

## 🎨 Visual Polish Tips

### Card Glow on Playable

Add to `CardVisualizer`:

```csharp
private void Update()
{
    bool canPlay = ownerCharacter != null && 
                   currentCard.CanUseCard(ownerCharacter);
    
    if (cardBorder != null)
    {
        cardBorder.color = canPlay ? Color.green : Color.white;
    }
}
```

### Enemy Flash on Hit

Add to damage handling:

```csharp
private void FlashEnemy(int enemyIndex)
{
    Image enemyImage = enemyPanels[enemyIndex].panelObject.GetComponent<Image>();
    if (enemyImage != null)
    {
        LeanTween.value(gameObject, Color.white, Color.red, 0.1f)
            .setOnUpdate((Color c) => enemyImage.color = c)
            .setLoopPingPong(1);
    }
}
```

### Card Trail Effect

Add particle system to card prefab, enable when playing.

---

## 🚀 Next Steps

### Enhancements to Add

1. **Status Effects** - Icons above enemy panels
2. **Combo Indicators** - Show when cards combo
3. **Mana Cost Highlighting** - Red cost when can't afford
4. **Draw/Discard Counters** - Animate when cards move
5. **Victory Screen** - Animate rewards and exp
6. **Card Tooltips** - Detailed info on hover
7. **Buff/Debuff Icons** - Visual status indicators

### Performance Optimizations

1. **Disable Update when not visible**
2. **Batch UI updates** - Update every N frames
3. **Cull off-screen elements**
4. **Use object pooling for effects**

---

## 📖 API Reference

### Public Methods

```csharp
// Update all UI elements
animatedCombatUI.UpdateAllUI();

// Get enemy screen position for animations
Vector3 pos = animatedCombatUI.GetEnemyScreenPosition(0);

// Get player screen position for animations
Vector3 pos = animatedCombatUI.GetPlayerScreenPosition();

// Log combat message
animatedCombatUI.LogMessage("Critical hit!");
```

### Configuration

```csharp
// Adjust in Inspector:
cardSpacing = 140f;      // Distance between cards
cardYPosition = -300f;   // Vertical position
cardScale = Vector3.one; // Card size
```

---

## ✅ Checklist

Before going live, verify:

- [ ] All UI references assigned in Inspector
- [ ] Card prefab created and configured
- [ ] Combat Manager connected
- [ ] Animation Manager in scene
- [ ] Cards appear correctly
- [ ] Card animations play
- [ ] Health/mana bars animate
- [ ] Enemy targeting works
- [ ] Turn transitions animate
- [ ] End turn button works
- [ ] Combat log displays messages
- [ ] No console errors

---

*Last Updated: October 2, 2025*
*Compatible with: Unity 2021.3+, LeanTween 2.50+*


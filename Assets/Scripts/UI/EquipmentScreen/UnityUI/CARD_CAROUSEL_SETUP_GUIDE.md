# Card Carousel Setup Guide
## Smooth Snapping Carousel for Embossing UI

**Component:** `CardCarouselUI.cs`  
**Purpose:** Display deck cards in a carousel with smooth snapping and scaling

---

## 🎯 What You're Building

A card carousel that:
- ✅ Shows one card centered (large and highlighted)
- ✅ Shows other cards at edges (smaller and dimmed)
- ✅ Smooth snapping when switching cards
- ✅ Button navigation (Previous/Next)
- ✅ Swipe/drag navigation
- ✅ Auto-loads from DeckManager

**Visual:**
```
┌─────────────────────────────────────┐
│  [小]  [████ CARD ████]  [小]       │
│  Side    CENTER CARD     Side       │
│  0.7x      1.0x           0.7x      │
│  Dimmed   Highlighted    Dimmed     │
└─────────────────────────────────────┘
   < Button              Button >
```

---

## 🏗️ Hierarchy Structure

```
EmbossingNavDisplay
└── EmbossingCardDisplay (Attach CardCarouselUI.cs)
    ├── Viewport (with Mask)
    │   └── Content (CardContainer)
    │       └── (Cards created dynamically)
    ├── ButtonPrevious
    └── ButtonNext
```

---

## 🛠️ Step 1: Create ScrollRect

1. **Select** `EmbossingCardDisplay`
2. **Add Component** → Scroll Rect
3. **Configure ScrollRect:**
   - Content: (will set later)
   - Horizontal: ✓ (checked)
   - Vertical: ☐ (unchecked)
   - Movement Type: Elastic
   - Elasticity: 0.1
   - Inertia: ✓
   - Deceleration Rate: 0.135
   - Scroll Sensitivity: 10
   - Viewport: (will set next)
   - Horizontal Scrollbar: None
   - Vertical Scrollbar: None

---

## 🛠️ Step 2: Create Viewport & Content

1. **Create Viewport:**
   - Right-click `EmbossingCardDisplay` → Create Empty
   - Name: `Viewport`
   - Add Component → Image (any color, will be masked)
   - Add Component → Mask
   - Mask component: Show Mask Graphic ☐ (unchecked)
   - **RectTransform:** Stretch to fill parent (anchor all corners)

2. **Create Content:**
   - Right-click `Viewport` → Create Empty
   - Name: `Content`
   - Add Component → Horizontal Layout Group
   - **RectTransform:**
     - Anchor: Top-Left
     - Pivot: (0, 1)
     - Position: (0, 0)
     - Width: 2000 (will auto-adjust)
     - Height: Same as Viewport

3. **Assign to ScrollRect:**
   - Select `EmbossingCardDisplay`
   - ScrollRect component:
     - Content: Drag **Content** here
     - Viewport: Drag **Viewport** here

---

## 🛠️ Step 3: Create Navigation Buttons

1. **Previous Button:**
   - Right-click `EmbossingCardDisplay` → UI → Button
   - Name: `ButtonPrevious`
   - Position: Left side of carousel
   - Text: "<" or use arrow sprite

2. **Next Button:**
   - Right-click `EmbossingCardDisplay` → UI → Button
   - Name: `ButtonNext`
   - Position: Right side of carousel
   - Text: ">" or use arrow sprite

---

## 🛠️ Step 4: Create Card Prefab

1. **Create Card GameObject:**
   - Right-click in Hierarchy → Create Empty
   - Name: `CardPrefab`
   - Add Component → Image (card background)
   - **RectTransform:**
     - Width: 200
     - Height: 300
     - (Adjust to your card size)

2. **Add Card Elements:**
   ```
   CardPrefab
   ├── CardImage (Image - main card art)
   ├── CardName (TextMeshProUGUI - card name)
   ├── CardDescription (TextMeshProUGUI - description)
   └── EmbossingSlots (Container for slot indicators)
   ```

3. **Attach Scripts:**
   - Add `CardDisplay.cs` to CardPrefab root
   - Assign references in Inspector

4. **Save as Prefab:**
   - Drag CardPrefab to Project folder: `Assets/Prefab/EquipmentScreen/`
   - Delete from Hierarchy

---

## 🛠️ Step 5: Configure CardCarouselUI

**Select** `EmbossingCardDisplay`:

### References:
- **Card Container:** Drag **Content** GameObject
- **Card Prefab:** Drag your **CardPrefab** from Project
- **Previous Button:** Drag **ButtonPrevious**
- **Next Button:** Drag **ButtonNext**
- **Scroll Rect:** Drag **EmbossingCardDisplay** itself (or auto-finds)

### Carousel Settings:
- **Card Spacing:** `50` (space between cards)
- **Center Card Scale:** `1.0` (scale of center card)
- **Side Card Scale:** `0.7` (scale of side cards - 70%)
- **Snap Speed:** `0.5` (seconds to snap to card)
- **Swipe Threshold:** `50` (pixels to trigger swipe)

### Visual Effects:
- **Side Card Alpha:** `0.6` (60% opacity for side cards)
- **Center Card Highlight:** White (RGB: 1, 1, 1)
- **Side Card Color:** Gray (RGB: 0.7, 0.7, 0.7)

### Testing:
- **Use Test Cards:** ✓ (checked - uses dummy cards if DeckManager not available)
- **Test Card Count:** `5` (number of test cards to generate)

---

## 🛠️ Step 6: Attach EventTrigger for Drag

**Select** `EmbossingCardDisplay`:
1. **Add Component** → Event Trigger
2. **Add Events:**
   - OnBeginDrag → CardCarouselUI.OnBeginDrag
   - OnDrag → CardCarouselUI.OnDrag
   - OnEndDrag → CardCarouselUI.OnEndDrag

OR:

The `CardCarouselUI` implements `IBeginDragHandler`, `IDragHandler`, `IEndDragHandler` so it should work automatically if attached to the same GameObject as ScrollRect!

---

## 🔗 Step 7: Integrate with DeckManager

The carousel auto-loads from DeckManager on Start().

**Test Mode (Recommended for Setup):**
1. Keep **"Use Test Cards"** ✓ checked in Inspector
2. Carousel will use dummy cards if DeckManager isn't found
3. Perfect for testing the carousel without loading other scenes!
4. Test cards: "Flame Strike", "Ice Barrier", "Thunder Bolt", etc.

**Production Mode:**
1. Uncheck **"Use Test Cards"** when ready
2. DeckManager must be available (singleton pattern)
3. Uses `DeckManager.Instance.GetActiveDeckAsCards()`

**If DeckManager is in a different scene:**
- Make sure DeckManager uses `DontDestroyOnLoad` (it already does - singleton pattern)
- DeckManager.Instance will persist across scene loads
- Equipment screen will access it automatically

---

## 🧪 Testing

1. **Play the game**
2. **Navigate to Embossing tab**
3. **Check:**
   - [ ] Cards appear in carousel
   - [ ] Center card is larger (1.0x scale)
   - [ ] Side cards are smaller (0.7x scale)
   - [ ] Side cards are dimmed (60% alpha)
   - [ ] Click Previous button → moves left
   - [ ] Click Next button → moves right
   - [ ] Drag/swipe → navigates cards
   - [ ] Cards snap to center smoothly
   - [ ] Previous button disabled at first card
   - [ ] Next button disabled at last card

---

## 🎨 Visual Settings Guide

### Scale Settings:
```
Recommended:
- Center: 1.0 (normal size)
- Sides: 0.7-0.8 (70-80% of center)

Dramatic:
- Center: 1.2 (larger)
- Sides: 0.5 (half size)

Subtle:
- Center: 1.0
- Sides: 0.9 (barely smaller)
```

### Spacing:
```
Tight: 20-30px (cards close together)
Normal: 50px (balanced)
Spacious: 100-150px (cards far apart)
```

### Snap Speed:
```
Fast: 0.2-0.3s (snappy)
Normal: 0.4-0.5s (smooth)
Slow: 0.6-0.8s (dramatic)
```

---

## 🚨 Common Issues

### ❌ Cards don't appear
**Problem:** DeckManager not found or deck is empty  
**Solution:** 
- **For Testing:** Check "Use Test Cards" in Inspector (generates 5 dummy cards)
- Check Console for messages:
  - "✓ Loaded X cards from DeckManager" = Working!
  - "⚠️ Using X TEST cards" = Test mode active
  - "No DeckManager found!" = Test mode disabled and no DeckManager
- **For Production:** Verify DeckManager.Instance is accessible
- Ensure active deck has cards

### ❌ Cards don't snap to center
**Problem:** Card positions not calculated correctly  
**Solution:**
- Ensure Content has HorizontalLayoutGroup
- Check that ScrollRect viewport size is correct
- Try clicking Previous/Next buttons (they force snap)

### ❌ Cards all bunched up
**Problem:** Card spacing too small or Content too narrow  
**Solution:**
- Increase Card Spacing setting
- Check that Content RectTransform width adjusts (ContentSizeFitter if needed)

### ❌ Drag doesn't work
**Problem:** EventTrigger not setup or ScrollRect blocking  
**Solution:**
- Make sure CardCarouselUI is on same GameObject as ScrollRect
- ScrollRect should have Movement Type set (Elastic or Clamped)

---

## 💡 Pro Tips

### Tip 1: Smooth Animations
This script uses **Coroutines** for smooth snapping animations:
- No external dependencies needed!
- Uses Ease Out Cubic curve for professional feel
- Adjustable snap speed in Inspector

### Tip 2: Card Prefab Design
Make your card prefab visually appealing:
- Add border/frame
- Use shadows/glow for center card
- Add rarity colors
- Show embossing slots clearly

### Tip 3: Performance
If you have 50+ cards:
- Consider object pooling
- Or virtualized scrolling
- Most decks are 20-40 cards, so should be fine

### Tip 4: Touch/Mobile Support
The swipe detection works on mobile automatically!
- Adjust Swipe Threshold for mobile (higher = easier)
- Test on device for smooth touch scrolling

---

## ✅ Quick Verification

After setup:
- [ ] CardCarouselUI.cs attached to EmbossingCardDisplay
- [ ] ScrollRect configured (horizontal only)
- [ ] Viewport with Mask component
- [ ] Content with HorizontalLayoutGroup
- [ ] Card Prefab assigned
- [ ] Buttons assigned and working
- [ ] Cards load from DeckManager
- [ ] Center card is highlighted
- [ ] Navigation works (buttons + swipe)
- [ ] Smooth snapping animation

---

**Perfect!** Your card carousel is ready! Select cards to apply embossing effects! 🃏✨


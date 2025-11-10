# 🔧 Fix: Card Hover Not Working

## Problem
The hover-over full card display is not appearing when hovering over cards in the deck preview list.

## Debug Steps

### 1. Check Console Messages
Look for these debug messages in the console:

**Expected Messages:**
```
Hover ENTER: [CardName]
Created hover card GameObject: HoverPreview_[CardName]
Showing hover preview for: [CardName]
```

**Missing Messages Indicate:**
- No "Hover ENTER" → EventTrigger not working
- No "Created hover card" → fullCardPrefab not assigned
- No "Showing hover preview" → DeckBuilderCardUI component missing

### 2. Inspector Setup
Ensure these are assigned in `CharacterCreationController`:

**Required:**
- ✅ `Full Card Prefab` → CardPrefab.prefab
- ✅ `Deck Card Prefab` → DeckCardPrefab.prefab

**Optional (Auto-created if missing):**
- ❌ `Card Preview Canvas` → [Auto-created]
- ❌ `Card Hover Preview Parent` → [Auto-created]

### 3. Common Issues & Solutions

#### Issue: No Hover Events Triggered
**Symptoms:** No "Hover ENTER" messages in console
**Causes:**
- DeckCardPrefab missing collider/raycast target
- EventSystem not present in scene
- Cards not receiving pointer events

**Solutions:**
1. Ensure DeckCardPrefab has `GraphicRaycaster` or `Collider`
2. Check that `EventSystem` exists in scene
3. Verify cards are not blocked by other UI elements

#### Issue: Hover Events Trigger But No Preview
**Symptoms:** "Hover ENTER" appears but no card preview
**Causes:**
- `fullCardPrefab` not assigned
- `fullCardPrefab` missing `DeckBuilderCardUI` component
- Canvas sorting order issues

**Solutions:**
1. Assign `Full Card Prefab` in Inspector
2. Ensure CardPrefab has `DeckBuilderCardUI` component
3. Check canvas sorting orders (hover canvas should be highest)

#### Issue: Preview Appears But Wrong Position
**Symptoms:** Card preview appears but in wrong location
**Causes:**
- Canvas positioning issues
- Parent transform not set correctly

**Solutions:**
1. Check `cardHoverPreviewParent` positioning
2. Verify canvas render mode and sorting order
3. Ensure parent is properly centered

### 4. Auto-Fix Features Added

**Auto-Canvas Creation:**
- If `cardHoverPreviewParent` not assigned, creates automatically
- Canvas set to `ScreenSpaceOverlay` with sorting order 100
- Includes `CanvasScaler` and `GraphicRaycaster`

**Enhanced Debug Logging:**
- Shows exactly which step is failing
- Identifies missing components or prefabs
- Tracks hover event flow

### 5. Testing Steps

1. **Open Character Creation scene**
2. **Select any class** (Marauder, Witch, etc.)
3. **Hover over deck preview cards**
4. **Check console** for debug messages
5. **Look for card preview** appearing on screen

### 6. Expected Behavior

**On Hover Enter:**
- Console shows: "Hover ENTER: [CardName]"
- Full card preview appears centered on screen
- Preview shows complete card details

**On Hover Exit:**
- Console shows: "Hover EXIT: [CardName]"
- Card preview disappears
- No lingering preview objects

## Quick Fix Checklist

- [ ] `Full Card Prefab` assigned in Inspector
- [ ] `Deck Card Prefab` assigned in Inspector  
- [ ] `EventSystem` present in scene
- [ ] Console shows hover debug messages
- [ ] Card preview appears on hover
- [ ] Preview disappears on mouse exit

If all steps pass but hover still doesn't work, check for UI element blocking or canvas sorting issues! 🔧












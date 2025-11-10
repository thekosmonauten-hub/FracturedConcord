# Character Display Card Preview Setup Guide

This guide explains how to set up the Card Preview system in the CharacterDisplayUI scene.

---

## 📋 Overview

The CharacterDisplayController displays starter deck cards on the left page of the character book. It uses:
- **StarterDeckManager** to load starter deck definitions
- **CardDisplay** component to render card data
- **CardDataExtended** ScriptableObjects for card definitions

---

## ✅ Setup Checklist

### 1. **StarterDeckManager Setup**

The `StarterDeckManager` must be initialized with the MarauderStarterDeck asset:

1. **Find or Create StarterDeckManager GameObject:**
   - In the scene hierarchy, look for a GameObject named "StarterDeckManager"
   - If it doesn't exist, it will be auto-created as a singleton

2. **Assign Starter Deck Definition:**
   - The `StarterDeckManager` uses `GetDefinitionForClass()` to find starter decks
   - It looks in:
     - `starterDeckDefinitions` list (assigned in Inspector)
     - `Resources/StarterDecks` folder (if `loadDefinitionsFromResources = true`)

3. **Verify MarauderStarterDeck Asset:**
   - Location: `Assets/Resources/Cards/Marauder/MarauderStarterDeck.asset`
   - Must have `characterClass = "Marauder"` (exact match required!)
   - Must have cards assigned in the `cards` list

---

### 2. **CharacterDisplayController Setup**

#### A. **Card Prefab Assignment**

1. **Select CharacterDisplayController GameObject** in the scene
2. **In Inspector → Card Preview:**
   - **Card Prefab**: Assign a prefab that has the `CardDisplay` component
   - **Card Grid Container**: Should be assigned to `LeftPage/StartingDeckContainer` (fileID: 1780562496)

#### B. **Card Prefab Requirements**

The card prefab **MUST** have one of these components:
- `DeckBuilderCardUI` (best - full card display with embossing, description, effects)
- `DeckCardListUI` (✅ supported - simplified row display: name + art only)
- `CardDisplay` (fallback - alternative simplified display)
- `RectTransform` component (for positioning)

**Recommended Prefabs:**

1. **CharacterScreenDeckCard.prefab** ✅ (Customized for character screen)
   - Path: `Assets/Art/CardArt/CharacterScreenDeckCard.prefab`
   - Component: `DeckCardListUI`
   - Shows: Card name, card art, rarity indicator
   - Best for: Clean, simplified card preview (like a deck list view)

2. **CardPrefab.prefab** (Full card display)
   - Path: `Assets/Art/CardArt/CardPrefab.prefab`
   - GUID: `d9493ee9e32bb564abd6b3277d0e023f`
   - Component: `DeckBuilderCardUI`
   - Shows: Card art, name, cost, description, effects, embossing slots, level
   - Best for: Detailed card preview with all information

**Note:** `CharacterScreenDeckCard.prefab` is a variant of `DeckCardPrefab.prefab` with cost icon and quantity removed for a cleaner look on the character screen.

#### C. **Test Mode (Editor Only)**

For testing without going through CharacterCreation scene:

1. **Select CharacterDisplayController GameObject**
2. **In Inspector → Test Mode:**
   - ✅ Check **Test Mode**
   - Set **Test Class** to `"Marauder"` (or any other class)
3. **Play the scene** - it will automatically load the test class

**Note:** Test mode only works in Editor. In builds, it will return to CharacterCreation if no class is selected.

---

### 3. **Component Display Comparison**

Different components show different levels of detail:

**DeckCardListUI** (CharacterScreenDeckCard.prefab):
- ✅ Card name
- ✅ Card art (as background)
- ✅ Rarity indicator
- ❌ No cost display (removed in variant)
- ❌ No quantity display (removed in variant)
- ❌ No description
- ❌ No effects
- Best for: Clean, minimalist card preview

**DeckBuilderCardUI** (CardPrefab.prefab):
- ✅ Card name
- ✅ Card art
- ✅ Mana cost
- ✅ Card description
- ✅ Card effects
- ✅ Embossing slots
- ✅ Card level/XP
- ✅ All card details
- Best for: Full card information display

**CardDisplay** (Custom prefabs):
- Simplified version of DeckBuilderCardUI
- Shows basic card info without embossing
- Requires manual setup of references

---

### 4. **Card Grid Container Setup**

The card grid container is located at:
- **Path:** `CharacterDisplayUI/Background/LeftPage/StartingDeckContainer`
- **RectTransform FileID:** 1780562496

**Current Setup:**
- Container is inside `LeftPage` which is rotated at -16.38° (angled book)
- Cards are positioned using `PositionCardInGrid()` method
- Uses manual positioning (not GridLayoutGroup)

**Positioning Settings:**
- `cardsPerRow = 6` (default)
- `cardSpacing = 10f` (default)

**If cards are not visible:**
1. Check if container is inside rotated parent (`LeftPage`)
2. Cards will inherit rotation from parent
3. Adjust `cardsPerRow` and `cardSpacing` in Inspector if needed

---

## 🧪 Testing

### Test Mode Setup

1. **Enable Test Mode:**
   - Open CharacterDisplayUI scene
   - Select `CharacterDisplayController` GameObject
   - In Inspector, check **Test Mode**
   - Set **Test Class** to `"Marauder"`

2. **Play Scene:**
   - Press Play button
   - Cards should automatically load from MarauderStarterDeck
   - Check Console for debug messages

### Expected Console Output

```
[CharacterDisplayController] Test mode enabled! Loading test class: Marauder
[CharacterDisplayController] Displaying class: Marauder
[CharacterDisplayController] Displaying 6 card types for Marauder
[CharacterDisplayController] ✓ Displayed card: Strike
[CharacterDisplayController] ✓ Displayed card: Strike
... (more cards)
[CharacterDisplayController] ✓ Spawned 18 total cards for Marauder
```

### Troubleshooting

#### ❌ "No starter deck found for Marauder"

**Fix:**
1. Check if `MarauderStarterDeck.asset` exists at `Assets/Resources/Cards/Marauder/MarauderStarterDeck.asset`
2. Verify `characterClass = "Marauder"` in the asset (exact match!)
3. Check if `StarterDeckManager` has `loadDefinitionsFromResources = true`
4. Or add the asset to `StarterDeckManager.starterDeckDefinitions` list in Inspector

#### ❌ "Card prefab has no compatible component"

**Fix:**
1. Check which prefab is assigned
2. Use one of the recommended prefabs:
   - `CharacterScreenDeckCard.prefab` (has DeckCardListUI)
   - `CardPrefab.prefab` (has DeckBuilderCardUI)
3. Or add a compatible component to your prefab

#### ❌ "Card prefab or container not assigned"

**Fix:**
1. Select `CharacterDisplayController` GameObject
2. In Inspector → Card Preview:
   - Assign **Card Prefab** (drag prefab from Project)
   - Assign **Card Grid Container** (should be `StartingDeckContainer`)

#### ❌ Cards not visible or positioned incorrectly

**Fix:**
1. Check if `StartingDeckContainer` is inside `LeftPage` (rotated parent)
2. Cards inherit rotation from parent - this is intentional for the angled book
3. Adjust `cardsPerRow` and `cardSpacing` values in Inspector
4. Check card prefab's RectTransform size (should be reasonable, e.g., 100x150)

---

## 📝 Code Flow

1. **CharacterDisplayController.Start()**
   - Gets `StarterDeckManager` singleton
   - Gets `ClassSelectionData` singleton
   - Loads selected class (or test class if test mode enabled)

2. **DisplayStarterDeck()**
   - Gets `StarterDeckDefinition` from `StarterDeckManager.GetDefinitionForClass()`
   - Iterates through `deckDef.cards` list
   - For each card entry, spawns `count` copies
   - Converts `CardDataExtended` to `Card` using `ToCard()`
   - Sets card data using `CardDisplay.SetCard()`
   - Positions cards using `PositionCardInGrid()`

3. **PositionCardInGrid()**
   - Calculates row/column from card index
   - Sets `RectTransform.anchoredPosition` based on grid layout

---

## 🎯 Current Status

**✅ Working:**
- Test mode for loading Marauder by default
- Error handling and debug logging
- Card spawning and positioning

**⚠️ Needs Setup:**
- Card prefab must have `CardDisplay` component
- StarterDeckManager must have MarauderStarterDeck assigned
- Card grid container must be assigned in Inspector

---

## 📚 Related Files

- `Assets/Scripts/UI/CharacterDisplayController.cs` - Main controller
- `Assets/Scripts/Cards/StarterDeckManager.cs` - Deck loading manager
- `Assets/Scripts/Cards/StarterDeckDefinition.cs` - Deck definition asset
- `Assets/Scripts/UI/EquipmentScreen/UnityUI/CardDisplay.cs` - Card display component
- `Assets/Resources/Cards/Marauder/MarauderStarterDeck.asset` - Marauder starter deck

---

## 🔧 Next Steps

1. **Verify Setup:**
   - Check if card prefab has CardDisplay component
   - Check if StarterDeckManager is initialized
   - Enable test mode and test in Editor

2. **Test Runtime:**
   - Play CharacterDisplayUI scene with test mode enabled
   - Verify cards load correctly
   - Check console for any errors

3. **Integration:**
   - Test full flow: CharacterCreation → CharacterDisplayUI
   - Verify class selection works correctly
   - Test with other classes (Ranger, Witch, etc.)

---

**Last Updated:** 2024-12-19
**Status:** ✅ Ready for Testing


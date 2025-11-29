# Seer Card Generation System - Setup Guide

## Overview

The Seer Card Generation system allows players to generate cards using Orbs of Generation as a base, with optional Orbs of Infusion/Perfection for rarity control and Spirits for element control.

## System Components

### 1. SeerCardGenerator.cs
**Location:** `Assets/Scripts/Seer/SeerCardGenerator.cs`

Core logic for card generation:
- **Rarity Determination:**
  - 20+ Perfection Orbs + 20+ Infusion Orbs = 100% Rare
  - 20+ Infusion Orbs = 100% Magic
  - Otherwise: Weighted random (Common: 100, Magic: 25, Rare: 5)
  
- **Element Determination:**
  - 20+ of a specific Spirit = 100% that element
  - Otherwise: Weighted random based on spirit counts, or fully random if no spirits

- **Spirit to Element Mapping:**
  - FireSpirit → Fire
  - ColdSpirit → Cold
  - LightningSpirit → Lightning
  - PhysicalSpirit → Physical
  - ChaosSpirit → Chaos
  - LifeSpirit, DefenseSpirit, CritSpirit, DivineSpirit → Basic

### 2. SeerCardGenerationUI.cs
**Location:** `Assets/Scripts/Seer/SeerCardGenerationUI.cs`

UI component for ingredient selection and card display. Requires:
- Panel root GameObject
- Close and Scry buttons
- Orb selection UI (Infusion and Perfection orbs with +/- buttons)
- Spirit selection UI (container with spirit item prefabs)
- Output card display area
- Summary text area

### 3. DialogueManager Integration
**Location:** `Assets/Scripts/Dialogue/DialogueManager.cs`

Added `OpenSeerCardGeneration()` method that opens the card generation UI when dialogue action `OpenShop` is called with value `"SeerShop"`.

## Setup Instructions

### Step 1: Create UI GameObject

1. Create a new GameObject in your scene (e.g., "SeerCardGenerationPanel")
2. Add the `SeerCardGenerationUI` component
3. Set up the UI structure:
   - Panel root (main container)
   - Close button
   - Scry button
   - Orb selection area:
     - Infusion orb count text
     - Infusion orb chance text
     - Infusion orb +/- buttons
     - Perfection orb count text
     - Perfection orb chance text
     - Perfection orb +/- buttons
   - Spirit selection container (parent for spirit items)
   - Output card display area
   - Summary text

### Step 2: Create Prefabs

#### Spirit Item Prefab

Create a prefab for individual spirit selection items with:
- Name text (spirit name) - **Auto-populated from CurrencyDatabase**
- Icon image (spirit sprite) - **Auto-populated from CurrencyDatabase**
- Count text (current selection)
- Chance text (element chance percentage)
- Plus button
- Minus button
- `SpiritItemData` component (auto-added by UI script)

**Recommended structure:**
```
SpiritItem
├── IconImage (Image) - Named: "IconImage", "Icon", or "SpriteImage"
├── NameText (TMP_Text) - Named: "NameText", "Name", or "LabelText"
├── CountText (TMP_Text) - Named: "CountText", "Count", or "QuantityText"
├── ChanceText (TMP_Text) - Named: "ChanceText", "Chance", or "PercentText"
├── PlusButton (Button) - Named: "PlusButton", "+Button", or "AddButton"
└── MinusButton (Button) - Named: "MinusButton", "-Button", or "RemoveButton"
```

#### Orb Item Prefab (Optional - Alternative to Legacy UI)

Create a prefab for orb selection items (same structure as Spirit Item):
- Name text (orb name) - **Auto-populated from CurrencyDatabase**
- Icon image (orb sprite) - **Auto-populated from CurrencyDatabase**
- Count text (current selection)
- Chance text (rarity chance percentage)
- Plus button
- Minus button
- `OrbItemData` component (auto-added by UI script)

#### Generation Orb Display Prefab (Optional - Alternative to Legacy Text)

Create a prefab for displaying available Orbs of Generation (read-only, no buttons):
- Name text (orb name) - **Auto-populated from CurrencyDatabase**
- Icon image (orb sprite) - **Auto-populated from CurrencyDatabase**
- Count text (available count) - **Auto-updated**
- `GenerationOrbDisplayData` component (auto-added by UI script)

**Recommended structure:**
```
GenerationOrbDisplay
├── IconImage (Image) - Named: "IconImage", "Icon", or "SpriteImage"
├── NameText (TMP_Text) - Named: "NameText", "Name", or "LabelText"
└── CountText (TMP_Text) - Named: "CountText", "Count", or "QuantityText"
```

**Note:** The UI script supports flexible naming - it will try multiple common name patterns to find components.

### Step 3: Assign References

In the `SeerCardGenerationUI` component inspector:

**Required:**
- Assign `panelRoot` (main panel GameObject)
- Assign `closeButton`
- Assign `scryButton`
- Assign `spiritContainer` (parent for spirit items)
- Assign `spiritItemPrefab` (spirit item prefab)
- Assign output card display elements
- Assign `summaryText`

**Generation Orb Display (Choose One):**
- **Option A - Prefab-based (Recommended):**
  - Assign `generationOrbContainer` (parent for Generation Orb display)
  - Assign `generationOrbDisplayPrefab` (display prefab without +/- buttons)
  - Leave `generationOrbCountText` empty
  
- **Option B - Legacy Text:**
  - Assign `generationOrbCountText` (direct text field)
  - Leave `generationOrbContainer` and `generationOrbDisplayPrefab` empty

**Orb Selection (Choose One):**
- **Option A - Prefab-based (Recommended):**
  - Assign `orbContainer` (parent for orb items)
  - Assign `orbItemPrefab` (orb item prefab)
  - Leave legacy orb fields empty
  
- **Option B - Legacy UI:**
  - Assign individual orb UI elements (count texts, chance texts, buttons)
  - Leave `orbContainer` and `orbItemPrefab` empty

**Note:** The UI automatically populates currency names and sprites from `CurrencyDatabase` when using prefabs. Ensure `CurrencyDatabase` is in the Resources folder.

### Step 4: Test Integration

1. Ensure the Seer dialogue has an `OpenShop` action with value `"SeerShop"`
2. When the dialogue opens the shop, the card generation UI should appear
3. Test with various ingredient combinations:
   - No ingredients (random card)
   - 20 Infusion orbs (guaranteed Magic)
   - 20 Infusion + 20 Perfection orbs (guaranteed Rare)
   - Various spirit combinations

## Usage Examples

### Example 1: Guaranteed Rare Fire Card
- 20 Orb of Infusion
- 20 Orb of Perfection
- 20 Fire Spirit
- Result: Rare Fire card

### Example 2: Partial Magic, Guaranteed Cold
- 5 Orb of Infusion (25% Magic)
- 20 Cold Spirit (100% Cold)
- Result: Common or Magic Cold card (25% chance for Magic)

### Example 3: Random Card
- No ingredients
- Result: Random rarity, random element (weighted: Common 100, Magic 25, Rare 5)

## Currency Consumption

- **Orb of Generation:** Consumed when "Scry" is clicked (1 per generation)
- **Orb of Infusion:** Consumed when "Scry" is clicked (amount selected)
- **Orb of Perfection:** Consumed when "Scry" is clicked (amount selected)
- **Spirits:** Consumed when "Scry" is clicked (amount selected)

**Note:** If currency consumption fails for any ingredient, all consumed currencies are refunded to prevent partial consumption.

## Card Unlocking

Generated cards are automatically unlocked for the current character via `CharacterManager.UnlockCard()`.

## Notes

- Maximum orb/spirit count is 20 (for 100% chance)
- Perfection orbs require 100% Magic chance (20 Infusion orbs) to be usable
- All cards in the database can be generated (no restrictions)
- If no cards match the exact rarity/element combination, the system falls back to:
  1. Any card of the target rarity
  2. Any card in the database


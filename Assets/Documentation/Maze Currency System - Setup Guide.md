# Maze Currency System - Setup Guide

**Created:** Based on user request  
**Purpose:** Maze-specific currency system for vendor and forge activities  
**Location:** `Assets/Scripts/MazeSystem/`

---

## Overview

The Maze Currency System provides 4 tiers of campaign-specific currencies that scale with difficulty:

1. **Mandate Fragments** (Tier 1: Broken Mandate) - Entry-level currency
2. **Shattered Sigils** (Tier 2: Shattered Law) - Mid-tier currency
3. **Contradiction Cores** (Tier 3: Grand Contradiction) - Rare currency
4. **Collapse Motifs** (Tier 4: Absolute Collapse) - Apex currency

Each difficulty tier drops its signature currency, plus smaller amounts of lower-tier currencies.

---

## Currency Tiers

### Tier 1: Broken Mandate → Mandate Fragments
- **Use Cases:**
  - Trade with Maze Vendor for pre-rolled warrants and equipment
  - Open chests
  - Minor shrine activations
  - Basic forge actions
  - Modify equipment at the Maze forge

### Tier 2: Shattered Law → Shattered Sigils
- **Use Cases:**
  - Reroll room rewards
  - Reroll shrine buffs
  - Strengthen a Warrant for the duration of the run
  - Access "side rooms" that aren't normally open
  - Modify equipment at the Maze forge

### Tier 3: Grand Contradiction → Contradiction Cores
- **Use Cases:**
  - Add modifiers to the Maze run (more danger, better reward)
  - Empower the boss for bigger drops
  - Open sealed treasure vaults
  - Activate unique event rooms
  - Modify equipment at the Maze forge

### Tier 4: Absolute Collapse → Collapse Motifs
- **Use Cases:**
  - Unlock ascendancy choices
  - Convert into Warrant resources
  - Create a shortcut within the Maze
  - Summon an optional miniboss
  - Modify equipment at the Maze forge
  - Upgrade the final chest of the run

---

## Setup Instructions

### Step 1: Create Currency Tier Configuration

1. Right-click in Project window → `Create > Dexiled > Maze Currency Tier Config`
2. Configure each tier:
   - **Tier Number**: 1-4
   - **Tier Name**: "Broken Mandate", "Shattered Law", etc.
   - **Primary Currency**: Select the currency type (MandateFragment, ShatteredSigil, etc.)
   - **Primary Currency Base Drop**: Base amount dropped per node/combat (e.g., 5)
   - **Lower Tier Multiplier**: How much lower-tier currency drops (e.g., 0.5 = half)
   - **Tier Color**: Visual theme color
   - **Tier Icon**: Sprite/icon for this tier

3. Save as `MazeCurrencyTierConfig.asset` in `Assets/Resources/`

### Step 2: Create Forge Affix Database

1. Right-click in Project window → `Create > Dexiled > Maze Forge Affix Database`
2. For each tier (1-4), add affixes that can be crafted:
   - **Affix Name**: Display name
   - **Description**: What it does
   - **Affix Data**: The actual `Affix` object to apply
   - **Required Currency Tier**: Which tier currency is needed
   - **Currency Cost**: How much currency to craft
   - **Can Be Prefix/Suffix**: Whether it can be either
   - **Allowed Item Types**: Which items can have this affix
   - **Min Item Level**: Minimum level requirement
   - **Affix Icon**: Visual icon
   - **Affix Color**: Display color

3. Save as `MazeForgeAffixDatabase.asset` in `Assets/Resources/`

### Step 3: Integrate Currency Drops

In your combat/loot completion code, add currency drops:

```csharp
using Dexiled.MazeSystem;

// After combat victory or node completion
int currencyTier = MazeCurrencyDropUtility.GetCurrencyTierFromDifficulty(difficultyName);
// OR
int currencyTier = MazeCurrencyDropUtility.GetCurrencyTierFromFloor(currentFloor, floorsPerTier: 2);

// Load tier config
MazeCurrencyTierConfig tierConfig = Resources.Load<MazeCurrencyTierConfig>("MazeCurrencyTierConfig");

// Generate drops (with difficulty multiplier if applicable)
float multiplier = PlayerPrefs.GetFloat("MazeDifficulty_CurrencyMultiplier", 1.0f);
Dictionary<CurrencyType, int> drops = MazeCurrencyDropUtility.GenerateCurrencyDrops(
    currencyTier, 
    tierConfig, 
    multiplier
);

// Apply drops
MazeCurrencyDropUtility.ApplyCurrencyDrops(drops);
```

### Step 4: Use in Vendor/Forge

**Vendor:**
- Check currency amounts: `MazeCurrencyManager.Instance.GetCurrencyAmount(CurrencyType.MandateFragment)`
- Spend currency: `MazeCurrencyManager.Instance.SpendCurrency(CurrencyType.MandateFragment, cost)`

**Forge:**
- Get available affixes: `MazeForgeAffixDatabase.Instance.GetAffixesForItem(item, maxTier: 4)`
- Filter by prefix/suffix: `GetPrefixAffixesForItem()` or `GetSuffixAffixesForItem()`
- Check currency cost and apply affix to equipment

---

## Currency Drop Scaling

**Example Drop Rates (per combat/node):**

- **Tier 1 Difficulty:**
  - Mandate Fragments: 5 (base)
  
- **Tier 2 Difficulty:**
  - Shattered Sigils: 5 (base)
  - Mandate Fragments: 2-3 (50% of base)
  
- **Tier 3 Difficulty:**
  - Contradiction Cores: 5 (base)
  - Shattered Sigils: 2-3 (50% of base)
  - Mandate Fragments: 1-2 (25% of base)
  
- **Tier 4 Difficulty:**
  - Collapse Motifs: 5 (base)
  - Contradiction Cores: 2-3 (50% of base)
  - Shattered Sigils: 1-2 (25% of base)
  - Mandate Fragments: 1 (12.5% of base)

**Difficulty Multipliers:**
- Currency drops are multiplied by `MazeDifficulty_CurrencyMultiplier` from PlayerPrefs
- Set in `MazeDifficultyConfig` per difficulty level

---

## Forge Affix System

The forge uses a **predetermined set of affixes** that players can select and apply:

1. **Player selects equipment** to modify
2. **System shows available affixes** based on:
   - Item type (Weapon, Armour, Jewellery)
   - Equipment type (if applicable)
   - Item level (must meet minimum)
   - Currency tier available
3. **Player selects affix** (prefix or suffix)
4. **System checks currency cost** and applies if sufficient
5. **Affix is added** to equipment (or replaces existing if at max)

**Affix Restrictions:**
- Each affix defines which item types it can apply to
- Each affix defines if it can be a prefix, suffix, or both
- Each affix has a minimum item level requirement
- Higher tier currencies unlock more powerful affixes

---

## Integration Points

### Maze Run Completion
- Add currency drops when completing nodes/combat
- Use `MazeCurrencyDropUtility` for automatic tier-based drops

### Maze Vendor
- Display currency amounts in UI
- Check currency availability before showing items
- Deduct currency on purchase

### Maze Forge
- Load `MazeForgeAffixDatabase` to show available affixes
- Filter by item type and currency tier
- Apply affixes to equipment using existing affix system

### Difficulty Selection
- Store difficulty tier in PlayerPrefs when starting run
- Use tier to determine currency drops throughout the run

---

## Notes

- Currencies are stored in `MazeCurrencyManager` (persists via PlayerPrefs)
- Currency amounts are character-specific (stored per character name)
- Forge affixes are separate from random loot affixes (predetermined selection)
- Higher tier currencies can be used for lower tier affixes (but not vice versa)
- Currency drops scale with difficulty multipliers set in `MazeDifficultyConfig`

---

## Next Steps

1. Create `MazeCurrencyTierConfig` asset with all 4 tiers configured
2. Create `MazeForgeAffixDatabase` asset with affixes for each tier
3. Integrate currency drops into combat/node completion
4. Implement Vendor UI to display and spend currencies
5. Implement Forge UI to show and apply affixes


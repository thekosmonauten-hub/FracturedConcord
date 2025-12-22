# Card & Embossing Leveling System

## Overview

Cards and their embossings gain experience alongside the player during combat, becoming progressively more powerful through leveling. This creates a progression system where frequently used cards become stronger over time.

**Created:** November 2, 2025  
**Purpose:** Card and embossing progression system for long-term player engagement

---

## System Architecture

```
Player gains XP in combat
    ↓
CharacterManager.AddExperience(xp, shareWithCards: true)
    ↓
    ├─→ Character levels up (existing system)
    └─→ CardExperienceManager.ApplyCombatExperience(xp)
            ↓
            Cards in active deck gain XP (by groupKey)
            ↓
            ├─→ Card levels up (10% bonus at max level)
            └─→ Embossings level up (20% bonus at max level)
```

---

## Card Leveling

### Core Properties

**CardData / Card:**
```csharp
public int cardLevel = 1;           // Current level (1-20)
public int cardExperience = 0;      // Current XP
```

### Level Progression

| Level | Required XP | Cumulative XP | Bonus % |
|-------|-------------|---------------|---------|
| 1 | 0 | 0 | 0% |
| 2 | 100 | 100 | 0.53% |
| 5 | 175 | 644 | 2.1% |
| 10 | 314 | 2,030 | 4.74% |
| 15 | 563 | 5,819 | 7.37% |
| 20 | 1,011 | 16,398 | 10% |

**Formula:**
- Base XP: 100
- Scaling: 1.15x per level (exponential)
- XP for next level = `100 * (1.15^(level-1))`

### Bonus Calculation

**Card Level Bonus:**
```csharp
float GetLevelBonusMultiplier()
{
    // Level 1 = 1.00x (no bonus)
    // Level 20 = 1.10x (+10% bonus)
    return 1.0f + ((cardLevel - 1) * 0.005263f);
}
```

**Examples:**
- Level 1: 1.00x (base damage)
- Level 5: 1.021x (+2.1% damage)
- Level 10: 1.047x (+4.7% damage)
- Level 20: 1.10x (+10% damage)

### Key Methods

```csharp
// Card class methods
bool AddExperience(int amount, bool shareWithEmbossings = true)
float GetLevelBonusMultiplier()
int GetRequiredExperienceForNextLevel()
bool CanLevelUp()
```

---

## Embossing Leveling

### Core Data Structure

**EmbossingInstance:**
```csharp
public string embossingId = "";     // Embossing effect ID
public int level = 1;                // Current level (1-20)
public int experience = 0;           // Current XP
public int slotIndex = 0;            // Which slot (0-4)
```

### Level Progression

| Level | Required XP | Cumulative XP | Bonus % |
|-------|-------------|---------------|---------|
| 1 | 0 | 0 | 0% |
| 2 | 150 | 150 | 1.05% |
| 5 | 263 | 966 | 4.21% |
| 10 | 471 | 3,045 | 9.47% |
| 15 | 845 | 8,728 | 14.74% |
| 20 | 1,517 | 24,597 | 20% |

**Formula:**
- Base XP: 150 (50% more than cards)
- Scaling: 1.15x per level (exponential)
- XP for next level = `150 * (1.15^(level-1))`

### Bonus Calculation

**Embossing Level Bonus:**
```csharp
float GetLevelBonusMultiplier()
{
    // Level 1 = 1.00x (no bonus)
    // Level 20 = 1.20x (+20% bonus)
    return 1.0f + ((level - 1) * 0.010526f);
}
```

**Examples:**
- Level 1: 1.00x (base effect)
- Level 5: 1.042x (+4.2% effect)
- Level 10: 1.095x (+9.5% effect)
- Level 20: 1.20x (+20% effect)

### Key Methods

```csharp
// EmbossingInstance methods
bool AddExperience(int amount)
float GetLevelBonusMultiplier()
int GetRequiredExperienceForNextLevel()
bool CanLevelUp()
float GetLevelProgress()
```

---

## Combined Multipliers

Cards with embossings use **multiplicative stacking**:

```csharp
float GetTotalBonusMultiplier()
{
    float cardBonus = GetLevelBonusMultiplier();      // e.g., 1.10x
    float embossingBonus = GetEmbossingBonusMultiplier(); // e.g., 1.15x
    
    return cardBonus * embossingBonus; // = 1.265x
}
```

### Example Calculations

**Level 20 Card with Level 20 Embossing:**
```
Card: Level 20 → 1.10x (+10%)
Embossing: Level 20 → 1.20x (+20%)

Total: 1.10 × 1.20 = 1.32x (+32% total bonus)
```

**Level 10 Card with 2× Level 10 Embossings:**
```
Card: Level 10 → 1.047x (+4.7%)
Embossing 1: Level 10 → 1.095x (+9.5%)
Embossing 2: Level 10 → 1.095x (+9.5%)
Average Embossing: (1.095 + 1.095) / 2 = 1.095x

Total: 1.047 × 1.095 = 1.146x (+14.6% total bonus)
```

---

## Experience Gain System

### When Do Cards Gain XP?

Cards gain experience from the **same sources** as the player:

1. **Enemy Kills** - Primary XP source
2. **Combat Victory** - Bonus XP at end of combat
3. **Quest Completion** - Shared with player
4. **Manual Rewards** - Any `CharacterManager.AddExperience()` call

### How Much XP Do Cards Get?

**Same as player:** Cards receive the exact same XP amount as the character.

```csharp
// When player gains 50 XP:
CharacterManager.Instance.AddExperience(50, shareWithCards: true);

// Result:
// - Player: +50 XP
// - All unique cards in active deck: +50 XP each
// - All embossings on those cards: +50 XP each
```

### XP Distribution Logic

**Per GroupKey, Not Per Card:**
- 6x Heavy Strike in deck = 1 groupKey
- Heavy Strike gains XP once (not 6 times)
- All 6 copies share the same level

**Example:**
```
Active Deck:
- 6x Heavy Strike (groupKey: "HeavyStrike")
- 4x Brace (groupKey: "Brace")
- 2x Cleave (groupKey: "Cleave")

Player gains 100 XP:
→ "HeavyStrike" group: +100 XP
→ "Brace" group: +100 XP
→ "Cleave" group: +100 XP

= 3 unique card groups gain XP
```

---

## CardExperienceManager

### Singleton Manager

Tracks card experience per groupKey across the character's entire collection.

```csharp
CardExperienceManager.Instance
```

### Key Methods

```csharp
// Add XP to a specific card group
void AddCardExperience(string groupKey, int amount)

// Get card group's level
int GetCardLevel(string groupKey)

// Get card group's current XP
int GetCardExperience(string groupKey)

// Get full experience data
CardExperienceData GetCardExperienceData(string groupKey)

// Apply XP from combat to all cards in active deck
void ApplyCombatExperience(int baseExperience)

// Save/load with character
void SaveToCharacter(Character character)
void LoadFromCharacter(Character character)
```

### Usage Example

```csharp
// Get card level for "HeavyStrike" group
int level = CardExperienceManager.Instance.GetCardLevel("HeavyStrike");

// Manually add XP to a specific card
CardExperienceManager.Instance.AddCardExperience("HeavyStrike", 500);

// Check if card leveled up
CardExperienceData data = CardExperienceManager.Instance.GetCardExperienceData("HeavyStrike");
if (data.CanLevelUp())
{
    Debug.Log($"Heavy Strike is ready to level up! {data.experience}/{data.GetRequiredExperienceForNextLevel()}");
}
```

---

## Integration with Combat

### Combat Flow

```
1. Enemy Defeated
   ↓
2. CardEffectProcessor.TryGrantKillExperience(enemy)
   ↓
3. CharacterManager.AddExperience(xp, shareWithCards: true)
   ↓
4. Character gains XP
   ↓
5. CardExperienceManager.ApplyCombatExperience(xp)
   ↓
6. For each unique card in active deck:
   ├─→ AddCardExperience(groupKey, xp)
   │     ├─→ Card levels up if enough XP
   │     └─→ OnCardLevelUp event fires
   └─→ Embossings on card also gain XP
         └─→ EmbossingInstance.AddExperience(xp)
               └─→ Embossing levels up if enough XP
```

### Combat Victory

```csharp
// In CombatManager.EndCombat()
if (playerWon)
{
    CharacterManager.Instance.AddExperience(50, shareWithCards: true);
    // Character + Cards + Embossings all gain 50 XP
}
```

---

## Persistence

### Save Data Structure

**CharacterDeckData:**
```csharp
[Header("Card Experience System")]
public List<CardExperienceData> cardExperienceData = new List<CardExperienceData>();
```

**CardExperienceData:**
```csharp
public string groupKey;    // Card group identifier
public int level;          // Current level (1-20)
public int experience;     // Current XP
```

**EmbossingInstance (on each Card):**
```csharp
public string embossingId; // Which embossing effect
public int level;          // Embossing level (1-20)
public int experience;     // Embossing XP
public int slotIndex;      // Which slot (0-4)
```

### Save/Load Flow

**On Character Save:**
```csharp
CharacterManager.SaveCharacter()
  ↓
CardExperienceManager.SaveToCharacter(character)
  ↓
character.deckData.cardExperienceData = [all card XP data]
  ↓
CharacterSaveSystem saves to disk
```

**On Character Load:**
```csharp
CharacterManager.LoadCharacter()
  ↓
CardExperienceManager.LoadFromCharacter(character)
  ↓
Loads cardExperienceData into manager
  ↓
Cards synced with saved levels/XP
```

---

## Visual Display

### Card Level Indicator

**Optional TextMeshProUGUI Component:**
- Name: `CardLevel`
- Location: Top-right or top-left corner of card
- Text: `"Lv. 5"`, `"Lv. 20"`, etc.
- Hidden for Level 1 cards (to reduce clutter)

**Auto-Assignment:**
The `CardDisplay` component automatically finds and wires up `CardLevel` text if it exists.

### Embossing Level Indicators

**On Filled Slots:**
Each `SlotXFilled` can display embossing level in a small badge:
- Small text overlay: "Lv. 15"
- Color coded by level tier:
  - 1-5: White
  - 6-10: Green
  - 11-15: Blue
  - 16-20: Gold

**Future Enhancement (Optional):**
```csharp
// Add to SlotXFilled GameObject:
TextMeshProUGUI embossingLevelText;

// In CardDisplay.DisplayEmbossingSlots():
if (isFilled && embossingLevelText != null)
{
    int embossingLevel = card.appliedEmbossings[i-1].level;
    embossingLevelText.text = $"{embossingLevel}";
}
```

---

## Balance Considerations

### Card Level Scaling

**Why +10% max?**
- Cards are core gameplay elements
- Too much power creep makes new cards obsolete
- Encourages deck variety over single card focus
- Balanced for long-term progression

**Leveling Speed:**
- Early levels: Fast (100-175 XP)
- Mid levels: Moderate (300-600 XP)
- Late levels: Slow (600-1000 XP)
- Encourages trying new cards vs. grinding old ones

### Embossing Level Scaling

**Why +20% max?**
- Embossings are rarer and harder to obtain
- Requires investment (applying embossing + leveling)
- Justifies embossing slot scarcity
- Rewards strategic embossing choices

**Leveling Speed:**
- 50% slower than cards (150 base XP vs. 100)
- Makes max-level embossings prestigious
- Encourages thoughtful embossing application

### Combined Power

**Maximum Possible Bonus:**
```
Level 20 Card: +10%
+ 5× Level 20 Embossings: +20% each

Card: 1.10x
Embossings: (1.20 + 1.20 + 1.20 + 1.20 + 1.20) / 5 = 1.20x
Total: 1.10 × 1.20 = 1.32x (+32% total)
```

**Realistic Endgame:**
```
Level 15 Card: +7.37%
+ 2× Level 10 Embossings: +9.47% each

Card: 1.074x
Embossings: (1.095 + 1.095) / 2 = 1.095x
Total: 1.074 × 1.095 = 1.176x (+17.6% total)
```

---

## Implementation Details

### Files Modified

1. **CardData.cs** - Added `cardLevel`, `cardExperience`, level methods
2. **Card.cs** - Added leveling properties and methods
3. **CardDataExtended.cs** - Updated `ToCard()` to include level data
4. **CharacterDeckData.cs** - Added `cardExperienceData` list for persistence
5. **CharacterManager.cs** - Added card XP sharing to `AddExperience()`
6. **CardExperienceManager.cs** (NEW) - Singleton for managing card XP per groupKey
7. **EmbossingInstance.cs** (NEW) - Tracks individual embossing level/XP
8. **CardDisplay.cs** - Added card level display
9. **CombatManager.cs** - Updated to use CharacterManager.AddExperience()

### New Classes

#### CardExperienceManager
- Singleton pattern
- Tracks experience per groupKey
- Persists with character save
- Distributes XP to active deck cards

#### EmbossingInstance
- Replaces `List<string>` for `appliedEmbossings`
- Each embossing has independent level/XP
- Serializable for save system

#### CardExperienceData
- Simple data class for save/load
- Stores groupKey, level, experience
- Provides level calculation methods

---

## Usage Examples

### Check Card Level

```csharp
Card card = GetCard();
int level = card.cardLevel;
float bonus = card.GetLevelBonusMultiplier();

Debug.Log($"{card.cardName} is level {level} (+{(bonus - 1.0f) * 100:F1}% bonus)");
```

### Apply Damage with Level Bonus

```csharp
Card card = GetCard();
float baseDamage = card.baseDamage;
float leveledDamage = baseDamage * card.GetLevelBonusMultiplier();
float totalDamage = leveledDamage * card.GetEmbossingBonusMultiplier();

// Or use combined:
float totalDamage = baseDamage * card.GetTotalBonusMultiplier();
```

### Add Experience

```csharp
// Automatically handled in combat:
CharacterManager.Instance.AddExperience(100, shareWithCards: true);

// Manually add to specific card group:
CardExperienceManager.Instance.AddCardExperience("HeavyStrike", 500);
```

### Check Embossing Levels

```csharp
Card card = GetCard();
foreach (EmbossingInstance embossing in card.appliedEmbossings)
{
    Debug.Log($"Embossing: {embossing.embossingId}");
    Debug.Log($"  Level: {embossing.level}");
    Debug.Log($"  Bonus: {embossing.GetLevelBonusMultiplier():P1}");
    Debug.Log($"  Progress: {embossing.GetLevelProgress():P0} to next level");
}
```

---

## GroupKey Integration

### Why GroupKey Matters

All cards with the same groupKey **share experience and level**:

```
Deck Contents:
- 6× "Heavy Strike" (groupKey: "HeavyStrike")
- All 6 cards are level 10
- All 6 cards have 1,234 XP

When any one is played in combat:
→ "HeavyStrike" group gains XP
→ All 6 cards level up together
```

### Benefits

✅ **Consistency** - All copies have same power  
✅ **Simplicity** - No tracking individual instances  
✅ **Balance** - Can't power-level one copy  
✅ **Storage** - One entry per groupKey vs. per card  

---

## Events

### Card Level Up Event

```csharp
CardExperienceManager.Instance.OnCardLevelUp += (groupKey, newLevel) =>
{
    Debug.Log($"Card group {groupKey} reached level {newLevel}!");
    
    // Show popup, play sound, etc.
};
```

### Card Gain Experience Event

```csharp
CardExperienceManager.Instance.OnCardGainExperience += (groupKey, amount) =>
{
    Debug.Log($"Card group {groupKey} gained {amount} XP");
};
```

---

## Testing

### Test Case 1: Card Gains XP

```csharp
// Setup
Card card = new Card();
card.cardName = "Heavy Strike";
card.cardLevel = 1;
card.cardExperience = 0;

// Action
bool leveledUp = card.AddExperience(100);

// Expected
Assert.IsTrue(leveledUp);
Assert.AreEqual(2, card.cardLevel);
Assert.AreEqual(0, card.cardExperience); // Exact XP for level 2
```

### Test Case 2: Embossing Gains XP

```csharp
// Setup
EmbossingInstance embossing = new EmbossingInstance("fire_damage", 0);
embossing.level = 1;
embossing.experience = 0;

// Action
bool leveledUp = embossing.AddExperience(150);

// Expected
Assert.IsTrue(leveledUp);
Assert.AreEqual(2, embossing.level);
Assert.AreEqual(0, embossing.experience);
```

### Test Case 3: Combat XP Distribution

```csharp
// Setup deck with unique cards
DeckPreset deck = CreateTestDeck();
// Contains: HeavyStrike, Brace, Cleave

DeckManager.Instance.SetActiveDeck(deck);

// Action
CharacterManager.Instance.AddExperience(100, shareWithCards: true);

// Expected
CardExperienceData strikeData = CardExperienceManager.Instance.GetCardExperienceData("HeavyStrike");
CardExperienceData braceData = CardExperienceManager.Instance.GetCardExperienceData("Brace");

Assert.AreEqual(100, strikeData.experience);
Assert.AreEqual(100, braceData.experience);
```

---

## Best Practices

### Card Design

**Set Default Levels:**
- Most cards: Level 1 (default)
- Starter cards: Can be level 1 (players level them naturally)
- Reward cards: Can start at level 5-10 (feel powerful immediately)

**Slot Count by Rarity:**
```
Common: 1 slot, Level 1
Magic: 2 slots, Level 1
Rare: 3 slots, Level 1-3
Unique: 4-5 slots, Level 5-10
```

### Embossing Design

**XP Requirements:**
- Higher than cards (150 vs. 100 base)
- Makes embossing investment meaningful
- Encourages keeping embossings long-term

**Effect Power:**
- Base effect should be useful
- +20% at max level should feel significant
- Don't make level 1 too weak (still usable)

### Balance Guidelines

**Avoid:**
- ❌ Making low-level cards useless
- ❌ Required grinding for progression
- ❌ XP gains that feel too slow

**Encourage:**
- ✅ Natural progression through normal play
- ✅ Experimenting with different cards
- ✅ Long-term card investment

---

## Future Enhancements

### Potential Features

**1. XP Boost Items:**
```csharp
// Temporary XP boost consumable
float xpMultiplier = 1.5f; // +50% XP for 1 hour
CharacterManager.Instance.AddExperience(100 * xpMultiplier);
```

**2. Prestige System:**
```csharp
// Reset card to level 1 for bonus prestige ranks
void PrestigeCard(string groupKey)
{
    // Reset level to 1, keep embossings
    // Gain prestige rank (cosmetic badge)
    // Unlock special embossing slots
}
```

**3. XP Sharing Options:**
```csharp
// In character settings:
public XPShareMode xpShareMode = XPShareMode.ActiveDeck; // or AllCards, PlayedCardsOnly
```

**4. Daily Bonuses:**
```csharp
// First combat of the day: 2x XP for cards
bool isFirstCombatToday = CheckDailyReset();
int xpMultiplier = isFirstCombatToday ? 2 : 1;
```

---

## Troubleshooting

### Cards Not Gaining XP

**Check:**
1. `CharacterManager.AddExperience()` called with `shareWithCards: true`
2. `CardExperienceManager.Instance` exists and initialized
3. Active deck has cards
4. Cards have valid groupKeys

**Debug:**
```csharp
// Enable detailed logging
CardExperienceManager.Instance.OnCardGainExperience += (groupKey, amount) =>
{
    Debug.Log($"[Debug] {groupKey} gained {amount} XP");
};
```

### XP Not Persisting

**Check:**
1. `CharacterManager.SaveCharacter()` called after XP gain
2. `cardExperienceData` in `CharacterDeckData` is serializable
3. Character save file contains experience data

**Test Save/Load:**
```csharp
// Add XP
CardExperienceManager.Instance.AddCardExperience("TestCard", 1000);

// Save
CharacterManager.Instance.SaveCharacter();

// Reload
CharacterManager.Instance.LoadCharacter(characterName);

// Check
int level = CardExperienceManager.Instance.GetCardLevel("TestCard");
// Should be > 1 if XP was enough to level up
```

### Level Bonuses Not Applying

**Check:**
1. Damage calculation uses `card.GetTotalBonusMultiplier()`
2. Card level is > 1 (no bonus at level 1)
3. Embossings exist and have level > 1

**Verify Calculation:**
```csharp
Card card = GetCard();
float cardBonus = card.GetLevelBonusMultiplier();
float embossingBonus = card.GetEmbossingBonusMultiplier();
float total = card.GetTotalBonusMultiplier();

Debug.Log($"Card Bonus: {cardBonus:F3}x");
Debug.Log($"Embossing Bonus: {embossingBonus:F3}x");
Debug.Log($"Total: {total:F3}x");
```

---

## Summary

✅ **Card leveling system** - Max +10% at level 20  
✅ **Embossing leveling system** - Max +20% at level 20  
✅ **GroupKey integration** - Shared XP per card group  
✅ **Combat integration** - Auto XP gain from combat  
✅ **Persistence** - Saves with character data  
✅ **Visual display** - Level indicators on cards  
✅ **Multiplicative bonuses** - Card × Embossing stacking  

**The system is fully implemented and ready for use!**

---

## Related Documentation

- `CARD_GROUPKEY_SYSTEM.md` - How cards are grouped
- `EMBOSSING_SLOTS_SYSTEM.md` - Embossing slot visuals
- `CardExperienceManager.cs` - Experience manager implementation
- `EmbossingInstance.cs` - Embossing instance data


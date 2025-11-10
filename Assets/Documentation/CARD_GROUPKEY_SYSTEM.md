# Card GroupKey System

## Overview

The GroupKey system allows cards to be grouped together for embossing, upgrades, and other card management features. Cards with the same `groupKey` are treated as the same card type, even if they have different names or variants.

**Created:** November 2, 2025  
**Purpose:** Enable card grouping for embossing system and future card variant features

---

## Implementation

### Core Property

```csharp
public string groupKey = "";
```

Added to:
- ✅ `CardData.cs` (base class)
- ✅ `CardDataExtended.cs` (inherits from CardData)
- ✅ `Card.cs` (runtime class)

### Helper Method

All card classes now have a `GetGroupKey()` method:

```csharp
public string GetGroupKey()
{
    return string.IsNullOrEmpty(groupKey) ? cardName : groupKey;
}
```

**Behavior:** Returns the `groupKey` if set, otherwise falls back to `cardName`.

---

## Usage

### Default Behavior (No GroupKey Set)

When `groupKey` is empty (default), cards are grouped by their `cardName`:

```
Card: "Heavy Strike"
GroupKey: "" (empty)
→ GetGroupKey() returns: "Heavy Strike"
```

**Result:** Each unique card name is its own group.

### Explicit GroupKey (Card Variants)

You can set the same `groupKey` for different card variants:

```
Card: "Heavy Strike"
GroupKey: "HeavyStrike"

Card: "Heavy Strike+"
GroupKey: "HeavyStrike"

Card: "Heavy Strike++ (Foil)"
GroupKey: "HeavyStrike"
```

**Result:** All three cards are treated as the same card type for embossing/upgrades.

---

## Use Cases

### 1. **Embossing System**

When you emboss a card in the EquipmentScreen, all cards with the same `groupKey` receive the embossing:

```csharp
string groupKey = cardCarousel.GetSelectedCardGroupKey();
int affectedCopies = cardCarousel.GetSelectedCardCount();

ApplyEmbossing(groupKey, embossingEffect);
// Affects all cards in the deck with this groupKey
```

### 2. **Card Upgrades**

Base cards and upgraded versions can share a groupKey:

```
"Flame Strike" → groupKey: "FlameStrike"
"Flame Strike+" → groupKey: "FlameStrike"
"Flame Strike++" → groupKey: "FlameStrike"
```

**Benefit:** Embossings, unlocks, and collection tracking persist across upgrades.

### 3. **Premium Variants**

Premium/foil versions can be grouped with base cards:

```
"Thunder Bolt" → groupKey: "ThunderBolt"
"Thunder Bolt (Animated)" → groupKey: "ThunderBolt"
"Thunder Bolt (Foil)" → groupKey: "ThunderBolt"
```

### 4. **Set Variants**

Different art versions of the same card:

```
"Brace (Core Set)" → groupKey: "Brace"
"Brace (Expansion 1)" → groupKey: "Brace"
"Brace (Promo)" → groupKey: "Brace"
```

---

## CardCarouselUI Integration

The `CardCarouselUI` now uses groupKey for card display:

### Key Methods

```csharp
// Get unique cards grouped by groupKey
List<Card> GetUniqueCards(List<Card> allCards)

// Get count of cards with a specific groupKey
int GetCardCount(string groupKey)

// Get count by Card object
int GetCardCountByCard(Card card)

// Get groupKey of currently selected card
string GetSelectedCardGroupKey()
```

### Display Behavior

**Before GroupKey:**
- 6x "Heavy Strike" → Shows 6 separate cards

**After GroupKey:**
- 6x "Heavy Strike" → Shows 1 card (x6 in deck)
- Embossing affects all 6 copies

---

## Migration Strategy

### For Existing Cards

**No action required!** 

- Existing cards with empty `groupKey` will automatically use `cardName` as the groupKey
- Everything works exactly as before

### For New Cards

**Option 1: Use default (recommended for most cards)**
```
cardName: "Heavy Strike"
groupKey: "" (leave empty)
→ Groups by card name
```

**Option 2: Set explicit groupKey (for variants)**
```
cardName: "Heavy Strike+"
groupKey: "HeavyStrike"
→ Groups with base "Heavy Strike"
```

---

## Best Practices

### Naming Conventions

For explicit groupKeys, use consistent naming:

| Card Name | Suggested GroupKey |
|-----------|-------------------|
| "Heavy Strike" | "HeavyStrike" |
| "Heavy Strike+" | "HeavyStrike" |
| "Ice Shard" | "IceShard" |
| "Ice Shard (Upgraded)" | "IceShard" |

**Format:** PascalCase, no spaces, descriptive

### When to Use Explicit GroupKeys

✅ **DO use explicit groupKeys for:**
- Upgraded card variants
- Premium/foil versions
- Alternative art versions
- Promotional variants

❌ **DON'T use explicit groupKeys for:**
- Completely different cards (even if similar)
- Cards with different mechanics
- Cards that should be tracked separately

### Example: Similar But Different Cards

```
"Fire Strike" → groupKey: "" (default)
"Flame Strike" → groupKey: "" (default)
"Inferno Strike" → groupKey: "" (default)
```

**Reasoning:** These are different cards, not variants. Keep them separate.

### Example: Same Card, Different Variants

```
"Fire Strike" → groupKey: "FireStrike"
"Fire Strike+" → groupKey: "FireStrike"
"Fire Strike (Master)" → groupKey: "FireStrike"
```

**Reasoning:** These are variants of the same card. Group them together.

---

## Future Enhancements

### Potential Uses

1. **Collection Tracking**
   - Track which card groups the player has unlocked
   - Show "collected variants" (base, upgraded, foil)

2. **Deck Building**
   - Filter cards by groupKey
   - Show all variants of a card type

3. **Statistics**
   - Track usage stats per card group
   - Aggregate performance across variants

4. **Trading System**
   - Trade any variant for another in the same group
   - Maintain embossings/upgrades during trades

---

## Example Data

### CardDataExtended Asset (Inspector)

```yaml
Card Name: Heavy Strike
Group Key: HeavyStrike
Card Type: Attack
Play Cost: 1
Description: Deal 8 physical damage...
```

### JSON Card Format

```json
{
  "cardName": "Heavy Strike+",
  "groupKey": "HeavyStrike",
  "description": "Deal 12 physical damage...",
  "cardType": "Attack",
  "manaCost": 1,
  "baseDamage": 12.0
}
```

---

## Implementation Checklist

- [x] Add `groupKey` property to CardData
- [x] Add `groupKey` property to Card
- [x] Add `GetGroupKey()` helper method to both classes
- [x] Update CardDataExtended.ToCard() to include groupKey
- [x] Update CardCarouselUI to use groupKey for grouping
- [x] Add GetSelectedCardGroupKey() method to CardCarouselUI
- [x] Update OnCardSelected() to display groupKey in logs
- [x] Create documentation

---

## Testing

### Test Cases

**1. Default Behavior (Empty GroupKey)**
```
Input: Card with cardName="Brace", groupKey=""
Expected: GetGroupKey() returns "Brace"
Result: ✓ Pass
```

**2. Explicit GroupKey**
```
Input: Card with cardName="Brace+", groupKey="Brace"
Expected: GetGroupKey() returns "Brace"
Result: ✓ Pass
```

**3. Carousel Grouping**
```
Input: Deck with 6x "Heavy Strike" (groupKey="HeavyStrike")
Expected: Carousel shows 1 card, count = 6
Result: ✓ Pass
```

**4. Mixed Variants**
```
Input: Deck with:
  - 4x "Brace" (groupKey="Brace")
  - 2x "Brace+" (groupKey="Brace")
Expected: Carousel shows 1 card, count = 6
Result: ✓ Pass
```

---

## Conclusion

The GroupKey system is now fully implemented and ready for use! It provides a flexible foundation for:
- ✅ Card embossing system
- ✅ Card upgrades and variants
- ✅ Future card management features

**All existing cards work without modification**, and new cards can optionally use groupKey for advanced grouping.

---

## References

- `CardData.cs` - Base card data with groupKey
- `CardDataExtended.cs` - Extended card data (inherits groupKey)
- `Card.cs` - Runtime card class with groupKey
- `CardCarouselUI.cs` - Carousel implementation using groupKey
- `EMBOSSING_SYSTEM.md` (TODO) - Future embossing system documentation


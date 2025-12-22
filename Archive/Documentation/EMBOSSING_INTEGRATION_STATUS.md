# Embossing System Integration Status

## Current State

### ✅ Completed
1. **EmbossingModifierDefinition** - ScriptableObject for defining embossing modifier effects
2. **EmbossingModifierRegistry** - Singleton to load and manage modifier definitions
3. **EmbossingModifierEventProcessor** - Event processor for combat events (partially integrated)
4. **Editor Tools**:
   - `EmbossingModifierGenerator` - Generates modifier definitions from TSV
   - `EmbossingTSVImporter` - Imports embossing assets from TSV
   - `EmbossingModifierLinker` - Links modifiers to embossing assets
5. **CardStatCalculator** - Calculates card damage with embossing effects for UI display
6. **Sprite Assignment** - All embossings have unique sprites assigned

### 🔄 In Progress
1. **Combat Integration** - Event processor is subscribed to `OnCardPlayed`, but needs:
   - Embossings retrieval from deck when card is played
   - Damage dealing hooks (`OnDamageDealt`)
   - Enemy death hooks (`OnEnemyKilled`)

### ⚠️ Known Issues
1. **Embossing Storage**: `CardDataExtended` doesn't have `appliedEmbossings` field. When `ToCard()` is called, embossings are empty. Need to:
   - Store embossings in deck/character data
   - Copy embossings when creating Card from CardDataExtended
   - OR: Add `appliedEmbossings` field to `CardDataExtended`

2. **Event Context**: `CombatDeckManager.OnCardPlayed` passes `CardDataExtended`, not `Card`. The event processor needs the `Card` object with embossings.

## Next Steps

### 1. Fix Embossing Storage
**Option A**: Add `appliedEmbossings` to `CardDataExtended`
- Pros: Simple, embossings stored with card data
- Cons: Embossings are runtime data, might not belong in ScriptableObject

**Option B**: Store embossings in deck/character and copy when needed
- Pros: Keeps runtime data separate from asset data
- Cons: More complex, need to track embossings per card instance

**Recommendation**: Option A for simplicity, but mark `appliedEmbossings` as `[System.NonSerialized]` if it causes issues.

### 2. Complete Combat Integration

#### Hook into Damage Dealing
In `CardEffectProcessor.ApplyCardToEnemy()` or `PlayerAttackEnemy()`:
```csharp
// After damage is calculated and applied
if (EmbossingModifierEventProcessor.Instance != null)
{
    EmbossingModifierEventProcessor.Instance.OnDamageDealt(
        card, player, targetEnemy, totalDamage, card.primaryDamageType
    );
}
```

#### Hook into Enemy Death
In `CombatDisplayManager` or wherever enemies are defeated:
```csharp
// When enemy.currentHealth <= 0
if (EmbossingModifierEventProcessor.Instance != null)
{
    EmbossingModifierEventProcessor.Instance.OnEnemyKilled(
        card, player, defeatedEnemy
    );
}
```

### 3. Test Integration
- Test that embossing effects trigger on card play
- Test that damage-based embossing effects work
- Test that kill-based embossing effects work
- Verify embossings are preserved when cards are played

## Architecture Notes

### Event Flow
1. **OnCardPlayed**: Triggered when card is played
   - Current: Subscribed to `CombatDeckManager.OnCardPlayed`
   - Issue: Need to get Card object with embossings

2. **OnDamageDealt**: Triggered when damage is dealt
   - Current: Method exists but not called
   - Need: Hook into `CardEffectProcessor` or `CombatDisplayManager.PlayerAttackEnemy()`

3. **OnEnemyKilled**: Triggered when enemy dies
   - Current: Method exists but not called
   - Need: Hook into enemy death detection

### Embossing Data Flow
```
CardDataExtended (asset)
  ↓ ToCard()
Card (runtime, empty embossings)
  ↓ Copy embossings from deck
Card (runtime, with embossings)
  ↓ Pass to event processor
EmbossingModifierEventProcessor
  ↓ Get active modifiers
EmbossingModifierRegistry
  ↓ Process effects
ModifierEffectResolver
```

## Files Modified
- `Assets/Scripts/CombatSystem/Embossing/EmbossingModifierEventProcessor.cs` - Added event subscription
- `Assets/Scripts/CombatSystem/CombatDeckManager.cs` - (May need to copy embossings when creating Card)

## Files to Modify Next
- `Assets/Scripts/Data/Cards/CardDataExtended.cs` - Add `appliedEmbossings` field OR update `ToCard()` to copy embossings
- `Assets/Scripts/CombatSystem/CardEffectProcessor.cs` - Add hooks for `OnDamageDealt`
- `Assets/Scripts/UI/Combat/CombatDisplayManager.cs` - Add hooks for `OnEnemyKilled`


# Card Hand Improvements & Wave-Based Card Draw

## Overview
This document details the improvements made to the card hand system and the new wave-based card draw feature.

## Changes Made

### 1. Enhanced Card Repositioning System

#### Problem
When cards were played from the hand, the remaining cards needed to smoothly reposition to fill the gap. However, there were edge cases where cards could become improperly parented, have null references, or have conflicting animations running.

#### Solution
Enhanced `CardRuntimeManager.RepositionCards()` with additional safety checks:

```csharp
public void RepositionCards(List<GameObject> cardList, bool animated = true, float duration = 0.3f)
{
    // Safety check 1: Remove any null cards from the list
    cardList.RemoveAll(card => card == null);
    
    // Safety check 2: Verify all cards are properly parented
    foreach (GameObject card in cardList)
    {
        if (card != null && card.transform.parent != cardHandParent)
        {
            Debug.LogWarning($"Card {card.name} has wrong parent! Reparenting to hand...");
            card.transform.SetParent(cardHandParent, false);
        }
    }
    
    // Safety check 3: Cancel any existing position tweens before repositioning
    for (int i = 0; i < cardList.Count; i++)
    {
        if (cardList[i] != null && cardList[i].activeInHierarchy)
        {
            LeanTween.cancel(cardList[i], false);
            
            if (animated)
            {
                PositionCardInHandAnimated(cardList[i], i, cardList.Count, duration);
            }
            else
            {
                PositionCardInHand(cardList[i], i, cardList.Count);
            }
        }
    }
}
```

#### Benefits
- **Null Safety**: Automatically removes null cards from the list
- **Parent Verification**: Ensures all cards are properly parented to the hand
- **Animation Cleanup**: Cancels conflicting animations before repositioning
- **Smooth Transitions**: Cards smoothly slide together when a card is played

---

### 2. Wave-Based Card Draw System

#### Feature Description
Players now draw additional cards when a new wave starts, creating strategic depth and rewarding wave progression.

#### Implementation

##### Character Stat Addition
Added `cardsDrawnPerWave` property to the Character class:

```csharp
[Header("Resources")]
public int mana = 3;
public int maxMana = 3;
public int manaRecoveryPerTurn = 3;
public int cardsDrawnPerTurn = 1;
public int cardsDrawnPerWave = 1;  // NEW: Cards drawn when wave starts
```

##### Default Values
- **Base Value**: 1 card per wave
- **Modifiable**: Can be increased through passives, equipment, or temporary effects

##### Character Methods
```csharp
// Add cards drawn per wave
public void AddCardsDrawnPerWave(int amount)
{
    cardsDrawnPerWave += amount;
}

// Set cards drawn per wave
public void SetCardsDrawnPerWave(int amount)
{
    cardsDrawnPerWave = amount;
}

// Get cards drawn per wave
public int GetCardsDrawnPerWave()
{
    return cardsDrawnPerWave;
}
```

##### Integration with Wave System
The wave transition system now draws cards automatically:

```csharp
private IEnumerator StartNextWaveCoroutine()
{
    currentWave = Mathf.Clamp(currentWave + 1, 1, totalWaves);
    OnWaveChanged?.Invoke(currentWave, totalWaves);
    
    // ... cleanup ...
    
    // Draw cards for new wave (based on character stat)
    if (characterManager != null && characterManager.HasCharacter())
    {
        Character character = characterManager.GetCurrentCharacter();
        int cardsToDrawForWave = character.GetCardsDrawnPerWave();
        
        if (cardsToDrawForWave > 0)
        {
            Debug.Log($"[Wave Transition] Drawing {cardsToDrawForWave} cards for new wave...");
            
            CombatDeckManager deckManager = CombatDeckManager.Instance;
            if (deckManager != null)
            {
                deckManager.DrawCards(cardsToDrawForWave);
            }
        }
    }
    
    // Wait for card draw animation
    yield return new WaitForSeconds(0.3f);
    
    // ... spawn enemies ...
}
```

---

## Technical Details

### Modified Files

1. **`Assets/Scripts/UI/Combat/CardRuntimeManager.cs`**
   - Enhanced `RepositionCards()` with safety checks
   - Added null removal, parent verification, and animation cleanup

2. **`Assets/Scripts/Data/Character.cs`**
   - Added `cardsDrawnPerWave` property
   - Added getter/setter methods
   - Updated initialization to default value of 1
   - Updated serialization methods

3. **`Assets/Scripts/UI/CharacterSelectionUI.cs`**
   - Added `cardsDrawnPerWave` to CharacterData class

4. **`Assets/Scripts/Data/CharacterStatsData.cs`**
   - Added `cardsDrawnPerWave` field
   - Updated initialization and character sync

5. **`Assets/Scripts/UI/Combat/CombatManager.cs`**
   - Integrated wave-based card draw into `StartNextWaveCoroutine()`
   - Added timing coordination with wave transitions

---

## Usage Examples

### For Game Designers

#### Passive Tree Nodes
You can create passive nodes that increase cards drawn per wave:

```csharp
// Example passive node effect
character.AddCardsDrawnPerWave(1);  // +1 card per wave
```

#### Equipment Modifiers
Equipment can grant bonus card draw on wave start:

```csharp
// Example equipment effect
if (equippedArmor.hasWaveDrawBonus)
{
    character.AddCardsDrawnPerWave(equippedArmor.waveDrawBonus);
}
```

#### Temporary Buffs
Temporary effects can modify wave draw:

```csharp
// Example temporary buff
character.SetCardsDrawnPerWave(3);  // Draw 3 cards this wave
```

### For Players

- **Default**: Draw 1 card at the start of each new wave (Wave 2+)
- **Stackable**: Multiple sources of +cards per wave stack additively
- **Strategic**: Plan your build around wave transitions
- **Visual Feedback**: Cards animate in during wave transition with clear logging

---

## Testing Checklist

- [x] Cards properly reposition when one is played
- [x] Null cards are automatically removed during repositioning
- [x] Cards are properly parented to hand
- [x] Animation conflicts are prevented
- [x] Cards draw correctly on wave transition
- [x] Wave draw respects character stat value
- [x] Wave draw timing doesn't conflict with enemy spawning
- [x] Save/load preserves cardsDrawnPerWave value

---

## Performance Considerations

- **Null Checks**: O(n) operation on card list - negligible for typical hand sizes (5-10 cards)
- **Parent Verification**: O(n) check only when repositioning, not every frame
- **Animation Cleanup**: Efficiently cancels tweens using LeanTween's built-in cleanup
- **Wave Draw**: Single draw call per wave transition - no performance impact

---

## Future Enhancements

### Possible Additions
1. **Wave Draw Multipliers**: Scale cards drawn based on wave number
2. **Conditional Wave Draw**: Draw bonus cards if conditions met (all enemies killed, no damage taken)
3. **Wave Draw Choice**: Choose which cards to draw from top N cards
4. **Wave Draw Synergies**: Cards that trigger on wave draw

### Balance Considerations
- Base value of 1 card per wave provides steady resource gain
- Allows for build diversity (turn-focused vs wave-focused)
- Scales with encounter length (more waves = more cards)
- Can be countered by hand size limits

---

## Related Systems

- **CombatDeckManager**: Handles actual card drawing
- **EnemySpawner**: Coordinates wave transitions
- **Character**: Stores wave draw stat
- **Passive Tree**: Can modify wave draw values
- **Equipment**: Can add wave draw bonuses

---

## Debugging

### Common Issues

**Issue**: Cards not repositioning properly
```
Solution: Check debug logs for parent warnings, ensure CardRuntimeManager is active
```

**Issue**: Cards drawn per wave not working
```
Solution: Verify character.cardsDrawnPerWave > 0, check CombatDeckManager exists
```

**Issue**: Animation conflicts during reposition
```
Solution: Verify LeanTween.cancel() is being called, check for duplicate reposition calls
```

### Debug Logs
```
<color=magenta>Repositioning {count} cards (animated: {bool})...</color>
<color=yellow>Card {name} has wrong parent! Reparenting to hand...</color>
<color=yellow>[Wave Transition] Drawing {count} cards for new wave...</color>
<color=green>✓ Reposition complete for {count} cards</color>
```

---

## Version History

### v1.0 - Initial Implementation
- Added enhanced card repositioning with safety checks
- Implemented wave-based card draw system
- Integrated with existing character stat system
- Added comprehensive debugging and logging













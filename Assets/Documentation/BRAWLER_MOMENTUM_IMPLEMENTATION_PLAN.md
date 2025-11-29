# Brawler Momentum Implementation Plan

## ✅ **Completed**

1. **MomentumManager Class** - Created `Assets/Scripts/CombatSystem/MomentumManager.cs`
   - Tracks momentum resource (0-10, configurable cap)
   - Methods: `GainMomentum()`, `SpendMomentum()`, `SpendAllMomentum()`, `HasMomentum()`
   - Threshold tracking (3+, 5+, 7+, 8+, 10+)
   - Turn decay support

2. **Character Integration** - Added to `Character.cs`
   - `Momentum` property (similar to `Channeling`)
   - Reset in `ResetRuntimeState()`
   - Decay in `StartPlayerTurn()` in `CombatManager`

3. **MomentumEffectParser Class** - Created `Assets/Scripts/CombatSystem/MomentumEffectParser.cs`
   - Parses "Gain X Momentum" from descriptions
   - Parses "Spend X Momentum" / "Spend all Momentum"
   - Parses threshold checks ("If you have X+ Momentum")
   - **NEW**: Parses "per Momentum spent" damage patterns
   - **NEW**: Parses attribute scaling in momentum damage (e.g., "+dex/6")
   - **NEW**: `CalculatePerMomentumDamage()` - calculates total damage with attribute scaling

4. **CardEffectProcessor Integration** - Updated `CardEffectProcessor.cs`
   - Momentum-based damage scaling in `ApplyAoECard()`
   - Momentum-based damage scaling in `ApplyAttackCard()`
   - Handles "Deal X damage (+STAT/Y) per Momentum spent" patterns
   - Example: "Deal 3 damage (+dex/6) per Momentum spent" with 5 momentum = (3 + dex/6) × 5

5. **Momentum Effects UI Display** - Added to `CardDataExtended.cs` and `CombatCardAdapter.cs`
   - New field: `momentumEffectDescription` in `CardDataExtended`
   - New method: `GetDynamicMomentumDescription()` - supports placeholders like `{momentum}`, `{momentumGain}`, `{momentumSpent}`
   - Integrated into `AdditionalEffectsText` display (shown alongside combo descriptions)
   - Automatically combines combo and momentum descriptions with line breaks

## 📋 **Remaining Tasks**

### **Task 1: Momentum Effect Parser** (High Priority)

Create a system to parse momentum effects from card descriptions. Cards use patterns like:
- `"Gain X Momentum"`
- `"Spend X Momentum"` / `"Spend all Momentum"`
- `"If you have X+ Momentum: [effect]"`

**Location**: `CardEffectProcessor.cs` or new `MomentumEffectParser.cs`

**Patterns to handle**:
```csharp
// Gain momentum
"Gain 1 Momentum"
"Gain 2 Momentum"
"Gain 3 Momentum"

// Spend momentum
"Spend all Momentum"
"Spend up to 2 Momentum"

// Threshold checks
"If you have 3+ Momentum:"
"If you have 5+ Momentum:"
"If you have 6+ Momentum:"
"If you have 7+ Momentum:"
"If you have 8+ Momentum:"
"If you have 10 Momentum:" (exact, not +)
```

### **Task 2: CardEffectProcessor Integration** (High Priority)

Add momentum processing to `CardEffectProcessor.ApplyCardToEnemy()` and related methods:

1. **Before card effect**: Parse and apply momentum gain/spend
2. **During card effect**: Check momentum thresholds and modify behavior
3. **Examples**:
   - `Rapid Strike`: "If you have 3+ Momentum: This card costs 0"
   - `Momentum Spike`: "Spend all Momentum. Deal 3 damage per Momentum spent"
   - `Steadfast Guard`: "If you have Momentum, gain +1 Guard per Momentum"

### **Task 3: Dynamic Descriptions** (Medium Priority)

Update `CardDataExtended.GetDynamicDescription()` to replace momentum placeholders:
- `{momentum}` - Current momentum value
- `{momentumGain}` - Momentum gained from this card
- `{momentumSpent}` - Momentum spent by this card

### **Task 4: Brawler Card Updates** (Medium Priority)

Update all Brawler cards to use dynamic descriptions and ensure momentum effects work:

**Cards to update**:
1. **Rapid Strike** - Gain 1 Momentum, cost reduction at 3+, AoE at 5+
2. **One-Two Punch** - Gain 1 Momentum, 2 random targets at 3+, 2 Momentum at 5+
3. **Momentum Spike** - Spend all Momentum, damage per Momentum, Bleed at 5+, Energy at 7+
4. **Steadfast Guard** - +1 Guard per Momentum, temp Dex at 4+
5. **Guardbreaker** - Spend up to 2 Momentum, 4 Guard per Momentum, +2 Guard at 4+, temp Str at 3+ spent
6. **Devastating Blow** - Gain 2 Momentum, cost reduction at 6+, AoE at 8+, Energy at 10
7. **Berserker's Fury** - Gain 3 Momentum, +1 Momentum per gain, double damage at 5+
8. **Adrenaline Rush** - Gain 2 Momentum, Draw 2 + temp Str at 8+, AoE damage at 10

### **Task 5: UI Display** (Low Priority)

Add momentum display to `PlayerCombatDisplay`:
- Momentum bar (similar to stagger bar)
- Current momentum value
- Threshold indicators (3, 5, 7, 8, 10)

## 🎯 **Implementation Strategy**

### **Phase 1: Core Momentum Effects** (2-3 hours)
1. Create `MomentumEffectParser` helper class
2. Integrate into `CardEffectProcessor`
3. Test with one card (e.g., Rapid Strike)

### **Phase 2: Threshold Effects** (2-3 hours)
1. Add threshold checking logic
2. Implement cost reduction, AoE conversion, etc.
3. Test with Momentum Spike and Devastating Blow

### **Phase 3: Dynamic Descriptions** (1-2 hours)
1. Add momentum placeholders to `GetDynamicDescription()`
2. Update all Brawler card descriptions
3. Test dynamic values

### **Phase 4: UI & Polish** (1-2 hours)
1. Add momentum bar to UI
2. Visual feedback for threshold crossings
3. Final testing

## 📝 **Code Examples**

### **Momentum Effect Parser**

```csharp
public static class MomentumEffectParser
{
    public static int ParseGainMomentum(string description)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            description, 
            @"Gain\s+(\d+)\s+Momentum", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        if (match.Success && int.TryParse(match.Groups[1].Value, out int amount))
            return amount;
        return 0;
    }
    
    public static bool ShouldSpendAllMomentum(string description)
    {
        return description.Contains("Spend all Momentum", System.StringComparison.OrdinalIgnoreCase);
    }
    
    public static int ParseSpendMomentum(string description)
    {
        if (ShouldSpendAllMomentum(description))
            return -1; // Special value for "all"
            
        var match = System.Text.RegularExpressions.Regex.Match(
            description, 
            @"Spend\s+(?:up\s+to\s+)?(\d+)\s+Momentum", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        if (match.Success && int.TryParse(match.Groups[1].Value, out int amount))
            return amount;
        return 0;
    }
    
    public static List<int> ParseThresholds(string description)
    {
        var thresholds = new List<int>();
        var matches = System.Text.RegularExpressions.Regex.Matches(
            description, 
            @"If\s+you\s+have\s+(\d+)(?:\+|)\s+Momentum", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            if (int.TryParse(match.Groups[1].Value, out int threshold))
                thresholds.Add(threshold);
        }
        return thresholds;
    }
}
```

### **CardEffectProcessor Integration**

```csharp
private void ProcessMomentumEffects(Card card, Character player)
{
    if (card == null || player == null) return;
    
    string desc = card.description ?? "";
    
    // Gain momentum
    int gainAmount = MomentumEffectParser.ParseGainMomentum(desc);
    if (gainAmount > 0)
    {
        player.Momentum.GainMomentum(gainAmount);
    }
    
    // Spend momentum
    int spendAmount = MomentumEffectParser.ParseSpendMomentum(desc);
    if (spendAmount > 0)
    {
        if (spendAmount == -1) // Spend all
            player.Momentum.SpendAllMomentum();
        else
            player.Momentum.SpendMomentum(spendAmount);
    }
    
    // Check thresholds and apply effects
    var thresholds = MomentumEffectParser.ParseThresholds(desc);
    foreach (int threshold in thresholds)
    {
        if (player.Momentum.HasMomentum(threshold))
        {
            ApplyMomentumThresholdEffect(card, player, threshold, desc);
        }
    }
}
```

## 🚀 **Next Steps**

1. Implement `MomentumEffectParser` helper class
2. Integrate momentum processing into `CardEffectProcessor`
3. Test with Rapid Strike card
4. Update all Brawler cards
5. Add UI display


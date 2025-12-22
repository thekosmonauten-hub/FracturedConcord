# Embossing System - Enum Compatibility Note

## ✅ Fixed: Duplicate Enum Definitions

### Issue
`EmbossingEffect.cs` originally defined duplicate enums that conflicted with existing game systems:
- `StatusEffectType` (duplicate of `CombatSystem/StatusEffect.cs`)
- `ElementType` (conflicts with existing `DamageType`)

### Solution
Updated `EmbossingEffect.cs` to use existing game enums:

**Uses `DamageType` (from `Combat/DamageTypes.cs`):**
```csharp
public DamageType elementType = DamageType.Physical;
```

**Available values:**
- `Physical` - Physical damage
- `Fire` - Fire damage
- `Cold` - Cold damage  
- `Lightning` - Lightning damage
- `Chaos` - Chaos damage

**Uses `StatusEffectType` (from `CombatSystem/StatusEffect.cs`):**
```csharp
public StatusEffectType statusEffect = StatusEffectType.Poison;
```

**Available values (debuffs):**
- `Poison` - Damage over time
- `Burn` - Fire damage over time (use instead of "Ignite")
- `ChaosDot` - Chaos damage over time
- `Freeze` - Skip next turn
- `Stun` - Skip next turn
- `Vulnerable` - Take increased damage
- `Weak` - Deal reduced damage
- `Frail` - Take increased damage from attacks
- `Slow` - Reduced action speed
- `Crumble` - Stores physical damage (consumed by Shout)

**Available values (buffs):**
- `Strength` - Increased physical damage
- `Dexterity` - Increased accuracy/evasion
- `Intelligence` - Increased spell damage
- `Shield` - Damage absorption
- `Bolster` - Damage reduction per stack
- `Block` - Chance to block
- `Evasion` - Chance to avoid
- `Regeneration` - Health over time
- `ManaRegen` - Mana over time
- And more...

---

## 📝 Embossing Effect Mapping

### Status Effects in Embossings

When creating embossings that apply status effects, use these mappings:

| Design Document | Game Enum | Description |
|----------------|-----------|-------------|
| Bleed | `Poison` | DoT effect (closest match) |
| Ignite | `Burn` | Fire DoT |
| Poison | `Poison` | Direct match |
| Shock | `Stun` | Closest match (disable enemy) |
| Freeze | `Freeze` | Direct match |
| Chill | `Slow` | Closest match (slow enemy) |
| Weak | `Weak` | Direct match |
| Vulnerable | `Vulnerable` | Direct match |

### Damage Types in Embossings

When creating conversion embossings:

| Conversion | From | To | Implementation |
|-----------|------|-----|----------------|
| of the Inferno | Physical → Fire | `elementType = DamageType.Fire` |
| of the Avalanche | Physical → Cold | `elementType = DamageType.Cold` |
| of Storms | Physical → Lightning | `elementType = DamageType.Lightning` |
| of Corruption | Elemental → Chaos | `elementType = DamageType.Chaos` |

---

## ✅ Result

All embossing system files now compile correctly and integrate with existing game systems:

- ✅ No duplicate enum definitions
- ✅ Uses game's existing `DamageType` enum
- ✅ Uses game's existing `StatusEffectType` enum
- ✅ Full compatibility with combat system
- ✅ All sample embossings work correctly

---

## 🔧 For Developers

When creating new embossings:

**For damage conversions:**
```csharp
embossing.elementType = DamageType.Fire; // or Cold, Lightning, Chaos
embossing.effectType = EmbossingEffectType.PhysicalToFireConversion;
embossing.effectValue = 0.5f; // 50% conversion
```

**For status effects:**
```csharp
embossing.statusEffect = StatusEffectType.Burn;
embossing.effectType = EmbossingEffectType.ApplyBurn; // Custom type
embossing.statusEffectChance = 0.15f; // 15% chance
```

**No status effect needed:**
```csharp
embossing.statusEffectChance = 0f; // Set chance to 0
// statusEffect value will be ignored if chance is 0
```

---

## 📚 Related Files

**Enum Definitions:**
- `Assets/Scripts/Combat/DamageTypes.cs` - DamageType enum
- `Assets/Scripts/CombatSystem/StatusEffect.cs` - StatusEffectType enum

**Embossing System:**
- `Assets/Scripts/Data/Embossing/EmbossingEffect.cs` - Uses both enums
- `Assets/Scripts/Managers/EmbossingDatabase.cs` - Processes embossings

**Documentation:**
- `EMBOSSING_SYSTEM.md` - Full system documentation
- `EMBOSSING_SETUP_GUIDE.md` - Setup instructions

---

**System is fully compatible with existing game systems!** ✅


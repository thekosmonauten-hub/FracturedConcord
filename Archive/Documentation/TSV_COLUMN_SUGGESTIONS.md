# Suggested TSV Columns for Reliance Auras

## Current Columns:
1. **Category** - Aura category (Fire, Cold, etc.)
2. **Aura Name** - Display name
3. **Description** - Flavor text description
4. **Effect Level 1** - Effect description at level 1
5. **Effect Level 20** - Effect description at level 20
6. **Reliance** - Reliance cost to activate

## Recommended Additional Columns:

### **Priority 1: Critical for Functionality**

#### **7. Modifier IDs** (Comma-separated)
- **Format**: `"ModifierID1, ModifierID2, ModifierID3"`
- **Purpose**: Links the aura to its `RelianceAuraModifierDefinition` assets
- **Example**: `"Pyreheart_FireDamage, Pyreheart_SpellBonus"`
- **Why**: Without this, auras won't have any actual game effects. The modifier definitions contain the actual logic.

### **Priority 2: Important for Gameplay**

#### **8. Embossing Slots** (Integer, 0-5)
- **Format**: `0`, `1`, `2`, `3`, `4`, or `5`
- **Purpose**: Number of embossing slots the aura can have
- **Default**: `1` (currently hardcoded)
- **Why**: Some auras might have 0 slots (can't be embossed) or more slots for customization

#### **9. Required Level** (Integer)
- **Format**: `1`, `5`, `10`, etc.
- **Purpose**: Minimum character level to unlock this aura
- **Default**: `1` (currently hardcoded)
- **Why**: Progression gating - some auras should be unlocked later

### **Priority 3: Nice to Have**

#### **10. Unlock Requirement** (String, optional)
- **Format**: `"Quest: Defeat the Fire Lord"` or `"Challenge: Complete Act 2"`
- **Purpose**: Additional unlock condition beyond level
- **Default**: Empty string
- **Why**: Allows for quest-based or challenge-based unlocks

#### **11. Icon Name** (String, optional)
- **Format**: `"Aura_Pyreheart"` or `"Icons/PyreheartIcon"`
- **Purpose**: Name/path to load icon sprite from Resources
- **Default**: Auto-generated from aura name or category
- **Why**: Allows specific icon assignment per aura

#### **12. Theme Color** (RGB, optional)
- **Format**: `"255,100,50"` or `"1.0,0.4,0.2"` (0-255 or 0-1 range)
- **Purpose**: Custom theme color for the aura
- **Default**: Auto-generated from category
- **Why**: Allows custom colors for special auras (like Law auras might have unique colors)

## Recommended TSV Format:

```
Category	Aura Name	Description	Effect Level 1	Effect Level 20	Reliance	Modifier IDs	Embossing Slots	Required Level	Unlock Requirement	Icon Name	Theme Color
Fire	Pyreheart	Casts an aura...	90 flat Fire...	160 flat Fire...	100	Pyreheart_FireDamage	1	1		Aura_Pyreheart	255,100,50
```

## Implementation Notes:

1. **Modifier IDs** should be comma-separated and will be split into the `modifierIds` list
2. **Embossing Slots** and **Required Level** should default to 1 if empty
3. **Unlock Requirement** and **Icon Name** can be empty (optional)
4. **Theme Color** can be in either format (0-255 or 0-1), importer should handle both
5. All new columns should be optional (allow empty values with sensible defaults)

## Benefits:

- **Modifier IDs**: Critical - auras will actually work in-game
- **Embossing Slots**: Allows customization per aura
- **Required Level**: Enables progression gating
- **Unlock Requirement**: Adds quest/challenge-based unlocks
- **Icon Name**: Better visual organization
- **Theme Color**: Visual customization for special auras


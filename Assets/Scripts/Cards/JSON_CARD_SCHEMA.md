# Card JSON Schema Documentation

## Overview
This document defines the JSON schema for importing cards into the Dexiled Unity project. The schema supports both the complex `Card` class and the simpler `CardData` ScriptableObject.

---

## Schema Structure

### **Single Card Format**
```json
{
  "cardName": "Heavy Strike",
  "description": "Deal {damage} physical damage. Scales with melee weapon and Strength.",
  "cardType": "Attack",
  "manaCost": 1,
  "baseDamage": 8.0,
  "baseGuard": 0.0,
  "primaryDamageType": "Physical",
  
  "weaponScaling": {
    "scalesWithMeleeWeapon": true,
    "scalesWithProjectileWeapon": false,
    "scalesWithSpellWeapon": false
  },
  
  "aoe": {
    "isAoE": false,
    "aoeTargets": 1
  },
  
  "requirements": {
    "requiredStrength": 25,
    "requiredDexterity": 0,
    "requiredIntelligence": 0,
    "requiredLevel": 1,
    "requiredWeaponTypes": []
  },
  
  "tags": ["Attack", "Physical", "Combo"],
  
  "additionalDamageTypes": [],
  
  "damageScaling": {
    "strengthScaling": 0.5,
    "dexterityScaling": 0.0,
    "intelligenceScaling": 0.0
  },
  
  "guardScaling": {
    "strengthScaling": 0.0,
    "dexterityScaling": 0.0,
    "intelligenceScaling": 0.0
  },
  
  "effects": [],
  
  "rarity": "Common",
  "element": "Physical",
  "category": "Attack"
}
```

### **Multiple Cards Format (Recommended for Class Starter Decks)**
```json
{
  "deckName": "Marauder Starter Deck",
  "characterClass": "Marauder",
  "description": "A brutal deck focused on physical damage and close combat",
  "cards": [
    {
      "cardName": "Heavy Strike",
      "count": 6,
      "data": {
        // ... card data as above
      }
    },
    {
      "cardName": "Brace",
      "count": 4,
      "data": {
        // ... card data
      }
    }
  ]
}
```

---

## Field Definitions

### **Core Properties**

| Field | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| `cardName` | string | ✅ | - | Display name of the card |
| `description` | string | ✅ | - | Card description (supports placeholders: {damage}, {guard}, {aoeTargets}) |
| `cardType` | enum | ✅ | - | Card type: "Attack", "Guard", "Skill", "Power", "Aura" |
| `manaCost` | int | ✅ | 1 | Mana cost to play the card |
| `baseDamage` | float | ❌ | 0.0 | Base damage value (for Attack cards) |
| `baseGuard` | float | ❌ | 0.0 | Base guard value (for Guard cards) |
| `primaryDamageType` | enum | ❌ | "Physical" | Damage type: "Physical", "Fire", "Cold", "Lightning", "Chaos", "None" |

### **Weapon Scaling**

```json
"weaponScaling": {
  "scalesWithMeleeWeapon": false,     // Adds melee weapon damage
  "scalesWithProjectileWeapon": false, // Adds projectile weapon damage
  "scalesWithSpellWeapon": false      // Adds spell weapon damage
}
```

### **Area of Effect**

```json
"aoe": {
  "isAoE": false,      // Whether card hits multiple enemies
  "aoeTargets": 1      // Number of enemies hit (3 = all enemies)
}
```

### **Requirements**

```json
"requirements": {
  "requiredStrength": 0,        // Minimum STR to use card
  "requiredDexterity": 0,       // Minimum DEX to use card
  "requiredIntelligence": 0,    // Minimum INT to use card
  "requiredLevel": 1,           // Minimum character level
  "requiredWeaponTypes": []     // Required weapon types: ["Melee", "Projectile", "Spell"]
}
```

### **Attribute Scaling**

```json
"damageScaling": {
  "strengthScaling": 0.0,       // Damage per STR (0.5 = +50% of STR)
  "dexterityScaling": 0.0,      // Damage per DEX
  "intelligenceScaling": 0.0    // Damage per INT
},
"guardScaling": {
  "strengthScaling": 0.0,       // Guard per STR
  "dexterityScaling": 0.0,      // Guard per DEX
  "intelligenceScaling": 0.0    // Guard per INT
}
```

### **Card Effects**

```json
"effects": [
  {
    "effectType": "Damage",      // Effect type enum (see below)
    "effectName": "Bonus Fire",
    "description": "Adds fire damage",
    "value": 10.0,               // Effect value
    "duration": 1,               // Duration in turns (for temporary effects)
    "damageType": "Fire",        // Damage type for damage effects
    "targetsSelf": true,         // Whether effect targets caster
    "targetsEnemy": false,       // Whether effect targets enemy
    "targetsAllEnemies": false,  // Whether effect targets all enemies
    "targetsAll": false,         // Whether effect targets all characters
    "condition": ""              // Condition string (e.g., "ifDiscarded")
  }
]
```

**Effect Types:**
- `Damage` - Deal damage
- `Heal` - Restore health
- `Guard` - Add guard/block
- `Draw` - Draw cards
- `Discard` - Discard cards
- `ApplyStatus` - Apply status effect
- `RemoveStatus` - Remove status effect
- `GainMana` - Restore mana
- `GainReliance` - Add reliance resource
- `TemporaryStatBoost` - Temporary stat increase
- `PermanentStatBoost` - Permanent stat increase

### **Metadata**

```json
"tags": ["Attack", "Physical", "Combo"],  // Card tags for filtering
"additionalDamageTypes": ["Fire"],        // Secondary damage types
"rarity": "Common",                       // Common, Magic, Rare, Unique
"element": "Physical",                    // Basic, Fire, Cold, Lightning, Physical, Chaos
"category": "Attack"                      // Attack, Skill, Power, Guard
```

---

## Validation Rules

### **Required Combinations**

1. **Attack Cards** MUST have:
   - `baseDamage > 0`
   - `primaryDamageType` set
   - `damageScaling` OR `weaponScaling` (recommended)

2. **Guard Cards** MUST have:
   - `baseGuard > 0`
   - `guardScaling` (recommended)

3. **Skill/Power/Aura Cards**:
   - MUST have `effects` array with at least one effect
   - OR have clear description of what they do

### **Scaling Best Practices**

- **Strength Scaling**: Typically 0.25 - 0.5 for damage, 0.25 for guard
- **Dexterity Scaling**: Typically 0.33 - 0.66 for projectile damage
- **Intelligence Scaling**: Typically 0.5 - 1.0 for spell damage
- **Weapon Scaling**: Use for weapon-based attacks
- **Attribute Scaling**: Use for stat-based abilities

### **Mana Cost Guidelines**

- **Basic Cards**: 0-1 mana
- **Standard Cards**: 1-2 mana
- **Powerful Cards**: 2-4 mana
- **Ultimate Cards**: 4+ mana

---

## Complete Example

### **Simple Attack Card**
```json
{
  "cardName": "Heavy Strike",
  "description": "Deal {damage} physical damage.",
  "cardType": "Attack",
  "manaCost": 1,
  "baseDamage": 8.0,
  "primaryDamageType": "Physical",
  "weaponScaling": {
    "scalesWithMeleeWeapon": true
  },
  "requirements": {
    "requiredStrength": 25
  },
  "damageScaling": {
    "strengthScaling": 0.5
  },
  "tags": ["Attack", "Physical", "Melee"],
  "rarity": "Common",
  "element": "Physical",
  "category": "Attack"
}
```

### **Complex Spell Card**
```json
{
  "cardName": "Fireball",
  "description": "Deal {damage} fire damage to all enemies. Applies Ignite.",
  "cardType": "Attack",
  "manaCost": 3,
  "baseDamage": 15.0,
  "primaryDamageType": "Fire",
  "weaponScaling": {
    "scalesWithSpellWeapon": true
  },
  "aoe": {
    "isAoE": true,
    "aoeTargets": 3
  },
  "requirements": {
    "requiredIntelligence": 40
  },
  "damageScaling": {
    "intelligenceScaling": 0.75
  },
  "effects": [
    {
      "effectType": "ApplyStatus",
      "effectName": "Ignite",
      "description": "Target is ignited",
      "value": 2.0,
      "duration": 3,
      "damageType": "Fire",
      "targetsSelf": false,
      "targetsEnemy": true,
      "targetsAllEnemies": true
    }
  ],
  "tags": ["Attack", "Fire", "Spell", "AoE"],
  "rarity": "Magic",
  "element": "Fire",
  "category": "Attack"
}
```

### **Guard Card with Draw**
```json
{
  "cardName": "Endure",
  "description": "Gain {guard} Guard. Draw a card.",
  "cardType": "Guard",
  "manaCost": 1,
  "baseGuard": 8.0,
  "requirements": {
    "requiredStrength": 22
  },
  "guardScaling": {
    "strengthScaling": 0.25
  },
  "effects": [
    {
      "effectType": "Draw",
      "effectName": "Draw Card",
      "description": "Draw 1 card",
      "value": 1.0,
      "targetsSelf": true
    }
  ],
  "tags": ["Guard", "Strength", "Draw"],
  "rarity": "Common",
  "element": "Basic",
  "category": "Guard"
}
```

---

## Placeholder System

The `description` field supports dynamic placeholders that are calculated at runtime:

| Placeholder | Replacement | Example |
|-------------|-------------|---------|
| `{damage}` | Total calculated damage | "Deal **12** physical damage" |
| `{baseDamage}` | Base damage value only | "Base damage: **8**" |
| `{guard}` | Total calculated guard | "Gain **10** Guard" |
| `{baseGuard}` | Base guard value only | "Base guard: **8**" |
| `{aoeTargets}` | Number of AoE targets | "Hit **3** enemies" |
| `{strBonus}` | Strength scaling bonus | "+**6** from Strength" |
| `{dexBonus}` | Dexterity scaling bonus | "+**8** from Dexterity" |
| `{intBonus}` | Intelligence scaling bonus | "+**10** from Intelligence" |
| `{weaponDamage}` | Weapon damage added | "+**15** weapon damage" |

---

## Import Notes

- All numerical fields use **period (.)** as decimal separator
- Boolean fields accept: `true`/`false` (lowercase)
- Enum fields are **case-insensitive** but prefer **PascalCase**
- Missing optional fields use default values
- Arrays can be empty: `[]`
- Use `null` for explicitly no value, or omit the field entirely

---

## Version History

- **v1.0** - Initial schema based on Card.cs and CardData.cs architecture


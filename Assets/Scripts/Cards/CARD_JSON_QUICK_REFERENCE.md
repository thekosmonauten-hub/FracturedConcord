# Card JSON Quick Reference

## 🚀 Quick Start

### 1. Create Your JSON File
Place your JSON files in: `Assets/Resources/CardJSON/`

### 2. Open the Importer
Unity Menu: `Tools > Cards > Import Cards from JSON`

### 3. Import Your Cards
- Browse to your JSON file
- Select target database (optional)
- Click "Import Cards"

---

## 📋 Minimal Card Examples

### Basic Attack Card
```json
{
  "cardName": "Strike",
  "description": "Deal {damage} physical damage.",
  "cardType": "Attack",
  "manaCost": 1,
  "baseDamage": 6.0,
  "primaryDamageType": "Physical",
  "weaponScaling": {
    "scalesWithMeleeWeapon": true
  },
  "requirements": {
    "requiredStrength": 10
  },
  "damageScaling": {
    "strengthScaling": 0.3
  },
  "tags": ["Attack", "Physical"],
  "rarity": "Common",
  "element": "Physical",
  "category": "Attack"
}
```

### Basic Guard Card
```json
{
  "cardName": "Block",
  "description": "Gain {guard} Guard.",
  "cardType": "Guard",
  "manaCost": 1,
  "baseGuard": 5.0,
  "guardScaling": {
    "strengthScaling": 0.2
  },
  "tags": ["Guard", "Defense"],
  "rarity": "Common",
  "element": "Basic",
  "category": "Guard"
}
```

### Spell Card
```json
{
  "cardName": "Lightning Bolt",
  "description": "Deal {damage} lightning damage.",
  "cardType": "Attack",
  "manaCost": 2,
  "baseDamage": 10.0,
  "primaryDamageType": "Lightning",
  "weaponScaling": {
    "scalesWithSpellWeapon": true
  },
  "requirements": {
    "requiredIntelligence": 35
  },
  "damageScaling": {
    "intelligenceScaling": 0.7
  },
  "tags": ["Attack", "Lightning", "Spell"],
  "rarity": "Magic",
  "element": "Lightning",
  "category": "Attack"
}
```

---

## 🎯 Common Patterns

### Strength-Based Melee Attack
```json
{
  "cardType": "Attack",
  "baseDamage": 8.0,
  "primaryDamageType": "Physical",
  "weaponScaling": { "scalesWithMeleeWeapon": true },
  "requirements": { "requiredStrength": 25 },
  "damageScaling": { "strengthScaling": 0.5 }
}
```

### Dexterity-Based Projectile Attack
```json
{
  "cardType": "Attack",
  "baseDamage": 7.0,
  "primaryDamageType": "Physical",
  "weaponScaling": { "scalesWithProjectileWeapon": true },
  "requirements": { "requiredDexterity": 30 },
  "damageScaling": { "dexterityScaling": 0.6 }
}
```

### Intelligence-Based Spell
```json
{
  "cardType": "Attack",
  "baseDamage": 12.0,
  "primaryDamageType": "Fire",
  "weaponScaling": { "scalesWithSpellWeapon": true },
  "requirements": { "requiredIntelligence": 40 },
  "damageScaling": { "intelligenceScaling": 0.75 }
}
```

### AoE Attack
```json
{
  "cardType": "Attack",
  "baseDamage": 5.0,
  "isAoE": true,
  "aoe": {
    "isAoE": true,
    "aoeTargets": 3
  }
}
```

### Card with Effect
```json
{
  "cardType": "Guard",
  "baseGuard": 8.0,
  "effects": [
    {
      "effectType": "Draw",
      "effectName": "Draw Card",
      "description": "Draw 1 card",
      "value": 1.0,
      "targetsSelf": true
    }
  ]
}
```

---

## 🔧 Field Defaults

If you omit optional fields, these defaults are used:

| Field | Default Value |
|-------|---------------|
| `baseDamage` | 0.0 |
| `baseGuard` | 0.0 |
| `primaryDamageType` | "Physical" |
| `manaCost` | 1 |
| `isAoE` | false |
| `aoeTargets` | 1 |
| `requiredLevel` | 1 |
| `requiredStrength` | 0 |
| `requiredDexterity` | 0 |
| `requiredIntelligence` | 0 |
| `strengthScaling` | 0.0 |
| `dexterityScaling` | 0.0 |
| `intelligenceScaling` | 0.0 |
| `scalesWithMeleeWeapon` | false |
| `scalesWithProjectileWeapon` | false |
| `scalesWithSpellWeapon` | false |

---

## 📝 Placeholder Guide

Use these in `description` field:

| Placeholder | Shows |
|-------------|-------|
| `{damage}` | Calculated total damage |
| `{guard}` | Calculated total guard |
| `{aoeTargets}` | Number of enemies hit |
| `{strBonus}` | STR scaling bonus |
| `{dexBonus}` | DEX scaling bonus |
| `{intBonus}` | INT scaling bonus |

Example:
```json
"description": "Deal {damage} damage ({baseDamage} + {strBonus} STR)"
```

---

## 🎨 Enum Values

### CardType
- `Attack` - Damage dealing cards
- `Guard` - Defense cards
- `Skill` - Utility cards
- `Power` - Buff cards
- `Aura` - Persistent effect cards

### DamageType
- `Physical` - Physical damage
- `Fire` - Fire damage
- `Cold` - Cold damage
- `Lightning` - Lightning damage
- `Chaos` - Chaos damage
- `None` - No damage

### Rarity
- `Common` - Common cards
- `Magic` - Magic cards
- `Rare` - Rare cards
- `Unique` - Unique cards

### Element
- `Basic` - No element
- `Fire` - Fire element
- `Cold` - Cold element
- `Lightning` - Lightning element
- `Physical` - Physical element
- `Chaos` - Chaos element

### EffectType
- `Damage` - Deal damage
- `Heal` - Restore health
- `Guard` - Add guard/block
- `Draw` - Draw cards
- `Discard` - Discard cards
- `ApplyStatus` - Apply status effect
- `RemoveStatus` - Remove status effect
- `GainMana` - Restore mana
- `GainReliance` - Add reliance
- `TemporaryStatBoost` - Temporary buff
- `PermanentStatBoost` - Permanent buff

---

## ⚖️ Balancing Guidelines

### Damage Values
- **Basic Attack (1 mana)**: 6-8 base damage
- **Strong Attack (1 mana)**: 8-10 base damage
- **AoE Attack (2 mana)**: 4-7 base damage per target
- **Spell (2-3 mana)**: 10-15 base damage

### Guard Values
- **Basic Guard (1 mana)**: 5-7 base guard
- **Strong Guard (1 mana)**: 8-10 base guard
- **Utility Guard (2 mana)**: 10-12 base guard + effect

### Scaling Ratios
- **Strength**: 0.25-0.5 per point
- **Dexterity**: 0.33-0.66 per point
- **Intelligence**: 0.5-1.0 per point

### Attribute Requirements
- **Early Game**: 15-30
- **Mid Game**: 30-50
- **Late Game**: 50-80

---

## 🚨 Common Errors

### ❌ Missing Required Fields
```json
{
  // ERROR: Missing cardName, description, cardType
  "manaCost": 1
}
```

**Fix:** Always include `cardName`, `description`, and `cardType`.

### ❌ Incorrect Enum Values
```json
{
  "cardType": "DamageCard"  // ERROR: Not a valid CardType
}
```

**Fix:** Use valid enum: `Attack`, `Guard`, `Skill`, `Power`, `Aura`.

### ❌ Attack Card Without Damage
```json
{
  "cardType": "Attack",
  "baseDamage": 0.0  // ERROR: Attack cards need damage
}
```

**Fix:** Set `baseDamage > 0` for Attack cards.

### ❌ Guard Card Without Guard
```json
{
  "cardType": "Guard",
  "baseGuard": 0.0  // ERROR: Guard cards need guard value
}
```

**Fix:** Set `baseGuard > 0` for Guard cards.

---

## 📦 Full Deck Template

```json
{
  "deckName": "My Custom Deck",
  "characterClass": "CustomClass",
  "description": "A custom deck for testing",
  "cards": [
    {
      "cardName": "Card 1",
      "count": 6,
      "data": { /* card data */ }
    },
    {
      "cardName": "Card 2",
      "count": 4,
      "data": { /* card data */ }
    }
  ]
}
```

---

## 🔗 Related Files

- **Full Schema**: `JSON_CARD_SCHEMA.md`
- **Example Decks**:
  - `Assets/Resources/CardJSON/MarauderStarterDeck.json`
  - `Assets/Resources/CardJSON/WitchStarterDeck.json`
- **Importer**: `Tools > Cards > Import Cards from JSON`

---

## 💡 Pro Tips

1. **Start Simple**: Begin with minimal cards, add complexity later
2. **Use Templates**: Copy existing cards and modify them
3. **Validate Early**: Use "Validate JSON" button before importing
4. **Backup Database**: Always backup your CardDatabase before mass imports
5. **Test Balance**: Import a few cards, test them, then adjust
6. **Consistent Naming**: Use clear, descriptive card names
7. **Tag Everything**: Use tags for filtering and searching

---

## 📞 Need Help?

1. **Validation Failed**: Check JSON syntax (commas, quotes, brackets)
2. **Import Failed**: Check Unity console for specific error messages
3. **Cards Don't Work**: Verify all required fields are present
4. **Scaling Issues**: Check attribute scaling values (use 0.0-1.0 range)

---

*Last Updated: 2025-10-02*


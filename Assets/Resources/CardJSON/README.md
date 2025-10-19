# Card JSON Files Directory

This directory contains JSON files for importing cards into the Dexiled Unity project.

## 📋 Available Files

### Starter Decks
- **MarauderStarterDeck.json** - Strength-based melee combat deck (18 cards)
- **WitchStarterDeck.json** - Intelligence-based elemental spell deck (12 cards)

### Reference Files
- **ExampleCardTypes.json** - Examples of all card types and mechanics (12 cards)

## 🚀 Quick Start

1. **Open Unity Editor**
2. **Go to**: Tools > Cards > Import Cards from JSON
3. **Click**: One of the quick load buttons OR browse to your file
4. **Click**: "Import Cards"

## 📝 Creating Your Own Deck

### Template
```json
{
  "deckName": "Your Deck Name",
  "characterClass": "YourClass",
  "description": "Deck description here",
  "cards": [
    {
      "cardName": "Card Name",
      "count": 6,
      "data": {
        "cardName": "Card Name",
        "description": "Card description",
        "cardType": "Attack",
        "manaCost": 1,
        "baseDamage": 8.0,
        "primaryDamageType": "Physical",
        "weaponScaling": { "scalesWithMeleeWeapon": true },
        "requirements": { "requiredStrength": 25 },
        "damageScaling": { "strengthScaling": 0.5 },
        "tags": ["Attack", "Physical"],
        "rarity": "Common",
        "element": "Physical",
        "category": "Attack"
      }
    }
  ]
}
```

### Steps
1. Copy template above
2. Save as `YourDeckName.json` in this directory
3. Modify card properties
4. Add more cards to the "cards" array
5. Import using Unity Editor tool

## 📖 Documentation

- **Full Schema**: `Assets/Scripts/Cards/JSON_CARD_SCHEMA.md`
- **Quick Reference**: `Assets/Scripts/Cards/CARD_JSON_QUICK_REFERENCE.md`
- **Implementation Guide**: `Assets/Scripts/Cards/CARD_JSON_IMPLEMENTATION_SUMMARY.md`

## 🎯 Card Type Reference

### Attack Card (Minimal)
```json
{
  "cardName": "Strike",
  "description": "Deal {damage} damage.",
  "cardType": "Attack",
  "manaCost": 1,
  "baseDamage": 6.0,
  "primaryDamageType": "Physical",
  "weaponScaling": { "scalesWithMeleeWeapon": true },
  "damageScaling": { "strengthScaling": 0.3 },
  "tags": ["Attack"],
  "rarity": "Common",
  "element": "Physical",
  "category": "Attack"
}
```

### Guard Card (Minimal)
```json
{
  "cardName": "Block",
  "description": "Gain {guard} Guard.",
  "cardType": "Guard",
  "manaCost": 1,
  "baseGuard": 5.0,
  "guardScaling": { "strengthScaling": 0.2 },
  "tags": ["Guard"],
  "rarity": "Common",
  "element": "Basic",
  "category": "Guard"
}
```

### Skill Card (Minimal)
```json
{
  "cardName": "Draw",
  "description": "Draw 2 cards.",
  "cardType": "Skill",
  "manaCost": 1,
  "effects": [
    {
      "effectType": "Draw",
      "effectName": "Draw",
      "description": "Draw 2 cards",
      "value": 2.0,
      "targetsSelf": true
    }
  ],
  "tags": ["Skill"],
  "rarity": "Common",
  "element": "Basic",
  "category": "Skill"
}
```

## 🎨 Class Starter Decks

### Recommended Deck Composition
- **6-8 Basic Attack Cards** (1 mana, 6-8 damage)
- **4-6 Guard Cards** (1 mana, 5-7 guard)
- **2-4 Utility Cards** (skills, draws, heals)
- **2-4 Special Cards** (unique class abilities)

**Total**: 14-22 cards (20 recommended for minimum deck size)

### Classes Needing Decks
- ✅ Marauder (Complete)
- ✅ Witch (Complete)
- ⬜ Ranger (Needs creation)
- ⬜ Brawler (Needs creation)
- ⬜ Thief (Needs creation)
- ⬜ Apostle (Needs creation)

## ⚡ Quick Tips

1. **Start with existing decks** - Copy and modify MarauderStarterDeck.json
2. **Validate before importing** - Use "Validate JSON" button
3. **Balance iteratively** - Import, test, adjust, repeat
4. **Use meaningful names** - Clear card names help organization
5. **Tag everything** - Tags make filtering easier
6. **Copy carefully** - Missing commas break JSON

## 🐛 Common Mistakes

### ❌ Missing Comma
```json
{
  "cardName": "Test"  // MISSING COMMA
  "cardType": "Attack"
}
```

### ✅ Correct
```json
{
  "cardName": "Test",
  "cardType": "Attack"
}
```

### ❌ Wrong Enum
```json
{
  "cardType": "DamageCard"  // Should be "Attack"
}
```

### ✅ Correct
```json
{
  "cardType": "Attack"
}
```

## 📞 Need Help?

1. Check Unity Console for errors
2. Use "Validate JSON" button
3. Review example files
4. Read documentation files
5. Check JSON syntax at jsonlint.com

---

**Happy Card Creating!** 🎴


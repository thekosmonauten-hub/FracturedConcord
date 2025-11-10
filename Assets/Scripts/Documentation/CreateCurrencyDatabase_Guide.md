# Create Currency Database Tool - Guide

## 🛠️ What It Does

This editor tool creates a **brand new CurrencyDatabase** ScriptableObject with all 28 currency types pre-configured and ready to use!

## 📍 How to Access

In Unity Editor:
```
Top Menu → Tools → Dexiled → Create Currency Database
```

## 🎯 Quick Start (30 seconds)

### Option 1: One-Click Setup (Recommended)
1. Open the tool: `Tools → Dexiled → Create Currency Database`
2. Click **"Create Database + Assign to Resources"** button
3. Done! ✅

This creates:
- **File**: `Assets/Resources/CurrencyDatabase.asset`
- **Contains**: All 28 currency types fully configured
- **Ready to use**: Immediately available in loot tables

### Option 2: Custom Location
1. Open the tool: `Tools → Dexiled → Create Currency Database`
2. Set **Database Name** (e.g., "MyCurrencyDatabase")
3. Set **Save Path** (or click "Browse")
4. Click **"Create Currency Database"** button
5. Asset created at your chosen location!

## 📦 What Gets Created

The database includes **all currency types** with descriptions:

### Orbs (9 types)
- **Orb of Generation** - Generate a card
- **Orb of Infusion** - Normal → Magic equipment
- **Orb of Perfection** - Magic → Rare equipment
- **Orb of Perpetuity** - Add random affix to Rare
- **Orb of Redundancy** - Reroll all affix values
- **Orb of the Void** - Corrupt item (powerful + unpredictable)
- **Orb of Mutation** - Transform one affix
- **Orb of Proliferation** - Duplicate a random affix
- **Orb of Amnesia** - Remove random affix (preserves locks)

### Spirits (9 types)
- **Fire Spirit** - Fire damage affixes
- **Cold Spirit** - Cold damage affixes
- **Lightning Spirit** - Lightning damage affixes
- **Chaos Spirit** - Chaos damage affixes
- **Physical Spirit** - Physical damage affixes
- **Life Spirit** - Life affixes
- **Defense Spirit** - Defense affixes
- **Crit Spirit** - Critical strike affixes
- **Divine Spirit** - Reroll to maximum value

### Seals (7 types)
- **Transposition Seal** - Swap affixes between items
- **Chaos Seal** - Shuffle all affixes
- **Memory Seal** - Save item state
- **Inscription Seal** - Lock an affix
- **Adaptation Seal** - Change affix type
- **Correction Seal** - Remove lowest tier affix
- **Etching Seal** - Improve affix tier

### Fragments (3 types)
- **Fragment 1, 2, 3** - Mysterious fragments (future use)

## 🎨 Adding Sprites Later

After creating the database:
1. Open the created `CurrencyDatabase.asset`
2. Expand the **Currencies** list
3. For each currency, drag a sprite into **Currency Sprite** field
4. Save

Or use this helper method in code:
```csharp
currencyDatabase.AssignSpriteToCurrency("Orb of Generation", mySprite);
```

## ✅ Verification

After creation, you should see:
- ✓ Asset appears in Project window at save location
- ✓ Inspector shows 28 currencies in list
- ✓ Each currency has name, description, rarity, slot index
- ✓ Can be dragged into Loot Table "Currency Database" field

## 🔧 Troubleshooting

**Tool menu not appearing?**
- Make sure `CreateCurrencyDatabase.cs` is in an `Editor` folder
- Restart Unity if needed

**Database created but empty?**
- Check console for error messages
- Try creating again with default settings

**Can't find the database after creation?**
- Check the save path you specified
- Default is `Assets/Resources/`
- Use Project window search: "CurrencyDatabase"

## 💡 Tips

- ✅ Use "Create Database + Assign to Resources" for fastest setup
- ✅ Only need ONE currency database for your whole game
- ✅ Add sprites after creation (not urgent for testing)
- ✅ Can create multiple databases for testing different setups

## 🔄 Replacing Old Database

If you have a corrupted database:
1. Delete or rename the old `CurrencyDatabase.asset`
2. Use this tool to create a fresh one
3. Reassign it to any Loot Tables that were using the old one

---

**Status**: ✅ Tool Ready  
**Time to Create**: ~5 seconds  
**Currencies Included**: 28/28














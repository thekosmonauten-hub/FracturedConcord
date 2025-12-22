# Embossing System - Setup Guide

## ✅ What's Been Implemented

### Core System (Complete)
- ✅ **EmbossingEffect ScriptableObject** - Define embossings with mana costs and requirements
- ✅ **EmbossingDatabase Manager** - Singleton that loads and manages all embossings
- ✅ **Mana Cost Formula** - Fully implemented: `(Base + N) × (1 + Σ Mult)`
- ✅ **Requirements System** - Level and stat validation
- ✅ **Card Integration** - Both `Card.cs` and `CardDataExtended.cs` support mana cost calculations
- ✅ **Editor Tools** - Quick embossing creation and sample generator

---

## 🚀 Setup Steps (In Unity)

### Step 1: Add EmbossingDatabase to Scene

**For Each Scene That Needs Embossings:**

1. Create empty GameObject: `EmbossingDatabase`
2. Add component: `EmbossingDatabase.cs`
3. Set `Embossing Resource Path`: `"Embossings"` (default)

**Recommended:** Add to your persistent managers scene or main menu.

---

### Step 2: Generate Sample Embossings

**In Unity Editor:**

1. Go to: `Tools > Card System > Generate Sample Embossings`
2. Click to generate 28 sample embossings
3. Assets created in: `Assets/Resources/Embossings/`

**Generated Embossings:**

**Damage (6):**
- of Ferocity (+35% Physical Damage)
- of the Inferno (50% Physical → Fire)
- of Amplification (+25% Spell Damage)
- of Momentum (+7% per hit)
- of Impact (+1 AoE radius)
- of Annihilation (15% double damage)

**Scaling (5):**
- of Focus (+15% primary stat scaling)
- of Efficiency (-20% mana cost)
- of Power (+3% per 50 STR)
- of Precision (+1% crit per 50 DEX)
- of Calculation (+10% per 50 INT)

**Utility (5):**
- of Recovery (3 Life on Hit)
- of Leeching (10% damage leeched)
- of Channeling (+20% per consecutive cast)
- of the Echo (Repeat at 50% damage)
- of Preparation (Double if prepared 2 turns)

**Defensive (4):**
- of Endurance (+1 Guard when played)
- of Fortification (Gain Bolster stacks)
- of the Bastion (+20% Guard effectiveness)
- of Reflection (Reflect 10% damage)

**Combo (3):**
- of the Gambit (Discard rebirth +50%)
- of Flow (+15% per previous skill)
- of Crescendo (+25% per repeat)

**Ailment (5):**
- of Cruelty (15% Bleed chance)
- of Immolation (15% Ignite chance)
- of Sepsis (15% Poison chance)
- of Conduction (15% Shock chance)
- of Brittle Frost (15% Freeze/Chill chance)

---

### Step 3: Verify Database Loading

**Test in Play Mode:**

1. Start Play Mode
2. Check Console for: `[EmbossingDatabase] Loaded X embossing effects`
3. If you see errors, check:
   - EmbossingDatabase GameObject exists in scene
   - Resource path is correct (`Resources/Embossings/`)
   - Embossing assets were created

---

## 🧪 Testing Mana Cost Calculations

### Test in Code

```csharp
// Get a card instance
Card card = someCard;

// Get its base mana cost
int baseCost = card.manaCost; // e.g., 2

// Apply some embossings (simulated for testing)
card.appliedEmbossings.Add(new EmbossingInstance 
{ 
    embossingId = "ferocity", // of Ferocity
    level = 1, 
    slotIndex = 0 
});

// Calculate new mana cost
int newCost = card.GetCurrentManaCost();
// With "of Ferocity" (+35%): (2+1) × (1+0.35) = 4.05 ≈ 5 mana

// Get formatted display
string display = card.GetManaCostDisplay();
// Result: "5 (+3)"
```

### Test with CardDataExtended

```csharp
CardDataExtended cardData = someCardData;
Card cardInstance = cardData.ToCard();

// Apply embossings to card instance
cardInstance.appliedEmbossings.Add(...);

// Calculate through CardDataExtended
int cost = cardData.GetCurrentManaCost(cardInstance);
string display = cardData.GetManaCostDisplay(cardInstance);
string breakdown = cardData.GetManaCostBreakdown(cardInstance);
```

---

## 📝 Creating Custom Embossings

### Method 1: Use the Creator Tool

**In Unity Editor:**

1. Go to: `Tools > Card System > Create Embossing`
2. Fill in the form:
   - **Name**: "of [YourName]"
   - **Description**: What it does
   - **Category**: Damage, Scaling, Utility, etc.
   - **Rarity**: Common, Uncommon, Rare, Epic, Legendary
   - **Mana Multiplier**: 0.25 = +25% mana cost
   - **Requirements**: Min Level, STR, DEX, INT
   - **Effect Type**: Choose from dropdown
   - **Effect Value**: Primary effect value
3. Click "Create Embossing"
4. Asset created in `Resources/Embossings/`

### Method 2: Create ScriptableObject Directly

**In Project Window:**

1. Right-click in `Resources/Embossings/`
2. Create > Card System > Embossing Effect
3. Configure in Inspector:
   - Basic Info (name, description, icon)
   - Category & Rarity
   - Mana Cost Impact (multiplier + flat)
   - Requirements (level, stats)
   - Effect Mechanics (type, values)
   - Special Flags (unique, exclusivity)

---

## 🎨 Embossing Properties Explained

### Mana Cost Impact

**Mana Cost Multiplier:**
- `0.15` = Low impact (+15%)
- `0.25` = Standard (+25%)
- `0.35` = High impact (+35%)
- `0.50+` = Very high impact (+50%+)

**Flat Mana Cost Increase:**
- Additional flat cost after multiplier
- Rarely used (most embossings use multiplier only)

### Requirements

**Level Requirements (Recommended Tiers):**
- **1-5**: Common/basic embossings
- **6-10**: Uncommon embossings
- **11-15**: Rare embossings
- **16-20**: Epic embossings
- **21+**: Legendary embossings

**Stat Requirements:**
- **Strength**: Physical damage, AoE, Guard effects
- **Dexterity**: Crit, combo, evasion effects
- **Intelligence**: Spell damage, conversions, utility

### Effect Types

**Damage Types:**
- `DamageMultiplier` - Generic +X% damage
- `PhysicalDamageMultiplier` - +X% physical only
- `SpellDamageMultiplier` - +X% spell only
- `FlatDamageBonus` - +X flat damage

**Conversion Types:**
- `PhysicalToFireConversion` - X% Phys → Fire
- `PhysicalToColdConversion` - X% Phys → Cold
- `PhysicalToLightningConversion` - X% Phys → Lightning

**Scaling Types:**
- `StrengthScaling` - +X% per strength
- `DexterityScaling` - +X% per dexterity
- `IntelligenceScaling` - +X% per intelligence

**Utility Types:**
- `ManaCostReduction` - -X% mana cost
- `CriticalChance` - +X% crit chance
- `LifeOnHit` - Gain X life per hit
- `LifeLeech` - X% damage leeched
- `CardDuplication` - Repeat at X% damage

**Status Effect Types:**
- `ApplyBleed`, `ApplyIgnite`, `ApplyPoison`, etc.
- Use `secondaryEffectValue` for chance (0.15 = 15%)

**Special:**
- `CustomEffect` - Unique mechanics (use `customEffectId`)

### Special Flags

**Unique:**
- `true` = Can only apply once per card
- Example: "of the Echo" (card duplication)

**Exclusivity Group:**
- Group name (e.g., `"elemental_conversion"`)
- Only one embossing from group can be applied
- Example: Can't have both Fire and Cold conversions

---

## 🔄 Next Steps

### Implement Embossing UI (Pending)

**In EquipmentScreen:**
1. Card carousel (✓ Already done)
2. Embossing selection panel
3. Preview mana cost changes
4. Apply button with validation
5. Visual feedback for requirements

### Integrate with Combat (Pending)

**In Combat System:**
1. Use `card.GetCurrentManaCost()` for mana checks
2. Apply embossing effects during card play
3. Effect processor for each embossing type
4. Status effect application
5. Special mechanics (Echo, Channeling, etc.)

---

## 🐛 Troubleshooting

### "EmbossingDatabase not found"
- **Solution**: Add EmbossingDatabase component to scene
- Check: GameObject is active and not destroyed

### "No embossings loaded"
- **Solution**: Check `Resources/Embossings/` folder exists
- Generate samples: `Tools > Card System > Generate Sample Embossings`
- Verify resource path in EmbossingDatabase inspector

### "Embossing ID not found"
- **Solution**: EmbossingEffect has empty `embossingId`
- Select embossing asset, it will auto-generate ID from name
- Or manually enter ID in Inspector

### Mana cost calculation wrong
- **Solution**: Check embossing `manaCostMultiplier` values
- Verify all applied embossings have valid IDs
- Test with `EmbossingDatabase.GetManaCostBreakdown(card, baseCost)`

---

## 📦 Files Summary

### Scripts Created
```
Assets/Scripts/Data/Embossing/
├── EmbossingEffect.cs              ✓ ScriptableObject definition
├── EmbossingInstance.cs            ✓ Applied embossing data (already existed)

Assets/Scripts/Managers/
├── EmbossingDatabase.cs            ✓ Singleton manager

Assets/Scripts/Cards/
├── Card.cs                         ✓ Added GetCurrentManaCost()
├── CardDataExtended.cs             ✓ Added GetCurrentManaCost(Card)

Assets/Scripts/Editor/
├── EmbossingCreator.cs             ✓ Manual creation tool
├── EmbossingSampleGenerator.cs     ✓ Sample generation tool
```

### Resources
```
Assets/Resources/Embossings/        (28 sample embossings)
├── Ferocity.asset
├── Inferno.asset
├── Amplification.asset
└── ... (25 more)
```

### Documentation
```
Assets/Documentation/
├── EMBOSSING_SYSTEM.md             ✓ Full system documentation
├── EMBOSSING_SETUP_GUIDE.md        ✓ This file
├── Embossing_Effects.md            ✓ Design document (user-provided)
```

---

## ✅ Status: Core System Ready!

**Implemented:**
- ✅ Mana cost formula
- ✅ Requirements validation
- ✅ Database management
- ✅ Card integration
- ✅ Editor tools
- ✅ 28 sample embossings

**Next Phase:**
- ⏳ Embossing UI panel
- ⏳ Combat integration
- ⏳ Effect processing

**Ready to test:** Mana cost calculations and requirement validation!

---

## 🎮 Quick Start Checklist

- [ ] Add `EmbossingDatabase` to scene
- [ ] Generate sample embossings (`Tools > Card System > Generate Sample Embossings`)
- [ ] Enter Play Mode and verify embossings loaded
- [ ] Test mana cost calculations in code
- [ ] Create custom embossings using Creator tool
- [ ] Ready to implement UI!

**You're all set to start using the Embossing System! 🎉**


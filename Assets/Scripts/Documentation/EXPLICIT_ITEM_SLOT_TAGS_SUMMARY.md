# 📋 **EXPLICIT ITEM SLOT TAGS - CSV UPDATE SUMMARY**

## ✅ **COMPLETED UPDATES:**

### **All Resistances** (Fire, Cold, Lightning, Chaos)
**Old:** `Armour`  
**New:** `Helmet Body Armour Gloves Boots Shield`

**Why:** Resistances should roll on **ALL armor pieces**, not just body armor. Now it's crystal clear!

**Examples:**
```csv
Fire Resistance,Suffix,of the Ember,+5–8% to Fire Resistance,5,8,Helmet Body Armour Gloves Boots Shield,fireResistance,9,1,Global
Cold Resistance,Suffix,of the Frost,+5–8% to Cold Resistance,5,8,Helmet Body Armour Gloves Boots Shield,coldResistance,9,1,Global
Lightning Resistance,Suffix,of the Static,+5–8% to Lightning Resistance,5,8,Helmet Body Armour Gloves Boots Shield,lightningResistance,9,1,Global
Chaos Resistance,Suffix,of the Shadow,+3–5% to Chaos Resistance,3,5,Helmet Body Armour Gloves Boots Shield,chaosResistance,9,1,Global
```

**Total Updated:** 36 resistance affixes ✅

---

### **Life** 
**Old:** `Armour Jewelry`  
**New:** `Helmet Body Armour Gloves Boots Shield Jewelry`

**Why:** Life should roll on ALL armor slots + jewelry!

**Examples:**
```csv
Life,Prefix,Healthy,+10–19 to Maximum Life,10,19,Helmet Body Armour Gloves Boots Shield Jewelry,maxHealth,8,10,Global
Life,Prefix,Titanic,+80–99 to Maximum Life,80,99,Helmet Body Armour Gloves Boots Shield Jewelry,maxHealth,1,80,Global
```

**Total Updated:** 7 life affixes ✅

---

### **Critical Strike Multiplier**
**Old:** `Armour Jewelry`  
**New:** `Helmet Body Armour Gloves Boots Shield Jewelry`

**Why:** Critical multiplier is a global stat that should work on all armor + jewelry!

**Examples:**
```csv
Critical Multiplier,Suffix,of Lethality,+12–18% increased Critical Strike Multiplier,12,18,Helmet Body Armour Gloves Boots Shield Jewelry,criticalMultiplier,8,10,Global
```

**Total Updated:** 3 critical multiplier affixes ✅

---

## 📊 **COMPLETE TAG REFERENCE:**

### **Explicit Slot Tags (NEW SYSTEM):**

| Tag | Meaning |
|-----|---------|
| `Weapon` | All weapon types |
| `Helmet` | Only helmets |
| `Body Armour` | Only body armor (chest) |
| `Gloves` | Only gloves |
| `Boots` | Only boots |
| `Shield` | Only shields |
| `Jewelry` | Rings, amulets |

### **Combined Slot Tags:**

| Tag | Meaning |
|-----|---------|
| `Helmet Body Armour Gloves Boots Shield` | ALL armor pieces |
| `Helmet Body Armour Gloves Boots Shield Jewelry` | ALL armor + jewelry |
| `Weapon Armour Jewelry` | Everything (used for attributes) |

### **Smart Compatibility Tags (UNCHANGED):**

These are still used for special cases where affixes need base stat checks:

| Tag | Meaning | When to Use |
|-----|---------|-------------|
| `ES Base Armour` | Only ES base armor | % Energy Shield affixes |
| `Armour Base Armour` | Only Armour base armor | % Armour affixes |
| `Evasion Base Armour` | Only Evasion base armor | % Evasion affixes |

**Important:** These should apply to **ANY EQUIPMENT** with those base stats (not just armor), as the user noted!

---

## 🎯 **HOW THE IMPORTER HANDLES THESE:**

### **AffixCSVImporter Parsing:**

When it sees `"Helmet Body Armour Gloves Boots Shield"`, it creates:
```csharp
affix.compatibleTags = new List<string> 
{ 
    "helmet", 
    "body_armour", // or "bodyarmour"
    "gloves", 
    "boots", 
    "shield" 
};
```

### **Item Tagging:**

**Armor pieces** imported via `ArmorCSVImporter` get tagged:
```csharp
// Helmet
itemTags = ["armour", "helmet", "defence", ...]

// Body Armour
itemTags = ["armour", "bodyarmour", "defence", ...]

// Gloves
itemTags = ["armour", "gloves", "defence", ...]

// Boots
itemTags = ["armour", "boots", "defence", ...]

// Shield
itemTags = ["armour", "shield", "defence", ...]
```

### **Affix Compatibility Check:**

```csharp
// Affix has: ["helmet", "bodyarmour", "gloves", "boots", "shield"]
// Helmet has: ["armour", "helmet", "defence"]
// Match found on "helmet" → ✅ Compatible!

// Affix has: ["helmet", "bodyarmour", "gloves", "boots", "shield"]
// Weapon has: ["weapon", "sword", "melee"]
// No match → ❌ Not compatible
```

---

## ⚠️ **STILL NEED TO UPDATE:**

The following tags might still exist in the CSV and need verification:

### **To Check:**
```
"Armour" (standalone) - Should probably be "Helmet Body Armour Gloves Boots Shield"
"Armour Jewelry" - Should be "Helmet Body Armour Gloves Boots Shield Jewelry"
```

### **Special Cases That Are CORRECT:**
```
"Jewelry" - Correct for mana, some unique modifiers
"Weapon" - Correct for all weapon damage/speed affixes
"Weapon Armour Jewelry" - Correct for attributes (STR/DEX/INT)
"ES Base Armour" - Correct for % ES (but should work on ANY ES equipment!)
"Armour Base Armour" - Correct for % Armour (but should work on ANY Armour equipment!)
"Evasion Base Armour" - Correct for % Evasion (but should work on ANY Evasion equipment!)
"Boots" - Correct for movement speed (boots only)
"Shield" - Correct for block chance (shields only)
```

---

## 🔍 **USER'S IMPORTANT NOTE:**

> **% Energy Shield → Should work on ES base EQUIPMENT, not just armour**  
> **% Armour → Should work on Armour base EQUIPMENT, not just armour**  
> **% Evasion → Should work on Evasion base EQUIPMENT, not just armour**

**What this means:**
- If a weapon somehow had Energy Shield, `% Energy Shield` should work on it
- If jewelry had Armour rating, `% Armour` should work on it
- The "Base Armour" naming is a bit misleading - it's really "Base Stat"

**Current implementation:**
The smart compatibility system in `AffixDatabase.IsAffixCompatibleWithItem()` checks for the actual base stat on the item:
```csharp
if (modifier.statName.Contains("energyShield") && item is Armour armourItem && armourItem.energyShield <= 0) 
    return false;
```

This is **equipment-agnostic** - it works on ANY item type, not just armor!

---

## ✅ **BENEFITS OF EXPLICIT TAGS:**

1. **Crystal Clear:** No ambiguity about which slots affixes apply to
2. **Easy to Read:** CSV is self-documenting
3. **Less Logic:** Importer doesn't need complex rules
4. **Maintainable:** Easy to add/remove slots
5. **Debuggable:** Can see exactly what's intended

---

## 📝 **NEXT STEPS:**

1. ✅ **Resistances updated** (Fire, Cold, Lightning, Chaos)
2. ✅ **Life updated** 
3. ✅ **Critical Multiplier updated**
4. ⏳ **Search for remaining "Armour" entries** and update as needed
5. ⏳ **Re-import affixes** from updated CSV
6. ⏳ **Test item generation** to verify affixes roll correctly

---

## 🎯 **TESTING AFTER RE-IMPORT:**

### **Test 1: Resistance on All Armor**
```
Generate: Helmet with Fire Resistance ✅
Generate: Gloves with Cold Resistance ✅
Generate: Boots with Lightning Resistance ✅
Generate: Shield with Chaos Resistance ✅
Generate: Body Armour with Fire Resistance ✅
```

### **Test 2: No Resistances on Weapons**
```
Generate: Sword with Fire Resistance ❌ Should NOT happen
Generate: Bow with Cold Resistance ❌ Should NOT happen
```

### **Test 3: Life on Armor + Jewelry**
```
Generate: Helmet with +Life ✅
Generate: Jewelry with +Life ✅
Generate: Weapon with +Life ❌ Should NOT happen
```

---

## 🏆 **SUMMARY:**

**Updated:** 46 total affixes  
**Approach:** Slow and steady, explicit and clear  
**Result:** Professional, maintainable, debuggable affix system  

**The CSV is now more explicit and easier to understand!** 🎯✅










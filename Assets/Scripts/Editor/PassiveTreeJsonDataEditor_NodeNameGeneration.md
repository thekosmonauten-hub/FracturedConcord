# Passive Tree JSON Data Editor - Automatic Node Name Generation

## ✅ **New Feature: Automatic Node Name Generation!**

The Passive Tree JSON Data Editor now includes **automatic node name generation** based on the applied stats. This makes it much easier to identify what each cell does at a glance!

## 🎯 **What's New**

### **Automatic Node Name Generation**
- **Smart naming** based on applied stats
- **Configurable naming options** (prefix, position, stats)
- **Real-time preview** of generated names
- **Automatic application** when using bulk operations

### **Example Transformations**
- **Before**: `Cell_3_4_Strength & Intelligence`
- **After**: `Cell_3_4_Max Health` (when applying 4% Max Health Increase)
- **Before**: `Cell_2_1_Attack Power`
- **After**: `Cell_2_1_Fire Damage & Critical Chance` (when applying fire damage and crit chance)

## 🚀 **How to Use**

### **Step 1: Enable Node Name Generation**
1. **Open the editor** (Tools → Passive Tree → JSON Data Editor)
2. **Go to "Bulk Operations" tab**
3. **Check "Auto Generate Node Names"** to enable the feature

### **Step 2: Configure Naming Options**
1. **Set "Name Prefix"** (default: "Cell")
2. **Toggle "Include Position in Name"** (adds _X_Y to the name)
3. **Toggle "Include Stats in Name"** (adds stat names to the name)
4. **See the preview** of how names will be generated

### **Step 3: Apply with Bulk Operations**
1. **Create your stat template** with the desired stats
2. **Enable "Bulk Mode"** and select cells
3. **Click "Apply Stat Template"** - names will be automatically generated!

## 🛠️ **Naming Configuration Options**

### **Name Prefix**
- **Default**: "Cell"
- **Customizable**: You can change this to anything (e.g., "Node", "Skill", "Passive")
- **Example**: "Node_3_4_Max Health" instead of "Cell_3_4_Max Health"

### **Include Position in Name**
- **Enabled**: Adds position coordinates to the name (e.g., "_3_4")
- **Disabled**: Omits position from the name
- **Example**: 
  - **Enabled**: `Cell_3_4_Max Health`
  - **Disabled**: `Cell_Max Health`

### **Include Stats in Name**
- **Enabled**: Adds stat names to the name (e.g., "_Max Health")
- **Disabled**: Omits stats from the name
- **Example**:
  - **Enabled**: `Cell_3_4_Max Health`
  - **Disabled**: `Cell_3_4`

## 📋 **Stat Name Mapping**

### **Core Attributes**
- `strength` → **"Strength"**
- `dexterity` → **"Dexterity"**
- `intelligence` → **"Intelligence"**

### **Combat Resources**
- `maxHealthIncrease` → **"Max Health"**
- `maxEnergyShieldIncrease` → **"Max Energy Shield"**
- `maxMana` → **"Max Mana"**
- `maxReliance` → **"Max Reliance"**

### **Combat Stats**
- `attackPower` → **"Attack Power"**
- `defense` → **"Defense"**
- `criticalChance` → **"Critical Chance"**
- `criticalMultiplier` → **"Critical Multiplier"**
- `accuracy` → **"Accuracy"**

### **Damage Modifiers (Increased)**
- `increasedPhysicalDamage` → **"Increased Physical Damage"**
- `increasedFireDamage` → **"Increased Fire Damage"**
- `increasedColdDamage` → **"Increased Cold Damage"**
- `increasedLightningDamage` → **"Increased Lightning Damage"**
- `increasedChaosDamage` → **"Increased Chaos Damage"**
- `increasedElementalDamage` → **"Increased Elemental Damage"**
- `increasedSpellDamage` → **"Increased Spell Damage"**
- `increasedAttackDamage` → **"Increased Attack Damage"**

### **Added Damage**
- `addedPhysicalDamage` → **"Added Physical Damage"**
- `addedFireDamage` → **"Added Fire Damage"**
- `addedColdDamage` → **"Added Cold Damage"**
- `addedLightningDamage` → **"Added Lightning Damage"**
- `addedChaosDamage` → **"Added Chaos Damage"**
- `addedElementalDamage` → **"Added Elemental Damage"**
- `addedSpellDamage` → **"Added Spell Damage"**
- `addedAttackDamage` → **"Added Attack Damage"**

### **Elemental Conversions**
- `addedPhysicalAsFire` → **"Physical as Fire"**
- `addedPhysicalAsCold` → **"Physical as Cold"**
- `addedPhysicalAsLightning` → **"Physical as Lightning"**
- `addedFireAsCold` → **"Fire as Cold"**
- `addedColdAsFire` → **"Cold as Fire"**
- `addedLightningAsFire` → **"Lightning as Fire"**

### **Resistances**
- `physicalResistance` → **"Physical Resistance"**
- `fireResistance` → **"Fire Resistance"**
- `coldResistance` → **"Cold Resistance"**
- `lightningResistance` → **"Lightning Resistance"**
- `chaosResistance` → **"Chaos Resistance"**
- `elementalResistance` → **"Elemental Resistance"**
- `allResistance` → **"All Resistance"**

### **Defense Stats**
- `armour` → **"Armour"**
- `evasion` → **"Evasion"**
- `energyShield` → **"Energy Shield"**
- `blockChance` → **"Block Chance"**
- `dodgeChance` → **"Dodge Chance"**
- `spellDodgeChance` → **"Spell Dodge"**
- `spellBlockChance` → **"Spell Block"**

### **Recovery Stats**
- `lifeRegeneration` → **"Life Regen"**
- `energyShieldRegeneration` → **"Energy Shield Regen"**
- `manaRegeneration` → **"Mana Regen"**
- `relianceRegeneration` → **"Reliance Regen"**
- `lifeLeech` → **"Life Leech"**
- `manaLeech` → **"Mana Leech"**
- `energyShieldLeech` → **"Energy Shield Leech"**

### **Combat Mechanics**
- `attackSpeed` → **"Attack Speed"**
- `castSpeed` → **"Cast Speed"**
- `movementSpeed` → **"Movement Speed"**
- `attackRange` → **"Attack Range"**
- `projectileSpeed` → **"Projectile Speed"**
- `areaOfEffect` → **"Area of Effect"**
- `skillEffectDuration` → **"Skill Duration"**
- `statusEffectDuration` → **"Status Duration"**

### **Card System Stats**
- `cardsDrawnPerTurn` → **"Cards Drawn"**
- `maxHandSize` → **"Max Hand Size"**
- `cardDrawChance` → **"Card Draw Chance"**
- `cardRetentionChance` → **"Card Retention"**
- `cardUpgradeChance` → **"Card Upgrade"**
- `discardPower` → **"Discard Power"**
- `manaPerTurn` → **"Mana Per Turn"**

### **Legacy Stats**
- `armorIncrease` → **"Armor Increase"**
- `increasedEvasion` → **"Evasion Increase"**
- `elementalResist` → **"Elemental Resist"**
- `spellPowerIncrease` → **"Spell Power"**
- `critChanceIncrease` → **"Crit Chance"**
- `critMultiplierIncrease` → **"Crit Multiplier"**

## 🎨 **Name Generation Examples**

### **Example 1: Max Health Template**
- **Template**: `maxHealthIncrease = 4`
- **Generated Name**: `Cell_3_4_Max Health`
- **Configuration**: Prefix="Cell", Position=true, Stats=true

### **Example 2: Fire Damage Template**
- **Template**: `increasedFireDamage = 15, criticalChance = 5`
- **Generated Name**: `Cell_2_1_Increased Fire Damage & Critical Chance`
- **Configuration**: Prefix="Cell", Position=true, Stats=true

### **Example 3: Complex Template**
- **Template**: `strength = 10, attackPower = 15, increasedPhysicalDamage = 20`
- **Generated Name**: `Cell_5_2_Strength & Attack Power & Increased Physical Damage`
- **Configuration**: Prefix="Cell", Position=true, Stats=true

### **Example 4: No Position**
- **Template**: `maxMana = 50, spellDamage = 25`
- **Generated Name**: `Cell_Max Mana & Spell Damage`
- **Configuration**: Prefix="Cell", Position=false, Stats=true

### **Example 5: No Stats**
- **Template**: `strength = 10, attackPower = 15`
- **Generated Name**: `Cell_3_4`
- **Configuration**: Prefix="Cell", Position=true, Stats=false

## 🧪 **Testing the Feature**

### **Test 1: Basic Name Generation**
1. **Enable "Auto Generate Node Names"**
2. **Set template** with `maxHealthIncrease = 4`
3. **Select a cell** at position (3,4)
4. **Apply template** and check the generated name
5. **Expected result**: `Cell_3_4_Max Health`

### **Test 2: Multiple Stats**
1. **Set template** with `strength = 10, attackPower = 15`
2. **Select a cell** at position (2,1)
3. **Apply template** and check the generated name
4. **Expected result**: `Cell_2_1_Strength & Attack Power`

### **Test 3: Configuration Options**
1. **Disable "Include Position in Name"**
2. **Set template** with `fireDamage = 15`
3. **Apply template** and check the generated name
4. **Expected result**: `Cell_Fire Damage`

### **Test 4: No Stats in Name**
1. **Disable "Include Stats in Name"**
2. **Set template** with any stats
3. **Apply template** and check the generated name
4. **Expected result**: `Cell_3_4` (just position)

## 🎯 **Best Practices**

### **For Naming Consistency**
- **Use consistent prefixes** across your project
- **Enable position** for easy identification
- **Enable stats** for quick understanding of what each cell does
- **Test with a few cells** before bulk application

### **For Complex Templates**
- **Limit to 3 stats** in names (automatically limited)
- **Use descriptive stat names** that are easy to understand
- **Consider the length** of generated names
- **Test readability** in your UI

### **For Bulk Operations**
- **Preview names** before applying to many cells
- **Use consistent templates** for similar node types
- **Verify generated names** after application
- **Check prefab changes** to ensure names are saved

## 🚨 **Important Notes**

### **Name Length Limits**
- **Maximum 3 stats** are included in names to keep them manageable
- **Long stat names** are shortened (e.g., "Energy Shield Regen" instead of "Energy Shield Regeneration")
- **Position coordinates** are always included when enabled

### **Stat Detection**
- **Only non-zero stats** are included in names
- **Zero values** are ignored completely
- **All stat categories** are supported

### **Prefab Saving**
- **Node names are saved** to prefabs automatically
- **Changes persist** when opening prefabs
- **Console logging** shows when names are updated

## 🎉 **Result**

You now have **automatic, intelligent node naming**! The system will:

- ✅ **Generate descriptive names** based on applied stats
- ✅ **Include position information** for easy identification
- ✅ **Use consistent naming** across all cells
- ✅ **Save changes** to prefabs automatically
- ✅ **Preview names** before applying
- ✅ **Support all stat types** with proper mapping

**No more manual naming - the system does it automatically!** 🎉

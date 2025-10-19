# Passive Tree JSON Data Editor - Improved Stat Naming

## ✅ **Updated: More Accurate Stat Naming!**

The stat name generation has been updated to provide more accurate and descriptive names that clearly distinguish between different types of damage modifiers.

## 🔧 **What Changed**

### **Before (Inaccurate)**
- `increasedFireDamage` → **"Fire Damage"** ❌
- `addedFireDamage` → **"Added Fire"** ❌

### **After (Accurate)**
- `increasedFireDamage` → **"Increased Fire Damage"** ✅
- `addedFireDamage` → **"Added Fire Damage"** ✅

## 🎯 **Improved Naming Convention**

### **Damage Modifiers (Increased) - Percentage Based**
- `increasedPhysicalDamage` → **"Increased Physical Damage"**
- `increasedFireDamage` → **"Increased Fire Damage"**
- `increasedColdDamage` → **"Increased Cold Damage"**
- `increasedLightningDamage` → **"Increased Lightning Damage"**
- `increasedChaosDamage` → **"Increased Chaos Damage"**
- `increasedElementalDamage` → **"Increased Elemental Damage"**
- `increasedSpellDamage` → **"Increased Spell Damage"**
- `increasedAttackDamage` → **"Increased Attack Damage"**

### **Added Damage - Flat Values**
- `addedPhysicalDamage` → **"Added Physical Damage"**
- `addedFireDamage` → **"Added Fire Damage"**
- `addedColdDamage` → **"Added Cold Damage"**
- `addedLightningDamage` → **"Added Lightning Damage"**
- `addedChaosDamage` → **"Added Chaos Damage"**
- `addedElementalDamage` → **"Added Elemental Damage"**
- `addedSpellDamage` → **"Added Spell Damage"**
- `addedAttackDamage` → **"Added Attack Damage"**

## 📋 **Name Generation Examples**

### **Example 1: Increased Fire Damage**
- **Template**: `increasedFireDamage = 15`
- **Generated Name**: `Cell_3_4_Increased Fire Damage`
- **Clear distinction**: This is percentage-based fire damage increase

### **Example 2: Added Fire Damage**
- **Template**: `addedFireDamage = 25`
- **Generated Name**: `Cell_2_1_Added Fire Damage`
- **Clear distinction**: This is flat fire damage addition

### **Example 3: Mixed Damage Types**
- **Template**: `increasedFireDamage = 15, addedColdDamage = 20`
- **Generated Name**: `Cell_5_2_Increased Fire Damage & Added Cold Damage`
- **Clear distinction**: Shows both percentage and flat damage

### **Example 4: Complex Template**
- **Template**: `strength = 10, increasedPhysicalDamage = 20, addedFireDamage = 15`
- **Generated Name**: `Cell_3_4_Strength & Increased Physical Damage & Added Fire Damage`
- **Clear distinction**: Shows attributes, percentage damage, and flat damage

## 🎨 **Benefits of the New Naming**

### **Clear Distinction**
- **"Increased Fire Damage"** = Percentage-based damage increase
- **"Added Fire Damage"** = Flat fire damage addition
- **No confusion** between different damage types

### **Better Understanding**
- **Immediate recognition** of damage type from the name
- **Consistent naming** across all damage modifiers
- **Professional appearance** in generated names

### **Accurate Representation**
- **Reflects the actual stat type** being applied
- **Matches game terminology** and conventions
- **Easier to understand** for developers and designers

## 🧪 **Testing the Updated Naming**

### **Test 1: Increased Damage**
1. **Set template** with `increasedFireDamage = 15`
2. **Apply to a cell** at position (3,4)
3. **Expected result**: `Cell_3_4_Increased Fire Damage`

### **Test 2: Added Damage**
1. **Set template** with `addedFireDamage = 25`
2. **Apply to a cell** at position (2,1)
3. **Expected result**: `Cell_2_1_Added Fire Damage`

### **Test 3: Mixed Damage Types**
1. **Set template** with `increasedPhysicalDamage = 20, addedColdDamage = 15`
2. **Apply to a cell** at position (5,2)
3. **Expected result**: `Cell_5_2_Increased Physical Damage & Added Cold Damage`

## 🎯 **Best Practices**

### **For Template Creation**
- **Use "Increased"** for percentage-based damage modifiers
- **Use "Added"** for flat damage additions
- **Mix both types** when creating complex templates
- **Test naming** with a few cells before bulk application

### **For Name Generation**
- **Enable "Include Stats in Name"** to see the full stat names
- **Use descriptive templates** that clearly show intent
- **Verify generated names** before applying to many cells
- **Check prefab changes** to ensure names are saved

## 🎉 **Result**

The stat naming is now **more accurate and descriptive**! You can:

- ✅ **Distinguish between** percentage and flat damage
- ✅ **Generate clear names** that reflect the actual stat type
- ✅ **Use consistent naming** across all damage modifiers
- ✅ **Create professional-looking** node names
- ✅ **Avoid confusion** between different damage types

**No more ambiguous naming - every stat type is clearly identified!** 🎉


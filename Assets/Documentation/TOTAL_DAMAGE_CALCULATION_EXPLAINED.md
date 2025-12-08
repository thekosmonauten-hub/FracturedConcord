# "Total" Damage Calculation Explained

**Date:** December 4, 2025  
**Question:** Where does the "Total" damage value come from in weapon tooltips?

---

## 📊 **Current Implementation**

### **Formula (in Weapon.cs):**

```csharp
public float GetTotalMinDamage()
{
    // 1. Start with base minimum damage
    float totalDamage = minDamage;  // Example: 6
    
    // 2. Add minimum from affixes (dual-range)
    // Example: "Adds (34-47) to (72-84)" → uses 34-47 range for min
    totalDamage += physicalMin;     // Example: +41
    
    // 3. Apply increased damage % (from affixes)
    // Example: "+18% Increased Physical Damage"
    totalDamage *= (1f + increasedDamage / 100f);  // Example: ×1.18
    
    return totalDamage;  // Result: 55.38 → 56
}

public float GetTotalMaxDamage()
{
    // Same formula, but uses maxDamage and affix maximums
    float totalDamage = maxDamage;  // Example: 11
    totalDamage += physicalMax;     // Example: +78
    totalDamage *= (1f + increasedDamage / 100f);  // ×1.18
    
    return totalDamage;  // Result: 105.02 → 105
}
```

---

## 🎯 **What Gets Displayed**

### **In Current Tooltip Code:**

```csharp
float totalMin = weapon.GetTotalMinDamage();  // 56
float totalMax = weapon.GetTotalMaxDamage();  // 105
int rolledTotal = Mathf.RoundToInt((totalMin + totalMax) / 2f);  // (56+105)/2 = 80.5 → 81

damageLabel.text = $"Damage: 8  (Total 81)";
```

---

## ⚠️ **POTENTIAL ISSUE**

### **Current Display:**
```
Worn Hatchet (rolled base: 8)
Damage: 8  (Total 81)
        ↑         ↑
     Rolled    Average of range-based total
```

### **The Problem:**

**"Total" is calculated using the RANGE method:**
- Base: 6-11 range → adds affixes → gets 56-105 range → averages to 81
- **But the rolled base (8) isn't being used in the total calculation!**

### **Should It Be?**

**Option 1: Current (Range-Based Total)**
```
Base Range: 6-11 → Rolled: 8
Total Range: 56-105 → Displayed: 81 (average)

Issue: Total doesn't reflect rolled base!
```

**Option 2: Rolled-Based Total (More Accurate)**
```
Base Rolled: 8
+ Affixes: 41-78 → Use average (59.5)
+ % Increase: ×1.18
= Total: (8 + 59.5) × 1.18 = 79.65 → 80

More accurate to what cards will actually use!
```

---

## 🔧 **What "Total" Actually Represents**

**Total Damage = Base Damage + All Affixes + % Increases**

### **Example Breakdown:**

```
Devastating Worn Hatchet of Mastery (Rare)

BASE:
├─ Asset: 6-11 damage
└─ Rolled: 8 damage ✅

AFFIXES:
├─ Prefix "Devastating": Adds (34-47) to (72-84) → Rolled: 41 to 78
└─ Suffix "of Mastery": +18% Increased Physical Damage

CALCULATION:
┌─ GetTotalMinDamage():
│  └─ (6 + 41) × 1.18 = 55.46 → 56
│
├─ GetTotalMaxDamage():
│  └─ (11 + 78) × 1.18 = 105.02 → 105
│
└─ Tooltip "Total" (current):
   └─ Average: (56 + 105) / 2 = 80.5 → 81
```

---

## 🎮 **How This Affects Cards**

### **Card Damage Calculation:**

```csharp
// In DamageCalculation.cs
float weaponDmg = character.weapons.GetWeaponDamage(WeaponType.Melee);

// WeaponSystem.cs
public float GetWeaponDamage()
{
    if (rolledBaseDamage > 0f)
        return rolledBaseDamage;  // Returns: 8 ✅
}
```

**Cards use ONLY the rolled base damage (8), not the total!**

---

## 💡 **The Disconnect**

### **Current State:**

```
Tooltip Shows:
├─ Base: 8 (rolled)
└─ Total: 81 (calculated from range)

Card Uses:
└─ 8 (just the rolled base, no affixes!)
```

**Cards DON'T use the "Total" damage!**

They use:
- Rolled base weapon damage (8)
- Plus card's own affixes/modifiers
- Plus character stats

---

## 🤔 **Questions for Consideration**

### **Q1: Should "Total" include affixes?**

**Currently:** Total = Base Range + Affixes  
**Issue:** Doesn't reflect rolled base

**Option A:** Total = Rolled Base + Affixes  
**Option B:** Don't show "Total" at all (just show rolled base)  
**Option C:** Show both range and rolled base separately  

### **Q2: Should cards use weapon affixes?**

**Currently:** Cards only use rolled base damage (8)  
**Weapon affixes don't affect card damage!**

**Alternative:** Include weapon affixes in card damage  
```
Strike card: 10 base
+ Weapon rolled base: 8
+ Weapon affixes: +59 (average of 41-78)
= 77 damage

vs Current:
Strike card: 10 base + 8 weapon = 18 damage
```

---

## 🎯 **Summary**

**"Total" damage comes from:**

1. **Base damage** (6-11 range, NOT the rolled value)
2. **+ Added damage from affixes** (e.g., +41-78)
3. **× Increased damage %** (e.g., ×1.18)
4. **Averaged** (min+max)/2

**Currently displayed as:**
- Single number (81) derived from range calculation
- **Doesn't use the rolled base (8)**
- **Only for display, not used by cards**

---

## ⚠️ **Recommendation**

I can see a potential design issue here. You have three options:

### **Option 1: Keep Current (Simple)**
- Total shows theoretical range-based calculation
- Cards only use rolled base
- Simpler, but total is misleading

### **Option 2: Calculate Total from Rolled Base (Accurate)**
- Total = Rolled Base (8) + Affix average + % increases
- More accurate representation
- Shows what weapon actually deals

### **Option 3: Remove "Total" Display**
- Just show: "Damage: 8"
- Simpler, less confusing
- Players understand rolled value is what matters

**Which approach do you prefer?**


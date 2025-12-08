# Testing Affix Rolling - Debug Guide 🔍

**Date:** December 4, 2025  
**Purpose:** Verify that affixes are being rolled to single values

---

## 🧪 **Run the Test**

1. Right-click `SimpleItemGenerator` component in Inspector
2. Select: **"Test Weapon Rolling (4 Weapons - Direct)"**
3. **Check Console** for detailed output

---

## ✅ **What to Look For in Console**

### **Step 1: Verify Affixes Are Being Rolled**

Look for lines like:
```
[Dual-Range Roll] addedFireDamage: (3-6) to (7-10) → 4-9 → FINAL: 7
[Single-Range Roll] increasedLightningDamage: 8-14 → 11
```

**Check:**
- ✅ You see `[Dual-Range Roll]` or `[Single-Range Roll]` messages
- ✅ Each shows a FINAL value
- ❌ If you DON'T see these, affixes aren't being rolled

---

### **Step 2: Verify Tooltip Formatting**

Look for lines like:
```
[TooltipFormat] Formatting rolled affix 'Flaming' (isRolled: True, rolledValue: 7)
[TooltipFormat] Dual-range: 'ADDS (3-6) TO (7-10) FIRE DAMAGE' → 'ADDS 7 FIRE DAMAGE'
```

**Check:**
- ✅ isRolled: True
- ✅ rolledValue: Shows a number
- ✅ Description transformed from range to single value
- ❌ If isRolled: False, something is wrong with rolling

---

### **Step 3: Verify Detailed Affix Output**

Look for lines like:
```
Affix Details:
  PREFIX: Flaming: Adds 7 Fire Damage
    isRolled: True, isDualRange: True, rolledValue: 7
  SUFFIX: of Lightning: +11% Increased Lightning Damage
    isRolled: True, isDualRange: False, rolledValue: 11
```

**Check:**
- ✅ Each affix shows a single value (not a range)
- ✅ isRolled is True for all affixes
- ✅ rolledValue is a reasonable number

---

## 🔍 **Possible Issues & Solutions**

### **Issue 1: No Rolling Messages**

**Console Shows:**
```
⚔️ Weapon #1: Worn Hatchet
(No [Dual-Range Roll] or [Single-Range Roll] messages)
```

**Problem:** Affixes aren't being rolled at all  
**Solution:** Check that `GenerateRolledAffix()` is being called in `BaseItem.AddPrefix/AddSuffix`

---

### **Issue 2: isRolled is False**

**Console Shows:**
```
PREFIX: Flaming: ADDS (3-6) TO (7-10) FIRE DAMAGE
  isRolled: False, isDualRange: True, rolledValue: 0
```

**Problem:** Rolling logic isn't setting the isRolled flag  
**Solution:** Check `GenerateRolledAffix()` in ItemRarity.cs

---

### **Issue 3: Tooltip Shows Ranges**

**Console Shows:**
```
[TooltipFormat] Affix 'Flaming' has no rolled values, using original description
```

**Problem:** FormatAffix() isn't detecting rolled values  
**Solution:** Rolling is working, but the check is too strict

---

### **Issue 4: Regex Not Matching**

**Console Shows:**
```
[TooltipFormat] Dual-range: 'ADDS (3-6) TO (7-10) FIRE DAMAGE' → 'ADDS (3-6) TO (7-10) FIRE DAMAGE'
(Description unchanged!)
```

**Problem:** Regex pattern isn't matching the format  
**Solution:** Adjust regex pattern in TooltipFormattingUtils.cs

---

## 📊 **Expected Perfect Output**

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
TESTING WEAPON BASE DAMAGE ROLLING
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

✅ Loaded asset: Worn Hatchet (Base: 6-11)

[Dual-Range Roll] addedFireDamage: (3-6) to (7-10) → 4-9 → FINAL: 7
[Single-Range Roll] increasedLightningDamage: 8-14 → 11
[Dual-Range Roll] addedChaosDamage: (3-6) to (7-10) → 5-8 → FINAL: 6

⚔️ Weapon #1: Worn Hatchet
Base Damage Range: 6-11
✅ Rolled Base Damage: 8
✅ Rolled Total Damage: 21

Affix Details:
  PREFIX: Flaming: Adds 7 Fire Damage
    isRolled: True, isDualRange: True, rolledValue: 7
  
  SUFFIX: of Lightning: +11% Increased Lightning Damage
    isRolled: True, isDualRange: False, rolledValue: 11

[TooltipFormat] Formatting rolled affix 'Flaming' (isRolled: True, rolledValue: 7)
[TooltipFormat] Dual-range: 'ADDS (3-6) TO (7-10) FIRE DAMAGE' → 'ADDS 7 FIRE DAMAGE'

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
ADDING TO INVENTORY
✅ Added to inventory: Worn Hatchet (Rolled: 8)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

---

## 🎯 **What This Proves**

If you see the expected output:
1. ✅ Affixes ARE being rolled
2. ✅ Rolling sets isRolled flag
3. ✅ Tooltip formatting IS working
4. ✅ Descriptions show single values

---

## 📝 **Next Steps After Test**

Based on the console output, we can:

1. **If rolling works but tooltips don't update:**
   - The issue is in UI refresh
   - Might need to regenerate tooltip when item changes

2. **If rolling doesn't work:**
   - Issue is in GenerateRolledAffix()
   - Need to check affix database setup

3. **If everything works in console but not in UI:**
   - Different tooltip component being used
   - Need to find and update that component

---

**Run the test now and share the console output!** 

The debug logs will tell us exactly what's happening. 🔍


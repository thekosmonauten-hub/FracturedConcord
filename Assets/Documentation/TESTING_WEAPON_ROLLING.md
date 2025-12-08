# Testing Weapon Base Damage Rolling 🎲

**Date:** December 4, 2025  
**Purpose:** Test the new weapon base damage rolling system

---

## 🎯 **Quick Test Guide (No Loot Tables Required!)**

### **Step 1: Add Test Component**

1. **Open Unity Editor**
2. **In Hierarchy**, create a new GameObject:
   - Right-click → Create Empty
   - Name it: `WeaponRollingTester`
3. **Add the SimpleItemGenerator component:**
   - Select `WeaponRollingTester` GameObject
   - In Inspector → Add Component
   - Search for: `SimpleItemGenerator`
   - Click to add

### **Step 2: Run Test**

4. **Right-click the component** in Inspector and select:
   - **"Test Weapon Rolling (4 Weapons - Direct)"**

**That's it!** The test will:
- ✅ Load Worn Hatchet directly from Resources
- ✅ Create 4 weapon instances
- ✅ Roll base damage for each (6-11 range)
- ✅ Show unique rolled values
- ✅ Works without loot tables configured

---

## ✅ **Expected Output:**

```
✅ Loaded asset: Worn Hatchet (Base: 6-11)

⚔️ Weapon #1: Worn Hatchet
Base Damage Range: 6-11
✅ Rolled Base Damage: 7.23 (for card scaling)
Final Damage: 47-89

⚔️ Weapon #2: Worn Hatchet
Base Damage Range: 6-11
✅ Rolled Base Damage: 9.87 (for card scaling)
Final Damage: 49-91

⚔️ Weapon #3: Worn Hatchet
Base Damage Range: 6-11
✅ Rolled Base Damage: 6.45 (for card scaling)
Final Damage: 46-88

⚔️ Weapon #4: Worn Hatchet
Base Damage Range: 6-11
✅ Rolled Base Damage: 10.12 (for card scaling)
Final Damage: 50-92

ROLLING TEST COMPLETE
4 weapons generated with unique rolled values!
```

---

## 🔍 **Verification Checklist**

### **✅ Each Weapon Should Have:**

1. **Same Base Range**
   - All Worn Hatchets: `6-11` (from asset)
   - ✅ This should be identical

2. **Different Rolled Values**
   - Each drop: Different `rolledBaseDamage` value
   - ✅ Should vary between 6.0 and 11.0

3. **Rolled Value Within Range**
   - `rolledBaseDamage` should be ≥ `minDamage` (6)
   - `rolledBaseDamage` should be ≤ `maxDamage` (11)
   - ✅ All values should be in range

4. **No Duplicates (Usually)**
   - 4 weapons should have 4 different rolled values
   - ✅ Very unlikely to get exact duplicates

5. **Validation in Console**
   - Each weapon shows ✅ or ❌ status
   - ✅ marks valid rolls
   - ❌ would indicate out-of-range (shouldn't happen)

---

## 🔧 **Troubleshooting**

### **Problem: "Could not load Worn Hatchet asset"**

**Solution:**
1. Make sure the asset exists at: `Assets/Resources/Items/Weapons/Axes/OneHanded/Worn Hatchet.asset`
2. The folder must be named **"Resources"** for Resources.Load() to work
3. Check the file name matches exactly (case-sensitive)
4. Verify the asset isn't corrupted

**Quick Fix:** Change the path in SimpleItemGenerator.cs line 320:
```csharp
WeaponItem asset = Resources.Load<WeaponItem>("Items/Weapons/Axes/OneHanded/Worn Hatchet");
// Update this path to match your actual Resources folder structure
```

### **Problem: All weapons show "NOT SET" (0.00)**

**Solution:**
1. Check `CreateWeaponWithRolling()` in SimpleItemGenerator.cs
2. Make sure this line exists:
   ```csharp
   copy.rolledBaseDamage = Random.Range(original.minDamage, original.maxDamage + 0.01f);
   ```
3. Check Console for errors during generation

### **Problem: All weapons have same rolled value**

**Solution:**
1. This is extremely unlikely (1 in millions)
2. If it happens, check that Random.Range() is being called each iteration
3. Try regenerating

### **Problem: Rolled value outside range (❌ OUT OF RANGE!)**

**Solution:**
1. Check the Random.Range() call
2. Should be: `Random.Range(original.minDamage, original.maxDamage + 0.01f)`
3. The +0.01f makes maxDamage inclusive

---

## 📝 **Alternative: Test with Loot Tables (If Configured)**

If you have loot tables set up, you can use the original method:

1. Configure settings:
   - Test Area Level: Use a level with a configured loot table
   - Force Item Type: ☑ Weapon
   - Force Rarity: ☑ Rare

2. Right-click → "Generate Items (Respects Inspector Settings)"

---

## 🎯 **Success Criteria**

✅ **All 4 weapons generated**  
✅ **Each has different `rolledBaseDamage`**  
✅ **All values within 6-11 range**  
✅ **Console shows "✅ Rolled Base Damage"**  
✅ **No "⚠️ NOT SET" warnings**  

**If all checkboxes pass, rolling is working correctly!** 🎉

---

## 📊 **What This Proves**

When the test succeeds, it confirms:
1. Weapons are being created properly
2. Base damage is being rolled on generation
3. Each weapon instance gets a unique rolled value
4. Values are within the expected range
5. The system is ready for card damage integration

---

**Ready to test!** Just right-click the component and select "Test Weapon Rolling (4 Weapons - Direct)" 🎲

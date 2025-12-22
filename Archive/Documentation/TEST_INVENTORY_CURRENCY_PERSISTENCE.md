# Testing Guide: Inventory & Currency Persistence 🧪

**Date:** December 4, 2025  
**Status:** Ready to Test

---

## 🎯 **Test Objectives**

Verify that:
1. ✅ Currency drops persist between sessions
2. ✅ Inventory items persist between sessions
3. ✅ Rolled item values (damage, affixes) persist correctly
4. ✅ Equipment Screen displays correct currency/inventory on load

---

## 🧪 **Test Procedure**

### **Test 1: Currency Persistence**

1. **Start Fresh:**
   - Load your character
   - Note current currency amounts (or screenshot)

2. **Collect Currency:**
   - Play 1-2 encounters
   - Defeat enemies and collect currency drops
   - Open Equipment Screen
   - Verify currency counts increased

3. **Exit and Reload:**
   - Close the game completely
   - Restart Unity/game
   - Load the same character

4. **Verify:**
   - ✅ Currency amounts should match what you had before exiting
   - ✅ All collected currencies should be present
   - Check console for: `[Character] Loaded X currency types from CharacterData`

---

### **Test 2: Inventory Persistence**

1. **Start Fresh:**
   - Load your character
   - Note current inventory count

2. **Collect Items:**
   - Play encounters
   - Collect weapon/armor drops
   - Open Equipment Screen → Check inventory
   - Note: Item names, damage values, affixes

3. **Exit and Reload:**
   - Close the game
   - Restart
   - Load character

4. **Verify:**
   - ✅ All inventory items should be present
   - ✅ Item damage rolls should match (e.g., if weapon had 71 damage, should still be 71)
   - ✅ Affix rolls should match (e.g., "+13% Fire Damage")
   - ✅ Item colors should match (normal/magic/rare)
   - Check console for: `[Character] Loaded X inventory items from CharacterData`

---

### **Test 3: Rolled Values Persistence**

1. **Generate Test Item:**
   - Use `SimpleItemGenerator` context menu if available
   - OR collect item naturally from combat

2. **Record Values:**
   - Hover item to see tooltip
   - Write down:
     - Base damage (e.g., "Dmg: 71")
     - Each affix rolled value (e.g., "Adds 63 Physical Damage")
     - Attack speed, crit chance, etc.

3. **Save and Reload:**
   - Exit game
   - Restart
   - Load character

4. **Verify:**
   - ✅ Hover same item
   - ✅ ALL values should match exactly
   - ✅ ALT-key view should show correct ranges

---

### **Test 4: Multiple Characters**

1. **Character A:**
   - Load Character A
   - Collect currency and items
   - Exit

2. **Character B:**
   - Load Character B (different character)
   - Verify currencies start at 0 (not inherited from Character A)
   - Verify inventory is separate

3. **Back to Character A:**
   - Load Character A again
   - ✅ Should have the same currency/items as before
   - ✅ Should NOT have Character B's items

---

## 📊 **Expected Console Output**

### **On Save:**
```
[Character] Saved 30 currency types to CharacterData
[Character] Saved 15 inventory items to CharacterData
[CharacterManager] Saved character: YourCharacterName
```

### **On Load:**
```
[LootManager] Cleared all currencies
[Character] Loaded 30 currency types from CharacterData
[LootManager] Set FireSpirit to 15
[LootManager] Set ColdSpirit to 23
... (more currencies)
[Character] Loaded 15 inventory items from CharacterData
[CharacterManager] Loaded character: YourCharacterName
```

---

## ⚠️ **Troubleshooting**

### **Currency Shows 0 After Reload**

**Check:**
- Console for `[Character] Loaded X currency types`
- If 0 types loaded → Save file may not have currency data (old save)
- Try collecting new currency and saving again

**Fix:**
- Collect currency in a new encounter
- Exit Equipment Screen (triggers auto-save)
- Reload character

---

### **Inventory Empty After Reload**

**Check:**
- Console for `[Character] Loaded X inventory items`
- If 0 items loaded → Items may not have been saved

**Fix:**
- Collect new items
- Open Equipment Screen (triggers refresh/save)
- Exit game properly (don't force quit)
- Reload

---

### **Item Values Wrong After Reload**

**Check:**
- Hover item before save → Write down exact values
- Reload character
- Hover same item → Compare values
- If different → Serialization issue with rolled values

**Report:**
- Which item type (weapon/armor)
- What values changed
- Console logs during save/load

---

## 🐛 **Known Issues to Watch For**

1. **First Load After Update:**
   - Old save files won't have inventory/currency fields
   - This is normal - collect new items/currency to populate

2. **Item Blueprint Loss:**
   - Items are recreated as runtime instances
   - May lose reference to original ScriptableObject
   - Doesn't affect gameplay, just editor references

3. **Large Save Files:**
   - With many items, save file may grow large
   - This is normal for now
   - Future optimization may compress data

---

## ✅ **Success Criteria**

The fix is successful if:
- ✅ Currency persists across sessions
- ✅ Inventory persists across sessions
- ✅ Rolled item values persist correctly
- ✅ No console errors during save/load
- ✅ Multiple characters have separate inventories
- ✅ Equipment Screen displays correct data immediately on load

---

## 📝 **Test Results Template**

```
Test Date: _____________
Character Name: _____________

Currency Before Exit: _____________
Currency After Reload: _____________
Match: ☐ Yes ☐ No

Inventory Count Before: _____
Inventory Count After: _____
Match: ☐ Yes ☐ No

Item Values Match: ☐ Yes ☐ No
Console Errors: ☐ Yes ☐ No

Notes:
_________________________________
_________________________________
```

---

**Happy Testing! 🎮**

If any test fails, check the console logs and compare with expected output above!



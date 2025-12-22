# Inventory & Currency Save Issues - Analysis & Fix 🔧

**Date:** December 4, 2025  
**Status:** ⚠️ **Critical Issues Identified**

---

## 🚨 **Issues Found**

### **Issue 1: Inventory Not Saving/Loading**

**Problem:**
- `CharacterManager.inventoryItems` is only stored in memory
- `CharacterData` class (line 214 in `CharacterSelectionUI.cs`) has **no inventory field**
- `Character.ToCharacterData()` does **not save inventory items**
- When loading a character, inventory is **never restored**

**Evidence:**
```csharp
// CharacterData.cs - NO inventory field!
public class CharacterData
{
    public string characterName;
    public int level;
    // ... other fields ...
    
    // ❌ NO inventory field!
    // ❌ NO currency field!
}
```

**Result:** All inventory items lost when exiting game!

---

### **Issue 2: Currency Not Saving to Character**

**Problem:**
- `LootManager.SavePlayerCurrencies()` (line 389) says **"TODO: Save to persistent storage"**
- Currencies are stored in `LootManager.playerCurrencies` dictionary (memory only)
- **NOT connected to Character object**
- **NOT persisted to disk**

**Evidence:**
```csharp
// LootManager.cs line 389
private void SavePlayerCurrencies()
{
    // TODO: Save to persistent storage
    // For now, just keep in memory  ❌
    Debug.Log($"[LootManager] Saved {playerCurrencies.Count} currency types");
}
```

**Result:** All currency drops lost when exiting game!

---

## 🛠️ **Solution Plan**

### **Step 1: Extend CharacterData**

Add inventory and currency fields:
```csharp
public class CharacterData
{
    // ... existing fields ...
    
    // NEW: Inventory
    public List<string> inventoryItemIds = new List<string>();
    public List<string> inventoryItemData = new List<string>(); // JSON serialized
    
    // NEW: Currencies
    public List<string> currencyTypes = new List<string>();
    public List<int> currencyAmounts = new List<int>();
}
```

### **Step 2: Update Character.ToCharacterData()**

Save inventory and currencies:
```csharp
public CharacterData ToCharacterData()
{
    var data = new CharacterData(characterName, characterClass, level, act);
    
    // ... existing fields ...
    
    // NEW: Save inventory
    var charManager = CharacterManager.Instance;
    if (charManager != null)
    {
        foreach (var item in charManager.inventoryItems)
        {
            // Serialize item to JSON
            string itemJson = JsonUtility.ToJson(item);
            data.inventoryItemData.Add(itemJson);
        }
    }
    
    // NEW: Save currencies
    var lootManager = LootManager.Instance;
    if (lootManager != null)
    {
        var currencies = lootManager.GetAllCurrencies();
        foreach (var kvp in currencies)
        {
            data.currencyTypes.Add(kvp.Key.ToString());
            data.currencyAmounts.Add(kvp.Value);
        }
    }
    
    return data;
}
```

### **Step 3: Update Character.FromCharacterData()**

Restore inventory and currencies:
```csharp
public static Character FromCharacterData(CharacterData data)
{
    Character character = new Character(data.characterName, data.characterClass);
    
    // ... existing fields ...
    
    // NEW: Restore inventory
    var charManager = CharacterManager.Instance;
    if (charManager != null && data.inventoryItemData != null)
    {
        charManager.inventoryItems.Clear();
        foreach (string itemJson in data.inventoryItemData)
        {
            // Deserialize item from JSON
            // (Need to determine item type first)
            BaseItem item = JsonUtility.FromJson<BaseItem>(itemJson);
            charManager.inventoryItems.Add(item);
        }
    }
    
    // NEW: Restore currencies
    var lootManager = LootManager.Instance;
    if (lootManager != null && data.currencyTypes != null)
    {
        for (int i = 0; i < data.currencyTypes.Count; i++)
        {
            CurrencyType type = (CurrencyType)System.Enum.Parse(typeof(CurrencyType), data.currencyTypes[i]);
            int amount = data.currencyAmounts[i];
            lootManager.SetCurrency(type, amount);
        }
    }
    
    return character;
}
```

### **Step 4: Fix LootManager.SavePlayerCurrencies()**

Actually save to CharacterData:
```csharp
private void SavePlayerCurrencies()
{
    // Currencies are now saved as part of CharacterData
    // when CharacterManager.SaveCharacter() is called
    Debug.Log($"[LootManager] Currencies will be saved with character");
}
```

---

## ⚠️ **Challenges**

### **Challenge 1: BaseItem Serialization**
- `BaseItem` is a `ScriptableObject` (can't serialize directly with JsonUtility)
- Need custom serialization for items
- Solution: Create `ItemData` serialization wrapper

### **Challenge 2: Item Type Detection**
- Need to know if item is `WeaponItem`, `ArmourItem`, `Effigy`, etc.
- Solution: Add `itemType` field to serialized data

### **Challenge 3: Affix Rolling Persistence**
- Rolled affixes need to be saved (not just blueprints)
- Solution: Serialize `rolledBaseDamage`, `rolledValue`, etc.

---

## 🎯 **Implementation Priority**

1. **HIGH**: Fix currency saving (simpler, no serialization issues)
2. **HIGH**: Add basic inventory persistence
3. **MEDIUM**: Handle complex item serialization (affixes, rolled values)
4. **LOW**: Optimize serialization size

---

**Status:** Ready to implement fixes! 🚀



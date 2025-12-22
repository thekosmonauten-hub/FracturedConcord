# Inventory & Stash Separation - COMPLETE ✅

**Date:** December 5, 2025  
**Status:** ✅ **READY TO USE**

---

## 🎯 **What Was Implemented**

Separated **Inventory** (character-specific) from **Stash** (global, shared across all characters):

### **Inventory (Character-Specific)**
- Stored in `CharacterManager.inventoryItems`
- Saved with character data
- Each character has their own inventory
- Items drop here after combat

### **Stash (Global)**
- Stored in `StashManager.stashItems`
- Saved separately (PlayerPrefs)
- Shared across ALL characters
- Manual transfer only (no auto-drops)

---

## 📁 **Files Created/Modified**

### **1. StashManager.cs** (NEW)
`Assets/Scripts/Data/StashManager.cs`

**Features:**
- Singleton pattern (DontDestroyOnLoad)
- Global stash storage (`List<BaseItem>`)
- Save/Load to PlayerPrefs
- Events: `OnItemAdded`, `OnItemRemoved`, `OnStashChanged`
- Methods: `AddItem()`, `RemoveItem()`, `SwapItems()`, etc.

**Save Location:** PlayerPrefs key `"GlobalStash"`

---

### **2. InventoryGridUI.cs** (MODIFIED)
`Assets/Scripts/UI/EquipmentScreen/UnityUI/InventoryGridUI.cs`

**Added:**
- `GridMode` enum: `CharacterInventory` | `GlobalStash`
- `gridMode` public field (set in Inspector)
- `GetItemList()` - Returns appropriate list based on mode
- `RefreshFromDataSource()` - Works for both modes
- `TryBindStashManager()` - Binds to StashManager for stash mode
- Event subscriptions for both managers

**Changed:**
- All methods now check `gridMode` to use correct data source
- `RefreshFromCharacterInventory()` → `RefreshFromDataSource()`
- Equipment drag only works from inventory (not stash)

---

### **3. EquipmentScreenUI.cs** (MODIFIED)
`Assets/Scripts/UI/EquipmentScreen/UnityUI/EquipmentScreenUI.cs`

**Added:**
- `ConfigureGrids()` - Sets grid modes on Start
- `TransferToStash()` - Move item from inventory → stash
- `TransferToInventory()` - Move item from stash → inventory
- `RefreshAllDisplays()` - Now refreshes both grids

**Fields:**
- `inventoryGrid` - Set to `GridMode.CharacterInventory`
- `stashGrid` - Set to `GridMode.GlobalStash`

---

## 🎮 **Setup in Unity**

### **Step 1: Configure Grid Modes**

1. Open `EquipmentScreen_New` scene/prefab
2. Find `inventoryGrid` GameObject
3. Inspector → `InventoryGridUI` component
4. Set **Grid Mode** to `Character Inventory`
5. Find `stashGrid` GameObject
6. Inspector → `InventoryGridUI` component
7. Set **Grid Mode** to `Global Stash`
8. ✅ Save scene/prefab

**Note:** `ConfigureGrids()` in `EquipmentScreenUI` will auto-set these on Start, but setting in Inspector is recommended.

---

### **Step 2: Verify StashManager Exists**

`StashManager` is created automatically as a singleton. It will:
- Auto-create on first access
- Persist across scenes (DontDestroyOnLoad)
- Auto-load stash on Awake
- Auto-save on changes

**No setup required!** ✅

---

## 🔄 **How It Works**

### **Item Flow:**

**Combat Drops:**
```
Combat → LootManager.ApplyRewards()
  ↓
HandleItemReward()
  ↓
CharacterManager.AddItem() → inventoryItems ✅
  ↓
Inventory Grid displays it
```

**Stash Transfer (Manual):**
```
Player clicks "Move to Stash" button
  ↓
EquipmentScreenUI.TransferToStash()
  ↓
Remove from CharacterManager.inventoryItems
  ↓
Add to StashManager.stashItems
  ↓
Stash Grid displays it
```

**Inventory Transfer (Manual):**
```
Player clicks "Move to Inventory" button
  ↓
EquipmentScreenUI.TransferToInventory()
  ↓
Remove from StashManager.stashItems
  ↓
Add to CharacterManager.inventoryItems
  ↓
Inventory Grid displays it
```

---

## 💾 **Save/Load System**

### **Inventory (Character-Specific):**
- Saved in `CharacterData.inventoryItems`
- Loaded via `Character.FromCharacterData()`
- Each character has separate save file

### **Stash (Global):**
- Saved in PlayerPrefs key `"GlobalStash"`
- Loaded on `StashManager.Awake()`
- Shared across all characters
- Persists even when switching characters

---

## 🎨 **UI Integration**

### **Current Setup:**
- **Inventory Grid** → Shows `CharacterManager.inventoryItems`
- **Stash Grid** → Shows `StashManager.stashItems`
- Both use same `InventoryGridUI` script
- Differentiated by `gridMode` field

### **Visual Distinction:**
Consider adding visual indicators:
- Different background colors
- Labels ("Inventory" vs "Stash")
- Different grid sizes
- Transfer buttons between grids

---

## 🔧 **Transfer Methods**

### **Transfer to Stash:**
```csharp
EquipmentScreenUI.TransferToStash(item, inventoryIndex);
```

### **Transfer to Inventory:**
```csharp
EquipmentScreenUI.TransferToInventory(item, stashIndex);
```

**Note:** These methods are public and can be called from UI buttons.

---

## 🧪 **Testing**

### **Test Inventory (Character-Specific):**
1. Play combat encounter
2. Collect item drops
3. Open Equipment Screen
4. ✅ Items appear in **Inventory Grid**
5. Switch to different character
6. ✅ Different character has different inventory

### **Test Stash (Global):**
1. Open Equipment Screen
2. Manually add item to stash (via transfer method or button)
3. ✅ Item appears in **Stash Grid**
4. Switch to different character
5. ✅ Same stash items visible (global!)

### **Test Persistence:**
1. Add items to stash
2. Exit game completely
3. Restart game
4. Load any character
5. ✅ Stash items persist!

---

## ⚠️ **Important Notes**

### **Item Drops Always Go to Inventory:**
- Combat drops → `CharacterManager.inventoryItems`
- **NOT** to stash
- Stash is manual transfer only

### **Equipment Only from Inventory:**
- Can only equip items from inventory grid
- Stash items must be transferred to inventory first
- Prevents accidental equipping from shared stash

### **Stash is Global:**
- All characters share the same stash
- Useful for transferring items between characters
- Stored separately from character data

---

## 📊 **Data Structure**

### **Inventory (Per Character):**
```
CharacterData
  └─ inventoryItems: List<SerializedItemData>
      └─ Saved in character JSON file
```

### **Stash (Global):**
```
PlayerPrefs["GlobalStash"]
  └─ JSON string
      └─ StashSaveData
          └─ items: List<SerializedItemData>
```

---

## 🐛 **Troubleshooting**

### **Both Grids Show Same Items:**
- ✅ Check `gridMode` is set correctly in Inspector
- ✅ Check `ConfigureGrids()` is being called
- ✅ Check console for `"[EquipmentScreenUI] Configured..."` messages

### **Stash Not Persisting:**
- ✅ Check `StashManager` exists in scene
- ✅ Check console for `"[StashManager] Saved..."` messages
- ✅ Verify PlayerPrefs are being saved

### **Items Not Appearing:**
- ✅ Check `RefreshFromDataSource()` is being called
- ✅ Check appropriate manager (CharacterManager/StashManager) exists
- ✅ Check console for warnings

---

## ✅ **Status: COMPLETE**

**No linter errors!** 🎯

Inventory and Stash are now fully separated:
- ✅ Inventory = Character-specific
- ✅ Stash = Global
- ✅ No duplication
- ✅ Proper save/load
- ✅ Ready to use!

---

## 🚀 **Next Steps (Optional)**

### **Add Transfer Buttons:**
Create UI buttons to transfer items between inventory and stash:
- "Move to Stash" button on inventory items
- "Move to Inventory" button on stash items
- Call `TransferToStash()` / `TransferToInventory()`

### **Visual Indicators:**
- Add labels above grids ("Inventory" / "Stash")
- Different background colors
- Different grid sizes

### **Stash Size Limits:**
- Add max stash size (e.g., 200 items)
- Show capacity indicator
- Warn when full

---

**Ready to test!** 🎮

Just configure the grid modes in Unity Inspector and you're good to go! 🚀


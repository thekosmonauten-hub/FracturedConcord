# Inventory Transfer Analysis

## Summary
This document analyzes how dropped/purchased items are transferred to PlayerInventory and whether they appear in EquipmentScreen's inventory grid.

## Key Findings

### ✅ Item Transfer Flow (Working Correctly)

**1. CombatScene - Dropped Items:**
- Items are dropped via `EnemyLootDropper.ApplyImmediateDrops()`
- This calls `LootManager.ApplyRewards()`
- Items are added to `CharacterManager.inventoryItems` via `CharacterManager.AddItem()`

**2. MazeScene - Dropped Items:**
- Same flow as CombatScene (uses LootManager)

**3. MazeHub - Purchased Items:**
- Vendor purchases should use the same `CharacterManager.AddItem()` method
- Need to verify vendor implementation

**Flow Diagram:**
```
Combat/Maze Drop → EnemyLootDropper → LootManager.ApplyRewards() 
→ HandleItemReward() → CharacterManager.AddItem() 
→ CharacterManager.inventoryItems (List<BaseItem>)
```

### ❌ EquipmentScreen Display Issue (NOT Working)

**Problem:**
- `EquipmentScreen` maintains its own `inventoryItems` list (type: `List<ItemData>`)
- This list is **NOT** synchronized with `CharacterManager.inventoryItems` (type: `List<BaseItem>`)
- EquipmentScreen only loads test items via `AddTestItems()` method
- There is **NO** method that loads items from `CharacterManager.inventoryItems`

**Current State:**
- EquipmentScreen's `Start()` method calls:
  1. `InitializeUI()` - Sets up UI elements
  2. `GenerateInventoryGrid()` - Creates visual slots
  3. `AddTestItems()` - Adds test items (NOT from CharacterManager)
  4. `UpdateInventoryDisplay()` - Displays items

**Missing Functionality:**
- No `LoadInventoryFromCharacterManager()` method
- No conversion from `BaseItem` to `ItemData`
- No subscription to `CharacterManager.OnItemAdded` event

## Code References

### Item Transfer (Working)
```149:173:Assets/Scripts/LootSystem/LootManager.cs
private void HandleItemReward(Character character, LootReward reward)
{
    if (character == null || reward == null)
        return;

    BaseItem baseItem = GetBaseItemFromReward(reward);

    if (baseItem == null)
    {
        Debug.LogWarning($"[LootManager] Unable to resolve item reward '{reward.itemData?.itemName}' to a BaseItem. Skipping.");
        return;
    }

    CharacterManager charManager = CharacterManager.Instance;
    if (charManager != null)
    {
        // Clone to ensure unique instance in inventory
        BaseItem itemInstance = ScriptableObject.Instantiate(baseItem);
        charManager.AddItem(itemInstance);
    }
    else
    {
        Debug.LogWarning("[LootManager] CharacterManager instance not available when applying item rewards.");
    }
}
```

```284:291:Assets/Scripts/Data/CharacterManager.cs
// Add an item to the player's inventory and notify listeners
public void AddItem(BaseItem item)
{
    if (item == null) return;
    inventoryItems.Add(item);
    OnItemAdded?.Invoke(item);
    Debug.Log($"[Loot] Added item to inventory: {item.itemName} ({item.rarity})");
}
```

### EquipmentScreen Issue (Not Loading from CharacterManager)
```105:105:Assets/Scripts/UI/EquipmentScreen/EquipmentScreen.cs
private List<ItemData> inventoryItems = new List<ItemData>();
```

```28:29:Assets/Scripts/Data/CharacterManager.cs
[Header("Inventory")]
public List<BaseItem> inventoryItems = new List<BaseItem>();
```

```1155:1336:Assets/Scripts/UI/EquipmentScreen/EquipmentScreen.cs
private void AddTestItems()
{
    // Clear existing items
    inventoryItems.Clear();
    
    // Try to get items from the database
    if (ItemDatabase.Instance != null)
    {
        // ... adds test items, NOT from CharacterManager
    }
}
```

## Recommendations

### To Fix EquipmentScreen Display:

1. **Add LoadInventoryFromCharacterManager() method:**
   - Convert `BaseItem` to `ItemData`
   - Load from `CharacterManager.Instance.inventoryItems`
   - Call this in `Start()` or `OnEnable()`

2. **Subscribe to CharacterManager.OnItemAdded event:**
   - When new items are added, convert and add to EquipmentScreen's inventory
   - Update the display

3. **Create BaseItem to ItemData converter:**
   - Handle all item types (WeaponItem, Armour, Jewellery, Consumable)
   - Preserve all stats, modifiers, and properties

4. **Sync on screen open:**
   - Load inventory when EquipmentScreen is opened
   - Refresh display when returning to the screen

## Scenes Analyzed

1. **CombatScene** (`CombatSceneManager.cs`)
   - No direct item handling
   - Items handled via LootManager (working)

2. **MazeScene** (`MazeSceneController.cs`)
   - No direct item handling
   - Items handled via LootManager (working)

3. **MazeHub** (`MazeHubController.cs`)
   - Vendor panel exists but vendor purchase logic not analyzed
   - Should use same CharacterManager.AddItem() flow

## Conclusion

**Item Transfer:** ✅ Working correctly - items are added to `CharacterManager.inventoryItems`

**EquipmentScreen Display:** ❌ **NOT working** - EquipmentScreen does not load items from CharacterManager, only displays test items

**Action Required:** Implement inventory loading in EquipmentScreen to sync with CharacterManager's inventory.


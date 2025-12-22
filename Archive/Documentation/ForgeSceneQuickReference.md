# Forge Scene Quick Reference

## Quick Setup Checklist

### 1. Scene Setup
- [ ] Create `ForgeScene.unity`
- [ ] Add Canvas (Screen Space - Overlay)
- [ ] Create "Managers" GameObject

### 2. Add Components
- [ ] Add `ForgeManager` script to Managers GameObject
- [ ] Ensure `CharacterManager.Instance` exists (singleton)
- [ ] Ensure `StashManager.Instance` exists (singleton)

### 3. UI Setup
- [ ] Create Inventory Panel with `InventoryGridUI` component
- [ ] Create Stash Panel with `InventoryGridUI` component
- [ ] Set Inventory Grid Mode: `CharacterInventory`
- [ ] Set Stash Grid Mode: `GlobalStash`
- [ ] Assign slot prefab to both grids
- [ ] Assign grid container to both grids

### 4. Wire References
- [ ] Assign Inventory Grid to ForgeManager
- [ ] Assign Stash Grid to ForgeManager
- [ ] Assign Inventory Panel to ForgeManager
- [ ] Assign Stash Panel to ForgeManager
- [ ] Wire buttons (Toggle Inventory, Toggle Stash, Return to Town)

---

## Code Snippets

### Access Inventory Items
```csharp
var items = CharacterManager.Instance?.inventoryItems;
```

### Access Stash Items
```csharp
var items = StashManager.Instance?.stashItems;
```

### Salvage Item from Inventory
```csharp
BaseItem item = // ... get item
Character character = CharacterManager.Instance.GetCurrentCharacter();
ForgeSalvageSystem.SalvageItem(item, character);
// Remove from inventory
CharacterManager.Instance.inventoryItems.Remove(item);
```

### Salvage Item from Stash
```csharp
BaseItem item = // ... get item
Character character = CharacterManager.Instance.GetCurrentCharacter();
ForgeSalvageSystem.SalvageItem(item, character);
// Remove from stash
StashManager.Instance.RemoveItem(item);
```

### Refresh Grids After Changes
```csharp
if (inventoryGrid != null) inventoryGrid.RefreshFromDataSource();
if (stashGrid != null) stashGrid.RefreshFromDataSource();
```

### Get Material Quantities
```csharp
Character character = CharacterManager.Instance.GetCurrentCharacter();
int weaponScraps = ForgeMaterialManager.GetMaterialQuantity(character, ForgeMaterialType.WeaponScraps);
int armourScraps = ForgeMaterialManager.GetMaterialQuantity(character, ForgeMaterialType.ArmourScraps);
int effigySplinters = ForgeMaterialManager.GetMaterialQuantity(character, ForgeMaterialType.EffigySplinters);
int warrantShards = ForgeMaterialManager.GetMaterialQuantity(character, ForgeMaterialType.WarrantShards);
```

---

## Component Requirements

### InventoryGridUI Requirements:
- `slotPrefab`: Prefab for inventory slots
- `gridContainer`: Transform container for slots
- `gridWidth`: Number of columns (default: 10)
- `gridHeight`: Number of rows (default: 6)
- `cellSize`: Size of each slot (default: 60x60)
- `spacing`: Spacing between slots (default: 2x2)
- `gridMode`: CharacterInventory or GlobalStash

### ForgeManager Requirements:
- Inventory Grid reference
- Stash Grid reference
- Inventory Panel reference (optional)
- Stash Panel reference (optional)
- Button references (optional)

---

## Common Issues & Solutions

**Issue**: Inventory not showing items
- **Solution**: Check `CharacterManager.Instance.inventoryItems` has items
- **Solution**: Verify `gridMode == CharacterInventory`
- **Solution**: Check `slotPrefab` is assigned

**Issue**: Stash not showing items
- **Solution**: Check `StashManager.Instance.stashItems` has items
- **Solution**: Verify `gridMode == GlobalStash`
- **Solution**: Check `slotPrefab` is assigned

**Issue**: Grids not updating after changes
- **Solution**: Call `RefreshFromDataSource()` manually
- **Solution**: Check `refreshOnEnable` is true
- **Solution**: Verify event subscriptions in `OnEnable()`

---

## Scene Navigation

### Load Forge Scene
```csharp
SceneManager.LoadScene("ForgeScene");
```

### Return to Town
```csharp
SceneManager.LoadScene("MainGameUI");
```

---

## Integration with Town

Add Forge button to MainGameUI:
```csharp
// In MainGameNavController or similar
public Button ForgeButton;

void Start()
{
    if (ForgeButton != null)
    {
        ForgeButton.onClick.AddListener(() => 
        {
            SceneManager.LoadScene("ForgeScene");
        });
    }
}
```


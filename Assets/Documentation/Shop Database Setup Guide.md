# Shop Database Setup Guide

## Overview

The Shop Database system provides a configurable way to map shop IDs (used in dialogue actions) to their corresponding UI panels. This replaces the hardcoded shop opening logic with a flexible, database-driven approach.

## System Components

### 1. **ShopDatabase.cs** - ScriptableObject
   - Stores mappings between shop IDs and their UI panels
   - Supports multiple access methods for finding panels
   - Located in: `Assets/Scripts/Dialogue/ShopDatabase.cs`

### 2. **DialogueManager.cs** - Updated Shop Opening Logic
   - Now uses `ShopDatabase` instead of hardcoded switch statements
   - Supports multiple panel access methods
   - Falls back to legacy method for backward compatibility

### 3. **CreateShopDatabase.cs** - Editor Tool
   - Menu: **Tools → Dexiled → Create Shop Database**
   - Visual editor for creating and configuring shop mappings
   - Located in: `Assets/Editor/CreateShopDatabase.cs`

## Panel Access Methods

The database supports four different ways to find and open shop panels:

### 1. **MazeHubController** (Recommended for Maze shops)
   - Accesses panel through `MazeHubController` component
   - Specify the field name (e.g., `vendorPanel`, `forgePanel`)
   - Example: `panelFieldName = "vendorPanel"`
   - **Best for**: Shops managed by `MazeHubController`

### 2. **DirectPanel**
   - Direct GameObject reference (set in Inspector)
   - Panel must be assigned directly in the database asset
   - **Best for**: Simple shops with static panel references

### 3. **ComponentType**
   - Finds component by full type name (e.g., `Dexiled.MazeSystem.MazeVendorUI`)
   - Activates the GameObject that has this component
   - **Best for**: Shops with dedicated component scripts

### 4. **GameObjectName**
   - Searches for GameObject by name
   - Supports multiple name options (tries each until one is found)
   - Example: `["VendorPanel", "MazeVendorPanel", "BlinketShopPanel"]`
   - **Best for**: Simple shops with known GameObject names

## Setup Instructions

### Step 1: Create Shop Database Asset

1. Open Unity Editor
2. Go to menu: **Tools → Dexiled → Create Shop Database**
3. Click **"Create New"** button
4. Choose save location (recommended: `Assets/Resources/ShopDatabase.asset`)
5. The database will be created with default mappings

### Step 2: Configure Shop Mappings

1. In the **Shop Database Creator** window, you'll see the list of shop mappings
2. Click **"Add Default Shops"** to add the Maze Vendor mapping
3. Configure each shop mapping:

   **For Maze Vendor (Blinket):**
   - **Shop ID**: `MazeVendor` (or `Blinket`, `Vendor` - all work)
   - **Shop Name**: `Blinket's Maze Vendor`
   - **Access Method**: `MazeHubController`
   - **Panel Field Name**: `vendorPanel`
   - **GameObject Names** (optional fallback): `VendorPanel`, `MazeVendorPanel`, `BlinketShopPanel`

4. Click **"Save"** to save your changes

### Step 3: Assign Shop Database to DialogueManager

1. Find or create `DialogueManager` in your scene
2. In the Inspector, find the **"Shop System References"** section
3. Assign the `ShopDatabase.asset` to the **Shop Database** field
4. Alternatively, place the database in `Assets/Resources/` folder - it will auto-load

### Step 4: Verify Dialogue Action

1. Open your dialogue asset (e.g., `Blinket_Dialogue.asset`)
2. Find the `open_shop` node
3. Ensure it has an `OpenShop` action with value: `MazeVendor`
4. The shop ID should match what's in your `ShopDatabase`

## Adding New Shops

To add a new shop to the database:

1. Open the **Shop Database Creator** tool
2. Click **"Add Empty Mapping"**
3. Fill in the details:
   - **Shop ID**: Unique identifier (used in dialogue actions)
   - **Shop Name**: Display name (for reference)
   - **Access Method**: Choose how to find the panel
   - Configure fields based on the access method

4. Click **"Save"**

## Example Configurations

### Example 1: Maze Vendor (MazeHubController)
```
Shop ID: MazeVendor
Shop Name: Blinket's Maze Vendor
Access Method: MazeHubController
Panel Field Name: vendorPanel
```

### Example 2: Direct Panel Reference
```
Shop ID: TownBlacksmith
Shop Name: Town Blacksmith Shop
Access Method: DirectPanel
Panel GameObject: (Assign directly in Inspector)
```

### Example 3: Component-Based Shop
```
Shop ID: WeaponShop
Shop Name: Weapon Merchant
Access Method: ComponentType
Component Type Name: Dexiled.Shops.WeaponShopUI
```

### Example 4: Name-Based Shop
```
Shop ID: PotionShop
Shop Name: Alchemist's Shop
Access Method: GameObjectName
GameObject Names: ["PotionShopPanel", "AlchemistPanel", "PotionVendor"]
```

## How It Works

### Flow Diagram

```
Dialogue Action (OpenShop, "MazeVendor")
    ↓
DialogueManager.OpenShop("MazeVendor")
    ↓
Load ShopDatabase (auto-find if not assigned)
    ↓
GetShopMapping("MazeVendor")
    ↓
Open shop based on accessMethod:
    - MazeHubController → Access hubController.vendorPanel
    - DirectPanel → Activate panelGameObject
    - ComponentType → Find component, activate GameObject
    - GameObjectName → Find by name, activate
    ↓
Shop Panel Opens
```

### Fallback System

The system includes fallbacks for backward compatibility:

1. **Primary**: Use `ShopDatabase` mapping
2. **Fallback 1**: Try legacy method for known shop IDs (`MazeVendor`, `Blinket`, `Vendor`)
3. **Fallback 2**: Search by GameObject names
4. **Final**: Log warning if nothing works

## Troubleshooting

### Shop Not Opening

**Check Console Logs:**
- Look for `[DialogueManager] Opening shop: [ID]`
- Check for warnings about missing database or mappings
- Verify panel access method logs

**Common Issues:**

1. **ShopDatabase not found**
   - Solution: Assign it in `DialogueManager` Inspector
   - Or place database at `Assets/Resources/ShopDatabase.asset`

2. **Shop ID not in database**
   - Solution: Add the shop mapping using the editor tool
   - Check that shop ID matches exactly (case-insensitive)

3. **Panel field not found (MazeHubController method)**
   - Solution: Verify `panelFieldName` matches the field name in `MazeHubController`
   - Field must be public (or use reflection)

4. **Panel is null**
   - Solution: Assign the panel GameObject in the scene
   - For `MazeHubController`, assign `vendorPanel` in Inspector

5. **Component type not found**
   - Solution: Verify `componentTypeName` is correct
   - Use fully qualified name (e.g., `Dexiled.MazeSystem.MazeVendorUI`)

### Debugging Tips

1. **Enable detailed logging**: The system logs at each step
2. **Check mapping configuration**: Open `ShopDatabase.asset` and verify settings
3. **Test panel access manually**: Try accessing the panel directly to verify it exists
4. **Verify scene setup**: Ensure all required components are in the scene

## Migration from Legacy System

If you were using the old hardcoded system:

1. Create `ShopDatabase.asset` using the editor tool
2. Add your existing shops to the database
3. Assign the database to `DialogueManager`
4. The legacy fallback will still work for `MazeVendor`, `Blinket`, and `Vendor` IDs

## Benefits

✅ **No Code Changes**: Add new shops without modifying `DialogueManager`  
✅ **Flexible Access**: Multiple ways to find panels  
✅ **Visual Configuration**: Editor tool for easy setup  
✅ **Backward Compatible**: Legacy system still works  
✅ **Error Handling**: Comprehensive logging and fallbacks  

## Files Created/Modified

### Created Files:
- `Assets/Scripts/Dialogue/ShopDatabase.cs` - Shop mapping database
- `Assets/Editor/CreateShopDatabase.cs` - Editor tool for managing database
- `Assets/Documentation/Shop Database Setup Guide.md` - This guide

### Modified Files:
- `Assets/Scripts/Dialogue/DialogueManager.cs` - Updated to use `ShopDatabase`

## Next Steps

1. **Create the database asset** using the editor tool
2. **Configure the Maze Vendor mapping** (or use defaults)
3. **Assign to DialogueManager** in your scene
4. **Test the dialogue** to verify the shop opens correctly
5. **Add more shops** as needed using the same system


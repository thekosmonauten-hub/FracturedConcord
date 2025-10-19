# Passive Tree JSON Data Editor - Node Name Generation Fix

## ✅ **Fixed: Node Name Generation Issue!**

The issue where node names were being appended instead of replaced has been resolved. The system now properly generates clean, descriptive node names based on applied stats.

## 🐛 **What Was the Problem?**

**Before (Broken):**
- Cell names: `"Cell_5_6_increased Evasion - Cell_5_6_Max Health"`
- **Issue**: Names were being appended instead of replaced
- **Cause**: Automatic `UpdateCellName()` calls were overriding manually generated names

**After (Fixed):**
- Cell names: `"Cell_5_6_Increased Evasion & Max Health"`
- **Solution**: Proper name replacement with conflict prevention

## 🔧 **What Was Fixed**

### **1. Enhanced SetNodeName Method**
- **Now updates both** `nodeName` field and GameObject name
- **Sets a flag** to prevent automatic overrides
- **Ensures consistency** between internal data and GameObject name

### **2. Added Manual Name Tracking**
- **New field**: `manuallySetName` flag
- **Prevents auto-updates** when names are manually set
- **Maintains control** over generated names

### **3. Updated UpdateCellName Method**
- **Checks manual flag** before auto-updating
- **Skips auto-update** if name was manually set
- **Preserves generated names** from the editor

## 🎯 **How It Works Now**

### **Step 1: Load Preset**
1. **Select a preset** (e.g., "Warrior")
2. **Load preset** to apply stats to template
3. **Template is ready** for application

### **Step 2: Apply Template**
1. **Select cells** in grid view
2. **Click "Apply Stat Template"**
3. **Stats are applied** to selected cells
4. **Names are generated** based on stats

### **Step 3: Name Generation**
1. **Stats are analyzed** (e.g., Strength: 10, Attack Power: 15)
2. **Name is generated** (e.g., "Strength & Attack Power")
3. **Full name created** (e.g., "Cell_3_4_Strength & Attack Power")
4. **GameObject name updated** to match

### **Step 4: Conflict Prevention**
1. **Manual flag is set** to prevent auto-override
2. **Automatic updates are skipped** for manually set names
3. **Generated names are preserved** across operations

## 🎨 **Name Generation Examples**

### **Example 1: Warrior Preset**
- **Stats**: Strength: 10, Attack Power: 15, Increased Physical Damage: 20%, Armour: 25
- **Generated Name**: `"Cell_3_4_Strength & Attack Power & Increased Physical Damage & Armour"`
- **Clean and descriptive** ✅

### **Example 2: Fire Elemental Preset**
- **Stats**: Increased Fire Damage: 30%, Added Fire Damage: 20, Fire Resistance: 25%, Chance to Ignite: 15%
- **Generated Name**: `"Cell_2_1_Increased Fire Damage & Added Fire Damage & Fire Resistance & Chance to Ignite"`
- **Clear damage type distinction** ✅

### **Example 3: Card System Preset**
- **Stats**: Cards Drawn Per Turn: 2, Max Hand Size: 3, Card Draw Chance: 25%, Mana Per Turn: 5
- **Generated Name**: `"Cell_5_2_Cards Drawn Per Turn & Max Hand Size & Card Draw Chance & Mana Per Turn"`
- **System-specific naming** ✅

## 🛠️ **Technical Details**

### **SetNodeName Method**
```csharp
public void SetNodeName(string newName)
{
    nodeName = newName;
    manuallySetName = true; // Prevent auto-override
    
    // Update GameObject name to match
    if (nodePosition != Vector2Int.zero)
    {
        string newGameObjectName = $"Cell_{nodePosition.x}_{nodePosition.y}_{newName}";
        gameObject.name = newGameObjectName;
    }
}
```

### **UpdateCellName Method**
```csharp
public void UpdateCellName()
{
    // Don't auto-update if the name was manually set
    if (manuallySetName)
    {
        return; // Skip auto-update
    }
    
    // ... rest of auto-update logic
}
```

### **Conflict Prevention**
- **Manual flag** prevents automatic overrides
- **Consistent naming** between internal data and GameObject
- **Preserved generated names** across operations

## 🧪 **Testing the Fix**

### **Test 1: Basic Name Generation**
1. **Load "Warrior" preset**
2. **Select a cell** at position (3,4)
3. **Apply template** and check the name
4. **Expected result**: `"Cell_3_4_Strength & Attack Power & Increased Physical Damage & Armour"`

### **Test 2: Multiple Applications**
1. **Load "Fire Elemental" preset**
2. **Select multiple cells** in different positions
3. **Apply template** to all selected cells
4. **Verify each cell** has a unique, clean name

### **Test 3: No Name Conflicts**
1. **Apply template** to a cell
2. **Check that name** is not appended to existing name
3. **Verify clean replacement** of the node name
4. **Confirm no** `"Cell_X_Y_OldName - Cell_X_Y_NewName"` format

## 🎉 **Benefits of the Fix**

### **Clean Names**
- ✅ **No more appended names** like "Cell_5_6_increased Evasion - Cell_5_6_Max Health"
- ✅ **Proper replacement** of existing names
- ✅ **Descriptive stat-based names** that make sense

### **Consistent Behavior**
- ✅ **Predictable naming** across all operations
- ✅ **No conflicts** between manual and automatic updates
- ✅ **Reliable name generation** every time

### **Better Workflow**
- ✅ **Easy identification** of node types from names
- ✅ **Professional appearance** in the hierarchy
- ✅ **Clear stat representation** in node names

## 🎯 **Result**

**Node name generation now works perfectly!** 

- ✅ **Clean, descriptive names** based on applied stats
- ✅ **No more name conflicts** or appended text
- ✅ **Consistent behavior** across all operations
- ✅ **Professional node naming** that makes sense

**Your passive tree nodes now have proper, meaningful names that reflect their actual stats!** 🎉


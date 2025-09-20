# CharacterStatsPanel Integration Guide

## Overview
This guide provides step-by-step instructions for connecting the CharacterStatsPanel prefab to the persistent Character data and UIManager for proper toggle functionality and stat synchronization.

## Architecture Overview
Following the established centralized management pattern:
- **CharacterStatsPanelManager**: Centralized manager for panel visibility and data synchronization
- **CharacterStatsController**: Handles UI updates and stat display
- **UIManager**: Provides toggle functionality with fallback support
- **CharacterManager**: Provides persistent character data

---

## 🎯 Step-by-Step Setup Instructions

### **Step 1: Add CharacterStatsPanelManager to Scene**

1. **In MainGameUI scene**, create a new empty GameObject
2. **Name it**: `CharacterStatsPanelManager`
3. **Add the CharacterStatsPanelManager script** to this GameObject
4. **Position it**: Anywhere in the scene (it will be marked as DontDestroyOnLoad)

### **Step 2: Configure CharacterStatsPanelManager**

1. **Open the CharacterStatsPanelManager inspector**
2. **Assign Panel References**:
   - **Character Stats Panel**: Drag your CharacterStatsPanel prefab/GameObject
   - **Stats Controller**: Drag the CharacterStatsController component from the panel
3. **Assign Toggle Button** (optional):
   - **Toggle Button**: Drag the button that should toggle the panel
   - If not assigned, the manager will auto-find buttons with "Character", "Stats", or "Char" in the name

### **Step 3: Verify CharacterStatsController Setup**

1. **On your CharacterStatsPanel**, ensure the CharacterStatsController has all specific UI references assigned:

   **Character Info Section:**
   - Character Portrait (Image)
   - Character Name Text (TextMeshPro)
   - Character Class Text (TextMeshPro)
   - Character Level Text (TextMeshPro)
   - Experience Text (TextMeshPro)
   - Experience Slider (Slider)

       **Resource Bars Section:**
    - Experience Slider (Slider) - Only slider used

    **Attribute Section:**
    - Strength Text (TextMeshPro)
    - Dexterity Text (TextMeshPro)
    - Intelligence Text (TextMeshPro)

    **Resource Section:**
    - Health Text (TextMeshPro)
    - Mana Text (TextMeshPro)
    - Reliance Text (TextMeshPro)

    **Damage Section - Physical:**
    - Physical Flat Value (TextMeshPro)
    - Physical Increase Value (TextMeshPro)
    - Physical More Value (TextMeshPro)

    **Damage Section - Fire:**
    - Fire Flat Value (TextMeshPro)
    - Fire Increase Value (TextMeshPro)
    - Fire More Value (TextMeshPro)

    **Damage Section - Cold:**
    - Cold Flat Value (TextMeshPro)
    - Cold Increase Value (TextMeshPro)
    - Cold More Value (TextMeshPro)

    **Damage Section - Lightning:**
    - Lightning Flat Value (TextMeshPro)
    - Lightning Increase Value (TextMeshPro)
    - Lightning More Value (TextMeshPro)

    **Damage Section - Chaos:**
    - Chaos Flat Value (TextMeshPro)
    - Chaos Increase Value (TextMeshPro)
    - Chaos More Value (TextMeshPro)

    **Defense Section:**
    - Armour Text (TextMeshPro)
    - Evasion Text (TextMeshPro)
    - Energy Shield Text (TextMeshPro)

    **Resistance Section:**
    - Physical Resistance Text (TextMeshPro)
    - Fire Resistance Text (TextMeshPro)
    - Cold Resistance Text (TextMeshPro)
    - Lightning Resistance Text (TextMeshPro)
    - Chaos Resistance Text (TextMeshPro)

    **Card Mechanics Section:**
    - Discard Power Text (TextMeshPro)
    - Mana Per Turn Text (TextMeshPro)
    - Draw Per Turn Text (TextMeshPro)
    - Hand Size Text (TextMeshPro)

2. **Test the UpdateCharacterStats method**:
   - The controller now has a new `UpdateCharacterStats(Character character)` method
   - This is called automatically by the manager when the panel opens
   - All specific fields will be updated with current character data

### **Step 4: Test the Integration**

1. **Play the scene**
2. **Check console logs** for initialization messages:
   ```
   [CharacterStatsPanelManager] Initialized successfully
   [CharacterStatsPanelManager] Connected to CharacterManager
   ```

3. **Test toggle functionality**:
   - Use the toggle button or call `CharacterStatsPanelManager.Instance.TogglePanel()`
   - Panel should open/close with proper stat updates

---

## 🔧 Integration Methods

### **Automatic Integration**
The system automatically integrates with existing UIManager calls:
- `UIManager.ToggleCharacterStats()` → Uses CharacterStatsPanelManager if available
- `UIManager.ShowCharacterStats()` → Uses CharacterStatsPanelManager if available
- `UIManager.HideCharacterStats()` → Uses CharacterStatsPanelManager if available

### **Manual Integration**
You can also call the manager directly:
```csharp
// Toggle the panel
CharacterStatsPanelManager.Instance.TogglePanel();

// Show the panel
CharacterStatsPanelManager.Instance.ShowPanel();

// Hide the panel
CharacterStatsPanelManager.Instance.HidePanel();

// Check if visible
bool isVisible = CharacterStatsPanelManager.Instance.IsPanelVisible();

// Force refresh data
CharacterStatsPanelManager.Instance.RefreshPanelData();
```

---

## 📊 Data Flow

### **Panel Opening Flow**
1. User clicks toggle button
2. `CharacterStatsPanelManager.TogglePanel()` called
3. Panel visibility set to true
4. `UpdatePanelData()` called automatically
5. `CharacterStatsController.UpdateCharacterStats()` called
6. Character data retrieved from `CharacterManager.Instance`
7. UI elements updated with current character stats

### **Data Synchronization**
- **On Panel Open**: Stats are automatically updated from CharacterManager
- **On Character Changes**: Call `RefreshPanelData()` to update visible panel
- **Persistent Data**: All data comes from the persistent CharacterManager singleton

---

## 🛠️ Troubleshooting

### **Common Issues**

#### **Issue: "CharacterStatsPanel is not assigned!"**
**Solution**: 
1. Drag the CharacterStatsPanel GameObject to the manager's "Character Stats Panel" field
2. Or use the "Setup Panel References" context menu option

#### **Issue: "CharacterManager not found!"**
**Solution**:
1. Ensure CharacterManager exists in the scene
2. Check that CharacterManager.Instance is not null
3. Verify a character is loaded with `CharacterManager.Instance.HasCharacter()`

#### **Issue: "StatsController not assigned!"**
**Solution**:
1. Ensure CharacterStatsController component is on the panel
2. Drag the CharacterStatsController to the manager's "Stats Controller" field
3. Or use the "Setup Panel References" context menu option

#### **Issue: Panel opens but stats are empty**
**Solution**:
1. Check that a character is loaded in CharacterManager
2. Verify CharacterStatsController has all UI references assigned
3. Check console for "No character loaded!" warnings

### **Debug Commands**
Use these context menu options for debugging:
- **Setup Panel References**: Auto-find and assign missing references
- **Toggle Panel**: Test panel visibility
- **Update Panel Data**: Force refresh character data

---

## 🔄 Future Enhancements

### **Event-Driven Updates**
When CharacterManager events are implemented:
```csharp
// Subscribe to character changes
CharacterManager.Instance.OnCharacterStatsChanged += RefreshPanelData;
```

### **Animation Integration**
Add smooth open/close animations:
```csharp
// In CharacterStatsPanelManager
public void TogglePanelWithAnimation()
{
    // Add animation logic here
}
```

### **Auto-Refresh on Scene Changes**
Ensure panel data stays current:
```csharp
// In CharacterStatsPanelManager
private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    if (IsPanelVisible())
    {
        RefreshPanelData();
    }
}
```

---

## ✅ Verification Checklist

- [ ] CharacterStatsPanelManager added to scene
- [ ] Panel references assigned in inspector
- [ ] CharacterStatsController has all UI references
- [ ] Toggle button connected (optional)
- [ ] CharacterManager has a loaded character
- [ ] Panel opens/closes correctly
- [ ] Stats display correctly when panel opens
- [ ] Console shows successful initialization messages
- [ ] UIManager integration works (fallback tested)

---

## 📝 Development Log

**Date**: [Current Date]
**Feature**: CharacterStatsPanel Integration
**Status**: ✅ Complete

**Key Decisions**:
- Used centralized management pattern (CharacterStatsPanelManager)
- Maintained backward compatibility with UIManager
- Automatic data synchronization on panel open
- Singleton pattern with DontDestroyOnLoad for persistence

**Files Modified**:
- `CharacterStatsPanelManager.cs` (new)
- `CharacterStatsController.cs` (added UpdateCharacterStats method)
- `UIManager.cs` (integrated with new manager)

**Testing Results**:
- Panel toggle functionality working
- Data synchronization working
- Fallback to direct UIManager control working
- Console logging for debugging working

# StatsPanel MultiColumnListView Implementation Guide

## Overview
This guide covers the implementation of a comprehensive character stats display system using Unity UI Toolkit's MultiColumnListView component. The system displays character attributes, resources, damage, and resistances in organized tabular formats.

## Architecture

### Components
1. **StatsPanel.cs** - Editor Window version for development/testing
2. **StatsPanelRuntime.cs** - Runtime component for in-game display
3. **StatsPanel.uxml** - UI structure definition
4. **StatsPanel.uss** - Comprehensive styling system
5. **AttributeData.cs** - Existing data classes for MultiColumnListView binding

### Data Classes
The system uses existing data classes from `Assets/Scripts/Data/AttributeData.cs`:

```csharp
[Serializable]
public class AttributeData
{
    public string strength;
    public string dexterity;
    public string intelligence;

    public AttributeData(string str, string dex, string intel)
    {
        strength = str;
        dexterity = dex;
        intelligence = intel;
    }
}

[Serializable]
public class ResourceData
{
    public string health;
    public string mana;
    public string energyShield;
    public string reliance;

    public ResourceData(string hp, string mp, string es, string rel)
    {
        health = hp;
        mana = mp;
        energyShield = es;
        reliance = rel;
    }
}

[Serializable]
public class DamageData
{
    public string flat;
    public string increased;
    public string more;

    public DamageData(string f, string inc, string m)
    {
        flat = f;
        increased = inc;
        more = m;
    }
}

[Serializable]
public class ResistanceData
{
    public string fire;
    public string cold;
    public string lightning;
    public string chaos;

    public ResistanceData(string f, string c, string l, string ch)
    {
        fire = f;
        cold = c;
        lightning = l;
        chaos = ch;
    }
}
```

## MultiColumnListView Implementation

### UXML Structure
The UXML file defines four MultiColumnListView components:

```xml
<!-- Attributes Section -->
<ui:MultiColumnListView header-title="Attributes" name="AttributeColumns" 
    style="height: 80px; show-border: true; show-alternating-row-backgrounds: ContentOnly;">
    <ui:Columns resize-preview="false">
        <ui:Column name="AttributeStrength" title="Strength" width="33.3333%" />
        <ui:Column name="AttributeDexterity" title="Dexterity" width="33.3333%" />
        <ui:Column name="AttributeIntelligence" title="Intelligence" width="33.3333%" />
    </ui:Columns>
</ui:MultiColumnListView>

<!-- Resources Section -->
<ui:MultiColumnListView header-title="Resources" name="ResourceColumns" 
    style="height: 80px; show-border: true; show-alternating-row-backgrounds: ContentOnly;">
    <ui:Columns resize-preview="false">
        <ui:Column name="ResourceHealth" title="Health" width="25%" />
        <ui:Column name="ResourceMana" title="Mana" width="25%" />
        <ui:Column name="ResourceEnergyShield" title="Energy Shield" width="25%" />
        <ui:Column name="ResourceReliance" title="Reliance" width="25%" />
    </ui:Columns>
</ui:MultiColumnListView>

<!-- Damage Section -->
<ui:MultiColumnListView header-title="Damage" name="DamageColumns" 
    style="height: 120px; show-border: true; show-alternating-row-backgrounds: ContentOnly;">
    <ui:Columns resize-preview="false">
        <ui:Column name="DamageFlat" title="Flat" width="33.3333%" />
        <ui:Column name="DamageIncreased" title="Increased" width="33.3333%" />
        <ui:Column name="DamageMore" title="More" width="33.3333%" />
    </ui:Columns>
</ui:MultiColumnListView>

<!-- Resistances Section -->
<ui:MultiColumnListView header-title="Resistances" name="ResistanceColumns" 
    style="height: 80px; show-border: true; show-alternating-row-backgrounds: ContentOnly;">
    <ui:Columns resize-preview="false">
        <ui:Column name="ResistanceFire" title="Fire" width="25%" />
        <ui:Column name="ResistanceCold" title="Cold" width="25%" />
        <ui:Column name="ResistanceLightning" title="Lightning" width="25%" />
        <ui:Column name="ResistanceChaos" title="Chaos" width="25%" />
    </ui:Columns>
</ui:MultiColumnListView>
```

### Key UXML Attributes
- **show-border**: Enables border display for visual separation
- **show-alternating-row-backgrounds**: Creates alternating row colors for better readability
- **resize-preview**: Disabled to prevent column resizing during development
- **width**: Percentage-based column widths for responsive design

## Styling System

### USS Class Hierarchy
The styling system uses Unity's internal class names for MultiColumnListView:

```css
/* Main container styling */
#AttributeColumns,
#ResourceColumns,
#DamageColumns,
#ResistanceColumns {
    background-color: rgba(35, 33, 30, 0.8);
    border-width: 1px;
    border-color: rgb(74, 64, 50);
    border-radius: 4px;
    margin: 8px 0;
    min-height: 100px;
}

/* Header styling */
.unity-multi-column-list-view__header {
    background-color: rgb(45, 43, 40);
    border-bottom-width: 1px;
    border-bottom-color: rgb(74, 64, 50);
    min-height: 28px;
    padding: 4px 0;
}

.unity-multi-column-list-view__header-label {
    color: rgb(255, 224, 102);
    -unity-font-style: bold;
    font-size: 14px;
    -unity-text-align: middle-center;
    padding: 8px 4px;
}

/* Row styling */
.unity-multi-column-list-view__row {
    background-color: rgba(35, 33, 30, 0.6);
    border-bottom-width: 1px;
    border-bottom-color: rgba(74, 64, 50, 0.3);
    transition-property: background-color;
    transition-duration: 0.2s;
    min-height: 24px;
}

.unity-multi-column-list-view__row:hover {
    background-color: rgba(45, 43, 40, 0.8);
}

/* Cell styling */
.unity-multi-column-list-view__cell {
    padding: 6px 8px;
    -unity-text-align: middle-center;
    color: rgb(255, 255, 255);
    font-size: 13px;
    border-right-width: 1px;
    border-right-color: rgba(74, 64, 50, 0.2);
}
```

### Color-Coded Columns
Each column type has specific color coding for visual distinction:

```css
/* Attribute column colors */
#AttributeColumns .unity-multi-column-list-view__cell:nth-child(1) {
    color: rgb(255, 150, 150); /* Red for Strength */
    -unity-font-style: bold;
}

#AttributeColumns .unity-multi-column-list-view__cell:nth-child(2) {
    color: rgb(150, 255, 150); /* Green for Dexterity */
    -unity-font-style: bold;
}

#AttributeColumns .unity-multi-column-list-view__cell:nth-child(3) {
    color: rgb(150, 150, 255); /* Blue for Intelligence */
    -unity-font-style: bold;
}

/* Resource column colors */
#ResourceColumns .unity-multi-column-list-view__cell:nth-child(1) {
    color: rgb(255, 100, 100); /* Health - Red */
}

#ResourceColumns .unity-multi-column-list-view__cell:nth-child(2) {
    color: rgb(100, 150, 255); /* Mana - Blue */
}

#ResourceColumns .unity-multi-column-list-view__cell:nth-child(3) {
    color: rgb(200, 200, 255); /* Energy Shield - Light Blue */
}

#ResourceColumns .unity-multi-column-list-view__cell:nth-child(4) {
    color: rgb(255, 200, 100); /* Reliance - Orange */
}
```

## Data Binding Implementation

### Column Binding Setup
The system uses manual cell creation and binding for maximum control:

```csharp
private void SetupColumnBindings(MultiColumnListView listView, string prefix)
{
    if (listView == null) return;

    var columns = listView.columns;
    
    foreach (var column in columns)
    {
        // Create cells manually
        column.makeCell = () => new Label();
        
        // Bind cells manually
        column.bindCell = (element, index) =>
        {
            if (element is Label label)
            {
                BindCellData(label, column.name, index, prefix);
            }
        };
    }
}
```

### Cell Data Binding
Centralized binding logic handles all column types:

```csharp
private void BindCellData(Label label, string columnName, int index, string prefix)
{
    switch (prefix)
    {
        case "Attribute":
            if (index < attributeDataList.Count)
            {
                var data = attributeDataList[index];
                switch (columnName)
                {
                    case "AttributeStrength": label.text = data.strength; break;
                    case "AttributeDexterity": label.text = data.dexterity; break;
                    case "AttributeIntelligence": label.text = data.intelligence; break;
                }
            }
            break;
            
        case "Resource":
            if (index < resourceDataList.Count)
            {
                var data = resourceDataList[index];
                switch (columnName)
                {
                    case "ResourceHealth": label.text = data.health; break;
                    case "ResourceMana": label.text = data.mana; break;
                    case "ResourceEnergyShield": label.text = data.energyShield; break;
                    case "ResourceReliance": label.text = data.reliance; break;
                }
            }
            break;
            
        // ... similar for Damage and Resistance
    }
}
```

## Character Data Integration

### Data Population
The system automatically populates data from the Character class:

```csharp
private void UpdateAttributeData(Character character)
{
    attributeDataList.Clear();
    
    // Get base stats for this character class
    var (baseStr, baseDex, baseInt) = GetBaseStatsForClass(character.characterClass);
    
    // Calculate level-up gains
    var levelGains = GetLevelUpGains(character.characterClass);
    int levelsGained = character.level - 1;
    
    // Calculate expected base stats at this level
    int expectedBaseStr = baseStr + (levelGains.str * levelsGained);
    int expectedBaseDex = baseDex + (levelGains.dex * levelsGained);
    int expectedBaseInt = baseInt + (levelGains.intel * levelsGained);
    
    // Calculate bonuses (current - expected base)
    int strBonus = character.strength - expectedBaseStr;
    int dexBonus = character.dexterity - expectedBaseDex;
    int intBonus = character.intelligence - expectedBaseInt;
    
    // Row 1: Current total values
    attributeDataList.Add(new AttributeData(
        character.strength.ToString(),
        character.dexterity.ToString(),
        character.intelligence.ToString()
    ));
    
    // Row 2: Bonuses from items/equipment
    attributeDataList.Add(new AttributeData(
        strBonus > 0 ? $"+{strBonus}" : strBonus < 0 ? $"{strBonus}" : "+0",
        dexBonus > 0 ? $"+{dexBonus}" : dexBonus < 0 ? $"{dexBonus}" : "+0",
        intBonus > 0 ? $"+{intBonus}" : intBonus < 0 ? $"{intBonus}" : "+0"
    ));
}
```

### List View Refresh
After updating data, all list views are refreshed:

```csharp
private void RefreshAllListViews()
{
    if (attributeListView != null)
    {
        attributeListView.itemsSource = attributeDataList;
        attributeListView.Rebuild();
    }
    
    if (resourceListView != null)
    {
        resourceListView.itemsSource = resourceDataList;
        resourceListView.Rebuild();
    }
    
    if (damageListView != null)
    {
        damageListView.itemsSource = damageDataList;
        damageListView.Rebuild();
    }
    
    if (resistanceListView != null)
    {
        resistanceListView.itemsSource = resistanceDataList;
        resistanceListView.Rebuild();
    }
}
```

## Usage Instructions

### Editor Window (Development)
1. Open Unity Editor
2. Go to Window > UI Toolkit > StatsPanel
3. The window will automatically load test data or character data if available
4. Use for development and testing of the stats display system

### Runtime Component (In-Game)
1. Create a GameObject in your scene
2. Add the StatsPanelRuntime component
3. Assign the UIDocument component
4. The component will auto-assign VisualTreeAsset and StyleSheet from Resources
5. The panel will automatically display character data when a character is loaded

### Manual Refresh
To refresh the panel with updated character data:

```csharp
// Get reference to StatsPanelRuntime component
StatsPanelRuntime statsPanel = FindObjectOfType<StatsPanelRuntime>();

// Refresh the panel
statsPanel.RefreshPanel();
```

## Troubleshooting

### Common Issues

1. **MultiColumnListView not displaying data**
   - Check that itemsSource is properly set
   - Verify column bindings are correctly configured
   - Ensure data lists are not empty

2. **Styling not applied**
   - Verify StyleSheet is assigned to UIDocument
   - Check that USS classes match Unity's internal class names
   - Ensure stylesheet is loaded from Resources folder

3. **Character data not loading**
   - Verify CharacterManager.Instance exists
   - Check that CharacterManager.HasCharacter() returns true
   - Ensure character data is properly initialized

4. **Column headers not visible**
   - Check that header-title is set in UXML
   - Verify header styling is applied correctly
   - Ensure column names match binding logic

### Debug Logging
The system includes comprehensive debug logging. Check the Console for:
- UI element reference status
- Data population confirmation
- List view setup completion
- Character data loading status

## Performance Considerations

1. **Efficient Data Binding**: Manual cell creation and binding provides optimal performance
2. **Minimal Rebuilds**: Only rebuild list views when data actually changes
3. **Resource Management**: Auto-assignment reduces manual configuration overhead
4. **Memory Usage**: Serializable data classes enable efficient data transfer

## Future Enhancements

1. **Sorting**: Implement column sorting functionality
2. **Filtering**: Add data filtering capabilities
3. **Export**: Add data export functionality
4. **Customization**: Allow user customization of displayed stats
5. **Animations**: Add smooth transitions for data updates
6. **Responsive Design**: Improve layout for different screen sizes

## Unity Version Compatibility

This implementation is designed for Unity 6.1+ and uses the latest UI Toolkit features. Key dependencies:
- MultiColumnListView (Unity 6.1+)
- USS styling system
- UXML structure definition
- UI Toolkit runtime components

## References

- [Unity MultiColumnListView Documentation](https://docs.unity3d.com/6000.1/Documentation/Manual/UIE-uxml-element-MultiColumnListView.html)
- Unity UI Toolkit Manual
- Unity USS Styling Guide

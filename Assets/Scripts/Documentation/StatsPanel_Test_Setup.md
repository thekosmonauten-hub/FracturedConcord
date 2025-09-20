# StatsPanel Test Setup Guide

## Quick Test Setup

### Step 1: Create Test Scene
1. Create a new scene called "StatsPanelTest"
2. Save it in `Assets/Scenes/`

### Step 2: Set Up StatsPanel GameObject
1. Create an empty GameObject named "StatsPanel"
2. Add the following components:
   - `UIDocument` component
   - `StatsPanelRuntime` component

### Step 3: Configure UIDocument
1. Select the StatsPanel GameObject
2. In the UIDocument component inspector:
   - **Panel Settings**: Create new or use existing PanelSettings
   - **Visual Tree Asset**: Leave empty (will auto-assign)
   - **Style Sheet**: Leave empty (will auto-assign)

### Step 4: Test Character Setup
1. Create an empty GameObject named "TestCharacterManager"
2. Add the `CharacterManager` component
3. In the CharacterManager inspector, set up a test character:
   ```csharp
   // Example test character data
   Character Name: "TestMarauder"
   Character Class: "Marauder"
   Level: 5
   Strength: 47
   Dexterity: 18
   Intelligence: 18
   ```

### Step 5: Run Test
1. Press Play in Unity
2. The StatsPanel should automatically display:
   - Character information (name, class, level)
   - Experience bar
   - Four MultiColumnListView sections:
     - **Attributes**: Strength, Dexterity, Intelligence
     - **Resources**: Health, Mana, Energy Shield, Reliance
     - **Damage**: Flat, Increased, More damage types
     - **Resistances**: Fire, Cold, Lightning, Chaos

## Expected Visual Results

### Color-Coded Columns
- **Attributes**: Red (Strength), Green (Dexterity), Blue (Intelligence)
- **Resources**: Red (Health), Blue (Mana), Light Blue (Energy Shield), Orange (Reliance)
- **Damage**: Gray (Physical), Orange (Fire), Light Blue (Cold), Yellow (Lightning)
- **Resistances**: Red (Fire), Light Blue (Cold), Yellow (Lightning), Purple (Chaos)

### Styling Features
- Dark theme with golden headers
- Alternating row backgrounds
- Hover effects on rows
- Proper borders and spacing
- Responsive column widths

## Troubleshooting Test Setup

### Issue: No Data Displayed
**Solution**: Check Console for debug messages. Common causes:
- CharacterManager not found
- Character data not loaded
- UIDocument not properly configured

### Issue: Styling Not Applied
**Solution**: 
1. Verify StatsPanel.uss is in `Assets/Resources/UI/CharacterStats/`
2. Check that StatsPanel.uxml is in `Assets/Resources/UI/CharacterStats/`
3. Ensure UIDocument can find the assets

### Issue: MultiColumnListView Empty
**Solution**:
1. Check that data lists are populated
2. Verify column bindings are set up
3. Ensure itemsSource is assigned before Rebuild()

## Debug Console Messages

Look for these debug messages in the Console:

```
[StatsPanelRuntime] Awake() called
[StatsPanelRuntime] Auto-assigned UIDocument: True
[StatsPanelRuntime] Auto-assigned VisualTreeAsset: True
[StatsPanelRuntime] Auto-assigned StyleSheet: True
[StatsPanelRuntime] Start() called
[StatsPanelRuntime] InitializeUI() started
[StatsPanelRuntime] visualTreeAsset set successfully
[StatsPanelRuntime] Style sheet loaded: StatsPanel
[StatsPanelRuntime] UI References - Name: True, Class: True, Level: True, Exp: True
[StatsPanelRuntime] ListView References - Attributes: True, Resources: True, Damage: True, Resistances: True
[StatsPanelRuntime] CharacterManager.Instance: CharacterManager
[StatsPanelRuntime] Character loaded: TestMarauder (Level 5)
[StatsPanelRuntime] Updating with character: TestMarauder (Level 5)
[StatsPanelRuntime] Setting up 3 columns for Attribute
[StatsPanelRuntime] Setting up 4 columns for Resource
[StatsPanelRuntime] Setting up 3 columns for Damage
[StatsPanelRuntime] Setting up 4 columns for Resistance
[StatsPanelRuntime] All list views refreshed
[StatsPanelRuntime] InitializeUI() completed successfully
```

## Manual Testing

### Test Character Data Updates
1. In Play mode, modify the CharacterManager's character data
2. Call `statsPanel.RefreshPanel()` from Console or script
3. Verify the MultiColumnListView updates with new data

### Test Different Character Classes
1. Change the character class in CharacterManager
2. Refresh the panel
3. Verify attribute calculations change based on class

### Test Edge Cases
1. **Empty Character**: Set character name to empty string
2. **Invalid Level**: Set level to 0 or negative
3. **Missing CharacterManager**: Disable CharacterManager GameObject
4. **Missing Assets**: Move USS/UXML files to different location

## Performance Testing

### Monitor Performance
1. Open Profiler window
2. Look for UI-related performance issues
3. Check memory usage of MultiColumnListView components
4. Verify no memory leaks during scene transitions

### Stress Testing
1. Create multiple StatsPanel instances
2. Rapidly update character data
3. Test with large amounts of data
4. Verify smooth performance

## Integration Testing

### With Existing Systems
1. Test with CombatSceneManager
2. Verify integration with CharacterManager
3. Test scene transitions
4. Check data persistence

### UI Integration
1. Test with other UI components
2. Verify proper layering
3. Check responsive design
4. Test different screen resolutions

## Common Test Scenarios

### Scenario 1: New Character
1. Create new character
2. Verify initial stats display correctly
3. Check level 1 calculations
4. Verify base class stats

### Scenario 2: Level Up
1. Increase character level
2. Verify attribute calculations update
3. Check experience bar updates
4. Verify bonus calculations

### Scenario 3: Equipment Changes
1. Modify character equipment
2. Verify stat bonuses update
3. Check damage calculations
4. Verify resistance updates

### Scenario 4: Multiple Characters
1. Switch between characters
2. Verify data updates correctly
3. Check memory management
4. Verify no data corruption

## Success Criteria

The StatsPanel test is successful when:

✅ **Visual**: All four MultiColumnListView sections display with proper styling  
✅ **Data**: Character information displays correctly  
✅ **Colors**: Column colors match expected color scheme  
✅ **Interactivity**: Hover effects work on rows  
✅ **Performance**: No lag or performance issues  
✅ **Integration**: Works with CharacterManager and existing systems  
✅ **Responsive**: Adapts to different screen sizes  
✅ **Debug**: Console shows proper debug messages  

## Next Steps After Testing

1. **Integration**: Integrate into main game scenes
2. **Customization**: Add user customization options
3. **Enhancement**: Add sorting and filtering features
4. **Optimization**: Optimize for production use
5. **Documentation**: Update user documentation










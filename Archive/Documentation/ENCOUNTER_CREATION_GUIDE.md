# Encounter Creation Guide

## Overview
This guide explains how to create and update Act 1, 2, and 3 encounter nodes based on the `ACT1-3 Node Data.md` document.

## Using the Editor Tool

### Accessing the Tool
1. Open Unity Editor
2. Go to menu: **Dexiled > Create Act Encounters**
3. A window will open with buttons to generate encounters for each act

### Generating Encounters

#### Option 1: Generate Individual Acts
- Click **"Generate/Update Act 1 Encounters (15 nodes)"** to create/update Act 1
- Click **"Generate/Update Act 2 Encounters (15 nodes)"** to create/update Act 2
- Click **"Generate/Update Act 3 Encounters (10 nodes + final boss)"** to create/update Act 3

#### Option 2: Generate All Acts at Once
- Click **"Generate All Acts"** to create/update all encounters in one go

### What Gets Created

The tool creates `EncounterDataAsset` ScriptableObjects in the following locations:
- **Act 1**: `Assets/Resources/Encounters/Act1/`
- **Act 2**: `Assets/Resources/Encounters/Act2/`
- **Act 3**: `Assets/Resources/Encounters/Act3/`

### Encounter Structure

Each encounter asset includes:
- **encounterID**: Global ID (1-41 across all acts)
- **encounterName**: Name from the document
- **sceneName**: "CombatScene" (default)
- **actNumber**: 1, 2, or 3
- **areaLevel**: Matches global ID (scales with progression)
- **totalWaves**: 3 (default, can be adjusted)
- **maxEnemiesPerWave**: 3 (default)
- **randomizeEnemyCount**: true (default)
- **prerequisiteEncounters**: Automatically set up (each encounter requires the previous one)

### Manual Steps After Generation

1. **Assign Boss EnemyData Assets**
   - Each encounter needs a `uniqueEnemy` (EnemyData) assigned
   - Boss names are listed in the document
   - Create EnemyData assets for each boss and assign them to the corresponding encounters

2. **Assign Loot Tables** (Optional)
   - Assign `lootTable` or `areaLootTable` to each encounter
   - This can be done manually or via another tool

3. **Adjust Wave Settings** (Optional)
   - Modify `totalWaves`, `maxEnemiesPerWave`, and `randomizeEnemyCount` as needed
   - Some encounters might need different wave configurations

4. **Assign Encounter Sprites** (Optional)
   - Add visual icons for each encounter node on the map

### Act Breakdown

#### Act 1 (15 Encounters)
- IDs: 1-15
- First encounter: "Where Night First Fell" (The First to Fall)
- Final encounter: "The Shattered Gate" (Gate Warden of Vassara)
- Note: "Camp Concordia" (ID 2) is a town node and may not require combat

#### Act 2 (15 Encounters)
- IDs: 16-30
- First encounter: "Outer Wards of Vassara" (Pale Sentry Construct)
- Final encounter: "Threshold of the Sundered Archive" (Guardian of Lost Articles)
- Prerequisite: Requires completion of Act 1 final encounter (ID 15)

#### Act 3 (11 Encounters)
- IDs: 31-41
- First encounter: "Hall of Forgotten Indictments" (The Indexer)
- Final encounter: "Breachheart" (Eduard, The Unwritten Path)
- Prerequisite: Requires completion of Act 2 final encounter (ID 30)
- Note: "Archive Root" (ID 40) is a mini-boss before the final boss

### Prerequisite System

The tool automatically sets up prerequisites:
- **Act 1, Encounter 1**: No prerequisites (starting encounter)
- **Act 1, Encounters 2-15**: Each requires the previous encounter
- **Act 2, Encounter 1**: Requires Act 1, Encounter 15 (The Shattered Gate)
- **Act 2, Encounters 2-15**: Each requires the previous encounter
- **Act 3, Encounter 1**: Requires Act 2, Encounter 15 (Threshold of the Sundered Archive)
- **Act 3, Encounters 2-11**: Each requires the previous encounter

### Boss Abilities Reference

Boss abilities are documented in `ACT1-3 Node Data.md`. These will need to be implemented as:
- EnemyAbility ScriptableObjects
- Assigned to the corresponding EnemyData assets
- Integrated into the combat system

### Troubleshooting

**Issue**: Encounters not appearing in game
- Check that `EncounterManager` has `loadFromResources = true`
- Verify the Resources paths match: `Encounters/Act1`, `Encounters/Act2`, `Encounters/Act3`
- Ensure all assets are saved (Ctrl+S in Unity)

**Issue**: Prerequisites not working
- Verify that prerequisite encounters are properly assigned
- Check that encounter IDs are unique and sequential
- Ensure previous encounters are marked as completed

**Issue**: Boss not spawning
- Verify that `uniqueEnemy` is assigned to each encounter
- Check that the EnemyData asset exists and is properly configured
- Ensure the boss spawns in the final wave (wave index = totalWaves - 1)

### Next Steps

After generating encounters:
1. Create EnemyData assets for all bosses
2. Assign bosses to their corresponding encounters
3. Create and assign loot tables
4. Test encounter progression in-game
5. Adjust difficulty/balance as needed


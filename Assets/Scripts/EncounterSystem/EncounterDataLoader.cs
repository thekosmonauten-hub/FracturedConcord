using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles loading encounter data from Resources.
/// Separated from EncounterManager for better organization.
/// </summary>
public static class EncounterDataLoader
{
    /// <summary>
    /// Load encounters from Resources path.
    /// </summary>
    public static List<EncounterData> LoadEncountersFromResources(string resourcesPath, bool ensureEncounter1Unlocked = false)
    {
        List<EncounterData> encounters = new List<EncounterData>();
        
        if (string.IsNullOrEmpty(resourcesPath))
        {
            Debug.LogWarning($"[EncounterDataLoader] Resources path is null or empty.");
            return encounters;
        }
        
        EncounterDataAsset[] assets = Resources.LoadAll<EncounterDataAsset>(resourcesPath);
        
        if (assets == null || assets.Length == 0)
        {
            Debug.Log($"[EncounterDataLoader] No encounters found at Resources/{resourcesPath}");
            return encounters;
        }
        
        foreach (var asset in assets)
        {
            if (asset == null) continue;
            
            EncounterData data = ConvertAssetToEncounterData(asset, ensureEncounter1Unlocked);
            if (data != null)
            {
                encounters.Add(data);
            }
        }
        
        // Sort by encounter ID
        encounters.Sort((a, b) => a.encounterID.CompareTo(b.encounterID));
        
        Debug.Log($"[EncounterDataLoader] Loaded {encounters.Count} encounters from Resources/{resourcesPath}");
        
        return encounters;
    }
    
    /// <summary>
    /// Convert EncounterDataAsset to EncounterData runtime object.
    /// </summary>
    private static EncounterData ConvertAssetToEncounterData(EncounterDataAsset asset, bool ensureEncounter1Unlocked)
    {
        if (asset == null) return null;
        
        bool hasPrereqs = asset.prerequisiteEncounters != null && asset.prerequisiteEncounters.Length > 0;
        
        EncounterData data = new EncounterData(asset.encounterID, asset.encounterName, asset.sceneName)
        {
            areaLevel = asset.areaLevel,
            totalWaves = Mathf.Max(1, asset.totalWaves),
            maxEnemiesPerWave = Mathf.Max(1, asset.maxEnemiesPerWave),
            randomizeEnemyCount = asset.randomizeEnemyCount,
            uniqueEnemy = asset.uniqueEnemy,
            lootTable = asset.lootTable,
            areaLootTable = asset.areaLootTable,
            actNumber = asset.actNumber,
            encounterSprite = asset.encounterSprite,
            // Encounter 1 is always unlocked, regardless of prerequisites
            isUnlocked = (asset.encounterID == 1) || !hasPrereqs,
            // Copy encounter-specific enemy pool
            encounterEnemyPool = asset.encounterEnemyPool != null ? new List<EnemyData>(asset.encounterEnemyPool) : new List<EnemyData>(),
            useExclusiveEnemyPool = asset.useExclusiveEnemyPool
        };
        
        // Debug logging for enemy pool
        if (data.encounterEnemyPool != null && data.encounterEnemyPool.Count > 0)
        {
            Debug.Log($"[EncounterDataLoader] Encounter {data.encounterID} ({data.encounterName}) loaded with {data.encounterEnemyPool.Count} enemies in pool (Exclusive: {data.useExclusiveEnemyPool})");
            foreach (var enemy in data.encounterEnemyPool)
            {
                Debug.Log($"  - {enemy.enemyName}");
            }
        }
        else
        {
            Debug.Log($"[EncounterDataLoader] Encounter {data.encounterID} ({data.encounterName}) has NO enemy pool - will use EnemyDatabase");
        }
        
        // Add prerequisites
        if (asset.prerequisiteEncounters != null)
        {
            foreach (var prereq in asset.prerequisiteEncounters)
            {
                if (prereq != null && prereq.encounterID > 0 && !data.prerequisiteEncounterIDs.Contains(prereq.encounterID))
                {
                    data.prerequisiteEncounterIDs.Add(prereq.encounterID);
                }
            }
        }
        
        // Ensure encounter 1 is always unlocked
        if (data.encounterID == 1)
        {
            data.isUnlocked = true;
        }
        
        return data;
    }
    
    /// <summary>
    /// Load all encounters from all act paths.
    /// </summary>
    public static Dictionary<int, List<EncounterData>> LoadAllEncounters(
        string act1Path, 
        string act2Path, 
        string act3Path, 
        string act4Path)
    {
        Dictionary<int, List<EncounterData>> encountersByAct = new Dictionary<int, List<EncounterData>>();
        
        encountersByAct[1] = LoadEncountersFromResources(act1Path, ensureEncounter1Unlocked: true);
        encountersByAct[2] = LoadEncountersFromResources(act2Path, ensureEncounter1Unlocked: false);
        encountersByAct[3] = LoadEncountersFromResources(act3Path, ensureEncounter1Unlocked: false);
        encountersByAct[4] = LoadEncountersFromResources(act4Path, ensureEncounter1Unlocked: false);
        
        return encountersByAct;
    }
}


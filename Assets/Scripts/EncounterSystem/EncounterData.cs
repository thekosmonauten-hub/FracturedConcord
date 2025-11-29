using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EncounterData
{
    public int encounterID;
    public string encounterName;
    public string sceneName;
    public int areaLevel = 1;
    [Tooltip("Story act this encounter belongs to (for act-based loot routing, progression, etc.)")]
    public int actNumber = 1;
    [Tooltip("Icon shown on encounter selection / world map nodes.")]
    public Sprite encounterSprite;
    [Header("Progression")]
    public List<int> prerequisiteEncounterIDs = new List<int>();
    public List<int> unlockedEncounterIDs = new List<int>();
    [Header("Waves")]
    public int totalWaves = 1;
    public int maxEnemiesPerWave = 3;
    public bool randomizeEnemyCount = true;
    [Header("Unique Boss (Final Wave)")]
    public EnemyData uniqueEnemy; // Assign the special Unique monster for this encounter
    [Header("Encounter-Specific Enemy Pool")]
    [Tooltip("If assigned, only enemies from this list can spawn in this encounter. If empty, uses EnemyDatabase with excludeFromRandom filtering.")]
    public List<EnemyData> encounterEnemyPool = new List<EnemyData>();
    [Tooltip("If true, encounterEnemyPool is used exclusively. If false, encounterEnemyPool is combined with EnemyDatabase (filtered).")]
    public bool useExclusiveEnemyPool = false;
    [Header("Loot Rewards")]
    public LootTable lootTable;
    public AreaLootTable areaLootTable;
    public bool isUnlocked = true;
    public bool isCompleted = false;
    
    public EncounterData(int id, string name, string scene)
    {
        encounterID = id;
        encounterName = name;
        sceneName = scene;
        areaLevel = 1;
        totalWaves = 1;
        maxEnemiesPerWave = 3;
        randomizeEnemyCount = true;
    }
}

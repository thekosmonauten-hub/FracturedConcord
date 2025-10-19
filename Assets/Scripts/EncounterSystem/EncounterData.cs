using UnityEngine;

[System.Serializable]
public class EncounterData
{
    public int encounterID;
    public string encounterName;
    public string sceneName;
    public int areaLevel = 1;
    [Header("Waves")]
    public int totalWaves = 1;
    [Header("Unique Boss (Final Wave)")]
    public EnemyData uniqueEnemy; // Assign the special Unique monster for this encounter
    public bool isUnlocked = true;
    public bool isCompleted = false;
    
    public EncounterData(int id, string name, string scene)
    {
        encounterID = id;
        encounterName = name;
        sceneName = scene;
        areaLevel = 1;
        totalWaves = 1;
    }
}

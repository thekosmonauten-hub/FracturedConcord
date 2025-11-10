using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class EncounterManager : MonoBehaviour
{
    private static EncounterManager _instance;
    public static EncounterManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<EncounterManager>();
                if (_instance == null)
                {
                    var go = new GameObject("EncounterManager");
                    _instance = go.AddComponent<EncounterManager>();
                    DontDestroyOnLoad(go);
                    _instance.InitializeEncounters();
                }
            }
            return _instance;
        }
    }
    
    [Header("Dependencies")]
    [Tooltip("Optional: assign an EnemyDatabase prefab or scene object to ensure it exists before encounters run.")]
    public EnemyDatabase enemyDatabaseRef;

    private void LoadActFromResources(string path, List<EncounterData> target, bool autoUnlockFirst)
    {
        if (string.IsNullOrEmpty(path) || target == null) return;
        var assets = Resources.LoadAll<EncounterDataAsset>(path);
        if (assets != null && assets.Length > 0)
        {
            foreach (var asset in assets)
            {
                if (asset == null) continue;
                var data = new EncounterData(asset.encounterID, asset.encounterName, asset.sceneName)
                {
                    areaLevel = asset.areaLevel,
                    totalWaves = Mathf.Max(1, asset.totalWaves),
                    maxEnemiesPerWave = Mathf.Max(1, asset.maxEnemiesPerWave),
                    randomizeEnemyCount = asset.randomizeEnemyCount,
                    uniqueEnemy = asset.uniqueEnemy,
                    lootTable = asset.lootTable
                };
                target.Add(data);
            }
            target.Sort((a, b) => a.encounterID.CompareTo(b.encounterID));
            if (autoUnlockFirst && target.Count > 0)
            {
                target[0].isUnlocked = true;
            }
            Debug.Log($"[EncounterManager] Loaded {target.Count} encounters from Resources/{path}: " + string.Join(", ", target.ConvertAll(e => e.encounterID.ToString()).ToArray()));
        }
        else
        {
            Debug.Log($"[EncounterManager] No encounters found at Resources/{path}");
        }
    }

    [Header("Encounter Data by Act")]
    public List<EncounterData> act1Encounters = new List<EncounterData>();
    public List<EncounterData> act2Encounters = new List<EncounterData>();
    public List<EncounterData> act3Encounters = new List<EncounterData>();
    public List<EncounterData> act4Encounters = new List<EncounterData>();

    [Header("Resources Loading (per Act)")]
    [Tooltip("Load EncounterDataAsset ScriptableObjects from Resources paths")] public bool loadFromResources = true;
    public string act1ResourcesPath = "Encounters/Act1";
    public string act2ResourcesPath = "Encounters/Act2";
    public string act3ResourcesPath = "Encounters/Act3";
    public string act4ResourcesPath = "Encounters/Act4";
    
    [Header("Current Encounter")]
    public int currentEncounterID = 0;
    
    private void Awake()
    {
        // Singleton pattern - only one instance should exist
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureEnemyDatabase();
            // Restore pending encounter id if any (ensures continuity across scene reloads)
            int pendingId = PlayerPrefs.GetInt("PendingEncounterID", 0);
            InitializeEncounters();
            if (currentEncounterID == 0 && pendingId > 0)
            {
                currentEncounterID = pendingId;
                PlayerPrefs.SetInt("PendingEncounterID", 0);
                PlayerPrefs.Save();
                Debug.Log($"[EncounterDebug] Restored pending encounter id {currentEncounterID} on Awake.");
            }
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void EnsureEnemyDatabase()
    {
        if (EnemyDatabase.Instance == null)
        {
            if (enemyDatabaseRef != null)
            {
                var dbGO = Instantiate(enemyDatabaseRef.gameObject);
                dbGO.name = "EnemyDatabase";
                DontDestroyOnLoad(dbGO);
            }
            else
            {
                var go = new GameObject("EnemyDatabase");
                go.AddComponent<EnemyDatabase>();
                DontDestroyOnLoad(go);
            }
        }
        // Reload to ensure content is available when entering from gameplay flow
        if (EnemyDatabase.Instance != null && (EnemyDatabase.Instance.allEnemies == null || EnemyDatabase.Instance.allEnemies.Count == 0))
        {
            EnemyDatabase.Instance.ReloadDatabase();
        }
    }

    private void InitializeEncounters()
    {
        act1Encounters.Clear();
        act2Encounters.Clear();
        act3Encounters.Clear();
        act4Encounters.Clear();

        if (loadFromResources)
        {
            LoadActFromResources(act1ResourcesPath, act1Encounters, autoUnlockFirst: true);
            LoadActFromResources(act2ResourcesPath, act2Encounters, autoUnlockFirst: false);
            LoadActFromResources(act3ResourcesPath, act3Encounters, autoUnlockFirst: false);
            LoadActFromResources(act4ResourcesPath, act4Encounters, autoUnlockFirst: false);

            if (act1Encounters.Count + act2Encounters.Count + act3Encounters.Count + act4Encounters.Count > 0)
                return;
        }

        // Fallback hardcoded defaults for Act 1 only if no assets found
        act1Encounters.Add(new EncounterData(1, "Where Night First Fell", "CombatScene"));
        act1Encounters.Add(new EncounterData(2, "The Wretched Shore", "CombatScene"));
        act1Encounters.Add(new EncounterData(3, "Tidal Island", "CombatScene"));
    }
    
    public void StartEncounter(int encounterID)
    {
        EncounterData encounter = GetEncounterByID(encounterID);
        if (encounter == null)
        {
            Debug.LogWarning($"[EncounterManager] Encounter ID {encounterID} not found in any act.");
            return;
        }
        if (!encounter.isUnlocked)
        {
            Debug.LogWarning($"[EncounterManager] Encounter {encounterID} is locked.");
            return;
        }
        currentEncounterID = encounterID;
        // Persist encounter id so a newly created EncounterManager in the next scene can restore it
        PlayerPrefs.SetInt("PendingEncounterID", encounterID);
        PlayerPrefs.Save();
        Debug.Log($"[EncounterDebug] [EncounterManager] Loading scene '{encounter.sceneName}' for encounter {encounterID}");
        SceneManager.LoadScene(encounter.sceneName);
    }
    
    public EncounterData GetEncounterByID(int encounterID)
    {
        // Search all acts for robustness
        EncounterData encounter = act1Encounters.Find(e => e.encounterID == encounterID);
        if (encounter != null) return encounter;
        encounter = act2Encounters.Find(e => e.encounterID == encounterID);
        if (encounter != null) return encounter;
        encounter = act3Encounters.Find(e => e.encounterID == encounterID);
        if (encounter != null) return encounter;
        encounter = act4Encounters.Find(e => e.encounterID == encounterID);
        return encounter;
    }
    
    public EncounterData GetCurrentEncounter()
    {
        return GetEncounterByID(currentEncounterID);
    }
    
    public int GetCurrentAreaLevel()
    {
        var enc = GetCurrentEncounter();
        return enc != null ? enc.areaLevel : 1;
    }
    
    public void CompleteCurrentEncounter()
    {
        EncounterData encounter = GetCurrentEncounter();
        if (encounter != null)
        {
            encounter.isCompleted = true;
            // Unlock next encounter if it exists
            EncounterData nextEncounter = GetEncounterByID(currentEncounterID + 1);
            if (nextEncounter != null)
            {
                nextEncounter.isUnlocked = true;
            }
        }
    }
    
    public void ReturnToMainUI()
    {
        SceneManager.LoadScene("MainGameUI");
    }
}

#if UNITY_EDITOR
public partial class EncounterManager
{
    [ContextMenu("Encounters/Import From Resources")] 
    private void Editor_ImportFromResources()
    {
        Undo.RecordObject(this, "Import Encounters From Resources");

        act1Encounters.Clear();
        act2Encounters.Clear();
        act3Encounters.Clear();
        act4Encounters.Clear();

        LoadActFromResources(act1ResourcesPath, act1Encounters, autoUnlockFirst: true);
        LoadActFromResources(act2ResourcesPath, act2Encounters, autoUnlockFirst: false);
        LoadActFromResources(act3ResourcesPath, act3Encounters, autoUnlockFirst: false);
        LoadActFromResources(act4ResourcesPath, act4Encounters, autoUnlockFirst: false);

        EditorUtility.SetDirty(this);
        Debug.Log("[EncounterManager] Imported encounters from Resources into serialized lists.");
    }

    [ContextMenu("Encounters/Clear Imported Lists")] 
    private void Editor_ClearImportedLists()
    {
        Undo.RecordObject(this, "Clear Imported Encounter Lists");
        act1Encounters.Clear();
        act2Encounters.Clear();
        act3Encounters.Clear();
        act4Encounters.Clear();
        EditorUtility.SetDirty(this);
        Debug.Log("[EncounterManager] Cleared serialized encounter lists.");
    }
}
#endif

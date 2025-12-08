using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Refactored EncounterManager - coordinates encounter system components.
/// Separated concerns into focused managers for better maintainability.
/// </summary>
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
                }
            }
            // Ensure system is initialized if it exists but wasn't initialized
            if (_instance != null && !_instance.isInitialized)
            {
                Debug.LogWarning("[EncounterManager] Instance exists but not initialized. Initializing now...");
                _instance.InitializeSystem();
            }
            return _instance;
        }
    }
    
    [Header("Dependencies")]
    [Tooltip("Optional: assign an EnemyDatabase prefab or scene object to ensure it exists before encounters run.")]
    public EnemyDatabase enemyDatabaseRef;

    [Header("Resources Loading (per Act)")]
    [Tooltip("Load EncounterDataAsset ScriptableObjects from Resources paths")]
    public bool loadFromResources = true;
    public string act1ResourcesPath = "Encounters/Act1";
    public string act2ResourcesPath = "Encounters/Act2";
    public string act3ResourcesPath = "Encounters/Act3";
    public string act4ResourcesPath = "Encounters/Act4";
    
    [Header("Current Encounter")]
    public int currentEncounterID = 0;

    // Component managers
    private EncounterGraphBuilder graphBuilder;
    private EncounterStateManager stateManager;
    private EncounterProgressionManager progressionManager;
    
    // Encounter data storage
    private Dictionary<int, EncounterData> allEncounters = new Dictionary<int, EncounterData>();
    private Dictionary<int, List<EncounterData>> encountersByAct = new Dictionary<int, List<EncounterData>>();
    
    // Track initialization state
    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;
    
    // Legacy support (for backwards compatibility)
    [Header("Legacy Support (Editor Only)")]
    public List<EncounterData> act1Encounters = new List<EncounterData>();
    public List<EncounterData> act2Encounters = new List<EncounterData>();
    public List<EncounterData> act3Encounters = new List<EncounterData>();
    public List<EncounterData> act4Encounters = new List<EncounterData>();
    
    private void Awake()
    {
        // Singleton pattern
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSystem();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    
    /// <summary>
    /// Initialize the encounter system.
    /// </summary>
    private void InitializeSystem()
    {
        EnsureEnemyDatabase();
        
        // Initialize component managers
        graphBuilder = new EncounterGraphBuilder();
        stateManager = new EncounterStateManager(graphBuilder);
        progressionManager = new EncounterProgressionManager(graphBuilder, stateManager);
        
        // Load encounters
        LoadEncounters();
        
        // Build graph
        BuildEncounterGraph();
        
        // Apply character progression (this ensures encounter 1 is unlocked)
        ApplyCharacterProgression();
        
        // Double-check: Ensure encounter 1 is unlocked after progression is applied
        var encounter1Prog = progressionManager.GetProgression(1);
        Debug.Log($"[EncounterManager] After ApplyCharacterProgression, encounter 1 progression: {(encounter1Prog != null ? $"Unlocked={encounter1Prog.isUnlocked}, Completed={encounter1Prog.isCompleted}" : "NULL")}");
        
        if (encounter1Prog == null || !encounter1Prog.isUnlocked)
        {
            Debug.LogWarning($"[EncounterManager] Encounter 1 was {(encounter1Prog == null ? "MISSING" : "LOCKED")} after progression! Force-unlocking...");
            progressionManager.MarkUnlocked(1);
            
            // Verify again
            encounter1Prog = progressionManager.GetProgression(1);
            Debug.Log($"[EncounterManager] After force-unlock, encounter 1: {(encounter1Prog != null ? $"Unlocked={encounter1Prog.isUnlocked}" : "STILL NULL!")}");
        }
        
        // Final verification
        bool isUnlocked = IsEncounterUnlocked(1);
        Debug.Log($"[EncounterManager] Final check - IsEncounterUnlocked(1): {isUnlocked}");
        
        // Restore pending encounter ID
        int pendingId = PlayerPrefs.GetInt("PendingEncounterID", 0);
        if (currentEncounterID == 0 && pendingId > 0)
        {
            currentEncounterID = pendingId;
            PlayerPrefs.SetInt("PendingEncounterID", 0);
            PlayerPrefs.Save();
            Debug.Log($"[EncounterManager] Restored pending encounter ID {currentEncounterID}");
        }
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        isInitialized = true;
        EncounterEvents.InvokeSystemInitialized();
        Debug.Log($"[EncounterManager] System initialized - Encounter 1 is always available. Loaded {allEncounters.Count} encounters.");
    }
    
    /// <summary>
    /// Load encounters from Resources or use fallback data.
    /// </summary>
    private void LoadEncounters()
        {
        allEncounters.Clear();
        encountersByAct.Clear();
        
        if (loadFromResources)
        {
            // Load from Resources using EncounterDataLoader
            encountersByAct = EncounterDataLoader.LoadAllEncounters(
                act1ResourcesPath,
                act2ResourcesPath,
                act3ResourcesPath,
                act4ResourcesPath
            );
            
            // Build unified lookup
            foreach (var actEncounters in encountersByAct.Values)
            {
                foreach (var encounter in actEncounters)
                {
                    if (encounter != null && encounter.encounterID > 0)
            {
                        allEncounters[encounter.encounterID] = encounter;
                    }
                }
            }
            
            // Update legacy lists for editor compatibility
            act1Encounters = encountersByAct.ContainsKey(1) ? encountersByAct[1] : new List<EncounterData>();
            act2Encounters = encountersByAct.ContainsKey(2) ? encountersByAct[2] : new List<EncounterData>();
            act3Encounters = encountersByAct.ContainsKey(3) ? encountersByAct[3] : new List<EncounterData>();
            act4Encounters = encountersByAct.ContainsKey(4) ? encountersByAct[4] : new List<EncounterData>();
            
            // Validate loaded data
            ValidateEncounters();
            
            EncounterEvents.InvokeDataLoaded();
            }
            else
            {
            // Fallback hardcoded defaults
            CreateFallbackEncounters();
            }
        }
    
    /// <summary>
    /// Create fallback encounters if Resources loading fails.
    /// </summary>
    private void CreateFallbackEncounters()
    {
        var fallback1 = new EncounterData(1, "Where Night First Fell", "CombatScene");
        fallback1.isUnlocked = true;
        var fallback2 = new EncounterData(2, "The Wretched Shore", "CombatScene") { isUnlocked = false };
        var fallback3 = new EncounterData(3, "Tidal Island", "CombatScene") { isUnlocked = false };
        
        act1Encounters = new List<EncounterData> { fallback1, fallback2, fallback3 };
        encountersByAct[1] = act1Encounters;
        
        foreach (var encounter in act1Encounters)
    {
            allEncounters[encounter.encounterID] = encounter;
        }
        
        Debug.LogWarning("[EncounterManager] Using fallback encounters. Check Resources paths.");
    }
    
    /// <summary>
    /// Validate loaded encounters.
    /// </summary>
    private void ValidateEncounters()
    {
        var allEncounterList = allEncounters.Values.ToList();
        var validationResult = EncounterValidator.ValidateEncounters(allEncounterList);
        
        if (!validationResult.IsValid)
        {
            Debug.LogError($"[EncounterManager] Encounter validation failed:\n{validationResult}");
        }
        else if (validationResult.warnings.Count > 0)
        {
            Debug.LogWarning($"[EncounterManager] Encounter validation warnings:\n{validationResult}");
        }
        else
        {
            Debug.Log("[EncounterManager] All encounters validated successfully.");
        }
    }
    
    /// <summary>
    /// Build the encounter graph.
    /// </summary>
    private void BuildEncounterGraph()
    {
        graphBuilder.BuildGraph(allEncounters.Values);
        EncounterEvents.InvokeGraphBuilt();
        Debug.Log($"[EncounterManager] Graph built with {allEncounters.Count} encounters");
                }
    
    /// <summary>
    /// Apply character progression to the encounter system.
    /// </summary>
    public void ApplyCharacterProgression()
    {
        var character = CharacterManager.Instance != null ? CharacterManager.Instance.GetCurrentCharacter() : null;
        Debug.Log($"[EncounterManager] ApplyCharacterProgression - Character: {(character != null ? character.characterName : "NULL")}");
        
        Dictionary<int, EncounterProgressionData> characterProgression = new Dictionary<int, EncounterProgressionData>();
        
        if (character != null)
        {
            Debug.Log($"[EncounterManager] Character has completedEncounterIDs: {(character.completedEncounterIDs != null ? string.Join(", ", character.completedEncounterIDs) : "NULL")}");
            Debug.Log($"[EncounterManager] Character has unlockedEncounterIDs: {(character.unlockedEncounterIDs != null ? string.Join(", ", character.unlockedEncounterIDs) : "NULL")}");
            
            // Convert character's progression lists to EncounterProgressionData
            if (character.completedEncounterIDs != null)
            {
                foreach (var id in character.completedEncounterIDs)
                {
                    var progData = new EncounterProgressionData(id);
                    progData.isCompleted = true;
                    progData.completionCount = 1; // Legacy: assume 1 completion
                    characterProgression[id] = progData;
                    Debug.Log($"[EncounterManager] Added completed encounter {id} to progression");
                }
            }
            
            if (character.unlockedEncounterIDs != null)
            {
                foreach (var id in character.unlockedEncounterIDs)
                {
                    if (!characterProgression.TryGetValue(id, out var progData))
                    {
                        progData = new EncounterProgressionData(id);
                        characterProgression[id] = progData;
                    }
                    progData.isUnlocked = true;
                    Debug.Log($"[EncounterManager] Added unlocked encounter {id} to progression");
                }
            }
        }
        
        // Ensure encounter 1 is always unlocked, regardless of character data
        if (!characterProgression.ContainsKey(1))
        {
            var encounter1Prog = new EncounterProgressionData(1);
            encounter1Prog.MarkUnlocked();
            characterProgression[1] = encounter1Prog;
            Debug.Log($"[EncounterManager] Added encounter 1 to progression (was missing) - Unlocked: {encounter1Prog.isUnlocked}");
        }
        else if (!characterProgression[1].isUnlocked)
        {
            characterProgression[1].MarkUnlocked();
            Debug.Log($"[EncounterManager] Force-unlocked encounter 1 (was locked in character data)");
        }
        else
        {
            Debug.Log($"[EncounterManager] Encounter 1 already in progression and unlocked");
        }
        
        // Load progression into progression manager
        progressionManager.LoadProgression(characterProgression);
        
        // Verify encounter 1 after loading
        var verifyProg = progressionManager.GetProgression(1);
        Debug.Log($"[EncounterManager] After LoadProgression, encounter 1 progression: {(verifyProg != null ? $"Unlocked={verifyProg.isUnlocked}, Completed={verifyProg.isCompleted}" : "NULL")}");
        
        EncounterEvents.InvokeProgressionApplied();
        Debug.Log($"[EncounterManager] Applied progression for {(character != null ? character.characterName : "no character")}. Encounter 1 is always unlocked.");
    }
    
    /// <summary>
    /// Start an encounter.
    /// </summary>
    public void StartEncounter(int encounterID)
    {
        // Ensure system is initialized before starting encounter
        if (!isInitialized)
        {
            Debug.LogWarning("[EncounterManager] System not initialized when StartEncounter called. Initializing now...");
            InitializeSystem();
        }
        
        EncounterData encounter = GetEncounterByID(encounterID);
        if (encounter == null)
        {
            Debug.LogError($"[EncounterManager] Encounter ID {encounterID} not found. Total encounters loaded: {allEncounters.Count}");
            Debug.LogError($"[EncounterManager] Available encounter IDs: {string.Join(", ", allEncounters.Keys)}");
            return;
        }
        
        if (!IsEncounterUnlocked(encounterID))
        {
            Debug.LogWarning($"[EncounterManager] Encounter {encounterID} is locked.");
            return;
        }
        
        currentEncounterID = encounterID;
        
        // Mark as attempted
        progressionManager.MarkAttempted(encounterID);
        
        // Mark as entered
        var character = CharacterManager.Instance != null ? CharacterManager.Instance.GetCurrentCharacter() : null;
        if (character != null)
        {
            character.MarkEncounterEntered(encounterID);
            CharacterManager.Instance.SaveCharacter();
        }
        
        EncounterEvents.InvokeEncounterEntered(encounterID);
        
        // Persist encounter ID for scene transition
        PlayerPrefs.SetInt("PendingEncounterID", encounterID);
        PlayerPrefs.Save();
        
        Debug.Log($"[EncounterManager] Starting encounter {encounterID}: {encounter.encounterName}");
        SceneManager.LoadScene(encounter.sceneName);
    }
    
    /// <summary>
    /// Complete the current encounter.
    /// </summary>
    public void CompleteCurrentEncounter(float completionTime = 0f, int score = 0)
    {
        if (currentEncounterID == 0)
        {
            Debug.LogWarning("[EncounterManager] No current encounter to complete.");
            return;
        }
        
        EncounterData encounter = GetEncounterByID(currentEncounterID);
        if (encounter == null)
        {
            Debug.LogWarning($"[EncounterManager] Current encounter {currentEncounterID} not found.");
            return;
        }
        
        // Mark as completed in progression manager
        progressionManager.MarkCompleted(currentEncounterID, completionTime, score);
        
        // Update character
        var character = CharacterManager.Instance != null ? CharacterManager.Instance.GetCurrentCharacter() : null;
        if (character != null)
        {
            character.MarkEncounterCompleted(currentEncounterID);
            
            // Sync progression back to character (for backwards compatibility)
            var progData = progressionManager.GetProgression(currentEncounterID);
            if (progData != null && progData.isUnlocked)
            {
                character.MarkEncounterUnlocked(currentEncounterID);
            }
            
            // Reset player state after combat (full HP, full mana, clear combat buffs)
            ResetPlayerStateAfterCombat(character);
            
            // Save equipment to ensure persistence
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.SaveEquipmentData();
                Debug.Log("[EncounterManager] Equipment data saved after combat");
            }
            
            CharacterManager.Instance.SaveCharacter();
        }
        
        Debug.Log($"[EncounterManager] Completed encounter {currentEncounterID}: {encounter.encounterName}");
    }
    
    /// <summary>
    /// Reset player state after combat (restore HP/mana, clear combat-acquired buffs)
    /// </summary>
    private void ResetPlayerStateAfterCombat(Character character)
    {
        if (character == null) return;
        
        // Restore to full health
        character.currentHealth = character.maxHealth;
        Debug.Log($"[EncounterManager] Reset player HP to {character.maxHealth}");
        
        // Restore to full mana
        character.mana = character.maxMana;
        Debug.Log($"[EncounterManager] Reset player mana to {character.maxMana}");
        
        // Clear temporary combat buffs (e.g., "Temporary Dexterity")
        // Clear momentum stacks using StackSystem singleton
        if (StackSystem.Instance != null)
        {
            // Clear momentum
            int momentumCleared = StackSystem.Instance.GetStacks(StackType.Momentum);
            if (momentumCleared > 0)
            {
                StackSystem.Instance.ClearStacks(StackType.Momentum);
                Debug.Log($"[EncounterManager] Cleared {momentumCleared} momentum stacks");
            }
            
            // Clear other temporary combat stacks (if needed)
            // Example: Flow, Potential, etc. - only clear if they're temporary
            // Note: Agitate, Tolerance typically persist, so don't clear those
        }
        
        // Clear temporary stat modifiers (if you have a system for this)
        // Note: If you have temporary dexterity/strength stored elsewhere, clear those here
        
        // Reset stagger
        character.currentStagger = 0f;
        Debug.Log("[EncounterManager] Reset player stagger to 0");
        
        // Reset guard
        character.currentGuard = 0f;
        Debug.Log("[EncounterManager] Reset player guard to 0");
        
        Debug.Log("[EncounterManager] Player state reset complete");
    }
    
    /// <summary>
    /// Get encounter by ID.
    /// </summary>
    public EncounterData GetEncounterByID(int encounterID)
    {
        // Ensure system is initialized before getting encounter
        if (!isInitialized)
        {
            Debug.LogWarning("[EncounterManager] System not initialized when GetEncounterByID called. Initializing now...");
            InitializeSystem();
        }
        
        if (allEncounters.TryGetValue(encounterID, out var encounter))
        {
            return encounter;
        }
        return null;
    }
    
    /// <summary>
    /// Get current encounter.
    /// </summary>
    public EncounterData GetCurrentEncounter()
    {
        return GetEncounterByID(currentEncounterID);
    }
    
    /// <summary>
    /// Get current area level.
    /// </summary>
    public int GetCurrentAreaLevel()
    {
        var enc = GetCurrentEncounter();
        return enc != null ? enc.areaLevel : 1;
    }

    /// <summary>
    /// Check if an encounter is unlocked.
    /// </summary>
    public bool IsEncounterUnlocked(int encounterID)
    {
        // Encounter 1 is always unlocked
        if (encounterID == 1)
        {
            Debug.Log($"[EncounterManager] IsEncounterUnlocked({encounterID}) - Encounter 1, returning TRUE (always unlocked)");
            return true;
        }
        
        bool result = progressionManager != null ? progressionManager.IsUnlocked(encounterID) : false;
        Debug.Log($"[EncounterManager] IsEncounterUnlocked({encounterID}) - ProgressionManager result: {result}");
        return result;
    }
    
    /// <summary>
    /// Check if an encounter is completed.
    /// </summary>
    public bool IsEncounterCompleted(int encounterID)
    {
        return progressionManager.IsCompleted(encounterID);
    }
    
    /// <summary>
    /// Get prerequisites for an encounter.
    /// </summary>
    public IReadOnlyList<int> GetPrerequisitesForEncounter(int encounterID)
    {
        return graphBuilder.GetPrerequisites(encounterID);
    }
    
    /// <summary>
    /// Get all encounter nodes (for UI).
    /// </summary>
    public IReadOnlyDictionary<int, EncounterData> GetEncounterNodes()
    {
        return allEncounters;
    }
    
    /// <summary>
    /// Get progression data for an encounter.
    /// </summary>
    public EncounterProgressionData GetProgression(int encounterID)
    {
        return progressionManager.GetProgression(encounterID);
                        }
    
    /// <summary>
    /// Return to main UI.
    /// </summary>
    public void ReturnToMainUI()
    {
        SceneManager.LoadScene("MainGameUI");
    }

    /// <summary>
    /// Initialize encounter graph (public API for external initialization).
    /// </summary>
    public void InitializeEncounterGraph()
    {
        Debug.Log("[EncounterManager] InitializeEncounterGraph called");
        LoadEncounters();
        BuildEncounterGraph();
        ApplyCharacterProgression();
        isInitialized = true;
        Debug.Log($"[EncounterManager] Encounter graph initialized. {allEncounters.Count} encounters loaded.");
    }

    /// <summary>
    /// Handle scene loaded event.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainGameUI")
        {
            Debug.Log("[EncounterManager] Scene loaded: MainGameUI. Refreshing encounter system.");
            // Re-initialize the graph (this will reload encounters and apply progression)
            InitializeEncounterGraph();
            // Mark as initialized after refresh
            isInitialized = true;
            Debug.Log($"[EncounterManager] Graph refreshed. {allEncounters.Count} encounters available.");
        }
    }

    /// <summary>
    /// Ensure EnemyDatabase exists.
    /// </summary>
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
        
        if (EnemyDatabase.Instance != null && (EnemyDatabase.Instance.allEnemies == null || EnemyDatabase.Instance.allEnemies.Count == 0))
        {
            EnemyDatabase.Instance.ReloadDatabase();
        }
    }
    
    // Legacy event for backwards compatibility
    public event System.Action EncounterGraphChanged;
    
    private void RaiseEncounterGraphChanged()
                        {
        EncounterGraphChanged?.Invoke();
        EncounterEvents.InvokeGraphBuilt();
    }
}

#if UNITY_EDITOR
public partial class EncounterManager
{
    [ContextMenu("Encounters/Import From Resources")] 
    private void Editor_ImportFromResources()
    {
        Undo.RecordObject(this, "Import Encounters From Resources");

        LoadEncounters();
        BuildEncounterGraph();
        
        EditorUtility.SetDirty(this);
        Debug.Log("[EncounterManager] Imported encounters from Resources.");
    }
    
    [ContextMenu("Encounters/Validate Encounters")]
    private void Editor_ValidateEncounters()
    {
        var allEncounterList = allEncounters.Values.ToList();
        var result = EncounterValidator.ValidateEncounters(allEncounterList);
        
        if (result.IsValid)
            {
            EditorUtility.DisplayDialog("Validation Result", $"Validation passed!\n\n{result}", "OK");
            }
        else
        {
            EditorUtility.DisplayDialog("Validation Result", $"Validation failed!\n\n{result}", "OK");
        }
    }

    [ContextMenu("Encounters/Reset Progression")]
    private void Editor_ResetProgression()
    {
        if (EditorUtility.DisplayDialog("Reset Progression", "This will reset all encounter progression. Continue?", "Yes", "No"))
        {
            progressionManager?.Clear();
            stateManager?.Clear();
            ApplyCharacterProgression();
            Debug.Log("[EncounterManager] Progression reset.");
        }
    }
}
#endif

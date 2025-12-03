using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Centralized enemy spawning system that manages dynamic enemy display instantiation
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Prefab for enemy display panels")]
    public GameObject enemyDisplayPrefab;
    
    [Tooltip("Parent transform where enemy displays will be spawned")]
    public Transform enemyContainer;
    
    [Tooltip("Spawn positions for enemies (left to right)")]
    public Transform[] spawnPoints;
    
    [Header("Boss Spawn")]
    [Tooltip("Optional override transform where unique bosses should spawn")]
    public Transform bossSpawnPoint;
    
    [Header("Pooling Settings")]
    [Tooltip("Enable object pooling for better performance")]
    public bool usePooling = true;
    
    [Tooltip("Initial pool size")]
    public int initialPoolSize = 5;
    
    [Header("Spawn Animations")]
    [Tooltip("Enable sweep-in animations for new enemies")]
    public bool enableSpawnAnimations = true;
    
    [Tooltip("Distance enemies start from (off-screen left)")]
    public float spawnStartDistance = -800f;
    
    [Tooltip("Duration of the sweep-in animation")]
    public float spawnAnimationDuration = 0.8f;
    
    [Tooltip("Animation curve for the sweep-in motion")]
    public AnimationCurve spawnAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Tooltip("Stagger delay between multiple enemy spawns")]
    public float spawnStaggerDelay = 0.2f;
    
    // Active enemy displays currently in the scene
    private List<EnemyCombatDisplay> activeEnemies = new List<EnemyCombatDisplay>();
    
    // Object pool for reuse
    private Queue<EnemyCombatDisplay> enemyPool = new Queue<EnemyCombatDisplay>();
    
    private void Awake()
    {
        if (usePooling)
        {
            InitializePool();
        }
    }
    
    /// <summary>
    /// Initialize the object pool with pre-created instances
    /// </summary>
    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject enemyGO = Instantiate(enemyDisplayPrefab, enemyContainer);
            enemyGO.name = $"PooledEnemy_{i}";
            enemyGO.SetActive(false);
            
            EnemyCombatDisplay display = enemyGO.GetComponent<EnemyCombatDisplay>();
            if (display != null)
            {
                enemyPool.Enqueue(display);
            }
            else
            {
                Debug.LogError($"[EnemySpawner] Prefab missing EnemyCombatDisplay component!");
                Destroy(enemyGO);
            }
        }
        
        Debug.Log($"[EnemySpawner] Initialized pool with {enemyPool.Count} enemies");
    }
    
    /// <summary>
    /// Spawn a new enemy display at the specified index
    /// </summary>
    public EnemyCombatDisplay SpawnEnemy(EnemyData enemyData, int spawnIndex)
    {
        if (enemyData == null)
        {
            Debug.LogWarning("[EnemySpawner] Cannot spawn null enemy data");
            return null;
        }
        
        if (spawnIndex < 0 || spawnIndex >= spawnPoints.Length)
        {
            Debug.LogWarning($"[EnemySpawner] Invalid spawn index {spawnIndex}, max is {spawnPoints.Length - 1}");
            return null;
        }
        
        // Get enemy display from pool or create new
        EnemyCombatDisplay enemyDisplay = GetEnemyDisplay();
        
        if (enemyDisplay == null)
        {
            Debug.LogError("[EnemySpawner] Failed to get enemy display!");
            return null;
        }
        
        // Reset pooled enemy state before reuse
        if (!enemyDisplay.gameObject.activeInHierarchy)
        {
            // Ensure it's properly reset for reuse
            enemyDisplay.gameObject.SetActive(false); // Make sure it's off first
        }
        
        // Position at spawn point
        enemyDisplay.transform.SetParent(spawnPoints[spawnIndex], false);
        enemyDisplay.transform.localPosition = Vector3.zero;
        enemyDisplay.transform.localScale = Vector3.one;
        enemyDisplay.transform.localRotation = Quaternion.identity; // Reset rotation
        
        // Get area level for scaling (from EncounterManager or maze context)
        int areaLevel = GetAreaLevel();
        
        // Roll rarity for all enemies (Normal, Magic, or Rare)
        // Unique enemies are typically scripted/bosses and set manually
        EnemyRarity rolledRarity = RollRarityForEncounter();
        Enemy enemy = enemyData.CreateEnemyWithRarity(rolledRarity, areaLevel);
        Debug.Log($"[EnemySpawner] Spawned {enemyData.enemyName} with rarity {rolledRarity} (Area Level {areaLevel})");
        
        enemyDisplay.SetEnemy(enemy, enemyData);
        
        // Activate and track
        enemyDisplay.gameObject.SetActive(true);
        
        // Force update display immediately to ensure enemy name/data shows
        enemyDisplay.RefreshDisplay();
        
        // Ensure visibility components are enabled
        var canvasGroup = enemyDisplay.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        activeEnemies.Add(enemyDisplay);
        
        // Apply spawn animation if enabled
        if (enableSpawnAnimations)
        {
            StartSpawnAnimation(enemyDisplay, spawnIndex);
        }
        
        Debug.Log($"[EnemySpawner] Spawned {enemyData.enemyName} at spawn point {spawnIndex}");
        
        return enemyDisplay;
    }
    
    /// <summary>
    /// Get an enemy display from pool or create new
    /// </summary>
    private EnemyCombatDisplay GetEnemyDisplay()
    {
        // Try to get from pool
        if (usePooling && enemyPool.Count > 0)
        {
            EnemyCombatDisplay display = enemyPool.Dequeue();
            Debug.Log($"[EnemySpawner] Reusing pooled enemy: {display.name}");
            return display;
        }
        
        // Create new instance
        GameObject enemyGO = Instantiate(enemyDisplayPrefab, enemyContainer);
        enemyGO.name = $"DynamicEnemy_{activeEnemies.Count}";
        
        EnemyCombatDisplay newDisplay = enemyGO.GetComponent<EnemyCombatDisplay>();
        if (newDisplay == null)
        {
            Debug.LogError("[EnemySpawner] Prefab missing EnemyCombatDisplay component!");
            Destroy(enemyGO);
            return null;
        }
        
        Debug.Log($"[EnemySpawner] Created new enemy display: {enemyGO.name}");
        return newDisplay;
    }
    
    /// <summary>
    /// Despawn a specific enemy display
    /// </summary>
    public void DespawnEnemy(EnemyCombatDisplay enemyDisplay)
    {
        if (enemyDisplay == null) return;
        
        // Remove from active list
        activeEnemies.Remove(enemyDisplay);
        
        // Clear enemy data
        enemyDisplay.ClearEnemy();
        
        // Return to pool or destroy
        if (usePooling)
        {
            enemyDisplay.gameObject.SetActive(false);
            enemyDisplay.transform.SetParent(enemyContainer);
            enemyPool.Enqueue(enemyDisplay);
            Debug.Log($"[EnemySpawner] Returned {enemyDisplay.name} to pool");
        }
        else
        {
            Destroy(enemyDisplay.gameObject);
            Debug.Log($"[EnemySpawner] Destroyed {enemyDisplay.name}");
        }
    }
    
    /// <summary>
    /// Despawn all active enemies (for wave transitions)
    /// </summary>
    public void DespawnAllEnemies()
    {
        Debug.Log($"[EnemySpawner] Despawning {activeEnemies.Count} active enemies");
        
        // Create a copy of the list to avoid modification during iteration
        List<EnemyCombatDisplay> enemiesToDespawn = new List<EnemyCombatDisplay>(activeEnemies);
        
        foreach (var enemy in enemiesToDespawn)
        {
            DespawnEnemy(enemy);
        }
        
        activeEnemies.Clear();
        
        Debug.Log($"[EnemySpawner] All enemies despawned. Pool size: {enemyPool.Count}");
    }
    
    /// <summary>
    /// Get all currently active enemy displays
    /// </summary>
    public List<EnemyCombatDisplay> GetActiveEnemies()
    {
        return new List<EnemyCombatDisplay>(activeEnemies);
    }
    
    /// <summary>
    /// Get count of active enemies
    /// </summary>
    public int GetActiveEnemyCount()
    {
        return activeEnemies.Count;
    }
    
    /// <summary>
    /// Check if a spawn point is available
    /// </summary>
    public bool IsSpawnPointAvailable(int index)
    {
        return index >= 0 && index < spawnPoints.Length;
    }
    
    /// <summary>
    /// Get max number of spawn points
    /// </summary>
    public int GetMaxSpawnPoints()
    {
        return spawnPoints != null ? spawnPoints.Length : 0;
    }
    
    /// <summary>
    /// Validate spawner setup
    /// </summary>
    private void OnValidate()
    {
        if (enemyDisplayPrefab == null)
        {
            Debug.LogWarning("[EnemySpawner] Enemy Display Prefab not assigned!");
        }
        
        if (enemyContainer == null)
        {
            Debug.LogWarning("[EnemySpawner] Enemy Container not assigned!");
        }
        
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[EnemySpawner] No spawn points assigned!");
        }
    }
    
    /// <summary>
    /// Spawn multiple enemies with staggered animations
    /// </summary>
    public void SpawnEnemiesWithAnimation(List<EnemyData> enemiesToSpawn)
    {
        if (enemiesToSpawn == null || enemiesToSpawn.Count == 0)
        {
            Debug.LogWarning("[EnemySpawner] No enemies to spawn!");
            return;
        }
        
        Debug.Log($"[EnemySpawner] Spawning {enemiesToSpawn.Count} enemies with staggered animations");
        
        for (int i = 0; i < enemiesToSpawn.Count && i < spawnPoints.Length; i++)
        {
            float delay = enableSpawnAnimations ? i * spawnStaggerDelay : 0f;
            StartCoroutine(SpawnEnemyWithDelay(enemiesToSpawn[i], i, delay));
        }
    }
    
    /// <summary>
    /// Spawn a single enemy with a delay
    /// </summary>
    private System.Collections.IEnumerator SpawnEnemyWithDelay(EnemyData enemyData, int spawnIndex, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        
        SpawnEnemy(enemyData, spawnIndex);
    }
    
    /// <summary>
    /// Start the sweep-in animation for a spawned enemy
    /// </summary>
    private void StartSpawnAnimation(EnemyCombatDisplay enemyDisplay, int spawnIndex)
    {
        if (enemyDisplay == null) return;
        
        // Store the final position
        Vector3 finalPosition = enemyDisplay.transform.localPosition;
        
        // Start from off-screen left
        Vector3 startPosition = finalPosition + new Vector3(spawnStartDistance, 0, 0);
        enemyDisplay.transform.localPosition = startPosition;
        
        // Animate to final position
        LeanTween.moveLocal(enemyDisplay.gameObject, finalPosition, spawnAnimationDuration)
            .setEase(spawnAnimationCurve)
            .setOnComplete(() => {
                Debug.Log($"[EnemySpawner] Spawn animation completed for {enemyDisplay.name}");
            });
        
        Debug.Log($"[EnemySpawner] Started sweep-in animation for {enemyDisplay.name} from {startPosition} to {finalPosition}");
    }

    public void SpawnEnemyWithAnimationAtIndex(EnemyData enemyData, int spawnIndex, float delay = 0f)
    {
        if (enemyData == null) return;
        if (!IsSpawnPointAvailable(spawnIndex)) return;
        StartCoroutine(SpawnEnemyWithDelay(enemyData, spawnIndex, Mathf.Max(0f, delay)));
    }
    
    public int GetBossSpawnIndex()
    {
        if (bossSpawnPoint == null)
        {
            return spawnPoints != null && spawnPoints.Length > 0 ? spawnPoints.Length - 1 : 0;
        }
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] == bossSpawnPoint)
            {
                return i;
            }
        }
        return spawnPoints != null && spawnPoints.Length > 0 ? spawnPoints.Length - 1 : 0;
    }
    
    /// <summary>
    /// Checks if this is maze combat (has maze context).
    /// </summary>
    private bool IsMazeCombat()
    {
        string mazeContext = PlayerPrefs.GetString("MazeCombatContext", "");
        return !string.IsNullOrEmpty(mazeContext);
    }
    
    /// <summary>
    /// Gets the area level for enemy scaling (from EncounterManager or maze context).
    /// </summary>
    private int GetAreaLevel()
    {
        // Check if this is maze combat
        if (IsMazeCombat() && Dexiled.MazeSystem.MazeRunManager.Instance != null)
        {
            var run = Dexiled.MazeSystem.MazeRunManager.Instance.GetCurrentRun();
            if (run != null)
            {
                // Use floor number as area level for maze combat
                return run.currentFloor;
            }
        }
        
        // Check EncounterManager for regular encounters
        if (EncounterManager.Instance != null)
        {
            var encounter = EncounterManager.Instance.GetCurrentEncounter();
            if (encounter != null)
            {
                return Mathf.Max(1, encounter.areaLevel);
            }
        }
        
        // Default fallback
        return 1;
    }
    
    /// <summary>
    /// Roll rarity for any encounter (Normal, Magic, or Rare).
    /// Unique enemies are typically scripted and set manually.
    /// </summary>
    private EnemyRarity RollRarityForEncounter()
    {
        // Check if this is maze combat
        bool isMazeCombat = IsMazeCombat();
        
        if (isMazeCombat)
        {
            int areaLevel = GetAreaLevel();
            return RollRarityForMaze(areaLevel);
        }
        else
        {
            // For regular combat, use EnemyDatabase rarity weights
            if (EnemyDatabase.Instance != null)
            {
                return EnemyDatabase.Instance.RollRarity(false); // Don't allow Unique for random spawns
            }
            return EnemyRarity.Normal; // Fallback
        }
    }
    
    /// <summary>
    /// Rolls enemy rarity for maze combat based on floor tier.
    /// Uses MazeConfig rarity weights if available, otherwise uses default EnemyDatabase weights.
    /// </summary>
    private EnemyRarity RollRarityForMaze(int floorNumber)
    {
        if (Dexiled.MazeSystem.MazeRunManager.Instance == null)
        {
            return EnemyDatabase.Instance != null ? EnemyDatabase.Instance.RollRarity(false) : EnemyRarity.Normal;
        }
        
        var mazeConfig = Dexiled.MazeSystem.MazeRunManager.Instance.mazeConfig;
        if (mazeConfig == null)
        {
            return EnemyDatabase.Instance != null ? EnemyDatabase.Instance.RollRarity(false) : EnemyRarity.Normal;
        }
        
        // Use MazeConfig's rarity weights for this floor
        return mazeConfig.RollRarityForFloor(floorNumber);
    }
}


using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Dexiled.MazeSystem;

public class CombatDisplayManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Optional root Transform that contains EnemyCombatDisplay panels (found even if inactive).")]
    public Transform enemyDisplayRoot;
    [Header("Combat Participants")]
    public PlayerCombatDisplay playerDisplay;
    
    [Header("Enemy Spawning")]
    [Tooltip("Dynamic enemy spawner that manages enemy display instantiation")]
    public EnemySpawner enemySpawner;
    
    [Header("Combat Settings")]
    public int maxEnemies = 3;
    public bool autoStartCombat = true;
    public float turnDelay = 1f;
    public float waveTransitionDelay = 2f; // Delay before spawning next wave
    
    [Header("Combat State")]
    public CombatState currentState = CombatState.Setup;
    public int currentTurn = 0;
    public bool isPlayerTurn = true;
	
	[Header("Waves")]
	public int totalWaves = 1; // 1 means old behavior
	public int currentWave = 0; // 1-based when active
	private int enemiesPerWave = 3; // Set from encounter or default
	private bool randomizeEnemyCount = true; // Whether to randomize enemy spawn count
	private bool isWaveTransitioning = false; // Prevent multiple wave transitions
	private Coroutine collectEnemiesCoroutine; // Track the collect coroutine
	private bool isAoEAttackInProgress = false; // Prevent cascading wave completion checks during AoE
    
    [Header("Test Configuration")]
    public bool createTestEnemies = true;
    public int testEnemyCount = 2;
    
    [Header("Enemy Data")]
    [SerializeField] private EnemyDatabase enemyDatabase;
    
    [Header("Loot System")]
    [Tooltip("Loot table for this combat encounter")]
    public LootTable encounterLootTable;
    [Tooltip("Optional AreaLootTable override for this combat encounter")]
    public AreaLootTable encounterAreaLootTable;
    private LootDropResult pendingLoot;
    private List<EnemyData> defeatedEnemiesData = new List<EnemyData>();
    private List<Enemy> defeatedEnemies = new List<Enemy>(); // Track actual Enemy instances for rarity modifiers
    
    [Header("Combat Effects")]
    private CombatEffectManager combatEffectManager;
    private FloatingDamageManager floatingDamageManager;
    private CardEffectProcessor cardEffectProcessor;
    
	// Combat events
    public System.Action<CombatState> OnCombatStateChanged;
    public System.Action<int> OnTurnChanged;
    public System.Action<bool> OnTurnTypeChanged;
    public System.Action<Enemy> OnEnemyDefeated;
    public System.Action OnCombatEnded;
	public System.Action<int, int> OnWaveChanged; // (current, total)
    
    private List<Enemy> activeEnemies = new List<Enemy>();
    private CharacterManager characterManager;
    private AnimatedCombatUI combatUI; // Updated to AnimatedCombatUI
    
    public enum CombatState
    {
        Setup,
        PlayerTurn,
        EnemyTurn,
        Victory,
        Defeat
    }
    
    private void Start()
    {
        InitializeCombat();
    }
    
    private void InitializeCombat()
    {
        // Get references
        characterManager = CharacterManager.Instance;
        combatUI = FindFirstObjectByType<AnimatedCombatUI>();
        floatingDamageManager = FindFirstObjectByType<FloatingDamageManager>();
        cardEffectProcessor = CardEffectProcessor.Instance;
        
        if (floatingDamageManager == null)
        {
            Debug.LogWarning("[CombatDisplayManager] FloatingDamageManager not found. Damage numbers will not display.");
        }
        
        // Auto-find player display if not assigned
        if (playerDisplay == null)
        {
            playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
        }
        
		// Validate enemy spawner is assigned
		if (enemySpawner == null)
		{
			enemySpawner = FindFirstObjectByType<EnemySpawner>();
			if (enemySpawner == null)
			{
				Debug.LogError("[CombatDisplayManager] EnemySpawner not found! Please assign EnemySpawner component.");
			}
			else
			{
				Debug.Log("[CombatDisplayManager] Auto-found EnemySpawner");
			}
		}
		
		if (enemySpawner != null)
		{
			Debug.Log($"[CombatDisplayManager] Enemy spawner ready with {enemySpawner.GetMaxSpawnPoints()} spawn points");
		}
		
		// Ensure EnemyDatabase singleton exists so spawning always works
		if (enemyDatabase == null)
		{
			enemyDatabase = EnemyDatabase.Instance;
			if (enemyDatabase == null)
			{
				var goDb = new GameObject("EnemyDatabase");
				enemyDatabase = goDb.AddComponent<EnemyDatabase>();
			}
		}
        
        // Get combat effect manager
        combatEffectManager = CombatEffectManager.Instance;
        
        // Check if this is a maze combat encounter
        bool isMazeCombat = IsMazeCombat();
        
        // Initialize waves and spawn first wave based on encounter if present; otherwise fall back
        if (totalWaves < 1) totalWaves = 1;
        var enc = EncounterManager.Instance != null ? EncounterManager.Instance.GetCurrentEncounter() : null;
        
        // Handle maze combat encounters
        if (isMazeCombat)
        {
            Debug.Log("[EncounterDebug] CombatDisplayManager: Maze combat detected. Starting maze combat.");
            autoStartCombat = false; // Disable auto-start for maze encounters
            createTestEnemies = false; // Don't create test enemies for maze
            
            // Set up maze combat defaults
            totalWaves = 1; // Maze combat is single wave
            enemiesPerWave = maxEnemies; // Use max enemies setting
            randomizeEnemyCount = true; // Randomize enemy count for maze
            
            // Start maze combat
            StartFirstWave();
            return; // Exit early after starting maze combat
        }
        
        if (enc != null)
        {
            totalWaves = Mathf.Max(1, enc.totalWaves);
            enemiesPerWave = Mathf.Max(1, enc.maxEnemiesPerWave);
            randomizeEnemyCount = enc.randomizeEnemyCount;
            encounterLootTable = enc.lootTable; // Set loot table from encounter
            encounterAreaLootTable = enc.areaLootTable;
            Debug.Log($"[EncounterDebug] CombatDisplayManager: Starting encounter {enc.encounterID} '{enc.encounterName}' with {totalWaves} waves and {enemiesPerWave} enemies per wave (randomize: {randomizeEnemyCount}).");
            
            // Verify loot table configuration for this encounter's area level
            if (AreaLootManager.Instance != null)
            {
                AreaLootManager.Instance.VerifyLootTableForArea(enc.areaLevel, $"Encounter '{enc.encounterName}' (ID: {enc.encounterID})");
            }
            
            StartFirstWave();
        }
        else if (createTestEnemies)
        {
            Debug.LogWarning("[EncounterDebug] CombatDisplayManager: No current encounter found on load. Spawning test enemies.");
            enemiesPerWave = testEnemyCount;
            randomizeEnemyCount = true;
            StartFirstWave();
        }
        
        // Start combat if auto-start is enabled
        if (autoStartCombat)
        {
            StartCombat();
        }
    }
    
    /// <summary>
    /// Checks if the current scene/encounter is a maze encounter (should not auto-start combat).
    /// </summary>
    private bool IsMazeEncounter()
    {
        // Check if current scene is MazeScene
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MazeScene")
        {
            return true;
        }
        
        // Check if encounter has sceneName = "MazeScene"
        if (EncounterManager.Instance != null)
        {
            var encounter = EncounterManager.Instance.GetCurrentEncounter();
            if (encounter != null && encounter.sceneName == "MazeScene")
            {
                return true;
            }
        }
        
        // Check if maze run is active
        if (Dexiled.MazeSystem.MazeRunManager.Instance != null && 
            Dexiled.MazeSystem.MazeRunManager.Instance.HasActiveRun())
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if current combat is a maze combat (has maze context).
    /// </summary>
    private bool IsMazeCombat()
    {
        string mazeContext = PlayerPrefs.GetString("MazeCombatContext", "");
        return !string.IsNullOrEmpty(mazeContext);
    }
    
    /// <summary>
    /// Gets enemies for maze combat using maze-specific enemy pools.
    /// Falls back to regular EnemyDatabase if maze pools not available.
    /// </summary>
    private List<EnemyData> GetMazeEnemyEncounter(int enemyCount)
    {
        List<EnemyData> encounter = new List<EnemyData>();
        
        if (Dexiled.MazeSystem.MazeRunManager.Instance == null)
        {
            Debug.LogWarning("[CombatDisplayManager] MazeRunManager not found! Using regular enemy pools.");
            return enemyDatabase != null ? enemyDatabase.GetRandomEncounter(enemyCount, EnemyTier.Normal) : encounter;
        }
        
        var run = Dexiled.MazeSystem.MazeRunManager.Instance.GetCurrentRun();
        if (run == null)
        {
            Debug.LogWarning("[CombatDisplayManager] No active maze run! Using regular enemy pools.");
            return enemyDatabase != null ? enemyDatabase.GetRandomEncounter(enemyCount, EnemyTier.Normal) : encounter;
        }
        
        // Get maze config for enemy pools
        var mazeConfig = Dexiled.MazeSystem.MazeRunManager.Instance.mazeConfig;
        if (mazeConfig == null)
        {
            Debug.LogWarning("[CombatDisplayManager] No maze config! Using regular enemy pools.");
            return enemyDatabase != null ? enemyDatabase.GetRandomEncounter(enemyCount, EnemyTier.Normal) : encounter;
        }
        
        // Get maze-specific enemy pool (all maze enemies)
        var mazePool = mazeConfig.GetMazeEnemyPool();
        
        if (mazePool != null && mazePool.Count > 0)
        {
            // Use maze-specific enemy pool with rarity-based spawning
            int tier = mazeConfig.GetTierForFloor(run.currentFloor);
            Debug.Log($"[CombatDisplayManager] Using maze-specific enemy pool (Tier {tier}, {mazePool.Count} enemies available, rarity-based)");
            
            for (int i = 0; i < enemyCount; i++)
            {
                if (mazePool.Count > 0)
                {
                    // Select random enemy from pool (rarity will be rolled during spawning)
                    EnemyData randomEnemy = mazePool[UnityEngine.Random.Range(0, mazePool.Count)];
                    encounter.Add(randomEnemy);
                    Debug.Log($"[CombatDisplayManager] Selected {randomEnemy.enemyName} for maze combat (rarity will be rolled on spawn)");
                }
            }
        }
        else
        {
            // Fallback to regular EnemyDatabase with tier based on floor
            EnemyTier floorTier = mazeConfig.GetEnemyTierForFloor(run.currentFloor);
            Debug.Log($"[CombatDisplayManager] No maze-specific pool for floor {run.currentFloor}. Using EnemyDatabase with tier {floorTier}.");
            encounter = enemyDatabase != null ? enemyDatabase.GetRandomEncounter(enemyCount, floorTier) : encounter;
        }
        
        return encounter;
    }
    
	private void CreateTestEnemies()
    {
		SpawnWaveInternal(testEnemyCount);
    }

	private void StartFirstWave()
	{
		currentWave = 1;
		isWaveTransitioning = false; // Reset transition flag
		OnWaveChanged?.Invoke(currentWave, totalWaves);
		// Ensure clean state
		if (enemySpawner != null)
		{
			enemySpawner.DespawnAllEnemies();
		}
		activeEnemies.Clear();
		defeatedEnemiesData.Clear(); // Clear defeated enemies for new combat
		defeatedEnemies.Clear(); // Clear defeated enemy instances
		// Randomize enemy count between 1 and max enemies per wave, or use exact count
		int spawnCount = randomizeEnemyCount ? UnityEngine.Random.Range(1, enemiesPerWave + 1) : enemiesPerWave;
		Debug.Log($"[EncounterDebug] Wave {currentWave}: Spawning {spawnCount} enemies (max: {enemiesPerWave}, randomized: {randomizeEnemyCount})");
		SpawnWaveInternal(spawnCount);
	}

	private IEnumerator DelayedWaveTransition()
	{
		isWaveTransitioning = true; // Mark that transition is starting
		Debug.Log($"[Wave Transition] All enemies defeated! Waiting {waveTransitionDelay} seconds before next wave...");
		
		// Wait for animations and cleanup to complete
		yield return new WaitForSeconds(waveTransitionDelay);
		
		// Start the next wave (as coroutine)
		StartCoroutine(StartNextWaveCoroutine());
	}

	private IEnumerator StartNextWaveCoroutine()
	{
		currentWave = Mathf.Clamp(currentWave + 1, 1, totalWaves);
		OnWaveChanged?.Invoke(currentWave, totalWaves);
		
		Debug.Log($"[Wave Transition] ========== Starting Wave {currentWave}/{totalWaves} ==========");
		
		// Reset transition flag after wave starts
		isWaveTransitioning = false;
		
		// Clear all enemy displays and reset indices
		if (enemySpawner != null)
		{
			enemySpawner.DespawnAllEnemies();
		}
		activeEnemies.Clear();
		
		// Wait for cleanup to fully process
		yield return new WaitForSeconds(0.5f);
		Debug.Log($"[Wave Transition] Cleanup complete, spawning enemies...");
		
		// Draw cards for new wave (based on character stat)
		if (characterManager != null && characterManager.HasCharacter())
		{
			Character character = characterManager.GetCurrentCharacter();
			int cardsToDrawForWave = character.GetCardsDrawnPerWave();
			
			if (cardsToDrawForWave > 0)
			{
				Debug.Log($"<color=yellow>[Wave Transition] Drawing {cardsToDrawForWave} cards for new wave...</color>");
				
				CombatDeckManager deckManager = CombatDeckManager.Instance;
				if (deckManager != null)
				{
					deckManager.DrawCards(cardsToDrawForWave);
				}
				else
				{
					Debug.LogWarning("[Wave Transition] CombatDeckManager not found! Cannot draw cards for wave.");
				}
			}
		}
		
		// Wait a moment for card draw animation
		yield return new WaitForSeconds(0.3f);
		
		// Randomize enemy count between 1 and max enemies per wave, or use exact count
		int spawnCount = randomizeEnemyCount ? UnityEngine.Random.Range(1, enemiesPerWave + 1) : enemiesPerWave;
		Debug.Log($"[EncounterDebug] Wave {currentWave}: Spawning {spawnCount} enemies (max: {enemiesPerWave}, randomized: {randomizeEnemyCount})");
		
		// Spawn new enemies (indices reset to 0-N)
		SpawnWaveInternal(spawnCount);
		
		// Wait for spawn to complete and UI to update
		yield return new WaitForSeconds(0.3f);
		Debug.Log($"[Wave Transition] Enemy spawn complete, starting player turn...");
		
		// Begin next player turn automatically
		StartPlayerTurn();
	}

	private void SpawnWaveInternal(int desiredCount)
	{
		// Validate spawner is available
		if (enemySpawner == null)
		{
			Debug.LogError("[SpawnWave] EnemySpawner is null! Cannot spawn enemies.");
			return;
		}
		
		// Cancel any running collect coroutines from previous waves
		if (collectEnemiesCoroutine != null)
		{
			StopCoroutine(collectEnemiesCoroutine);
			collectEnemiesCoroutine = null;
		}
		
		// Despawn all enemies from previous wave and ensure clean state
		enemySpawner.DespawnAllEnemies();
		activeEnemies.Clear();
		
		// Try to use Enemy Database if available
		if (enemyDatabase == null)
		{
			enemyDatabase = EnemyDatabase.Instance;
		}
		// Ensure database is populated at runtime if empty
		if (enemyDatabase != null && enemyDatabase.allEnemies.Count == 0)
		{
			enemyDatabase.ReloadDatabase();
			Debug.Log($"CombatDisplayManager: EnemyDatabase loaded {enemyDatabase.allEnemies.Count} enemies.");
		}
		
		// Get max spawn points from spawner
		int maxSpawns = enemySpawner.GetMaxSpawnPoints();
		int spawnCount = Mathf.Clamp(desiredCount, 1, Mathf.Min(maxEnemies, maxSpawns));
		
		Debug.Log($"[SpawnWave] Spawning {spawnCount} enemies (requested: {desiredCount}, max spawns: {maxSpawns})");
		
		if (enemyDatabase != null && enemyDatabase.allEnemies.Count > 0)
		{
		List<EnemyData> encounterData;
		EnemyData bossData = null;
		int bossSpawnIndex = -1;
		// If this is the final wave and the encounter has a unique boss, force it here
		var currentEncounter = EncounterManager.Instance != null ? EncounterManager.Instance.GetCurrentEncounter() : null;
		bool isFinalWave = currentWave >= totalWaves;
            
        if (isFinalWave && currentEncounter != null && currentEncounter.uniqueEnemy != null)
        {
            encounterData = new List<EnemyData>();
            bossData = currentEncounter.uniqueEnemy;
            if (enemySpawner != null)
            {
                bossSpawnIndex = enemySpawner.GetBossSpawnIndex();
            }
            
            // If boss has minions, add them up to spawnCount-1
            if (currentEncounter.uniqueEnemy.summonPool != null && currentEncounter.uniqueEnemy.summonPool.Count > 0)
            {
                int maxMinionSlots = Mathf.Max(0, spawnCount - 1);
                for (int s = 0; s < maxMinionSlots; s++)
                {
                    var pick = currentEncounter.uniqueEnemy.summonPool[UnityEngine.Random.Range(0, currentEncounter.uniqueEnemy.summonPool.Count)];
                    encounterData.Add(pick);
                }
            }
            
            // Fill remaining slots with enemies from encounter pool (respects exclusive pool setting)
            while (encounterData.Count < spawnCount - 1)
            {
                EnemyData fillEnemy = null;
                
                // Check if encounter has a specific enemy pool
                if (currentEncounter.encounterEnemyPool != null && currentEncounter.encounterEnemyPool.Count > 0)
                {
                    // Use encounter-specific pool
                    fillEnemy = currentEncounter.encounterEnemyPool[UnityEngine.Random.Range(0, currentEncounter.encounterEnemyPool.Count)];
                    Debug.Log($"[Boss Wave] Filling minion slot from encounter pool: {fillEnemy.enemyName}");
                }
                else if (enemyDatabase != null)
                {
                    // Fallback to database if no encounter pool
                    fillEnemy = enemyDatabase.GetRandomEnemy();
                }
                
                if (fillEnemy != null)
                    encounterData.Add(fillEnemy);
                else
                    break;
            }
        }
        else
        {
            // Check if this is maze combat - use maze-specific enemy pools if available
            bool isMazeCombat = IsMazeCombat();
            if (isMazeCombat)
            {
                encounterData = GetMazeEnemyEncounter(spawnCount);
            }
            else
            {
                // Regular encounter - check for encounter-specific enemy pool first
                var currentEnc = EncounterManager.Instance != null ? EncounterManager.Instance.GetCurrentEncounter() : null;
                
                Debug.Log($"[SpawnWave] Current encounter: {(currentEnc != null ? currentEnc.encounterName : "NULL")}");
                if (currentEnc != null)
                {
                    Debug.Log($"[SpawnWave] Encounter pool count: {(currentEnc.encounterEnemyPool != null ? currentEnc.encounterEnemyPool.Count : 0)}, Exclusive: {currentEnc.useExclusiveEnemyPool}");
                }
                
                if (currentEnc != null && currentEnc.encounterEnemyPool != null && currentEnc.encounterEnemyPool.Count > 0)
                {
                    // Use encounter-specific enemy pool
                    List<EnemyData> pool = new List<EnemyData>(currentEnc.encounterEnemyPool);
                    
                    if (!currentEnc.useExclusiveEnemyPool && enemyDatabase != null)
                    {
                        // Combine with EnemyDatabase (filtered by excludeFromRandom)
                        EnemyTier maxTier = currentEnc.areaLevel > 4 ? EnemyTier.Elite : EnemyTier.Normal;
                        var dbEnemies = enemyDatabase.GetFilteredEnemies(maxTier);
                        if (dbEnemies != null && dbEnemies.Count > 0)
                        {
                            pool.AddRange(dbEnemies);
                            Debug.Log($"[Encounter] Combined encounter pool ({currentEnc.encounterEnemyPool.Count}) with EnemyDatabase ({dbEnemies.Count} filtered enemies)");
                        }
                    }
                    else if (currentEnc.useExclusiveEnemyPool)
                    {
                        Debug.Log($"[Encounter] Using exclusive encounter enemy pool: {pool.Count} enemies");
                    }
                    
                    // Select random enemies from the pool
                    encounterData = new List<EnemyData>();
                    for (int i = 0; i < spawnCount && pool.Count > 0; i++)
                    {
                        int randomIndex = UnityEngine.Random.Range(0, pool.Count);
                        encounterData.Add(pool[randomIndex]);
                        // Optionally remove to avoid duplicates, or keep for multiple spawns
                        // pool.RemoveAt(randomIndex);
                    }
                    
                    Debug.Log($"[Encounter] Selected {encounterData.Count} enemies from pool of {pool.Count} available");
                }
                else
                {
                    // Fallback to EnemyDatabase (already filters excludeFromRandom)
                    EnemyTier maxTier = currentEnc != null && currentEnc.areaLevel > 4 ? EnemyTier.Elite : EnemyTier.Normal;
                    encounterData = enemyDatabase.GetRandomEncounter(spawnCount, maxTier);
                }
            }
        }
            
            if (encounterData == null || encounterData.Count == 0)
            {
                encounterData = new List<EnemyData>();
                for (int i = 0; i < spawnCount; i++)
                {
                    var any = enemyDatabase.GetRandomEnemy();
                    if (any != null) encounterData.Add(any);
                }
                Debug.LogWarning("[EncounterDebug] EnemyDatabase returned no encounter by tier. Falling back to any random enemies.");
            }

            // Spawn boss first at the dedicated slot
            if (bossData != null && enemySpawner != null)
            {
                if (bossSpawnIndex < 0 || bossSpawnIndex >= enemySpawner.GetMaxSpawnPoints())
                {
                    bossSpawnIndex = Mathf.Clamp(bossSpawnIndex, 0, enemySpawner.GetMaxSpawnPoints() - 1);
                }
                enemySpawner.SpawnEnemyWithAnimationAtIndex(bossData, bossSpawnIndex);
            }
            
            // Spawn remaining enemies, skipping the boss slot if needed
            if (enemySpawner != null)
            {
                if (bossData != null)
                {
                    List<EnemyData> minions = new List<EnemyData>(encounterData);
                    if (minions.Count > 0)
                    {
                        int minionSpawns = enemySpawner.GetMaxSpawnPoints();
                        Debug.Log($"[EncounterDebug] Spawning {minions.Count} minions alongside boss.");
                        enemySpawner.SpawnEnemiesWithAnimation(minions);
                    }
                }
                else if (encounterData.Count > 0)
                {
                    enemySpawner.SpawnEnemiesWithAnimation(encounterData);
                }
            }
            
            // Collect spawned enemies when animations finish
            collectEnemiesCoroutine = StartCoroutine(CollectSpawnedEnemies(encounterData));
            
            Debug.Log($"[EncounterDebug] <color=green>✓ Wave {currentWave}/{totalWaves}: Started spawning {spawnCount} enemies (collecting via coroutine...)</color>");
		}
		else
		{
			// Fallback: Create hardcoded test enemies if no database
			List<string> testEnemyNames = new List<string> { "Goblin Scout", "Orc Warrior", "Dark Mage" };
			
			for (int i = 0; i < Mathf.Min(spawnCount, testEnemyNames.Count); i++)
			{
				// Create a basic EnemyData for testing
				EnemyData testData = ScriptableObject.CreateInstance<EnemyData>();
				testData.enemyName = testEnemyNames[i];
				testData.minHealth = 30 + (i * 15);
				testData.maxHealth = 30 + (i * 15);
				testData.baseDamage = 6 + (i * 2);
				
				EnemyCombatDisplay display = enemySpawner.SpawnEnemy(testData, i);
				if (display != null)
				{
					Enemy enemy = display.GetEnemy();
					if (enemy != null)
					{
						activeEnemies.Add(enemy);
					}
				}
			}
			
			Debug.Log($"[EncounterDebug] Wave {currentWave}/{totalWaves}: Spawned {activeEnemies.Count} fallback enemies");
		}
	}
    
    public void StartCombat()
    {
        currentState = CombatState.Setup;
        currentTurn = 1;
        isPlayerTurn = true;
        
        // Set initial enemy intents
        foreach (Enemy enemy in activeEnemies)
        {
            enemy.SetIntent();
        }
        
        // Update all displays
        RefreshAllDisplays();
        
        // Start player turn
        StartPlayerTurn();
        
        Debug.Log("Combat started!");
    }
    
    private void StartPlayerTurn()
    {
        currentState = CombatState.PlayerTurn;
        isPlayerTurn = true;
        
        // Reset turn-based counters for boss abilities
        CombatDeckManager combatDeckMgr = CombatDeckManager.Instance;
        if (combatDeckMgr != null)
        {
            combatDeckMgr.ResetTurnCounters();
        }
        
        // Check if player is frozen or stunned - if so, skip their turn
        bool isPlayerFrozen = false;
        bool isPlayerStunned = false;
        if (playerDisplay != null)
        {
            var statusManager = playerDisplay.GetStatusEffectManager();
            if (statusManager != null)
            {
                isPlayerFrozen = statusManager.HasStatusEffect(StatusEffectType.Freeze);
                isPlayerStunned = statusManager.HasStatusEffect(StatusEffectType.Stun);
            }
        }
        
        if (isPlayerFrozen)
        {
            Debug.Log($"<color=cyan>[Freeze] Player is frozen and cannot act this turn!</color>");
            // Show a message to the player
            if (combatUI != null)
            {
                combatUI.LogMessage("You are FROZEN! Your turn is skipped.");
            }
            // Auto-end turn after a brief delay
            StartCoroutine(AutoEndFrozenTurn());
            return;
        }
        
        if (isPlayerStunned)
        {
            Debug.Log($"<color=yellow>[Stun] Player is stunned and cannot act this turn!</color>");
            // Show a message to the player
            if (combatUI != null)
            {
                combatUI.LogMessage("You are STUNNED! Your turn is skipped.");
            }
            // Auto-end turn after a brief delay
            StartCoroutine(AutoEndFrozenTurn());
            return;
        }
        
        // Update prepared cards (increment charges and apply bonuses)
        // This happens at the START of the player's turn so cards gain power
        var prepManager = PreparationManager.Instance;
        if (prepManager != null)
        {
            prepManager.OnTurnEnd(); // This increments charges for all prepared cards
        }
        
        // Decay player stagger and momentum
        if (characterManager != null && characterManager.HasCharacter())
        {
            Character player = characterManager.GetCurrentCharacter();
            if (player != null)
            {
                player.DecayStagger();
                // Momentum decay is now handled by StackSystem if configured
                // Note: StackSystem doesn't have built-in decay, so momentum persists across turns unless explicitly cleared
                // If decay is needed, it should be implemented via card effects or status effects that remove momentum stacks
                
                // Update player stagger display
                if (playerDisplay != null)
                {
                    playerDisplay.UpdateStaggerDisplay();
                }
            }
        }
        
        // Decay enemy stagger
        var activeDisplays = enemySpawner != null ? enemySpawner.GetActiveEnemies() : new List<EnemyCombatDisplay>();
        foreach (var display in activeDisplays)
        {
            if (display != null)
            {
                Enemy enemy = display.GetEnemy();
                if (enemy != null && enemy.IsAlive())
                {
                    enemy.DecayStagger();
                    display.UpdateStaggerDisplay();
                }
            }
        }
        
        CleanupDefeatedEnemies();
        ForceCleanupDeadEnemies();
        
        AdvanceAllStatusEffects();
        
        // Advance temporary stat boosts from card effects
        if (CardEffectProcessor.Instance != null)
        {
            CardEffectProcessor.Instance.AdvanceTurn();
        }
        
        // Restore player resources (guard + mana)
        if (characterManager != null && characterManager.HasCharacter())
        {
            Character character = characterManager.GetCurrentCharacter();
            if (character != null)
            {
                if (character.currentGuard > 0f)
                {
                    float retention = character.GetGuardPersistenceMultiplier();
                    float decayedGuard = character.currentGuard * retention;
                    if (!Mathf.Approximately(decayedGuard, character.currentGuard))
                    {
                        Debug.Log($"[Guard] Decaying guard from {character.currentGuard:F2} to {decayedGuard:F2} (retention {retention:P0}).");
                        character.currentGuard = Mathf.Max(0f, decayedGuard);
                    }
                }

                int manaRecovery = Mathf.Max(0, character.GetManaRecoveryPerTurn());
                if (manaRecovery > 0)
                {
                    character.RestoreMana(manaRecovery);
                }

                if (playerDisplay != null)
                {
                    playerDisplay.UpdateGuardDisplay();
                    playerDisplay.UpdateManaDisplay();
                }

                Debug.Log($"Player resources refreshed. Guard: {character.currentGuard}/{character.maxHealth}, Mana: {character.mana}/{character.maxMana} (+{manaRecovery}).");
            }
        }
        
        // Process delayed player cards that are ready to execute
        ProcessDelayedPlayerCards();
        
        CombatDeckManager deckMgr = CombatDeckManager.Instance;
        Character currentCharacter = characterManager != null && characterManager.HasCharacter() ? characterManager.GetCurrentCharacter() : null;
        if (deckMgr != null && currentCharacter != null)
        {
            int cardsToDraw = Mathf.Max(0, currentCharacter.GetCardsDrawnPerTurn());
            if (cardsToDraw > 0)
            {
                deckMgr.DrawCards(cardsToDraw);
            }
        }
        
        RefreshAllDisplays();
        
        OnCombatStateChanged?.Invoke(currentState);
        OnTurnChanged?.Invoke(currentTurn);
        OnTurnTypeChanged?.Invoke(isPlayerTurn);
        
        Debug.Log($"Player turn {currentTurn} started");
    }
    
    private void StartEnemyTurn()
    {
        currentState = CombatState.EnemyTurn;
        isPlayerTurn = false;
        
        AdvanceAllStatusEffects();
        
        // Regenerate enemy energy and decay guard/stagger at start of enemy turn
        var activeDisplays = enemySpawner != null ? enemySpawner.GetActiveEnemies() : new List<EnemyCombatDisplay>();
        foreach (var display in activeDisplays)
        {
            if (display != null)
            {
                Enemy enemy = display.GetEnemy();
                if (enemy != null && enemy.IsAlive())
                {
                    // Regenerate energy
                    if (enemy.usesEnergy)
                    {
                        enemy.RegenerateEnergy(enemy.energyRegenPerTurn);
                    }
                    
                    // Decay guard
                    enemy.DecayGuard();
                    // Update guard display after decay
                    if (display != null)
                    {
                        display.UpdateGuardDisplay();
                    }
                    
                    // Decay stagger
                    enemy.DecayStagger();
                    
                    // Update display
                    display.UpdateStaggerDisplay();
                    
                    // Process modifier turn-start effects (e.g., Grant Tolerance stacks)
                    ModifierEffectHandler.ProcessOnTurnStartEffects(enemy, display);
                }
            }
        }
        
        RefreshAllDisplays();
        
        NotifyEnemyAbilityRunners(runner => runner.OnTurnStart());
        
        OnCombatStateChanged?.Invoke(currentState);
        OnTurnTypeChanged?.Invoke(isPlayerTurn);
        
        Debug.Log($"Enemy turn {currentTurn} started");
        
        StartCoroutine(ExecuteEnemyActions());
    }
    
    private IEnumerator ExecuteEnemyActions()
    {
        // First, process any delayed actions that are ready
        ProcessDelayedActions();
        
        // Create a copy of active enemies to avoid modification during iteration
        List<Enemy> enemiesToAct = new List<Enemy>(activeEnemies);
        
        Debug.Log($"[Enemy Turn] {enemiesToAct.Count} enemies will take actions");
        
        // Execute each enemy's action
        for (int i = 0; i < enemiesToAct.Count; i++)
        {
            Enemy enemy = enemiesToAct[i];
            
            // Skip if enemy is no longer alive or was removed from combat
            if (enemy == null || enemy.currentHealth <= 0 || !activeEnemies.Contains(enemy))
            {
                Debug.Log($"[Enemy Turn] Skipping enemy at index {i} (defeated or removed)");
                continue;
            }
            
            // Find the current index of this enemy in active enemies
            int currentIndex = activeEnemies.IndexOf(enemy);
            if (currentIndex >= 0)
            {
                yield return ExecuteEnemyAction(enemy, currentIndex);
                yield return new WaitForSeconds(turnDelay);
            }
        }
        
        Debug.Log($"[Enemy Turn] All enemy actions complete. Active enemies remaining: {activeEnemies.Count}");
        
        NotifyEnemyAbilityRunners(runner => runner.OnTurnEnd());
        EndEnemyTurn();
    }
    
    private IEnumerator ExecuteEnemyAction(Enemy enemy, int enemyIndex)
    {
        Debug.Log($"{enemy.enemyName} is taking action...");
        
        var activeDisplays = enemySpawner != null ? enemySpawner.GetActiveEnemies() : new List<EnemyCombatDisplay>();
        EnemyCombatDisplay enemyDisplay = (enemyIndex >= 0 && enemyIndex < activeDisplays.Count) ? activeDisplays[enemyIndex] : null;
        
        // Boss Ability Handler - enemy turn start
        BossAbilityHandler.OnEnemyTurnStart(enemy, enemyDisplay);
        
        // Check if enemy is staggered (has Stun status effect) or frozen
        bool isStaggered = false;
        bool isFrozen = false;
        if (enemyDisplay != null)
        {
            var statusManager = enemyDisplay.GetStatusEffectManager();
            if (statusManager != null)
            {
                isStaggered = statusManager.HasStatusEffect(StatusEffectType.Stun);
                isFrozen = statusManager.HasStatusEffect(StatusEffectType.Freeze);
            }
        }
        
        if (isStaggered)
        {
            Debug.Log($"[Stagger] {enemy.enemyName} is staggered and cannot act this turn!");
            // Still update intent for next turn
            enemy.SetIntent();
            if (enemyDisplay != null)
            {
                enemyDisplay.UpdateIntent();
            }
            yield break; // Skip this enemy's action
        }
        
        if (isFrozen)
        {
            Debug.Log($"[Freeze] {enemy.enemyName} is frozen and cannot act this turn!");
            // Still update intent for next turn
            enemy.SetIntent();
            if (enemyDisplay != null)
            {
                enemyDisplay.UpdateIntent();
            }
            yield break; // Skip this enemy's action
        }
        
        // Check if this enemy should delay their action (Time-Lagged modifier)
        int delayTurns = 0;
        bool shouldDelay = ModifierEffectHandler.ShouldDelayAction(enemy, out delayTurns);
        
        if (shouldDelay && delayTurns > 0)
        {
            // Queue the action for later
            DelayedAction delayedAction = new DelayedAction(enemy.currentIntent, enemy.intentDamage, delayTurns, enemy);
            enemy.delayedActions.Add(delayedAction);
            
            // Apply damage bonus from delayed actions
            float damageBonus = ModifierEffectHandler.GetDelayedActionDamageBonus(enemy);
            if (damageBonus > 0f)
            {
                Debug.Log($"[Time-Lagged] {enemy.enemyName} has {enemy.delayedActions.Count} delayed action(s), gaining {damageBonus:F1}% damage bonus!");
            }
            
            // Set new intent for next turn (they'll declare another action)
            enemy.SetIntent();
            if (enemyDisplay != null)
            {
                enemyDisplay.UpdateIntent();
            }
            
            Debug.Log($"[Time-Lagged] {enemy.enemyName} queued {enemy.currentIntent} action for {delayTurns} turn(s) later");
            yield break; // Don't execute this turn
        }
        
        var abilityRunner = enemyDisplay != null ? enemyDisplay.GetAbilityRunner() : null;

        if (abilityRunner != null && abilityRunner.HasQueuedAbility)
        {
            EnemyAbility executed = abilityRunner.ExecuteQueuedAbility();
            if (executed != null && executed.consumesTurn)
            {
                int preview = abilityRunner != null ? abilityRunner.LastPreviewDamage : 0;
                enemyDisplay?.ShowAbilityIntent(executed.displayName, preview > 0 ? preview : (int?)null);
                yield return new WaitForSeconds(turnDelay);
                enemy.SetIntent();
                enemyDisplay?.UpdateIntent();
                yield break;
            }
        }

        switch (enemy.currentIntent)
        {
            case EnemyIntent.Attack:
                // Check energy cost for attack (5 energy)
                const float attackEnergyCost = 5f;
                if (enemy.usesEnergy && enemy.currentEnergy < attackEnergyCost)
                {
                    Debug.Log($"{enemy.enemyName} doesn't have enough energy to attack! ({enemy.currentEnergy:F1}/{attackEnergyCost} required)");
                    break; // Skip attack if not enough energy
                }
                
                // Consume energy for attack
                if (enemy.usesEnergy)
                {
                    bool energyDrained = enemy.DrainEnergy(attackEnergyCost, "Attack");
                    if (!energyDrained)
                    {
                        Debug.LogWarning($"{enemy.enemyName} failed to drain energy for attack!");
                    }
                }
                
                // Play attack animation
                if (enemyDisplay != null)
                {
                    enemyDisplay.PlayAttackAnimation();
                    yield return new WaitForSeconds(0.3f); // Wait for animation to start
                }
                
                abilityRunner?.OnAttack();
                
                // Attack the player
                if (characterManager != null && characterManager.HasCharacter())
                {
                    int damage = enemy.GetAttackDamage();
                    Character player = characterManager.GetCurrentCharacter();
                    
                    // Process modifier on-hit effects (e.g., Shock on hit)
                    ModifierEffectHandler.ProcessOnHitEffects(enemy, enemyDisplay, player, playerDisplay);
                    
                    // Apply stagger to player (if enemies can apply stagger)
                    // Base stagger: 10% of damage dealt
                    float staggerAmount = damage * 0.1f;
                    float staggerEffectiveness = 1f;
                    
                    // Check if player has guard - reduce stagger effectiveness by 50% if guard is present
                    if (player != null && player.currentGuard > 0f)
                    {
                        staggerEffectiveness *= 0.5f;
                        Debug.Log($"[Stagger] Player has guard ({player.currentGuard:F1}), stagger effectiveness reduced to 50%");
                    }
                    
                    // Apply stagger to player
                    if (staggerAmount > 0f && player != null && player.staggerThreshold > 0f)
                    {
                        bool staggerThresholdReached = player.AddStagger(staggerAmount, staggerEffectiveness);
                        
                        if (staggerThresholdReached)
                        {
                            // Apply Stun status effect to player when stagger threshold is reached
                            ApplyStaggerStunToPlayer(player);
                            player.ResetStagger();
                        }
                        
                        // Update player stagger display
                        if (playerDisplay != null)
                        {
                            playerDisplay.UpdateStaggerDisplay();
                        }
                    }
                    
                    characterManager.TakeDamage(damage);
                    Debug.Log($"{enemy.enemyName} attacks for {damage} damage! (Energy: {enemy.currentEnergy:F1}/{enemy.maxEnergy:F1})");
                    
                    // Show floating damage on player
                    if (floatingDamageManager != null && playerDisplay != null)
                    {
                        floatingDamageManager.ShowDamage(damage, false, playerDisplay.transform);
                    }
                }
                break;
                
            case EnemyIntent.Defend:
                // Check energy cost for defend (15 energy)
                const float defendEnergyCost = 15f;
                if (enemy.usesEnergy && enemy.currentEnergy < defendEnergyCost)
                {
                    Debug.Log($"{enemy.enemyName} doesn't have enough energy to defend! ({enemy.currentEnergy:F1}/{defendEnergyCost} required)");
                    break; // Skip defend if not enough energy
                }
                
                // Consume energy for defend
                if (enemy.usesEnergy)
                {
                    bool energyDrained = enemy.DrainEnergy(defendEnergyCost, "Defend");
                    if (!energyDrained)
                    {
                        Debug.LogWarning($"{enemy.enemyName} failed to drain energy for defend!");
                    }
                }
                
                // Enemy gains guard when defending
                // Guard amount: configurable percentage of max health (default 10%)
                float guardAmount = enemy.maxHealth * enemy.defendGuardPercent;
                enemy.AddGuard(guardAmount);
                Debug.Log($"{enemy.enemyName} is defending and gains {guardAmount:F1} guard ({enemy.defendGuardPercent * 100f:F0}% of max health)! (Total guard: {enemy.currentGuard:F1}/{enemy.maxGuard:F1}, Energy: {enemy.currentEnergy:F1}/{enemy.maxEnergy:F1})");
                
                // Update enemy display to show guard if available
                if (enemyDisplay != null)
                {
                    enemyDisplay.UpdateIntent();
                    enemyDisplay.UpdateGuardDisplay(); // Update guard display when enemy defends
                }
                break;
        }
        
        // Set new intent for next turn
        enemy.SetIntent();
        
        // Update enemy display
        if (enemyDisplay != null)
        {
            enemyDisplay.UpdateIntent();
        }
        
        // Boss Ability Handler - enemy turn end
        BossAbilityHandler.OnEnemyTurnEnd(enemy, enemyDisplay);
        
        yield return null;
    }
    
    /// <summary>
    /// Process delayed actions that are ready to execute
    /// </summary>
    private void ProcessDelayedActions()
    {
        var activeDisplays = enemySpawner != null ? enemySpawner.GetActiveEnemies() : new List<EnemyCombatDisplay>();
        
        foreach (var display in activeDisplays)
        {
            if (display == null) continue;
            
            Enemy enemy = display.GetEnemy();
            if (enemy == null || !enemy.IsAlive() || enemy.delayedActions == null) continue;
            
            // Process delayed actions (in reverse to avoid index issues when removing)
            for (int i = enemy.delayedActions.Count - 1; i >= 0; i--)
            {
                DelayedAction delayedAction = enemy.delayedActions[i];
                if (delayedAction == null) continue;
                
                // Tick the action
                if (delayedAction.Tick())
                {
                    // Action is ready to execute - queue it for execution this turn
                    Debug.Log($"[DelayedAction] {enemy.enemyName}'s delayed {delayedAction.intent} is ready to execute!");
                    
                    // Execute immediately by temporarily setting intent
                    EnemyIntent originalIntent = enemy.currentIntent;
                    int originalDamage = enemy.intentDamage;
                    
                    enemy.currentIntent = delayedAction.intent;
                    enemy.intentDamage = delayedAction.intentDamage;
                    
                    // Execute the action (will be handled in ExecuteEnemyActions loop)
                    // We'll add it to a special queue for this turn
                    StartCoroutine(ExecuteDelayedAction(delayedAction, enemy, display));
                    
                    // Remove from queue
                    enemy.delayedActions.RemoveAt(i);
                }
            }
        }
    }
    
    /// <summary>
    /// Execute a delayed action
    /// </summary>
    private IEnumerator ExecuteDelayedAction(DelayedAction delayedAction, Enemy enemy, EnemyCombatDisplay enemyDisplay)
    {
        // Temporarily set enemy's intent to the delayed action's intent
        EnemyIntent originalIntent = enemy.currentIntent;
        int originalDamage = enemy.intentDamage;
        
        enemy.currentIntent = delayedAction.intent;
        enemy.intentDamage = delayedAction.intentDamage;
        
        // Execute the action
        int enemyIndex = GetEnemyIndex(enemy);
        yield return StartCoroutine(ExecuteEnemyAction(enemy, enemyIndex));
        
        // Restore original intent (for next turn)
        enemy.currentIntent = originalIntent;
        enemy.intentDamage = originalDamage;
    }
    
    /// <summary>
    /// Get the index of an enemy in the active displays list
    /// </summary>
    private int GetEnemyIndex(Enemy enemy)
    {
        var activeDisplays = enemySpawner != null ? enemySpawner.GetActiveEnemies() : new List<EnemyCombatDisplay>();
        for (int i = 0; i < activeDisplays.Count; i++)
        {
            if (activeDisplays[i] != null && activeDisplays[i].GetEnemy() == enemy)
            {
                return i;
            }
        }
        return -1;
    }
    
    /// <summary>
    /// Process delayed player cards that are ready to execute
    /// </summary>
    private void ProcessDelayedPlayerCards()
    {
        if (characterManager == null || !characterManager.HasCharacter()) return;
        
        Character player = characterManager.GetCurrentCharacter();
        if (player == null || player.delayedActions == null) return;
        
        CombatDeckManager deckManager = CombatDeckManager.Instance;
        if (deckManager == null) return;
        
        // Process delayed actions (in reverse to avoid index issues when removing)
        for (int i = player.delayedActions.Count - 1; i >= 0; i--)
        {
            DelayedAction delayedAction = player.delayedActions[i];
            if (delayedAction == null || delayedAction.actionType != DelayedAction.ActionType.PlayerCard) continue;
            
            // Tick the action
            if (delayedAction.Tick())
            {
                // Card is ready to execute
                Debug.Log($"[DelayedCard] {delayedAction.delayedCard.cardName} is ready to execute!");
                
                // Execute the delayed card
                ExecuteDelayedCard(delayedAction.delayedCard, delayedAction.targetEnemy, delayedAction.targetPosition, deckManager);
                
                // Remove from queue
                player.delayedActions.RemoveAt(i);
            }
        }
    }
    
    /// <summary>
    /// Execute a delayed card
    /// </summary>
    private void ExecuteDelayedCard(CardDataExtended card, Enemy targetEnemy, Vector3 targetPosition, CombatDeckManager deckManager)
    {
        if (card == null || deckManager == null) return;
        
        Character player = characterManager != null && characterManager.HasCharacter() ? 
            characterManager.GetCurrentCharacter() : null;
        if (player == null) return;
        
        Debug.Log($"[DelayedCard] Executing delayed card: {card.cardName} (mana was already spent when queued)");
        
        // NOTE: Mana was already spent when the card was queued, so we don't spend it again here
        
        // Execute the card through CardEffectProcessor
        if (cardEffectProcessor != null && targetEnemy != null)
        {
            // Convert to Card for CardEffectProcessor (temporary until CardEffectProcessor is updated)
            #pragma warning disable CS0618
            Card cardForProcessor = card.ToCard();
            #pragma warning restore CS0618
            
            // Apply card effects with delayed bonus flag
            cardEffectProcessor.ApplyCardToEnemy(cardForProcessor, targetEnemy, player, targetPosition, isDelayed: true);
            
            Debug.Log($"[DelayedCard] {card.cardName} executed on {targetEnemy.enemyName} with delayed bonuses!");
        }
        else if (cardEffectProcessor != null)
        {
            // Card might not target an enemy (guard, heal, etc.)
            // For now, we'll need to handle these cases separately
            Debug.LogWarning($"[DelayedCard] {card.cardName} has no target enemy - may need special handling for non-damage cards");
        }
        
        // Update UI (AnimatedCombatUI doesn't have UpdateCombatUI, so we skip this)
        // UI will update automatically through other systems
    }
    
    /// <summary>
    /// Coroutine to auto-end player turn when frozen/stunned
    /// </summary>
    private System.Collections.IEnumerator AutoEndFrozenTurn()
    {
        // Wait a moment so player can see the message
        yield return new WaitForSeconds(1.5f);
        
        Debug.Log("[Freeze/Stun] Auto-ending player turn...");
        EndPlayerTurn();
    }
    
    private void EndEnemyTurn()
    {
        currentTurn++;
        StartPlayerTurn();
    }
    
    public void EndPlayerTurn()
    {
        // This would be called when the player finishes their turn
        // (e.g., after playing cards or clicking "End Turn")
        Debug.Log("Player ended their turn. Starting enemy turn...");
        
        // Store cards played this turn for boss abilities (Black Dawn Crash)
        var deckManager = CombatDeckManager.Instance;
        if (deckManager != null)
        {
            int cardsPlayedThisTurn = deckManager.GetCardsPlayedThisTurn();
            
            // Store on all active enemies for next turn
            var activeDisplays = enemySpawner != null ? enemySpawner.GetActiveEnemies() : new System.Collections.Generic.List<EnemyCombatDisplay>();
            foreach (var display in activeDisplays)
            {
                Enemy enemy = display?.GetCurrentEnemy();
                if (enemy != null)
                {
                    enemy.SetBossData("cardsPlayedLastTurn", cardsPlayedThisTurn);
                }
            }
            
            Debug.Log($"[Boss Ability] Stored {cardsPlayedThisTurn} cards played for scaling abilities");
        }
        
        // Safety check: Remove any enemies that reached 0 HP but weren't removed
        CleanupDefeatedEnemies();
        
        // Additional safety: Force cleanup any enemies that should be dead
        ForceCleanupDeadEnemies();
        
        StartEnemyTurn();
    }
    
    /// <summary>
    /// Call this when a card is played to check if turn should end automatically
    /// </summary>
    public void OnCardPlayed()
    {
        // For now, we'll let the player manually end their turn
        // In the future, this could implement automatic turn ending conditions
        Debug.Log("Card played. Turn continues...");
    }
    
    /// <summary>
    /// Handle enemy death from DoT damage (called by StatusEffectManager)
    /// </summary>
    public void OnEnemyDefeatedByDoT(EnemyCombatDisplay enemyDisplay, Enemy defeatedEnemy)
    {
        if (enemyDisplay == null || defeatedEnemy == null) return;
        
        var activeDisplays = enemySpawner != null ? enemySpawner.GetActiveEnemies() : new List<EnemyCombatDisplay>();
        int enemyIndex = activeDisplays.IndexOf(enemyDisplay);
        
        if (enemyIndex < 0)
        {
            Debug.LogWarning($"[DoT Death] Could not find {defeatedEnemy.enemyName} in active displays");
            return;
        }
        
        Debug.Log($"[DoT Death] {defeatedEnemy.enemyName} defeated by damage over time!");
        
        // Award loot and experience (same as regular defeat)
        if (activeEnemies.Contains(defeatedEnemy))
        {
            EnemyData enemyData = enemyDisplay.GetEnemyData();
            if (enemyData != null)
            {
                // Track for end-of-combat loot table
                if (!defeatedEnemiesData.Contains(enemyData))
                {
                    defeatedEnemiesData.Add(enemyData);
                }
                // Track Enemy instance for rarity modifiers
                if (!defeatedEnemies.Contains(defeatedEnemy))
                {
                    defeatedEnemies.Add(defeatedEnemy);
                }
                
                // Award character experience for this kill
                AwardExperienceForKill(enemyData);
                
                // Generate immediate loot drops
                GenerateAndApplyImmediateLoot(enemyData);
            }
            
            // Remove from active enemies
            OnEnemyDefeated?.Invoke(defeatedEnemy);
            activeEnemies.Remove(defeatedEnemy);
            Debug.Log($"[DoT Death] Removed {defeatedEnemy.enemyName} from active enemies. Remaining: {activeEnemies.Count}");
            
            // Despawn callback
            System.Action despawnEnemy = () => {
                enemyDisplay?.NotifyAbilityRunnerDeath();
                DespawnEnemyAtIndex(enemyIndex);
                CheckWaveCompletion();
            };
            
            // Trigger death animation
            if (enemyDisplay != null && enemyDisplay.gameObject.activeInHierarchy)
            {
                enemyDisplay.StartDeathFadeOut(despawnEnemy);
                StartCoroutine(ForceDespawnAfterDelay(enemyIndex, enemyDisplay, 1.0f));
            }
            else
            {
                despawnEnemy();
            }
        }
    }
    
    public void PlayerAttackEnemy(int enemyIndex, float damage, Card playedCard = null)
    {
        // enemyIndex is a DISPLAY index, not an activeEnemies list index!
        // We need to get the enemy from the display, not from activeEnemies
        var activeDisplays = enemySpawner != null ? enemySpawner.GetActiveEnemies() : new List<EnemyCombatDisplay>();
        
        if (enemyIndex < 0 || enemyIndex >= activeDisplays.Count)
        {
            Debug.LogWarning($"[PlayerAttackEnemy] Invalid display index: {enemyIndex} (total displays: {activeDisplays.Count})");
            return;
        }
        
        EnemyCombatDisplay targetDisplay = activeDisplays[enemyIndex];
        Enemy targetEnemy = targetDisplay.GetEnemy();
        
        if (targetEnemy == null)
        {
            Debug.LogWarning($"[PlayerAttackEnemy] No enemy at display index {enemyIndex}");
            return;
        }
        
        Debug.Log($"<color=cyan>[PlayerAttackEnemy] Attacking display {enemyIndex}: {targetEnemy.enemyName} (HP: {targetEnemy.currentHealth}/{targetEnemy.maxHealth})</color>");
        
        // Get player stats for stagger calculation
        Character player = characterManager != null ? characterManager.GetCurrentCharacter() : null;
        float staggerAmount = 0f;
        float staggerEffectiveness = 1f;
        
        if (player != null)
        {
            // Calculate base stagger amount (can be modified by cards/weapons)
            // For now, use a simple formula: 10% of damage dealt as stagger
            staggerAmount = damage * 0.1f;
            
            // Apply stagger effectiveness from player stats
            var statsData = new CharacterStatsData(player);
            staggerEffectiveness = 1f + (statsData.staggerEffectivenessIncreased / 100f);
            
            // Apply reduced enemy stagger threshold (effectively increases stagger damage)
            // If enemy threshold is reduced by 10%, stagger is 10% more effective
            if (statsData.reducedEnemyStaggerThreshold > 0f)
            {
                float thresholdReductionMultiplier = 1f + (statsData.reducedEnemyStaggerThreshold / 100f);
                staggerEffectiveness *= thresholdReductionMultiplier;
                Debug.Log($"[Stagger] Reduced Enemy Stagger Threshold: {statsData.reducedEnemyStaggerThreshold:F1}% -> Stagger effectiveness multiplier: x{thresholdReductionMultiplier:F2}");
            }
        }
        
        // Check if enemy is already staggered (has Stun status) for damage multiplier
        bool isStaggered = false;
        if (targetDisplay != null)
        {
            var statusManager = targetDisplay.GetStatusEffectManager();
            if (statusManager != null)
            {
                isStaggered = statusManager.HasStatusEffect(StatusEffectType.Stun);
            }
        }
        
        // Apply increased damage to staggered enemies
        if (isStaggered && player != null)
        {
            var statsData = new CharacterStatsData(player);
            // Apply increased damage to staggered enemies from player stats
            float increasedDamagePercent = statsData.increasedDamageToStaggered;
            float staggeredDamageMultiplier = 1f + (increasedDamagePercent / 100f);
            damage *= staggeredDamageMultiplier;
            Debug.Log($"[Stagger] {targetEnemy.enemyName} is staggered! Applying damage multiplier: x{staggeredDamageMultiplier:F2} (+{increasedDamagePercent:F1}% increased damage to staggered)");
        }
        
        // Apply stagger to enemy
        if (staggerAmount > 0f && targetEnemy.staggerThreshold > 0f)
        {
            // Check if enemy has guard - reduce stagger effectiveness by 50% if guard is present
            bool hasGuard = targetEnemy.currentGuard > 0f;
            
            // Also check for Shield status effect as additional guard source
            if (!hasGuard && targetDisplay != null)
            {
                var statusManager = targetDisplay.GetStatusEffectManager();
                if (statusManager != null)
                {
                    hasGuard = statusManager.HasStatusEffect(StatusEffectType.Shield);
                }
            }
            
            // Apply half stagger effectiveness if target has guard
            if (hasGuard)
            {
                float guardAmount = targetEnemy.currentGuard;
                if (guardAmount <= 0f && targetDisplay != null)
                {
                    var statusManager = targetDisplay.GetStatusEffectManager();
                    if (statusManager != null)
                    {
                        guardAmount = statusManager.GetTotalMagnitude(StatusEffectType.Shield);
                    }
                }
                
                if (guardAmount > 0f)
                {
                    staggerEffectiveness *= 0.5f;
                    Debug.Log($"[Stagger] {targetEnemy.enemyName} has guard ({guardAmount:F1}), stagger effectiveness reduced to 50%");
                }
            }
            
            bool staggerThresholdReached = targetEnemy.AddStagger(staggerAmount, staggerEffectiveness);
            
            if (staggerThresholdReached)
            {
                // Apply Stun status effect when stagger threshold is reached
                ApplyStaggerStun(targetDisplay, targetEnemy);
                targetEnemy.ResetStagger(); // Reset stagger meter after applying stun
            }
            
            // Update stagger display after applying stagger
            if (targetDisplay != null)
            {
                targetDisplay.UpdateStaggerDisplay();
            }
        }
        
        // Check for Blind status effect (miss chance)
        if (playerDisplay != null)
        {
            var statusManager = playerDisplay.GetStatusEffectManager();
            if (statusManager != null && statusManager.HasStatusEffect(StatusEffectType.Blind))
            {
                float blindMagnitude = statusManager.GetTotalMagnitude(StatusEffectType.Blind);
                float missChance = blindMagnitude / 100f; // Magnitude is percentage (e.g., 30 = 30%)
                
                if (UnityEngine.Random.Range(0f, 1f) < missChance)
                {
                    Debug.Log($"<color=yellow>[Blind] Attack MISSED! ({blindMagnitude}% miss chance)</color>");
                    
                    // Show miss indicator
                    if (floatingDamageManager != null && enemyIndex < activeDisplays.Count)
                    {
                        floatingDamageManager.ShowDamage(0f, false, activeDisplays[enemyIndex].transform);
                    }
                    
                    if (combatUI != null)
                    {
                        combatUI.LogMessage($"<color=grey>Blinded!</color> Your attack missed!");
                    }
                    
                    return; // Attack misses entirely
                }
            }
        }
        
        // Calculate critical strike chance
        float critChance = 0f;
        
        if (player != null)
        {
            // 1. Add base crit chance from card type (2% for attacks, 5% for spells)
            if (playedCard != null)
            {
                critChance = CardTypeConstants.GetBaseCritChance(playedCard);
                Debug.Log($"<color=cyan>[Crit] Base crit chance for {playedCard.cardName} ({playedCard.cardType}): {critChance}%</color>");
            }
            
            // 2. Add equipment crit modifiers
            var damageModifiers = player.GetDamageModifiers();
            if (damageModifiers != null)
            {
                critChance += damageModifiers.criticalStrikeChance;
                Debug.Log($"<color=cyan>[Crit] + Equipment crit chance: {damageModifiers.criticalStrikeChance}% (Total: {critChance}%)</color>");
            }
            
            // 3. Add character crit modifiers (from ascendancy/passives)
            critChance += player.criticalStrikeChance;
            if (player.criticalStrikeChance > 0)
            {
                Debug.Log($"<color=cyan>[Crit] + Character crit chance: {player.criticalStrikeChance}% (Total: {critChance}%)</color>");
            }
            
            // 4. Add stack system bonuses
            if (StackSystem.Instance != null)
            {
                float stackCritBonus = StackSystem.Instance.GetCritChanceBonus();
                critChance += stackCritBonus;
                if (stackCritBonus > 0)
                {
                    Debug.Log($"<color=cyan>[Crit] + Stack crit bonus: {stackCritBonus}% (Total: {critChance}%)</color>");
                }
            }
        }
        
        // Clamp crit chance to 0-100%
        critChance = Mathf.Clamp(critChance, 0f, 100f);
        
        // Apply charge modifiers: Always Crit, or roll for crit
        bool wasCritical = CombatDeckManager.ShouldAlwaysCrit() || UnityEngine.Random.Range(0f, 100f) < critChance;
            
            if (wasCritical)
            {
                // Check for modifier effects that reduce/no extra crit damage
                float critDamageMultiplier = ModifierEffectHandler.GetCritDamageMultiplier(targetEnemy, wasCritical);
                if (critDamageMultiplier < 1f)
                {
                    // Enemy has "No Extra Crit Damage" or "Reduced Crit Damage" modifier
                    // Apply normal damage (1x) plus the reduction multiplier
                    damage = damage * critDamageMultiplier;
                    Debug.Log($"[Modifier] {targetEnemy.enemyName} has crit damage reduction! Damage multiplier: x{critDamageMultiplier:F2}");
                }
                else
                {
                    // Normal critical hit
                    damage *= 1.5f; // Critical damage multiplier
                }
            }
            
            // Apply charge modifiers: Ignore Guard/Armor (bypass Bolster reduction)
            // We'll handle this by storing a flag and checking it in Enemy.TakeDamage
            // For now, we'll apply the damage directly with a flag
            bool ignoreGuardArmor = CombatDeckManager.ShouldIgnoreGuardArmor();
            
            // REMOVED: targetEnemy.TakeDamage(damage) - was causing double damage
            // The EnemyCombatDisplay.TakeDamage() below handles applying damage to the enemy
            
            // Play damage impact effect
            if (combatEffectManager != null && enemyIndex < activeDisplays.Count)
            {
                // Use physical damage as default for direct attacks
                combatEffectManager.PlayElementalDamageEffectOnTarget(activeDisplays[enemyIndex].transform, DamageType.Physical, wasCritical);
            }
            
            // Boss Ability Handler - check for evasion (Empty Footfalls)
            bool shouldEvade = BossAbilityHandler.ShouldEvadeAttack(targetEnemy);
            if (shouldEvade)
            {
                Debug.Log($"<color=yellow>[Boss Ability] {targetEnemy.enemyName} EVADED the attack!</color>");
                
                // Show "EVADED" floating text (use 0 damage as miss indicator)
                if (floatingDamageManager != null && enemyIndex < activeDisplays.Count)
                {
                    // ShowDamage with 0 damage can represent a miss/evade
                    floatingDamageManager.ShowDamage(0f, false, activeDisplays[enemyIndex].transform);
                }
                
                // Log to combat UI
                if (combatUI != null)
                {
                    combatUI.LogMessage($"<color=yellow>{targetEnemy.enemyName} evaded the attack!</color>");
                }
                
                return; // Skip damage entirely
            }
            
            // Update enemy display and apply damage
            if (enemyIndex < activeDisplays.Count)
            {
                activeDisplays[enemyIndex].TakeDamage(damage, ignoreGuardArmor);
                activeDisplays[enemyIndex].PlayDamageAnimation();
                
                // Show floating damage number or status effect name
                if (floatingDamageManager != null)
                {
                    // If damage is 0 and card applies status effects, show status effect name instead
                    if (damage <= 0.01f && playedCard != null && playedCard.effects != null && playedCard.effects.Count > 0)
                    {
                        // Find the first ApplyStatus effect
                        string statusEffectName = null;
                        foreach (var effect in playedCard.effects)
                        {
                            if (effect != null && effect.effectType == EffectType.ApplyStatus && 
                                (effect.targetsEnemy || effect.targetsAllEnemies))
                            {
                                // Format the status effect name nicely
                                statusEffectName = FormatStatusEffectName(effect.effectName);
                                break;
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(statusEffectName))
                        {
                            // Show status effect name in a distinct color (e.g., purple/cyan for debuffs)
                            Color statusColor = new Color(0.8f, 0.6f, 1f); // Light purple
                            floatingDamageManager.ShowAbilityName(statusEffectName, activeDisplays[enemyIndex].transform, statusColor);
                        }
                        else
                        {
                            // No status effect found, show 0 damage
                            floatingDamageManager.ShowDamage(damage, wasCritical, activeDisplays[enemyIndex].transform);
                        }
                    }
                    else
                    {
                        // Normal damage display
                        floatingDamageManager.ShowDamage(damage, wasCritical, activeDisplays[enemyIndex].transform);
                    }
                }
                
                // Trigger embossing modifier event for damage dealt
                if (playedCard != null && player != null && Dexiled.CombatSystem.Embossing.EmbossingModifierEventProcessor.Instance != null)
                {
                    DamageType damageType = playedCard.primaryDamageType;
                    Dexiled.CombatSystem.Embossing.EmbossingModifierEventProcessor.Instance.OnDamageDealt(
                        playedCard, player, targetEnemy, damage, damageType
                    );
                }
            }
            
				// Check if enemy is defeated
				if (targetEnemy.currentHealth <= 0)
				{
					Debug.Log($"[Enemy Defeat] {targetEnemy.enemyName} defeated! HP: {targetEnemy.currentHealth}");
					
					// Trigger embossing modifier event for enemy killed
					if (playedCard != null && player != null && Dexiled.CombatSystem.Embossing.EmbossingModifierEventProcessor.Instance != null)
					{
						Dexiled.CombatSystem.Embossing.EmbossingModifierEventProcessor.Instance.OnEnemyKilled(
							playedCard, player, targetEnemy
						);
					}
					
					// Prevent double-defeat by immediately marking enemy as defeated
					if (activeEnemies.Contains(targetEnemy))
					{
						// Generate and apply immediate loot drops from this enemy
						var enemyDisplaysList = enemySpawner != null ? enemySpawner.GetActiveEnemies() : new List<EnemyCombatDisplay>();
						var disp = (enemyIndex < enemyDisplaysList.Count) ? enemyDisplaysList[enemyIndex] : null;
						
						if (disp != null)
						{
							EnemyData enemyData = disp.GetEnemyData();
							Enemy enemy = disp.GetEnemy();
							if (enemyData != null)
							{
								// Track for end-of-combat loot table
								if (!defeatedEnemiesData.Contains(enemyData))
								{
									defeatedEnemiesData.Add(enemyData);
								}
								// Track Enemy instance for rarity modifiers
								if (enemy != null && !defeatedEnemies.Contains(enemy))
								{
									defeatedEnemies.Add(enemy);
								}
								
								// Award character experience for this kill
								AwardExperienceForKill(enemyData);
								
								// Generate immediate loot drops
								GenerateAndApplyImmediateLoot(enemyData);
							}
						}
						
						// Remove from active enemies IMMEDIATELY to prevent stuck enemies
						if (activeEnemies.Contains(targetEnemy) == false)
						{
							Debug.LogWarning($"[Enemy Defeat] {targetEnemy.enemyName} not found in active list. Rebuilding from spawner.");
							RebuildActiveEnemiesFromSpawner();
						}

						if (activeEnemies.Contains(targetEnemy))
						{
							OnEnemyDefeated?.Invoke(targetEnemy);
							int idx = activeEnemies.IndexOf(targetEnemy);
							if (idx >= 0) 
							{
								activeEnemies.RemoveAt(idx);
								Debug.Log($"[Enemy Defeat] Removed {targetEnemy.enemyName} from active enemies. Remaining: {activeEnemies.Count}");
							}

							System.Action despawnEnemy = () => {
								disp?.NotifyAbilityRunnerDeath();
								DespawnEnemyAtIndex(enemyIndex);
								Debug.Log($"[Enemy Defeat] Despawned enemy at index {enemyIndex}");

								if (!isAoEAttackInProgress)
								{
									CheckWaveCompletion();
								}
							};

							if (disp != null && disp.gameObject.activeInHierarchy)
							{
								disp.StartDeathFadeOut(despawnEnemy);
								StartCoroutine(ForceDespawnAfterDelay(enemyIndex, disp, 1.0f));
							}
							else
							{
								Debug.LogWarning($"[Enemy Defeat] Display null or inactive for {targetEnemy.enemyName}, despawning immediately");
								despawnEnemy();
							}
						}
						else
						{
							Debug.LogError($"[Enemy Defeat] Failed to locate {targetEnemy.enemyName} in active enemies even after rebuild.");
							System.Action fallbackDespawn = () => {
								disp?.NotifyAbilityRunnerDeath();
								DespawnEnemyAtIndex(enemyIndex);
								if (!isAoEAttackInProgress)
								{
									CheckWaveCompletion();
								}
							};

							if (disp != null && disp.gameObject.activeInHierarchy)
							{
								disp.StartDeathFadeOut(fallbackDespawn);
								StartCoroutine(ForceDespawnAfterDelay(enemyIndex, disp, 1.0f));
							}
							else
							{
								fallbackDespawn();
							}
						}
					}
					else
					{
						Debug.LogWarning($"[Enemy Defeat] {targetEnemy.enemyName} already removed from active enemies (double-defeat prevented)");
					}
				}
    }
    
    /// <summary>
    /// Safety cleanup for enemies stuck at 0 HP
    /// </summary>
    private void CleanupDefeatedEnemies()
    {
        List<Enemy> enemiesToRemove = new List<Enemy>();
        
        // Find all enemies at or below 0 HP
        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy != null && enemy.currentHealth <= 0)
            {
                enemiesToRemove.Add(enemy);
                Debug.LogWarning($"[Cleanup] Found stuck defeated enemy: {enemy.enemyName} (HP: {enemy.currentHealth})");
            }
        }
        
        // Remove them
        foreach (Enemy enemy in enemiesToRemove)
        {
            Debug.Log($"[Cleanup] Force removing stuck enemy: {enemy.enemyName}");
            
            // Find and despawn the display
            if (enemySpawner != null)
            {
                var activeDisplays = enemySpawner.GetActiveEnemies();
                for (int i = 0; i < activeDisplays.Count; i++)
                {
                    if (activeDisplays[i] != null && activeDisplays[i].GetEnemy() == enemy)
                    {
                        DespawnEnemyAtIndex(i);
                        break;
                    }
                }
            }
            
            activeEnemies.Remove(enemy);
        }
        
        if (enemiesToRemove.Count > 0)
        {
            Debug.Log($"[Cleanup] Removed {enemiesToRemove.Count} stuck enemies. Remaining: {activeEnemies.Count}");
            
            // Check if wave is now complete
            CheckWaveCompletion();
        }
    }
    
    /// <summary>
    /// More aggressive cleanup - checks visual displays and forces removal of dead enemies
    /// </summary>
    private void ForceCleanupDeadEnemies()
    {
        if (enemySpawner == null) return;
        
        var activeDisplays = enemySpawner.GetActiveEnemies();
        List<int> indicesToRemove = new List<int>();
        
        // Check each display for dead enemies
        for (int i = 0; i < activeDisplays.Count; i++)
        {
            var display = activeDisplays[i];
            if (display != null && display.gameObject.activeInHierarchy)
            {
                var enemy = display.GetEnemy();
                if (enemy != null && enemy.currentHealth <= 0)
                {
                    Debug.LogWarning($"[Force Cleanup] Found dead enemy in display {i}: {enemy.enemyName} (HP: {enemy.currentHealth})");
                    indicesToRemove.Add(i);
                }
            }
        }
        
        // Remove dead enemies from both active list and despawn displays
        for (int i = indicesToRemove.Count - 1; i >= 0; i--)
        {
            int index = indicesToRemove[i];
            if (index < activeDisplays.Count)
            {
                var display = activeDisplays[index];
                if (display != null)
                {
                    var enemy = display.GetEnemy();
                    if (enemy != null)
                    {
                        // Remove from active enemies list
                        if (activeEnemies.Contains(enemy))
                        {
                            activeEnemies.Remove(enemy);
                            Debug.Log($"[Force Cleanup] Removed dead enemy from active list: {enemy.enemyName}");
                        }
                        
                        DespawnEnemyAtIndex(index);
                    }
                }
            }
        }
        
        if (indicesToRemove.Count > 0)
        {
            Debug.Log($"[Force Cleanup] Force cleaned {indicesToRemove.Count} dead enemies. Remaining: {activeEnemies.Count}");
            CheckWaveCompletion();
        }
    }
    
    /// <summary>
    /// Format status effect name for display (capitalize first letter, handle common variations)
    /// </summary>
    private string FormatStatusEffectName(string effectName)
    {
        if (string.IsNullOrEmpty(effectName))
            return "";
        
        // Handle common status effect name variations
        string lower = effectName.ToLower();
        switch (lower)
        {
            case "vulnerable":
            case "vulnerability":
                return "Vulnerability";
            case "poison":
            case "poisoned":
                return "Poison";
            case "burn":
            case "burning":
            case "ignite":
            case "ignited":
                return "Burn";
            case "chill":
            case "chilled":
                return "Chill";
            case "freeze":
            case "frozen":
                return "Freeze";
            case "stun":
            case "stunned":
                return "Stun";
            case "weak":
                return "Weak";
            case "frail":
                return "Frail";
            case "bleed":
            case "bleeding":
                return "Bleed";
            default:
                // Capitalize first letter
                if (effectName.Length > 0)
                {
                    return char.ToUpper(effectName[0]) + (effectName.Length > 1 ? effectName.Substring(1).ToLower() : "");
                }
                return effectName;
        }
    }
    
    /// <summary>
    /// Mark that an AoE attack is starting (prevents cascading wave completion checks)
    /// </summary>
    public void StartAoEAttack()
    {
        isAoEAttackInProgress = true;
        Debug.Log("[AoE] Marking AoE attack as in progress - wave completion checks deferred");
    }
    
    /// <summary>
    /// Mark that an AoE attack is complete (allows wave completion check)
    /// </summary>
    public void EndAoEAttack()
    {
        isAoEAttackInProgress = false;
        Debug.Log("[AoE] AoE attack complete - checking wave completion");
        CheckWaveCompletion();
    }
    
    /// <summary>
    /// Check if wave is complete and trigger next wave or victory
    /// </summary>
    private void CheckWaveCompletion()
    {
		// IMPORTANT: Rebuild activeEnemies list from spawner to ensure we have the most up-to-date count
		// This prevents issues where the list might be out of sync (e.g., when boss dies but other enemies remain)
		RebuildActiveEnemiesFromSpawner();
		
		// Also check spawner's active displays directly as a double-check
		int spawnerActiveCount = 0;
		if (enemySpawner != null)
		{
			var activeDisplays = enemySpawner.GetActiveEnemies();
			if (activeDisplays != null)
			{
				foreach (var display in activeDisplays)
				{
					var enemy = display?.GetEnemy();
					if (enemy != null && enemy.currentHealth > 0)
					{
						spawnerActiveCount++;
					}
				}
			}
		}
		
		// Use the higher count to be safe (in case one list is out of sync)
		int actualEnemyCount = Mathf.Max(activeEnemies.Count, spawnerActiveCount);
		
		Debug.Log($"[Wave Status] Checking completion - activeEnemies: {activeEnemies.Count}, spawner active: {spawnerActiveCount}, using: {actualEnemyCount}");
		
		if (actualEnemyCount == 0)
		{
			// Prevent multiple wave transitions when multiple enemies die simultaneously
			if (isWaveTransitioning)
			{
				Debug.Log($"[Wave Complete] Wave transition already in progress, ignoring duplicate check");
				return;
			}
			
			Debug.Log($"[Wave Complete] All enemies defeated. Current wave: {currentWave}/{totalWaves}");
			
			if (currentWave < totalWaves)
			{
				Debug.Log($"[Wave Complete] Starting wave transition to wave {currentWave + 1}");
				StartCoroutine(DelayedWaveTransition());
			}
			else
			{
				Debug.Log($"[Wave Complete] All waves complete! Victory!");
				EndCombat(true);
			}
		}
		else
		{
			Debug.Log($"[Wave Status] {actualEnemyCount} enemies remaining - combat continues");
		}
    }
    
    /// <summary>
    /// Safety coroutine to force despawn if animation fails
    /// </summary>
    private IEnumerator ForceDespawnAfterDelay(int enemyIndex, EnemyCombatDisplay display, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Check if display is still active (animation might not have completed)
        if (display != null && display.gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"[Force Despawn] Enemy at index {enemyIndex} still active after {delay}s, forcing despawn");
            display.NotifyAbilityRunnerDeath();
            DespawnEnemyAtIndex(enemyIndex);
            RebuildActiveEnemiesFromSpawner();
            if (!isAoEAttackInProgress)
            {
                CheckWaveCompletion();
            }
        }
    }
    
    public void PlayerHeal(int amount)
    {
        if (characterManager != null)
        {
            characterManager.Heal(amount);
            
            // Play heal impact effect
            if (combatEffectManager != null && playerDisplay != null)
            {
                combatEffectManager.PlayHealEffectOnTarget(playerDisplay.transform);
            }
            
            // Show floating heal number
            if (floatingDamageManager != null && playerDisplay != null)
            {
                floatingDamageManager.ShowHeal(amount, playerDisplay.transform);
            }
        }
    }
    
    public void PlayerGainGuard(float amount)
    {
        if (characterManager != null && characterManager.HasCharacter())
        {
            Character character = characterManager.GetCurrentCharacter();
            if (character != null)
            {
                // Add guard to character (automatically capped at max health)
                character.AddGuard(amount);
                
                // Update the display
                if (playerDisplay != null)
                {
                    playerDisplay.UpdateGuardDisplay();
                }
                
                // Play visual effect
                if (combatEffectManager != null && playerDisplay != null)
                {
                    combatEffectManager.PlayGuardEffectOnTarget(playerDisplay.transform);
                }
                
                Debug.Log($"Player gained {amount} guard. Current guard: {character.currentGuard}/{character.maxHealth}");
            }
        }
    }
    
    /// <summary>
    /// Advance all status effects by one turn for all entities
    /// </summary>
    private void AdvanceAllStatusEffects()
    {
        Debug.Log($"<color=cyan>[Status Effects] Starting turn advancement...</color>");
        
        try
        {
            // Advance player status effects
            if (playerDisplay != null)
            {
                StatusEffectManager playerStatusManager = playerDisplay.GetStatusEffectManager();
                if (playerStatusManager != null)
                {
                    Debug.Log($"[Status Effects] Advancing player status effects...");
                    playerStatusManager.AdvanceAllEffectsOneTurn();
                    Debug.Log($"[Status Effects] Player status effects advanced successfully");
                }
                else
                {
                    Debug.LogWarning($"[Status Effects] Player StatusEffectManager is null!");
                }
            }
            else
            {
                Debug.LogWarning($"[Status Effects] PlayerDisplay is null!");
            }
            
            // Advance enemy status effects
            EnemyCombatDisplay[] activeEnemyDisplays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
            Debug.Log($"[Status Effects] Found {activeEnemyDisplays.Length} enemy displays");
            
            foreach (var enemyDisplay in activeEnemyDisplays)
            {
                if (enemyDisplay != null && enemyDisplay.gameObject.activeInHierarchy)
                {
                    Enemy enemy = enemyDisplay.GetEnemy();
                    string enemyName = enemy != null ? enemy.enemyName : "Unknown";
                    
                    StatusEffectManager enemyStatusManager = enemyDisplay.GetStatusEffectManager();
                    if (enemyStatusManager != null)
                    {
                        Debug.Log($"[Status Effects] Advancing {enemyName}'s status effects...");
                        enemyStatusManager.AdvanceAllEffectsOneTurn();
                        Debug.Log($"[Status Effects] {enemyName}'s status effects advanced successfully");
                    }
                }
            }
            
            Debug.Log($"<color=cyan>[Status Effects] All status effects advanced by one turn</color>");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Status Effects] ERROR during status effect advancement: {ex.Message}\n{ex.StackTrace}");
            // Don't rethrow - allow turn to continue even if status effects fail
        }
    }
    
    /// <summary>
    /// Apply Stun status effect to enemy when stagger threshold is reached
    /// </summary>
    private void ApplyStaggerStun(EnemyCombatDisplay enemyDisplay, Enemy enemy)
    {
        if (enemyDisplay == null || enemy == null) return;
        
        var statusManager = enemyDisplay.GetStatusEffectManager();
        if (statusManager == null)
        {
            Debug.LogWarning($"[Stagger] Cannot apply stun to {enemy.enemyName}: StatusEffectManager not found");
            return;
        }
        
        // Get player stats for stagger duration increase
        Character player = characterManager != null ? characterManager.GetCurrentCharacter() : null;
        int stunDuration = 1; // Base duration: 1 turn
        
        if (player != null)
        {
            var statsData = new CharacterStatsData(player);
            // Apply increased stagger duration (flat addition in turns)
            if (statsData.increasedStaggerDuration > 0f)
            {
                stunDuration = Mathf.RoundToInt(1 + statsData.increasedStaggerDuration);
                Debug.Log($"[Stagger] Increased Stagger Duration: +{statsData.increasedStaggerDuration:F1} turns -> Total duration: {stunDuration} turns");
            }
        }
        
        // Create Stun status effect (duration = base 1 + increasedStaggerDuration)
        StatusEffect stunEffect = new StatusEffect(
            StatusEffectType.Stun,
            "Staggered",
            1f, // magnitude (not used for Stun)
            stunDuration,  // duration: base 1 turn + increasedStaggerDuration
            true // isDebuff
        );
        stunEffect.description = stunDuration > 1 ? $"Cannot act for {stunDuration} turns" : "Cannot act this turn";
        
        bool added = statusManager.AddStatusEffect(stunEffect);
        if (added)
        {
            Debug.Log($"[Stagger] {enemy.enemyName} is now STUNNED for {stunDuration} turn{(stunDuration > 1 ? "s" : "")}!");
        }
        else
        {
            Debug.LogWarning($"[Stagger] Failed to apply Stun to {enemy.enemyName}");
        }
    }
    
    /// <summary>
    /// Apply Stun status effect to player when stagger threshold is reached
    /// </summary>
    private void ApplyStaggerStunToPlayer(Character player)
    {
        if (player == null || playerDisplay == null) return;
        
        var statusManager = playerDisplay.GetStatusEffectManager();
        if (statusManager == null)
        {
            Debug.LogWarning($"[Stagger] Cannot apply stun to {player.characterName}: StatusEffectManager not found");
            return;
        }
        
        // Create Stun status effect (duration 1 = skip next turn)
        StatusEffect stunEffect = new StatusEffect(
            StatusEffectType.Stun,
            "Staggered",
            1f, // magnitude (not used for Stun)
            1,  // duration: 1 turn
            true // isDebuff
        );
        stunEffect.description = "Cannot act this turn";
        
        bool added = statusManager.AddStatusEffect(stunEffect);
        if (added)
        {
            Debug.Log($"[Stagger] {player.characterName} is now STUNNED for 1 turn!");
        }
        else
        {
            Debug.LogWarning($"[Stagger] Failed to apply Stun to {player.characterName}");
        }
    }
    
    private void EndCombat(bool victory)
    {
        currentState = victory ? CombatState.Victory : CombatState.Defeat;
        
        if (victory)
        {
            // Generate loot rewards
            GenerateLootRewards();
            
            // Check if this is a maze combat encounter
            bool isMazeCombat = PlayerPrefs.HasKey("MazeCombatContext") || PlayerPrefs.HasKey("MazeRunId");
            if (isMazeCombat && MazeRunManager.Instance != null)
            {
                Debug.Log("[CombatManager] Maze combat victory detected. Notifying MazeRunManager...");
                MazeRunManager.Instance.OnCombatVictory();
                // Note: MazeRunManager.OnCombatVictory() will handle scene transition, so we return early
                OnCombatStateChanged?.Invoke(currentState);
                OnCombatEnded?.Invoke();
                return;
            }
        }
        else
        {
            // Check if this is a maze combat encounter (defeat)
            bool isMazeCombat = PlayerPrefs.HasKey("MazeCombatContext") || PlayerPrefs.HasKey("MazeRunId");
            if (isMazeCombat && MazeRunManager.Instance != null)
            {
                Debug.Log("[CombatManager] Maze combat defeat detected. Notifying MazeRunManager...");
                MazeRunManager.Instance.OnCombatDefeat();
                // Note: MazeRunManager.OnCombatDefeat() will handle scene transition, so we return early
                OnCombatStateChanged?.Invoke(currentState);
                OnCombatEnded?.Invoke();
                return;
            }
        }
        
        OnCombatStateChanged?.Invoke(currentState);
        OnCombatEnded?.Invoke();
        
        Debug.Log($"Combat ended: {(victory ? "Victory!" : "Defeat!")}");
        
        // Show victory/defeat UI via CombatSceneManager
        CombatSceneManager sceneManager = FindFirstObjectByType<CombatSceneManager>();
        if (sceneManager != null)
        {
            if (victory)
            {
                sceneManager.CompleteEncounter();
            }
            else
            {
                sceneManager.FailEncounter();
            }
        }
        else
        {
            Debug.LogWarning("[CombatManager] CombatSceneManager not found! Victory/Defeat UI will not be displayed.");
        }
    }
    
    private void GenerateLootRewards()
    {
        // Get area level from current encounter
        int areaLevel = 1;
        var currentEncounter = EncounterManager.Instance != null ? EncounterManager.Instance.GetCurrentEncounter() : null;
        if (currentEncounter != null)
        {
            areaLevel = currentEncounter.areaLevel;
        }
        
        LootManager lootManager = LootManager.Instance;
        LootDropResult lootResult = null;

        if (encounterLootTable != null && lootManager == null)
        {
            Debug.LogError("[Combat Victory] LootManager not found while attempting to evaluate encounter loot table.");
        }

        // Primary path: classic loot table
        if (encounterLootTable != null && lootManager != null)
        {
            lootResult = lootManager.GenerateLoot(encounterLootTable, areaLevel, defeatedEnemiesData);
        }
        else
        {
            // Area loot table override or global area loot system fallback
            List<LootReward> areaRewards = null;
            // Increased base drops: at least 2 items per encounter, plus 1 per enemy (minimum 2, scales with enemies)
            int maxDrops = Mathf.Max(2, 2 + enemiesPerWave);

            if (encounterAreaLootTable != null)
            {
                areaRewards = encounterAreaLootTable.GenerateAllLoot(areaLevel, maxDrops);
            }
            else if (AreaLootManager.Instance != null)
            {
                areaRewards = AreaLootManager.Instance.GenerateLootRewardsForArea(areaLevel, maxDrops);
            }

            if (areaRewards != null && areaRewards.Count > 0)
            {
                lootResult = BuildLootResultFromRewards(areaRewards);
            }

            // Add encounter-wide experience and spirit-tag currency when using area loot path
            if (lootResult == null)
            {
                lootResult = new LootDropResult();
            }

            AddBaseEncounterExperience(lootResult, areaLevel);
            AddSpiritDropsFromDefeatedEnemies(lootResult);
        }

        if (lootResult == null)
        {
            lootResult = new LootDropResult();
        }

        // Apply quantity and rarity multipliers from defeated enemies
        ApplyEnemyLootMultipliers(lootResult);
        
        pendingLoot = lootResult;

        if (pendingLoot != null && pendingLoot.rewards.Count > 0)
        {
            Debug.Log($"[Combat Victory] Generated {pendingLoot.rewards.Count} rewards (from {defeatedEnemiesData.Count} defeated enemies):");
            foreach (var reward in pendingLoot.rewards)
            {
                Debug.Log($"  - {reward.GetDisplayName()}");
            }
        }
        else
        {
            Debug.LogWarning("[Combat Victory] No loot generated!");
        }
    }
    
    /// <summary>
    /// Apply quantity and rarity multipliers from defeated enemies to loot rewards
    /// </summary>
    private void ApplyEnemyLootMultipliers(LootDropResult lootResult)
    {
        if (lootResult == null || defeatedEnemies == null || defeatedEnemies.Count == 0)
            return;
        
        // Calculate average multipliers from all defeated enemies
        float totalQuantityMultiplier = 1f;
        float totalRarityMultiplier = 1f;
        int enemyCount = 0;
        
        foreach (var enemy in defeatedEnemies)
        {
            if (enemy != null)
            {
                // Multiply (stack multiplicatively across enemies)
                totalQuantityMultiplier *= enemy.quantityMultiplier;
                totalRarityMultiplier *= enemy.rarityMultiplier;
                enemyCount++;
            }
        }
        
        if (enemyCount > 0)
        {
            // Apply quantity multiplier to existing rewards
            if (totalQuantityMultiplier > 1f)
            {
                lootResult.ApplyQuantityMultiplier(totalQuantityMultiplier);
                Debug.Log($"[Loot] Applied quantity multiplier: {totalQuantityMultiplier:F2}x from {enemyCount} defeated enemy/enemies");
            }
            
            // Note: Rarity multiplier is typically applied during item generation
            // Store it for future use in item generation systems
            if (totalRarityMultiplier > 1f)
            {
                lootResult.ApplyRarityMultiplier(totalRarityMultiplier);
                Debug.Log($"[Loot] Applied rarity multiplier: {totalRarityMultiplier:F2}x from {enemyCount} defeated enemy/enemies");
            }
        }
    }
    
    public LootDropResult GetPendingLoot()
    {
        return pendingLoot;
    }
    
    public void ApplyPendingLoot()
    {
        if (pendingLoot != null)
        {
            LootManager lootManager = LootManager.Instance;
            if (lootManager != null)
            {
                lootManager.ApplyRewards(pendingLoot);
                Debug.Log("[Combat] Loot rewards applied to character");
                pendingLoot = null; // Clear pending loot
            }
        }
    }
    
    private void GenerateAndApplyImmediateLoot(EnemyData enemyData)
    {
        if (enemyData == null)
            return;
        
        // Get area level
        int areaLevel = 1;
        var currentEncounter = EncounterManager.Instance != null ? EncounterManager.Instance.GetCurrentEncounter() : null;
        if (currentEncounter != null)
        {
            areaLevel = currentEncounter.areaLevel;
        }
        
        // Generate loot for this specific enemy
        EnemyLootDropper lootDropper = EnemyLootDropper.Instance;
        if (lootDropper != null)
        {
            List<LootReward> drops = lootDropper.GenerateEnemyLoot(enemyData, areaLevel);
			
			if (drops == null)
			{
				drops = new List<LootReward>();
			}

			// Supplement with area loot drops if configured
			// Increased from 1 to 2 drops per enemy for better drop frequency
			if (encounterAreaLootTable != null)
			{
				var areaDrops = encounterAreaLootTable.GenerateAllLoot(areaLevel, 2);
				if (areaDrops != null && areaDrops.Count > 0)
				{
					drops.AddRange(areaDrops);
				}
			}
			else if (AreaLootManager.Instance != null)
			{
				AreaLootManager.Instance.AddAreaLootToEnemyDrops(enemyData, areaLevel, drops);
			}
            
            if (drops != null && drops.Count > 0)
            {
                // Apply drops immediately
                lootDropper.ApplyImmediateDrops(drops);
                
                // Display in combat log
                CombatLog combatLog = CombatLog.Instance;
                if (combatLog != null)
                {
                    combatLog.AddEnemyLootDrops(enemyData.enemyName, drops);
                }
                
                Debug.Log($"[Immediate Loot] {enemyData.enemyName} dropped {drops.Count} items");
            }
        }
    }

	private void AwardExperienceForKill(EnemyData enemyData)
	{
		if (enemyData == null)
			return;

		var characterMgr = CharacterManager.Instance;
		if (characterMgr == null || !characterMgr.HasCharacter())
			return;

		int areaLevel = 1;
		var currentEncounter = EncounterManager.Instance != null ? EncounterManager.Instance.GetCurrentEncounter() : null;
		if (currentEncounter != null)
		{
			areaLevel = Mathf.Max(1, currentEncounter.areaLevel);
		}

		// Base XP pulls from enemy data, fallback scales modestly with area level
		int baseXp = Mathf.Max(0, enemyData.experienceReward);
		if (baseXp == 0)
		{
			baseXp = Mathf.RoundToInt(10f * (1f + 0.15f * (areaLevel - 1)));
		}

		if (baseXp <= 0)
			return;

		characterMgr.AddExperience(baseXp);
		characterMgr.SaveCharacter();
		Debug.Log($"[Combat XP] Awarded {baseXp} XP for defeating {enemyData.enemyName} (Area Lv {areaLevel}).");
	}

	private LootDropResult BuildLootResultFromRewards(List<LootReward> rewards)
	{
		LootDropResult result = new LootDropResult();
		if (rewards == null)
			return result;

		foreach (var reward in rewards)
		{
			if (reward != null)
			{
				result.AddReward(reward);
			}
		}
		return result;
	}

	private void AddBaseEncounterExperience(LootDropResult result, int areaLevel)
	{
		if (result == null)
			return;

		int experienceAmount = Mathf.RoundToInt(50f + 10f * (areaLevel - 1));
		if (experienceAmount > 0)
		{
			result.AddExperience(experienceAmount);
		}
	}

	private void AddSpiritDropsFromDefeatedEnemies(LootDropResult result)
	{
		if (result == null || defeatedEnemiesData == null || defeatedEnemiesData.Count == 0)
			return;

		foreach (var enemyData in defeatedEnemiesData)
		{
			if (enemyData == null || enemyData.spiritTags == null)
				continue;

			foreach (var tag in enemyData.spiritTags)
			{
				bool guaranteed = enemyData.guaranteedSpiritDrop;
				bool shouldDrop = guaranteed || UnityEngine.Random.Range(0f, 100f) <= 3f;
				if (shouldDrop)
				{
					var currencyType = GetSpiritCurrencyForTag(tag);
					result.AddCurrency(currencyType, 1);
				}
			}
		}
	}

	private Dexiled.Data.Items.CurrencyType GetSpiritCurrencyForTag(EnemySpiritTag tag)
	{
		switch (tag)
		{
			case EnemySpiritTag.Fire:
				return Dexiled.Data.Items.CurrencyType.FireSpirit;
			case EnemySpiritTag.Cold:
				return Dexiled.Data.Items.CurrencyType.ColdSpirit;
			case EnemySpiritTag.Lightning:
				return Dexiled.Data.Items.CurrencyType.LightningSpirit;
			case EnemySpiritTag.Chaos:
				return Dexiled.Data.Items.CurrencyType.ChaosSpirit;
			case EnemySpiritTag.Physical:
				return Dexiled.Data.Items.CurrencyType.PhysicalSpirit;
			case EnemySpiritTag.Life:
				return Dexiled.Data.Items.CurrencyType.LifeSpirit;
			case EnemySpiritTag.Defense:
				return Dexiled.Data.Items.CurrencyType.DefenseSpirit;
			case EnemySpiritTag.Crit:
				return Dexiled.Data.Items.CurrencyType.CritSpirit;
			default:
				return Dexiled.Data.Items.CurrencyType.FireSpirit;
		}
	}
    
    private void RefreshAllDisplays()
    {
        if (playerDisplay != null)
        {
            playerDisplay.RefreshDisplay();
        }
        
        if (enemySpawner != null)
        {
            foreach (EnemyCombatDisplay enemyDisplay in enemySpawner.GetActiveEnemies())
            {
                if (enemyDisplay != null)
                {
                    enemyDisplay.RefreshDisplay();
                }
            }
        }
    }

    private void NotifyEnemyAbilityRunners(Action<EnemyAbilityRunner> notifyAction, bool requireAlive = true)
    {
        if (notifyAction == null || enemySpawner == null)
            return;

        var displays = enemySpawner.GetActiveEnemies();
        foreach (var display in displays)
        {
            if (display == null)
                continue;

            var enemy = display.GetEnemy();
            if (requireAlive && (enemy == null || enemy.currentHealth <= 0))
                continue;

            var runner = display.GetAbilityRunner();
            if (runner != null)
            {
                notifyAction(runner);
            }
        }
    }
    
    /// <summary>
    /// Despawn an enemy at a specific index (when defeated)
    /// </summary>
    private void DespawnEnemyAtIndex(int enemyIndex)
    {
        if (enemySpawner == null) return;
        
        var activeDisplays = enemySpawner.GetActiveEnemies();
        if (enemyIndex >= 0 && enemyIndex < activeDisplays.Count)
        {
            var display = activeDisplays[enemyIndex];
            display?.NotifyAbilityRunnerDeath();
            enemySpawner.DespawnEnemy(display);
            Debug.Log($"[Despawn] Despawned enemy at index {enemyIndex}");
        }

        RebuildActiveEnemiesFromSpawner();
    }

    private void RebuildActiveEnemiesFromSpawner()
    {
        if (enemySpawner == null)
            return;

        var activeDisplays = enemySpawner.GetActiveEnemies();
        activeEnemies.Clear();
        foreach (var display in activeDisplays)
        {
            var enemy = display?.GetEnemy();
            if (enemy != null && enemy.currentHealth > 0)
            {
                activeEnemies.Add(enemy);
            }
        }
    }
    
    // Public methods for external control
    [System.Obsolete("Use SpawnSpecificEnemy with EnemyData instead", true)]
    public void AddEnemy(Enemy enemy)
    {
        Debug.LogWarning("[CombatManager] AddEnemy is deprecated. Use SpawnSpecificEnemy with EnemyData.");
    }
    
    [System.Obsolete("Use DespawnEnemyAtIndex or enemySpawner.DespawnEnemy instead", true)]
    public void RemoveEnemy(Enemy enemy)
    {
        Debug.LogWarning("[CombatManager] RemoveEnemy is deprecated.");
    }

    /// <summary>
    /// Spawn a specific enemy data into the first available slot with a fixed rarity.
    /// Returns true if spawned.
    /// </summary>
    public bool SpawnSpecificEnemy(EnemyData data, EnemyRarity rarity)
    {
        if (data == null) return false;
        if (enemySpawner == null)
        {
            Debug.LogError("[SpawnSpecificEnemy] EnemySpawner is null!");
            return false;
        }
        
        // Find first available spawn point
        int maxSpawns = enemySpawner.GetMaxSpawnPoints();
        var activeDisplays = enemySpawner.GetActiveEnemies();
        
        for (int i = 0; i < maxSpawns; i++)
        {
            // Check if this spawn point is available
            if (i >= activeDisplays.Count || activeDisplays[i] == null)
            {
                // Spawn the enemy
                EnemyCombatDisplay display = enemySpawner.SpawnEnemy(data, i);
                if (display != null)
                {
                    Enemy enemy = display.GetEnemy();
                    if (enemy != null)
                    {
                        // Apply rarity name suffix
                switch (rarity)
                {
                    case EnemyRarity.Magic: enemy.enemyName = $"{enemy.enemyName} (Magic)"; break;
                    case EnemyRarity.Rare: enemy.enemyName = $"{enemy.enemyName} (Rare)"; break;
                    case EnemyRarity.Unique: enemy.enemyName = $"{enemy.enemyName} (Unique)"; break;
                }
                        
                activeEnemies.Add(enemy);
                        Debug.Log($"[SpawnSpecificEnemy] Spawned {enemy.enemyName} at spawn point {i}");
                return true;
            }
        }
            }
        }
        
        Debug.LogWarning("[SpawnSpecificEnemy] No available spawn points to spawn specific enemy.");
        return false;
    }
    
    public List<Enemy> GetActiveEnemies()
    {
        return new List<Enemy>(activeEnemies);
    }
    
    /// <summary>
    /// Get list of currently active enemy displays (for external systems)
    /// </summary>
    public List<EnemyCombatDisplay> GetActiveEnemyDisplays()
    {
        if (enemySpawner != null)
        {
            return enemySpawner.GetActiveEnemies();
        }
        return new List<EnemyCombatDisplay>();
    }
    
    public bool IsPlayerTurn()
    {
        return isPlayerTurn;
    }
    
    public CombatState GetCombatState()
    {
        return currentState;
    }
    
    // Debug methods
    [ContextMenu("Start Combat")]
    public void DebugStartCombat()
    {
        StartCombat();
    }
    
    [ContextMenu("End Player Turn")]
    public void DebugEndPlayerTurn()
    {
        EndPlayerTurn();
    }
    
    [ContextMenu("Test Player Attack")]
    public void DebugPlayerAttack()
    {
        if (activeEnemies.Count > 0)
        {
            PlayerAttackEnemy(0, 15);
        }
    }
    
    [ContextMenu("Test Player Heal")]
    public void DebugPlayerHeal()
    {
        PlayerHeal(10);
    }
    
    /// <summary>
    /// Spawn a specific number of enemies (for testing different encounter sizes)
    /// </summary>
    public void SpawnEnemies(int enemyCount)
    {
        if (enemySpawner == null)
        {
            Debug.LogError("[SpawnEnemies] EnemySpawner is null!");
            return;
        }
        
        // Limit to max enemies and available spawn points
        int maxSpawns = enemySpawner.GetMaxSpawnPoints();
        enemyCount = Mathf.Min(enemyCount, maxEnemies, maxSpawns);
        
        // Clear existing enemies
        enemySpawner.DespawnAllEnemies();
        activeEnemies.Clear();
        
        // Spawn test enemies
        List<string> testEnemyNames = new List<string> { "Goblin Scout", "Orc Warrior", "Dark Mage", "Skeleton", "Zombie" };
        
        for (int i = 0; i < enemyCount; i++)
        {
            EnemyData testData = ScriptableObject.CreateInstance<EnemyData>();
            testData.enemyName = i < testEnemyNames.Count ? testEnemyNames[i] : $"Test Enemy {i+1}";
            testData.minHealth = 30 + (i * 15);
            testData.maxHealth = 30 + (i * 15);
            testData.baseDamage = 6 + (i * 2);
            
            EnemyCombatDisplay display = enemySpawner.SpawnEnemy(testData, i);
            if (display != null)
            {
                Enemy enemy = display.GetEnemy();
                if (enemy != null)
                {
                    activeEnemies.Add(enemy);
                }
            }
        }
        
        // Update displays
        RefreshAllDisplays();
        
        Debug.Log($"<color=green>Spawned {activeEnemies.Count} test enemies</color>");
    }
    
    [ContextMenu("Test Spawn 1 Enemy")]
    private void TestSpawn1Enemy()
    {
        SpawnEnemies(1);
    }
    
    [ContextMenu("Test Spawn 2 Enemies")]
    private void TestSpawn2Enemies()
    {
        SpawnEnemies(2);
    }
    
    [ContextMenu("Test Spawn 3 Enemies")]
    private void TestSpawn3Enemies()
    {
        SpawnEnemies(3);
    }
    
    [ContextMenu("Create Test Enemies")]
    public void DebugCreateEnemies()
    {
        CreateTestEnemies();
    }
    
    [ContextMenu("Force Cleanup Defeated Enemies")]
    public void DebugForceCleanup()
    {
        Debug.Log("=== FORCE CLEANUP DEFEATED ENEMIES ===");
        CleanupDefeatedEnemies();
        ForceCleanupDeadEnemies();
        Debug.Log("Cleanup complete!");
    }
    
    [ContextMenu("Debug Enemy Status")]
    public void DebugEnemyStatus()
    {
        Debug.Log("=== ENEMY STATUS DEBUG ===");
        Debug.Log($"Active Enemies Count: {activeEnemies.Count}");
        
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            var enemy = activeEnemies[i];
            if (enemy != null)
            {
                Debug.Log($"  Enemy {i}: {enemy.enemyName} - HP: {enemy.currentHealth}/{enemy.maxHealth}");
            }
            else
            {
                Debug.LogWarning($"  Enemy {i}: NULL!");
            }
        }
        
        if (enemySpawner != null)
        {
            var displays = enemySpawner.GetActiveEnemies();
            Debug.Log($"Active Displays Count: {displays.Count}");
            
            for (int i = 0; i < displays.Count; i++)
            {
                var display = displays[i];
                if (display != null)
                {
                    var enemy = display.GetEnemy();
                    if (enemy != null)
                    {
                        Debug.Log($"  Display {i}: {enemy.enemyName} - HP: {enemy.currentHealth}/{enemy.maxHealth} - Active: {display.gameObject.activeInHierarchy}");
                    }
                    else
                    {
                        Debug.LogWarning($"  Display {i}: NULL enemy!");
                    }
                }
                else
                {
                    Debug.LogWarning($"  Display {i}: NULL display!");
                }
            }
        }
        
        Debug.Log("=== END ENEMY STATUS DEBUG ===");
    }
    
    [ContextMenu("Fix AoE Stuck Enemies")]
    public void DebugFixAoEStuckEnemies()
    {
        Debug.Log("=== FIXING AoE STUCK ENEMIES ===");
        
        if (enemySpawner != null)
        {
            var displays = enemySpawner.GetActiveEnemies();
            int fixedCount = 0;
            
            for (int i = 0; i < displays.Count; i++)
            {
                var display = displays[i];
                if (display != null && display.gameObject.activeInHierarchy)
                {
                    var enemy = display.GetEnemy();
                    if (enemy != null && enemy.currentHealth <= 0)
                    {
                        Debug.Log($"<color=red>FIXING: Display {i} has dead enemy {enemy.enemyName} (HP: {enemy.currentHealth})</color>");
                        
                        // Remove from active enemies list
                        if (activeEnemies.Contains(enemy))
                        {
                            activeEnemies.Remove(enemy);
                            Debug.Log($"  ✓ Removed from active enemies list");
                        }
                        
                        // Force despawn the display
                        display.gameObject.SetActive(false);
                        Debug.Log($"  ✓ Force despawned display");
                        
                        fixedCount++;
                    }
                }
            }
            
            Debug.Log($"<color=green>Fixed {fixedCount} stuck enemies</color>");
            
            if (fixedCount > 0)
            {
                CheckWaveCompletion();
            }
        }
        
		Debug.Log("=== END AoE STUCK ENEMIES FIX ===");
    }
    
    /// <summary>
    /// Collect enemies that were spawned with animations and add them to the active list
    /// </summary>
    private IEnumerator CollectSpawnedEnemies(List<EnemyData> encounterData)
    {
        // Wait for all spawn animations to complete
        float maxWaitTime = (encounterData.Count * enemySpawner.spawnStaggerDelay) + enemySpawner.spawnAnimationDuration + 0.5f;
        yield return new WaitForSeconds(maxWaitTime);
        
        // Collect all active enemies from the spawner
        var activeDisplays = enemySpawner.GetActiveEnemies();
        activeEnemies.Clear();
        
        foreach (var display in activeDisplays)
        {
            if (display != null && display.gameObject.activeInHierarchy)
            {
                var enemy = display.GetEnemy();
                if (enemy != null)
                {
                    activeEnemies.Add(enemy);
                    Debug.Log($"[Wave Spawn] ✓ Collected {enemy.enemyName} from spawner");
                }
            }
        }
        
        Debug.Log($"[EncounterDebug] <color=green>✓ Wave {currentWave}/{totalWaves}: Collected {activeEnemies.Count} enemies with animations</color>");
    }
    
    [ContextMenu("Check Enemy Setup")]
    public void DebugCheckEnemySetup()
    {
        Debug.Log("=== ENEMY SETUP CHECK ===");
        
        // Check database
        if (enemyDatabase == null)
            enemyDatabase = EnemyDatabase.Instance;
        
        if (enemyDatabase != null)
        {
            Debug.Log($"✓ EnemyDatabase found with {enemyDatabase.allEnemies.Count} enemies");
            
            EnemyData coconut = enemyDatabase.GetEnemyByName("CoconutCrab");
            if (coconut != null)
            {
                Debug.Log($"✓ CoconutCrab in database");
                Debug.Log($"  Sprite assigned? {coconut.enemySprite != null}");
                if (coconut.enemySprite != null)
                    Debug.Log($"  Sprite name: {coconut.enemySprite.name}");
            }
            else
            {
                Debug.LogWarning("✗ CoconutCrab not in database!");
            }
        }
        else
        {
            Debug.LogError("✗ EnemyDatabase not found!");
        }
        
        // Check spawner and displays
        if (enemySpawner != null)
        {
            var activeDisplays = enemySpawner.GetActiveEnemies();
            Debug.Log($"\nEnemy Spawner: {enemySpawner.GetMaxSpawnPoints()} spawn points, {activeDisplays.Count} active enemies");
            
            for (int i = 0; i < activeDisplays.Count; i++)
            {
                if (activeDisplays[i] != null)
                {
                    Enemy enemy = activeDisplays[i].GetEnemy();
                if (enemy != null)
                {
                        Debug.Log($"  Spawn Point {i}: {enemy.enemyName} (HP: {enemy.currentHealth}/{enemy.maxHealth})");
                }
                else
                {
                        Debug.Log($"  Spawn Point {i}: Display active but no enemy data");
                }
            }
            }
        }
        else
        {
            Debug.LogError("✗ EnemySpawner not found!");
        }
        
        Debug.Log($"\nActive Enemies: {activeEnemies.Count}");
    }
}


using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
    private LootDropResult pendingLoot;
    private List<EnemyData> defeatedEnemiesData = new List<EnemyData>();
    
    [Header("Combat Effects")]
    private CombatEffectManager combatEffectManager;
    private FloatingDamageManager floatingDamageManager;
    
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
        
        // Initialize waves and spawn first wave based on encounter if present; otherwise fall back
        if (totalWaves < 1) totalWaves = 1;
        var enc = EncounterManager.Instance != null ? EncounterManager.Instance.GetCurrentEncounter() : null;
        if (enc != null)
        {
            totalWaves = Mathf.Max(1, enc.totalWaves);
            enemiesPerWave = Mathf.Max(1, enc.maxEnemiesPerWave);
            randomizeEnemyCount = enc.randomizeEnemyCount;
            encounterLootTable = enc.lootTable; // Set loot table from encounter
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
		// Randomize enemy count between 1 and max enemies per wave, or use exact count
		int spawnCount = randomizeEnemyCount ? Random.Range(1, enemiesPerWave + 1) : enemiesPerWave;
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
		int spawnCount = randomizeEnemyCount ? Random.Range(1, enemiesPerWave + 1) : enemiesPerWave;
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
		// If this is the final wave and the encounter has a unique boss, force it here
		var currentEncounter = EncounterManager.Instance != null ? EncounterManager.Instance.GetCurrentEncounter() : null;
		bool isFinalWave = currentWave >= totalWaves;
			
		if (isFinalWave && currentEncounter != null && currentEncounter.uniqueEnemy != null)
		{
			encounterData = new List<EnemyData> { currentEncounter.uniqueEnemy };
			// If boss has a summon pool, allow additional minions to co-spawn on final wave
			if (currentEncounter.uniqueEnemy.summonPool != null && currentEncounter.uniqueEnemy.summonPool.Count > 0)
			{
					int remainingSlots = Mathf.Clamp(spawnCount - 1, 0, maxSpawns - 1);
				for (int s = 0; s < remainingSlots; s++)
				{
					var pick = currentEncounter.uniqueEnemy.summonPool[Random.Range(0, currentEncounter.uniqueEnemy.summonPool.Count)];
					encounterData.Add(pick);
				}
			}
				spawnCount = Mathf.Min(encounterData.Count, maxSpawns);
		}
		else
		{
			encounterData = enemyDatabase.GetRandomEncounter(spawnCount, EnemyTier.Boss);
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
			
			// Use the new animation system for spawning enemies
			enemySpawner.SpawnEnemiesWithAnimation(encounterData);
			
			// Add enemies to active list after spawning (they'll be added individually in SpawnEnemy)
			// We need to wait a frame for the coroutines to complete, then collect the active enemies
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
        
        // Safety check: Clean up any stuck defeated enemies before player turn
        CleanupDefeatedEnemies();
        
        // Additional safety: Force cleanup any enemies that should be dead
        ForceCleanupDeadEnemies();
        
        // Advance status effects at the start of each turn
        AdvanceAllStatusEffects();
        
        // Restore player resources
        if (characterManager != null && characterManager.HasCharacter())
        {
            Character character = characterManager.GetCurrentCharacter();
            character.RestoreMana(character.manaRecoveryPerTurn);
            
            // Draw cards using CombatDeckManager (only if not the first turn)
            CombatDeckManager deckManager = CombatDeckManager.Instance;
            if (deckManager != null && currentTurn > 1)
            {
                int cardsToDraw = character.cardsDrawnPerTurn;
                Debug.Log($"<color=yellow>Drawing {cardsToDraw} cards for turn {currentTurn}...</color>");
                deckManager.DrawCards(cardsToDraw);
            }
            else if (currentTurn == 1)
            {
                Debug.Log($"<color=green>Turn {currentTurn}: Skipping card draw (initial hand already drawn)</color>");
            }
            else
            {
                Debug.LogWarning("CombatDeckManager not found! Cannot draw cards automatically.");
            }
        }
        
        // Update displays
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
        
        // Advance status effects at the start of each turn
        AdvanceAllStatusEffects();
        
        // Update displays
        RefreshAllDisplays();
        
        OnCombatStateChanged?.Invoke(currentState);
        OnTurnTypeChanged?.Invoke(isPlayerTurn);
        
        Debug.Log($"Enemy turn {currentTurn} started");
        
        // Execute enemy actions
        StartCoroutine(ExecuteEnemyActions());
    }
    
    private IEnumerator ExecuteEnemyActions()
    {
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
        
        // End enemy turn and start next player turn
        EndEnemyTurn();
    }
    
    private IEnumerator ExecuteEnemyAction(Enemy enemy, int enemyIndex)
    {
        Debug.Log($"{enemy.enemyName} is taking action...");
        
        // Get the enemy display for animation
        var activeDisplays = enemySpawner != null ? enemySpawner.GetActiveEnemies() : new List<EnemyCombatDisplay>();
        EnemyCombatDisplay enemyDisplay = (enemyIndex >= 0 && enemyIndex < activeDisplays.Count) ? activeDisplays[enemyIndex] : null;
        
        switch (enemy.currentIntent)
        {
            case EnemyIntent.Attack:
                // Play attack animation
                if (enemyDisplay != null)
                {
                    enemyDisplay.PlayAttackAnimation();
                    yield return new WaitForSeconds(0.3f); // Wait for animation to start
                }
                
                // Attack the player
                if (characterManager != null && characterManager.HasCharacter())
                {
                    int damage = enemy.GetAttackDamage();
                    characterManager.TakeDamage(damage);
                    Debug.Log($"{enemy.enemyName} attacks for {damage} damage!");
                    
                    // Show floating damage on player
                    if (floatingDamageManager != null && playerDisplay != null)
                    {
                        floatingDamageManager.ShowDamage(damage, false, playerDisplay.transform);
                    }
                }
                break;
                
            case EnemyIntent.Defend:
                // Enemy defends (could add block/armor system)
                Debug.Log($"{enemy.enemyName} is defending!");
                break;
        }
        
        // Set new intent for next turn
        enemy.SetIntent();
        
        // Update enemy display
        if (enemyDisplay != null)
        {
            enemyDisplay.UpdateIntent();
        }
        
        yield return null;
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
    
    public void PlayerAttackEnemy(int enemyIndex, float damage)
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
        
        bool wasCritical = Random.Range(0f, 1f) < 0.1f; // 10% critical chance for demo
            
            if (wasCritical)
            {
                damage *= 1.5f; // Critical damage multiplier
            }
            
            // REMOVED: targetEnemy.TakeDamage(damage) - was causing double damage
            // The EnemyCombatDisplay.TakeDamage() below handles applying damage to the enemy
            
            // Play damage impact effect
            if (combatEffectManager != null && enemyIndex < activeDisplays.Count)
            {
                // Use physical damage as default for direct attacks
                combatEffectManager.PlayElementalDamageEffectOnTarget(activeDisplays[enemyIndex].transform, DamageType.Physical, wasCritical);
            }
            
            // Update enemy display and apply damage
            if (enemyIndex < activeDisplays.Count)
            {
                activeDisplays[enemyIndex].TakeDamage(damage);
                activeDisplays[enemyIndex].PlayDamageAnimation();
                
                // Show floating damage number
                if (floatingDamageManager != null)
                {
                    floatingDamageManager.ShowDamage(damage, wasCritical, activeDisplays[enemyIndex].transform);
                }
            }
            
				// Check if enemy is defeated
				if (targetEnemy.currentHealth <= 0)
				{
					Debug.Log($"[Enemy Defeat] {targetEnemy.enemyName} defeated! HP: {targetEnemy.currentHealth}");
					
					// Prevent double-defeat by immediately marking enemy as defeated
					if (activeEnemies.Contains(targetEnemy))
					{
						// Generate and apply immediate loot drops from this enemy
						var enemyDisplaysList = enemySpawner != null ? enemySpawner.GetActiveEnemies() : new List<EnemyCombatDisplay>();
						var disp = (enemyIndex < enemyDisplaysList.Count) ? enemyDisplaysList[enemyIndex] : null;
						
						if (disp != null)
						{
							EnemyData enemyData = disp.GetEnemyData();
							if (enemyData != null)
							{
								// Track for end-of-combat loot table
								if (!defeatedEnemiesData.Contains(enemyData))
								{
									defeatedEnemiesData.Add(enemyData);
								}
								
								// Generate immediate loot drops
								GenerateAndApplyImmediateLoot(enemyData);
							}
						}
						
						// Remove from active enemies IMMEDIATELY to prevent stuck enemies
						OnEnemyDefeated?.Invoke(targetEnemy);
						int idx = activeEnemies.IndexOf(targetEnemy);
						if (idx >= 0) 
						{
							activeEnemies.RemoveAt(idx);
							Debug.Log($"[Enemy Defeat] Removed {targetEnemy.enemyName} from active enemies. Remaining: {activeEnemies.Count}");
						}
						
						// Start fade-out animation and despawn after
						System.Action despawnEnemy = () => {
							DespawnEnemyAtIndex(enemyIndex);
							Debug.Log($"[Enemy Defeat] Despawned enemy at index {enemyIndex}");
							
							// Check if wave is complete (only if not in AoE attack)
							if (!isAoEAttackInProgress)
							{
								CheckWaveCompletion();
							}
						};
						
						if (disp != null && disp.gameObject.activeInHierarchy)
						{
							// Start fade-out with safety timeout
							disp.StartDeathFadeOut(despawnEnemy);
							
							// Safety fallback: Force despawn after 1 second if animation doesn't complete
							StartCoroutine(ForceDespawnAfterDelay(enemyIndex, disp, 1.0f));
						}
						else
						{
							// No display or display inactive - despawn immediately
							Debug.LogWarning($"[Enemy Defeat] Display null or inactive for {targetEnemy.enemyName}, despawning immediately");
							despawnEnemy();
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
                        
                        // Force despawn the display
                        display.gameObject.SetActive(false);
                        Debug.Log($"[Force Cleanup] Force despawned dead enemy display at index {index}");
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
		if (activeEnemies.Count == 0)
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
			Debug.Log($"[Wave Status] {activeEnemies.Count} enemies remaining");
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
            display.gameObject.SetActive(false);
            // Don't call CheckWaveCompletion again as despawnEnemy callback already did
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
        // Advance player status effects
        if (playerDisplay != null)
        {
            StatusEffectManager playerStatusManager = playerDisplay.GetStatusEffectManager();
            if (playerStatusManager != null)
            {
                playerStatusManager.AdvanceAllEffectsOneTurn();
            }
        }
        
        // Advance enemy status effects
        EnemyCombatDisplay[] activeEnemyDisplays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        foreach (var enemyDisplay in activeEnemyDisplays)
        {
            if (enemyDisplay != null && enemyDisplay.gameObject.activeInHierarchy)
            {
                StatusEffectManager enemyStatusManager = enemyDisplay.GetStatusEffectManager();
                if (enemyStatusManager != null)
                {
                    enemyStatusManager.AdvanceAllEffectsOneTurn();
                }
            }
        }
        
        Debug.Log($"<color=cyan>Advanced all status effects by one turn</color>");
    }
    
    private void EndCombat(bool victory)
    {
        currentState = victory ? CombatState.Victory : CombatState.Defeat;
        
        if (victory)
        {
            // Generate loot rewards
            GenerateLootRewards();
        }
        
        OnCombatStateChanged?.Invoke(currentState);
        OnCombatEnded?.Invoke();
        
        Debug.Log($"Combat ended: {(victory ? "Victory!" : "Defeat!")}");
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
        
        // Generate loot from loot table
        LootManager lootManager = LootManager.Instance;
        if (lootManager != null)
        {
            // Pass defeated enemies data to enable tag-based spirit drops
            pendingLoot = lootManager.GenerateLoot(encounterLootTable, areaLevel, defeatedEnemiesData);
            
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
        else
        {
            Debug.LogError("[Combat Victory] LootManager not found!");
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
    
    /// <summary>
    /// Despawn an enemy at a specific index (when defeated)
    /// </summary>
    private void DespawnEnemyAtIndex(int enemyIndex)
    {
        if (enemySpawner == null) return;
        
        // Find the enemy display at this index
        var activeDisplays = enemySpawner.GetActiveEnemies();
        if (enemyIndex >= 0 && enemyIndex < activeDisplays.Count)
        {
            enemySpawner.DespawnEnemy(activeDisplays[enemyIndex]);
            Debug.Log($"[Despawn] Despawned enemy at index {enemyIndex}");
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


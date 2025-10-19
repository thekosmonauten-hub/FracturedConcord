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
    public List<EnemyCombatDisplay> enemyDisplays = new List<EnemyCombatDisplay>();
    
    [Header("Combat Settings")]
    public int maxEnemies = 3;
    public bool autoStartCombat = true;
    public float turnDelay = 1f;
    
    [Header("Combat State")]
    public CombatState currentState = CombatState.Setup;
    public int currentTurn = 0;
    public bool isPlayerTurn = true;
	
	[Header("Waves")]
	public int totalWaves = 1; // 1 means old behavior
	public int currentWave = 0; // 1-based when active
    
    [Header("Test Configuration")]
    public bool createTestEnemies = true;
    public int testEnemyCount = 2;
    
    [Header("Enemy Data")]
    [SerializeField] private EnemyDatabase enemyDatabase;
    
    [Header("Combat Effects")]
    private CombatEffectManager combatEffectManager;
    
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
        
        // Auto-find player display if not assigned
        if (playerDisplay == null)
        {
            playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
        }
        
		// Auto-find enemy displays if not assigned
		if (enemyDisplays.Count == 0)
		{
			if (enemyDisplayRoot != null)
			{
				var children = enemyDisplayRoot.GetComponentsInChildren<EnemyCombatDisplay>(true);
				if (children != null && children.Length > 0) enemyDisplays.AddRange(children);
			}
			if (enemyDisplays.Count == 0)
			{
				var foundActive = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
				if (foundActive != null && foundActive.Length > 0) enemyDisplays.AddRange(foundActive);
			}
			if (enemyDisplays.Count == 0)
			{
				var foundAll = Resources.FindObjectsOfTypeAll<EnemyCombatDisplay>();
				foreach (var disp in foundAll)
				{
					if (disp != null && disp.gameObject.scene.IsValid()) enemyDisplays.Add(disp);
				}
			}
		}

		if (enemyDisplays.Count == 0)
		{
			Debug.LogError("CombatDisplayManager: No EnemyCombatDisplay panels found in scene. Assign enemyDisplayRoot or add panels.");
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
            Debug.Log($"[EncounterDebug] CombatDisplayManager: Starting encounter {enc.encounterID} '{enc.encounterName}' with {totalWaves} waves.");
            StartFirstWave();
        }
        else if (createTestEnemies)
        {
            Debug.LogWarning("[EncounterDebug] CombatDisplayManager: No current encounter found on load. Spawning test enemies.");
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
		OnWaveChanged?.Invoke(currentWave, totalWaves);
		// Ensure clean state
		DisableAllEnemyDisplays();
		activeEnemies.Clear();
		SpawnWaveInternal(testEnemyCount);
	}

	private void StartNextWave()
	{
		currentWave = Mathf.Clamp(currentWave + 1, 1, totalWaves);
		OnWaveChanged?.Invoke(currentWave, totalWaves);
		DisableAllEnemyDisplays();
		activeEnemies.Clear();
		SpawnWaveInternal(testEnemyCount);
		// Begin next player turn automatically
		StartPlayerTurn();
	}

	private void SpawnWaveInternal(int desiredCount)
	{
		// First, disable all enemy displays
		DisableAllEnemyDisplays();
		
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
		
		int spawnCount = Mathf.Clamp(desiredCount, 1, Mathf.Min(maxEnemies, enemyDisplays.Count));
		if (spawnCount <= 0)
		{
			Debug.LogError("CombatDisplayManager: spawnCount resolved to 0 because no enemy displays are available.");
			return;
		}
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
				int remainingSlots = Mathf.Clamp(spawnCount - 1, 0, enemyDisplays.Count - 1);
				for (int s = 0; s < remainingSlots; s++)
				{
					var pick = currentEncounter.uniqueEnemy.summonPool[Random.Range(0, currentEncounter.uniqueEnemy.summonPool.Count)];
					encounterData.Add(pick);
				}
			}
			spawnCount = Mathf.Min(encounterData.Count, enemyDisplays.Count);
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
			for (int i = 0; i < encounterData.Count && i < enemyDisplays.Count; i++)
			{
				EnemyRarity rolled = enemyDatabase.RollRarity(false);
				Enemy enemy = encounterData[i].CreateEnemyWithRarity(rolled);
				enemyDisplays[i].SetEnemy(enemy, encounterData[i]);
				switch (rolled)
				{
					case EnemyRarity.Magic: enemy.enemyName = $"{enemy.enemyName} (Magic)"; break;
					case EnemyRarity.Rare: enemy.enemyName = $"{enemy.enemyName} (Rare)"; break;
					case EnemyRarity.Unique: enemy.enemyName = $"{enemy.enemyName} (Unique)"; break;
				}
				activeEnemies.Add(enemy);
				EnableEnemyDisplay(i);
			}
			Debug.Log($"[EncounterDebug] <color=green>✓ Wave {currentWave}/{totalWaves}: Spawned {activeEnemies.Count} enemies</color>");
			for (int di = 0; di < enemyDisplays.Count; di++)
			{
				var e = enemyDisplays[di].GetCurrentEnemy();
				Debug.Log($"[EncounterDebug]   Slot {di}: {(e != null ? e.enemyName : "[empty]")}");
			}
		}
		else
		{
			// Fallback: Create hardcoded test enemies if no database
			List<Enemy> testEnemies = new List<Enemy>
			{
				new Enemy("Goblin Scout", 30, 6),
				new Enemy("Orc Warrior", 45, 8),
				new Enemy("Dark Mage", 35, 10)
			};
			for (int i = 0; i < Mathf.Min(spawnCount, enemyDisplays.Count); i++)
			{
				if (i < testEnemies.Count)
				{
					enemyDisplays[i].SetEnemy(testEnemies[i]);
					activeEnemies.Add(testEnemies[i]);
					EnableEnemyDisplay(i);
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
        // Execute each enemy's action
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            Enemy enemy = activeEnemies[i];
            if (enemy.currentHealth > 0)
            {
                yield return ExecuteEnemyAction(enemy, i);
                yield return new WaitForSeconds(turnDelay);
            }
        }
        
        // End enemy turn and start next player turn
        EndEnemyTurn();
    }
    
    private IEnumerator ExecuteEnemyAction(Enemy enemy, int enemyIndex)
    {
        Debug.Log($"{enemy.enemyName} is taking action...");
        
        switch (enemy.currentIntent)
        {
            case EnemyIntent.Attack:
                // Attack the player
                if (characterManager != null && characterManager.HasCharacter())
                {
                    int damage = enemy.GetAttackDamage();
                    characterManager.TakeDamage(damage);
                    Debug.Log($"{enemy.enemyName} attacks for {damage} damage!");
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
        if (enemyIndex < enemyDisplays.Count)
        {
            enemyDisplays[enemyIndex].UpdateIntent();
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
        if (enemyIndex >= 0 && enemyIndex < activeEnemies.Count)
        {
            Enemy targetEnemy = activeEnemies[enemyIndex];
            bool wasCritical = Random.Range(0f, 1f) < 0.1f; // 10% critical chance for demo
            
            if (wasCritical)
            {
                damage *= 1.5f; // Critical damage multiplier
            }
            
            targetEnemy.TakeDamage(damage);
            
            // Play damage impact effect
            if (combatEffectManager != null && enemyIndex < enemyDisplays.Count)
            {
                // Use physical damage as default for direct attacks
                combatEffectManager.PlayElementalDamageEffectOnTarget(enemyDisplays[enemyIndex].transform, DamageType.Physical, wasCritical);
            }
            
            // Update enemy display
            if (enemyIndex < enemyDisplays.Count)
            {
                enemyDisplays[enemyIndex].TakeDamage(damage);
                enemyDisplays[enemyIndex].PlayDamageAnimation();
            }
            
			// Check if enemy is defeated
			if (targetEnemy.currentHealth <= 0)
            {
                OnEnemyDefeated?.Invoke(targetEnemy);
                activeEnemies.RemoveAt(enemyIndex);
                
                // Disable the enemy display
                DisableEnemyDisplay(enemyIndex);
                
				// If no enemies remain, either start next wave or end combat
				if (activeEnemies.Count == 0)
				{
					if (currentWave < totalWaves)
					{
						StartNextWave();
					}
					else
					{
						EndCombat(true);
					}
				}
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
        EnemyCombatDisplay[] enemyDisplays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        foreach (var enemyDisplay in enemyDisplays)
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
        
        OnCombatStateChanged?.Invoke(currentState);
        OnCombatEnded?.Invoke();
        
        Debug.Log($"Combat ended: {(victory ? "Victory!" : "Defeat!")}");
    }
    
    private void RefreshAllDisplays()
    {
        if (playerDisplay != null)
        {
            playerDisplay.RefreshDisplay();
        }
        
        foreach (EnemyCombatDisplay enemyDisplay in enemyDisplays)
        {
            if (enemyDisplay != null)
            {
                enemyDisplay.RefreshDisplay();
            }
        }
    }
    
    /// <summary>
    /// Disable all enemy display GameObjects
    /// </summary>
    private void DisableAllEnemyDisplays()
    {
        for (int i = 0; i < enemyDisplays.Count; i++)
        {
            if (enemyDisplays[i] != null)
            {
                enemyDisplays[i].gameObject.SetActive(false);
                Debug.Log($"Disabled enemy display {i}: {enemyDisplays[i].name}");
            }
        }
    }
    
    /// <summary>
    /// Enable a specific enemy display GameObject
    /// </summary>
    private void EnableEnemyDisplay(int index)
    {
        if (index >= 0 && index < enemyDisplays.Count && enemyDisplays[index] != null)
        {
            enemyDisplays[index].gameObject.SetActive(true);
            Debug.Log($"Enabled enemy display {index}: {enemyDisplays[index].name}");
        }
    }
    
    /// <summary>
    /// Disable a specific enemy display GameObject
    /// </summary>
    private void DisableEnemyDisplay(int index)
    {
        if (index >= 0 && index < enemyDisplays.Count && enemyDisplays[index] != null)
        {
            enemyDisplays[index].gameObject.SetActive(false);
            Debug.Log($"Disabled enemy display {index}: {enemyDisplays[index].name}");
        }
    }
    
    // Public methods for external control
    public void AddEnemy(Enemy enemy)
    {
        if (activeEnemies.Count < maxEnemies)
        {
            activeEnemies.Add(enemy);
            
            // Find available enemy display
            for (int i = 0; i < enemyDisplays.Count; i++)
            {
                if (enemyDisplays[i].GetCurrentEnemy() == null)
                {
                    enemyDisplays[i].SetEnemy(enemy);
                    EnableEnemyDisplay(i);
                    break;
                }
            }
        }
    }
    
    public void RemoveEnemy(Enemy enemy)
    {
        activeEnemies.Remove(enemy);
        
        // Clear enemy display
        foreach (EnemyCombatDisplay enemyDisplay in enemyDisplays)
        {
            if (enemyDisplay.GetCurrentEnemy() == enemy)
            {
                enemyDisplay.SetEnemy(null);
                break;
            }
        }
    }

    /// <summary>
    /// Spawn a specific enemy data into the first available slot with a fixed rarity.
    /// Returns true if spawned.
    /// </summary>
    public bool SpawnSpecificEnemy(EnemyData data, EnemyRarity rarity)
    {
        if (data == null) return false;
        // Find an available panel (inactive or empty)
        for (int i = 0; i < enemyDisplays.Count; i++)
        {
            var disp = enemyDisplays[i];
            if (disp == null) continue;
            var existing = disp.GetCurrentEnemy();
            if (disp.gameObject.activeSelf == false || existing == null || existing.currentHealth <= 0)
            {
                var enemy = data.CreateEnemyWithRarity(rarity);
                // Name suffix for clarity if not Normal
                switch (rarity)
                {
                    case EnemyRarity.Magic: enemy.enemyName = $"{enemy.enemyName} (Magic)"; break;
                    case EnemyRarity.Rare: enemy.enemyName = $"{enemy.enemyName} (Rare)"; break;
                    case EnemyRarity.Unique: enemy.enemyName = $"{enemy.enemyName} (Unique)"; break;
                }
                disp.SetEnemy(enemy, data);
                if (!disp.gameObject.activeSelf) EnableEnemyDisplay(i);
                activeEnemies.Add(enemy);
                return true;
            }
        }
        Debug.LogWarning("No available enemy slots to spawn specific enemy.");
        return false;
    }
    
    public List<Enemy> GetActiveEnemies()
    {
        return new List<Enemy>(activeEnemies);
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
        // Limit to max enemies and available displays
        enemyCount = Mathf.Min(enemyCount, maxEnemies, enemyDisplays.Count);
        
        // Clear existing enemies
        activeEnemies.Clear();
        DisableAllEnemyDisplays();
        
        // Set test enemy count to match requested count
        testEnemyCount = enemyCount;
        
        // Create new enemies
        CreateTestEnemies();
        
        // Update displays
        RefreshAllDisplays();
        
        Debug.Log($"<color=green>Spawned {activeEnemies.Count} enemies</color>");
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
        
        // Check displays
        Debug.Log($"\nEnemy Displays: {enemyDisplays.Count}");
        for (int i = 0; i < enemyDisplays.Count; i++)
        {
            if (enemyDisplays[i] != null)
            {
                Enemy enemy = enemyDisplays[i].GetCurrentEnemy();
                if (enemy != null)
                {
                    Debug.Log($"  Panel {i}: {enemy.enemyName} (HP: {enemy.currentHealth}/{enemy.maxHealth})");
                }
                else
                {
                    Debug.Log($"  Panel {i}: No enemy assigned");
                }
            }
        }
        
        Debug.Log($"\nActive Enemies: {activeEnemies.Count}");
    }
}

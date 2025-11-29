using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Central database for all enemy data in the game.
/// Similar to CardDatabase.
/// </summary>
public class EnemyDatabase : MonoBehaviour
{
    public static EnemyDatabase Instance { get; private set; }
    
    [Header("Enemy Collections")]
    public List<EnemyData> allEnemies = new List<EnemyData>();

	[Header("Resources Loading")]
	[Tooltip("Folders under any Resources/ to scan for EnemyData assets. Leave empty to scan all.")]
	public string[] resourcePaths = new string[] { "Enemies" };
    
    [Header("Rarity Weights")]
    public int normalWeight = 80;
    public int magicWeight = 15;
    public int rareWeight = 5;
    public int uniqueWeight = 0; // reserved for bosses/minibosses
    
    [Header("Categorized Enemies")]
    public List<EnemyData> normalEnemies = new List<EnemyData>();
    public List<EnemyData> eliteEnemies = new List<EnemyData>();
    public List<EnemyData> bossEnemies = new List<EnemyData>();
    
    [Header("By Category")]
    public List<EnemyData> meleeEnemies = new List<EnemyData>();
    public List<EnemyData> rangedEnemies = new List<EnemyData>();
    public List<EnemyData> casterEnemies = new List<EnemyData>();
    
	private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
			if (allEnemies == null || allEnemies.Count == 0)
			{
				ReloadDatabase();
			}
			else
			{
				OrganizeEnemies();
			}
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Randomly choose an enemy rarity by weights. Unique is optionally included.
    /// </summary>
    public EnemyRarity RollRarity(bool allowUnique = false)
    {
        int uw = allowUnique ? uniqueWeight : 0;
        int total = Mathf.Max(1, normalWeight + magicWeight + rareWeight + uw);
        int roll = Random.Range(0, total);
        if (roll < normalWeight) return EnemyRarity.Normal;
        roll -= normalWeight;
        if (roll < magicWeight) return EnemyRarity.Magic;
        roll -= magicWeight;
        if (roll < rareWeight) return EnemyRarity.Rare;
        return EnemyRarity.Unique;
    }
    
    /// <summary>
    /// Organize enemies into categories for easy lookup.
    /// </summary>
	public void OrganizeEnemies()
    {
        normalEnemies.Clear();
        eliteEnemies.Clear();
        bossEnemies.Clear();
        meleeEnemies.Clear();
        rangedEnemies.Clear();
        casterEnemies.Clear();
        
		foreach (EnemyData enemy in allEnemies)
        {
            if (enemy == null) continue;
			if (enemy.excludeFromRandom) continue; // exclude summon-only or special enemies
            
            // By tier
            switch (enemy.tier)
            {
                case EnemyTier.Normal:
                    normalEnemies.Add(enemy);
                    break;
                case EnemyTier.Elite:
                case EnemyTier.Miniboss:
                    eliteEnemies.Add(enemy);
                    break;
                case EnemyTier.Boss:
                    bossEnemies.Add(enemy);
                    break;
            }
            
            // By category
            switch (enemy.category)
            {
                case EnemyCategory.Melee:
                case EnemyCategory.Tank:
                    meleeEnemies.Add(enemy);
                    break;
                case EnemyCategory.Ranged:
                    rangedEnemies.Add(enemy);
                    break;
                case EnemyCategory.Caster:
                case EnemyCategory.Support:
                    casterEnemies.Add(enemy);
                    break;
            }
        }
        
        Debug.Log($"EnemyDatabase organized: {allEnemies.Count} total enemies");
        Debug.Log($"  Normal: {normalEnemies.Count}, Elite: {eliteEnemies.Count}, Boss: {bossEnemies.Count}");
    }
    
    #region Random Enemy Selection
    
	public EnemyData GetRandomEnemy()
    {
		var pool = allEnemies.Where(e => e != null && !e.excludeFromRandom).ToList();
		if (pool.Count == 0) return null;
		return pool[Random.Range(0, pool.Count)];
    }
    
    public EnemyData GetRandomEnemyByTier(EnemyTier tier)
    {
        List<EnemyData> enemies = tier switch
        {
            EnemyTier.Normal => normalEnemies,
            EnemyTier.Elite => eliteEnemies,
            EnemyTier.Miniboss => eliteEnemies,
            EnemyTier.Boss => bossEnemies,
            _ => allEnemies
        };
        
        if (enemies.Count == 0) return null;
        return enemies[Random.Range(0, enemies.Count)];
    }
    
    public EnemyData GetRandomEnemyByCategory(EnemyCategory category)
    {
        List<EnemyData> enemies = category switch
        {
            EnemyCategory.Melee => meleeEnemies,
            EnemyCategory.Ranged => rangedEnemies,
            EnemyCategory.Caster => casterEnemies,
            EnemyCategory.Tank => meleeEnemies,
            EnemyCategory.Support => casterEnemies,
            _ => allEnemies
        };
        
        if (enemies.Count == 0) return null;
        return enemies[Random.Range(0, enemies.Count)];
    }
    
    public EnemyData GetEnemyByName(string name)
    {
        return allEnemies.FirstOrDefault(e => e.enemyName == name);
    }
    
    /// <summary>
    /// Get all enemies filtered by excludeFromRandom flag (for combining with encounter-specific pools)
    /// </summary>
    public List<EnemyData> GetFilteredEnemies(EnemyTier maxTier = EnemyTier.Normal)
    {
        List<EnemyData> filtered = new List<EnemyData>();
        
        foreach (EnemyData enemy in allEnemies)
        {
            if (enemy == null) continue;
            if (enemy.excludeFromRandom) continue; // Exclude summon-only or special enemies
            if ((int)enemy.tier <= (int)maxTier)
            {
                filtered.Add(enemy);
            }
        }
        
        return filtered;
    }

    #endregion
    
    #region Encounter Generation
    
    /// <summary>
    /// Get a random mix of enemies for an encounter.
    /// </summary>
    public List<EnemyData> GetRandomEncounter(int enemyCount, EnemyTier maxTier = EnemyTier.Normal)
    {
        List<EnemyData> encounter = new List<EnemyData>();
        List<EnemyData> availableEnemies = new List<EnemyData>();
        
        // Filter by max tier and exclude enemies marked as excludeFromRandom
        foreach (EnemyData enemy in allEnemies)
        {
            if (enemy == null) continue;
            if (enemy.excludeFromRandom) continue; // Exclude summon-only or special enemies
            if ((int)enemy.tier <= (int)maxTier)
            {
                availableEnemies.Add(enemy);
            }
        }
        
        if (availableEnemies.Count == 0)
        {
            Debug.LogWarning($"No enemies available for tier {maxTier}!");
            return encounter;
        }
        
        // Pick random enemies
        for (int i = 0; i < enemyCount; i++)
        {
            EnemyData randomEnemy = availableEnemies[Random.Range(0, availableEnemies.Count)];
            encounter.Add(randomEnemy);
        }
        
        return encounter;
    }
    
    /// <summary>
    /// Get a balanced encounter (mix of enemy types).
    /// </summary>
    public List<EnemyData> GetBalancedEncounter(int enemyCount)
    {
        List<EnemyData> encounter = new List<EnemyData>();
        
        // Try to get a mix of categories
        if (enemyCount >= 3 && meleeEnemies.Count > 0 && rangedEnemies.Count > 0)
        {
            // Get at least one of each type
            encounter.Add(meleeEnemies[Random.Range(0, meleeEnemies.Count)]);
            encounter.Add(rangedEnemies[Random.Range(0, rangedEnemies.Count)]);
            
            // Fill rest randomly
            for (int i = 2; i < enemyCount; i++)
            {
                encounter.Add(GetRandomEnemy());
            }
        }
        else
        {
            // Just random enemies
            for (int i = 0; i < enemyCount; i++)
            {
                encounter.Add(GetRandomEnemy());
            }
        }
        
        return encounter;
    }
    
    #endregion
    
    #region Editor Tools
    
    [ContextMenu("Reload and Organize")]
	public void ReloadDatabase()
    {
		// Load EnemyData assets from configured Resources paths; fallback to scan all Resources
		allEnemies.Clear();
		int loaded = 0;
		if (resourcePaths != null && resourcePaths.Length > 0)
		{
			foreach (var p in resourcePaths)
			{
				if (string.IsNullOrWhiteSpace(p)) continue;
				var arr = Resources.LoadAll<EnemyData>(p);
				if (arr != null && arr.Length > 0)
				{
					allEnemies.AddRange(arr);
					loaded += arr.Length;
				}
			}
		}
		if (loaded == 0)
		{
			var all = Resources.LoadAll<EnemyData>(string.Empty); // scan all Resources
			if (all != null && all.Length > 0)
			{
				allEnemies.AddRange(all);
				loaded += all.Length;
			}
		}
		OrganizeEnemies();
		Debug.Log($"EnemyDatabase Reloaded: loaded {loaded} EnemyData assets. Normal:{normalEnemies.Count} Elite:{eliteEnemies.Count} Boss:{bossEnemies.Count}");
    }
    
    [ContextMenu("Log Database Info")]
    public void LogDatabaseInfo()
    {
        Debug.Log("=== ENEMY DATABASE ===");
        Debug.Log($"Total Enemies: {allEnemies.Count}");
        Debug.Log($"\nBy Tier:");
        Debug.Log($"  Normal: {normalEnemies.Count}");
        Debug.Log($"  Elite: {eliteEnemies.Count}");
        Debug.Log($"  Boss: {bossEnemies.Count}");
        Debug.Log($"\nBy Category:");
        Debug.Log($"  Melee: {meleeEnemies.Count}");
        Debug.Log($"  Ranged: {rangedEnemies.Count}");
        Debug.Log($"  Caster: {casterEnemies.Count}");
        
        if (allEnemies.Count > 0)
        {
            Debug.Log($"\nAll Enemies:");
            foreach (EnemyData enemy in allEnemies)
            {
                Debug.Log($"  - {enemy.enemyName} ({enemy.tier}, {enemy.category})");
            }
        }
    }
    
    #endregion
}


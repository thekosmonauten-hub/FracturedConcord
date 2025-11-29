using UnityEngine;
using System.Collections.Generic;

namespace Dexiled.MazeSystem
{
    /// <summary>
    /// ScriptableObject configuration for maze generation parameters.
    /// </summary>
    [CreateAssetMenu(fileName = "Maze Config", menuName = "Dexiled/Maze Config", order = 1)]
    public class MazeConfig : ScriptableObject
    {
        [Header("Floor Settings")]
        [Tooltip("Total number of floors in a maze run")]
        [Range(1, 20)]
        public int totalFloors = 8;
        
        [Tooltip("Number of nodes per floor (12-16 recommended)")]
        [Range(10, 20)]
        public int minNodesPerFloor = 12;
        
        [Range(10, 20)]
        public int maxNodesPerFloor = 16;
        
        [Header("Grid Settings")]
        [Tooltip("Size of the grid for node placement (e.g., 4x4 = 16 cells)")]
        public Vector2Int gridSize = new Vector2Int(4, 4);
        
        [Header("Node Type Weights")]
        [Tooltip("Probability weights for each node type. Higher = more common.")]
        public int combatWeight = 45;
        public int chestWeight = 15;
        public int shrineWeight = 10;
        public int trapWeight = 10;
        public int forgeWeight = 10;
        public int specialWeight = 5;
        
        [Header("Floor Enemy Scaling")]
        [Tooltip("Enemy tier per floor. Normal=0, Elite=1, Boss=2")]
        public List<EnemyTier> floorEnemyTiers = new List<EnemyTier>();
        
        [Header("Boss Settings")]
        [Tooltip("Which floors spawn bosses (e.g., 2, 4, 6 = bosses on floors 2, 4, 6)")]
        public List<int> bossFloors = new List<int> { 2, 4, 6 };
        
        [Tooltip("Boss retreat HP threshold (e.g., 0.45 = retreats at 45% HP)")]
        [Range(0f, 1f)]
        public float bossRetreatThreshold = 0.45f;
        
        [Header("Grid Visual Tiles")]
        [Tooltip("Background tile sprites for each maze tier. Used for empty grid cells and as base for node buttons. Tier 1: Floors 1-2, Tier 2: Floors 3-4, Tier 3: Floors 5-6, Tier 4: Floors 7-8 (or customize based on Floors Per Tier)")]
        public Sprite tier1GridTile;
        public Sprite tier2GridTile;
        public Sprite tier3GridTile;
        public Sprite tier4GridTile;
        
        [Tooltip("How many floors per tier (default: 2 floors per tier for 8 total floors)")]
        [Range(1, 5)]
        public int floorsPerTier = 2;
        
        [Header("Maze Background Images")]
        [Tooltip("Random background images that change when entering a new room. Can be organized by tier or shared across all tiers.")]
        public List<Sprite> backgroundImages = new List<Sprite>();
        
        [Tooltip("If true, backgrounds are organized by tier. If false, all backgrounds can appear at any tier.")]
        public bool useTierBasedBackgrounds = false;
        
        [Tooltip("Tier 1 background images (Floors 1-2). Only used if Use Tier Based Backgrounds is true.")]
        public List<Sprite> tier1Backgrounds = new List<Sprite>();
        
        [Tooltip("Tier 2 background images (Floors 3-4). Only used if Use Tier Based Backgrounds is true.")]
        public List<Sprite> tier2Backgrounds = new List<Sprite>();
        
        [Tooltip("Tier 3 background images (Floors 5-6). Only used if Use Tier Based Backgrounds is true.")]
        public List<Sprite> tier3Backgrounds = new List<Sprite>();
        
        [Tooltip("Tier 4 background images (Floors 7+). Only used if Use Tier Based Backgrounds is true.")]
        public List<Sprite> tier4Backgrounds = new List<Sprite>();
        
        [Header("Maze Enemy Pools")]
        [Tooltip("Optional: Maze-specific enemy pool. All enemies in this pool can spawn at any tier, with rarity determined by tier weights below. Leave empty to use regular EnemyDatabase pools.")]
        public List<EnemyData> mazeEnemies = new List<EnemyData>(); // All maze enemies (can spawn at any tier)
        
        [Header("Maze Rarity Weights (per Tier)")]
        [Tooltip("Rarity spawn weights for Tier 1 (Floors 1-2). Higher tier = higher chance of rare enemies.")]
        public int tier1NormalWeight = 90;
        public int tier1MagicWeight = 9;
        public int tier1RareWeight = 1;
        public int tier1UniqueWeight = 0;
        
        [Tooltip("Rarity spawn weights for Tier 2 (Floors 3-4).")]
        public int tier2NormalWeight = 75;
        public int tier2MagicWeight = 20;
        public int tier2RareWeight = 5;
        public int tier2UniqueWeight = 0;
        
        [Tooltip("Rarity spawn weights for Tier 3 (Floors 5-6).")]
        public int tier3NormalWeight = 60;
        public int tier3MagicWeight = 30;
        public int tier3RareWeight = 9;
        public int tier3UniqueWeight = 1;
        
        [Tooltip("Rarity spawn weights for Tier 4 (Floors 7+).")]
        public int tier4NormalWeight = 50;
        public int tier4MagicWeight = 35;
        public int tier4RareWeight = 13;
        public int tier4UniqueWeight = 2;
        
        private void OnValidate()
        {
            // Ensure min <= max
            if (minNodesPerFloor > maxNodesPerFloor)
                maxNodesPerFloor = minNodesPerFloor;
            
            // Ensure grid can fit max nodes
            int maxGridCells = gridSize.x * gridSize.y;
            if (maxNodesPerFloor > maxGridCells)
            {
                Debug.LogWarning($"Max nodes per floor ({maxNodesPerFloor}) exceeds grid capacity ({maxGridCells}). Adjusting.");
                maxNodesPerFloor = maxGridCells;
            }
            
            // Populate floor enemy tiers if empty
            if (floorEnemyTiers.Count < totalFloors)
            {
                while (floorEnemyTiers.Count < totalFloors)
                {
                    int floorIndex = floorEnemyTiers.Count + 1;
                    EnemyTier tier = floorIndex <= 2 ? EnemyTier.Normal :
                                    floorIndex <= 4 ? EnemyTier.Elite :
                                    EnemyTier.Boss;
                    floorEnemyTiers.Add(tier);
                }
            }
        }
        
        /// <summary>
        /// Gets the total weight of all node types (for probability calculations).
        /// </summary>
        public int GetTotalNodeWeight()
        {
            return combatWeight + chestWeight + shrineWeight + trapWeight + forgeWeight + specialWeight;
        }
        
        /// <summary>
        /// Gets enemy tier for a specific floor (1-indexed).
        /// </summary>
        public EnemyTier GetEnemyTierForFloor(int floorNumber)
        {
            if (floorNumber > 0 && floorNumber <= floorEnemyTiers.Count)
                return floorEnemyTiers[floorNumber - 1];
            
            // Default fallback
            if (floorNumber <= 2) return EnemyTier.Normal;
            if (floorNumber <= 4) return EnemyTier.Elite;
            return EnemyTier.Boss;
        }
        
        /// <summary>
        /// Checks if a floor should spawn a boss.
        /// </summary>
        public bool ShouldSpawnBoss(int floorNumber)
        {
            return bossFloors.Contains(floorNumber);
        }
        
        /// <summary>
        /// Gets the tier number (1-4) for a specific floor.
        /// Tier 1-3 use floorsPerTier, Tier 4 is used for all remaining floors.
        /// Example with floorsPerTier=2: T1=1-2, T2=3-4, T3=5-6, T4=7+
        /// </summary>
        public int GetTierForFloor(int floorNumber)
        {
            if (floorNumber <= 0) return 1;
            
            // Tier 1-3 are calculated based on floorsPerTier
            int tier3End = floorsPerTier * 3; // Floors 1-6 if floorsPerTier=2
            if (floorNumber <= tier3End)
            {
                int tier = Mathf.CeilToInt((float)floorNumber / floorsPerTier);
                return Mathf.Clamp(tier, 1, 3); // Tier 1, 2, or 3
            }
            
            // Tier 4 for all floors beyond Tier 3
            return 4;
        }
        
        /// <summary>
        /// Gets the grid tile sprite for a specific floor based on its tier.
        /// </summary>
        public Sprite GetGridTileForFloor(int floorNumber)
        {
            int tier = GetTierForFloor(floorNumber);
            
            return tier switch
            {
                1 => tier1GridTile,
                2 => tier2GridTile,
                3 => tier3GridTile,
                4 => tier4GridTile,
                _ => tier1GridTile // Default fallback
            };
        }
        
        /// <summary>
        /// Gets maze-specific enemy pool (all maze enemies, not filtered by tier).
        /// Returns null if no maze-specific pool is assigned (will use regular EnemyDatabase).
        /// </summary>
        public List<EnemyData> GetMazeEnemyPool()
        {
            return mazeEnemies != null && mazeEnemies.Count > 0 ? mazeEnemies : null;
        }
        
        /// <summary>
        /// Gets a random background image for a specific floor.
        /// Uses tier-based backgrounds if enabled, otherwise uses the global background list.
        /// </summary>
        public Sprite GetRandomBackgroundForFloor(int floorNumber)
        {
            List<Sprite> availableBackgrounds;
            
            if (useTierBasedBackgrounds)
            {
                int tier = GetTierForFloor(floorNumber);
                availableBackgrounds = tier switch
                {
                    1 => tier1Backgrounds != null && tier1Backgrounds.Count > 0 ? tier1Backgrounds : backgroundImages,
                    2 => tier2Backgrounds != null && tier2Backgrounds.Count > 0 ? tier2Backgrounds : backgroundImages,
                    3 => tier3Backgrounds != null && tier3Backgrounds.Count > 0 ? tier3Backgrounds : backgroundImages,
                    4 => tier4Backgrounds != null && tier4Backgrounds.Count > 0 ? tier4Backgrounds : backgroundImages,
                    _ => backgroundImages
                };
            }
            else
            {
                availableBackgrounds = backgroundImages;
            }
            
            if (availableBackgrounds == null || availableBackgrounds.Count == 0)
            {
                return null; // No backgrounds configured
            }
            
            // Return random background from available list
            return availableBackgrounds[UnityEngine.Random.Range(0, availableBackgrounds.Count)];
        }
        
        /// <summary>
        /// Rolls enemy rarity based on floor tier. Higher tiers have higher chance of rare enemies.
        /// </summary>
        public EnemyRarity RollRarityForFloor(int floorNumber)
        {
            int tier = GetTierForFloor(floorNumber);
            return RollRarityForTier(tier);
        }
        
        /// <summary>
        /// Rolls enemy rarity based on tier.
        /// </summary>
        public EnemyRarity RollRarityForTier(int tier)
        {
            int normalWeight, magicWeight, rareWeight, uniqueWeight;
            
            switch (tier)
            {
                case 1:
                    normalWeight = tier1NormalWeight;
                    magicWeight = tier1MagicWeight;
                    rareWeight = tier1RareWeight;
                    uniqueWeight = tier1UniqueWeight;
                    break;
                case 2:
                    normalWeight = tier2NormalWeight;
                    magicWeight = tier2MagicWeight;
                    rareWeight = tier2RareWeight;
                    uniqueWeight = tier2UniqueWeight;
                    break;
                case 3:
                    normalWeight = tier3NormalWeight;
                    magicWeight = tier3MagicWeight;
                    rareWeight = tier3RareWeight;
                    uniqueWeight = tier3UniqueWeight;
                    break;
                case 4:
                    normalWeight = tier4NormalWeight;
                    magicWeight = tier4MagicWeight;
                    rareWeight = tier4RareWeight;
                    uniqueWeight = tier4UniqueWeight;
                    break;
                default:
                    normalWeight = tier1NormalWeight;
                    magicWeight = tier1MagicWeight;
                    rareWeight = tier1RareWeight;
                    uniqueWeight = tier1UniqueWeight;
                    break;
            }
            
            int total = Mathf.Max(1, normalWeight + magicWeight + rareWeight + uniqueWeight);
            int roll = UnityEngine.Random.Range(0, total);
            
            if (roll < normalWeight) return EnemyRarity.Normal;
            roll -= normalWeight;
            if (roll < magicWeight) return EnemyRarity.Magic;
            roll -= magicWeight;
            if (roll < rareWeight) return EnemyRarity.Rare;
            return EnemyRarity.Unique;
        }
    }
}


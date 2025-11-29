using UnityEngine;

namespace Dexiled.MazeSystem
{
    /// <summary>
    /// Configuration for maze difficulty levels.
    /// Used to define different challenge tiers for maze runs.
    /// </summary>
    [CreateAssetMenu(fileName = "Maze Difficulty Config", menuName = "Dexiled/Maze Difficulty Config", order = 2)]
    public class MazeDifficultyConfig : ScriptableObject
    {
        [Header("Difficulty Information")]
        [Tooltip("Display name for this difficulty level")]
        public string difficultyName = "Standard";
        
        [Tooltip("Description of this difficulty level")]
        [TextArea(3, 5)]
        public string description = "Standard maze difficulty";
        
        [Tooltip("Icon/sprite for this difficulty level")]
        public Sprite difficultyIcon;
        
        [Header("Maze Configuration")]
        [Tooltip("MazeConfig to use for this difficulty")]
        public MazeConfig mazeConfig;
        
        [Header("Run Modifiers")]
        [Tooltip("Enemy level scaling multiplier (1.0 = normal, 1.5 = +50% levels)")]
        [Range(0.5f, 3.0f)]
        public float enemyLevelMultiplier = 1.0f;
        
        [Tooltip("Experience multiplier for rewards")]
        [Range(0.5f, 3.0f)]
        public float experienceMultiplier = 1.0f;
        
        [Tooltip("Loot drop rate multiplier")]
        [Range(0.5f, 3.0f)]
        public float lootMultiplier = 1.0f;
        
        [Tooltip("Currency drop multiplier")]
        [Range(0.5f, 3.0f)]
        public float currencyMultiplier = 1.0f;
        
        [Header("Entry Requirements")]
        [Tooltip("Minimum player level required to start this difficulty")]
        public int requiredLevel = 1;
        
        [Tooltip("Currency cost to start a run at this difficulty (0 = free)")]
        public int entryCost = 0;
        
        [Tooltip("Currency type for entry cost")]
        public Dexiled.Data.Items.CurrencyType entryCurrencyType;
        
        [Header("First Clear Bonus")]
        [Tooltip("Attunement points awarded on first clear of this difficulty (minimum 2)")]
        [Range(2, 20)]
        public int firstClearAttunementPoints = 2;
        
        [Tooltip("Maze currency bonus on first clear")]
        public int firstClearCurrencyBonus = 0;
        
        [Tooltip("Currency type for first clear bonus")]
        public Dexiled.Data.Items.CurrencyType firstClearCurrencyType;
        
        [Header("Visual Settings")]
        [Tooltip("Color theme for this difficulty level")]
        public Color difficultyColor = Color.white;
        
        [Tooltip("Recommended difficulty indicator")]
        public bool isRecommended = false;
    }
}



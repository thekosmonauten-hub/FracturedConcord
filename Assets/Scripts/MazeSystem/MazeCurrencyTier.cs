using UnityEngine;

namespace Dexiled.MazeSystem
{
    /// <summary>
    /// Defines the currency tier system for maze difficulties.
    /// Each difficulty tier drops its signature currency, plus lower tier currencies.
    /// </summary>
    [System.Serializable]
    public class MazeCurrencyTier
    {
        [Header("Tier Information")]
        [Tooltip("Tier number (1-4)")]
        public int tierNumber = 1;
        
        [Tooltip("Tier name (e.g., 'Broken Mandate')")]
        public string tierName = "Broken Mandate";
        
        [Header("Primary Currency")]
        [Tooltip("The signature currency for this tier")]
        public Dexiled.Data.Items.CurrencyType primaryCurrency;
        
        [Tooltip("Base drop amount for primary currency per node/combat")]
        [Range(1, 50)]
        public int primaryCurrencyBaseDrop = 5;
        
        [Header("Lower Tier Currency Drops")]
        [Tooltip("Multiplier for lower tier currency drops (0.5 = half amount)")]
        [Range(0.1f, 1.0f)]
        public float lowerTierMultiplier = 0.5f;
        
        [Header("Visual")]
        [Tooltip("Color theme for this tier")]
        public Color tierColor = Color.white;
        
        [Tooltip("Icon/sprite for this tier")]
        public Sprite tierIcon;
    }
    
    /// <summary>
    /// ScriptableObject for maze currency tier configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "Maze Currency Tier Config", menuName = "Dexiled/Maze Currency Tier Config", order = 3)]
    public class MazeCurrencyTierConfig : ScriptableObject
    {
        [Header("Currency Tiers")]
        [Tooltip("Tier 1: Broken Mandate")]
        public MazeCurrencyTier tier1;
        
        [Tooltip("Tier 2: Shattered Law")]
        public MazeCurrencyTier tier2;
        
        [Tooltip("Tier 3: Grand Contradiction")]
        public MazeCurrencyTier tier3;
        
        [Tooltip("Tier 4: Absolute Collapse")]
        public MazeCurrencyTier tier4;
        
        /// <summary>
        /// Gets the currency tier configuration for a given tier number.
        /// </summary>
        public MazeCurrencyTier GetTier(int tierNumber)
        {
            switch (tierNumber)
            {
                case 1: return tier1;
                case 2: return tier2;
                case 3: return tier3;
                case 4: return tier4;
                default: return tier1;
            }
        }
        
        /// <summary>
        /// Gets the primary currency for a given tier.
        /// </summary>
        public Dexiled.Data.Items.CurrencyType GetPrimaryCurrency(int tierNumber)
        {
            MazeCurrencyTier tier = GetTier(tierNumber);
            return tier != null ? tier.primaryCurrency : Dexiled.Data.Items.CurrencyType.MandateFragment;
        }
    }
}


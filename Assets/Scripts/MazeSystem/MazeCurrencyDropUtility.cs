using UnityEngine;
using System.Collections.Generic;
using Dexiled.Data.Items;

namespace Dexiled.MazeSystem
{
    /// <summary>
    /// Utility class for handling maze currency drops based on difficulty tier.
    /// </summary>
    public static class MazeCurrencyDropUtility
    {
        /// <summary>
        /// Gets the currency tier (1-4) based on difficulty name or floor number.
        /// </summary>
        public static int GetCurrencyTierFromDifficulty(string difficultyName)
        {
            if (string.IsNullOrEmpty(difficultyName))
                return 1;
            
            // Map difficulty names to tiers
            // Adjust these based on your actual difficulty naming convention
            string lowerName = difficultyName.ToLower();
            
            if (lowerName.Contains("broken mandate") || lowerName.Contains("tier 1") || lowerName.Contains("standard"))
                return 1;
            else if (lowerName.Contains("shattered law") || lowerName.Contains("tier 2") || lowerName.Contains("veteran"))
                return 2;
            else if (lowerName.Contains("grand contradiction") || lowerName.Contains("tier 3") || lowerName.Contains("elite"))
                return 3;
            else if (lowerName.Contains("absolute collapse") || lowerName.Contains("tier 4") || lowerName.Contains("master"))
                return 4;
            
            // Default to tier 1
            return 1;
        }
        
        /// <summary>
        /// Gets the currency tier based on floor number (assuming 2 floors per tier).
        /// </summary>
        public static int GetCurrencyTierFromFloor(int floorNumber, int floorsPerTier = 2)
        {
            // Tier 1: Floors 1-2
            // Tier 2: Floors 3-4
            // Tier 3: Floors 5-6
            // Tier 4: Floors 7-8
            return Mathf.Clamp((floorNumber - 1) / floorsPerTier + 1, 1, 4);
        }
        
        /// <summary>
        /// Generates currency drops for a completed node/combat based on tier.
        /// </summary>
        public static Dictionary<CurrencyType, int> GenerateCurrencyDrops(int tier, MazeCurrencyTierConfig tierConfig, float multiplier = 1.0f)
        {
            Dictionary<CurrencyType, int> drops = new Dictionary<CurrencyType, int>();
            
            if (tierConfig == null)
            {
                Debug.LogWarning("[MazeCurrencyDropUtility] Tier config is null! Using default drops.");
                // Fallback: use default currency types
                CurrencyType defaultCurrency = GetDefaultCurrencyForTier(tier);
                drops[defaultCurrency] = Mathf.RoundToInt(5 * multiplier);
                return drops;
            }
            
            MazeCurrencyTier currentTier = tierConfig.GetTier(tier);
            if (currentTier == null)
            {
                Debug.LogWarning($"[MazeCurrencyDropUtility] Tier {tier} not found in config! Using default.");
                CurrencyType defaultCurrency = GetDefaultCurrencyForTier(tier);
                drops[defaultCurrency] = Mathf.RoundToInt(5 * multiplier);
                return drops;
            }
            
            // Add primary currency for this tier
            int primaryAmount = Mathf.RoundToInt(currentTier.primaryCurrencyBaseDrop * multiplier);
            if (primaryAmount > 0)
            {
                drops[currentTier.primaryCurrency] = primaryAmount;
            }
            
            // Add lower tier currencies (if not tier 1)
            if (tier > 1)
            {
                for (int lowerTier = 1; lowerTier < tier; lowerTier++)
                {
                    MazeCurrencyTier lowerTierData = tierConfig.GetTier(lowerTier);
                    if (lowerTierData != null)
                    {
                        int lowerAmount = Mathf.RoundToInt(lowerTierData.primaryCurrencyBaseDrop * currentTier.lowerTierMultiplier * multiplier);
                        if (lowerAmount > 0)
                        {
                            // Add to existing amount if already in dictionary
                            if (drops.ContainsKey(lowerTierData.primaryCurrency))
                            {
                                drops[lowerTierData.primaryCurrency] += lowerAmount;
                            }
                            else
                            {
                                drops[lowerTierData.primaryCurrency] = lowerAmount;
                            }
                        }
                    }
                }
            }
            
            return drops;
        }
        
        /// <summary>
        /// Gets the default currency type for a tier (fallback).
        /// </summary>
        private static CurrencyType GetDefaultCurrencyForTier(int tier)
        {
            switch (tier)
            {
                case 1: return CurrencyType.MandateFragment;
                case 2: return CurrencyType.ShatteredSigil;
                case 3: return CurrencyType.ContradictionCore;
                case 4: return CurrencyType.CollapseMotif;
                default: return CurrencyType.MandateFragment;
            }
        }
        
        /// <summary>
        /// Applies currency drops to the MazeCurrencyManager.
        /// </summary>
        public static void ApplyCurrencyDrops(Dictionary<CurrencyType, int> drops)
        {
            if (drops == null || drops.Count == 0)
                return;
            
            MazeCurrencyManager currencyManager = MazeCurrencyManager.Instance;
            if (currencyManager == null)
            {
                Debug.LogWarning("[MazeCurrencyDropUtility] MazeCurrencyManager not found! Cannot apply currency drops.");
                return;
            }
            
            foreach (var drop in drops)
            {
                currencyManager.AddCurrency(drop.Key, drop.Value);
            }
        }
    }
}


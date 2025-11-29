using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Dexiled.Data.Items;

/// <summary>
/// Handles card generation for the Seer using orbs and spirits.
/// </summary>
public static class SeerCardGenerator
{
    // Constants
    private const float ORB_CHANCE_PER_UNIT = 0.05f; // 5% per orb
    private const float SPIRIT_CHANCE_PER_UNIT = 0.05f; // 5% per spirit
    private const int MAX_ORBS = 20; // Max orbs for 100% chance
    private const int MAX_SPIRITS = 20; // Max spirits for 100% chance
    
    // Rarity weights for random generation (when no orbs used)
    private static readonly Dictionary<CardRarity, int> RARITY_WEIGHTS = new Dictionary<CardRarity, int>
    {
        { CardRarity.Common, 100 },
        { CardRarity.Magic, 25 },
        { CardRarity.Rare, 5 }
    };
    
    /// <summary>
    /// Maps CurrencyType spirits to CardElement
    /// </summary>
    private static Dictionary<CurrencyType, CardElement> SpiritToElementMap = new Dictionary<CurrencyType, CardElement>
    {
        { CurrencyType.FireSpirit, CardElement.Fire },
        { CurrencyType.ColdSpirit, CardElement.Cold },
        { CurrencyType.LightningSpirit, CardElement.Lightning },
        { CurrencyType.PhysicalSpirit, CardElement.Physical },
        { CurrencyType.ChaosSpirit, CardElement.Chaos },
        { CurrencyType.LifeSpirit, CardElement.Basic }, // Life maps to Basic
        { CurrencyType.DefenseSpirit, CardElement.Basic }, // Defense maps to Basic
        { CurrencyType.CritSpirit, CardElement.Basic }, // Crit maps to Basic
        { CurrencyType.DivineSpirit, CardElement.Basic } // Divine maps to Basic
    };
    
    /// <summary>
    /// Generate a card based on ingredient selection.
    /// </summary>
    /// <param name="infusionOrbs">Number of Orb of Infusion (Magic chance)</param>
    /// <param name="perfectionOrbs">Number of Orb of Perfection (Rare chance, requires 100% Magic)</param>
    /// <param name="spiritCounts">Dictionary mapping spirit types to their counts</param>
    /// <returns>Generated CardData, or null if generation failed</returns>
    public static CardData GenerateCard(
        int infusionOrbs,
        int perfectionOrbs,
        Dictionary<CurrencyType, int> spiritCounts)
    {
        // Validate: Perfection orbs require 100% Magic chance
        if (perfectionOrbs > 0 && infusionOrbs < MAX_ORBS)
        {
            Debug.LogWarning($"[SeerCardGenerator] Cannot use Perfection orbs without 100% Magic chance (need {MAX_ORBS} Infusion orbs).");
            return null;
        }
        
        // Determine rarity
        CardRarity rarity = DetermineRarity(infusionOrbs, perfectionOrbs);
        
        // Determine element
        CardElement element = DetermineElement(spiritCounts);
        
        // Get card from database
        CardDatabase db = CardDatabase.Instance;
        if (db == null)
        {
            Debug.LogError("[SeerCardGenerator] CardDatabase.Instance is null!");
            return null;
        }
        
        // Get all cards matching rarity and element
        List<CardData> matchingCards = db.GetCardsWithFilters(
            category: null, // All categories
            element: element,
            rarity: rarity
        );
        
        // If no cards match, try without element filter
        if (matchingCards.Count == 0)
        {
            Debug.LogWarning($"[SeerCardGenerator] No cards found for {rarity} {element}. Trying any {rarity} card...");
            matchingCards = db.GetCardsByRarity(rarity);
        }
        
        // If still no cards, try any card
        if (matchingCards.Count == 0)
        {
            Debug.LogWarning($"[SeerCardGenerator] No {rarity} cards found. Generating random card...");
            matchingCards = db.allCards;
        }
        
        if (matchingCards.Count == 0)
        {
            Debug.LogError("[SeerCardGenerator] No cards available in database!");
            return null;
        }
        
        // Select random card from matching cards
        CardData generatedCard = matchingCards[Random.Range(0, matchingCards.Count)];
        
        Debug.Log($"[SeerCardGenerator] Generated: {generatedCard.cardName} ({rarity} {element})");
        return generatedCard;
    }
    
    /// <summary>
    /// Determine card rarity based on orb counts.
    /// </summary>
    private static CardRarity DetermineRarity(int infusionOrbs, int perfectionOrbs)
    {
        // Check for guaranteed Rare (100% Perfection + 100% Magic)
        if (perfectionOrbs >= MAX_ORBS && infusionOrbs >= MAX_ORBS)
        {
            return CardRarity.Rare;
        }
        
        // Check for guaranteed Magic (100% Infusion)
        if (infusionOrbs >= MAX_ORBS)
        {
            return CardRarity.Magic;
        }
        
        // Calculate chances for Magic and Rare
        float magicChance = Mathf.Clamp01(infusionOrbs * ORB_CHANCE_PER_UNIT);
        float rareChance = Mathf.Clamp01(perfectionOrbs * ORB_CHANCE_PER_UNIT);
        
        bool hasOrbs = infusionOrbs > 0 || perfectionOrbs > 0;
        
        // Roll for Rare first (if applicable)
        if (rareChance > 0f && Random.value < rareChance)
        {
            return CardRarity.Rare;
        }
        
        // Roll for Magic
        if (magicChance > 0f && Random.value < magicChance)
        {
            return CardRarity.Magic;
        }
        
        // If orbs were used but didn't roll, generate Common
        // Otherwise, use weighted random (for no orbs case)
        if (hasOrbs)
        {
            return CardRarity.Common;
        }
        
        // No orbs: use weighted random
        return RollWeightedRarity();
    }
    
    /// <summary>
    /// Roll rarity using weights: Common 100, Magic 25, Rare 5
    /// </summary>
    private static CardRarity RollWeightedRarity()
    {
        int totalWeight = RARITY_WEIGHTS.Values.Sum();
        int roll = Random.Range(0, totalWeight);
        
        int currentWeight = 0;
        foreach (var kvp in RARITY_WEIGHTS.OrderByDescending(x => x.Value)) // Check Rare first, then Magic, then Common
        {
            currentWeight += kvp.Value;
            if (roll < currentWeight)
            {
                return kvp.Key;
            }
        }
        
        // Fallback to Common
        return CardRarity.Common;
    }
    
    /// <summary>
    /// Determine card element based on spirit counts.
    /// </summary>
    private static CardElement DetermineElement(Dictionary<CurrencyType, int> spiritCounts)
    {
        if (spiritCounts == null || spiritCounts.Count == 0)
        {
            // No spirits: random element
            return GetRandomElement();
        }
        
        // Find the spirit type with the highest count
        var maxSpirit = spiritCounts.OrderByDescending(x => x.Value).First();
        
        if (maxSpirit.Value >= MAX_SPIRITS)
        {
            // 100% chance: guaranteed element
            if (SpiritToElementMap.TryGetValue(maxSpirit.Key, out CardElement element))
            {
                return element;
            }
        }
        
        // Calculate chance for each element
        Dictionary<CardElement, float> elementChances = new Dictionary<CardElement, float>();
        float totalChance = 0f;
        
        foreach (var kvp in spiritCounts)
        {
            if (SpiritToElementMap.TryGetValue(kvp.Key, out CardElement element))
            {
                float chance = Mathf.Clamp01(kvp.Value * SPIRIT_CHANCE_PER_UNIT);
                if (elementChances.ContainsKey(element))
                {
                    elementChances[element] += chance;
                }
                else
                {
                    elementChances[element] = chance;
                }
                totalChance += chance;
            }
        }
        
        // If total chance >= 1.0, use weighted selection
        if (totalChance >= 1.0f)
        {
            float roll = Random.Range(0f, totalChance);
            float current = 0f;
            foreach (var kvp in elementChances.OrderByDescending(x => x.Value))
            {
                current += kvp.Value;
                if (roll < current)
                {
                    return kvp.Key;
                }
            }
        }
        else if (totalChance > 0f)
        {
            // Partial chance: roll for spirit element, otherwise random
            float roll = Random.Range(0f, 1f);
            if (roll < totalChance)
            {
                // Use weighted selection from spirits
                float spiritRoll = Random.Range(0f, totalChance);
                float current = 0f;
                foreach (var kvp in elementChances.OrderByDescending(x => x.Value))
                {
                    current += kvp.Value;
                    if (spiritRoll < current)
                    {
                        return kvp.Key;
                    }
                }
            }
        }
        
        // No spirits or roll failed: random element
        return GetRandomElement();
    }
    
    /// <summary>
    /// Get a random card element.
    /// </summary>
    private static CardElement GetRandomElement()
    {
        CardElement[] elements = System.Enum.GetValues(typeof(CardElement)) as CardElement[];
        return elements[Random.Range(0, elements.Length)];
    }
    
    /// <summary>
    /// Calculate the chance percentage for a given orb/spirit count.
    /// </summary>
    public static float GetChancePercentage(int count, bool isSpirit = false)
    {
        float chancePerUnit = isSpirit ? SPIRIT_CHANCE_PER_UNIT : ORB_CHANCE_PER_UNIT;
        return Mathf.Clamp01(count * chancePerUnit) * 100f;
    }
    
    /// <summary>
    /// Get the maximum count needed for 100% chance.
    /// </summary>
    public static int GetMaxCountForGuaranteed()
    {
        return MAX_ORBS;
    }
}


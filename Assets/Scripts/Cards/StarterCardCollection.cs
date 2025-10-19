using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Defines starter card collections for each character class.
/// These cards are unlocked immediately when creating a new character.
/// </summary>
public static class StarterCardCollection
{
    /// <summary>
    /// Get starter card names for a specific class.
    /// </summary>
    public static List<string> GetStarterCards(string characterClass)
    {
        switch (characterClass.ToLower())
        {
            case "marauder":
                return GetMarauderStarters();
            
            case "ranger":
                return GetRangerStarters();
            
            case "witch":
                return GetWitchStarters();
            
            case "brawler":
                return GetBrawlerStarters();
            
            case "thief":
                return GetThiefStarters();
            
            case "apostle":
                return GetApostleStarters();
            
            default:
                Debug.LogWarning($"Unknown class: {characterClass}. Using basic starter cards.");
                return GetBasicStarters();
        }
    }
    
    private static List<string> GetMarauderStarters()
    {
        return new List<string>
        {
            // Attack Cards (6)
            "Heavy Strike",
            "Cleave",
            "Ground Slam",
            "Ground Slam",
            "Molten Strike",
            "Sunder",
            
            // Guard Cards (4)
            "Steel Shield",
            "Steel Shield",
            "Enduring Guard",
            "Fortify",
            
            // Skill Cards (2)
            "Rallying Cry",
            "War Cry",
            
            // Total: 12 cards (minimum deck size is 20, so they need to build up)
        };
    }
    
    private static List<string> GetRangerStarters()
    {
        return new List<string>
        {
            "Split Arrow",
            "Lightning Arrow",
            "Ice Shot",
            "Puncture",
            "Barrage",
            "Blink Arrow",
            "Evasive Roll",
            "Evasive Roll",
            "Quick Step",
            "Bear Trap",
            "Precision Stance",
            "Hunter's Mark",
        };
    }
    
    private static List<string> GetWitchStarters()
    {
        return new List<string>
        {
            "Fireball",
            "Fireball",
            "Frost Bolt",
            "Frost Bolt",
            "Spark",
            "Arc",
            "Flame Shield",
            "Ice Barrier",
            "Arcane Cloak",
            "Mana Surge",
            "Elemental Focus",
            "Spell Echo",
        };
    }
    
    private static List<string> GetBrawlerStarters()
    {
        return new List<string>
        {
            "Jab",
            "Jab",
            "Cross Punch",
            "Uppercut",
            "Hook",
            "Body Blow",
            "Iron Skin",
            "Counter Stance",
            "Parry",
            "Adrenaline Rush",
            "Combat Focus",
            "Relentless",
        };
    }
    
    private static List<string> GetThiefStarters()
    {
        return new List<string>
        {
            "Backstab",
            "Poison Dagger",
            "Quick Slash",
            "Quick Slash",
            "Whirling Blades",
            "Shadow Strike",
            "Dodge",
            "Smoke Bomb",
            "Vanish",
            "Preparation",
            "Opportunist",
            "Cunning",
        };
    }
    
    private static List<string> GetApostleStarters()
    {
        return new List<string>
        {
            "Holy Smite",
            "Holy Smite",
            "Divine Shield",
            "Divine Shield",
            "Heal",
            "Heal",
            "Blessing",
            "Blessing",
            "Purify",
            "Holy Light",
            "Prayer",
            "Faith",
        };
    }
    
    private static List<string> GetBasicStarters()
    {
        // Generic starter set for testing or unknown classes
        return new List<string>
        {
            "Strike",
            "Strike",
            "Strike",
            "Guard",
            "Guard",
            "Guard",
            "Draw Card",
            "Draw Card",
            "Heal",
            "Heal",
            "Power Up",
            "Power Up",
        };
    }
    
    /// <summary>
    /// Check if all starter cards exist in the database.
    /// Use this to validate your card setup.
    /// </summary>
    public static bool ValidateStarterCards(string characterClass, CardDatabase database)
    {
        List<string> starterCards = GetStarterCards(characterClass);
        List<string> missingCards = new List<string>();
        
        foreach (string cardName in starterCards)
        {
            bool found = database.allCards.Exists(card => card.cardName == cardName);
            if (!found)
            {
                missingCards.Add(cardName);
            }
        }
        
        if (missingCards.Count > 0)
        {
            Debug.LogError($"Missing starter cards for {characterClass}: {string.Join(", ", missingCards)}");
            return false;
        }
        
        Debug.Log($"All starter cards validated for {characterClass}");
        return true;
    }
}









using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Loads starter decks from JSON files and converts them to Card objects.
/// Integrates with the combat system to provide class-specific decks.
/// 
/// Usage:
/// List<Card> deck = DeckLoader.LoadStarterDeck("Marauder");
/// </summary>
public static class DeckLoader
{
    private const string DECK_PATH = "Cards/starter_deck_";
    
    /// <summary>
    /// Load starter deck for a specific character class
    /// </summary>
    public static List<Card> LoadStarterDeck(string characterClass)
    {
        // Normalize class name to lowercase
        string className = characterClass.ToLower();
        
        // Load JSON file
        string resourcePath = $"{DECK_PATH}{className}";
        TextAsset jsonFile = Resources.Load<TextAsset>(resourcePath);
        
        if (jsonFile == null)
        {
            Debug.LogError($"Could not find starter deck JSON for {characterClass} at Resources/{resourcePath}");
            return CreateFallbackDeck();
        }
        
        // Parse JSON
        try
        {
            JSONDeckFormat deckData = JsonUtility.FromJson<JSONDeckFormat>(jsonFile.text);
            List<Card> deck = ParseDeckFromJSON(deckData, jsonFile.text);
            
            Debug.Log($"<color=green>Loaded {characterClass} starter deck:</color> {deckData.deckName} ({deck.Count} cards)");
            return deck;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse deck JSON for {characterClass}: {e.Message}");
            return CreateFallbackDeck();
        }
    }
    
    /// <summary>
    /// Load all available starter decks
    /// </summary>
    public static Dictionary<string, List<Card>> LoadAllStarterDecks()
    {
        Dictionary<string, List<Card>> allDecks = new Dictionary<string, List<Card>>();
        
        string[] classes = { "apostle", "brawler", "marauder", "ranger", "thief", "witch" };
        
        foreach (string className in classes)
        {
            List<Card> deck = LoadStarterDeck(className);
            if (deck != null && deck.Count > 0)
            {
                allDecks[className] = deck;
            }
        }
        
        Debug.Log($"Loaded {allDecks.Count} starter decks");
        return allDecks;
    }
    
    /// <summary>
    /// Parse deck data from JSON format
    /// </summary>
    private static List<Card> ParseDeckFromJSON(JSONDeckFormat deckData, string rawJsonData = null)
    {
        List<Card> deck = new List<Card>();
        
        foreach (JSONCardEntry cardEntry in deckData.cards)
        {
            // Create card from JSON data
            Card card = ConvertJSONToCard(cardEntry.data, rawJsonData);
            
            // Add multiple copies based on count
            for (int i = 0; i < cardEntry.count; i++)
            {
                deck.Add(card);
            }
        }
        
        return deck;
    }
    
    /// <summary>
    /// Convert JSON card format to Card object
    /// </summary>
    private static Card ConvertJSONToCard(JSONCardFormat jsonCard, string rawJsonData = null)
    {
        // Debug logging to trace card art loading
        Debug.Log($"<color=yellow>[CardArt] ConvertJSONToCard: Processing '{jsonCard.cardName}'</color>");
        Debug.Log($"<color=yellow>[CardArt]   - JSON cardArtName: '{jsonCard.cardArtName}'</color>");
        
        // Try manual parsing if Unity JsonUtility failed
        string manualCardArtName = ExtractCardArtNameManually(jsonCard.cardName, rawJsonData);
        if (!string.IsNullOrEmpty(manualCardArtName) && string.IsNullOrEmpty(jsonCard.cardArtName))
        {
            Debug.Log($"<color=orange>[CardArt] Manual parsing found cardArtName: '{manualCardArtName}'</color>");
            jsonCard.cardArtName = manualCardArtName;
        }
        
        Card card = new Card
        {
            cardName = jsonCard.cardName,
            description = jsonCard.description,
            cardType = ParseCardType(jsonCard.cardType),
            manaCost = jsonCard.manaCost,
            baseDamage = jsonCard.baseDamage,
            baseGuard = jsonCard.baseGuard,
            primaryDamageType = ParseDamageType(jsonCard.primaryDamageType),
            
            // Load card art from Resources if specified
            cardArtName = jsonCard.cardArtName,
            cardArt = LoadCardArt(jsonCard.cardArtName),
            
            // Weapon scaling
            scalesWithMeleeWeapon = jsonCard.weaponScaling.scalesWithMeleeWeapon,
            scalesWithProjectileWeapon = jsonCard.weaponScaling.scalesWithProjectileWeapon,
            scalesWithSpellWeapon = jsonCard.weaponScaling.scalesWithSpellWeapon,
            
            // AoE
            isAoE = jsonCard.aoe.isAoE,
            aoeTargets = jsonCard.aoe.aoeTargets,
            
            // Requirements
            requirements = new CardRequirements
            {
                requiredStrength = jsonCard.requirements.requiredStrength,
                requiredDexterity = jsonCard.requirements.requiredDexterity,
                requiredIntelligence = jsonCard.requirements.requiredIntelligence,
                requiredLevel = jsonCard.requirements.requiredLevel,
                requiredWeaponTypes = ParseWeaponTypes(jsonCard.requirements.requiredWeaponTypes)
            },
            
            // Tags
            tags = new List<string>(jsonCard.tags),
            
            // Additional damage types
            additionalDamageTypes = ParseDamageTypes(jsonCard.additionalDamageTypes),
            
            // Scaling
            damageScaling = new AttributeScaling
            {
                strengthScaling = jsonCard.damageScaling.strengthScaling,
                dexterityScaling = jsonCard.damageScaling.dexterityScaling,
                intelligenceScaling = jsonCard.damageScaling.intelligenceScaling
            },
            
            guardScaling = new AttributeScaling
            {
                strengthScaling = jsonCard.guardScaling.strengthScaling,
                dexterityScaling = jsonCard.guardScaling.dexterityScaling,
                intelligenceScaling = jsonCard.guardScaling.intelligenceScaling
            },
            
            // Effects
            effects = ParseEffects(jsonCard.effects)
        };
        
        return card;
    }
    
    // Parsing helpers
    private static CardType ParseCardType(string type)
    {
        return System.Enum.TryParse<CardType>(type, true, out var result) ? result : CardType.Attack;
    }
    
    private static DamageType ParseDamageType(string type)
    {
        return System.Enum.TryParse<DamageType>(type, true, out var result) ? result : DamageType.Physical;
    }
    
    private static List<WeaponType> ParseWeaponTypes(string[] types)
    {
        List<WeaponType> weaponTypes = new List<WeaponType>();
        foreach (string type in types)
        {
            if (System.Enum.TryParse<WeaponType>(type, true, out var result))
            {
                weaponTypes.Add(result);
            }
        }
        return weaponTypes;
    }
    
    private static List<DamageType> ParseDamageTypes(string[] types)
    {
        List<DamageType> damageTypes = new List<DamageType>();
        foreach (string type in types)
        {
            if (System.Enum.TryParse<DamageType>(type, true, out var result))
            {
                damageTypes.Add(result);
            }
        }
        return damageTypes;
    }
    
    private static List<CardEffect> ParseEffects(JSONEffect[] jsonEffects)
    {
        List<CardEffect> effects = new List<CardEffect>();
        
        foreach (JSONEffect jsonEffect in jsonEffects)
        {
            CardEffect effect = new CardEffect
            {
                effectType = ParseEffectType(jsonEffect.effectType),
                effectName = jsonEffect.effectName,
                description = jsonEffect.description,
                value = jsonEffect.value,
                duration = jsonEffect.duration,
                damageType = ParseDamageType(jsonEffect.damageType),
                targetsSelf = jsonEffect.targetsSelf,
                targetsEnemy = jsonEffect.targetsEnemy,
                targetsAllEnemies = jsonEffect.targetsAllEnemies,
                targetsAll = jsonEffect.targetsAll,
                condition = jsonEffect.condition
            };
            
            effects.Add(effect);
        }
        
        return effects;
    }
    
    private static EffectType ParseEffectType(string type)
    {
        return System.Enum.TryParse<EffectType>(type, true, out var result) ? result : EffectType.Damage;
    }
    
    /// <summary>
    /// Load card art sprite from Resources folder
    /// </summary>
    private static Sprite LoadCardArt(string artName)
    {
        // Debug logging to trace the issue
        Debug.Log($"<color=orange>[CardArt] LoadCardArt called with artName: '{artName}'</color>");
        
        // Return null if no art name specified
        if (string.IsNullOrEmpty(artName))
        {
            Debug.Log($"<color=red>[CardArt] No card art specified for card</color>");
            return null;
        }
        
        // Try to load sprite from Resources folder
        Sprite sprite = Resources.Load<Sprite>(artName);
        
        if (sprite == null)
        {
            Debug.LogWarning($"[CardArt] Failed to load card art: {artName}. Make sure the sprite exists at Resources/{artName}.png (or other format)");
            return null;
        }
        
        Debug.Log($"<color=green>[CardArt] Loaded card art: {artName}</color>");
        return sprite;
    }
    
    /// <summary>
    /// Manually extract cardArtName from raw JSON if Unity JsonUtility fails
    /// </summary>
    private static string ExtractCardArtNameManually(string cardName, string rawJsonData)
    {
        if (string.IsNullOrEmpty(rawJsonData)) return null;
        
        try
        {
            // Find the card section in the JSON
            string searchPattern = $"\"cardName\": \"{cardName}\"";
            int cardStart = rawJsonData.IndexOf(searchPattern);
            if (cardStart == -1) return null;
            
            // Look for cardArtName in the next 1000 characters after the card name
            string cardSection = rawJsonData.Substring(cardStart, System.Math.Min(1000, rawJsonData.Length - cardStart));
            
            // Find cardArtName pattern
            int artNameStart = cardSection.IndexOf("\"cardArtName\":");
            if (artNameStart == -1) return null;
            
            // Extract the value
            int valueStart = cardSection.IndexOf("\"", artNameStart + 14) + 1;
            int valueEnd = cardSection.IndexOf("\"", valueStart);
            
            if (valueStart > 0 && valueEnd > valueStart)
            {
                string extractedValue = cardSection.Substring(valueStart, valueEnd - valueStart);
                Debug.Log($"<color=cyan>[CardArt] Manual extraction found: '{extractedValue}'</color>");
                return extractedValue;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CardArt] Manual parsing failed: {e.Message}");
        }
        
        return null;
    }
    
    /// <summary>
    /// Create a basic fallback deck if JSON loading fails
    /// </summary>
    private static List<Card> CreateFallbackDeck()
    {
        List<Card> deck = new List<Card>();
        
        // Create 10 basic strike cards
        for (int i = 0; i < 10; i++)
        {
            deck.Add(new Card
            {
                cardName = "Basic Strike",
                description = "Deal 6 physical damage",
                cardType = CardType.Attack,
                manaCost = 1,
                baseDamage = 6f,
                primaryDamageType = DamageType.Physical
            });
        }
        
        // Create 5 basic guard cards
        for (int i = 0; i < 5; i++)
        {
            deck.Add(new Card
            {
                cardName = "Basic Guard",
                description = "Gain 5 Guard",
                cardType = CardType.Guard,
                manaCost = 1,
                baseGuard = 5f
            });
        }
        
        Debug.LogWarning("Using fallback deck (15 basic cards)");
        return deck;
    }
}


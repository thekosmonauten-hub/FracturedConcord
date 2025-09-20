using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "CardDatabase", menuName = "Dexiled/Cards/Card Database")]
public class CardDatabase : ScriptableObject
{
    [Header("Card Collections")]
    public List<CardData> allCards = new List<CardData>();
    
    [Header("Card Categories")]
    public List<CardData> attackCards = new List<CardData>();
    public List<CardData> skillCards = new List<CardData>();
    public List<CardData> powerCards = new List<CardData>();
    public List<CardData> guardCards = new List<CardData>();
    
    [Header("Card Elements")]
    public List<CardData> basicCards = new List<CardData>();
    public List<CardData> fireCards = new List<CardData>();
    public List<CardData> coldCards = new List<CardData>();
    public List<CardData> lightningCards = new List<CardData>();
    public List<CardData> physicalCards = new List<CardData>();
    public List<CardData> chaosCards = new List<CardData>();
    
    [Header("Card Rarities")]
    public List<CardData> commonCards = new List<CardData>();
    public List<CardData> magicCards = new List<CardData>();
    public List<CardData> rareCards = new List<CardData>();
    public List<CardData> uniqueCards = new List<CardData>();
    
    // Singleton pattern
    private static CardDatabase _instance;
    public static CardDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<CardDatabase>("CardDatabase");
                if (_instance == null)
                {
                    Debug.LogError("CardDatabase not found in Resources folder!");
                }
            }
            return _instance;
        }
    }
    
    private void OnValidate()
    {
        // Auto-categorize cards when the database is modified
        CategorizeCards();
    }
    
    public void CategorizeCards()
    {
        // Clear all categories
        attackCards.Clear();
        skillCards.Clear();
        powerCards.Clear();
        guardCards.Clear();
        
        basicCards.Clear();
        fireCards.Clear();
        coldCards.Clear();
        lightningCards.Clear();
        physicalCards.Clear();
        chaosCards.Clear();
        
        commonCards.Clear();
        magicCards.Clear();
        rareCards.Clear();
        uniqueCards.Clear();
        
        // Categorize by card type
        foreach (var card in allCards)
        {
            if (card == null) continue;
            
            // Categorize by category
            switch (card.category)
            {
                case CardCategory.Attack:
                    attackCards.Add(card);
                    break;
                case CardCategory.Skill:
                    skillCards.Add(card);
                    break;
                case CardCategory.Power:
                    powerCards.Add(card);
                    break;
                case CardCategory.Guard:
                    guardCards.Add(card);
                    break;
            }
            
            // Categorize by element
            switch (card.element)
            {
                case CardElement.Basic:
                    basicCards.Add(card);
                    break;
                case CardElement.Fire:
                    fireCards.Add(card);
                    break;
                case CardElement.Cold:
                    coldCards.Add(card);
                    break;
                case CardElement.Lightning:
                    lightningCards.Add(card);
                    break;
                case CardElement.Physical:
                    physicalCards.Add(card);
                    break;
                case CardElement.Chaos:
                    chaosCards.Add(card);
                    break;
            }
            
            // Categorize by rarity
            switch (card.rarity)
            {
                case CardRarity.Common:
                    commonCards.Add(card);
                    break;
                case CardRarity.Magic:
                    magicCards.Add(card);
                    break;
                case CardRarity.Rare:
                    rareCards.Add(card);
                    break;
                case CardRarity.Unique:
                    uniqueCards.Add(card);
                    break;
            }
        }
    }
    
    // Get random card methods
    public CardData GetRandomCard()
    {
        if (allCards.Count == 0) return null;
        return allCards[Random.Range(0, allCards.Count)];
    }
    
    public CardData GetRandomCardByCategory(CardCategory category)
    {
        List<CardData> categoryCards = GetCardsByCategory(category);
        if (categoryCards.Count == 0) return null;
        return categoryCards[Random.Range(0, categoryCards.Count)];
    }
    
    public CardData GetRandomCardByElement(CardElement element)
    {
        List<CardData> elementCards = GetCardsByElement(element);
        if (elementCards.Count == 0) return null;
        return elementCards[Random.Range(0, elementCards.Count)];
    }
    
    public CardData GetRandomCardByRarity(CardRarity rarity)
    {
        List<CardData> rarityCards = GetCardsByRarity(rarity);
        if (rarityCards.Count == 0) return null;
        return rarityCards[Random.Range(0, rarityCards.Count)];
    }
    
    // Get cards by filter methods
    public List<CardData> GetCardsByCategory(CardCategory category)
    {
        switch (category)
        {
            case CardCategory.Attack: return attackCards;
            case CardCategory.Skill: return skillCards;
            case CardCategory.Power: return powerCards;
            case CardCategory.Guard: return guardCards;
            default: return new List<CardData>();
        }
    }
    
    public List<CardData> GetCardsByElement(CardElement element)
    {
        switch (element)
        {
            case CardElement.Basic: return basicCards;
            case CardElement.Fire: return fireCards;
            case CardElement.Cold: return coldCards;
            case CardElement.Lightning: return lightningCards;
            case CardElement.Physical: return physicalCards;
            case CardElement.Chaos: return chaosCards;
            default: return new List<CardData>();
        }
    }
    
    public List<CardData> GetCardsByRarity(CardRarity rarity)
    {
        switch (rarity)
        {
            case CardRarity.Common: return commonCards;
            case CardRarity.Magic: return magicCards;
            case CardRarity.Rare: return rareCards;
            case CardRarity.Unique: return uniqueCards;
            default: return new List<CardData>();
        }
    }
    
    // Find card by name
    public CardData GetCardByName(string cardName)
    {
        return allCards.FirstOrDefault(card => card.cardName == cardName);
    }
    
    // Get all cards with filters
    public List<CardData> GetCardsWithFilters(CardCategory? category = null, CardElement? element = null, CardRarity? rarity = null)
    {
        var filteredCards = allCards.AsEnumerable();
        
        if (category.HasValue)
            filteredCards = filteredCards.Where(card => card.category == category.Value);
        
        if (element.HasValue)
            filteredCards = filteredCards.Where(card => card.element == element.Value);
        
        if (rarity.HasValue)
            filteredCards = filteredCards.Where(card => card.rarity == rarity.Value);
        
        return filteredCards.ToList();
    }
}

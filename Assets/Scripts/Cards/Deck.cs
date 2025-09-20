using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Deck
{
    [Header("Deck Information")]
    public string deckName;
    public string description;
    public string characterClass;
    
    [Header("Cards")]
    public List<Card> cards = new List<Card>();
    
    // Add a card to the deck
    public void AddCard(Card card)
    {
        cards.Add(card);
    }
    
    // Remove a card from the deck
    public void RemoveCard(Card card)
    {
        cards.Remove(card);
    }
    
    // Get all cards of a specific type
    public List<Card> GetCardsByType(CardType cardType)
    {
        return cards.Where(card => card.cardType == cardType).ToList();
    }
    
    // Get all attack cards
    public List<Card> GetAttackCards()
    {
        return GetCardsByType(CardType.Attack);
    }
    
    // Get all guard cards
    public List<Card> GetGuardCards()
    {
        return GetCardsByType(CardType.Guard);
    }
    
    // Get all skill cards
    public List<Card> GetSkillCards()
    {
        return GetCardsByType(CardType.Skill);
    }
    
    // Get all power cards
    public List<Card> GetPowerCards()
    {
        return GetCardsByType(CardType.Power);
    }
    
    // Get all aura cards
    public List<Card> GetAuraCards()
    {
        return GetCardsByType(CardType.Aura);
    }
    
    // Get cards that a character can use
    public List<Card> GetUsableCards(Character character)
    {
        return cards.Where(card => card.CanUseCard(character)).ToList();
    }
    
    // Get deck statistics
    public DeckStatistics GetDeckStatistics()
    {
        return new DeckStatistics
        {
            totalCards = cards.Count,
            attackCards = GetAttackCards().Count,
            guardCards = GetGuardCards().Count,
            skillCards = GetSkillCards().Count,
            powerCards = GetPowerCards().Count,
            auraCards = GetAuraCards().Count,
            averageManaCost = cards.Count > 0 ? (float)cards.Average(card => card.manaCost) : 0f,
            totalManaCost = cards.Sum(card => card.manaCost)
        };
    }
    
    // Shuffle the deck
    public void Shuffle()
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Card temp = cards[i];
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
    }
    
    // Draw a card from the deck
    public Card DrawCard()
    {
        if (cards.Count > 0)
        {
            Card drawnCard = cards[0];
            cards.RemoveAt(0);
            return drawnCard;
        }
        return null;
    }
    
    // Get a copy of the deck (for gameplay)
    public Deck GetCopy()
    {
        Deck copy = new Deck
        {
            deckName = this.deckName,
            description = this.description,
            characterClass = this.characterClass
        };
        
        foreach (Card card in cards)
        {
            copy.AddCard(card);
        }
        
        return copy;
    }
}

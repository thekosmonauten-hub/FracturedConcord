using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ThiefStarterDeck", menuName = "Cards/Starter Decks/Thief")]
public class ThiefStarterDeck : ScriptableObject
{
    public Deck CreateThiefStarterDeck()
    {
        Deck deck = new Deck
        {
            deckName = "Thief's Cunning",
            description = "A stealthy deck focused on dexterity and intelligence",
            characterClass = "Thief"
        };
        
        // Add cards here once you share your TypeScript files
        
        return deck;
    }
}

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BrawlerStarterDeck", menuName = "Cards/Starter Decks/Brawler")]
public class BrawlerStarterDeck : ScriptableObject
{
    public Deck CreateBrawlerStarterDeck()
    {
        Deck deck = new Deck
        {
            deckName = "Brawler's Might",
            description = "A balanced deck focused on strength and dexterity",
            characterClass = "Brawler"
        };
        
        // Add cards here once you share your TypeScript files
        
        return deck;
    }
}

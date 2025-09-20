using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ApostleStarterDeck", menuName = "Cards/Starter Decks/Apostle")]
public class ApostleStarterDeck : ScriptableObject
{
    public Deck CreateApostleStarterDeck()
    {
        Deck deck = new Deck
        {
            deckName = "Apostle's Faith",
            description = "A divine deck focused on strength and intelligence",
            characterClass = "Apostle"
        };
        
        // Add cards here once you share your TypeScript files
        
        return deck;
    }
}

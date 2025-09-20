using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RangerStarterDeck", menuName = "Cards/Starter Decks/Ranger")]
public class RangerStarterDeck : ScriptableObject
{
    public Deck CreateRangerStarterDeck()
    {
        Deck deck = new Deck
        {
            deckName = "Ranger's Precision",
            description = "A tactical deck focused on ranged attacks and mobility",
            characterClass = "Ranger"
        };
        
        // Add cards here once you share your TypeScript files
        
        return deck;
    }
}

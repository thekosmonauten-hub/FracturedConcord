using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WitchStarterDeck", menuName = "Cards/Starter Decks/Witch")]
public class WitchStarterDeck : ScriptableObject
{
    public Deck CreateWitchStarterDeck()
    {
        Deck deck = new Deck
        {
            deckName = "Witch's Arcana",
            description = "A mystical deck focused on elemental magic and spellcasting",
            characterClass = "Witch"
        };
        
        // Add cards here once you share your TypeScript files
        
        return deck;
    }
}

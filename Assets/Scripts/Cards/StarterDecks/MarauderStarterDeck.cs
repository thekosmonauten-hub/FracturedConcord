using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MarauderStarterDeck", menuName = "Cards/Starter Decks/Marauder")]
public class MarauderStarterDeck : ScriptableObject
{
    public Deck CreateMarauderStarterDeck()
    {
        Deck deck = new Deck
        {
            deckName = "Marauder's Fury",
            description = "A brutal deck focused on physical damage and close combat",
            characterClass = "Marauder"
        };
        
        // Add all Marauder starter cards
        AddMarauderCards(deck);
        
        return deck;
    }
    
    private void AddMarauderCards(Deck deck)
    {
        // Heavy Strike (6 copies) - Basic Attack Card
        for (int i = 0; i < 6; i++)
        {
            Card heavyStrike = CreateHeavyStrikeCard();
            deck.AddCard(heavyStrike);
        }
        
        // Brace (4 copies) - Basic Defense Card
        for (int i = 0; i < 4; i++)
        {
            Card brace = CreateBraceCard();
            deck.AddCard(brace);
        }
        
        // Ground Slam (2 copies)
        for (int i = 0; i < 2; i++)
        {
            Card groundSlam = CreateGroundSlamCard();
            deck.AddCard(groundSlam);
        }
        
        // Intimidating Shout (2 copies)
        for (int i = 0; i < 2; i++)
        {
            Card intimidatingShout = CreateIntimidatingShoutCard();
            deck.AddCard(intimidatingShout);
        }
        
        // Cleave (2 copies)
        for (int i = 0; i < 2; i++)
        {
            Card cleave = CreateCleaveCard();
            deck.AddCard(cleave);
        }
        
        // Endure (2 copies)
        for (int i = 0; i < 2; i++)
        {
            Card endure = CreateEndureCard();
            deck.AddCard(endure);
        }
    }
    
    // Heavy Strike Card - Basic Attack
    private Card CreateHeavyStrikeCard()
    {
        return new Card
        {
            cardName = "Heavy Strike",
            description = "Deal {damage} physical damage. Scales with melee weapon and Strength.",
            cardType = CardType.Attack,
            manaCost = 1,
            baseDamage = 8f,
            primaryDamageType = DamageType.Physical,
            scalesWithMeleeWeapon = true,
            tags = new List<string> { "Attack", "Physical", "Combo" },
            requirements = new CardRequirements
            {
                requiredStrength = 25
            },
            damageScaling = new AttributeScaling
            {
                strengthScaling = 0.5f  // + Strength/2
            }
        };
    }
    
    // Brace Card - Basic Defense
    private Card CreateBraceCard()
    {
        return new Card
        {
            cardName = "Brace",
            description = "Gain {guard} Guard. Scales with Strength.",
            cardType = CardType.Guard,
            manaCost = 1,
            baseGuard = 5f, // Base guard amount
            tags = new List<string> { "Guard", "Strength", "Combo" },
            requirements = new CardRequirements
            {
                requiredStrength = 20
            },
            guardScaling = new AttributeScaling
            {
                strengthScaling = 0.25f  // + Strength/4
            }
        };
    }
    
    // Ground Slam Card - Area Attack
    private Card CreateGroundSlamCard()
    {
        return new Card
        {
            cardName = "Ground Slam",
            description = "Deal {damage} physical damage to all {aoeTargets} enemies. Scales with melee weapon and Strength.",
            cardType = CardType.Attack,
            manaCost = 2,
            baseDamage = 4f,
            primaryDamageType = DamageType.Physical,
            scalesWithMeleeWeapon = true,
            isAoE = true, // Area of Effect - hits all enemies
            aoeTargets = 3, // Hits all 3 enemies
            tags = new List<string> { "Attack", "Physical", "Combo" },
            requirements = new CardRequirements
            {
                requiredStrength = 30
            },
            damageScaling = new AttributeScaling
            {
                strengthScaling = 0.25f  // + Strength/4
            }
        };
    }
    
    // Intimidating Shout Card - Skill
    private Card CreateIntimidatingShoutCard()
    {
        return new Card
        {
            cardName = "Intimidating Shout",
            description = "Apply 1 Vulnerability to all enemies. Effect increases with Strength.",
            cardType = CardType.Skill,
            manaCost = 1,
            tags = new List<string> { "Warcry", "Skill", "Strength", "Combo" },
            requirements = new CardRequirements
            {
                requiredStrength = 15 // No strength requirement for this skill
            }
        };
    }
    
    // Cleave Card - Multi-Target Attack
    private Card CreateCleaveCard()
    {
        return new Card
        {
            cardName = "Cleave",
            description = "Deal {damage} physical damage to all {aoeTargets} enemies. Scales with melee weapon and Strength.",
            cardType = CardType.Attack,
            manaCost = 1,
            baseDamage = 7f,
            primaryDamageType = DamageType.Physical,
            scalesWithMeleeWeapon = true,
            isAoE = true, // Area of Effect - hits multiple enemies
            aoeTargets = 3, // Hits all 3 enemies
            tags = new List<string> { "Attack", "Physical", "Combo" },
            requirements = new CardRequirements
            {
                requiredStrength = 28
            },
            damageScaling = new AttributeScaling
            {
                strengthScaling = 0.333f  // + Strength/3 (approximately)
            }
        };
    }
    
    // Endure Card - Guard with Draw
    private Card CreateEndureCard()
    {
        return new Card
        {
            cardName = "Endure",
            description = "Gain {guard} Guard. Scales with Strength. Draw a card.",
            cardType = CardType.Guard,
            manaCost = 1,
            baseGuard = 8f, // Base guard amount
            tags = new List<string> { "Guard", "Strength", "Combo" },
            requirements = new CardRequirements
            {
                requiredStrength = 22
            },
            guardScaling = new AttributeScaling
            {
                strengthScaling = 0.25f  // + Strength/4
            }
        };
    }
}

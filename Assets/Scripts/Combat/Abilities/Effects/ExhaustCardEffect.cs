using UnityEngine;

[CreateAssetMenu(fileName = "ExhaustCardEffect", menuName = "Dexiled/Enemies/Effects/Exhaust Card")]
public class ExhaustCardEffect : AbilityEffect
{
    [Header("Exhaust Settings")]
    [Tooltip("Number of cards to exhaust from player's hand")]
    [Min(1)] public int cardsToExhaust = 1;
    
    [Tooltip("If true, targets cards in hand. If false, targets discard pile.")]
    public bool exhaustFromHand = true;
    
    public override void Execute(AbilityContext ctx)
    {
        if (ctx == null) return;
        
        var deckManager = CombatDeckManager.Instance;
        if (deckManager == null)
        {
            Debug.LogWarning("[ExhaustCardEffect] CombatDeckManager not found!");
            return;
        }
        
        var combatUI = UnityEngine.Object.FindFirstObjectByType<AnimatedCombatUI>();
        
        for (int i = 0; i < cardsToExhaust; i++)
        {
            if (exhaustFromHand)
            {
                // Exhaust from hand
                var hand = deckManager.GetHand();
                if (hand != null && hand.Count > 0)
                {
                    int randomIndex = Random.Range(0, hand.Count);
                    var exhaustedCard = hand[randomIndex];
                    
                    // Exhaust from hand
                    deckManager.ExhaustCardFromHand(randomIndex);
                    
                    Debug.Log($"[ExhaustCardEffect] Exhausted {exhaustedCard.cardName} from hand");
                    
                    if (combatUI != null)
                    {
                        combatUI.LogMessage($"<color=grey>Exhausted!</color> {exhaustedCard.cardName} was removed.");
                    }
                }
                else
                {
                    Debug.Log($"[ExhaustCardEffect] No cards in hand to exhaust");
                }
            }
            else
            {
                // Exhaust from discard pile
                var discardPile = deckManager.GetDiscardPile();
                if (discardPile != null && discardPile.Count > 0)
                {
                    int randomIndex = Random.Range(0, discardPile.Count);
                    var exhaustedCard = discardPile[randomIndex];
                    
                    // Use proper exhaust method for tracking
                    deckManager.ExhaustCardFromDiscardPile(randomIndex);
                    
                    Debug.Log($"[ExhaustCardEffect] Exhausted {exhaustedCard.cardName} from discard pile");
                    
                    if (combatUI != null)
                    {
                        combatUI.LogMessage($"<color=grey>Exhausted!</color> {exhaustedCard.cardName} was removed from discard.");
                    }
                }
                else
                {
                    Debug.Log($"[ExhaustCardEffect] No cards in discard pile to exhaust");
                }
            }
        }
    }
}


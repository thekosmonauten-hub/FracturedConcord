using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Processes ascendancy modifier events when combat events occur.
/// Subscribes to combat events and triggers appropriate modifier effects.
/// </summary>
public class AscendancyModifierEventProcessor : MonoBehaviour
{
    private static AscendancyModifierEventProcessor _instance;
    public static AscendancyModifierEventProcessor Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AscendancyModifierEventProcessor>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("AscendancyModifierEventProcessor");
                    _instance = go.AddComponent<AscendancyModifierEventProcessor>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    private CombatDeckManager deckManager;
    private CharacterManager characterManager;
    private Dictionary<string, Dictionary<string, object>> modifierStates = new Dictionary<string, Dictionary<string, object>>();
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        SubscribeToEvents();
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    private void SubscribeToEvents()
    {
        // Subscribe to CombatDeckManager events
        if (deckManager == null)
        {
            deckManager = CombatDeckManager.Instance;
        }
        
        if (deckManager != null)
        {
            deckManager.OnCardDrawn += HandleCardDrawn;
            deckManager.OnCardPlayed += HandleCardPlayed;
        }
        
        // Subscribe to turn start/end events (would need to find the appropriate manager)
        // This will be called manually from CombatDisplayManager or similar
    }
    
    private void UnsubscribeFromEvents()
    {
        if (deckManager != null)
        {
            deckManager.OnCardDrawn -= HandleCardDrawn;
            deckManager.OnCardPlayed -= HandleCardPlayed;
        }
    }
    
    /// <summary>
    /// Process modifiers for a specific event type
    /// </summary>
    public void ProcessModifierEvent(ModifierEventType eventType, Dictionary<string, object> eventContext)
    {
        if (characterManager == null)
        {
            characterManager = CharacterManager.Instance;
        }
        
        Character character = characterManager != null && characterManager.HasCharacter() ? 
            characterManager.GetCurrentCharacter() : null;
        
        if (character == null)
        {
            Debug.LogWarning("[AscendancyModifierEventProcessor] No character available for modifier processing");
            return;
        }
        
        // Get active modifiers from registry
        var registry = AscendancyModifierRegistry.Instance;
        if (registry == null)
        {
            Debug.LogWarning("[AscendancyModifierEventProcessor] AscendancyModifierRegistry not found");
            return;
        }
        
        // Get all active modifiers for this character's ascendancy
        var activeModifiers = registry.GetActiveModifiers(character);
        if (activeModifiers == null || activeModifiers.Count == 0)
        {
            return; // No active modifiers
        }
        
        // Add event type to context
        if (eventContext == null)
        {
            eventContext = new Dictionary<string, object>();
        }
        eventContext["eventType"] = eventType;
        
        // Process each modifier that listens to this event
        foreach (var modifier in activeModifiers)
        {
            if (modifier == null || modifier.effects == null) continue;
            
            foreach (var effect in modifier.effects)
            {
                if (effect.eventType == eventType)
                {
                    // Get or create modifier state
                    if (!modifierStates.ContainsKey(modifier.modifierId))
                    {
                        modifierStates[modifier.modifierId] = new Dictionary<string, object>();
                    }
                    var modifierState = modifierStates[modifier.modifierId];
                    
                    // Process each action in this effect
                    if (effect.actions != null)
                    {
                        foreach (var action in effect.actions)
                        {
                            if (action != null)
                            {
                                ModifierEffectResolver.ResolveAction(
                                    action, 
                                    eventContext, 
                                    character, 
                                    modifierState, 
                                    modifier
                                );
                            }
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Handle card drawn event
    /// </summary>
    private void HandleCardDrawn(CardDataExtended card)
    {
        if (card == null) return;
        
        var eventContext = new Dictionary<string, object>
        {
            ["card"] = card,
            ["cardType"] = card.GetCardTypeEnum()
        };
        
        ProcessModifierEvent(ModifierEventType.OnCardDrawn, eventContext);
        
        // Update card visual if it was marked as Temporal
        if (eventContext.ContainsKey("isTemporal") && Convert.ToBoolean(eventContext["isTemporal"]))
        {
            UpdateTemporalCardVisual(card);
        }
    }
    
    /// <summary>
    /// Handle card played event
    /// </summary>
    private void HandleCardPlayed(CardDataExtended card)
    {
        if (card == null) return;
        
        var eventContext = new Dictionary<string, object>
        {
            ["card"] = card,
            ["cardType"] = card.GetCardTypeEnum()
        };
        
        ProcessModifierEvent(ModifierEventType.OnCardPlayed, eventContext);
    }
    
    /// <summary>
    /// Called manually from combat systems for turn start
    /// </summary>
    public void OnTurnStart()
    {
        var eventContext = new Dictionary<string, object>();
        ProcessModifierEvent(ModifierEventType.OnTurnStart, eventContext);
    }
    
    /// <summary>
    /// Called manually from combat systems for turn end
    /// </summary>
    public void OnTurnEnd()
    {
        var eventContext = new Dictionary<string, object>();
        ProcessModifierEvent(ModifierEventType.OnTurnEnd, eventContext);
    }
    
    /// <summary>
    /// Called manually from combat systems for combat start
    /// </summary>
    public void OnCombatStart()
    {
        // Reset modifier states for new combat
        modifierStates.Clear();
        
        var eventContext = new Dictionary<string, object>();
        ProcessModifierEvent(ModifierEventType.OnCombatStart, eventContext);
    }
    
    /// <summary>
    /// Update visual for a Temporal card
    /// </summary>
    private void UpdateTemporalCardVisual(CardDataExtended card)
    {
        // The visual update will be handled automatically by DeckBuilderCardUI.UpdateTemporalCardVisual()
        // when the card is displayed. We just need to ensure the card has the Temporal tag,
        // which is already done in ResolveMarkCardAsTemporal.
        Debug.Log($"[AscendancyModifierEventProcessor] Card '{card.cardName}' marked as Temporal - visual will update on next display refresh");
    }
}


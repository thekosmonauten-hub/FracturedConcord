using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Processes Reliance Aura modifier events during combat.
/// Similar to AscendancyModifierEventProcessor but for Reliance Auras.
/// Automatically applies scaled effects based on aura levels.
/// </summary>
public class RelianceAuraModifierEventProcessor : MonoBehaviour
{
    private static RelianceAuraModifierEventProcessor _instance;
    public static RelianceAuraModifierEventProcessor Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<RelianceAuraModifierEventProcessor>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("RelianceAuraModifierEventProcessor");
                    _instance = go.AddComponent<RelianceAuraModifierEventProcessor>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    // Track modifier state per modifier ID (for effects that need state like Crumble stacks)
    private Dictionary<string, Dictionary<string, object>> modifierStates = new Dictionary<string, Dictionary<string, object>>();
    
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Process a modifier event for all active Reliance Auras.
    /// Effects are automatically scaled based on aura levels.
    /// </summary>
    public void ProcessEvent(ModifierEventType eventType, Dictionary<string, object> eventContext = null)
    {
        CharacterManager characterManager = CharacterManager.Instance;
        if (characterManager == null || !characterManager.HasCharacter())
        {
            return; // No character available
        }
        
        Character character = characterManager.GetCurrentCharacter();
        if (character == null)
        {
            return;
        }
        
        // Get active effects from registry (already scaled based on aura levels)
        var registry = RelianceAuraModifierRegistry.Instance;
        if (registry == null)
        {
            return; // No registry available
        }
        
        // Get all active effects (scaled based on aura levels)
        var activeEffects = registry.GetActiveEffects(character);
        if (activeEffects == null || activeEffects.Count == 0)
        {
            return; // No active effects
        }
        
        // Add event type to context
        if (eventContext == null)
        {
            eventContext = new Dictionary<string, object>();
        }
        eventContext["eventType"] = eventType;
        
        // Process each effect that listens to this event
        foreach (var effect in activeEffects)
        {
            if (effect == null || effect.eventType != eventType) continue;
            
            // Get modifier ID for state tracking (from the effect's source modifier)
            // Note: We need to track which modifier this effect came from for state management
            // For now, we'll use a simplified approach - in the future, we might need to
            // add a modifierId field to ModifierEffect or track it differently
            
            // Process each action in this effect
            if (effect.actions != null)
            {
                foreach (var action in effect.actions)
                {
                    if (action != null)
                    {
                        // Create a modifier state for this effect (simplified - uses effect hash as key)
                        string stateKey = $"AuraEffect_{effect.GetHashCode()}";
                        if (!modifierStates.ContainsKey(stateKey))
                        {
                            modifierStates[stateKey] = new Dictionary<string, object>();
                        }
                        var modifierState = modifierStates[stateKey];
                        
                        // Check if this is a Reliance Aura specific action
                        if (action.actionType >= ModifierActionType.SpreadStatusOnKill && 
                            action.actionType <= ModifierActionType.ScaleDiscardPower)
                        {
                            // Route to RelianceAuraModifierEffectResolver
                            RelianceAuraModifierDefinition dummyDef = ScriptableObject.CreateInstance<RelianceAuraModifierDefinition>();
                            dummyDef.modifierId = stateKey;
                            dummyDef.description = "Runtime Aura Effect";
                            
                            RelianceAuraModifierEffectResolver.ResolveAction(
                                action,
                                eventContext,
                                character,
                                modifierState,
                                dummyDef
                            );
                        }
                        else
                        {
                            // Route to standard ModifierEffectResolver
                            AscendancyModifierDefinition dummyDef = ScriptableObject.CreateInstance<AscendancyModifierDefinition>();
                            dummyDef.modifierId = stateKey;
                            dummyDef.description = "Runtime Aura Effect";
                            
                            ModifierEffectResolver.ResolveAction(
                                action,
                                eventContext,
                                character,
                                modifierState,
                                dummyDef
                            );
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Clear all modifier states (call when combat ends)
    /// </summary>
    public void ClearStates()
    {
        modifierStates.Clear();
    }
}


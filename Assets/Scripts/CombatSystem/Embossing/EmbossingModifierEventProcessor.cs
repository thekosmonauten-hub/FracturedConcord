using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Dexiled.CombatSystem.Embossing
{
    /// <summary>
    /// Processes embossing modifier events during combat
    /// Subscribes to combat events and applies embossing modifier effects
    /// </summary>
    public class EmbossingModifierEventProcessor : MonoBehaviour
    {
        private static EmbossingModifierEventProcessor _instance;
        public static EmbossingModifierEventProcessor Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<EmbossingModifierEventProcessor>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("EmbossingModifierEventProcessor");
                        _instance = go.AddComponent<EmbossingModifierEventProcessor>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private Dictionary<string, Dictionary<string, object>> modifierStates = new Dictionary<string, Dictionary<string, object>>();
        private Character currentCharacter;

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

        private CombatDeckManager deckManager;

        void OnEnable()
        {
            // Subscribe to combat events
            deckManager = CombatDeckManager.Instance;
            if (deckManager != null)
            {
                deckManager.OnCardPlayed += HandleCardPlayed;
            }
            
            // Also ensure we're initialized
            if (CharacterManager.Instance != null && CharacterManager.Instance.HasCharacter())
            {
                Initialize(CharacterManager.Instance.GetCurrentCharacter());
            }
        }

        void OnDisable()
        {
            // Unsubscribe from events
            if (deckManager != null)
            {
                deckManager.OnCardPlayed -= HandleCardPlayed;
            }
        }

        /// <summary>
        /// Handle CardDataExtended from CombatDeckManager.OnCardPlayed event
        /// Convert it to Card (embossings are now copied automatically in ToCard())
        /// </summary>
        private void HandleCardPlayed(CardDataExtended cardData)
        {
            if (cardData == null || currentCharacter == null)
                return;

            // Convert CardDataExtended to Card
            // ToCard() now automatically copies embossings from CardDataExtended
            Card card = cardData.ToCard();
            
            // Get target enemy (if available) - this is tricky since the event doesn't provide it
            // We'll process OnCardPlayed events without target for now
            OnCardPlayed(card, currentCharacter, null);
        }

        /// <summary>
        /// Initialize the processor with the current character
        /// </summary>
        public void Initialize(Character character)
        {
            currentCharacter = character;
            modifierStates.Clear();
        }

        /// <summary>
        /// Process embossing modifiers when a card is played
        /// Call this from CombatManager or CardEffectProcessor when a card is played
        /// </summary>
        public void OnCardPlayed(Card card, Character character, Enemy targetEnemy = null)
        {
            if (card == null || character == null || card.appliedEmbossings == null || card.appliedEmbossings.Count == 0)
                return;

            // Get active embossing modifiers for this card
            List<ModifierEffect> activeEffects = EmbossingModifierRegistry.Instance.GetActiveEffects(card, character);

            if (activeEffects == null || activeEffects.Count == 0)
                return;

            // Build event context
            Dictionary<string, object> eventContext = new Dictionary<string, object>
            {
                ["eventType"] = ModifierEventType.OnCardPlayed,
                ["card"] = card,
                ["character"] = character,
                ["targetEnemy"] = targetEnemy
            };

            // Process each effect
            foreach (var effect in activeEffects)
            {
                if (effect.eventType == ModifierEventType.OnCardPlayed)
                {
                    ProcessModifierEffect(effect, eventContext, character, card);
                }
            }
        }

        /// <summary>
        /// Process embossing modifiers when damage is dealt
        /// Call this from DamageCalculator or CardEffectProcessor when damage is calculated/dealt
        /// </summary>
        public void OnDamageDealt(Card card, Character character, Enemy targetEnemy, float damage, DamageType damageType)
        {
            if (card == null || character == null || card.appliedEmbossings == null || card.appliedEmbossings.Count == 0)
                return;

            // Get active embossing modifiers for this card
            List<ModifierEffect> activeEffects = EmbossingModifierRegistry.Instance.GetActiveEffects(card, character);

            if (activeEffects == null || activeEffects.Count == 0)
                return;

            // Build event context
            Dictionary<string, object> eventContext = new Dictionary<string, object>
            {
                ["eventType"] = ModifierEventType.OnDamageDealt,
                ["card"] = card,
                ["character"] = character,
                ["targetEnemy"] = targetEnemy,
                ["damage"] = damage,
                ["damageType"] = damageType
            };

            // Process each effect
            foreach (var effect in activeEffects)
            {
                if (effect.eventType == ModifierEventType.OnDamageDealt || effect.eventType == ModifierEventType.OnDamageDealtToEnemy)
                {
                    ProcessModifierEffect(effect, eventContext, character, card);
                }
            }
        }

        /// <summary>
        /// Process embossing modifiers when an enemy is killed
        /// Call this from CombatManager or Enemy when an enemy is defeated
        /// </summary>
        public void OnEnemyKilled(Card card, Character character, Enemy defeatedEnemy)
        {
            if (card == null || character == null || card.appliedEmbossings == null || card.appliedEmbossings.Count == 0)
                return;

            // Get active embossing modifiers for this card
            List<ModifierEffect> activeEffects = EmbossingModifierRegistry.Instance.GetActiveEffects(card, character);

            if (activeEffects == null || activeEffects.Count == 0)
                return;

            // Build event context
            Dictionary<string, object> eventContext = new Dictionary<string, object>
            {
                ["eventType"] = ModifierEventType.OnEnemyKilled,
                ["card"] = card,
                ["character"] = character,
                ["defeatedEnemy"] = defeatedEnemy
            };

            // Process each effect
            foreach (var effect in activeEffects)
            {
                if (effect.eventType == ModifierEventType.OnEnemyKilled || effect.eventType == ModifierEventType.OnEnemyDefeated)
                {
                    ProcessModifierEffect(effect, eventContext, character, card);
                }
            }
        }

        /// <summary>
        /// Process embossing modifiers when a status effect is applied
        /// Call this from StatusEffectManager when a status is applied
        /// </summary>
        public void OnStatusApplied(Card card, Character character, Enemy targetEnemy, StatusEffectType statusType)
        {
            if (card == null || character == null || card.appliedEmbossings == null || card.appliedEmbossings.Count == 0)
                return;

            // Get active embossing modifiers for this card
            List<ModifierEffect> activeEffects = EmbossingModifierRegistry.Instance.GetActiveEffects(card, character);

            if (activeEffects == null || activeEffects.Count == 0)
                return;

            // Build event context
            Dictionary<string, object> eventContext = new Dictionary<string, object>
            {
                ["eventType"] = ModifierEventType.OnStatusApplied,
                ["card"] = card,
                ["character"] = character,
                ["targetEnemy"] = targetEnemy,
                ["statusType"] = statusType
            };

            // Process each effect
            foreach (var effect in activeEffects)
            {
                if (effect.eventType == ModifierEventType.OnStatusApplied)
                {
                    ProcessModifierEffect(effect, eventContext, character, card);
                }
            }
        }

        /// <summary>
        /// Process a single modifier effect
        /// </summary>
        private void ProcessModifierEffect(ModifierEffect effect, Dictionary<string, object> eventContext, Character character, Card card)
        {
            if (effect == null || effect.actions == null || effect.actions.Count == 0)
                return;

            // Check conditions
            if (!CheckConditions(effect.conditions, eventContext, character))
                return;

            // Get or create modifier state for this card
            string stateKey = GetStateKey(card);
            if (!modifierStates.ContainsKey(stateKey))
            {
                modifierStates[stateKey] = new Dictionary<string, object>();
            }
            Dictionary<string, object> modifierState = modifierStates[stateKey];

            // Process each action in order
            var sortedActions = effect.actions.OrderBy(a => a.executionOrder).ToList();
            foreach (var action in sortedActions)
            {
                ProcessModifierAction(action, eventContext, character, modifierState, card);
            }
        }

        /// <summary>
        /// Check if modifier conditions are met
        /// </summary>
        private bool CheckConditions(List<ModifierCondition> conditions, Dictionary<string, object> eventContext, Character character)
        {
            if (conditions == null || conditions.Count == 0)
                return true;

            foreach (var condition in conditions)
            {
                if (!CheckSingleCondition(condition, eventContext, character))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check a single condition
        /// </summary>
        private bool CheckSingleCondition(ModifierCondition condition, Dictionary<string, object> eventContext, Character character)
        {
            if (condition == null)
                return true;

            switch (condition.conditionType)
            {
                case ModifierConditionType.HasStack:
                    // Check if character has required stack
                    // Implementation depends on your stack system
                    break;

                case ModifierConditionType.StackCount:
                    // Check stack count
                    break;

                case ModifierConditionType.HasStatus:
                    // Check if character has status effect
                    break;

                case ModifierConditionType.HealthPercent:
                    if (character != null)
                    {
                        float healthPercent = character.currentHealth / character.maxHealth;
                        // Compare with condition value
                    }
                    break;

                case ModifierConditionType.CardType:
                    if (eventContext.ContainsKey("card"))
                    {
                        Card card = eventContext["card"] as Card;
                        // Check card type matches condition
                    }
                    break;

                case ModifierConditionType.DamageType:
                    if (eventContext.ContainsKey("damageType"))
                    {
                        DamageType damageType = (DamageType)eventContext["damageType"];
                        // Check damage type matches condition
                    }
                    break;

                case ModifierConditionType.RandomChance:
                    float chance = 0f;
                    if (condition.parameterDict != null && condition.parameterDict.ContainsKey("chance"))
                    {
                        chance = condition.parameterDict.GetParameter<float>("chance");
                    }
                    return Random.Range(0f, 1f) < chance;

                default:
                    return true;
            }

            return true; // Default: condition passes
        }

        /// <summary>
        /// Process a single modifier action
        /// </summary>
        private void ProcessModifierAction(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState, Card card)
        {
            if (action == null || character == null)
                return;

            // Use the action directly - ModifierEffectResolver can work with the action's parameterDict
            // The action.parameters property returns a converted Dictionary when needed

            // Get the embossing modifier definition for context (optional)
            EmbossingModifierDefinition embossingDef = null;
            if (card != null && card.appliedEmbossings != null && card.appliedEmbossings.Count > 0)
            {
                // Try to find the modifier definition
                foreach (var embossingInstance in card.appliedEmbossings)
                {
                    var modifiers = EmbossingModifierRegistry.Instance.GetModifiersForEmbossing(embossingInstance.embossingId);
                    if (modifiers != null && modifiers.Count > 0)
                    {
                        embossingDef = modifiers[0]; // Use first modifier as context
                        break;
                    }
                }
            }

            // Use the standard ModifierEffectResolver
            // Note: ModifierEffectResolver expects AscendancyModifierDefinition, but we can pass null
            // For embossing-specific actions, we may need an EmbossingModifierEffectResolver similar to RelianceAuraModifierEffectResolver
            ModifierEffectResolver.ResolveAction(action, eventContext, character, modifierState, null);
        }

        /// <summary>
        /// Get a unique state key for a card's embossings
        /// </summary>
        private string GetStateKey(Card card)
        {
            if (card == null || card.appliedEmbossings == null || card.appliedEmbossings.Count == 0)
                return "default";

            // Create a key from all embossing IDs
            var embossingIds = card.appliedEmbossings.Select(e => e.embossingId).OrderBy(id => id);
            return string.Join("_", embossingIds);
        }

        /// <summary>
        /// Clear modifier states (call when combat ends)
        /// </summary>
        public void ClearStates()
        {
            modifierStates.Clear();
        }
    }
}


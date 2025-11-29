using System.Collections.Generic;
using UnityEngine;

namespace Dexiled.Data.Status
{
    /// <summary>
    /// Central database for all status effect definitions, sprites, and metadata.
    /// Similar to CurrencyDatabase, provides a single source of truth for status effects.
    /// </summary>
    [CreateAssetMenu(fileName = "StatusDatabase", menuName = "Dexiled/Status Database")]
    public class StatusDatabase : ScriptableObject
    {
        [Header("Global Default Icons")]
        [Tooltip("Default positive icon used for all effects that support positive/negative icons when effect-specific positive icon is not set.")]
        public Sprite globalPositiveIcon;
        [Tooltip("Default negative icon used for all effects that support positive/negative icons when effect-specific negative icon is not set.")]
        public Sprite globalNegativeIcon;
        
        [Header("Status Effect Definitions")]
        public List<StatusEffectData> statusEffects = new List<StatusEffectData>();

        /// <summary>
        /// Get status effect data by type
        /// </summary>
        public StatusEffectData GetStatusEffect(StatusEffectType type)
        {
            return statusEffects.Find(s => s.effectType == type);
        }

        /// <summary>
        /// Get status effect data by name
        /// </summary>
        public StatusEffectData GetStatusEffectByName(string name)
        {
            return statusEffects.Find(s => s.effectName == name);
        }

        /// <summary>
        /// Get sprite for a status effect type
        /// </summary>
        public Sprite GetStatusEffectSprite(StatusEffectType type)
        {
            StatusEffectData data = GetStatusEffect(type);
            return data != null ? data.iconSprite : null;
        }
        
        /// <summary>
        /// Get sprite for a status effect type, considering positive/negative icons based on magnitude
        /// Uses global defaults if effect-specific icons aren't set
        /// </summary>
        public Sprite GetStatusEffectSprite(StatusEffectType type, float magnitude)
        {
            StatusEffectData data = GetStatusEffect(type);
            if (data == null) return null;
            
            // If this effect uses positive/negative icons, return the appropriate one
            if (data.usePositiveNegativeIcons)
            {
                if (magnitude > 0f)
                {
                    // Use effect-specific positive icon, or fall back to global positive icon
                    if (data.positiveIconSprite != null)
                    {
                        return data.positiveIconSprite;
                    }
                    else if (globalPositiveIcon != null)
                    {
                        return globalPositiveIcon;
                    }
                }
                else if (magnitude < 0f)
                {
                    // Use effect-specific negative icon, or fall back to global negative icon
                    if (data.negativeIconSprite != null)
                    {
                        return data.negativeIconSprite;
                    }
                    else if (globalNegativeIcon != null)
                    {
                        return globalNegativeIcon;
                    }
                }
            }
            
            // Fallback to default icon
            return data.iconSprite;
        }

        /// <summary>
        /// Get sprite by icon name (for backward compatibility with existing iconName system)
        /// </summary>
        public Sprite GetStatusEffectSpriteByName(string iconName)
        {
            // Try to find by matching iconName to effectName or description
            StatusEffectData data = statusEffects.Find(s => 
                s.effectName.Equals(iconName, System.StringComparison.OrdinalIgnoreCase) ||
                s.effectName.Replace(" ", "").Equals(iconName, System.StringComparison.OrdinalIgnoreCase));
            
            return data != null ? data.iconSprite : null;
        }

        /// <summary>
        /// Get color for a status effect type
        /// </summary>
        public Color GetStatusEffectColor(StatusEffectType type)
        {
            StatusEffectData data = GetStatusEffect(type);
            return data != null ? data.effectColor : Color.white;
        }

        /// <summary>
        /// Check if a status effect is a debuff
        /// </summary>
        public bool IsDebuff(StatusEffectType type)
        {
            StatusEffectData data = GetStatusEffect(type);
            return data != null ? data.isDebuff : true;
        }

        /// <summary>
        /// Get tooltip description for a status effect
        /// </summary>
        public string GetTooltipDescription(StatusEffectType type)
        {
            StatusEffectData data = GetStatusEffect(type);
            if (data != null && !string.IsNullOrEmpty(data.tooltipDescription))
            {
                return data.tooltipDescription;
            }
            return data != null ? data.description : "";
        }
    }
}


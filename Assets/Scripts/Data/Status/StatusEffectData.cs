using UnityEngine;

namespace Dexiled.Data.Status
{
    /// <summary>
    /// Data structure for a status effect entry in the StatusDatabase
    /// </summary>
    [System.Serializable]
    public class StatusEffectData
    {
        [Header("Basic Information")]
        public StatusEffectType effectType;
        public string effectName;
        public string description;
        
        [Header("Visual Properties")]
        public Sprite iconSprite;
        [Tooltip("Optional: Icon for positive values (e.g., +2 Strength). Used for temp stats and buffs.")]
        public Sprite positiveIconSprite;
        [Tooltip("Optional: Icon for negative values (e.g., -2 Strength). Used for temp stats and debuffs.")]
        public Sprite negativeIconSprite;
        public Color effectColor = Color.white;
        public bool isDebuff = true;
        
        [Header("Icon Selection")]
        [Tooltip("If true, will use positive/negative icons based on magnitude. Applies to temp stats (Strength, Dexterity, Intelligence) and other effects that can be negative.")]
        public bool usePositiveNegativeIcons = false;
        
        [Header("Effect Properties")]
        public float defaultMagnitude = 0f;
        public int defaultDuration = 1;
        public float tickInterval = 1f;
        public DamageType damageType = DamageType.Physical;
        
        [Header("Tooltip Information")]
        public string tooltipDescription = "";
    }
}


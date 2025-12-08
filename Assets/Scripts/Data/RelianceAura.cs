using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject defining a Reliance Aura - a persistent buff that can be activated in EquipmentScreen.
/// Auras work like cards and can have embossings applied to modify their effects.
/// </summary>
[CreateAssetMenu(fileName = "New Reliance Aura", menuName = "Dexiled/Reliance Aura", order = 4)]
public class RelianceAura : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("Aura name (e.g., 'Pyreheart Mantle', 'Frostgraft Veil')")]
    public string auraName = "";
    
    [Tooltip("Category (Fire, Cold, Lightning, Physical, Ailment, Speed, Critical, Spell, Mana, Armor, Evasion, Energy Shield, Life Regen, Resist, Card Draw, Prep, Discard)")]
    public string category = "";
    
    [TextArea(3, 5)]
    [Tooltip("Description of what this aura does")]
    public string description = "";
    
    [Header("Visual Assets")]
    [Tooltip("Icon for this aura (displayed in UI)")]
    public Sprite icon;
    
    [Tooltip("Background color/theme for this aura")]
    public Color themeColor = Color.white;
    
    [Header("Effect Scaling")]
    [Tooltip("Effect at level 1")]
    [TextArea(2, 3)]
    public string effectLevel1 = "";
    
    [Tooltip("Effect at level 20")]
    [TextArea(2, 3)]
    public string effectLevel20 = "";
    
    [Header("Reliance Cost")]
    [Tooltip("Reliance cost to activate this aura (persistent cost while active)")]
    [Range(0, 200)]
    public int relianceCost = 100;
    
    [Header("Embossing System")]
    [Tooltip("Number of embossing slots (0-5, like cards)")]
    [Range(0, 5)]
    public int embossingSlots = 1;
    
    [Tooltip("Applied embossing effect IDs (runtime, not serialized in asset)")]
    [System.NonSerialized]
    public List<string> appliedEmbossings = new List<string>();
    
    [Header("Modifier Definitions")]
    [Tooltip("List of modifier IDs that define this aura's effects")]
    public List<string> modifierIds = new List<string>();
    
    [Header("Unlock Requirements")]
    [Tooltip("Character level required to unlock this aura")]
    public int requiredLevel = 1;
    
    [Tooltip("Quest or challenge required to unlock (optional)")]
    public string unlockRequirement = "";
    
    /// <summary>
    /// Check if this aura can be embossed
    /// </summary>
    public bool CanEmboss()
    {
        return embossingSlots > 0 && (appliedEmbossings == null || appliedEmbossings.Count < embossingSlots);
    }
    
    /// <summary>
    /// Get number of empty embossing slots
    /// </summary>
    public int GetEmptySlotCount()
    {
        if (appliedEmbossings == null)
            return embossingSlots;
        return Mathf.Max(0, embossingSlots - appliedEmbossings.Count);
    }
    
    /// <summary>
    /// Get number of filled embossing slots
    /// </summary>
    public int GetFilledSlotCount()
    {
        return appliedEmbossings != null ? appliedEmbossings.Count : 0;
    }
    
    /// <summary>
    /// Check if aura has any empty slots
    /// </summary>
    public bool HasEmptyEmbossingSlot()
    {
        return GetEmptySlotCount() > 0;
    }
    
    /// <summary>
    /// Get formatted effect description interpolated between level 1 and 20
    /// </summary>
    public string GetEffectAtLevel(int level)
    {
        if (level <= 1)
            return effectLevel1;
        if (level >= 20)
            return effectLevel20;
        
        // Simple interpolation - could be enhanced with actual value parsing
        return $"{effectLevel1} → {effectLevel20} (Level {level})";
    }
}


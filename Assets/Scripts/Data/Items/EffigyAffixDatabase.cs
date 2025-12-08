using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Dedicated affix database for Effigies with shape-specific affix categories.
/// Each shape (Cross, L-Shape, Line, etc.) has a unified affix pool - no prefix/suffix distinction.
/// </summary>
[CreateAssetMenu(fileName = "Effigy Affix Database", menuName = "Dexiled/Items/Effigy Affix Database")]
public class EffigyAffixDatabase : ScriptableObject
{
    [Header("Cross Shape Affixes")]
    [Tooltip("Cross shape provides all-attribute bonuses. Affixes should focus on attributes and hybrid stats.")]
    public List<AffixCategory> crossAffixCategories = new List<AffixCategory>();
    
    [Header("L-Shape Affixes")]
    [Tooltip("L-Shape provides life bonuses. Affixes should focus on life, life regeneration, and life recovery.")]
    public List<AffixCategory> lShapeAffixCategories = new List<AffixCategory>();
    
    [Header("Line Shape Affixes")]
    [Tooltip("Line shape provides damage bonuses. Affixes should focus on damage, damage increased, and damage scaling.")]
    public List<AffixCategory> lineAffixCategories = new List<AffixCategory>();
    
    [Header("S-Shape Affixes")]
    [Tooltip("S-Shape provides evasion and dodge. Affixes should focus on evasion, dodge chance, and movement speed.")]
    public List<AffixCategory> sShapeAffixCategories = new List<AffixCategory>();
    
    [Header("Single Shape Affixes")]
    [Tooltip("Single shape provides random/adaptive bonuses. Affixes should be small, generic stat boosts.")]
    public List<AffixCategory> singleAffixCategories = new List<AffixCategory>();
    
    [Header("Square Shape Affixes")]
    [Tooltip("Square shape provides guard and defense. Affixes should focus on guard, guard effectiveness, and armor.")]
    public List<AffixCategory> squareAffixCategories = new List<AffixCategory>();
    
    [Header("Small L Shape Affixes")]
    [Tooltip("Small L provides offensive defense. Affixes should focus on damage after guard, counter-attacks, and retaliation.")]
    public List<AffixCategory> smallLAffixCategories = new List<AffixCategory>();
    
    [Header("T-Shape Affixes")]
    [Tooltip("T-Shape provides buff duration. Affixes should focus on buff duration, buff effectiveness, and status effect duration.")]
    public List<AffixCategory> tShapeAffixCategories = new List<AffixCategory>();
    
    [Header("Z-Shape Affixes")]
    [Tooltip("Z-Shape provides ailment bonuses. Affixes should focus on ailment chance, ailment damage, and chaos damage.")]
    public List<AffixCategory> zShapeAffixCategories = new List<AffixCategory>();
    
    // Singleton instance
    private static EffigyAffixDatabase _instance;
    public static EffigyAffixDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<EffigyAffixDatabase>("EffigyAffixDatabase");
                if (_instance == null)
                {
                    Debug.LogWarning("[EffigyAffixDatabase] EffigyAffixDatabase not found in Resources folder! Make sure it's named 'EffigyAffixDatabase.asset'");
                }
                else
                {
                    Debug.Log($"[EffigyAffixDatabase] Loaded database with {_instance.GetTotalAffixCount()} total affixes");
                }
            }
            return _instance;
        }
    }
    
    /// <summary>
    /// Get affix categories for a specific shape
    /// </summary>
    public List<AffixCategory> GetAffixCategories(EffigyShapeCategory shape)
    {
        return shape switch
        {
            EffigyShapeCategory.Cross => crossAffixCategories,
            EffigyShapeCategory.LShape => lShapeAffixCategories,
            EffigyShapeCategory.Line => lineAffixCategories,
            EffigyShapeCategory.SShape => sShapeAffixCategories,
            EffigyShapeCategory.Single => singleAffixCategories,
            EffigyShapeCategory.Square => squareAffixCategories,
            EffigyShapeCategory.SmallL => smallLAffixCategories,
            EffigyShapeCategory.TShape => tShapeAffixCategories,
            EffigyShapeCategory.ZShape => zShapeAffixCategories,
            _ => new List<AffixCategory>()
        };
    }
    
    /// <summary>
    /// Get all affixes for a specific shape (unified pool - no prefix/suffix distinction)
    /// </summary>
    public List<Affix> GetAllAffixes(EffigyShapeCategory shape)
    {
        var categories = GetAffixCategories(shape);
        return categories.SelectMany(c => c.GetAllAffixes()).ToList();
    }
    
    /// <summary>
    /// Get total affix count across all shapes
    /// </summary>
    public int GetTotalAffixCount()
    {
        int count = 0;
        var allShapes = new[]
        {
            EffigyShapeCategory.Cross, EffigyShapeCategory.LShape, EffigyShapeCategory.Line,
            EffigyShapeCategory.SShape, EffigyShapeCategory.Single, EffigyShapeCategory.Square,
            EffigyShapeCategory.SmallL, EffigyShapeCategory.TShape, EffigyShapeCategory.ZShape
        };
        
        foreach (var shape in allShapes)
        {
            count += GetAllAffixes(shape).Count;
        }
        
        return count;
    }
}


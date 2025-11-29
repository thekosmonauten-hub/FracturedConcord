using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Effigy element type - determines affix pools and visual theming
/// </summary>
public enum EffigyElement
{
    Fire,       // Red - Strength, Fire Damage, Ignite
    Cold,       // Blue - Intelligence, Energy Shield, Freeze
    Lightning,  // Yellow - Critical, Attack Speed, Shock
    Physical,   // Grey - Armor, Life, Defense
    Chaos       // Purple - Chaos Damage, DoT, Mixed
}

/// <summary>
/// Effigy size tier - determines power potential
/// </summary>
public enum EffigySizeTier
{
    Tiny,       // 1-2 cells - Minor stat boosts
    Medium,     // 3-4 cells - Hybrid stat rolls or small special effects
    Large       // 5-6 cells - Multi-affix, powerful or unique effects
}

public enum EffigyShapeCategory
{
    Unknown,
    Cross,
    LShape,
    Line,
    SShape,
    Single,
    Square,
    SmallL,
    TShape,
    ZShape
}

/// <summary>
/// Effigy data - puzzle pieces with shapes that fit into a 6x4 grid
/// Similar to Last Epoch's Idols or Path of Exile's Cluster Jewels
/// </summary>
[CreateAssetMenu(fileName = "New Effigy", menuName = "Dexiled/Items/Effigy")]
public class Effigy : BaseItem
{
    [Header("Basic Information")]
    public string effigyName = "New Effigy"; // Note: This is separate from BaseItem.itemName
    public Sprite icon;
    
    // Note: description and requiredLevel are inherited from BaseItem
    // We sync effigyName to itemName in OnValidate()
    
    [Header("Element & Tier")]
    [Tooltip("Element type determines affix pools and visual theme")]
    public EffigyElement element = EffigyElement.Fire;
    
    [Tooltip("Size tier is automatically calculated from cell count")]
    public EffigySizeTier sizeTier = EffigySizeTier.Tiny;
    
    [Header("Shape Definition")]
    [Tooltip("Width of the effigy shape (in grid cells)")]
    public int shapeWidth = 1;
    
    [Tooltip("Height of the effigy shape (in grid cells)")]
    public int shapeHeight = 1;
    
    [Tooltip("Shape mask: true = occupied cell, false = empty cell. Row-major order.")]
    public bool[] shapeMask;
    
    [Header("Presentation")]
    [Tooltip("Optional player-facing name (e.g., Shape alias). Falls back to effigyName when empty.")]
    public string displayAlias = string.Empty;
    
    public const int ExplicitAffixTarget = 4;
    // Note: Requirements (requiredLevel) are inherited from BaseItem
    // Use base.requiredLevel field, no duplicate needed
    
    /// <summary>
    /// Get the number of occupied cells in this effigy's shape
    /// </summary>
    public int GetCellCount()
    {
        if (shapeMask == null) return 0;
        return shapeMask.Count(cell => cell);
    }
    
    /// <summary>
    /// Update size tier based on cell count
    /// </summary>
    public void UpdateSizeTier()
    {
        int cellCount = GetCellCount();
        if (cellCount <= 2)
            sizeTier = EffigySizeTier.Tiny;
        else if (cellCount <= 4)
            sizeTier = EffigySizeTier.Medium;
        else
            sizeTier = EffigySizeTier.Large;
    }
    
    /// <summary>
    /// Get element color for UI display
    /// </summary>
    public Color GetElementColor()
    {
        switch (element)
        {
            case EffigyElement.Fire:
                return new Color(1f, 0.3f, 0.2f); // Red
            case EffigyElement.Cold:
                return new Color(0.2f, 0.6f, 1f); // Blue
            case EffigyElement.Lightning:
                return new Color(1f, 0.9f, 0.2f); // Yellow
            case EffigyElement.Physical:
                return new Color(0.7f, 0.7f, 0.7f); // Grey
            case EffigyElement.Chaos:
                return new Color(0.8f, 0.2f, 0.8f); // Purple
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// Get maximum affix slots based on rarity
    /// </summary>
    public int GetMaxAffixSlots()
    {
        if (isUnique)
            return -1;
        return ExplicitAffixTarget;
    }
    
    /// <summary>
    /// Check if a specific grid cell (x, y) is occupied by this effigy's shape
    /// </summary>
    public bool IsCellOccupied(int x, int y)
    {
        if (x < 0 || x >= shapeWidth || y < 0 || y >= shapeHeight)
            return false;
        
        int index = y * shapeWidth + x;
        if (index < 0 || index >= shapeMask.Length)
            return false;
        
        return shapeMask[index];
    }
    
    /// <summary>
    /// Get all occupied cell positions relative to the effigy's origin
    /// </summary>
    public List<Vector2Int> GetOccupiedCells()
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        
        for (int y = 0; y < shapeHeight; y++)
        {
            for (int x = 0; x < shapeWidth; x++)
            {
                if (IsCellOccupied(x, y))
                {
                    cells.Add(new Vector2Int(x, y));
                }
            }
        }
        
        return cells;
    }
    
    /// <summary>
    /// Validate that the shape mask matches the dimensions
    /// </summary>
    private void OnValidate()
    {
        int expectedSize = shapeWidth * shapeHeight;
        
        if (shapeMask == null || shapeMask.Length != expectedSize)
        {
            bool[] newMask = new bool[expectedSize];
            
            // Copy existing data if possible
            if (shapeMask != null)
            {
                for (int i = 0; i < Mathf.Min(shapeMask.Length, expectedSize); i++)
                {
                    newMask[i] = shapeMask[i];
                }
            }
            // Default to a single cell in top-left if no shape exists
            else if (expectedSize > 0)
            {
                newMask[0] = true;
            }
            
            shapeMask = newMask;
        }
        
        // Update size tier when shape changes
        UpdateSizeTier();
        
        // Sync itemName with effigyName for BaseItem compatibility
        if (!string.IsNullOrEmpty(displayAlias))
        {
            itemName = displayAlias;
        }
        else if (!string.IsNullOrEmpty(effigyName))
        {
            itemName = effigyName;
        }
    }

    public override bool CanAddPrefix()
    {
        return prefixes.Count + suffixes.Count < 4;
    }

    public override bool CanAddSuffix()
    {
        return prefixes.Count + suffixes.Count < 4;
    }

    public override ItemRarity GetCalculatedRarity()
    {
        if (isUnique)
            return ItemRarity.Unique;

        int totalAffixes = prefixes.Count + suffixes.Count;

        if (totalAffixes == 0)
            return ItemRarity.Normal;
        if (totalAffixes <= 2)
            return ItemRarity.Magic;
        return ItemRarity.Rare;
    }

    public EffigyShapeCategory GetShapeCategory()
    {
        string reference = !string.IsNullOrEmpty(displayAlias) ? displayAlias : effigyName;
        if (string.IsNullOrEmpty(reference))
            reference = name;

        string token = reference.Replace("_", string.Empty).Replace(" ", string.Empty);
        string lower = token.ToLowerInvariant();

        if (lower.Contains("cross"))
            return EffigyShapeCategory.Cross;
        if (lower.Contains("square") || lower.StartsWith("sq"))
            return EffigyShapeCategory.Square;
        if (lower.Contains("smalll"))
            return EffigyShapeCategory.SmallL;
        if (lower.Contains("line"))
            return EffigyShapeCategory.Line;
        if (lower.Contains("single") || lower.Contains("solo"))
            return EffigyShapeCategory.Single;
        if (lower.StartsWith("t"))
            return EffigyShapeCategory.TShape;
        if (lower.StartsWith("z"))
            return EffigyShapeCategory.ZShape;
        if (lower.StartsWith("s"))
            return EffigyShapeCategory.SShape;
        if (lower.StartsWith("l"))
            return EffigyShapeCategory.LShape;

        return EffigyShapeCategory.Unknown;
    }
}


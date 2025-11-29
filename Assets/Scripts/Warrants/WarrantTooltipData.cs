using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight data container describing how a warrant tooltip should be rendered.
/// </summary>
public class WarrantTooltipData
{
    public string title;
    public string subtitle;
    public Sprite icon;
    public WarrantRarity rarity;
    public readonly List<WarrantTooltipSection> sections = new List<WarrantTooltipSection>();
    
    // Notable-specific data (for dedicated Notable display)
    public string notableDisplayName;
    public string notableDescription;
    public readonly List<string> notableModifierNames = new List<string>();
}

/// <summary>
/// Represents a group of lines (affixes/modifiers) in the tooltip. Each warrant can
/// emit one or more sections depending on how the view wants to organize content.
/// </summary>
public class WarrantTooltipSection
{
    public string header;
    public readonly List<WarrantTooltipLine> lines = new List<WarrantTooltipLine>();
}

public class WarrantTooltipLine
{
    public string text;
    public Color color = Color.white;
    public bool emphasize;
}


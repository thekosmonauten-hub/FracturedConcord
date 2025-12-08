using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Binds effigy data to the effigy tooltip prefab.
/// </summary>
public class EffigyTooltipView : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image iconBackground;
    [SerializeField] private Image iconFrame;

    [Header("Stats")]
    [SerializeField] private List<TextMeshProUGUI> baseStatLabels = new List<TextMeshProUGUI>();
    [SerializeField] private TextMeshProUGUI requirementsLabel;

    [Header("Affixes")]
    [SerializeField] private TextMeshProUGUI implicitLabel;
    [SerializeField] private List<TextMeshProUGUI> explicitLabels = new List<TextMeshProUGUI>();

    private bool cached;
    private Effigy currentEffigy;
    private bool lastAltState = false;

    private void Awake()
    {
        CacheUIElements();
    }
    
    private void Update()
    {
        // Check if ALT key state changed while tooltip is active
        if (gameObject.activeSelf && currentEffigy != null)
        {
            bool currentAltState = IsAltKeyHeld();
            if (currentAltState != lastAltState)
            {
                lastAltState = currentAltState;
                Debug.Log($"<color=yellow>[EffigyTooltip] ALT state changed: {currentAltState}</color>");
                // Refresh tooltip with new ALT state
                SetData(currentEffigy);
            }
        }
    }

    public void SetData(Effigy effigy)
    {
        if (effigy == null)
        {
            gameObject.SetActive(false);
            currentEffigy = null;
            return;
        }

        EnsureUIReferencesCached();
        gameObject.SetActive(true);
        
        // Cache current effigy for ALT-key updates
        currentEffigy = effigy;
        
        // Check if ALT key is held
        bool showRanges = IsAltKeyHeld();
        lastAltState = showRanges;

        ItemRarity rarity = effigy.GetCalculatedRarity();
        string displayName = !string.IsNullOrEmpty(effigy.displayAlias)
            ? effigy.displayAlias
            : (!string.IsNullOrEmpty(effigy.effigyName) ? effigy.effigyName : effigy.GetDisplayName());

        if (nameLabel != null)
        {
            nameLabel.text = TooltipFormattingUtils.ColorizeByRarity(displayName, rarity);
        }

        if (iconImage != null)
        {
            Sprite sprite = effigy.icon != null ? effigy.icon : effigy.itemIcon;
            iconImage.sprite = sprite;
            iconImage.enabled = sprite != null;
        }

        if (iconFrame != null)
        {
            iconFrame.color = effigy.GetElementColor();
        }

        ApplyBaseStats(effigy);
        ApplyRequirements(effigy);
        ApplyAffixes(implicitLabel, effigy.implicitModifiers, showRanges, isImplicit: true);

        var explicitAffixes = new List<Affix>();
        if (effigy.prefixes != null)
            explicitAffixes.AddRange(effigy.prefixes);
        if (effigy.suffixes != null)
            explicitAffixes.AddRange(effigy.suffixes);

        SetAffixTexts(explicitLabels, explicitAffixes, showRanges);
    }

    private void ApplyBaseStats(Effigy effigy)
    {
        var lines = new List<string>
        {
            $"Element: {effigy.element}",
            $"Shape: {effigy.GetShapeCategory()} ({effigy.shapeWidth}x{effigy.shapeHeight}, {effigy.GetCellCount()} cells)",
            $"Tier: {effigy.sizeTier}"
        };

        if (!string.IsNullOrWhiteSpace(effigy.description))
        {
            lines.Add(effigy.description);
        }

        for (int i = 0; i < baseStatLabels.Count; i++)
        {
            var label = baseStatLabels[i];
            if (label == null)
                continue;

            if (i < lines.Count)
            {
                label.text = lines[i];
                label.gameObject.SetActive(true);
            }
            else
            {
                label.gameObject.SetActive(false);
            }
        }
    }

    private void ApplyRequirements(Effigy effigy)
    {
        if (requirementsLabel == null)
            return;

        string text = TooltipFormattingUtils.FormatRequirements(effigy.requiredLevel, 0, 0, 0);
        requirementsLabel.text = $"Requirements:\n{text}";
        requirementsLabel.gameObject.SetActive(true);
    }

    private void ApplyAffixes(TextMeshProUGUI target, List<Affix> affixes, bool showRanges = false, bool isImplicit = false)
    {
        if (target == null)
            return;

        // Deduplicate affixes by content (name + description) to prevent duplicates
        // This handles cases where the same affix appears as different instances
        var uniqueAffixes = new List<Affix>();
        if (affixes != null)
        {
            var seen = new HashSet<string>();
            foreach (var affix in affixes)
            {
                if (affix == null)
                    continue;
                
                // Create a unique key based on affix name and description
                string key = $"{affix.name ?? ""}|{affix.description ?? ""}";
                
                // Also check modifiers to ensure we're not duplicating the same affix
                if (affix.modifiers != null && affix.modifiers.Count > 0)
                {
                    var modifierKeys = affix.modifiers
                        .Where(m => m != null)
                        .Select(m => $"{m.statName}:{m.minValue}-{m.maxValue}")
                        .OrderBy(s => s);
                    key += "|" + string.Join(",", modifierKeys);
                }
                
                if (!seen.Contains(key))
                {
                    seen.Add(key);
                    uniqueAffixes.Add(affix);
                }
            }
        }
        
        string text = BuildAffixBlock(uniqueAffixes, showRanges, isImplicit);
        bool hasAffix = !string.IsNullOrEmpty(text);
        target.text = hasAffix ? text : "None";
        target.gameObject.SetActive(true);
    }

    private string BuildAffixBlock(List<Affix> affixes, bool showRanges = false, bool isImplicit = false)
    {
        if (affixes == null || affixes.Count == 0)
            return string.Empty;

        // If ALT is held, show individual affixes with rolled values (no combining)
        if (showRanges)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < affixes.Count; i++)
            {
                string formatted = FormatAffixWithRolledValue(affixes[i]);
                if (string.IsNullOrEmpty(formatted))
                    continue;

                if (builder.Length > 0)
                    builder.Append('\n');

                builder.Append(formatted);
            }
            return builder.ToString();
        }
        
        // Otherwise, combine duplicate stats
        return BuildCombinedAffixBlock(affixes, isImplicit);
    }
    
    /// <summary>
    /// Build affix block with combined duplicate stats
    /// </summary>
    private string BuildCombinedAffixBlock(List<Affix> affixes, bool isImplicit = false)
    {
        // First check if any affix uses "AllAttributes" stat name directly (new approach)
        foreach (var affix in affixes ?? new List<Affix>())
        {
            if (affix?.modifiers != null)
            {
                foreach (var modifier in affix.modifiers)
                {
                    if (modifier != null && modifier.statName == "AllAttributes")
                    {
                        // Found "AllAttributes" stat - use it directly
                        float value = modifier.isRolled ? modifier.rolledValue : modifier.minValue;
                        string description = modifier.description ?? affix.description ?? $"+{value:F0} to All Attributes";
                        // Clean up description to avoid duplication
                        description = CleanAllAttributesDescription(description, value);
                        return TooltipFormattingUtils.ColorizeImplicit(description);
                    }
                }
            }
        }
        
        // For implicits with a single affix, check if it's "All Attributes" first
        // This prevents single affixes with multiple modifiers (like "Elusive Pattern") from being displayed multiple times
        // BUT we still want to detect and format "All Attributes" correctly
        if (isImplicit && affixes != null && affixes.Count == 1)
        {
            var affix = affixes[0];
            if (affix != null)
            {
                // Check if this is an "All Attributes" affix (old approach with three separate modifiers)
                var detectedAllAttributes = DetectAllAttributesAffixes(affixes);
                if (detectedAllAttributes.Count > 0)
                {
                    // Display as "All Attributes"
                    float value = GetTotalAllAttributesValue(detectedAllAttributes);
                    return CreateAllAttributesDescription(detectedAllAttributes, value);
                }
                
                // Not "All Attributes", display normally
                string description = TooltipFormattingUtils.GetRolledAffixDescription(affix) ?? affix.description ?? "";
                return TooltipFormattingUtils.ColorizeImplicit(description);
            }
        }
        
        // First, detect affixes that give all three attributes with the same value
        // This includes both single affixes with all three modifiers AND multiple affixes with matching values
        var allAttributesAffixes = DetectAllAttributesAffixes(affixes);
        
        // Also check if we have separate affixes for each attribute with the same value
        // (e.g., one affix for Strength, one for Dexterity, one for Intelligence, all with value +7)
        if (allAttributesAffixes.Count == 0)
        {
            allAttributesAffixes = DetectSeparateAllAttributesAffixes(affixes);
        }
        
        // Group affixes by stat name (excluding attributes that are part of "All Attributes")
        var grouped = CombineAffixesByStat(affixes, allAttributesAffixes);
        
        // If we found "All Attributes" affixes, add them as a single combined entry
        if (allAttributesAffixes.Count > 0)
        {
            // Calculate the total value (handles both "AllAttributes" stat name and three separate attributes)
            float totalValue = GetTotalAllAttributesValue(allAttributesAffixes);
            grouped["AllAttributes"] = allAttributesAffixes;
        }
        
        var builder = new StringBuilder();
        foreach (var group in grouped.OrderBy(g => g.Key))
        {
            if (builder.Length > 0)
                builder.Append('\n');
            
            // Special handling for AllAttributes
            if (group.Key == "AllAttributes")
            {
                builder.Append(CreateAllAttributesDescription(group.Value, GetTotalAllAttributesValue(group.Value)));
            }
            else
            {
                builder.Append(CreateCombinedAffixDescription(group.Key, group.Value, isImplicit));
            }
        }
        
        return builder.ToString();
    }
    
    /// <summary>
    /// Detect affixes that give all three attributes (Strength, Dexterity, Intelligence) with the same value
    /// </summary>
    private List<Affix> DetectAllAttributesAffixes(List<Affix> affixes)
    {
        var allAttributesAffixes = new List<Affix>();
        
        foreach (var affix in affixes)
        {
            if (affix?.modifiers == null)
                continue;
            
            // Check if this affix has modifiers for all three attributes
            float? strValue = null;
            float? dexValue = null;
            float? intValue = null;
            
            foreach (var modifier in affix.modifiers)
            {
                if (modifier == null)
                    continue;
                
                float value = modifier.isRolled ? modifier.rolledValue : modifier.minValue;
                
                if (modifier.statName == "Strength")
                    strValue = value;
                else if (modifier.statName == "Dexterity")
                    dexValue = value;
                else if (modifier.statName == "Intelligence")
                    intValue = value;
            }
            
            // If all three attributes are present and have the same value (within tolerance)
            if (strValue.HasValue && dexValue.HasValue && intValue.HasValue)
            {
                if (Mathf.Abs(strValue.Value - dexValue.Value) < 0.1f && 
                    Mathf.Abs(strValue.Value - intValue.Value) < 0.1f)
                {
                    allAttributesAffixes.Add(affix);
                }
            }
        }
        
        return allAttributesAffixes;
    }
    
    /// <summary>
    /// Detect separate affixes that each give one attribute (Strength, Dexterity, Intelligence) with the same value
    /// This handles the case where we have 3 separate affixes instead of 1 affix with 3 modifiers
    /// </summary>
    private List<Affix> DetectSeparateAllAttributesAffixes(List<Affix> affixes)
    {
        var strAffixes = new List<Affix>();
        var dexAffixes = new List<Affix>();
        var intAffixes = new List<Affix>();
        
        // Group affixes by which attribute they modify
        foreach (var affix in affixes)
        {
            if (affix?.modifiers == null)
                continue;
            
            bool hasStr = false;
            bool hasDex = false;
            bool hasInt = false;
            float? strValue = null;
            float? dexValue = null;
            float? intValue = null;
            
            foreach (var modifier in affix.modifiers)
            {
                if (modifier == null)
                    continue;
                
                float value = modifier.isRolled ? modifier.rolledValue : modifier.minValue;
                
                if (modifier.statName == "Strength")
                {
                    hasStr = true;
                    strValue = value;
                }
                else if (modifier.statName == "Dexterity")
                {
                    hasDex = true;
                    dexValue = value;
                }
                else if (modifier.statName == "Intelligence")
                {
                    hasInt = true;
                    intValue = value;
                }
            }
            
            // Only consider affixes that modify exactly one attribute (not multiple)
            if (hasStr && !hasDex && !hasInt && strValue.HasValue)
            {
                strAffixes.Add(affix);
            }
            else if (hasDex && !hasStr && !hasInt && dexValue.HasValue)
            {
                dexAffixes.Add(affix);
            }
            else if (hasInt && !hasStr && !hasDex && intValue.HasValue)
            {
                intAffixes.Add(affix);
            }
        }
        
        // Check if we have at least one affix for each attribute
        if (strAffixes.Count > 0 && dexAffixes.Count > 0 && intAffixes.Count > 0)
        {
            // Get the values from the first affix of each type
            float strVal = GetTotalStatValue("Strength", strAffixes);
            float dexVal = GetTotalStatValue("Dexterity", dexAffixes);
            float intVal = GetTotalStatValue("Intelligence", intAffixes);
            
            // If all three have the same value (within tolerance), combine them
            if (Mathf.Abs(strVal - dexVal) < 0.1f && Mathf.Abs(strVal - intVal) < 0.1f)
            {
                var combined = new List<Affix>();
                combined.AddRange(strAffixes);
                combined.AddRange(dexAffixes);
                combined.AddRange(intAffixes);
                return combined;
            }
        }
        
        return new List<Affix>();
    }
    
    /// <summary>
    /// Get total stat value from a list of affixes for a specific stat name
    /// </summary>
    private float GetTotalStatValue(string statName, List<Affix> affixes)
    {
        float total = 0f;
        foreach (var affix in affixes)
        {
            if (affix?.modifiers == null)
                continue;
            
            foreach (var modifier in affix.modifiers)
            {
                if (modifier != null && modifier.statName == statName)
                {
                    total += modifier.isRolled ? modifier.rolledValue : modifier.minValue;
                }
            }
        }
        return total;
    }
    
    /// <summary>
    /// Get total value for "AllAttributes" stat (handles both "AllAttributes" stat name and three separate attributes)
    /// </summary>
    private float GetTotalAllAttributesValue(List<Affix> affixes)
    {
        float total = 0f;
        foreach (var affix in affixes)
        {
            if (affix?.modifiers == null)
                continue;
            
            // First check for "AllAttributes" stat name directly
            foreach (var modifier in affix.modifiers)
            {
                if (modifier != null && modifier.statName == "AllAttributes")
                {
                    total += modifier.isRolled ? modifier.rolledValue : modifier.minValue;
                    break; // Only count once per affix
                }
            }
            
            // If not found, check for Strength (they should all be the same value)
            if (total == 0f)
            {
                foreach (var modifier in affix.modifiers)
                {
                    if (modifier != null && modifier.statName == "Strength")
                    {
                        total += modifier.isRolled ? modifier.rolledValue : modifier.minValue;
                        break; // Only count once per affix
                    }
                }
            }
        }
        return total;
    }
    
    /// <summary>
    /// Clean up "All Attributes" description to avoid duplication like "+#+#"
    /// </summary>
    private string CleanAllAttributesDescription(string description, float value)
    {
        if (string.IsNullOrEmpty(description))
            return $"+{value:F0} to All Attributes";
        
        // Remove any existing value patterns and replace with single value
        // Pattern matches: +#, +#-#, +#+#, etc.
        string pattern = @"[+-]?\d+(?:\.\d+)?(?:[-–—]\d+(?:\.\d+)?)?";
        string cleaned = Regex.Replace(description, pattern, $"+{value:F0}", RegexOptions.None, System.TimeSpan.FromMilliseconds(100));
        
        // Replace attribute names with "All Attributes" if present
        cleaned = Regex.Replace(cleaned, @"\b(Strength|Dexterity|Intelligence)\b", "All Attributes", RegexOptions.IgnoreCase);
        
        // Remove duplicate "All Attributes" if it appears multiple times
        cleaned = Regex.Replace(cleaned, @"\bAll Attributes\b(?=.*\bAll Attributes\b)", "", RegexOptions.IgnoreCase);
        cleaned = Regex.Replace(cleaned, @"\s+", " "); // Clean up extra spaces
        
        return cleaned.Trim();
    }
    
    /// <summary>
    /// Create description for "All Attributes" modifier
    /// </summary>
    private string CreateAllAttributesDescription(List<Affix> affixes, float value)
    {
        if (affixes == null || affixes.Count == 0)
            return string.Empty;
        
        // Get description template from first affix, but replace attribute names with "All Attributes"
        // When not holding ALT, use rolled description (no affix name/tier)
        string template = TooltipFormattingUtils.GetRolledAffixDescription(affixes[0]) ?? affixes[0].description ?? "";
        
        // Clean the description to avoid duplication
        string cleaned = CleanAllAttributesDescription(template, value);
        
        // If multiple sources, add count
        if (affixes.Count > 1)
        {
            cleaned += $" ({affixes.Count} sources)";
        }
        
        return TooltipFormattingUtils.ColorizePrefix(cleaned);
    }
    
    /// <summary>
    /// Group affixes by their stat names (from modifiers)
    /// Excludes affixes that are part of "All Attributes" from individual attribute grouping
    /// </summary>
    private Dictionary<string, List<Affix>> CombineAffixesByStat(List<Affix> affixes, List<Affix> excludeAffixes = null)
    {
        var grouped = new Dictionary<string, List<Affix>>();
        var excludeSet = excludeAffixes != null ? new HashSet<Affix>(excludeAffixes) : new HashSet<Affix>();
        
        foreach (var affix in affixes)
        {
            if (affix == null || affix.modifiers == null || affix.modifiers.Count == 0)
                continue;
            
            // Skip affixes that are part of "All Attributes"
            if (excludeSet.Contains(affix))
                continue;
            
            // For each modifier in the affix, group by stat name
            foreach (var modifier in affix.modifiers)
            {
                if (modifier == null || string.IsNullOrEmpty(modifier.statName))
                    continue;
                
                string statName = modifier.statName;
                
                // If this affix uses "AllAttributes" stat name directly, group it as "AllAttributes"
                if (statName == "AllAttributes")
                {
                    if (!grouped.ContainsKey("AllAttributes"))
                    {
                        grouped["AllAttributes"] = new List<Affix>();
                    }
                    if (!grouped["AllAttributes"].Contains(affix))
                    {
                        grouped["AllAttributes"].Add(affix);
                    }
                    continue; // Don't process individual attributes if using AllAttributes stat
                }
                
                // Skip individual attributes if this affix is part of "All Attributes"
                if (statName == "Strength" || statName == "Dexterity" || statName == "Intelligence")
                {
                    if (excludeSet.Contains(affix))
                        continue;
                }
                
                if (!grouped.ContainsKey(statName))
                {
                    grouped[statName] = new List<Affix>();
                }
                
                // Only add if not already in the list (avoid duplicates from same affix)
                if (!grouped[statName].Contains(affix))
                {
                    grouped[statName].Add(affix);
                }
            }
        }
        
        return grouped;
    }
    
    /// <summary>
    /// Create a combined description for multiple affixes affecting the same stat
    /// </summary>
    private string CreateCombinedAffixDescription(string statName, List<Affix> affixes, bool isImplicit = false)
    {
        if (affixes == null || affixes.Count == 0)
            return string.Empty;
        
        // If only one affix, just format it normally (without name/tier when not holding ALT)
        if (affixes.Count == 1)
        {
            // When not holding ALT, show only the description (no affix name/tier)
            string description = TooltipFormattingUtils.GetRolledAffixDescription(affixes[0]);
            // Use implicit color if this is an implicit, otherwise prefix color
            return isImplicit ? TooltipFormattingUtils.ColorizeImplicit(description) : TooltipFormattingUtils.ColorizePrefix(description);
        }
        
        // Multiple affixes - sum up the values and create combined description
        float totalValue = 0f;
        ModifierType? modifierType = null;
        string combinedDesc = string.Empty;
        
        foreach (var affix in affixes)
        {
            if (affix?.modifiers == null)
                continue;
            
            foreach (var modifier in affix.modifiers)
            {
                if (modifier == null || modifier.statName != statName)
                    continue;
                
                // Get rolled value
                float value = modifier.isRolled ? modifier.rolledValue : modifier.minValue;
                totalValue += value;
                
                // Store modifier type (should be consistent across all)
                if (!modifierType.HasValue)
                {
                    modifierType = modifier.modifierType;
                }
                
                // Get description from first affix for template
                if (string.IsNullOrEmpty(combinedDesc))
                {
                    combinedDesc = TooltipFormattingUtils.GetRolledAffixDescription(affix);
                }
            }
        }
        
        if (string.IsNullOrEmpty(combinedDesc))
        {
            // Fallback: just show combined value
            string fallback = $"+{totalValue:F1} {statName} ({affixes.Count} sources)";
            return TooltipFormattingUtils.ColorizePrefix(fallback);
        }
        
        // Extract the stat description part (e.g., "+X% increased Fire Damage")
        // Try to find a pattern like "+X" or "+X%" and replace with combined value
        string pattern = @"([+-]?)(\d+(?:\.\d+)?)(%?)";
        Match match = Regex.Match(combinedDesc, pattern);
        
        if (match.Success)
        {
            string sign = match.Groups[1].Value;
            if (string.IsNullOrEmpty(sign) && totalValue >= 0)
                sign = "+";
            
            string percent = match.Groups[3].Value;
            
            // Format the combined value
            string combinedValue;
            if (modifierType == ModifierType.Increased || modifierType == ModifierType.More)
            {
                combinedValue = totalValue.ToString("F1");
            }
            else
            {
                combinedValue = totalValue.ToString("F0");
            }
            
            // Replace first occurrence with combined value
            string newDesc = Regex.Replace(combinedDesc, pattern, 
                $"{sign}{combinedValue}{percent}", 
                RegexOptions.None, 
                System.TimeSpan.FromMilliseconds(100));
            
            // Add source count
            newDesc += $" ({affixes.Count} sources)";
            
            // Apply color (use prefix color as default)
            return TooltipFormattingUtils.ColorizePrefix(newDesc);
        }
        
        // Fallback: append combined value
        string fallbackDesc = $"{combinedDesc} (Total: {totalValue:F1}, {affixes.Count} sources)";
        return TooltipFormattingUtils.ColorizePrefix(fallbackDesc);
    }
    
    private void SetAffixTexts(IEnumerable<TextMeshProUGUI> targets, IList<Affix> affixes, bool showRanges = false)
    {
        if (targets == null)
            return;

        // If ALT is held, show individual affixes with rolled values
        if (showRanges)
        {
            // When ALT is held, we want to show individual affixes, but for combined stats,
            // we need to show all the affixes that contribute to that stat
            var affixList = affixes?.ToList() ?? new List<Affix>();
            
            // Build a list of all individual affixes to display
            var displayList = new List<string>();
            
            foreach (var affix in affixList)
            {
                if (affix == null)
                    continue;
                
                string formatted = FormatAffixWithRolledValue(affix);
                if (!string.IsNullOrEmpty(formatted))
                {
                    displayList.Add(formatted);
                }
            }
            
            int index = 0;
            foreach (var label in targets)
            {
                if (label == null)
                    continue;

                if (displayList != null && index < displayList.Count)
                {
                    label.text = displayList[index];
                    label.gameObject.SetActive(true);
                }
                else
                {
                    label.gameObject.SetActive(false);
                }

                index++;
            }
        }
        else
        {
            // Combine affixes by stat
            var affixList = affixes?.ToList() ?? new List<Affix>();
            
            // Detect "All Attributes" affixes first
            var allAttributesAffixes = DetectAllAttributesAffixes(affixList);
            var grouped = CombineAffixesByStat(affixList, allAttributesAffixes);
            
            // If we found "All Attributes" affixes, add them as a single combined entry
            if (allAttributesAffixes.Count > 0)
            {
                grouped["AllAttributes"] = allAttributesAffixes;
            }
            
            var combinedList = grouped.Select(g => 
                g.Key == "AllAttributes" 
                    ? CreateAllAttributesDescription(g.Value, GetTotalAllAttributesValue(g.Value))
                    : CreateCombinedAffixDescription(g.Key, g.Value)
            ).ToList();
            
            int index = 0;
            foreach (var label in targets)
            {
                if (label == null)
                    continue;

                if (combinedList != null && index < combinedList.Count)
                {
                    label.text = combinedList[index];
                    label.gameObject.SetActive(true);
                }
                else
                {
                    label.gameObject.SetActive(false);
                }

                index++;
            }
        }
    }

    private void CacheUIElements()
    {
        if (cached)
            return;

        cached = true;

        nameLabel ??= FindLabel("Header/EffigyName");
        iconImage ??= FindImage("Content/IconContainer/WeaponIcon");
        iconBackground ??= FindImage("Content/IconContainer/IconBackground");
        iconFrame ??= FindImage("Content/IconContainer/ImageFrame");

        var statsContainer = transform.Find("Content/BaseWeaponStats");
        baseStatLabels ??= new List<TextMeshProUGUI>();
        baseStatLabels.Clear();

        if (statsContainer != null)
        {
            foreach (Transform child in statsContainer)
            {
                var label = child.GetComponent<TextMeshProUGUI>();
                if (label == null)
                    continue;

                if (child.name == "Requirements")
                {
                    requirementsLabel ??= label;
                }
                else
                {
                    baseStatLabels.Add(label);
                }
            }
        }

        implicitLabel ??= FindLabel("Content/AffixContainer/Implicit/ImplicitModifider");

        if (explicitLabels == null || explicitLabels.Count == 0)
        {
            explicitLabels = CollectChildLabels("Content/AffixContainer/Affixes", "Affix");
        }
    }

    private void EnsureUIReferencesCached()
    {
        if (!cached)
        {
            CacheUIElements();
        }
    }

    private TextMeshProUGUI FindLabel(string path)
    {
        Transform target = transform.Find(path);
        return target ? target.GetComponent<TextMeshProUGUI>() : null;
    }

    private Image FindImage(string path)
    {
        Transform target = transform.Find(path);
        return target ? target.GetComponent<Image>() : null;
    }

    private List<TextMeshProUGUI> CollectChildLabels(string path, string namePrefix = null, int startIndex = 0)
    {
        var list = new List<TextMeshProUGUI>();
        Transform parent = transform.Find(path);
        if (parent == null)
            return list;

        int currentIndex = 0;
        foreach (Transform child in parent)
        {
            if (!string.IsNullOrEmpty(namePrefix) && !child.name.StartsWith(namePrefix))
                continue;

            if (currentIndex++ < startIndex)
                continue;

            var label = child.GetComponent<TextMeshProUGUI>();
            if (label != null)
                list.Add(label);
        }

        return list;
    }
    
    /// <summary>
    /// Format an affix showing rolled value in parentheses before the range (for ALT view)
    /// Example: "ROBUST (Tier10): (3) +3-5%"
    /// </summary>
    private string FormatAffixWithRolledValue(Affix affix)
    {
        if (affix == null)
            return string.Empty;
        
        // Get the rolled description (shows ranges)
        string description = affix.description;
        
        // Extract rolled values from modifiers and add them in parentheses
        if (affix.modifiers != null && affix.modifiers.Count > 0)
        {
            var rolledValues = new List<string>();
            foreach (var modifier in affix.modifiers)
            {
                if (modifier != null && modifier.isRolled)
                {
                    string rolledStr = modifier.rolledValue.ToString("F0");
                    rolledValues.Add(rolledStr);
                }
            }
            
            // If we have rolled values, prepend them to the description
            if (rolledValues.Count > 0)
            {
                string rolledPart = $"({string.Join(", ", rolledValues)}) ";
                description = rolledPart + description;
            }
        }
        
        // Format with affix name only (no tier when holding ALT)
        // Remove any existing tier info from description to avoid duplication
        string cleanDescription = Regex.Replace(description, @"\s*\(TIER\d+\)\s*", "", RegexOptions.IgnoreCase);
        
        string formatted = !string.IsNullOrEmpty(affix.name) 
            ? $"{affix.name.ToUpper()}: {cleanDescription}" 
            : cleanDescription;
        
        // Apply color based on affix type
        switch (affix.affixType)
        {
            case AffixType.Prefix:
                return TooltipFormattingUtils.ColorizePrefix(formatted);
            case AffixType.Suffix:
                return TooltipFormattingUtils.ColorizeSuffix(formatted);
            default:
                return formatted;
        }
    }
    
    /// <summary>
    /// Check if ALT key is held (supports both old and new Input System)
    /// </summary>
    private bool IsAltKeyHeld()
    {
#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            bool isPressed = keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed;
            return isPressed;
        }
        return false;
#else
        return UnityEngine.Input.GetKey(KeyCode.LeftAlt) || UnityEngine.Input.GetKey(KeyCode.RightAlt);
#endif
    }
}



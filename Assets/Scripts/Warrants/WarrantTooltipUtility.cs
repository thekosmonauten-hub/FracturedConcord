using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Helper methods for converting warrant data into tooltip-friendly structures.
/// Centralizes the logic so both socket and effect nodes render consistent output.
/// </summary>
public static class WarrantTooltipUtility
{
    private const int MaxLinesPerSection = 8;
    private static readonly Color MultiTwoColor = new Color(0.3f, 0.6f, 1f); // blue
    private static readonly Color MultiThreeColor = new Color(1f, 0.85f, 0.2f); // yellow
    private static readonly Color SpecialColor = new Color(1f, 0.55f, 0.15f); // orange

    public static WarrantTooltipData BuildSingleWarrantData(WarrantDefinition definition, string subtitle = null, WarrantNotableDatabase notableDatabase = null)
    {
        if (definition == null)
            return null;

        var data = new WarrantTooltipData
        {
            title = string.IsNullOrWhiteSpace(definition.displayName) ? definition.warrantId : definition.displayName,
            subtitle = subtitle,
            icon = definition.icon,
            rarity = definition.rarity
        };

		// Separate socket-only modifiers (Notable) from regular modifiers
		var regularModifiers = new List<WarrantModifier>();
		var notableModifiers = new List<WarrantModifier>();
		
		if (definition.modifiers != null)
		{
			foreach (var mod in definition.modifiers)
			{
				if (mod == null) continue;
				
				// Check if this is a socket-only modifier (Notable)
				// Check both modifierId and displayName for the prefix to catch all cases
				bool isSocketOnly = (!string.IsNullOrWhiteSpace(mod.modifierId) && mod.modifierId.StartsWith("__SOCKET_ONLY__")) ||
				                    (!string.IsNullOrWhiteSpace(mod.displayName) && mod.displayName.Contains("(Socket Only)"));
				
				if (isSocketOnly)
				{
					notableModifiers.Add(mod);
				}
				else
				{
					regularModifiers.Add(mod);
				}
			}
		}

		// Core modifier section (regular modifiers that spill to effect nodes)
		// Only show this section if there are actual regular modifiers (not Notable modifiers)
		if (regularModifiers.Count > 0)
		{
			var section = new WarrantTooltipSection
			{
				header = "Modifiers"
			};
			FillSectionWithModifiers(section, regularModifiers);
			data.sections.Add(section);
		}
		
		// Notable section (socket-only) - check both old and new systems
		WarrantNotableDatabase.NotableEntry notableEntry = null;
		string notableDisplayName = null;
		string notableDescription = null;
		
		// New system: Look up Notable from database using notableId
		if (!string.IsNullOrWhiteSpace(definition.notableId) && notableDatabase != null)
		{
			notableEntry = notableDatabase.GetById(definition.notableId);
			if (notableEntry != null)
			{
				notableDisplayName = notableEntry.displayName;
				notableDescription = notableEntry.description;
				
				// Collect modifiers from the Notable database entry
				if (notableEntry.modifiers != null)
				{
					foreach (var notableMod in notableEntry.modifiers)
					{
						if (notableMod == null || string.IsNullOrWhiteSpace(notableMod.statKey))
							continue;
						
						// Convert NotableModifier to WarrantModifier for consistency
						var warrantMod = new WarrantModifier
						{
							modifierId = $"__SOCKET_ONLY__{notableMod.statKey}",
							displayName = !string.IsNullOrWhiteSpace(notableMod.displayName) 
							? notableMod.displayName 
							: $"{Mathf.RoundToInt(notableMod.value):+0;-0;0}% {notableMod.statKey}",
							operation = WarrantModifierOperation.Additive,
							value = notableMod.value,
							description = !string.IsNullOrWhiteSpace(notableMod.displayName) 
								? notableMod.displayName 
								: $"{Mathf.RoundToInt(notableMod.value):+0;-0;0}% {notableMod.statKey}"
						};
						
						// Check if this modifier is already in notableModifiers (from definition.modifiers)
						bool alreadyAdded = notableModifiers.Any(m => m != null && 
							(m.modifierId == warrantMod.modifierId || 
							 m.displayName == warrantMod.displayName));
						
						if (!alreadyAdded)
						{
							notableModifiers.Add(warrantMod);
						}
						
						// Remove from regularModifiers if it's there (shouldn't happen, but just in case)
						regularModifiers.RemoveAll(m => m != null && 
							(m.modifierId == warrantMod.modifierId || 
							 m.displayName == warrantMod.displayName));
					}
				}
			}
		}
		
		// Old system: Use WarrantNotableDefinition ScriptableObject
		if (definition.notable != null)
		{
			notableDisplayName = definition.notable.displayName;
			notableDescription = definition.notable.description;
			
			// Add modifiers from old system if not already added from new system
			// Also remove them from regularModifiers if they somehow ended up there
			if (definition.notable.modifiers != null)
			{
				foreach (var notableMod in definition.notable.modifiers)
				{
					if (notableMod == null) continue;
					
					// Check if this modifier is already in notableModifiers
					bool alreadyAdded = notableModifiers.Any(m => m != null && 
						(m.modifierId == notableMod.modifierId || 
						 m.displayName == notableMod.displayName));
					
					if (!alreadyAdded)
					{
						notableModifiers.Add(notableMod);
					}
					
					// Remove from regularModifiers if it's there (shouldn't happen, but just in case)
					regularModifiers.RemoveAll(m => m != null && 
						(m.modifierId == notableMod.modifierId || 
						 m.displayName == notableMod.displayName));
				}
			}
		}
		
		// Populate Notable-specific fields for dedicated display
		data.notableDisplayName = notableDisplayName;
		data.notableDescription = notableDescription;
		data.notableModifierNames.Clear();
		
		// Collect modifier display names - use description if displayName is empty
		foreach (var mod in notableModifiers)
		{
			if (mod == null) continue;
			
			string displayName = null;
			
			// Priority 1: Use displayName if available (this is set from the original stat string in the importer)
			if (!string.IsNullOrWhiteSpace(mod.displayName))
			{
				displayName = mod.displayName;
			}
			// Priority 2: Use description if available
			else if (!string.IsNullOrWhiteSpace(mod.description))
			{
				displayName = mod.description;
			}
			// Priority 3: Format from modifierId and value
			else
			{
				displayName = FormatModifierText(mod, mod.value);
			}
			
			if (!string.IsNullOrWhiteSpace(displayName))
			{
				data.notableModifierNames.Add(displayName);
			}
		}
		
		// If Notable description is empty but we have modifier names, append them to description
		if (string.IsNullOrWhiteSpace(data.notableDescription) && data.notableModifierNames.Count > 0)
		{
			data.notableDescription = string.Join("\n", data.notableModifierNames);
		}
		
		// Show Notable section ONLY if we have Notable modifiers
		// Don't show an empty Notable section
		if (notableModifiers.Count > 0)
		{
			var notableSection = new WarrantTooltipSection
			{
				header = string.IsNullOrWhiteSpace(notableDisplayName)
					? "Notable (Socket Only)"
					: $"Notable: {notableDisplayName} (Socket Only)"
			};
			
			// Show description if available
			if (!string.IsNullOrWhiteSpace(notableDescription))
			{
				notableSection.lines.Add(new WarrantTooltipLine
				{
					text = notableDescription,
					color = Color.white
				});
			}

			// Show Notable modifiers (should always have some if we're showing this section)
			if (notableModifiers.Count > 0)
			{
				// Use display names from notableModifierNames if available, otherwise use FillSectionWithModifiers
				if (data.notableModifierNames.Count > 0)
				{
					// Use the collected display names directly
					foreach (var modifierName in data.notableModifierNames)
					{
						if (string.IsNullOrWhiteSpace(modifierName))
							continue;
						
						notableSection.lines.Add(new WarrantTooltipLine
						{
							text = modifierName,
							color = Color.white
						});
					}
				}
				else
				{
					// Fallback to standard formatting
					FillSectionWithModifiers(notableSection, notableModifiers);
				}
			}
			
			data.sections.Add(notableSection);
		}
		
        return data;
    }

    public static WarrantTooltipData BuildCombinedData(string title, IEnumerable<WarrantDefinition> definitions, string subtitle = null)
    {
        if (definitions == null)
            return null;

        var defList = definitions.Where(d => d != null).ToList();
        if (defList.Count == 0)
            return null;

        var data = new WarrantTooltipData
        {
            title = title,
            subtitle = subtitle,
            rarity = defList.Max(d => d.rarity)
        };

        var section = new WarrantTooltipSection
        {
            header = string.Empty // Warrant names are now shown in the title, so no need to repeat here
        };

        FillSectionWithAggregatedModifiers(section, defList);
        data.sections.Add(section);
        return data;
    }

    private static void FillSectionWithModifiers(WarrantTooltipSection section, IList<WarrantModifier> modifiers)
    {
        section.lines.Clear();

        if (modifiers == null || modifiers.Count == 0)
        {
            section.lines.Add(new WarrantTooltipLine { text = "No modifiers configured." });
            return;
        }

        for (int i = 0; i < modifiers.Count; i++)
        {
            if (i >= MaxLinesPerSection)
            {
                section.lines.Add(new WarrantTooltipLine { text = "...and more" });
                break;
            }

            var modifier = modifiers[i];
            if (modifier == null)
                continue;

            section.lines.Add(new WarrantTooltipLine
            {
                text = FormatModifierText(modifier, modifier.value),
                color = Color.white
            });
        }
    }

    private static void FillSectionWithAggregatedModifiers(WarrantTooltipSection section, List<WarrantDefinition> definitions)
    {
        section.lines.Clear();

        if (definitions == null || definitions.Count == 0)
        {
            section.lines.Add(new WarrantTooltipLine { text = "No modifiers configured." });
            return;
        }

        var grouped = new Dictionary<string, AggregatedModifier>();

        foreach (var definition in definitions)
        {
            if (definition?.modifiers == null)
                continue;

            // Track which modifier IDs this warrant has contributed to (to avoid double-counting)
            var warrantContributedModifiers = new HashSet<string>();

            foreach (var modifier in definition.modifiers)
            {
                if (modifier == null)
                    continue;

                // Skip socket-only modifiers (marked with __SOCKET_ONLY__ prefix)
                // These should only appear on the socket node itself, not on effect nodes
                if (!string.IsNullOrWhiteSpace(modifier.modifierId) && modifier.modifierId.StartsWith("__SOCKET_ONLY__"))
                    continue;

                // Create a robust grouping key that handles cases where modifierId might be empty
                // or displayNames might differ slightly between warrants
                string key = null;
                
                // First, try to use modifierId (most reliable for grouping)
                if (!string.IsNullOrWhiteSpace(modifier.modifierId))
                {
                    // Strip __SOCKET_ONLY__ prefix if present
                    key = modifier.modifierId.StartsWith("__SOCKET_ONLY__") 
                        ? modifier.modifierId.Substring("__SOCKET_ONLY__".Length)
                        : modifier.modifierId;
                }
                
                // If modifierId is empty, use displayName as fallback
                // Normalize it by removing spaces and converting to lowercase for consistent grouping
                if (string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(modifier.displayName))
                {
                    // Remove common prefixes and normalize
                    key = modifier.displayName
                        .Replace("Increased ", "", System.StringComparison.OrdinalIgnoreCase)
                        .Replace("+", "", System.StringComparison.OrdinalIgnoreCase)
                        .Replace("%", "", System.StringComparison.OrdinalIgnoreCase)
                        .Replace(" ", "")
                        .Trim();
                }
                
                // Fallback to hash if both are empty (shouldn't happen)
                if (string.IsNullOrWhiteSpace(key))
                    key = modifier.GetHashCode().ToString();
                
                // Normalize key to lowercase for consistent grouping
                key = key.ToLowerInvariant();

                if (!grouped.TryGetValue(key, out var agg))
                {
                    agg = new AggregatedModifier
                    {
                        baseModifier = modifier
                    };
                    grouped[key] = agg;
                }

                // Sum the values
                agg.totalValue += modifier.value;
                
                // Only count this warrant once per modifier ID (not once per modifier instance)
                if (!warrantContributedModifiers.Contains(key))
                {
                    agg.contributorCount++;
                    warrantContributedModifiers.Add(key);
                }
                
                agg.containsSpecial |= definition.isKeystone;
            }
        }

        if (grouped.Count == 0)
        {
            section.lines.Add(new WarrantTooltipLine { text = "No modifiers configured." });
            return;
        }

        int lineCount = 0;
        foreach (var pair in grouped.Values)
        {
            if (lineCount >= MaxLinesPerSection)
            {
                section.lines.Add(new WarrantTooltipLine { text = "...and more" });
                break;
            }

            var color = GetColorForContributor(pair.contributorCount, pair.containsSpecial);

            section.lines.Add(new WarrantTooltipLine
            {
                text = FormatModifierText(pair.baseModifier, pair.totalValue),
                color = color
            });

            lineCount++;
        }
    }

    private static string FormatModifierText(WarrantModifier modifier, float value)
    {
        if (modifier == null)
            return string.Empty;

        // Strip __SOCKET_ONLY__ prefix from modifierId for display
        string cleanModifierId = modifier.modifierId;
        if (!string.IsNullOrWhiteSpace(cleanModifierId) && cleanModifierId.StartsWith("__SOCKET_ONLY__"))
        {
            cleanModifierId = cleanModifierId.Substring("__SOCKET_ONLY__".Length);
        }

        var label = !string.IsNullOrWhiteSpace(modifier.displayName)
            ? modifier.displayName
            : cleanModifierId;

        if (string.IsNullOrWhiteSpace(label))
        {
            label = "Modifier";
        }

        // Round to whole number for all affixes (all warrants roll whole numbers)
        int intValue = Mathf.RoundToInt(value);

        // If description has {value} placeholder, use it
        if (!string.IsNullOrWhiteSpace(modifier.description) && modifier.description.Contains("{value}"))
        {
            return modifier.description.Replace("{value}", intValue.ToString("0"));
        }

        // Check if this is a flat stat (by checking modifierId for "Flat" prefix or if description doesn't contain "%")
        bool isFlatStat = (!string.IsNullOrWhiteSpace(modifier.modifierId) && modifier.modifierId.StartsWith("Flat", System.StringComparison.OrdinalIgnoreCase)) ||
                          (!string.IsNullOrWhiteSpace(modifier.description) && !modifier.description.Contains("%") && modifier.description.StartsWith("+"));

        // Always format with the provided value (which is the aggregated total when called from aggregation)
        // This ensures aggregated values are displayed correctly even if the base modifier has a hardcoded description
        if (modifier.operation == WarrantModifierOperation.Additive || modifier.operation == WarrantModifierOperation.Multiplicative)
        {
            if (isFlatStat)
            {
                // Format flat stats as whole numbers: "+X" instead of "X%"
                return $"+{intValue} {label}";
            }
            else
            {
                // Format percentage stats as whole numbers: "X%" (no decimals)
                return $"{intValue:+0;-0;0}% {label}";
            }
        }

        return $"{label} ({intValue:+0;-0;0})";
    }

    private static Color GetColorForContributor(int contributors, bool hasSpecial)
    {
        if (hasSpecial)
            return SpecialColor;

        if (contributors >= 3)
            return MultiThreeColor;

        if (contributors >= 2)
            return MultiTwoColor;

        return Color.white;
    }

    private class AggregatedModifier
    {
        public WarrantModifier baseModifier;
        public float totalValue;
        public int contributorCount;
        public bool containsSpecial;
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "WarrantDatabase", menuName = "Dexiled/Warrants/Database", order = 0)]
public class WarrantDatabase : ScriptableObject
{
    [SerializeField] private List<WarrantDefinition> definitions = new List<WarrantDefinition>();
	[SerializeField] private WarrantAffixDatabase affixDatabase;
	[SerializeField] private WarrantNotableDatabase notableDatabase;
	[SerializeField] private WarrantIconLibrary iconLibrary;

	[Header("Rarity Rules (Blueprint)")]
	[Tooltip("Rarity is automatically determined by affix count:\n" +
	         "1 Notable + 1 affix = Common\n" +
	         "1 Notable + 2 affixes = Magic\n" +
	         "1 Notable + 3 affixes = Rare\n" +
	         "1 Notable + 4 affixes = Rare")]
	[SerializeField, TextArea(2, 4)] private string rarityRulesInfo = "Rarity is calculated automatically based on affix count.";

    private Dictionary<string, WarrantDefinition> lookup;

    public IReadOnlyList<WarrantDefinition> Definitions => definitions;

    public IEnumerable<WarrantDefinition> GetAll()
    {
        return definitions?.Where(def => def != null) ?? Enumerable.Empty<WarrantDefinition>();
    }

    public IEnumerable<WarrantDefinition> GetByRarity(WarrantRarity rarity)
    {
        return definitions?.Where(def => def != null && def.rarity == rarity) ?? Enumerable.Empty<WarrantDefinition>();
    }

    public WarrantDefinition GetById(string warrantId)
    {
        if (string.IsNullOrEmpty(warrantId))
            return null;

        EnsureLookup();
        lookup.TryGetValue(warrantId, out var definition);
        return definition;
    }

    public bool Contains(string warrantId)
    {
        if (string.IsNullOrEmpty(warrantId))
            return false;

        EnsureLookup();
        return lookup.ContainsKey(warrantId);
    }

	/// <summary>
	/// Creates a runtime warrant instance from a blueprint, rolling affixes from the WarrantAffixDatabase.
	/// The new instance is not persisted as an asset, but can be registered at runtime (e.g., via WarrantLockerGrid.RegisterDefinitions).
	/// </summary>
	public WarrantDefinition CreateInstanceFromBlueprint(WarrantDefinition blueprint, int minAffixes = 1, int maxAffixes = 3)
	{
		if (blueprint == null)
		{
			Debug.LogWarning("[WarrantDatabase] CreateInstanceFromBlueprint called with null blueprint.");
			return null;
		}

		if (!blueprint.isBlueprint)
		{
			Debug.LogWarning($"[WarrantDatabase] Warrant '{blueprint.warrantId}' is not marked as blueprint. Proceeding anyway.");
		}

		if (affixDatabase == null)
		{
			Debug.LogWarning("[WarrantDatabase] No WarrantAffixDatabase assigned; blueprint instances will copy modifiers but not roll new affixes.");
			// Fallback: shallow clone without new affixes
			return CloneDefinitionRuntime(blueprint, false, Guid.NewGuid().ToString("N"), GetRandomIconFromLibrary());
		}

		// Get a random icon from the icon library (if available)
		Sprite randomIcon = GetRandomIconFromLibrary();
		
		var instance = CloneDefinitionRuntime(blueprint, true, Guid.NewGuid().ToString("N"), randomIcon);
		instance.isBlueprint = false; // rolled instance, not a blueprint
		instance.modifiers.Clear();

		// Copy base modifiers from blueprint as a starting point (optional)
		if (blueprint.modifiers != null && blueprint.modifiers.Count > 0)
		{
			foreach (var m in blueprint.modifiers)
			{
				if (m == null) continue;
				instance.modifiers.Add(new WarrantModifier
				{
					modifierId = m.modifierId,
					displayName = m.displayName,
					operation = m.operation,
					value = m.value,
					description = m.description
				});
			}
		}

		// STEP 1: Roll exactly one Notable from WarrantNotableDatabase
		string notableDisplayName = null;
		if (notableDatabase != null)
		{
			var notableEntry = notableDatabase.RollRandomNotable();
			if (notableEntry != null && notableEntry.modifiers != null && notableEntry.modifiers.Count > 0)
			{
				// Store the Notable ID for reference
				instance.notableId = notableEntry.notableId;
				notableDisplayName = notableEntry.displayName;
				
				// Convert NotableModifiers to WarrantModifiers (all are socket-only)
				foreach (var notableMod in notableEntry.modifiers)
				{
					if (notableMod == null || string.IsNullOrWhiteSpace(notableMod.statKey))
						continue;

					var mod = new WarrantModifier
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

					instance.modifiers.Add(mod);
				}
			}
		}
		else if (affixDatabase != null)
		{
			// Fallback to old system: Roll Notable group from AffixDatabase
			var notableGroup = affixDatabase.RollNotableGroup();
			if (notableGroup != null && notableGroup.Count > 0)
			{
				foreach (var entry in notableGroup)
				{
					if (entry == null || string.IsNullOrWhiteSpace(entry.statKey))
						continue;

				float rolled;
				string description;
				
				// Always roll as integer (whole numbers only) for both flat and percentage stats
				int minValue = Mathf.RoundToInt(entry.minPercent);
				int maxValue = Mathf.RoundToInt(entry.maxPercent);
				int rolledInt = UnityEngine.Random.Range(minValue, maxValue + 1);
				rolled = rolledInt; // Store as float for consistency, but it's a whole number
				
				if (entry.isFlat)
				{
					// Format as flat value: "+X" instead of "X%"
					description = string.IsNullOrWhiteSpace(entry.displayName) 
						? $"+{rolledInt} {entry.statKey}" 
						: entry.displayName;
				}
				else
				{
					// Format as percentage (but still whole number)
					description = string.IsNullOrWhiteSpace(entry.displayName) 
						? $"{rolledInt:+0;-0;0}% {entry.statKey}" 
						: entry.displayName;
				}

					var mod = new WarrantModifier
					{
						modifierId = entry.affixId,
						displayName = entry.displayName,
						operation = WarrantModifierOperation.Additive,
						value = rolled,
						description = description
					};
					
					// Store socketOnly flag in modifierId prefix for filtering
					if (entry.socketOnly)
					{
						mod.modifierId = $"__SOCKET_ONLY__{entry.affixId}";
					}

					instance.modifiers.Add(mod);
				}
			}
		}

		// STEP 2: Roll regular affixes (non-Notable, non-socketOnly)
		int regularAffixCount = 0;
		var regularEntries = affixDatabase.GetRegularAffixes();
		if (regularEntries != null && regularEntries.Count > 0)
		{
			// Clamp affix count
			minAffixes = Mathf.Max(0, minAffixes);
			maxAffixes = Mathf.Max(minAffixes, maxAffixes);
			int affixCount = UnityEngine.Random.Range(minAffixes, maxAffixes + 1);

			for (int i = 0; i < affixCount; i++)
			{
				var entry = RollAffix(regularEntries);
				if (entry == null || string.IsNullOrWhiteSpace(entry.statKey))
					continue;

				float rolled;
				string description;
				
				// Always roll as integer (whole numbers only) for both flat and percentage stats
				int minValue = Mathf.RoundToInt(entry.minPercent);
				int maxValue = Mathf.RoundToInt(entry.maxPercent);
				int rolledInt = UnityEngine.Random.Range(minValue, maxValue + 1);
				rolled = rolledInt; // Store as float for consistency, but it's a whole number
				
				if (entry.isFlat)
				{
					// Format as flat value: "+X" instead of "X%"
					description = string.IsNullOrWhiteSpace(entry.displayName) 
						? $"+{rolledInt} {entry.statKey}" 
						: entry.displayName;
				}
				else
				{
					// Format as percentage (but still whole number)
					description = string.IsNullOrWhiteSpace(entry.displayName) 
						? $"{rolledInt:+0;-0;0}% {entry.statKey}" 
						: entry.displayName;
				}

				var mod = new WarrantModifier
				{
					modifierId = entry.affixId,
					displayName = entry.displayName,
					operation = WarrantModifierOperation.Additive,
					value = rolled,
					description = description
				};

				instance.modifiers.Add(mod);
				regularAffixCount++;
			}
		}
		else
		{
			Debug.LogWarning("[WarrantDatabase] Affix database has no regular affix entries; blueprint instances will only have Notable modifiers.");
		}

		// STEP 3: Update warrant name to include Notable name
		if (!string.IsNullOrWhiteSpace(notableDisplayName))
		{
			instance.displayName = $"Warrant of {notableDisplayName}";
		}
		else if (!string.IsNullOrEmpty(instance.notableId))
		{
			// Fallback: if Notable was rolled but has no display name, use the Notable ID
			instance.displayName = $"Warrant of {instance.notableId}";
		}
		// If no Notable was rolled, keep the blueprint's display name

		// STEP 4: Calculate rarity based on affix count
		// Rules: 1 Notable + N regular affixes
		// 1 Notable + 1 affix = Common
		// 1 Notable + 2 affixes = Magic
		// 1 Notable + 3 affixes = Rare
		// 1 Notable + 4 affixes = Rare
		bool hasNotable = !string.IsNullOrEmpty(instance.notableId);
		int totalAffixCount = (hasNotable ? 1 : 0) + regularAffixCount;
		
		if (totalAffixCount == 2) // 1 Notable + 1 affix
		{
			instance.rarity = WarrantRarity.Common;
		}
		else if (totalAffixCount == 3) // 1 Notable + 2 affixes
		{
			instance.rarity = WarrantRarity.Magic;
		}
		else if (totalAffixCount >= 4) // 1 Notable + 3+ affixes
		{
			instance.rarity = WarrantRarity.Rare;
		}
		else
		{
			// Fallback: if no Notable or no affixes, default to Common
			instance.rarity = WarrantRarity.Common;
		}

		return instance;
	}


	private static WarrantDefinition CloneDefinitionRuntime(WarrantDefinition source, bool generateNewId, string idSuffix, Sprite overrideIcon = null)
	{
		if (source == null) return null;

		var clone = ScriptableObject.CreateInstance<WarrantDefinition>();
		clone.warrantId = generateNewId
			? $"{source.warrantId}_{idSuffix}"
			: source.warrantId;
		clone.displayName = source.displayName;
		// Use override icon if provided (from icon library), otherwise use source icon
		clone.icon = overrideIcon != null ? overrideIcon : source.icon;
		clone.rarity = source.rarity;
		clone.isKeystone = source.isKeystone;
		clone.isBlueprint = source.isBlueprint;
		clone.rangeDirection = source.rangeDirection;
		clone.rangeDepth = source.rangeDepth;
		clone.affectDiagonals = source.affectDiagonals;
		
		if (source.modifiers != null)
		{
			foreach (var m in source.modifiers)
			{
				if (m == null) continue;
				clone.modifiers.Add(new WarrantModifier
				{
					modifierId = m.modifierId,
					displayName = m.displayName,
					operation = m.operation,
					value = m.value,
					description = m.description
				});
			}
		}

		clone.notable = source.notable;
		clone.notableId = source.notableId;
		return clone;
	}

	private static WarrantAffixDatabase.WarrantAffixEntry RollAffix(List<WarrantAffixDatabase.WarrantAffixEntry> pool)
	{
		if (pool == null || pool.Count == 0)
			return null;

		int totalWeight = 0;
		for (int i = 0; i < pool.Count; i++)
		{
			var e = pool[i];
			if (e == null || e.weight <= 0)
				continue;
			totalWeight += e.weight;
		}

		if (totalWeight <= 0)
			return null;

		int roll = UnityEngine.Random.Range(0, totalWeight);
		int cumulative = 0;
		foreach (var e in pool)
		{
			if (e == null || e.weight <= 0)
				continue;
			cumulative += e.weight;
			if (roll < cumulative)
				return e;
		}

		return pool[pool.Count - 1];
	}

    private void EnsureLookup()
    {
        if (lookup != null)
            return;

        lookup = new Dictionary<string, WarrantDefinition>();
        if (definitions == null)
            return;

        foreach (var def in definitions)
        {
            if (def == null || string.IsNullOrEmpty(def.warrantId))
                continue;

            lookup[def.warrantId] = def;
        }
    }

	/// <summary>
	/// Gets a random icon from the icon library (if assigned).
	/// Returns null if no icon library is assigned or if the pool is empty.
	/// </summary>
	private Sprite GetRandomIconFromLibrary()
	{
		if (iconLibrary == null)
		{
			// No icon library assigned - this is fine, will use blueprint icon
			return null;
		}

		Sprite randomIcon = iconLibrary.GetRandomIcon();
		if (randomIcon != null)
		{
			Debug.Log($"[WarrantDatabase] Assigned random icon from icon library to rolled warrant instance.");
		}
		
		return randomIcon;
	}

#if UNITY_EDITOR
    private void OnValidate()
    {
        lookup = null;
    }
#endif
}

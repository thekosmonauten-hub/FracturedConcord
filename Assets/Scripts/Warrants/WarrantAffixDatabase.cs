using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Scriptable database of warrant affixes (source of truth for rollable % increased modifiers).
/// Only includes percentage-based "increased" style affixes that map to CharacterStatsData.
/// </summary>
[CreateAssetMenu(fileName = "WarrantAffixDatabase", menuName = "Dexiled/Warrants/Warrant Affix Database")]
public class WarrantAffixDatabase : ScriptableObject
{
	[Serializable]
	public enum AffixRarity
	{
		Common,
		Magic,
		Rare,
		Notable
	}
	
	[Serializable]
	public enum AffixKind
	{
		Regular,    // Normal affix that can spill to effect nodes
		Notable,    // Notable affix group (socket-only)
		Unique      // Unique/Keystone effects (future use)
	}
	
	[Serializable]
	public class WarrantAffixEntry
	{
		[Tooltip("Unique ID for this affix. Typically matches the stat key in CharacterStatsData (e.g., 'increasedPhysicalDamage').")]
		public string affixId;
		
		[Tooltip("Human friendly name to show in UI tooltips.")]
		public string displayName;
		
		[Tooltip("The CharacterStatsData key to modify (must be a supported % increased stat).")]
		public string statKey;
		
		[Tooltip("If true, this affix adds a flat value instead of % increased (min/max act as flat range).")]
		public bool isFlat = false;
		
		[Tooltip("Minimum value (for % increased or flat, depending on isFlat).")]
		public float minPercent = 4f;
		
		[Tooltip("Maximum value (for % increased or flat, depending on isFlat).")]
		public float maxPercent = 8f;
		
		[Tooltip("Categorization tags to enable filtering/loot tables (e.g., 'elemental', 'melee').")]
		public List<string> tags = new List<string>();
		
		[Tooltip("Tier grouping used for generation weight tables.")]
		public AffixRarity rarity = AffixRarity.Common;
		
		[Tooltip("Relative selection weight for this entry within its rarity.")]
		public int weight = 100;
		
		[Tooltip("Kind of affix: Regular (spills to effect nodes), Notable (socket-only bundle), or Unique.")]
		public AffixKind kind = AffixKind.Regular;
		
		[Tooltip("If true, this affix only applies to the socket node and does NOT spill to adjacent effect nodes.")]
		public bool socketOnly = false;
		
		[Tooltip("Group ID for Notable affixes. All entries with the same groupId are rolled together as one Notable bundle.")]
		public string groupId = string.Empty;
	}
	
	[SerializeField] private List<WarrantAffixEntry> entries = new List<WarrantAffixEntry>();
	
	private Dictionary<string, WarrantAffixEntry> idToEntry;
	private HashSet<string> validStatKeysCache;
	
	/// <summary>
	/// Adds a new affix entry to the database. Rebuilds caches after adding.
	/// </summary>
	public void AddEntry(WarrantAffixEntry entry)
	{
		if (entry == null)
		{
			Debug.LogWarning("[WarrantAffixDatabase] Attempted to add null entry.");
			return;
		}
		
		entries.Add(entry);
		RebuildCaches();
		
		#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
		#endif
	}
	
	/// <summary>
	/// Removes all entries of the specified kind. Rebuilds caches after removal.
	/// </summary>
	public void RemoveEntriesByKind(AffixKind kind)
	{
		entries.RemoveAll(e => e != null && e.kind == kind);
		RebuildCaches();
		
		#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
		#endif
	}
	
	private void OnEnable()
	{
		RebuildCaches();
	}
	
	public void RebuildCaches()
	{
		idToEntry = new Dictionary<string, WarrantAffixEntry>(StringComparer.OrdinalIgnoreCase);
		foreach (var e in entries)
		{
			if (e == null || string.IsNullOrWhiteSpace(e.affixId))
				continue;
			idToEntry[e.affixId] = e;
		}
		
		// Supported % increased keys (must map to CharacterStatsData and be additive/increased)
		validStatKeysCache = new HashSet<string>(new[]
		{
			// Damage (increased%)
			"increasedPhysicalDamage",
			"increasedFireDamage",
			"increasedColdDamage",
			"increasedLightningDamage",
			"increasedChaosDamage",
			"increasedElementalDamage",
			"increasedSpellDamage",
			"increasedAttackDamage",
			"increasedProjectileDamage",
			"increasedAreaDamage",
			"increasedMeleeDamage",
			"increasedRangedDamage",
			
			// Defense/avoidance (increased%)
			"evasionIncreased",
			"maxHealthIncreased",
			"maxManaIncreased",
			"energyShieldIncreased",
			"armourIncreased",
			
			// Ailment magnitude / over-time (increased%)
			"increasedIgniteMagnitude",
			"increasedShockMagnitude",
			"increasedChillMagnitude",
			"increasedFreezeMagnitude",
			"increasedBleedMagnitude",
			"increasedPoisonMagnitude",
			"increasedDamageOverTime",
			"increasedPoisonDamage",
			"increasedPoisonDuration",
			"increasedDamageVsChilled",
			"increasedDamageVsShocked",
			"increasedDamageVsIgnited",
			
			// Speed / duration
			"attackSpeed",
			"castSpeed",
			"statusEffectDuration",
			
			// Charge/resource gains
			"aggressionGainIncreased",
			"focusGainIncreased",
			
			// Weapon/Type Damage Modifiers
			"increasedAxeDamage",
			"increasedBowDamage",
			"increasedMaceDamage",
			"increasedSwordDamage",
			"increasedWandDamage",
			"increasedOneHandedDamage",
			"increasedTwoHandedDamage",
			
			// Guard/Defense Utilities
			"guardEffectivenessIncreased",
			"lessDamageFromElites",
			"statusAvoidance",
			
			// Resist umbrella – supported values (note: these are flat %, but additive)
			// Include only if you intend to allow as "increased" style. If not, omit.
			// "allResistance" // commented out by default
		}, StringComparer.OrdinalIgnoreCase);
	}
	
	public IReadOnlyList<WarrantAffixEntry> GetAll() => entries;
	
	public bool TryGetById(string affixId, out WarrantAffixEntry entry)
	{
		if (idToEntry == null) RebuildCaches();
		return idToEntry.TryGetValue(affixId, out entry);
	}
	
	public List<WarrantAffixEntry> GetByTag(string tag)
	{
		if (string.IsNullOrWhiteSpace(tag))
			return new List<WarrantAffixEntry>();
		return entries.Where(e => e != null && e.tags != null && e.tags.Any(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase))).ToList();
	}
	
	public List<WarrantAffixEntry> GetByRarity(AffixRarity rarity)
	{
		return entries.Where(e => e != null && e.rarity == rarity).ToList();
	}
	
	/// <summary>
	/// Returns all entries whose statKey is recognized as a valid % increased stat.
	/// </summary>
	public List<WarrantAffixEntry> GetValidatedEntries()
	{
		if (validStatKeysCache == null) RebuildCaches();
		return entries.Where(IsEntryValid).ToList();
	}
	
	/// <summary>
	/// Returns all regular (non-Notable) affix entries for rolling.
	/// </summary>
	public List<WarrantAffixEntry> GetRegularAffixes()
	{
		return entries.Where(e => e != null && e.kind == AffixKind.Regular && IsEntryValid(e)).ToList();
	}
	
	/// <summary>
	/// Returns all Notable affix groups (grouped by groupId).
	/// </summary>
	public Dictionary<string, List<WarrantAffixEntry>> GetNotableGroups()
	{
		var groups = new Dictionary<string, List<WarrantAffixEntry>>();
		foreach (var entry in entries)
		{
			if (entry == null || entry.kind != AffixKind.Notable || !IsEntryValid(entry))
				continue;
			
			if (string.IsNullOrWhiteSpace(entry.groupId))
				continue;
			
			if (!groups.TryGetValue(entry.groupId, out var list))
			{
				list = new List<WarrantAffixEntry>();
				groups[entry.groupId] = list;
			}
			list.Add(entry);
		}
		return groups;
	}
	
	/// <summary>
	/// Rolls a random Notable group by weight (returns all entries in that group).
	/// </summary>
	public List<WarrantAffixEntry> RollNotableGroup()
	{
		var groups = GetNotableGroups();
		if (groups.Count == 0)
			return new List<WarrantAffixEntry>();
		
		// Calculate total weight per group (use first entry's weight as group weight)
		var weightedGroups = new List<(string groupId, int weight, List<WarrantAffixEntry> entries)>();
		foreach (var kvp in groups)
		{
			if (kvp.Value == null || kvp.Value.Count == 0)
				continue;
			
			// Use the first entry's weight as the group weight
			int groupWeight = kvp.Value[0].weight;
			weightedGroups.Add((kvp.Key, groupWeight, kvp.Value));
		}
		
		if (weightedGroups.Count == 0)
			return new List<WarrantAffixEntry>();
		
		// Weighted random selection
		int totalWeight = weightedGroups.Sum(g => g.weight);
		if (totalWeight <= 0)
			return weightedGroups[0].entries;
		
		int roll = UnityEngine.Random.Range(0, totalWeight);
		int cumulative = 0;
		foreach (var group in weightedGroups)
		{
			cumulative += group.weight;
			if (roll < cumulative)
				return group.entries;
		}
		
		return weightedGroups[weightedGroups.Count - 1].entries;
	}
	
	public bool IsEntryValid(WarrantAffixEntry entry)
	{
		if (entry == null) return false;
		if (string.IsNullOrWhiteSpace(entry.statKey)) return false;
		
		// Flat stats are allowed for any non-empty statKey that CharacterStatsData understands.
		// Validation for those is done where they are applied (via Get/Set/AddToStat).
		if (entry.isFlat)
		{
			return true;
		}
		
		// % increased stats must be in the curated list.
		if (validStatKeysCache == null) RebuildCaches();
		return validStatKeysCache.Contains(entry.statKey);
	}
	
	/// <summary>
	/// Simple helper to seed entries programmatically from code if needed.
	/// Safe to call in Editor to ensure entries exist.
	/// </summary>
	[ContextMenu("Seed Basic % Increased Affixes")]
	public void SeedBasicIncreasedAffixes()
	{
		var seed = new List<WarrantAffixEntry>
		{
			// 1. Generic Damage Affixes (Common tier examples)
			new WarrantAffixEntry{ affixId="increasedDamage_generic_elemental", displayName="Increased Elemental Damage", statKey="increasedElementalDamage", minPercent=4, maxPercent=8, tags=new List<string>{"elemental","damage"}, rarity=AffixRarity.Common, weight=100 },
			new WarrantAffixEntry{ affixId="increasedDamage_generic_physical", displayName="Increased Physical Damage", statKey="increasedPhysicalDamage", minPercent=4, maxPercent=8, tags=new List<string>{"physical","damage"}, rarity=AffixRarity.Common, weight=100 },
			new WarrantAffixEntry{ affixId="increasedDamage_generic_attack", displayName="Increased Attack Damage", statKey="increasedAttackDamage", minPercent=5, maxPercent=10, tags=new List<string>{"attack","damage"}, rarity=AffixRarity.Common, weight=100 },
			new WarrantAffixEntry{ affixId="increasedDamage_generic_spell", displayName="Increased Spell Damage", statKey="increasedSpellDamage", minPercent=6, maxPercent=12, tags=new List<string>{"spell","damage"}, rarity=AffixRarity.Common, weight=100 },
			new WarrantAffixEntry{ affixId="increasedDamage_generic_projectile", displayName="Increased Projectile Damage", statKey="increasedProjectileDamage", minPercent=6, maxPercent=12, tags=new List<string>{"projectile","damage"}, rarity=AffixRarity.Magic, weight=90 },
			new WarrantAffixEntry{ affixId="increasedDamage_generic_area", displayName="Increased Area Damage", statKey="increasedAreaDamage", minPercent=10, maxPercent=14, tags=new List<string>{"area","damage"}, rarity=AffixRarity.Magic, weight=90 },
			
			// 2. Life/Defense (only % increased that map to CharacterStatsData)
			new WarrantAffixEntry{ affixId="increasedEvasion_percent", displayName="Increased Evasion", statKey="evasionIncreased", minPercent=8, maxPercent=12, tags=new List<string>{"defense","evasion"}, rarity=AffixRarity.Common, weight=100 },
			new WarrantAffixEntry{ affixId="increasedMaxLife_percent", displayName="Increased Maximum Life", statKey="maxHealthIncreased", minPercent=3, maxPercent=5, tags=new List<string>{"defense","life"}, rarity=AffixRarity.Common, weight=100 },
			new WarrantAffixEntry{ affixId="increasedMaxMana_percent", displayName="Increased Maximum Mana", statKey="maxManaIncreased", minPercent=1, maxPercent=2, tags=new List<string>{"defense","mana"}, rarity=AffixRarity.Common, weight=100 },
			new WarrantAffixEntry{ affixId="increasedEnergyShield_percent", displayName="Increased Energy Shield", statKey="energyShieldIncreased", minPercent=4, maxPercent=8, tags=new List<string>{"defense","energyshield"}, rarity=AffixRarity.Common, weight=100 },
			new WarrantAffixEntry{ affixId="increasedArmour_percent", displayName="Increased Armour", statKey="armourIncreased", minPercent=4, maxPercent=8, tags=new List<string>{"defense","armour"}, rarity=AffixRarity.Common, weight=100 },
			
			// 4. Spell / Element (Common examples at the higher band)
			new WarrantAffixEntry{ affixId="increasedFireDamage_common", displayName="Increased Fire Damage", statKey="increasedFireDamage", minPercent=8, maxPercent=12, tags=new List<string>{"elemental","fire"}, rarity=AffixRarity.Common, weight=100 },
			new WarrantAffixEntry{ affixId="increasedColdDamage_common", displayName="Increased Cold Damage", statKey="increasedColdDamage", minPercent=8, maxPercent=12, tags=new List<string>{"elemental","cold"}, rarity=AffixRarity.Common, weight=100 },
			new WarrantAffixEntry{ affixId="increasedLightningDamage_common", displayName="Increased Lightning Damage", statKey="increasedLightningDamage", minPercent=8, maxPercent=12, tags=new List<string>{"elemental","lightning"}, rarity=AffixRarity.Common, weight=100 },
			new WarrantAffixEntry{ affixId="increasedChaosDamage_common", displayName="Increased Chaos Damage", statKey="increasedChaosDamage", minPercent=8, maxPercent=12, tags=new List<string>{"chaos"}, rarity=AffixRarity.Common, weight=100 },
			
			// 9. Ailments (magnitude/effect % that exist)
			new WarrantAffixEntry{ affixId="increasedIgniteMagnitude", displayName="Increased Ignite Magnitude", statKey="increasedIgniteMagnitude", minPercent=10, maxPercent=16, tags=new List<string>{"ailment","ignite"}, rarity=AffixRarity.Magic, weight=80 },
			new WarrantAffixEntry{ affixId="increasedShockMagnitude", displayName="Increased Shock Magnitude", statKey="increasedShockMagnitude", minPercent=10, maxPercent=16, tags=new List<string>{"ailment","shock"}, rarity=AffixRarity.Magic, weight=80 },
			new WarrantAffixEntry{ affixId="increasedChillMagnitude", displayName="Increased Chill Magnitude", statKey="increasedChillMagnitude", minPercent=10, maxPercent=16, tags=new List<string>{"ailment","chill"}, rarity=AffixRarity.Magic, weight=80 },
			new WarrantAffixEntry{ affixId="increasedFreezeMagnitude", displayName="Increased Freeze Magnitude", statKey="increasedFreezeMagnitude", minPercent=10, maxPercent=16, tags=new List<string>{"ailment","freeze"}, rarity=AffixRarity.Magic, weight=80 },
			new WarrantAffixEntry{ affixId="increasedBleedMagnitude", displayName="Increased Bleed Magnitude", statKey="increasedBleedMagnitude", minPercent=10, maxPercent=16, tags=new List<string>{"ailment","bleed"}, rarity=AffixRarity.Magic, weight=80 },
			new WarrantAffixEntry{ affixId="increasedPoisonMagnitude", displayName="Increased Poison Magnitude", statKey="increasedPoisonMagnitude", minPercent=10, maxPercent=16, tags=new List<string>{"ailment","poison"}, rarity=AffixRarity.Magic, weight=80 },
			
			// Melee/Projectile/Melee Damage
			new WarrantAffixEntry{ affixId="increasedMeleeDamage_magic", displayName="Increased Melee Damage", statKey="increasedMeleeDamage", minPercent=6, maxPercent=12, tags=new List<string>{"melee","damage"}, rarity=AffixRarity.Magic, weight=90 },
			new WarrantAffixEntry{ affixId="increasedRangedDamage_magic", displayName="Increased Ranged Damage", statKey="increasedRangedDamage", minPercent=6, maxPercent=12, tags=new List<string>{"ranged","damage"}, rarity=AffixRarity.Magic, weight=90 },
			
			// 3. Weapon / Attack-Specific (from WarrantDatabase.md)
			new WarrantAffixEntry{ affixId="increasedAxeDamage_common", displayName="Increased Damage with Axes", statKey="increasedAxeDamage", minPercent=6, maxPercent=10, tags=new List<string>{"weapon","axe","attack"}, rarity=AffixRarity.Common, weight=100 },
			new WarrantAffixEntry{ affixId="increasedBowDamage_common", displayName="Increased Damage with Bows", statKey="increasedBowDamage", minPercent=6, maxPercent=10, tags=new List<string>{"weapon","bow","attack"}, rarity=AffixRarity.Common, weight=100 },
			new WarrantAffixEntry{ affixId="increasedMaceDamage_common", displayName="Increased Damage with Maces", statKey="increasedMaceDamage", minPercent=6, maxPercent=10, tags=new List<string>{"weapon","mace","attack"}, rarity=AffixRarity.Common, weight=100 },
			new WarrantAffixEntry{ affixId="increasedSwordDamage_common", displayName="Increased Damage with Swords", statKey="increasedSwordDamage", minPercent=6, maxPercent=10, tags=new List<string>{"weapon","sword","attack"}, rarity=AffixRarity.Common, weight=100 },
			new WarrantAffixEntry{ affixId="increasedWandDamage_common", displayName="Increased Damage with Wands", statKey="increasedWandDamage", minPercent=6, maxPercent=10, tags=new List<string>{"weapon","wand","attack"}, rarity=AffixRarity.Common, weight=100 },
			new WarrantAffixEntry{ affixId="increasedOneHandedDamage_common", displayName="Increased One-Handed Damage", statKey="increasedOneHandedDamage", minPercent=6, maxPercent=10, tags=new List<string>{"weapon","onehanded","attack"}, rarity=AffixRarity.Common, weight=100 },
			new WarrantAffixEntry{ affixId="increasedTwoHandedDamage_common", displayName="Increased Two-Handed Damage", statKey="increasedTwoHandedDamage", minPercent=6, maxPercent=10, tags=new List<string>{"weapon","twohanded","attack"}, rarity=AffixRarity.Common, weight=100 },
			
			// 7. Speed / Turn Manipulation (subset that maps to existing stats)
			new WarrantAffixEntry{ affixId="attackSpeed_increased_common", displayName="Increased Attack Speed", statKey="attackSpeed", minPercent=4, maxPercent=8, tags=new List<string>{"speed","attack"}, rarity=AffixRarity.Common, weight=100 },
			new WarrantAffixEntry{ affixId="castSpeed_increased_common", displayName="Increased Cast Speed", statKey="castSpeed", minPercent=4, maxPercent=8, tags=new List<string>{"speed","cast"}, rarity=AffixRarity.Common, weight=100 },
			
			// 8/9. Ailments – duration and damage over time
			new WarrantAffixEntry{ affixId="ailmentDuration_increased_magic", displayName="Increased Ailment Duration", statKey="statusEffectDuration", minPercent=10, maxPercent=20, tags=new List<string>{"ailment","duration"}, rarity=AffixRarity.Magic, weight=85 },
			new WarrantAffixEntry{ affixId="damageOverTime_increased_magic", displayName="Increased Damage over Time", statKey="increasedDamageOverTime", minPercent=5, maxPercent=10, tags=new List<string>{"dot","damage"}, rarity=AffixRarity.Magic, weight=85 },
			new WarrantAffixEntry{ affixId="poisonDamage_increased_magic", displayName="Increased Poison Damage", statKey="increasedPoisonDamage", minPercent=10, maxPercent=14, tags=new List<string>{"poison","dot"}, rarity=AffixRarity.Magic, weight=80 },
			new WarrantAffixEntry{ affixId="poisonDuration_increased_magic", displayName="Increased Poison Duration", statKey="increasedPoisonDuration", minPercent=15, maxPercent=20, tags=new List<string>{"poison","duration"}, rarity=AffixRarity.Magic, weight=80 },
			
			// 9. Ailment conditional damage
			new WarrantAffixEntry{ affixId="damageVsChilled_magic", displayName="Increased Damage against Chilled Enemies", statKey="increasedDamageVsChilled", minPercent=15, maxPercent=15, tags=new List<string>{"ailment","cold","chill","damage"}, rarity=AffixRarity.Magic, weight=80 },
			new WarrantAffixEntry{ affixId="damageVsShocked_magic", displayName="Increased Damage against Shocked Enemies", statKey="increasedDamageVsShocked", minPercent=15, maxPercent=15, tags=new List<string>{"ailment","lightning","shock","damage"}, rarity=AffixRarity.Magic, weight=80 },
			new WarrantAffixEntry{ affixId="damageVsIgnited_magic", displayName="Increased Damage against Ignited Enemies", statKey="increasedDamageVsIgnited", minPercent=15, maxPercent=15, tags=new List<string>{"ailment","fire","ignite","damage"}, rarity=AffixRarity.Magic, weight=80 },
			
			// 6. Aggression / Focus (gain)
			new WarrantAffixEntry{ affixId="aggressionGain_increased_common", displayName="Increased Aggression Gain", statKey="aggressionGainIncreased", minPercent=10, maxPercent=10, tags=new List<string>{"aggression","charge"}, rarity=AffixRarity.Common, weight=100 },
			new WarrantAffixEntry{ affixId="focusGain_increased_common", displayName="Increased Focus Gain", statKey="focusGainIncreased", minPercent=10, maxPercent=10, tags=new List<string>{"focus","charge"}, rarity=AffixRarity.Common, weight=100 },
			
			// 10. Defensive Utility
			new WarrantAffixEntry{ affixId="guardEffectiveness_increased_magic", displayName="Increased Guard Effectiveness", statKey="guardEffectivenessIncreased", minPercent=15, maxPercent=15, tags=new List<string>{"guard","defense"}, rarity=AffixRarity.Magic, weight=80 },
			new WarrantAffixEntry{ affixId="lessDamageFromElites_magic", displayName="Less Damage from Elites", statKey="lessDamageFromElites", minPercent=4, maxPercent=6, tags=new List<string>{"defense","elite"}, rarity=AffixRarity.Magic, weight=75 },
			new WarrantAffixEntry{ affixId="statusAvoidance_magic", displayName="Increased Status Avoidance", statKey="statusAvoidance", minPercent=4, maxPercent=6, tags=new List<string>{"defense","status"}, rarity=AffixRarity.Magic, weight=75 }
		};
		
		foreach (var s in seed)
		{
			if (string.IsNullOrWhiteSpace(s.affixId)) continue;
			if (entries.Any(e => e != null && string.Equals(e.affixId, s.affixId, StringComparison.OrdinalIgnoreCase)))
				continue;
			entries.Add(s);
		}
		
		RebuildCaches();
		#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
		#endif
	}
}



using UnityEngine;
using System;
using System.Collections.Generic;

namespace PassiveTree
{
/// <summary>
	/// Applies passive tree node bonuses to the active Character via CharacterManager.
	/// Uses simple, configurable per-node-type bonuses and recalculates on character load and node changes.
/// </summary>
public class PassiveTreeStatsIntegration : MonoBehaviour
{
		[Header("Node Bonuses (additive)")]
		public int smallNodeStrength = 5;
		public int smallNodeDexterity = 0;
		public int smallNodeIntelligence = 0;

		public int notableNodeStrength = 10;
		public int notableNodeDexterity = 0;
		public int notableNodeIntelligence = 0;

		public int keystoneNodeStrength = 0;
		public int keystoneNodeDexterity = 0;
		public int keystoneNodeIntelligence = 0;

		[Header("Damage Bonuses (increased %, applied additively)")]
		public float smallNodeIncreasedPhysicalDamage = 0f;
		public float notableNodeIncreasedPhysicalDamage = 0f;
		public float keystoneNodeIncreasedPhysicalDamage = 0f;

		private PassiveTreeManager passiveTreeManager;

		private struct PassiveBonuses
		{
			public int addStr;
			public int addDex;
			public int addInt;
			public float incPhys;
		}

		private PassiveBonuses lastApplied;

		public void SetPassiveTreeManager(PassiveTreeManager mgr)
		{
			passiveTreeManager = mgr;
		}

		public void SetupIntegration()
		{
			// Subscribe to character events
			if (CharacterManager.Instance != null)
			{
				CharacterManager.Instance.OnCharacterLoaded += OnCharacterLoaded;
				CharacterManager.Instance.OnCharacterLevelUp += OnCharacterLoaded;
        }

        // Subscribe to passive tree events
			PassiveTreeManager.OnNodeAllocated += OnNodeChanged;
			PassiveTreeManager.OnNodeDeallocated += OnNodeChanged;

			// Initial apply if character exists
			OnCharacterLoaded(CharacterManager.Instance != null ? CharacterManager.Instance.GetCurrentCharacter() : null);
		}

		private void OnDestroy()
		{
			if (CharacterManager.Instance != null)
			{
				CharacterManager.Instance.OnCharacterLoaded -= OnCharacterLoaded;
				CharacterManager.Instance.OnCharacterLevelUp -= OnCharacterLoaded;
			}
			PassiveTreeManager.OnNodeAllocated -= OnNodeChanged;
			PassiveTreeManager.OnNodeDeallocated -= OnNodeChanged;
		}

		private void OnNodeChanged(Vector2Int _, CellController __)
		{
			RecalculateAndApply();
		}

		private void OnCharacterLoaded(Character _)
		{
			RecalculateAndApply();
		}

		private void RecalculateAndApply()
		{
			var character = CharacterManager.Instance != null ? CharacterManager.Instance.GetCurrentCharacter() : null;
			if (character == null || passiveTreeManager == null) return;

			// Build totals from all purchased nodes
			PassiveBonuses total = new PassiveBonuses();
            var allCells = passiveTreeManager.GetAllCells();
			foreach (var kv in allCells)
			{
				var cell = kv.Value;
				if (cell == null || !cell.IsPurchased) continue;
				switch (cell.NodeType)
				{
					case NodeType.Travel:
					case NodeType.Small:
						Accumulate(ref total, smallNodeStrength, smallNodeDexterity, smallNodeIntelligence, smallNodeIncreasedPhysicalDamage);
						break;
					case NodeType.Notable:
						Accumulate(ref total, notableNodeStrength, notableNodeDexterity, notableNodeIntelligence, notableNodeIncreasedPhysicalDamage);
						break;
					case NodeType.Keystone:
						Accumulate(ref total, keystoneNodeStrength, keystoneNodeDexterity, keystoneNodeIntelligence, keystoneNodeIncreasedPhysicalDamage);
						break;
				}
			}

			ApplyBonuses(character, total);
		}

		private static void Accumulate(ref PassiveBonuses dst, int str, int dex, int intel, float incPhys)
		{
			dst.addStr += str;
			dst.addDex += dex;
			dst.addInt += intel;
			dst.incPhys += incPhys;
		}

		private void ApplyBonuses(Character c, PassiveBonuses now)
		{
			// Remove previous bonuses
			c.strength -= lastApplied.addStr;
			c.dexterity -= lastApplied.addDex;
			c.intelligence -= lastApplied.addInt;

			// There isn't a single field for increasedPhysicalDamage on Character; if you have a modifiers container, apply there.
			// For now, just adjust CharacterStatsData on next rebuild via CalculateDerivedStats.

			// Apply new bonuses
			c.strength += now.addStr;
			c.dexterity += now.addDex;
			c.intelligence += now.addInt;

			lastApplied = now;

			// Recalculate derived stats if available
			try
			{
				c.CalculateDerivedStats();
			}
			catch { }
    }

    /// <summary>
		/// Validate passive node stat mappings against CharacterStatsData. Optional helper used by setup scripts.
    /// </summary>
    public void ValidateStatMappings()
    {
			try
			{
				var boardDataManager = FindFirstObjectByType<EnhancedBoardDataManager>();
        if (boardDataManager == null)
        {
					Debug.Log("[PassiveTreeStatsIntegration] Stat mapping validation skipped: no EnhancedBoardDataManager found");
            return;
        }

        var allNodeData = boardDataManager.GetAllNodeData();
        List<string> allStats = new List<string>();
        foreach (var kvp in allNodeData)
        {
            var statsDict = kvp.Value.GetStats();
					if (statsDict == null) continue;
                foreach (var statKvp in statsDict)
                {
                    allStats.Add($"{statKvp.Key}: {statKvp.Value}");
            }
        }

				var unmapped = PassiveTreeStatMapper.ValidateStatMappings(allStats);
				if (unmapped != null && unmapped.Count > 0)
        {
					Debug.LogWarning($"[PassiveTreeStatsIntegration] Found {unmapped.Count} unmapped stats");
					foreach (var stat in unmapped)
            {
                Debug.LogWarning($"  - {stat}");
            }
        }
        else
        {
					Debug.Log("[PassiveTreeStatsIntegration] All passive stats are mapped to CharacterStatsData.");
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning($"[PassiveTreeStatsIntegration] ValidateStatMappings encountered an issue: {e.Message}");
			}
		}
	}
}

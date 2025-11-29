using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Scriptable database of Notable effects for warrants.
/// Each Notable is a self-contained entry with its own modifiers.
/// </summary>
[CreateAssetMenu(fileName = "WarrantNotableDatabase", menuName = "Dexiled/Warrants/Warrant Notable Database")]
public class WarrantNotableDatabase : ScriptableObject
{
	[Serializable]
	public class NotableModifier
	{
		[Tooltip("The CharacterStatsData key to modify (e.g., 'increasedPhysicalDamage').")]
		public string statKey;
		
		[Tooltip("Human friendly name to show in UI tooltips.")]
		public string displayName;
		
		[Tooltip("The value for this modifier (can be % increased or flat depending on stat type).")]
		public float value;
		
		[Tooltip("If true, this modifier only applies to the socket node and does NOT spill to adjacent effect nodes.")]
		public bool socketOnly = true; // Notables are socket-only by default
	}
	
	[Serializable]
	public class NotableEntry
	{
		[Tooltip("Unique ID for this Notable (e.g., 'courage', 'call_to_arms').")]
		public string notableId;
		
		[Tooltip("Display name shown in tooltips and UI.")]
		public string displayName;
		
		[Tooltip("Description text for this Notable.")]
		[TextArea(2, 4)]
		public string description;
		
		[Tooltip("Tags for categorization and filtering.")]
		public List<string> tags = new List<string>();
		
		[Tooltip("Relative selection weight when rolling Notables.")]
		public int weight = 100;
		
		[Tooltip("Modifiers that this Notable provides. All modifiers are socket-only by default.")]
		public List<NotableModifier> modifiers = new List<NotableModifier>();
	}
	
	[SerializeField] private List<NotableEntry> notables = new List<NotableEntry>();
	
	private Dictionary<string, NotableEntry> idToNotable;
	
	/// <summary>
	/// Gets all Notable entries.
	/// </summary>
	public List<NotableEntry> GetAll()
	{
		return notables ?? new List<NotableEntry>();
	}
	
	/// <summary>
	/// Gets a Notable by its ID.
	/// </summary>
	public NotableEntry GetById(string notableId)
	{
		if (string.IsNullOrEmpty(notableId))
			return null;
		
		RebuildCaches();
		
		if (idToNotable != null && idToNotable.TryGetValue(notableId, out var entry))
		{
			return entry;
		}
		
		return null;
	}
	
	/// <summary>
	/// Checks if a Notable exists with the given ID.
	/// </summary>
	public bool Contains(string notableId)
	{
		return GetById(notableId) != null;
	}
	
	/// <summary>
	/// Adds a new Notable entry. Rebuilds caches after adding.
	/// </summary>
	public void AddNotable(NotableEntry notable)
	{
		if (notable == null)
		{
			Debug.LogWarning("[WarrantNotableDatabase] Attempted to add null Notable entry.");
			return;
		}
		
		if (string.IsNullOrEmpty(notable.notableId))
		{
			Debug.LogWarning("[WarrantNotableDatabase] Notable entry must have a valid notableId.");
			return;
		}
		
		// Remove existing entry with same ID if present
		notables.RemoveAll(n => n != null && n.notableId == notable.notableId);
		
		notables.Add(notable);
		RebuildCaches();
		
		#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
		#endif
	}
	
	/// <summary>
	/// Removes a Notable by ID.
	/// </summary>
	public void RemoveNotable(string notableId)
	{
		if (string.IsNullOrEmpty(notableId))
			return;
		
		int removed = notables.RemoveAll(n => n != null && n.notableId == notableId);
		
		if (removed > 0)
		{
			RebuildCaches();
			
			#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
			#endif
		}
	}
	
	/// <summary>
	/// Clears all Notable entries.
	/// </summary>
	public void ClearAll()
	{
		notables.Clear();
		RebuildCaches();
		
		#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
		#endif
	}
	
	/// <summary>
	/// Gets a random Notable using weighted selection.
	/// </summary>
	public NotableEntry RollRandomNotable()
	{
		if (notables == null || notables.Count == 0)
			return null;
		
		var validNotables = notables.Where(n => n != null && n.weight > 0).ToList();
		if (validNotables.Count == 0)
			return null;
		
		int totalWeight = validNotables.Sum(n => n.weight);
		if (totalWeight <= 0)
			return validNotables[UnityEngine.Random.Range(0, validNotables.Count)];
		
		int roll = UnityEngine.Random.Range(0, totalWeight);
		int currentWeight = 0;
		
		foreach (var notable in validNotables)
		{
			currentWeight += notable.weight;
			if (roll < currentWeight)
			{
				return notable;
			}
		}
		
		// Fallback (shouldn't reach here)
		return validNotables[validNotables.Count - 1];
	}
	
	/// <summary>
	/// Rebuilds internal caches for fast lookups.
	/// </summary>
	private void RebuildCaches()
	{
		if (notables == null)
		{
			notables = new List<NotableEntry>();
		}
		
		idToNotable = new Dictionary<string, NotableEntry>();
		
		foreach (var notable in notables)
		{
			if (notable != null && !string.IsNullOrEmpty(notable.notableId))
			{
				if (idToNotable.ContainsKey(notable.notableId))
				{
					Debug.LogWarning($"[WarrantNotableDatabase] Duplicate Notable ID found: {notable.notableId}. Keeping first occurrence.");
					continue;
				}
				
				idToNotable[notable.notableId] = notable;
			}
		}
	}
	
	private void OnEnable()
	{
		RebuildCaches();
	}
}


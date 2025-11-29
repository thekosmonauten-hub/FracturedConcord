using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a reusable "Notable" block that can be attached to a warrant.
/// Notables are socket-only: they apply to the socket node that holds the warrant,
/// and do NOT spill into the effect-node range aggregation.
/// </summary>
[CreateAssetMenu(fileName = "WarrantNotable", menuName = "Dexiled/Warrants/Notable", order = 1)]
public class WarrantNotableDefinition : ScriptableObject
{
	[Header("Identity")]
	public string notableId;
	public string displayName;
	[TextArea] public string description;
	public Sprite icon;
	
	[Header("Tags & Weighting")]
	public List<string> tags = new List<string>();
	public int weight = 100;
	
	[Header("Modifiers (Socket-Only)")]
	[Tooltip("Modifiers provided by this notable. These should be interpreted as affecting ONLY the socket node.")]
	public List<WarrantModifier> modifiers = new List<WarrantModifier>();
}



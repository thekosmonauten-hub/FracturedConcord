using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WarrantDefinition", menuName = "Dexiled/Warrants/Warrant Definition")]
public class WarrantDefinition : ScriptableObject
{
    [Header("Identity")]
    public string warrantId;
    public string displayName;
    public Sprite icon;
    public WarrantRarity rarity = WarrantRarity.Common;
    public bool isKeystone;
	public bool isBlueprint; // Master blueprint template (rolled into runtime instances)
	[Tooltip("If true, this warrant should NOT be rolled when given. It will be given as-is (useful for blueprint warrants that should be given directly).")]
	public bool doNotRoll; // If true, give this warrant directly without rolling

    [Header("Range & Behavior")]
    [Tooltip("Directions affected relative to the socket. Forward = nodes above, Backward = nodes below.")]
    public WarrantRangeDirection rangeDirection = WarrantRangeDirection.All;
    [Tooltip("How many nodes away this socket reaches in each direction.")]
    [Range(1, 5)] public int rangeDepth = 1;
    [Tooltip("If true, applies to diagonal neighbors as well.")]
    public bool affectDiagonals = true;

    [Header("Modifiers")]
    public List<WarrantModifier> modifiers = new List<WarrantModifier>();
	
	[Header("Notable (Socket-Only)")]
	[Tooltip("Optional notable attached to this warrant. Applies only to the socket node, not to effect nodes. Legacy: Use ScriptableObject Notable. New: Notable will be rolled from WarrantNotableDatabase when creating from blueprint.")]
	public WarrantNotableDefinition notable;
	
	[Tooltip("Runtime Notable ID (set when rolling from WarrantNotableDatabase). Used to reference NotableEntry from the database.")]
	public string notableId;
}





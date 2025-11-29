using UnityEngine;

[CreateAssetMenu(fileName = "New Encounter", menuName = "Dexiled/Encounter Data", order = 0)]
public class EncounterDataAsset : ScriptableObject
{
	[Header("Identity")]
	public int encounterID = 1;
	public string encounterName = "Encounter";
	public string sceneName = "CombatScene";
	[Header("Campaign")]
	[Tooltip("Story act this encounter belongs to (e.g., 1 = Act 1, 2 = Act 2)")]
	[Min(1)] public int actNumber = 1;
	[Tooltip("Icon displayed on encounter selection and world map nodes.")]
	public Sprite encounterSprite;

	[Header("Progression")]
	[Tooltip("Encounters that must be completed before this node unlocks.")]
	public EncounterDataAsset[] prerequisiteEncounters;
	[Header("Difficulty")]
	public int areaLevel = 1;
	[Header("Waves")]
	[Min(1)] public int totalWaves = 1;
	[Tooltip("Maximum number of enemies that can spawn per wave (capped by available enemy display slots)")]
	[Range(1, 5)] public int maxEnemiesPerWave = 3;
	[Tooltip("If true, randomly spawn 1 to maxEnemiesPerWave enemies. If false, always spawn exactly maxEnemiesPerWave enemies.")]
	public bool randomizeEnemyCount = true;

	[Header("Unique Boss (Final Wave)")]
	public EnemyData uniqueEnemy;
	
	[Header("Loot Rewards")]
	[Tooltip("Classic loot table that defines rewards for completing this encounter")]
	public LootTable lootTable;
	[Tooltip("Optional AreaLootTable override. If assigned, this will be used instead of the classic LootTable.")]
	public AreaLootTable areaLootTable;
}




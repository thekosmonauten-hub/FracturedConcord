using UnityEngine;

[CreateAssetMenu(fileName = "New Encounter", menuName = "Dexiled/Encounter Data", order = 0)]
public class EncounterDataAsset : ScriptableObject
{
	[Header("Identity")]
	public int encounterID = 1;
	public string encounterName = "Encounter";
	public string sceneName = "CombatScene";

	[Header("Difficulty")]
	public int areaLevel = 1;
	[Header("Waves")]
	[Min(1)] public int totalWaves = 1;

	[Header("Unique Boss (Final Wave)")]
	public EnemyData uniqueEnemy;
}




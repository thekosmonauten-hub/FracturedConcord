using UnityEngine;
using PassiveTree;

/// <summary>
/// Test script to check which JSON fields are being parsed
/// </summary>
public class JsonFieldTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool testOnStart = true;

    void Start()
    {
        if (testOnStart)
        {
            TestJsonFields();
        }
    }

    /// <summary>
    /// Test which JSON fields are being parsed
    /// </summary>
    [ContextMenu("Test JSON Fields")]
    public void TestJsonFields()
    {
        Debug.Log("🔍 [JsonFieldTest] Starting JSON field test...");

        // Find JsonBoardDataManager
        var dataManager = FindFirstObjectByType<JsonBoardDataManager>();
        if (dataManager == null)
        {
            Debug.LogError("❌ [JsonFieldTest] JsonBoardDataManager not found!");
            return;
        }

        var jsonFile = dataManager.BoardDataJson;
        if (jsonFile == null)
        {
            Debug.LogError("❌ [JsonFieldTest] No JSON file assigned!");
            return;
        }

        // Test with a minimal structure that only has the fields we know exist
        Debug.Log("🔍 [JsonFieldTest] Testing with minimal structure...");
        try
        {
            MinimalBoardData minimalData = JsonUtility.FromJson<MinimalBoardData>(jsonFile.text);
            Debug.Log($"  - minimalData: {minimalData != null}");
            if (minimalData != null)
            {
                Debug.Log($"  - id: '{minimalData.id}'");
                Debug.Log($"  - name: '{minimalData.name}'");
                Debug.Log($"  - description: '{minimalData.description}'");
                Debug.Log($"  - theme: '{minimalData.theme}'");
                Debug.Log($"  - size: {minimalData.size != null}");
                if (minimalData.size != null)
                {
                    Debug.Log($"    - rows: {minimalData.size.rows}");
                    Debug.Log($"    - columns: {minimalData.size.columns}");
                }
                Debug.Log($"  - nodes: {minimalData.nodes != null}");
                if (minimalData.nodes != null)
                {
                    Debug.Log($"    - nodes.Length: {minimalData.nodes.Length}");
                    if (minimalData.nodes.Length > 0)
                    {
                        Debug.Log($"    - First row: {minimalData.nodes[0] != null}");
                        if (minimalData.nodes[0] != null)
                        {
                            Debug.Log($"      - First row length: {minimalData.nodes[0].Length}");
                            if (minimalData.nodes[0].Length > 0)
                            {
                                Debug.Log($"      - First node: {minimalData.nodes[0][0] != null}");
                                if (minimalData.nodes[0][0] != null)
                                {
                                    var firstNode = minimalData.nodes[0][0];
                                    Debug.Log($"        - id: '{firstNode.id}'");
                                    Debug.Log($"        - name: '{firstNode.name}'");
                                    Debug.Log($"        - description: '{firstNode.description}'");
                                    Debug.Log($"        - position: {firstNode.position != null}");
                                    if (firstNode.position != null)
                                    {
                                        Debug.Log($"          - row: {firstNode.position.row}");
                                        Debug.Log($"          - column: {firstNode.position.column}");
                                    }
                                    Debug.Log($"        - type: '{firstNode.type}'");
                                    Debug.Log($"        - stats: {firstNode.stats != null}");
                                    if (firstNode.stats != null)
                                    {
                                        Debug.Log($"          - strength: {firstNode.stats.strength}");
                                        Debug.Log($"          - intelligence: {firstNode.stats.intelligence}");
                                    }
                                    Debug.Log($"        - maxRank: {firstNode.maxRank}");
                                    Debug.Log($"        - currentRank: {firstNode.currentRank}");
                                    Debug.Log($"        - cost: {firstNode.cost}");
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [JsonFieldTest] Minimal structure test failed: {e.Message}");
        }

        Debug.Log("🔍 [JsonFieldTest] JSON field test complete!");
    }

    /// <summary>
    /// Minimal board data structure for testing
    /// </summary>
    [System.Serializable]
    public class MinimalBoardData
    {
        public string id;
        public string name;
        public string description;
        public string theme;
        public MinimalSize size;
        public MinimalNode[][] nodes;
    }

    [System.Serializable]
    public class MinimalSize
    {
        public int rows;
        public int columns;
    }

    [System.Serializable]
    public class MinimalNode
    {
        public string id;
        public string name;
        public string description;
        public MinimalPosition position;
        public string type;
        public MinimalStats stats;
        public int maxRank;
        public int currentRank;
        public int cost;
    }

    [System.Serializable]
    public class MinimalPosition
    {
        public int row;
        public int column;
    }

    [System.Serializable]
    public class MinimalStats
    {
        public int strength;
        public int dexterity;
        public int intelligence;
        public int maxHealthIncrease;
        public int maxEnergyShieldIncrease;
        public int maxEnergyShield;
        public int armor;
        public int armorIncrease;
        public int evasion;
        public int evasionIncrease;
        public int energyShield;
        public int energyShieldIncrease;
        public int maxManaIncrease;
        public int manaRegeneration;
        public int lifeRegeneration;
        public int energyShieldRegeneration;
        public int attackSpeed;
        public int castSpeed;
        public int movementSpeed;
        public int criticalStrikeChance;
        public int criticalStrikeMultiplier;
        public int accuracy;
        public int blockChance;
        public int dodgeChance;
        public int spellBlockChance;
        public int spellDodgeChance;
        public int physicalResistance;
        public int fireResistance;
        public int coldResistance;
        public int lightningResistance;
        public int chaosResistance;
        public int elementalResistance;
        public int allResistance;
        public int lifeLeech;
        public int manaLeech;
        public int energyShieldLeech;
        public int areaOfEffect;
        public int skillEffectDuration;
        public int statusEffectDuration;
        public int projectileSpeed;
        public int attackRange;
        public int maxHandSize;
        public int cardsDrawnPerTurn;
        public int discardPower;
        public int manaPerTurn;
    }
}




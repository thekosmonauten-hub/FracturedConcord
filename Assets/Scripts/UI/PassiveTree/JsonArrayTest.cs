using UnityEngine;
using PassiveTree;

/// <summary>
/// Test script to check if the issue is with jagged arrays
/// </summary>
public class JsonArrayTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool testOnStart = true;

    void Start()
    {
        if (testOnStart)
        {
            TestJsonArrays();
        }
    }

    /// <summary>
    /// Test JSON parsing with different array structures
    /// </summary>
    [ContextMenu("Test JSON Arrays")]
    public void TestJsonArrays()
    {
        Debug.Log("🔍 [JsonArrayTest] Starting JSON array test...");

        // Find JsonBoardDataManager
        var dataManager = FindFirstObjectByType<JsonBoardDataManager>();
        if (dataManager == null)
        {
            Debug.LogError("❌ [JsonArrayTest] JsonBoardDataManager not found!");
            return;
        }

        var jsonFile = dataManager.BoardDataJson;
        if (jsonFile == null)
        {
            Debug.LogError("❌ [JsonArrayTest] No JSON file assigned!");
            return;
        }

        // Test 1: Try with flat array structure
        Debug.Log("🔍 [JsonArrayTest] Test 1: Flat array structure...");
        try
        {
            FlatBoardData flatData = JsonUtility.FromJson<FlatBoardData>(jsonFile.text);
            Debug.Log($"  - flatData: {flatData != null}");
            if (flatData != null)
            {
                Debug.Log($"  - id: '{flatData.id}'");
                Debug.Log($"  - name: '{flatData.name}'");
                Debug.Log($"  - description: '{flatData.description}'");
                Debug.Log($"  - theme: '{flatData.theme}'");
                Debug.Log($"  - size: {flatData.size != null}");
                if (flatData.size != null)
                {
                    Debug.Log($"    - rows: {flatData.size.rows}");
                    Debug.Log($"    - columns: {flatData.size.columns}");
                }
                Debug.Log($"  - nodes: {flatData.nodes != null}");
                if (flatData.nodes != null)
                {
                    Debug.Log($"    - nodes.Length: {flatData.nodes.Length}");
                    if (flatData.nodes.Length > 0)
                    {
                        var firstNode = flatData.nodes[0];
                        Debug.Log($"    - First node: {firstNode != null}");
                        if (firstNode != null)
                        {
                            Debug.Log($"      - id: '{firstNode.id}'");
                            Debug.Log($"      - name: '{firstNode.name}'");
                            Debug.Log($"      - description: '{firstNode.description}'");
                            Debug.Log($"      - position: {firstNode.position != null}");
                            if (firstNode.position != null)
                            {
                                Debug.Log($"        - row: {firstNode.position.row}");
                                Debug.Log($"        - column: {firstNode.position.column}");
                            }
                            Debug.Log($"      - type: '{firstNode.type}'");
                            Debug.Log($"      - stats: {firstNode.stats != null}");
                            if (firstNode.stats != null)
                            {
                                Debug.Log($"        - strength: {firstNode.stats.strength}");
                                Debug.Log($"        - intelligence: {firstNode.stats.intelligence}");
                            }
                            Debug.Log($"      - maxRank: {firstNode.maxRank}");
                            Debug.Log($"      - currentRank: {firstNode.currentRank}");
                            Debug.Log($"      - cost: {firstNode.cost}");
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [JsonArrayTest] Flat array test failed: {e.Message}");
        }

        // Test 2: Try with the original structure but with debug info
        Debug.Log("🔍 [JsonArrayTest] Test 2: Original structure with debug...");
        try
        {
            JsonBoardData originalData = JsonUtility.FromJson<JsonBoardData>(jsonFile.text);
            Debug.Log($"  - originalData: {originalData != null}");
            if (originalData != null)
            {
                Debug.Log($"  - id: '{originalData.id}'");
                Debug.Log($"  - name: '{originalData.name}'");
                Debug.Log($"  - description: '{originalData.description}'");
                Debug.Log($"  - theme: '{originalData.theme}'");
                Debug.Log($"  - size: {originalData.size != null}");
                if (originalData.size != null)
                {
                    Debug.Log($"    - rows: {originalData.size.rows}");
                    Debug.Log($"    - columns: {originalData.size.columns}");
                }
                Debug.Log($"  - nodes: {originalData.nodes != null}");
                if (originalData.nodes != null)
                {
                    Debug.Log($"    - nodes.Length: {originalData.nodes.Length}");
                    if (originalData.nodes.Length > 0)
                    {
                        Debug.Log($"    - First node: {originalData.nodes[0] != null}");
                        if (originalData.nodes[0] != null)
                        {
                            var firstNode = originalData.nodes[0];
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
                Debug.Log($"  - extensionPoints: {originalData.extensionPoints != null}");
                Debug.Log($"  - maxPoints: {originalData.maxPoints}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [JsonArrayTest] Original structure test failed: {e.Message}");
        }

        Debug.Log("🔍 [JsonArrayTest] JSON array test complete!");
    }

    /// <summary>
    /// Flat array board data structure for testing
    /// </summary>
    [System.Serializable]
    public class FlatBoardData
    {
        public string id;
        public string name;
        public string description;
        public string theme;
        public FlatSize size;
        public FlatNode[] nodes;
    }

    [System.Serializable]
    public class FlatSize
    {
        public int rows;
        public int columns;
    }

    [System.Serializable]
    public class FlatNode
    {
        public string id;
        public string name;
        public string description;
        public FlatPosition position;
        public string type;
        public FlatStats stats;
        public int maxRank;
        public int currentRank;
        public int cost;
    }

    [System.Serializable]
    public class FlatPosition
    {
        public int row;
        public int column;
    }

    [System.Serializable]
    public class FlatStats
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

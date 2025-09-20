using UnityEngine;
using PassiveTree;

/// <summary>
/// Test script to debug JSON parsing issues
/// </summary>
public class JsonParsingTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool testOnStart = true;

    void Start()
    {
        if (testOnStart)
        {
            TestJsonParsing();
        }
    }

    /// <summary>
    /// Test JSON parsing with detailed debugging
    /// </summary>
    [ContextMenu("Test JSON Parsing")]
    public void TestJsonParsing()
    {
        Debug.Log("🧪 [JsonParsingTest] Starting JSON parsing test...");

        // Find JsonBoardDataManager
        var dataManager = FindFirstObjectByType<JsonBoardDataManager>();
        if (dataManager == null)
        {
            Debug.LogError("❌ [JsonParsingTest] JsonBoardDataManager not found!");
            return;
        }

        var jsonFile = dataManager.BoardDataJson;
        if (jsonFile == null)
        {
            Debug.LogError("❌ [JsonParsingTest] No JSON file assigned!");
            return;
        }

        Debug.Log($"✅ [JsonParsingTest] JSON file: {jsonFile.name}");

        // Test 1: Parse with JsonBoardData
        Debug.Log("🔍 [JsonParsingTest] Test 1: Parsing with JsonBoardData...");
        try
        {
            JsonBoardData boardData = JsonUtility.FromJson<JsonBoardData>(jsonFile.text);
            Debug.Log($"  - boardData: {boardData != null}");
            if (boardData != null)
            {
                Debug.Log($"  - id: {boardData.id}");
                Debug.Log($"  - name: {boardData.name}");
                Debug.Log($"  - description: {boardData.description}");
                Debug.Log($"  - theme: {boardData.theme}");
                Debug.Log($"  - size: {boardData.size != null}");
                if (boardData.size != null)
                {
                    Debug.Log($"    - rows: {boardData.size.rows}");
                    Debug.Log($"    - columns: {boardData.size.columns}");
                }
                Debug.Log($"  - nodes: {boardData.nodes != null}");
                if (boardData.nodes != null)
                {
                    Debug.Log($"    - nodes.Length: {boardData.nodes.Length}");
                }
                Debug.Log($"  - extensionPoints: {boardData.extensionPoints != null}");
                Debug.Log($"  - maxPoints: {boardData.maxPoints}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [JsonParsingTest] Test 1 failed: {e.Message}");
        }

        // Test 2: Parse with a simpler structure
        Debug.Log("🔍 [JsonParsingTest] Test 2: Parsing with SimpleBoardData...");
        try
        {
            SimpleBoardData simpleData = JsonUtility.FromJson<SimpleBoardData>(jsonFile.text);
            Debug.Log($"  - simpleData: {simpleData != null}");
            if (simpleData != null)
            {
                Debug.Log($"  - id: {simpleData.id}");
                Debug.Log($"  - name: {simpleData.name}");
                Debug.Log($"  - description: {simpleData.description}");
                Debug.Log($"  - theme: {simpleData.theme}");
                Debug.Log($"  - size: {simpleData.size != null}");
                if (simpleData.size != null)
                {
                    Debug.Log($"    - rows: {simpleData.size.rows}");
                    Debug.Log($"    - columns: {simpleData.size.columns}");
                }
                Debug.Log($"  - nodes: {simpleData.nodes != null}");
                if (simpleData.nodes != null)
                {
                    Debug.Log($"    - nodes.Length: {simpleData.nodes.Length}");
                    if (simpleData.nodes.Length > 0)
                    {
                        Debug.Log($"    - First row: {simpleData.nodes[0] != null}");
                        if (simpleData.nodes[0] != null)
                        {
                            Debug.Log($"      - First row length: {simpleData.nodes[0].Length}");
                            if (simpleData.nodes[0].Length > 0)
                            {
                                Debug.Log($"      - First node: {simpleData.nodes[0][0] != null}");
                                if (simpleData.nodes[0][0] != null)
                                {
                                    var firstNode = simpleData.nodes[0][0];
                                    Debug.Log($"        - id: {firstNode.id}");
                                    Debug.Log($"        - name: {firstNode.name}");
                                    Debug.Log($"        - description: {firstNode.description}");
                                    Debug.Log($"        - position: {firstNode.position != null}");
                                    if (firstNode.position != null)
                                    {
                                        Debug.Log($"          - row: {firstNode.position.row}");
                                        Debug.Log($"          - column: {firstNode.position.column}");
                                    }
                                    Debug.Log($"        - type: {firstNode.type}");
                                    Debug.Log($"        - stats: {firstNode.stats != null}");
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
            Debug.LogError($"❌ [JsonParsingTest] Test 2 failed: {e.Message}");
        }

        Debug.Log("🧪 [JsonParsingTest] JSON parsing test complete!");
    }

    /// <summary>
    /// Simple board data structure for testing
    /// </summary>
    [System.Serializable]
    public class SimpleBoardData
    {
        public string id;
        public string name;
        public string description;
        public string theme;
        public SimpleSize size;
        public SimpleNode[][] nodes;
    }

    [System.Serializable]
    public class SimpleSize
    {
        public int rows;
        public int columns;
    }

    [System.Serializable]
    public class SimpleNode
    {
        public string id;
        public string name;
        public string description;
        public SimplePosition position;
        public string type;
        public SimpleStats stats;
        public int maxRank;
        public int currentRank;
        public int cost;
    }

    [System.Serializable]
    public class SimplePosition
    {
        public int row;
        public int column;
    }

    [System.Serializable]
    public class SimpleStats
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




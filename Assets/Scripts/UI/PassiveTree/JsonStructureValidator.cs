using UnityEngine;
using PassiveTree;

/// <summary>
/// Validates JSON structure and shows what's actually in the file
/// </summary>
public class JsonStructureValidator : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool validateOnStart = true;

    void Start()
    {
        if (validateOnStart)
        {
            ValidateJsonStructure();
        }
    }

    /// <summary>
    /// Validate the JSON structure
    /// </summary>
    [ContextMenu("Validate JSON Structure")]
    public void ValidateJsonStructure()
    {
        Debug.Log("🔍 [JsonStructureValidator] Starting JSON structure validation...");

        // Find JsonBoardDataManager
        var dataManager = FindFirstObjectByType<JsonBoardDataManager>();
        if (dataManager == null)
        {
            Debug.LogError("❌ [JsonStructureValidator] JsonBoardDataManager not found!");
            return;
        }

        var jsonFile = dataManager.BoardDataJson;
        if (jsonFile == null)
        {
            Debug.LogError("❌ [JsonStructureValidator] No JSON file assigned!");
            return;
        }

        Debug.Log($"✅ [JsonStructureValidator] JSON file: {jsonFile.name}");
        Debug.Log($"📄 [JsonStructureValidator] File size: {jsonFile.text.Length} characters");

        // Show first 500 characters of JSON
        string preview = jsonFile.text.Substring(0, Mathf.Min(500, jsonFile.text.Length));
        Debug.Log($"📋 [JsonStructureValidator] JSON preview (first 500 chars):");
        Debug.Log(preview);

        // Try to parse the JSON manually
        try
        {
            JsonBoardData boardData = JsonUtility.FromJson<JsonBoardData>(jsonFile.text);
            
            Debug.Log($"✅ [JsonStructureValidator] JSON parsing successful!");
            Debug.Log($"  - boardData: {boardData != null}");
            
            if (boardData != null)
            {
                Debug.Log($"  - boardData.nodes: {boardData.nodes != null}");
                
                if (boardData.nodes != null)
                {
                    Debug.Log($"  - boardData.nodes.Length: {boardData.nodes.Length}");
                    
                    // Check each node
                    for (int i = 0; i < Mathf.Min(5, boardData.nodes.Length); i++)
                    {
                        var node = boardData.nodes[i];
                        Debug.Log($"    - Node {i}: {node?.name ?? "null"} at ({node?.position.column ?? -1}, {node?.position.row ?? -1})");
                    }
                }
                else
                {
                    Debug.LogError("❌ [JsonStructureValidator] boardData.nodes is null!");
                }
            }
            else
            {
                Debug.LogError("❌ [JsonStructureValidator] boardData is null after parsing!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [JsonStructureValidator] JSON parsing failed: {e.Message}");
            Debug.LogError($"❌ [JsonStructureValidator] Stack trace: {e.StackTrace}");
        }

        // Try alternative parsing approach
        Debug.Log("🔄 [JsonStructureValidator] Trying alternative parsing approach...");
        
        try
        {
            // Try parsing as a simple object first
            var simpleData = JsonUtility.FromJson<SimpleJsonData>(jsonFile.text);
            Debug.Log($"✅ [JsonStructureValidator] Simple parsing successful!");
            Debug.Log($"  - id: {simpleData.id}");
            Debug.Log($"  - name: {simpleData.name}");
            Debug.Log($"  - description: {simpleData.description}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [JsonStructureValidator] Simple parsing also failed: {e.Message}");
        }

        Debug.Log("🔍 [JsonStructureValidator] JSON structure validation complete!");
    }

    /// <summary>
    /// Simple JSON data structure for testing
    /// </summary>
    [System.Serializable]
    public class SimpleJsonData
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

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PassiveTree;

/// <summary>
/// Integrates passive tree stats with the CharacterStatsData system
/// Applies passive tree node effects to character stats when nodes are allocated
/// </summary>
public class PassiveTreeStatsIntegration : MonoBehaviour
{
    [Header("Integration Settings")]
    [SerializeField] private bool autoIntegrateOnStart = true;
    [SerializeField] private bool debugMode = false;
    [SerializeField] private bool validateStatsOnApply = true;

    [Header("References")]
    [SerializeField] private CharacterStatsData characterStats;
    [SerializeField] private PassiveTreeManager passiveTreeManager;
    [SerializeField] private EnhancedBoardDataManager boardDataManager;

    // Runtime data
    private Dictionary<Vector2Int, List<string>> appliedStats = new Dictionary<Vector2Int, List<string>>();
    private bool isIntegrationActive = false;

    void Start()
    {
        if (autoIntegrateOnStart)
        {
            SetupIntegration();
        }
    }

    /// <summary>
    /// Set up the passive tree stats integration
    /// </summary>
    [ContextMenu("Setup Integration")]
    public void SetupIntegration()
    {
        // Find required components
        if (characterStats == null)
        {
            // CharacterStatsData is not a MonoBehaviour, so we need to find it differently
            // Look for a component that has CharacterStatsData
            var characterManager = FindFirstObjectByType<CharacterManager>();
            if (characterManager != null)
            {
                // Try to get CharacterStatsData from CharacterManager
                var characterStatsField = typeof(CharacterManager).GetField("characterStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (characterStatsField != null)
                {
                    characterStats = characterStatsField.GetValue(characterManager) as CharacterStatsData;
                }
            }
            
            if (characterStats == null)
            {
                Debug.LogWarning("[PassiveTreeStatsIntegration] No CharacterStatsData found. Creating one...");
                CreateCharacterStatsData();
            }
        }

        if (passiveTreeManager == null)
        {
            passiveTreeManager = FindFirstObjectByType<PassiveTreeManager>();
        }

        if (boardDataManager == null)
        {
            boardDataManager = FindFirstObjectByType<EnhancedBoardDataManager>();
        }

        // Subscribe to passive tree events
        if (passiveTreeManager != null)
        {
            // Note: We'll need to add events to PassiveTreeManager for node allocation
            Debug.Log("[PassiveTreeStatsIntegration] Integration setup complete");
        }

        isIntegrationActive = true;
    }

    /// <summary>
    /// Apply stats from a passive tree node to character stats
    /// </summary>
    public void ApplyNodeStats(Vector2Int nodePosition, bool isAllocated)
    {
        if (!isIntegrationActive || characterStats == null)
        {
            Debug.LogWarning("[PassiveTreeStatsIntegration] Integration not active or character stats not found");
            return;
        }

        if (boardDataManager == null)
        {
            Debug.LogWarning("[PassiveTreeStatsIntegration] Board data manager not found");
            return;
        }

        // Get node data
        PassiveNodeData nodeData = boardDataManager.GetNodeData(nodePosition);
        if (nodeData == null)
        {
            if (debugMode)
            {
                Debug.LogWarning($"[PassiveTreeStatsIntegration] No node data found for position {nodePosition}");
            }
            return;
        }

        if (isAllocated)
        {
            // Apply stats - convert Dictionary to List<string> format
            List<string> statsList = ConvertStatsDictionaryToList(nodeData.GetStats());
            ApplyStatsToCharacter(nodePosition, statsList);
        }
        else
        {
            // Remove stats
            RemoveStatsFromCharacter(nodePosition);
        }

        if (debugMode)
        {
            Debug.Log($"[PassiveTreeStatsIntegration] {(isAllocated ? "Applied" : "Removed")} stats for node {nodePosition}: {nodeData.NodeName}");
        }
    }

    /// <summary>
    /// Apply stats from a node to character stats
    /// </summary>
    private void ApplyStatsToCharacter(Vector2Int nodePosition, List<string> stats)
    {
        if (stats == null || stats.Count == 0)
            return;

        List<string> appliedNodeStats = new List<string>();

        foreach (string stat in stats)
        {
            if (string.IsNullOrEmpty(stat))
                continue;

            // Parse stat format: "statName: value"
            string[] parts = stat.Split(':');
            if (parts.Length == 2)
            {
                string statName = parts[0].Trim();
                string valueString = parts[1].Trim();

                if (float.TryParse(valueString, out float value))
                {
                    // Validate stat name if enabled
                    if (validateStatsOnApply && !IsValidStatName(statName))
                    {
                        Debug.LogWarning($"[PassiveTreeStatsIntegration] Invalid stat name: {statName}");
                        continue;
                    }

                    // Apply the stat
                    characterStats.AddToStat(statName, value);
                    appliedNodeStats.Add(stat);

                    if (debugMode)
                    {
                        Debug.Log($"[PassiveTreeStatsIntegration] Applied stat: {statName} +{value}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[PassiveTreeStatsIntegration] Could not parse stat value: {valueString}");
                }
            }
            else
            {
                Debug.LogWarning($"[PassiveTreeStatsIntegration] Invalid stat format: {stat}");
            }
        }

        // Store applied stats for this node
        appliedStats[nodePosition] = appliedNodeStats;
    }

    /// <summary>
    /// Remove stats from a node from character stats
    /// </summary>
    private void RemoveStatsFromCharacter(Vector2Int nodePosition)
    {
        if (!appliedStats.ContainsKey(nodePosition))
            return;

        List<string> nodeStats = appliedStats[nodePosition];

        foreach (string stat in nodeStats)
        {
            if (string.IsNullOrEmpty(stat))
                continue;

            // Parse stat format: "statName: value"
            string[] parts = stat.Split(':');
            if (parts.Length == 2)
            {
                string statName = parts[0].Trim();
                string valueString = parts[1].Trim();

                if (float.TryParse(valueString, out float value))
                {
                    // Remove the stat (subtract the value)
                    characterStats.AddToStat(statName, -value);

                    if (debugMode)
                    {
                        Debug.Log($"[PassiveTreeStatsIntegration] Removed stat: {statName} -{value}");
                    }
                }
            }
        }

        // Remove from applied stats
        appliedStats.Remove(nodePosition);
    }

    /// <summary>
    /// Check if a stat name is valid in the character stats system
    /// </summary>
    private bool IsValidStatName(string statName)
    {
        if (string.IsNullOrEmpty(statName))
            return false;

        // Try to get the stat value - if it doesn't throw an error, it's valid
        try
        {
            float value = characterStats.GetStatValue(statName);
            return true; // If we can get a value, the stat exists
        }
        catch
        {
            // Check if it's a custom stat in equipment stats
            return characterStats.equipmentStats.ContainsKey(statName);
        }
    }

    /// <summary>
    /// Apply stats from all allocated nodes
    /// </summary>
    [ContextMenu("Apply All Allocated Node Stats")]
    public void ApplyAllAllocatedNodeStats()
    {
        if (!isIntegrationActive || passiveTreeManager == null || boardDataManager == null)
        {
            Debug.LogWarning("[PassiveTreeStatsIntegration] Integration not properly set up");
            return;
        }

        // Get all cells from passive tree manager
        var allCells = passiveTreeManager.GetAllCells();
        
        foreach (var kvp in allCells)
        {
            Vector2Int position = kvp.Key;
            CellController cell = kvp.Value;

            if (cell.IsPurchased)
            {
                ApplyNodeStats(position, true);
            }
        }

        Debug.Log($"[PassiveTreeStatsIntegration] Applied stats from {appliedStats.Count} allocated nodes");
    }

    /// <summary>
    /// Remove stats from all allocated nodes
    /// </summary>
    [ContextMenu("Remove All Allocated Node Stats")]
    public void RemoveAllAllocatedNodeStats()
    {
        if (!isIntegrationActive)
        {
            Debug.LogWarning("[PassiveTreeStatsIntegration] Integration not active");
            return;
        }

        // Remove stats from all applied nodes
        var positionsToRemove = new List<Vector2Int>(appliedStats.Keys);
        
        foreach (Vector2Int position in positionsToRemove)
        {
            RemoveStatsFromCharacter(position);
        }

        Debug.Log("[PassiveTreeStatsIntegration] Removed stats from all allocated nodes");
    }

    /// <summary>
    /// Get currently applied stats for a node
    /// </summary>
    public List<string> GetAppliedStatsForNode(Vector2Int nodePosition)
    {
        return appliedStats.ContainsKey(nodePosition) ? appliedStats[nodePosition] : new List<string>();
    }

    /// <summary>
    /// Get all currently applied stats
    /// </summary>
    public Dictionary<Vector2Int, List<string>> GetAllAppliedStats()
    {
        return new Dictionary<Vector2Int, List<string>>(appliedStats);
    }

    /// <summary>
    /// Create a CharacterStatsData instance if none exists
    /// </summary>
    private void CreateCharacterStatsData()
    {
        characterStats = new CharacterStatsData();
        Debug.Log("[PassiveTreeStatsIntegration] Created CharacterStatsData instance");
    }

    /// <summary>
    /// Set the character stats reference
    /// </summary>
    public void SetCharacterStats(CharacterStatsData stats)
    {
        characterStats = stats;
    }

    /// <summary>
    /// Set the passive tree manager reference
    /// </summary>
    public void SetPassiveTreeManager(PassiveTreeManager manager)
    {
        passiveTreeManager = manager;
    }

    /// <summary>
    /// Set the board data manager reference
    /// </summary>
    public void SetBoardDataManager(EnhancedBoardDataManager manager)
    {
        boardDataManager = manager;
    }

    /// <summary>
    /// Convert stats dictionary to list format
    /// </summary>
    private List<string> ConvertStatsDictionaryToList(Dictionary<string, float> statsDict)
    {
        List<string> statsList = new List<string>();
        
        if (statsDict != null)
        {
            foreach (var kvp in statsDict)
            {
                statsList.Add($"{kvp.Key}: {kvp.Value}");
            }
        }
        
        return statsList;
    }

    /// <summary>
    /// Validate all stat mappings
    /// </summary>
    [ContextMenu("Validate Stat Mappings")]
    public void ValidateStatMappings()
    {
        if (boardDataManager == null)
        {
            Debug.LogWarning("[PassiveTreeStatsIntegration] Board data manager not found");
            return;
        }

        var allNodeData = boardDataManager.GetAllNodeData();
        List<string> allStats = new List<string>();

        foreach (var kvp in allNodeData)
        {
            var statsDict = kvp.Value.GetStats();
            if (statsDict != null)
            {
                foreach (var statKvp in statsDict)
                {
                    allStats.Add($"{statKvp.Key}: {statKvp.Value}");
                }
            }
        }

        List<string> unmappedStats = PassiveTreeStatMapper.ValidateStatMappings(allStats);

        if (unmappedStats.Count > 0)
        {
            Debug.LogWarning($"[PassiveTreeStatsIntegration] Found {unmappedStats.Count} unmapped stats:");
            foreach (string stat in unmappedStats)
            {
                Debug.LogWarning($"  - {stat}");
            }
        }
        else
        {
            Debug.Log("[PassiveTreeStatsIntegration] All stats are properly mapped!");
        }
    }
}

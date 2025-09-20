using UnityEngine;
using PassiveTree;

/// <summary>
/// Simple setup script to configure the passive tree data system
/// Automatically creates and configures PassiveTreeBoardData and EnhancedBoardDataManager
/// </summary>
public class PassiveTreeDataSetup : MonoBehaviour
{
    [Header("Setup Configuration")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool createBoardDataAsset = true;
    [SerializeField] private bool debugMode = false; // Used for debug logging

    [Header("Board Data Settings")]
    [SerializeField] private string boardDataAssetName = "CorePassiveTreeData";
    [SerializeField] private Vector2Int boardSize = new Vector2Int(7, 7);
    [SerializeField] private string boardName = "Core Passive Tree"; // Used for board creation
    [SerializeField] private string boardDescription = "The main passive tree board"; // Used for board creation

    [Header("Components")]
    [SerializeField] private PassiveTreeBoardData boardData;
    [SerializeField] private EnhancedBoardDataManager dataManager;

    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupPassiveTreeData();
        }
    }

    /// <summary>
    /// Set up the passive tree data system
    /// </summary>
    [ContextMenu("Setup Passive Tree Data")]
    public void SetupPassiveTreeData()
    {
        Debug.Log("[PassiveTreeDataSetup] Setting up passive tree data system...");

        // Find or create board data
        if (boardData == null)
        {
            boardData = FindOrCreateBoardData();
        }

        // Find or create data manager
        if (dataManager == null)
        {
            dataManager = FindOrCreateDataManager();
        }

        // Configure data manager
        if (dataManager != null && boardData != null)
        {
            dataManager.SetBoardData(boardData);
            Debug.Log("[PassiveTreeDataSetup] Connected board data to data manager");
        }

        // Generate cell data if needed
        if (boardData != null)
        {
            boardData.RegenerateAllCellData();
            Debug.Log("[PassiveTreeDataSetup] Generated cell data");
        }

        Debug.Log("[PassiveTreeDataSetup] Setup complete!");
    }

    /// <summary>
    /// Find or create PassiveTreeBoardData
    /// </summary>
    private PassiveTreeBoardData FindOrCreateBoardData()
    {
        // Try to find existing board data
        PassiveTreeBoardData existing = FindFirstObjectByType<PassiveTreeBoardData>();
        if (existing != null)
        {
            Debug.Log("[PassiveTreeDataSetup] Found existing board data");
            return existing;
        }

        // Create new board data
        if (createBoardDataAsset)
        {
            boardData = ScriptableObject.CreateInstance<PassiveTreeBoardData>();
            boardData.name = boardDataAssetName;
            
            #if UNITY_EDITOR
            // Save as asset in editor
            string assetPath = $"Assets/Scripts/Data/PassiveTree/{boardDataAssetName}.asset";
            UnityEditor.AssetDatabase.CreateAsset(boardData, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"[PassiveTreeDataSetup] Created board data asset: {assetPath}");
            #endif
        }
        else
        {
            boardData = ScriptableObject.CreateInstance<PassiveTreeBoardData>();
            boardData.name = boardDataAssetName;
            Debug.Log("[PassiveTreeDataSetup] Created runtime board data");
        }

        return boardData;
    }

    /// <summary>
    /// Find or create EnhancedBoardDataManager
    /// </summary>
    private EnhancedBoardDataManager FindOrCreateDataManager()
    {
        // Try to find existing data manager
        EnhancedBoardDataManager existing = FindFirstObjectByType<EnhancedBoardDataManager>();
        if (existing != null)
        {
            Debug.Log("[PassiveTreeDataSetup] Found existing data manager");
            return existing;
        }

        // Create new data manager
        GameObject managerGO = new GameObject("EnhancedBoardDataManager");
        dataManager = managerGO.AddComponent<EnhancedBoardDataManager>();
        Debug.Log("[PassiveTreeDataSetup] Created data manager");

        return dataManager;
    }

    /// <summary>
    /// Assign data to all cells in the scene
    /// </summary>
    [ContextMenu("Assign Data to All Cells")]
    public void AssignDataToAllCells()
    {
        if (dataManager == null)
        {
            Debug.LogWarning("[PassiveTreeDataSetup] No data manager found. Please run Setup Passive Tree Data first.");
            return;
        }

        dataManager.AssignDataToAllCells();
    }

    /// <summary>
    /// Test the data system
    /// </summary>
    [ContextMenu("Test Data System")]
    public void TestDataSystem()
    {
        if (dataManager == null)
        {
            Debug.LogWarning("[PassiveTreeDataSetup] No data manager found. Please run Setup Passive Tree Data first.");
            return;
        }

        // Test getting data for a few positions
        Vector2Int[] testPositions = {
            new Vector2Int(3, 3), // Start
            new Vector2Int(0, 3), // Extension
            new Vector2Int(1, 3), // Travel
            new Vector2Int(1, 1)  // Notable
        };

        Debug.Log("[PassiveTreeDataSetup] Testing data system...");
        
        foreach (Vector2Int pos in testPositions)
        {
            PassiveNodeData data = dataManager.GetNodeData(pos);
            if (data != null)
            {
                Debug.Log($"  Position {pos}: {data.NodeName} ({data.NodeType}) - {data.Description}");
            }
            else
            {
                Debug.LogWarning($"  Position {pos}: No data found");
            }
        }

        Debug.Log($"[PassiveTreeDataSetup] Data system test complete. Total entries: {dataManager.GetNodeDataCount()}");
    }

    /// <summary>
    /// Get the board data
    /// </summary>
    public PassiveTreeBoardData GetBoardData()
    {
        return boardData;
    }

    /// <summary>
    /// Get the data manager
    /// </summary>
    public EnhancedBoardDataManager GetDataManager()
    {
        return dataManager;
    }

    /// <summary>
    /// Set the board data
    /// </summary>
    public void SetBoardData(PassiveTreeBoardData newBoardData)
    {
        boardData = newBoardData;
        if (dataManager != null)
        {
            dataManager.SetBoardData(newBoardData);
        }
    }
}

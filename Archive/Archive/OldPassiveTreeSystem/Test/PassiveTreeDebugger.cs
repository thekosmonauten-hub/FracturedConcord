using UnityEngine;
using UnityEngine.UI;
using PassiveTree;

/// <summary>
/// Debug script to help identify passive tree setup issues
/// </summary>
public class PassiveTreeDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool runOnStart = true;
    
    private void Start()
    {
        if (runOnStart)
        {
            DebugPassiveTreeSetup();
        }
    }
    
    /// <summary>
    /// Debug the entire passive tree setup
    /// </summary>
    [ContextMenu("Debug Passive Tree Setup")]
    public void DebugPassiveTreeSetup()
    {
        Debug.Log("=== PASSIVE TREE DEBUG REPORT ===");
        
        // Check PassiveTreeManager
        DebugPassiveTreeManager();
        
        // Check Board UI
        DebugBoardUI();
        
        // Check Prefabs
        DebugPrefabs();
        
        // Check Canvas
        DebugCanvas();
        
        Debug.Log("=== END DEBUG REPORT ===");
    }
    
    /// <summary>
    /// Debug PassiveTreeManager setup
    /// </summary>
    private void DebugPassiveTreeManager()
    {
        Debug.Log("--- PassiveTreeManager Check ---");
        
        var manager = PassiveTreeManager.Instance;
        if (manager == null)
        {
            Debug.LogError("❌ PassiveTreeManager.Instance is NULL!");
            Debug.LogError("   - Check if PassiveTreeManager exists in scene");
            Debug.LogError("   - Check if PassiveTreeManager component is enabled");
            return;
        }
        
        Debug.Log("✅ PassiveTreeManager found");
        
        // Check core board assignment
        if (manager.coreBoardAsset == null)
        {
            Debug.LogError("❌ Core Board Asset is NOT ASSIGNED!");
            Debug.LogError("   - Assign your CoreBoard ScriptableObject to PassiveTreeManager");
        }
        else
        {
            Debug.Log($"✅ Core Board Asset: {manager.coreBoardAsset.name}");
        }
        
        // Check passive tree data
        if (manager.PassiveTree == null)
        {
            Debug.LogError("❌ Passive Tree data is NULL!");
            Debug.LogError("   - PassiveTreeManager may not be initialized");
        }
        else
        {
            Debug.Log("✅ Passive Tree data exists");
            
            if (manager.PassiveTree.coreBoard == null)
            {
                Debug.LogError("❌ Core Board data is NULL!");
            }
            else
            {
                var nodeCount = manager.PassiveTree.coreBoard.GetAllNodes().Count;
                Debug.Log($"✅ Core Board has {nodeCount} nodes");
            }
        }
    }
    
    /// <summary>
    /// Debug Board UI setup
    /// </summary>
    private void DebugBoardUI()
    {
        Debug.Log("--- Board UI Check ---");
        
        var boardUI = FindFirstObjectByType<PassiveTreeBoardUI>();
        if (boardUI == null)
        {
            Debug.LogError("❌ PassiveTreeBoardUI not found in scene!");
            Debug.LogError("   - Add PassiveTreeBoardUI component to a GameObject");
            return;
        }
        
        Debug.Log("✅ PassiveTreeBoardUI found");
        
        // Check prefab assignments using public properties
        if (boardUI.NodePrefab == null)
        {
            Debug.LogError("❌ Node Prefab is NOT ASSIGNED!");
            Debug.LogError("   - Assign your NodePrefab to PassiveTreeBoardUI");
        }
        else
        {
            Debug.Log($"✅ Node Prefab: {boardUI.NodePrefab.name}");
            
            // Check if prefab has required components
            var nodeUI = boardUI.NodePrefab.GetComponent<PassiveTreeNodeUI>();
            if (nodeUI == null)
            {
                Debug.LogError("❌ NodePrefab missing PassiveTreeNodeUI component!");
            }
            else
            {
                Debug.Log("✅ NodePrefab has PassiveTreeNodeUI component");
            }
        }
        
        if (boardUI.ConnectionLinePrefab == null)
        {
            Debug.LogWarning("⚠️ Connection Line Prefab is NOT ASSIGNED!");
            Debug.LogWarning("   - Connections won't be drawn");
        }
        else
        {
            Debug.Log($"✅ Connection Line Prefab: {boardUI.ConnectionLinePrefab.name}");
        }
        
        // Check board data
        if (boardUI.BoardData == null)
        {
            Debug.LogError("❌ Board Data is NULL!");
            Debug.LogError("   - Use 'Set Board Data from Manager' context menu");
        }
        else
        {
            Debug.Log($"✅ Board Data: {boardUI.BoardData.name}");
            var nodeCount = boardUI.BoardData.GetAllNodes().Count;
            Debug.Log($"   - Board has {nodeCount} nodes");
        }
    }
    
    /// <summary>
    /// Debug prefab setup
    /// </summary>
    private void DebugPrefabs()
    {
        Debug.Log("--- Prefab Check ---");
        
        // Find all PassiveTreeNodeUI prefabs in scene
        var nodeUIs = FindObjectsByType<PassiveTreeNodeUI>(FindObjectsSortMode.None);
        Debug.Log($"Found {nodeUIs.Length} PassiveTreeNodeUI instances in scene");
        
        if (nodeUIs.Length == 0)
        {
            Debug.LogWarning("⚠️ No PassiveTreeNodeUI instances found!");
            Debug.LogWarning("   - This might be normal if board hasn't been created yet");
        }
        else
        {
            foreach (var nodeUI in nodeUIs)
            {
                Debug.Log($"   - {nodeUI.name} (State: {nodeUI.CurrentState})");
            }
        }
    }
    
    /// <summary>
    /// Debug Canvas setup
    /// </summary>
    private void DebugCanvas()
    {
        Debug.Log("--- Canvas Check ---");
        
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ No Canvas found in scene!");
            Debug.LogError("   - Add a Canvas to your scene");
            return;
        }
        
        Debug.Log("✅ Canvas found");
        
        // Check Canvas settings
        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Debug.LogWarning($"⚠️ Canvas Render Mode is {canvas.renderMode}");
            Debug.LogWarning("   - Consider using Screen Space - Overlay for UI");
        }
        
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            Debug.LogWarning("⚠️ Canvas has no CanvasScaler component");
        }
        else
        {
            Debug.Log($"✅ Canvas Scaler: {scaler.uiScaleMode}");
        }
        
        // Check EventSystem
        var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogError("❌ No EventSystem found in scene!");
            Debug.LogError("   - Add EventSystem for UI interactions");
        }
        else
        {
            Debug.Log("✅ EventSystem found");
        }
    }
    
    /// <summary>
    /// Force refresh the board visual
    /// </summary>
    [ContextMenu("Force Refresh Board")]
    public void ForceRefreshBoard()
    {
        Debug.Log("--- Force Refresh Board ---");
        
        var boardUI = FindFirstObjectByType<PassiveTreeBoardUI>();
        if (boardUI != null)
        {
            boardUI.RefreshBoardVisual();
            Debug.Log("✅ Board visual refreshed");
        }
        else
        {
            Debug.LogError("❌ No PassiveTreeBoardUI found to refresh");
        }
    }
    
    /// <summary>
    /// Add test skill points
    /// </summary>
    [ContextMenu("Add Test Skill Points")]
    public void AddTestSkillPoints()
    {
        Debug.Log("--- Adding Test Skill Points ---");
        
        var manager = PassiveTreeManager.Instance;
        if (manager != null)
        {
            manager.AddSkillPoints(5);
            Debug.Log("✅ Added 5 skill points");
        }
        else
        {
            Debug.LogError("❌ No PassiveTreeManager found");
        }
    }
}

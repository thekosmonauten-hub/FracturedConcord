using UnityEngine;
using PassiveTree;
using System.Collections.Generic;

/// <summary>
/// Comprehensive diagnostic script to identify why passive tree nodes aren't appearing
/// </summary>
public class PassiveTreeNodeVisibilityDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool runOnStart = true;
    public bool showDetailedInfo = true;
    
    private void Start()
    {
        if (runOnStart)
        {
            Debug.Log("=== PASSIVE TREE NODE VISIBILITY DIAGNOSTIC ===");
            RunFullDiagnostic();
        }
    }
    
    [ContextMenu("Run Full Diagnostic")]
    public void RunFullDiagnostic()
    {
        Debug.Log("🔍 Starting comprehensive node visibility diagnostic...");
        
        // 1. Check PassiveTreeManager
        CheckPassiveTreeManager();
        
        // 2. Check CoreBoard Data
        CheckCoreBoardData();
        
        // 3. Check UI Components
        CheckUIComponents();
        
        // 4. Check Canvas and Camera
        CheckCanvasAndCamera();
        
        // 5. Check Node Prefabs
        CheckNodePrefabs();
        
        // 6. Force Refresh
        ForceRefreshAll();
        
        Debug.Log("=== END DIAGNOSTIC ===");
    }
    
    private void CheckPassiveTreeManager()
    {
        Debug.Log("\n📋 1. PASSIVE TREE MANAGER CHECK");
        
        var manager = FindFirstObjectByType<PassiveTreeManager>();
        if (manager == null)
        {
            Debug.LogError("❌ PassiveTreeManager not found in scene!");
            Debug.LogError("   → Add PassiveTreeManager GameObject to scene");
            return;
        }
        
        Debug.Log("✅ PassiveTreeManager found");
        
        if (manager.PassiveTree == null)
        {
            Debug.LogError("❌ PassiveTree is null!");
            Debug.LogError("   → Check if CoreBoard asset is assigned");
            return;
        }
        
        Debug.Log("✅ PassiveTree data exists");
        
        if (manager.PassiveTree.coreBoard == null)
        {
            Debug.LogError("❌ CoreBoard is null!");
            Debug.LogError("   → Assign CoreBoard ScriptableObject to manager");
            return;
        }
        
        Debug.Log("✅ CoreBoard exists");
        
        var nodeCount = manager.PassiveTree.coreBoard.GetAllNodes().Count;
        Debug.Log($"📊 CoreBoard has {nodeCount} nodes");
        
        if (nodeCount == 0)
        {
            Debug.LogError("❌ CoreBoard has 0 nodes!");
            Debug.LogError("   → Use 'Force Initialize CoreBoard' context menu");
        }
    }
    
    private void CheckCoreBoardData()
    {
        Debug.Log("\n📊 2. CORE BOARD DATA CHECK");
        
        var manager = FindFirstObjectByType<PassiveTreeManager>();
        if (manager?.PassiveTree?.coreBoard == null)
        {
            Debug.LogError("❌ Cannot check CoreBoard data - manager or board is null");
            return;
        }
        
        var board = manager.PassiveTree.coreBoard;
        Debug.Log($"📋 Board: {board.name} ({board.id})");
        Debug.Log($"📏 Size: {board.size.x}x{board.size.y}");
        Debug.Log($"🎯 Theme: {board.theme}");
        
        var allNodes = board.GetAllNodes();
        Debug.Log($"📊 Total nodes: {allNodes.Count}");
        
        if (allNodes.Count > 0)
        {
            var sampleNode = allNodes[0];
            Debug.Log($"🔍 Sample node: {sampleNode.name} ({sampleNode.id}) at {sampleNode.position}");
            Debug.Log($"   Type: {sampleNode.type}, Cost: {sampleNode.cost}");
        }
        
        // Check for starting node
        var startNode = board.GetNode(3, 3);
        if (startNode != null)
        {
            Debug.Log($"🎯 Starting node found: {startNode.name} at (3,3)");
            Debug.Log($"   Current rank: {startNode.currentRank}/{startNode.maxRank}");
        }
        else
        {
            Debug.LogError("❌ Starting node not found at (3,3)!");
        }
    }
    
    private void CheckUIComponents()
    {
        Debug.Log("\n🖥️ 3. UI COMPONENTS CHECK");
        
        var boardUI = FindFirstObjectByType<PassiveTreeBoardUI>();
        if (boardUI == null)
        {
            Debug.LogError("❌ PassiveTreeBoardUI not found in scene!");
            Debug.LogError("   → Add PassiveTreeBoardUI component to a GameObject");
            return;
        }
        
        Debug.Log("✅ PassiveTreeBoardUI found");
        
        // Check board data assignment
        if (boardUI.BoardData == null)
        {
            Debug.LogError("❌ BoardUI has no board data assigned!");
            Debug.LogError("   → Board data should be auto-assigned from manager");
        }
        else
        {
            Debug.Log($"✅ BoardUI has board data: {boardUI.BoardData.name}");
        }
        
        // Check prefab assignments
        if (boardUI.NodePrefab == null)
        {
            Debug.LogError("❌ Node prefab not assigned to BoardUI!");
            Debug.LogError("   → Assign NodePrefab in inspector");
        }
        else
        {
            Debug.Log($"✅ Node prefab assigned: {boardUI.NodePrefab.name}");
        }
        
        if (boardUI.ConnectionLinePrefab == null)
        {
            Debug.LogWarning("⚠️ Connection line prefab not assigned (optional)");
        }
        else
        {
            Debug.Log($"✅ Connection line prefab assigned: {boardUI.ConnectionLinePrefab.name}");
        }
        
        // Check created UI instances
        var nodeUIs = FindObjectsByType<PassiveTreeNodeUI>(FindObjectsSortMode.None);
        Debug.Log($"📊 Found {nodeUIs.Length} PassiveTreeNodeUI instances in scene");
        
        if (nodeUIs.Length == 0)
        {
            Debug.LogError("❌ No PassiveTreeNodeUI instances found!");
            Debug.LogError("   → BoardUI should create node instances automatically");
        }
        else
        {
            Debug.Log($"✅ {nodeUIs.Length} node UI instances exist");
            foreach (var nodeUI in nodeUIs)
            {
                Debug.Log($"   - {nodeUI.name} (Active: {nodeUI.gameObject.activeInHierarchy})");
            }
        }
    }
    
    private void CheckCanvasAndCamera()
    {
        Debug.Log("\n🎨 4. CANVAS AND CAMERA CHECK");
        
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ No Canvas found in scene!");
            Debug.LogError("   → Add Canvas to scene for UI rendering");
            return;
        }
        
        Debug.Log("✅ Canvas found");
        Debug.Log($"   Render mode: {canvas.renderMode}");
        Debug.Log($"   Active: {canvas.gameObject.activeInHierarchy}");
        
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Debug.Log("✅ ScreenSpaceOverlay mode (no camera needed)");
        }
        else
        {
            var camera = canvas.worldCamera;
            if (camera == null)
            {
                Debug.LogError("❌ Canvas needs camera but none assigned!");
                Debug.LogError("   → Assign camera to canvas or use ScreenSpaceOverlay");
            }
            else
            {
                Debug.Log($"✅ Canvas camera assigned: {camera.name}");
            }
        }
        
        // Check for EventSystem
        var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogError("❌ No EventSystem found!");
            Debug.LogError("   → Add EventSystem for UI interactions");
        }
        else
        {
            Debug.Log("✅ EventSystem found");
        }
    }
    
    private void CheckNodePrefabs()
    {
        Debug.Log("\n🎯 5. NODE PREFAB CHECK");
        
        var boardUI = FindFirstObjectByType<PassiveTreeBoardUI>();
        if (boardUI?.NodePrefab == null)
        {
            Debug.LogError("❌ Cannot check node prefab - not assigned");
            return;
        }
        
        var nodePrefab = boardUI.NodePrefab;
        Debug.Log($"📋 Node prefab: {nodePrefab.name}");
        
        // Check components
        var nodeUI = nodePrefab.GetComponent<PassiveTreeNodeUI>();
        if (nodeUI == null)
        {
            Debug.LogError("❌ Node prefab missing PassiveTreeNodeUI component!");
        }
        else
        {
            Debug.Log("✅ PassiveTreeNodeUI component found");
        }
        
        var image = nodePrefab.GetComponent<UnityEngine.UI.Image>();
        if (image == null)
        {
            Debug.LogError("❌ Node prefab missing Image component!");
        }
        else
        {
            Debug.Log("✅ Image component found");
            Debug.Log($"   Source image: {(image.sprite != null ? image.sprite.name : "NULL")}");
            Debug.Log($"   Color: {image.color}");
        }
        
        var rectTransform = nodePrefab.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("❌ Node prefab missing RectTransform!");
        }
        else
        {
            Debug.Log("✅ RectTransform found");
            Debug.Log($"   Size: {rectTransform.sizeDelta}");
        }
    }
    
    private void ForceRefreshAll()
    {
        Debug.Log("\n🔄 6. FORCE REFRESH ALL");
        
        // Force initialize CoreBoard
        var manager = FindFirstObjectByType<PassiveTreeManager>();
        if (manager != null)
        {
            Debug.Log("🔄 Force initializing CoreBoard...");
            manager.ForceInitializeCoreBoard();
        }
        
        // Force refresh all BoardUI components
        var boardUIs = FindObjectsByType<PassiveTreeBoardUI>(FindObjectsSortMode.None);
        foreach (var boardUI in boardUIs)
        {
            Debug.Log($"🔄 Refreshing BoardUI: {boardUI.name}");
            boardUI.RefreshBoardVisual();
        }
        
        // Check results
        var finalNodeCount = FindObjectsByType<PassiveTreeNodeUI>(FindObjectsSortMode.None).Length;
        Debug.Log($"📊 Final node count: {finalNodeCount}");
        
        if (finalNodeCount > 0)
        {
            Debug.Log("✅ Nodes should now be visible!");
        }
        else
        {
            Debug.LogError("❌ Still no nodes visible after refresh!");
            Debug.LogError("   → Check console for specific error messages above");
        }
    }
    
    [ContextMenu("Check Single Node Creation")]
    public void CheckSingleNodeCreation()
    {
        Debug.Log("\n🧪 TESTING SINGLE NODE CREATION");
        
        var boardUI = FindFirstObjectByType<PassiveTreeBoardUI>();
        if (boardUI?.NodePrefab == null)
        {
            Debug.LogError("❌ Cannot test - no BoardUI or NodePrefab");
            return;
        }
        
        // Try to create a single test node
        var testNode = Instantiate(boardUI.NodePrefab, boardUI.transform);
        testNode.name = "TestNode";
        testNode.transform.localPosition = Vector3.zero;
        
        Debug.Log($"✅ Created test node: {testNode.name}");
        Debug.Log($"   Position: {testNode.transform.position}");
        Debug.Log($"   Active: {testNode.activeInHierarchy}");
        Debug.Log($"   Visible: {testNode.GetComponent<UnityEngine.UI.Image>()?.isActiveAndEnabled}");
        
        // Clean up after 3 seconds
        Destroy(testNode, 3f);
    }
}

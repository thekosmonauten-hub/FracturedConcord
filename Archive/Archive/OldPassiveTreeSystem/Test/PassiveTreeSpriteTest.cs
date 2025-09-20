using UnityEngine;
using PassiveTree;

/// <summary>
/// Test script to verify passive tree sprite assignments
/// </summary>
public class PassiveTreeSpriteTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private PassiveTreeSpriteManager spriteManager;
    [SerializeField] private PassiveBoard testBoard;
    
    [Header("Debug Options")]
    [SerializeField] private bool logOnStart = true;
    [SerializeField] private bool testCoreBoard = true;
    
    private void Start()
    {
        if (logOnStart)
        {
            TestSpriteAssignments();
        }
    }
    
    [ContextMenu("Test Sprite Assignments")]
    public void TestSpriteAssignments()
    {
        Debug.Log("=== Passive Tree Sprite Test ===");
        
        if (spriteManager != null)
        {
            spriteManager.LogSpriteConfiguration();
        }
        else
        {
            Debug.LogWarning("No sprite manager assigned for testing!");
        }
        
        if (testCoreBoard)
        {
            TestCoreBoardSprites();
        }
        
        if (testBoard != null)
        {
            TestBoardSprites(testBoard);
        }
        
        Debug.Log("=== End Sprite Test ===");
    }
    
    [ContextMenu("Test CoreBoard Sprites")]
    public void TestCoreBoardSprites()
    {
        Debug.Log("--- Testing CoreBoard Sprites ---");
        
        var passiveTreeManager = FindFirstObjectByType<PassiveTreeManager>();
        if (passiveTreeManager == null)
        {
            Debug.LogWarning("No PassiveTreeManager found in scene!");
            return;
        }
        
        var coreBoard = passiveTreeManager.GetCoreBoard();
        if (coreBoard == null)
        {
            Debug.LogWarning("CoreBoard is null!");
            return;
        }
        
        Debug.Log($"CoreBoard: {coreBoard.name}");
        Debug.Log($"Theme: {coreBoard.theme}");
        Debug.Log($"Use Custom Sprites: {coreBoard.GetUseCustomSprites()}");
        
        var boardSpriteManager = coreBoard.GetSpriteManager();
        if (boardSpriteManager != null)
        {
            Debug.Log($"Sprite Manager: {boardSpriteManager.name}");
            
            // Test board container sprite
            var boardContainerSprite = coreBoard.GetBoardContainerSprite();
            Debug.Log($"Board Container Sprite: {(boardContainerSprite != null ? boardContainerSprite.name : "NULL")}");
            
            // Test cell sprite
            var cellSprite = coreBoard.GetCellSprite();
            Debug.Log($"Cell Sprite: {(cellSprite != null ? cellSprite.name : "NULL")}");
            
            // Test node sprites for different types
            TestNodeSprites(coreBoard, NodeType.Main, "Main Node");
            TestNodeSprites(coreBoard, NodeType.Travel, "Travel Node");
            TestNodeSprites(coreBoard, NodeType.Keystone, "Keystone Node");
            TestNodeSprites(coreBoard, NodeType.Notable, "Notable Node");
            TestNodeSprites(coreBoard, NodeType.Small, "Small Node");
        }
        else
        {
            Debug.LogWarning("CoreBoard has no sprite manager assigned!");
        }
    }
    
    [ContextMenu("Test Board Sprites")]
    public void TestBoardSprites(PassiveBoard board)
    {
        if (board == null)
        {
            Debug.LogWarning("Board is null!");
            return;
        }
        
        Debug.Log($"--- Testing Board: {board.name} ---");
        Debug.Log($"Theme: {board.theme}");
        Debug.Log($"Use Custom Sprites: {board.GetUseCustomSprites()}");
        
        var boardSpriteManager = board.GetSpriteManager();
        if (boardSpriteManager != null)
        {
            Debug.Log($"Sprite Manager: {boardSpriteManager.name}");
            
            // Test board container sprite
            var boardContainerSprite = board.GetBoardContainerSprite();
            Debug.Log($"Board Container Sprite: {(boardContainerSprite != null ? boardContainerSprite.name : "NULL")}");
            
            // Test cell sprite
            var cellSprite = board.GetCellSprite();
            Debug.Log($"Cell Sprite: {(cellSprite != null ? cellSprite.name : "NULL")}");
        }
        else
        {
            Debug.LogWarning($"Board '{board.name}' has no sprite manager assigned!");
        }
    }
    
    private void TestNodeSprites(PassiveBoard board, NodeType nodeType, string nodeTypeName)
    {
        var nodeSprite = board.GetNodeSprite(nodeType);
        Debug.Log($"{nodeTypeName} Sprite: {(nodeSprite != null ? nodeSprite.name : "NULL")}");
    }
    
    [ContextMenu("Find All Sprite Managers")]
    public void FindAllSpriteManagers()
    {
        Debug.Log("--- Finding All Sprite Managers ---");
        
        var spriteManagers = FindObjectsByType<PassiveTreeSpriteManager>(FindObjectsSortMode.None);
        Debug.Log($"Found {spriteManagers.Length} sprite manager(s):");
        
        foreach (var manager in spriteManagers)
        {
            Debug.Log($"  - {manager.name} ({(manager != null ? "Valid" : "NULL")})");
        }
    }
    
    [ContextMenu("Find All Passive Boards")]
    public void FindAllPassiveBoards()
    {
        Debug.Log("--- Finding All Passive Boards ---");
        
        var passiveTreeManager = FindFirstObjectByType<PassiveTreeManager>();
        if (passiveTreeManager == null)
        {
            Debug.LogWarning("No PassiveTreeManager found!");
            return;
        }
        
        var coreBoard = passiveTreeManager.GetCoreBoard();
        if (coreBoard != null)
        {
            Debug.Log($"CoreBoard: {coreBoard.name} (Sprite Manager: {(coreBoard.GetSpriteManager() != null ? coreBoard.GetSpriteManager().name : "NULL")})");
        }
        
        var connectedBoards = passiveTreeManager.GetConnectedBoards();
        Debug.Log($"Connected Boards: {connectedBoards.Count}");
        
        foreach (var board in connectedBoards)
        {
            Debug.Log($"  - {board.name} (Sprite Manager: {(board.GetSpriteManager() != null ? board.GetSpriteManager().name : "NULL")})");
        }
    }
}

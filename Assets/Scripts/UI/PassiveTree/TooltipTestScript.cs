using UnityEngine;
using PassiveTree;

/// <summary>
/// Simple test script to verify tooltip system is working
/// </summary>
public class TooltipTestScript : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool testOnStart = false;
    [SerializeField] private float testDuration = 3f;
    
    [Header("References")]
    [SerializeField] private JsonPassiveTreeTooltip tooltipSystem;
    [SerializeField] private CellController testCell;
    
    void Start()
    {
        if (testOnStart)
        {
            Invoke(nameof(RunTooltipTest), 1f);
        }
    }
    
    /// <summary>
    /// Test the tooltip system
    /// </summary>
    [ContextMenu("Test Tooltip System")]
    public void RunTooltipTest()
    {
        Debug.Log("[TooltipTestScript] Starting tooltip test...");
        
        // Find tooltip system if not assigned
        if (tooltipSystem == null)
        {
            tooltipSystem = FindFirstObjectByType<JsonPassiveTreeTooltip>();
        }
        
        if (tooltipSystem == null)
        {
            Debug.LogError("[TooltipTestScript] No JsonPassiveTreeTooltip found! Please set up the tooltip system first.");
            return;
        }
        
        // Find a test cell if not assigned
        if (testCell == null)
        {
            testCell = FindFirstObjectByType<CellController>();
        }
        
        if (testCell == null)
        {
            Debug.LogError("[TooltipTestScript] No CellController found to test with!");
            return;
        }
        
        Debug.Log($"[TooltipTestScript] Testing tooltip with cell at {testCell.GetGridPosition()}");
        Debug.Log($"  - Node Name: {testCell.GetNodeName()}");
        Debug.Log($"  - Node Type: {testCell.GetNodeType()}");
        Debug.Log($"  - Node Description: {testCell.NodeDescription}");
        
        // Show tooltip
        tooltipSystem.ShowTooltip(testCell);
        
        // Hide after test duration
        Invoke(nameof(HideTestTooltip), testDuration);
    }
    
    private void HideTestTooltip()
    {
        if (tooltipSystem != null)
        {
            tooltipSystem.HideTooltip();
            Debug.Log("[TooltipTestScript] Test tooltip hidden");
        }
    }
    
    /// <summary>
    /// Test with a specific cell
    /// </summary>
    public void TestWithCell(CellController cell)
    {
        if (tooltipSystem == null)
        {
            tooltipSystem = FindFirstObjectByType<JsonPassiveTreeTooltip>();
        }
        
        if (tooltipSystem != null && cell != null)
        {
            Debug.Log($"[TooltipTestScript] Testing tooltip with specific cell at {cell.GetGridPosition()}");
            tooltipSystem.ShowTooltip(cell);
            
            Invoke(nameof(HideTestTooltip), testDuration);
        }
    }
}
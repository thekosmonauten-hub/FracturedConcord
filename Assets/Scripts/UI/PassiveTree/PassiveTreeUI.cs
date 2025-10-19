using UnityEngine;
using TMPro;
using PassiveTree;

/// <summary>
/// UI Manager for displaying passive tree cell information
/// Connects to TextMeshPro components to show selected cell data
/// </summary>
public class PassiveTreeUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI cellInfoText;
    [SerializeField] private TextMeshProUGUI cellNameText;
    [SerializeField] private TextMeshProUGUI cellDescriptionText;
    [SerializeField] private TextMeshProUGUI cellTypeText;
    [SerializeField] private TextMeshProUGUI cellCostText;
    [SerializeField] private TextMeshProUGUI cellStatusText;
    [SerializeField] private TextMeshProUGUI availablePointsText;
    [SerializeField] private TextMeshProUGUI pendingPointsText;
    [SerializeField] private UnityEngine.UI.Button confirmButton;
    [SerializeField] private UnityEngine.UI.Button cancelButton;

    [Header("UI Settings")]
    [SerializeField] private bool showDetailedInfo = true;
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private string defaultText = "Select a cell to view information";
    [SerializeField] private string noDataText = "No data available for this cell";

    [Header("Formatting")]
    [SerializeField] private string cellNameFormat = "<b>{0}</b>";
    [SerializeField] private string cellTypeFormat = "Type: <color=#FFD700>{0}</color>";
    [SerializeField] private string cellCostFormat = "Cost: <color=#00FF00>{0}</color> skill points";
    [SerializeField] private string cellStatusFormat = "Status: <color={1}>{0}</color>";

    // References
    private PassiveTreeManager treeManager;
    private BoardDataManager dataManager;
    private CellController currentSelectedCell;
    private PassiveTree.PlayerPassiveState playerPassiveState;

    void Start()
    {
        SetupUI();
        ConnectToPassiveTree();
    }

    /// <summary>
    /// Set up the UI components
    /// </summary>
    private void SetupUI()
    {
        // If no specific TextMeshPro components are assigned, try to find them
        if (cellInfoText == null)
        {
            cellInfoText = GetComponent<TextMeshProUGUI>();
        }

        // Set default text
        UpdateUIWithDefaultText();

        Debug.Log("[PassiveTreeUI] UI setup complete");
    }

    /// <summary>
    /// Connect to the passive tree system
    /// </summary>
    private void ConnectToPassiveTree()
    {
        // Find PassiveTreeManager
        treeManager = FindFirstObjectByType<PassiveTreeManager>();
        if (treeManager == null)
        {
            Debug.LogWarning("[PassiveTreeUI] No PassiveTreeManager found in scene!");
            return;
        }

        // Find BoardDataManager
        dataManager = FindFirstObjectByType<BoardDataManager>();
        if (dataManager == null)
        {
            Debug.LogWarning("[PassiveTreeUI] No BoardDataManager found in scene!");
        }

        // Subscribe to cell selection events
        // Note: You'll need to add this event to PassiveTreeManager
        // treeManager.OnCellSelected += OnCellSelected;

        // Hook pending allocation updates if available
        PassiveTree.PassiveTreeManager.OnPendingAllocationsChanged += RefreshPointsUI;
        
        // Try to locate PlayerPassiveState via CharacterManager (if integrated)
        // Optional: fallback null (UI will show 0)
        playerPassiveState = null;
        
        // Hook confirm/cancel buttons
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmAllocations);
        }
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelAllocations);
        }
        
        RefreshPointsUI();
        Debug.Log("[PassiveTreeUI] Connected to passive tree system");
    }

    private void OnDestroy()
    {
        PassiveTree.PassiveTreeManager.OnPendingAllocationsChanged -= RefreshPointsUI;
        if (confirmButton != null) confirmButton.onClick.RemoveListener(OnConfirmAllocations);
        if (cancelButton != null) cancelButton.onClick.RemoveListener(OnCancelAllocations);
    }

    private void RefreshPointsUI()
    {
        // Available Points
        int available = 0;
        if (playerPassiveState != null)
        {
            available = playerPassiveState.GetAvailablePoints();
        }
        if (availablePointsText != null)
        {
            availablePointsText.text = $"Available Points: {available}";
        }
        
        // Pending allocations (count)
        int pending = treeManager != null ? treeManager.GetPendingAllocationsCount() : 0;
        if (pendingPointsText != null)
        {
            pendingPointsText.text = pending > 0 ? $"Pending: {pending}" : "Pending: 0";
        }
        
        // Enable/disable confirm/cancel
        if (confirmButton != null) confirmButton.interactable = pending > 0 && available >= pending;
        if (cancelButton != null) cancelButton.interactable = pending > 0;
    }

    private void OnConfirmAllocations()
    {
        if (treeManager == null) return;
        // Spend points and finalize purchases
        int pendingCount = treeManager.GetPendingAllocationsCount();
        if (pendingCount == 0) return;
        if (playerPassiveState != null && playerPassiveState.GetAvailablePoints() < pendingCount)
        {
            Debug.LogWarning("[PassiveTreeUI] Not enough points to confirm pending selections");
            return;
        }
        
        int allocated = treeManager.ConfirmPendingAllocations();
        
        // Spend points
        if (playerPassiveState != null)
        {
            playerPassiveState.AddSkillPoints(-allocated);
        }
        RefreshPointsUI();
    }

    private void OnCancelAllocations()
    {
        if (treeManager == null) return;
        treeManager.CancelPendingAllocations();
        RefreshPointsUI();
    }

    private PassiveTree.CellController FindCell(Vector2Int gridPos)
    {
        var cells = FindObjectsByType<PassiveTree.CellController>(FindObjectsSortMode.None);
        foreach (var c in cells)
        {
            if (c.GridPosition == gridPos) return c;
        }
        return null;
    }

    /// <summary>
    /// Handle cell selection events
    /// </summary>
    public void OnCellSelected(Vector2Int cellPosition)
    {
        if (showDebugInfo)
        {
            Debug.Log($"[PassiveTreeUI] Cell selected: {cellPosition}");
        }

        // Find the cell controller
        CellController[] allCells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        CellController selectedCell = null;

        foreach (CellController cell in allCells)
        {
            if (cell.GridPosition == cellPosition)
            {
                selectedCell = cell;
                break;
            }
        }

        if (selectedCell != null)
        {
            UpdateUIWithCellData(selectedCell);
        }
        else
        {
            UpdateUIWithError($"Cell at {cellPosition} not found");
        }
    }

    /// <summary>
    /// Update UI with cell data
    /// </summary>
    public void UpdateUIWithCellData(CellController cell)
    {
        if (cell == null)
        {
            UpdateUIWithError("No cell selected");
            return;
        }

        currentSelectedCell = cell;

        // Get node data if available
        PassiveNodeData nodeData = null;
        if (dataManager != null)
        {
            nodeData = dataManager.GetNodeData(cell.GridPosition);
        }

        // Update individual text components if available
        if (cellNameText != null)
        {
            string cellName = nodeData != null ? nodeData.NodeName : $"Cell ({cell.GridPosition.x},{cell.GridPosition.y})";
            cellNameText.text = string.Format(cellNameFormat, cellName);
        }

        if (cellDescriptionText != null)
        {
            string description = nodeData != null ? nodeData.Description : cell.NodeDescription;
            cellDescriptionText.text = description;
        }

        if (cellTypeText != null)
        {
            cellTypeText.text = string.Format(cellTypeFormat, cell.NodeType.ToString());
        }

        if (cellCostText != null)
        {
            int cost = nodeData != null ? nodeData.SkillPointsCost : GetDefaultCost(cell.NodeType);
            cellCostText.text = string.Format(cellCostFormat, cost);
        }

        if (cellStatusText != null)
        {
            string status = GetCellStatus(cell);
            string statusColor = GetStatusColor(cell);
            cellStatusText.text = string.Format(cellStatusFormat, status, statusColor);
        }

        // Update main info text
        if (cellInfoText != null)
        {
            if (showDetailedInfo)
            {
                cellInfoText.text = FormatDetailedCellInfo(cell, nodeData);
            }
            else
            {
                cellInfoText.text = FormatSimpleCellInfo(cell, nodeData);
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"[PassiveTreeUI] Updated UI for cell {cell.GridPosition}: {cell.NodeType}");
        }
    }

    /// <summary>
    /// Format detailed cell information
    /// </summary>
    private string FormatDetailedCellInfo(CellController cell, PassiveNodeData nodeData)
    {
        string cellName = nodeData != null ? nodeData.NodeName : $"Cell ({cell.GridPosition.x},{cell.GridPosition.y})";
        string description = nodeData != null ? nodeData.Description : cell.NodeDescription;
        int cost = nodeData != null ? nodeData.SkillPointsCost : GetDefaultCost(cell.NodeType);
        string status = GetCellStatus(cell);
        string statusColor = GetStatusColor(cell);

        return $"<b>{cellName}</b>\n" +
               $"<size=80%>{description}</size>\n\n" +
               $"<color=#FFD700>Type:</color> {cell.NodeType}\n" +
               $"<color=#00FF00>Cost:</color> {cost} skill points\n" +
               $"<color={statusColor}>Status:</color> {status}\n" +
               $"<color=#87CEEB>Position:</color> ({cell.GridPosition.x}, {cell.GridPosition.y})";
    }

    /// <summary>
    /// Format simple cell information
    /// </summary>
    private string FormatSimpleCellInfo(CellController cell, PassiveNodeData nodeData)
    {
        string cellName = nodeData != null ? nodeData.NodeName : $"Cell ({cell.GridPosition.x},{cell.GridPosition.y})";
        string status = GetCellStatus(cell);

        return $"{cellName}\n{status}";
    }

    /// <summary>
    /// Get cell status string
    /// </summary>
    private string GetCellStatus(CellController cell)
    {
        if (cell.IsPurchased)
            return "Purchased";
        else if (cell.IsUnlocked)
            return "Available";
        else if (cell.IsAvailable)
            return "Locked";
        else
            return "Unavailable";
    }

    /// <summary>
    /// Get status color for formatting
    /// </summary>
    private string GetStatusColor(CellController cell)
    {
        if (cell.IsPurchased)
            return "#00FF00"; // Green
        else if (cell.IsUnlocked)
            return "#FFD700"; // Gold
        else if (cell.IsAvailable)
            return "#FF6B6B"; // Red
        else
            return "#808080"; // Gray
    }

    /// <summary>
    /// Get default cost for node type
    /// </summary>
    private int GetDefaultCost(NodeType nodeType)
    {
        switch (nodeType)
        {
            case NodeType.Start:
                return 0;
            case NodeType.Travel:
                return 1;
            case NodeType.Extension:
                return 0;
            case NodeType.Notable:
                return 2;
            case NodeType.Small:
                return 1;
            case NodeType.Keystone:
                return 1;
            default:
                return 1;
        }
    }

    /// <summary>
    /// Update UI with default text
    /// </summary>
    private void UpdateUIWithDefaultText()
    {
        if (cellInfoText != null)
        {
            cellInfoText.text = defaultText;
        }

        if (cellNameText != null)
        {
            cellNameText.text = "";
        }

        if (cellDescriptionText != null)
        {
            cellDescriptionText.text = "";
        }

        if (cellTypeText != null)
        {
            cellTypeText.text = "";
        }

        if (cellCostText != null)
        {
            cellCostText.text = "";
        }

        if (cellStatusText != null)
        {
            cellStatusText.text = "";
        }
    }

    /// <summary>
    /// Update UI with error message
    /// </summary>
    private void UpdateUIWithError(string errorMessage)
    {
        if (cellInfoText != null)
        {
            cellInfoText.text = $"<color=#FF6B6B>Error: {errorMessage}</color>";
        }
    }

    /// <summary>
    /// Manually update UI with a specific cell
    /// </summary>
    [ContextMenu("Update UI with Current Cell")]
    public void UpdateUIWithCurrentCell()
    {
        if (currentSelectedCell != null)
        {
            UpdateUIWithCellData(currentSelectedCell);
        }
        else
        {
            UpdateUIWithDefaultText();
        }
    }

    /// <summary>
    /// Clear the UI display
    /// </summary>
    [ContextMenu("Clear UI")]
    public void ClearUI()
    {
        UpdateUIWithDefaultText();
        currentSelectedCell = null;
    }

    /// <summary>
    /// Get the currently selected cell
    /// </summary>
    public CellController GetCurrentSelectedCell()
    {
        return currentSelectedCell;
    }

    /// <summary>
    /// Set the main info text component
    /// </summary>
    public void SetCellInfoText(TextMeshProUGUI textComponent)
    {
        cellInfoText = textComponent;
    }

    /// <summary>
    /// Set individual text components
    /// </summary>
    public void SetTextComponents(TextMeshProUGUI nameText, TextMeshProUGUI descText, TextMeshProUGUI typeText, TextMeshProUGUI costText, TextMeshProUGUI statusText)
    {
        cellNameText = nameText;
        cellDescriptionText = descText;
        cellTypeText = typeText;
        cellCostText = costText;
        cellStatusText = statusText;
    }
}

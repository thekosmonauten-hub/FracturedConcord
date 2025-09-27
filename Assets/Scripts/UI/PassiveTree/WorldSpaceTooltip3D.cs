using UnityEngine;
using TMPro;
using PassiveTree;

/// <summary>
/// World space tooltip system that doesn't require Canvas - uses 3D Text and UI elements
/// </summary>
public class WorldSpaceTooltip3D : MonoBehaviour
{
    [Header("Tooltip Prefab")]
    [SerializeField] private GameObject tooltipPrefab;
    
    [Header("World Space Settings")]
    [SerializeField] private float tooltipDistance = 2f;
    [SerializeField] private Vector3 tooltipOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private bool followMouse = true;
    [SerializeField] private bool faceCamera = true;
    
    [Header("References")]
    [SerializeField] private JsonBoardDataManager dataManager;
    [SerializeField] private Camera mainCamera;
    
    // Runtime
    private GameObject currentTooltip;
    private TextMeshPro nameText;
    private TextMeshPro descriptionText;
    private CellController currentHoveredCell;
    
    void Start()
    {
        // Find required components
        if (dataManager == null)
        {
            dataManager = FindFirstObjectByType<JsonBoardDataManager>();
        }
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }
        }
    }
    
    void Update()
    {
        if (currentTooltip != null && currentHoveredCell != null)
        {
            UpdateTooltipPosition();
        }
    }
    
    /// <summary>
    /// Show tooltip for a cell
    /// </summary>
    public void ShowTooltip(CellController cell)
    {
        PassiveTreeLogger.LogCategory($"ShowTooltip called for cell at {cell?.GridPosition}", "tooltip");
        
        if (cell == null) 
        {
            PassiveTreeLogger.LogWarning("Cell is null!");
            return;
        }

        currentHoveredCell = cell;
        
        // Get node data from CellJsonData component
        var cellJsonData = cell.GetComponent<CellJsonData>();
        
        if (cellJsonData != null && cellJsonData.HasJsonData())
        {
            PassiveTreeLogger.LogCategory($"Found node data: {cellJsonData.NodeName} ({cellJsonData.NodeType})", "tooltip");
            CreateTooltipWithCellData(cellJsonData);
        }
        else
        {
            CreateTooltipWithCellBasicData(cell);
        }
    }
    
    /// <summary>
    /// Hide tooltip
    /// </summary>
    public void HideTooltip()
    {
        if (currentTooltip != null)
        {
            Destroy(currentTooltip);
            currentTooltip = null;
        }
        
        currentHoveredCell = null;
    }
    
    /// <summary>
    /// Create tooltip using CellJsonData component
    /// </summary>
    private void CreateTooltipWithCellData(CellJsonData cellJsonData)
    {
        // Create or update tooltip
        if (currentTooltip == null)
        {
            CreateTooltip();
        }
        
        if (currentTooltip != null)
        {
            UpdateTooltipContentWithCellData(cellJsonData);
            PositionTooltip(currentHoveredCell);
            currentTooltip.SetActive(true);
            
            PassiveTreeLogger.LogCategory($"Showing tooltip for {cellJsonData.NodeName} at {currentHoveredCell.GridPosition}", "tooltip");
        }
    }
    
    /// <summary>
    /// Create tooltip using cell's basic data (fallback)
    /// </summary>
    private void CreateTooltipWithCellBasicData(CellController cell)
    {
        // Create or update tooltip
        if (currentTooltip == null)
        {
            CreateTooltip();
        }
        
        if (currentTooltip != null)
        {
            UpdateTooltipContentWithCellBasicData(cell);
            PositionTooltip(currentHoveredCell);
            currentTooltip.SetActive(true);
            
            PassiveTreeLogger.LogCategory($"Showing tooltip for {cell.GetNodeName()} at {currentHoveredCell.GridPosition}", "tooltip");
        }
    }
    
    /// <summary>
    /// Create tooltip GameObject
    /// </summary>
    private void CreateTooltip()
    {
        if (tooltipPrefab == null)
        {
            PassiveTreeLogger.LogWarning("No tooltip prefab assigned, creating dynamic 3D tooltip...");
            CreateDynamicTooltip();
            return;
        }

        // Instantiate tooltip in world space
        currentTooltip = Instantiate(tooltipPrefab);
        currentTooltip.name = "WorldSpaceTooltip";
        
        // Find text components
        TextMeshPro[] texts = currentTooltip.GetComponentsInChildren<TextMeshPro>();
        foreach (var text in texts)
        {
            string textName = text.name.ToLower();
            if (textName.Contains("passive name") || textName.Contains("name") || textName.Contains("title"))
            {
                nameText = text;
                PassiveTreeLogger.LogCategory($"Found name text: {text.name}", "tooltip");
            }
            else if (textName.Contains("description") || textName.Contains("desc"))
            {
                descriptionText = text;
                PassiveTreeLogger.LogCategory($"Found description text: {text.name}", "tooltip");
            }
        }

        if (nameText == null || descriptionText == null)
        {
            PassiveTreeLogger.LogWarning($"Could not find text components in tooltip prefab. Found {texts.Length} TextMeshPro components:");
            foreach (var text in texts)
            {
                PassiveTreeLogger.LogWarning($"  - {text.name}");
            }
        }
        else
        {
            PassiveTreeLogger.LogCategory("3D Tooltip prefab loaded successfully", "tooltip");
        }
    }
    
    /// <summary>
    /// Create a dynamic 3D tooltip when no prefab is available
    /// </summary>
    private void CreateDynamicTooltip()
    {
        // Create tooltip GameObject
        currentTooltip = new GameObject("DynamicWorldSpaceTooltip");
        
        // Create background (optional)
        GameObject background = GameObject.CreatePrimitive(PrimitiveType.Quad);
        background.transform.SetParent(currentTooltip.transform);
        background.transform.localPosition = Vector3.zero;
        background.transform.localScale = new Vector3(2f, 1f, 1f);
        
        // Remove collider from background
        Collider bgCollider = background.GetComponent<Collider>();
        if (bgCollider != null)
        {
            Destroy(bgCollider);
        }
        
        // Set background material
        Renderer bgRenderer = background.GetComponent<Renderer>();
        if (bgRenderer != null)
        {
            Material bgMaterial = new Material(Shader.Find("Unlit/Color"));
            bgMaterial.color = new Color(0, 0, 0, 0.8f);
            bgRenderer.material = bgMaterial;
        }
        
        // Create name text
        GameObject nameObject = new GameObject("Name");
        nameObject.transform.SetParent(currentTooltip.transform);
        nameObject.transform.localPosition = new Vector3(0, 0.2f, -0.01f);
        nameText = nameObject.AddComponent<TextMeshPro>();
        nameText.text = "Node Name";
        nameText.fontSize = 0.3f;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.Center;
        
        // Create description text
        GameObject descObject = new GameObject("Description");
        descObject.transform.SetParent(currentTooltip.transform);
        descObject.transform.localPosition = new Vector3(0, -0.2f, -0.01f);
        descriptionText = descObject.AddComponent<TextMeshPro>();
        descriptionText.text = "Node Description";
        descriptionText.fontSize = 0.2f;
        descriptionText.color = Color.white;
        descriptionText.alignment = TextAlignmentOptions.Center;
        
        PassiveTreeLogger.LogCategory("Dynamic 3D tooltip created", "tooltip");
    }
    
    /// <summary>
    /// Update tooltip content using CellJsonData
    /// </summary>
    private void UpdateTooltipContentWithCellData(CellJsonData cellJsonData)
    {
        if (nameText != null)
        {
            nameText.text = cellJsonData.NodeName;
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = cellJsonData.GetFormattedDescription();
        }
    }
    
    /// <summary>
    /// Update tooltip content using cell's basic data
    /// </summary>
    private void UpdateTooltipContentWithCellBasicData(CellController cell)
    {
        if (nameText != null)
        {
            nameText.text = cell.GetNodeName();
        }
        
        if (descriptionText != null)
        {
            string description = cell.NodeDescription;
            
            // Add basic info if no description
            if (string.IsNullOrEmpty(description))
            {
                description = $"Node Type: {cell.GetNodeType()}\nPosition: {cell.GetGridPosition()}";
            }
            
            descriptionText.text = description;
        }
    }
    
    /// <summary>
    /// Position tooltip in world space
    /// </summary>
    private void PositionTooltip(CellController cell)
    {
        if (currentTooltip == null || cell == null) return;
        
        // Position tooltip above the cell
        Vector3 cellPosition = cell.transform.position;
        Vector3 tooltipPosition = cellPosition + tooltipOffset;
        
        // If following mouse, position near mouse cursor
        if (followMouse && mainCamera != null)
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, tooltipDistance));
            tooltipPosition = mouseWorldPos + tooltipOffset;
        }
        
        currentTooltip.transform.position = tooltipPosition;
        
        // Face camera if enabled
        if (faceCamera && mainCamera != null)
        {
            currentTooltip.transform.LookAt(mainCamera.transform);
            currentTooltip.transform.Rotate(0, 180, 0); // Flip to face camera
        }
    }
    
    /// <summary>
    /// Update tooltip position during runtime
    /// </summary>
    private void UpdateTooltipPosition()
    {
        if (currentTooltip != null && currentHoveredCell != null)
        {
            PositionTooltip(currentHoveredCell);
        }
    }
}













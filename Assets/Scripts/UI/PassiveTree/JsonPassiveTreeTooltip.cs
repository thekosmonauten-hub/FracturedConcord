using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace PassiveTree
{
    /// <summary>
    /// JSON-based tooltip system for passive tree
    /// Much simpler and more efficient than ScriptableObject approach
    /// </summary>
    public class JsonPassiveTreeTooltip : MonoBehaviour
    {
        [Header("Tooltip Prefab")]
        [SerializeField] private GameObject tooltipPrefab;
        
        [Header("Settings")]
        [SerializeField] private bool followMouse = true;
        [SerializeField] private Vector2 mouseOffset = new Vector2(0, 50);
        [SerializeField] private bool keepOnScreen = true;
        [SerializeField] private Vector2 screenMargin = new Vector2(10, 10);
        
        [Header("References")]
        [SerializeField] private JsonBoardDataManager dataManager;
        [SerializeField] private Camera mainCamera;

        // Runtime
        private GameObject currentTooltip;
        private RectTransform tooltipRectTransform;
        private TextMeshProUGUI nameText;
        private TextMeshProUGUI descriptionText;
        private CellController currentHoveredCell;

        // Public properties for debugging
        public GameObject TooltipPrefab => tooltipPrefab;
        public JsonBoardDataManager DataManager => dataManager;

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
            }

            Debug.Log("[JsonPassiveTreeTooltip] JSON-based tooltip system initialized");
        }

        void Update()
        {
            if (currentTooltip != null)
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
                
                // Create tooltip with cell JSON data
                CreateTooltipWithCellData(cellJsonData);
                return;
            }
            
            // Fallback: Use cell's basic data if no JSON data
            CreateTooltipWithCellBasicData(cell);
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
                
                // Debug.Log($"[JsonPassiveTreeTooltip] Showing tooltip for {cellJsonData.NodeName} at {currentHoveredCell.GridPosition}");
            }
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
        /// Position tooltip for a specific cell
        /// </summary>
        private void PositionTooltip(CellController cell)
        {
            if (cell != null)
            {
                UpdateTooltipPosition();
            }
        }

        /// <summary>
        /// Hide tooltip
        /// </summary>
        public void HideTooltip()
        {
            if (currentTooltip != null)
            {
                currentTooltip.SetActive(false);
            }
            
            currentHoveredCell = null;
        }

        /// <summary>
        /// Create tooltip GameObject
        /// </summary>
        private void CreateTooltip()
        {
            if (tooltipPrefab == null)
            {
                PassiveTreeLogger.LogWarning("No tooltip prefab assigned, creating dynamic tooltip...");
                CreateDynamicTooltip();
                return;
            }

            // Find the best Canvas for tooltips
            Canvas canvas = FindBestTooltipCanvas();
            if (canvas == null)
            {
                PassiveTreeLogger.LogError("No suitable Canvas found! Tooltip needs a Canvas to display properly.");
                return;
            }

            // Instantiate tooltip as child of Canvas
            currentTooltip = Instantiate(tooltipPrefab, canvas.transform);
            tooltipRectTransform = currentTooltip.GetComponent<RectTransform>();
            
            // Find text components by name (matching your TooltipPanel.prefab structure)
            TextMeshProUGUI[] texts = currentTooltip.GetComponentsInChildren<TextMeshProUGUI>();
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
                PassiveTreeLogger.LogCategory("Tooltip prefab loaded successfully", "tooltip");
            }
        }

        /// <summary>
        /// Find the best Canvas for tooltips (prioritizes custom Canvas, then tooltip-specific Canvas)
        /// </summary>
        private Canvas FindBestTooltipCanvas()
        {
            // First, look for a custom Canvas assigned in TooltipSetupHelper
            TooltipSetupHelper setupHelper = FindFirstObjectByType<TooltipSetupHelper>();
            if (setupHelper != null)
            {
                // Use reflection to get the customCanvas field
                var customCanvasField = typeof(TooltipSetupHelper).GetField("customCanvas", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (customCanvasField != null)
                {
                    Canvas customCanvas = customCanvasField.GetValue(setupHelper) as Canvas;
                    if (customCanvas != null)
                    {
                        PassiveTreeLogger.LogCategory($"Using custom Canvas: {customCanvas.name}", "tooltip");
                        return customCanvas;
                    }
                }
            }
            
            // Second, look for a dedicated tooltip Canvas
            WorldSpaceTooltipCanvas tooltipCanvasManager = FindFirstObjectByType<WorldSpaceTooltipCanvas>();
            if (tooltipCanvasManager != null)
            {
                Canvas tooltipCanvas = tooltipCanvasManager.GetTooltipCanvas();
                if (tooltipCanvas != null)
                {
                    PassiveTreeLogger.LogCategory("Using dedicated tooltip Canvas", "tooltip");
                    return tooltipCanvas;
                }
            }
            
            // Fallback to any Canvas
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in allCanvases)
            {
                if (canvas.name.ToLower().Contains("tooltip"))
                {
                    PassiveTreeLogger.LogCategory($"Using tooltip Canvas: {canvas.name}", "tooltip");
                    return canvas;
                }
            }
            
            // Use the first available Canvas
            if (allCanvases.Length > 0)
            {
                PassiveTreeLogger.LogCategory($"Using fallback Canvas: {allCanvases[0].name}", "tooltip");
                return allCanvases[0];
            }
            
            return null;
        }

        /// <summary>
        /// Create a dynamic tooltip when no prefab is available
        /// </summary>
        private void CreateDynamicTooltip()
        {
            // Create tooltip GameObject
            currentTooltip = new GameObject("DynamicTooltip");
            currentTooltip.transform.SetParent(transform);
            
            // Add RectTransform
            tooltipRectTransform = currentTooltip.AddComponent<RectTransform>();
            tooltipRectTransform.sizeDelta = new Vector2(300, 200);
            
            // Add Image component for background
            Image backgroundImage = currentTooltip.AddComponent<Image>();
            backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            
            // Add ContentSizeFitter
            ContentSizeFitter sizeFitter = currentTooltip.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Add VerticalLayoutGroup
            VerticalLayoutGroup layoutGroup = currentTooltip.AddComponent<VerticalLayoutGroup>();
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.spacing = 5;
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;

            // Create title text
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(currentTooltip.transform);
            
            nameText = titleObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Node Name";
            nameText.fontSize = 16;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;
            
            // Add LayoutElement to title
            LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 26;
            
            // Create description text
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(currentTooltip.transform);
            
            descriptionText = descObj.AddComponent<TextMeshProUGUI>();
            descriptionText.text = "Node description and stats will appear here";
            descriptionText.fontSize = 14;
            descriptionText.color = Color.white;
            descriptionText.alignment = TextAlignmentOptions.TopLeft;
            descriptionText.textWrappingMode = TextWrappingModes.Normal;
            
            // Add LayoutElement to description
            LayoutElement descLayout = descObj.AddComponent<LayoutElement>();
            descLayout.flexibleHeight = 1;
            descLayout.minHeight = 50;

            Debug.Log("[JsonPassiveTreeTooltip] Created dynamic tooltip");
        }

        /// <summary>
        /// Populate tooltip with JSON data
        /// </summary>
        private void PopulateTooltip(JsonNodeData nodeData, CellController cell)
        {
            if (nameText != null)
            {
                nameText.text = nodeData.name;
            }

            if (descriptionText != null)
            {
                // Build rich description with stats
                string description = nodeData.description;
                
                // Add stats information
                if (nodeData.stats != null)
                {
                    description += "\n\n<color=#00FF00>Stats:</color>";
                    
                    var statsDict = ConvertStatsToDictionary(nodeData.stats);
                    foreach (var kvp in statsDict)
                    {
                        if (kvp.Value != 0)
                        {
                            description += $"\n• {kvp.Key}: +{kvp.Value}";
                        }
                    }
                }
                
                // Add node type and cost
                description += $"\n\n<color=#FFFF00>Type:</color> {nodeData.type}";
                description += $"\n<color=#FFFF00>Cost:</color> {nodeData.cost} skill points";
                
                // Add status
                string status = cell.IsPurchased ? "<color=#00FF00>Purchased</color>" : 
                               cell.IsUnlocked ? "<color=#FFFF00>Available</color>" : 
                               "<color=#FF0000>Locked</color>";
                description += $"\n<color=#FFFF00>Status:</color> {status}";
                
                descriptionText.text = description;
            }
        }

        /// <summary>
        /// Convert JSON stats to Dictionary
        /// </summary>
        private Dictionary<string, float> ConvertStatsToDictionary(JsonStats jsonStats)
        {
            Dictionary<string, float> statsDict = new Dictionary<string, float>();
            
            // Use reflection to get all properties from JsonStats
            var properties = typeof(JsonStats).GetProperties();
            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(float))
                {
                    object value = prop.GetValue(jsonStats);
                    if (value != null)
                    {
                        float floatValue = System.Convert.ToSingle(value);
                        if (floatValue != 0) // Only add non-zero values
                        {
                            // Map TypeScript stat names to Unity stat names
                            string unityStatName = PassiveTreeStatMapper.MapStatName(prop.Name);
                            statsDict[unityStatName] = floatValue;
                        }
                    }
                }
            }
            
            return statsDict;
        }

        /// <summary>
        /// Update tooltip position
        /// </summary>
        private void UpdateTooltipPosition()
        {
            if (currentTooltip == null || tooltipRectTransform == null) return;

            Vector2 position;
            Vector2 tooltipSize = tooltipRectTransform.sizeDelta;
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            if (followMouse)
            {
                // Follow mouse position using new Input System
                if (Mouse.current != null)
                {
                    Vector2 mousePos = Mouse.current.position.ReadValue();

                    // Smart positioning: try to place tooltip above mouse first
                    Vector2 preferredOffset = new Vector2(0, tooltipSize.y + 20); // Above mouse with gap
                    position = mousePos + preferredOffset;

                    // If tooltip would go off screen at top, place it below mouse
                    if (position.y + tooltipSize.y > screenSize.y - screenMargin.y)
                    {
                        position = mousePos + new Vector2(0, -tooltipSize.y - 20); // Below mouse with gap
                    }

                    // If still off screen at bottom, place it to the right of mouse
                    if (position.y < screenMargin.y)
                    {
                        position = mousePos + new Vector2(tooltipSize.x + 20, 0); // Right of mouse with gap
                    }

                    // If still off screen at right, place it to the left of mouse
                    if (position.x + tooltipSize.x > screenSize.x - screenMargin.x)
                    {
                        position = mousePos + new Vector2(-tooltipSize.x - 20, 0); // Left of mouse with gap
                    }
                }
                else
                {
                    // Fallback to screen center if mouse is not available
                    position = new Vector2(Screen.width / 2f, Screen.height / 2f) + mouseOffset;
                    Debug.LogWarning("[JsonPassiveTreeTooltip] Mouse.current is null, using screen center as fallback");
                }
            }
            else if (currentHoveredCell != null)
            {
                // Position near the cell
                Vector3 cellWorldPos = currentHoveredCell.transform.position;
                Vector3 cellScreenPos = mainCamera.WorldToScreenPoint(cellWorldPos);
                position = new Vector2(cellScreenPos.x, cellScreenPos.y) + mouseOffset;
            }
            else
            {
                return;
            }

            // Final screen boundary check
            if (keepOnScreen)
            {
                // Adjust X position
                if (position.x + tooltipSize.x > screenSize.x - screenMargin.x)
                {
                    position.x = screenSize.x - tooltipSize.x - screenMargin.x;
                }
                if (position.x < screenMargin.x)
                {
                    position.x = screenMargin.x;
                }

                // Adjust Y position
                if (position.y + tooltipSize.y > screenSize.y - screenMargin.y)
                {
                    position.y = screenSize.y - tooltipSize.y - screenMargin.y;
                }
                if (position.y < screenMargin.y)
                {
                    position.y = screenMargin.y;
                }
            }

            tooltipRectTransform.position = position;
        }

        /// <summary>
        /// Set the data manager reference
        /// </summary>
        public void SetDataManager(JsonBoardDataManager manager)
        {
            dataManager = manager;
        }
    }
}

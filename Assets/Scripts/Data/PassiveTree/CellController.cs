using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace PassiveTree
{
    /// <summary>
    /// Simple cell controller for individual passive tree cells
    /// Handles mouse interaction and visual state
    /// </summary>
    public class CellController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Cell Settings")]
        [SerializeField] protected Vector2Int gridPosition;
        [SerializeField] protected NodeType nodeType = NodeType.Travel;
        [SerializeField] protected string nodeName = "Basic Node";
        [SerializeField] protected string nodeDescription = "Basic node";
        
        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] protected Button button;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = Color.yellow;
        [SerializeField] private Color selectedColor = Color.green;
        [SerializeField] private Color unavailableColor = Color.gray;
        [SerializeField] private Color lockedColor = Color.red;
        [SerializeField] private Color purchasedColor = Color.blue;
        
        [Header("Auto Sprite Assignment")]
        [SerializeField] protected bool autoAssignSprite = true;
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Sprite startNodeSprite;
        [SerializeField] private Sprite travelNodeSprite;
        [SerializeField] private Sprite extensionPointSprite;
        [SerializeField] private Sprite notableNodeSprite;
        [SerializeField] private Sprite smallNodeSprite;
        [SerializeField] private Sprite keystoneNodeSprite;
        
        [Header("State")]
        [SerializeField] private bool isSelected = false;
        [SerializeField] private bool isHovered = false;
        [SerializeField] protected bool isAvailable = true;
        [SerializeField] protected bool isUnlocked = false;
        [SerializeField] protected bool isPurchased = false;
        [SerializeField] protected bool isExtensionPoint = false;
        
        [Header("Debug")]
        [SerializeField] protected bool showDebugInfo = false;
        
        [Header("Attribute Overlay Sprites")]
        [SerializeField] private bool enableAttributeOverlays = false;
        [SerializeField] private SpriteRenderer overlaySpriteRenderer;
        [SerializeField] private Sprite strengthOverlay;
        [SerializeField] private Sprite dexterityOverlay;
        [SerializeField] private Sprite intelligenceOverlay;
        [SerializeField] private Sprite strengthDexterityOverlay;
        [SerializeField] private Sprite strengthIntelligenceOverlay;
        [SerializeField] private Sprite dexterityIntelligenceOverlay;
        [SerializeField] private Sprite allAttributesOverlay;
        [SerializeField] private Vector2 overlaySize = new Vector2(1.5f, 1.5f);
        [SerializeField] private Vector2 overlayOffset = new Vector2(0f, 0f);
        [SerializeField] private Color overlayTint = Color.white;
        
        // References
        private PassiveTreeManager treeManager;
        private PassiveTreeTooltip tooltipManager;
        private JsonPassiveTreeTooltip jsonTooltipManager;
        private PassiveTreeStaticTooltip staticTooltipManager;
        private ExtensionPoint extensionPoint;
        
        // Properties
        public Vector2Int GridPosition => gridPosition;
        public NodeType NodeType => nodeType;
        public string NodeName => nodeName;
        public string NodeDescription => nodeDescription;
        public bool IsSelected => isSelected;
        public bool IsAvailable => isAvailable;
        public bool IsUnlocked => isUnlocked;
        public bool IsPurchased => isPurchased;
        public bool IsExtensionPoint => isExtensionPoint;
        
        protected virtual void Awake()
        {
            // Get sprite renderer if not assigned
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            
            // Get button component if not assigned
            if (button == null)
                button = GetComponent<Button>();
            
            // Set up overlay sprite renderer if not assigned
            if (enableAttributeOverlays && overlaySpriteRenderer == null)
            {
                // Always create a separate overlay object to avoid interfering with the main cell sprite
                GameObject overlayObject = new GameObject("AttributeOverlay");
                overlayObject.transform.SetParent(transform);
                overlayObject.transform.localPosition = Vector3.zero;
                overlaySpriteRenderer = overlayObject.AddComponent<SpriteRenderer>();
                SetupOverlayRenderer();
            }
            
            // Set up button click listener
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClick);
            }
            
            // Auto-assign sprite based on node type if enabled
            if (autoAssignSprite)
            {
                AssignSpriteBasedOnNodeType();
            }
            
            // Set initial visual state
            UpdateVisualState();
            
            // Debug component setup
            DebugComponentSetup();
        }
        
        void Update()
        {
            // Debug mouse position relative to this cell (only for unlocked cells to reduce spam)
            if (isUnlocked && showDebugInfo)
            {
                DebugMousePositionRelativeToCell();
            }
        }
        
        /// <summary>
        /// Debug method to show mouse position relative to this cell
        /// </summary>
        private void DebugMousePositionRelativeToCell()
        {
            var camera = Camera.main;
            if (camera == null) return;
            
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPos = camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0f));
            Vector3 cellPos = transform.position;
            
            float distance = Vector2.Distance(worldPos, cellPos);
            
            // Only log when mouse is very close to this cell
            if (distance < 1f)
            {
                PassiveTreeLogger.LogCategory($"Mouse near {gridPosition}: distance={distance:F2}", "input");
                
                // Check if mouse is actually over the collider
                var collider = GetComponent<Collider2D>();
                if (collider != null)
                {
                    bool isOverCollider = collider.OverlapPoint(worldPos);
                    Debug.Log($"  - Over collider: {isOverCollider}");
                }
            }
        }
        
        /// <summary>
        /// Debug the component setup for click detection
        /// </summary>
        private void DebugComponentSetup()
        {
            Debug.Log($"[CellController] Component setup for {gridPosition}:");
            
            // Check for required components
            var collider2D = GetComponent<Collider2D>();
            if (collider2D != null)
            {
                PassiveTreeLogger.LogCategory($"Collider2D: {collider2D.GetType().Name} (enabled: {collider2D.enabled})", "input");
            }
            else
            {
                Debug.LogError($"  ❌ Missing Collider2D on {gridPosition}");
            }
            
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                PassiveTreeLogger.LogCategory($"SpriteRenderer: present (enabled: {spriteRenderer.enabled})", "sprite");
            }
            else
            {
                Debug.LogError($"  ❌ Missing SpriteRenderer on {gridPosition}");
            }
            
            // Check for Button component
            var buttonComponent = GetComponent<Button>();
            if (buttonComponent != null)
            {
                PassiveTreeLogger.LogCategory($"Button: present (enabled: {buttonComponent.enabled}, interactable: {buttonComponent.interactable})", "input");
            }
            else
            {
                Debug.LogError($"  ❌ Missing Button component on {gridPosition}");
            }
        }
        
        /// <summary>
        /// Initialize the cell with the tree manager
        /// </summary>
        public void Initialize(PassiveTreeManager manager)
        {
            treeManager = manager;
            
            // Set initial state based on node type
            switch (nodeType)
            {
                case NodeType.Start:
                    isAvailable = true;
                    break;
                case NodeType.Travel:
                case NodeType.Notable:
                case NodeType.Keystone:
                    isAvailable = true;
                    break;
                default:
                    isAvailable = false;
                    break;
            }
            
            UpdateVisualState();
            UpdateButtonInteractable();
        }
        
        /// <summary>
        /// Handle button click
        /// </summary>
        public virtual void OnButtonClick()
        {
            Debug.Log($"🖱️ [CellController] BUTTON CLICK DETECTED on {gridPosition} - Available: {isAvailable}, Unlocked: {isUnlocked}, Purchased: {isPurchased}, Type: {nodeType}");
            
            // Always log the interaction validation steps
            Debug.Log($"[CellController] Interaction validation for {gridPosition}:");
            Debug.Log($"  - isAvailable: {isAvailable}");
            Debug.Log($"  - isUnlocked: {isUnlocked}");
            Debug.Log($"  - isPurchased: {isPurchased}");
            Debug.Log($"  - nodeType: {nodeType}");
            
            if (!CanBeInteractedWith()) 
            {
                Debug.LogWarning($"❌ [CellController] Cell {gridPosition} cannot be interacted with - validation failed");
                return;
            }
            
            Debug.Log($"✅ [CellController] Cell {gridPosition} passed interaction validation");
            
            // Check if this cell is on an extension board
            ExtensionBoardController extensionBoard = GetComponentInParent<ExtensionBoardController>();
            if (extensionBoard != null)
            {
                Debug.Log($"🔄 [CellController] Cell {gridPosition} is on extension board, handling purchase directly");
                HandleExtensionBoardCellPurchase();
            }
            else if (treeManager != null)
            {
                Debug.Log($"🔄 [CellController] Calling treeManager.OnCellClicked({gridPosition})");
                treeManager.OnCellClicked(gridPosition);
            }
            else
            {
                Debug.LogError($"❌ [CellController] No tree manager reference for cell {gridPosition}");
            }
            
            Debug.Log($"✅ [CellController] Cell {gridPosition} button click processing complete - {nodeDescription}");
        }
        
        /// <summary>
        /// Handle purchase for extension board cells
        /// </summary>
        private void HandleExtensionBoardCellPurchase()
        {
            Debug.Log($"[CellController] Handling extension board cell purchase for {gridPosition}");
            
            // Check if we can purchase this cell
            if (!CanBeInteractedWith())
            {
                Debug.Log($"[CellController] ❌ Cannot purchase extension board cell {gridPosition} - failed interaction check");
                return;
            }
            
            // Purchase the cell
            SetPurchased(true);
            Debug.Log($"[CellController] ✅ Successfully purchased extension board cell {gridPosition}");
            
            // Notify the extension board controller
            ExtensionBoardController extensionBoard = GetComponentInParent<ExtensionBoardController>();
            if (extensionBoard != null)
            {
                extensionBoard.OnCellPurchased(gridPosition);
            }
        }
        
        /// <summary>
        /// Check if this cell can be interacted with
        /// </summary>
        protected virtual bool CanBeInteractedWith()
        {
            Debug.Log($"[CellController] CanBeInteractedWith check for {gridPosition}:");
            
            // Can't interact if not available
            if (!isAvailable) 
            {
                Debug.Log($"  ❌ Failed: Not available");
                return false;
            }
            Debug.Log($"  ✅ Available: {isAvailable}");
            
            // Can't interact if locked
            if (!isUnlocked) 
            {
                Debug.Log($"  ❌ Failed: Not unlocked");
                return false;
            }
            Debug.Log($"  ✅ Unlocked: {isUnlocked}");
            
            // Can't interact if already purchased (unless it's a toggleable node)
            if (isPurchased && nodeType != NodeType.Start) 
            {
                Debug.Log($"  ❌ Failed: Already purchased and not start node");
                return false;
            }
            Debug.Log($"  ✅ Purchase check passed: purchased={isPurchased}, type={nodeType}");
            
            Debug.Log($"  ✅ All checks passed - can interact");
            return true;
        }
        
        /// <summary>
        /// Handle mouse enter
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            UpdateVisualState();
            
            // Show tooltip - try both tooltip systems
            ShowTooltip();
            
            PassiveTreeLogger.LogCategory($"HOVER ENTER on {gridPosition} - {nodeName} ({nodeType})", "cell");
        }
        
        /// <summary>
        /// Handle mouse exit
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            UpdateVisualState();
            
            // Hide tooltip - try both tooltip systems
            HideTooltip();
            
            PassiveTreeLogger.LogCategory($"HOVER EXIT on {gridPosition}", "cell");
        }
        
        /// <summary>
        /// Set the selected state
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateVisualState();
        }
        
        /// <summary>
        /// Set the available state
        /// </summary>
        public void SetAvailable(bool available)
        {
            isAvailable = available;
            UpdateVisualState();
        }
        
        /// <summary>
        /// Set the unlocked state
        /// </summary>
        public void SetUnlocked(bool unlocked)
        {
            bool wasUnlocked = isUnlocked;
            isUnlocked = unlocked;
            UpdateVisualState();
            UpdateButtonInteractable();
            
            Debug.Log($"[CellController] SetUnlocked({unlocked}) for {gridPosition} - was: {wasUnlocked}, now: {isUnlocked}, Type: {nodeType}");
        }
        
        /// <summary>
        /// Set the purchased state
        /// </summary>
        public void SetPurchased(bool purchased)
        {
            bool wasPurchased = isPurchased;
            isPurchased = purchased;
            UpdateVisualState();
            UpdateButtonInteractable();
            
            Debug.Log($"[CellController] SetPurchased({purchased}) for {gridPosition} - was: {wasPurchased}, now: {isPurchased}");
        }
        
        /// <summary>
        /// Update the button's interactable state
        /// </summary>
        protected virtual void UpdateButtonInteractable()
        {
            if (button != null)
            {
                button.interactable = CanBeInteractedWith();
            }
        }
        
        /// <summary>
        /// Update the visual state of the cell
        /// </summary>
        public void UpdateVisualState()
        {
            if (spriteRenderer == null) return;
            
            Color targetColor = normalColor;
            
            // Priority order: purchased > locked > unavailable > selected > hovered > normal
            if (isPurchased)
            {
                targetColor = purchasedColor;
            }
            else if (!isUnlocked)
            {
                targetColor = lockedColor;
            }
            else if (!isAvailable)
            {
                targetColor = unavailableColor;
            }
            else if (isSelected)
            {
                targetColor = selectedColor;
            }
            else if (isHovered)
            {
                targetColor = hoverColor;
            }
            
            spriteRenderer.color = targetColor;
        }
        
        /// <summary>
        /// Set the grid position (for editor setup)
        /// </summary>
        public void SetGridPosition(Vector2Int position)
        {
            gridPosition = position;
        }
        
        /// <summary>
        /// Set the node type
        /// </summary>
        public void SetNodeType(NodeType type)
        {
            nodeType = type;
            
            // Update sprite if auto-assignment is enabled
            if (autoAssignSprite)
            {
                AssignSpriteBasedOnNodeType();
            }
        }
        
        /// <summary>
        /// Set the node description
        /// </summary>
        public void SetNodeDescription(string description)
        {
            nodeDescription = description;
        }

        /// <summary>
        /// Set the node name
        /// </summary>
        public void SetNodeName(string name)
        {
            nodeName = name;
        }

        /// <summary>
        /// Set the skill points cost
        /// </summary>
        public void SetSkillPointsCost(int cost)
        {
            // Store cost in a field or use existing logic
            // For now, we'll add this to the description
            if (!string.IsNullOrEmpty(nodeDescription))
            {
                nodeDescription = $"{nodeDescription} (Cost: {cost})";
            }
        }

        /// <summary>
        /// Set the node stats
        /// </summary>
        public void SetNodeStats(Dictionary<string, float> stats)
        {
            // Store stats in description for now, or add a separate field
            if (stats != null && stats.Count > 0)
            {
                string statsText = "";
                foreach (var kvp in stats)
                {
                    if (kvp.Value != 0)
                    {
                        statsText += $"{kvp.Key}: +{kvp.Value} ";
                    }
                }
                
                if (!string.IsNullOrEmpty(statsText))
                {
                    nodeDescription = $"{nodeDescription}\n{statsText.Trim()}";
                }
            }
        }

        /// <summary>
        /// Set node data from PassiveNodeData ScriptableObject
        /// </summary>
        public void SetNodeData(PassiveNodeData nodeData)
        {
            if (nodeData == null)
            {
                Debug.LogWarning($"[CellController] Cannot set null node data for cell at {gridPosition}");
                return;
            }

            Debug.Log($"🔧 [CellController] Setting node data for cell at {gridPosition}:");
            Debug.Log($"  - Node Name: '{nodeData.NodeName}'");
            Debug.Log($"  - Description: '{nodeData.Description}'");
            Debug.Log($"  - Node Type: {nodeData.NodeType}");
            Debug.Log($"  - Is Unlocked: {nodeData.IsUnlocked}");

            // Update cell properties from node data
            nodeType = nodeData.NodeType;
            nodeDescription = nodeData.Description;
            
            // Update visual properties
            normalColor = nodeData.NodeColor;
            hoverColor = nodeData.HighlightColor;
            selectedColor = nodeData.SelectedColor;
            
            // Update state
            isUnlocked = nodeData.IsUnlocked;
            
            // Update sprite if available
            if (nodeData.NodeSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = nodeData.NodeSprite;
            }
            else
            {
                // Fall back to auto-assignment
                AssignSpriteBasedOnNodeType();
            }

            // Update visual state
            UpdateVisualState();

            // Update attribute overlay if enabled
            if (enableAttributeOverlays)
            {
                UpdateAttributeOverlay();
            }

            // Check if this is an extension point based on JSON data
            CheckExtensionPointStatus();

            Debug.Log($"[CellController] Set node data for {gridPosition}: {nodeData.NodeName} ({nodeData.NodeType})");
        }

        /// <summary>
        /// Get the current node data (creates a runtime instance)
        /// </summary>
        public PassiveNodeData GetNodeData()
        {
            // Create a runtime instance with current cell data
            PassiveNodeData nodeData = ScriptableObject.CreateInstance<PassiveNodeData>();
            
            // Use reflection to set private fields
            SetNodeDataField(nodeData, "_nodeName", $"Node ({gridPosition.x},{gridPosition.y})");
            SetNodeDataField(nodeData, "_description", nodeDescription);
            SetNodeDataField(nodeData, "_nodeType", nodeType);
            SetNodeDataField(nodeData, "_nodeColor", normalColor);
            SetNodeDataField(nodeData, "_highlightColor", hoverColor);
            SetNodeDataField(nodeData, "_selectedColor", selectedColor);
            SetNodeDataField(nodeData, "_isUnlocked", isUnlocked);
            SetNodeDataField(nodeData, "_isStartNode", nodeType == NodeType.Start);
            SetNodeDataField(nodeData, "_skillPointsCost", GetDefaultCost(nodeType));
            
            return nodeData;
        }

        /// <summary>
        /// Use reflection to set private fields on PassiveNodeData
        /// </summary>
        private void SetNodeDataField(PassiveNodeData nodeData, string fieldName, object value)
        {
            var field = typeof(PassiveNodeData).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(nodeData, value);
            }
        }

        /// <summary>
        /// Get default cost for node type
        /// </summary>
        private int GetDefaultCost(NodeType type)
        {
            switch (type)
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
        /// Assign sprite based on node type (attribute icons are handled separately)
        /// </summary>
        protected virtual void AssignSpriteBasedOnNodeType()
        {
            if (spriteRenderer == null) return;
            
            Sprite targetSprite = GetSpriteForNodeType(nodeType);
            
            if (targetSprite != null)
            {
                spriteRenderer.sprite = targetSprite;
            }
            else if (defaultSprite != null)
            {
                spriteRenderer.sprite = defaultSprite;
            }
        }
        
        /// <summary>
        /// Check if this cell should be marked as an extension point based on JSON data
        /// </summary>
        private void CheckExtensionPointStatus()
        {
            var cellJsonData = GetComponent<CellJsonData>();
            if (cellJsonData != null && cellJsonData.HasJsonData())
            {
                // Check if the node type indicates this is an extension point
                string jsonNodeType = cellJsonData.NodeType?.ToLower();
                if (jsonNodeType == "extension" || jsonNodeType == "extensionpoint")
                {
                    isExtensionPoint = true;
                    // Update the nodeType to Extension to match the extension point status
                    nodeType = NodeType.Extension;
                    // Force assign extension point sprite even if autoAssignSprite is disabled
                    AssignExtensionPointSprite();
                    if (showDebugInfo)
                    {
                        Debug.Log($"[CellController] Marked {gridPosition} as extension point based on JSON data - Updated nodeType to Extension");
                    }
                }
                else
                {
                    isExtensionPoint = false;
                }
            }
        }

        /// <summary>
        /// Set up the overlay sprite renderer
        /// </summary>
        private void SetupOverlayRenderer()
        {
            if (overlaySpriteRenderer == null) return;

            // Configure the overlay sprite renderer
            overlaySpriteRenderer.sortingOrder = 1; // Ensure it's on top of the cell sprite
            overlaySpriteRenderer.sortingLayerName = "Default";
            overlaySpriteRenderer.gameObject.SetActive(false);
            overlaySpriteRenderer.sprite = null;

            // Apply size and offset
            overlaySpriteRenderer.transform.localScale = new Vector3(overlaySize.x, overlaySize.y, 1f);
            overlaySpriteRenderer.transform.localPosition = new Vector3(overlayOffset.x, overlayOffset.y, -0.1f);
            overlaySpriteRenderer.color = overlayTint;
        }

        /// <summary>
        /// Update the attribute overlay based on JSON data
        /// </summary>
        private void UpdateAttributeOverlay()
        {
            if (overlaySpriteRenderer == null) return;

            // Get JSON data from CellJsonData component
            var cellJsonData = GetComponent<CellJsonData>();
            if (cellJsonData == null || !cellJsonData.HasJsonData())
            {
                overlaySpriteRenderer.gameObject.SetActive(false);
                return;
            }

            var stats = cellJsonData.NodeStats;
            Sprite overlaySprite = DetermineOverlaySprite(stats);

            if (overlaySprite != null)
            {
                overlaySpriteRenderer.sprite = overlaySprite;
                overlaySpriteRenderer.gameObject.SetActive(true);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[CellController] Showing overlay for {gridPosition}: {overlaySprite.name}");
                }
            }
            else
            {
                overlaySpriteRenderer.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Determine which overlay sprite to show based on stats
        /// </summary>
        private Sprite DetermineOverlaySprite(JsonStats stats)
        {
            if (stats == null) return null;

            // Count how many attributes have values > 0
            int strengthCount = stats.strength > 0 ? 1 : 0;
            int dexterityCount = stats.dexterity > 0 ? 1 : 0;
            int intelligenceCount = stats.intelligence > 0 ? 1 : 0;
            int totalAttributes = strengthCount + dexterityCount + intelligenceCount;

            // Return appropriate overlay sprite based on attribute combination
            if (totalAttributes == 3)
            {
                return allAttributesOverlay;
            }
            else if (strengthCount == 1 && dexterityCount == 1 && intelligenceCount == 0)
            {
                return strengthDexterityOverlay;
            }
            else if (strengthCount == 1 && intelligenceCount == 1 && dexterityCount == 0)
            {
                return strengthIntelligenceOverlay;
            }
            else if (dexterityCount == 1 && intelligenceCount == 1 && strengthCount == 0)
            {
                return dexterityIntelligenceOverlay;
            }
            else if (strengthCount == 1 && dexterityCount == 0 && intelligenceCount == 0)
            {
                return strengthOverlay;
            }
            else if (dexterityCount == 1 && strengthCount == 0 && intelligenceCount == 0)
            {
                return dexterityOverlay;
            }
            else if (intelligenceCount == 1 && strengthCount == 0 && dexterityCount == 0)
            {
                return intelligenceOverlay;
            }

            return null; // No overlay if no attributes or multiple single attributes
        }

        /// <summary>
        /// Get the appropriate sprite for a node type
        /// </summary>
        private Sprite GetSpriteForNodeType(NodeType type)
        {
            switch (type)
            {
                case NodeType.Start:
                    return startNodeSprite;
                case NodeType.Travel:
                    return travelNodeSprite;
                case NodeType.Extension:
                    return extensionPointSprite;
                case NodeType.Notable:
                    return notableNodeSprite;
                case NodeType.Small:
                    return smallNodeSprite;
                case NodeType.Keystone:
                    return keystoneNodeSprite;
                default:
                    return defaultSprite;
            }
        }
        
        /// <summary>
        /// Force assign extension point sprite (bypasses autoAssignSprite setting)
        /// </summary>
        private void AssignExtensionPointSprite()
        {
            if (spriteRenderer == null) return;
            
            if (extensionPointSprite != null)
            {
                spriteRenderer.sprite = extensionPointSprite;
                if (showDebugInfo)
                {
                    Debug.Log($"[CellController] Assigned extension point sprite to {gridPosition}");
                }
            }
            else
            {
                Debug.LogWarning($"[CellController] Extension point sprite is null for {gridPosition} - please assign EXT_Cell sprite in inspector");
            }
        }
        
        /// <summary>
        /// Manually assign a sprite (for runtime changes)
        /// </summary>
        public void AssignSprite(Sprite sprite)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
            }
        }
        
        /// <summary>
        /// Update sprite when node type changes
        /// </summary>
        public void UpdateSpriteForNodeType()
        {
            if (autoAssignSprite)
            {
                AssignSpriteBasedOnNodeType();
            }
        }
        
        /// <summary>
        /// Update sprite when JSON data is loaded or changed
        /// </summary>
        public void UpdateSpriteForJsonData()
        {
            if (autoAssignSprite)
            {
                AssignSpriteBasedOnNodeType();
            }
            
            // Update attribute overlay if enabled
            if (enableAttributeOverlays)
            {
                UpdateAttributeOverlay();
            }
        }

        /// <summary>
        /// Show tooltip using available tooltip system
        /// </summary>
        private void ShowTooltip()
        {
            // PassiveTreeLogger.LogCategory($"ShowTooltip called for cell {gridPosition}", "tooltip");
            
            // Try to get static tooltip from PassiveTreeManager first (centralized reference)
            if (staticTooltipManager == null)
            {
                var passiveTreeManager = FindFirstObjectByType<PassiveTreeManager>();
                if (passiveTreeManager != null)
                {
                    staticTooltipManager = passiveTreeManager.GetStaticTooltip();
                }
                
                // Fallback to direct search if manager doesn't have reference
                if (staticTooltipManager == null)
                {
                    staticTooltipManager = FindFirstObjectByType<PassiveTreeStaticTooltip>();
                }
            }
            
            if (staticTooltipManager != null)
            {
                // PassiveTreeLogger.LogCategory($"Using PassiveTreeStaticTooltip for cell {gridPosition}", "tooltip");
                staticTooltipManager.UpdateTooltipContent(this);
                return;
            }
            
            // Fallback to JSON tooltip system
            if (jsonTooltipManager == null)
            {
                jsonTooltipManager = FindFirstObjectByType<JsonPassiveTreeTooltip>();
            }
            
            if (jsonTooltipManager != null)
            {
                // PassiveTreeLogger.LogCategory($"Using JsonPassiveTreeTooltip for cell {gridPosition}", "tooltip");
                jsonTooltipManager.ShowTooltip(this);
                return;
            }
            
            // Fallback to ScriptableObject tooltip system
            if (tooltipManager == null)
            {
                tooltipManager = FindFirstObjectByType<PassiveTreeTooltip>();
            }
            
            if (tooltipManager != null)
            {
                // PassiveTreeLogger.LogCategory($"Using PassiveTreeTooltip for cell {gridPosition}", "tooltip");
                tooltipManager.ShowTooltip(this);
            }
            else
            {
                // PassiveTreeLogger.LogWarning($"No tooltip manager found for cell {gridPosition}");
            }
        }

        /// <summary>
        /// Hide tooltip using available tooltip system
        /// </summary>
        private void HideTooltip()
        {
            // Try static tooltip system first
            if (staticTooltipManager != null)
            {
                staticTooltipManager.HideTooltip();
            }
            else
            {
                // Try to get static tooltip from PassiveTreeManager if not cached
                var passiveTreeManager = FindFirstObjectByType<PassiveTreeManager>();
                if (passiveTreeManager != null)
                {
                    var managerTooltip = passiveTreeManager.GetStaticTooltip();
                    if (managerTooltip != null)
                    {
                        managerTooltip.HideTooltip();
                    }
                }
            }
            
            // Try JSON tooltip system
            if (jsonTooltipManager != null)
            {
                jsonTooltipManager.HideTooltip();
            }
            
            // Also try ScriptableObject tooltip system
            if (tooltipManager != null)
            {
                tooltipManager.HideTooltip();
            }
        }

        #region Attribute Overlay Methods

        /// <summary>
        /// Enable or disable attribute overlays for this cell
        /// </summary>
        public void SetAttributeOverlaysEnabled(bool enabled)
        {
            enableAttributeOverlays = enabled;
            
            if (overlaySpriteRenderer != null)
            {
                overlaySpriteRenderer.gameObject.SetActive(enabled && overlaySpriteRenderer.sprite != null);
            }
        }

        /// <summary>
        /// Manually refresh the attribute overlay
        /// </summary>
        public void RefreshAttributeOverlay()
        {
            if (enableAttributeOverlays)
            {
                UpdateAttributeOverlay();
            }
        }

        /// <summary>
        /// Get information about the current overlay
        /// </summary>
        public string GetOverlayInfo()
        {
            if (!enableAttributeOverlays || overlaySpriteRenderer == null || !overlaySpriteRenderer.gameObject.activeInHierarchy)
            {
                return "No overlay";
            }

            var cellJsonData = GetComponent<CellJsonData>();
            if (cellJsonData == null || !cellJsonData.HasJsonData())
            {
                return "No JSON data";
            }

            var stats = cellJsonData.NodeStats;
            if (stats == null) return "No stats";

            // Count attributes
            int strengthCount = stats.strength > 0 ? 1 : 0;
            int dexterityCount = stats.dexterity > 0 ? 1 : 0;
            int intelligenceCount = stats.intelligence > 0 ? 1 : 0;
            int totalAttributes = strengthCount + dexterityCount + intelligenceCount;

            if (totalAttributes == 3) return "All Attributes";
            if (strengthCount == 1 && dexterityCount == 1 && intelligenceCount == 0) return "Strength + Dexterity";
            if (strengthCount == 1 && intelligenceCount == 1 && dexterityCount == 0) return "Strength + Intelligence";
            if (dexterityCount == 1 && intelligenceCount == 1 && strengthCount == 0) return "Dexterity + Intelligence";
            if (strengthCount == 1 && dexterityCount == 0 && intelligenceCount == 0) return "Strength";
            if (dexterityCount == 1 && strengthCount == 0 && intelligenceCount == 0) return "Dexterity";
            if (intelligenceCount == 1 && strengthCount == 0 && dexterityCount == 0) return "Intelligence";

            return "Unknown";
        }

        /// <summary>
        /// Check if this cell is currently showing an attribute overlay
        /// </summary>
        public bool IsShowingAttributeOverlay()
        {
            return enableAttributeOverlays && overlaySpriteRenderer != null && 
                   overlaySpriteRenderer.gameObject.activeInHierarchy && overlaySpriteRenderer.sprite != null;
        }

        /// <summary>
        /// Manually set this cell as an extension point
        /// </summary>
        public void SetAsExtensionPoint(bool isExtension)
        {
            isExtensionPoint = isExtension;
            
            if (isExtension)
            {
                // Update the nodeType to Extension to match the extension point status
                nodeType = NodeType.Extension;
                // Force assign extension point sprite when manually setting as extension point
                AssignExtensionPointSprite();
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[CellController] Set {gridPosition} extension point status to: {isExtension} - Updated nodeType to Extension");
            }
        }

        /// <summary>
        /// Get information about this cell's extension point status
        /// </summary>
        public string GetExtensionPointInfo()
        {
            if (isExtensionPoint)
            {
                return "Extension Point";
            }
            else
            {
                return "Regular Node";
            }
        }

        #endregion
        
        #region Context Menu Methods
        
        /// <summary>
        /// Manually assign extension point sprite (for debugging)
        /// </summary>
        [ContextMenu("Assign Extension Point Sprite")]
        public void ManualAssignExtensionPointSprite()
        {
            AssignExtensionPointSprite();
        }
        
        /// <summary>
        /// Force update sprite based on current node type
        /// </summary>
        [ContextMenu("Force Update Sprite")]
        public void ForceUpdateSprite()
        {
            if (autoAssignSprite)
            {
                AssignSpriteBasedOnNodeType();
            }
            else if (isExtensionPoint)
            {
                AssignExtensionPointSprite();
            }
        }
        
        #endregion
        
        // Debug helper methods
        public NodeType GetNodeType() => nodeType;
        public string GetNodeName() => nodeName;
        public string GetNodeDescription() => nodeDescription;
        public Vector2Int GetGridPosition() => gridPosition;
        public bool GetAutoAssignSprite() => autoAssignSprite;
        public Sprite GetExtensionPointSprite() => extensionPointSprite;
        
        /// <summary>
        /// Set the extension point data for this cell
        /// </summary>
        public void SetExtensionPoint(ExtensionPoint point)
        {
            extensionPoint = point;
            if (point != null)
            {
                isExtensionPoint = true;
                AssignExtensionPointSprite();
            }
        }
        
        /// <summary>
        /// Get the extension point data for this cell
        /// </summary>
        public ExtensionPoint GetExtensionPoint()
        {
            return extensionPoint;
        }

    }
}

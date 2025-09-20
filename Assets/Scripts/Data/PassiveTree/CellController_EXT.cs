using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace PassiveTree
{
    /// <summary>
    /// Extension board specific cell controller
    /// Handles mouse interaction and visual state for extension board cells only
    /// Optimized for extension board behavior without core board dependencies
    /// Inherits from CellController for tooltip system compatibility
    /// </summary>
    public class CellController_EXT : CellController
    {
        // Extension board specific references
        public ExtensionBoardController extensionBoardController;
        
        void Awake()
        {
            // Call base class Awake first
            base.Awake();
            
            // Get extension board controller reference
            extensionBoardController = GetComponentInParent<ExtensionBoardController>();
        }
        
        void Start()
        {
            // Base class doesn't have Start method, so we just do our own initialization
            // Set initial visual state
            UpdateVisualState();
            
            // Assign sprite based on node type
            if (autoAssignSprite)
            {
                AssignSpriteBasedOnNodeType();
            }
        }
        
        /// <summary>
        /// Initialize the extension board cell
        /// </summary>
        public void Initialize(ExtensionBoardController controller)
        {
            extensionBoardController = controller;
            
            // Set initial state based on node type
            SetInitialState();
            
            if (showDebugInfo)
            {
                Debug.Log($"[CellController_EXT] Initialized extension board cell {gridPosition} with controller {controller?.name}");
            }
        }
        
        /// <summary>
        /// Set initial state based on node type for extension boards
        /// Extension boards never have Start nodes - they connect to existing boards
        /// </summary>
        public void SetInitialState()
        {
            switch (nodeType)
            {
                case NodeType.Extension:
                    // Extension points start locked - only the connecting extension point will be made available by BoardPositioningManager
                    isUnlocked = false;
                    isAvailable = false;
                    isPurchased = false;
                    isExtensionPoint = true;
                    break;
                default:
                    // All other nodes start locked and unavailable
                    isUnlocked = false;
                    isAvailable = false;
                    isPurchased = false;
                    break;
            }
        }
        
        /// <summary>
        /// Override button click for extension board specific behavior
        /// </summary>
        public override void OnButtonClick()
        {
            if (showDebugInfo)
            {
                Debug.Log($"[CellController_EXT] Button clicked on extension board cell {gridPosition}");
                Debug.Log($"  - isAvailable: {isAvailable}");
                Debug.Log($"  - isUnlocked: {isUnlocked}");
                Debug.Log($"  - isPurchased: {isPurchased}");
                Debug.Log($"  - nodeType: {nodeType}");
            }
            
            if (!CanBeInteractedWith()) 
            {
                Debug.LogWarning($"❌ [CellController_EXT] Extension board cell {gridPosition} cannot be interacted with - validation failed");
                return;
            }
            
            Debug.Log($"✅ [CellController_EXT] Extension board cell {gridPosition} passed interaction validation");
            
            // Route to the extension board controller for proper node type handling
            if (extensionBoardController != null)
            {
                extensionBoardController.OnCellClicked(gridPosition);
            }
            else
            {
                Debug.LogError($"[CellController_EXT] No extension board controller found for cell {gridPosition}");
            }
            
            Debug.Log($"✅ [CellController_EXT] Extension board cell {gridPosition} button click processing complete - {nodeDescription}");
        }
        
        /// <summary>
        /// Handle purchase for extension board cells
        /// </summary>
        public void HandleExtensionBoardCellPurchase()
        {
            Debug.Log($"[CellController_EXT] Handling extension board cell purchase for {gridPosition}");
            
            // Check if we can purchase this cell
            if (!CanBeInteractedWith())
            {
                Debug.Log($"[CellController_EXT] ❌ Cannot purchase extension board cell {gridPosition} - failed interaction check");
                return;
            }
            
            // Purchase the cell
            SetPurchased(true);
            Debug.Log($"[CellController_EXT] ✅ Successfully purchased extension board cell {gridPosition}");
            
            // Notify the extension board controller
            if (extensionBoardController != null)
            {
                extensionBoardController.OnCellPurchased(gridPosition);
            }
            else
            {
                Debug.LogWarning($"[CellController_EXT] No extension board controller found for cell {gridPosition}");
            }
        }
        
        /// <summary>
        /// Handle initial allocation for extension board cells
        /// Called when extension board is first created and connected
        /// </summary>
        public void HandleInitialAllocation()
        {
            if (nodeType == NodeType.Extension)
            {
                // Extension points are automatically allocated when board is created
                SetPurchased(true);
                isUnlocked = true;
                isAvailable = true;
                isExtensionPoint = true;
                
                Debug.Log($"[CellController_EXT] ✅ Extension point {gridPosition} automatically allocated");
            }
        }
        
        /// <summary>
        /// Unlock this cell for purchasing (called when adjacent cell is purchased)
        /// </summary>
        public void UnlockForPurchasing()
        {
            if (!isPurchased && !isUnlocked)
            {
                isUnlocked = true;
                isAvailable = true;
                UpdateVisualState();
                
                Debug.Log($"[CellController_EXT] 🔓 Cell {gridPosition} unlocked for purchasing");
            }
        }
        
        /// <summary>
        /// Load JSON data for this extension board cell
        /// Called when the extension board JSON data is loaded
        /// </summary>
        public void LoadJsonDataForExtensionBoard()
        {
            // Get the CellJsonData component
            var cellJsonData = GetComponent<CellJsonData>();
            if (cellJsonData != null && cellJsonData.HasJsonData())
            {
                // Update node data from JSON
                nodeName = cellJsonData.NodeName ?? "Extension Node";
                nodeDescription = cellJsonData.NodeDescription ?? "Extension board node";
                
                // Check if this should be an extension point based on JSON
                string jsonNodeType = cellJsonData.NodeType?.ToLower();
                if (jsonNodeType == "extension" || jsonNodeType == "extensionpoint")
                {
                    nodeType = NodeType.Extension;
                    isExtensionPoint = true;
                }
                
                // Update visual state
                UpdateVisualState();
                
                if (autoAssignSprite)
                {
                    AssignSpriteBasedOnNodeType();
                }
                
                Debug.Log($"[CellController_EXT] ✅ Loaded JSON data for extension board cell {gridPosition}: {nodeName} ({nodeType})");
            }
            else
            {
                Debug.LogWarning($"[CellController_EXT] No JSON data found for extension board cell {gridPosition}");
            }
        }
        
        /// <summary>
        /// Override button interactable to include adjacency validation for extension points
        /// </summary>
        protected override void UpdateButtonInteractable()
        {
            if (button != null)
            {
                // For extension points, we need to check both basic interaction and adjacency validation
                if (nodeType == NodeType.Extension)
                {
                    // Check basic interaction first
                    bool canInteract = CanBeInteractedWith();
                    
                    if (canInteract)
                    {
                        // For extension points, also check if they have adjacent purchased nodes
                        // This requires access to the ExtensionBoardController
                        if (extensionBoardController != null)
                        {
                            // Use the extension board controller's adjacency validation
                            canInteract = extensionBoardController.HasAdjacentPurchasedNode(gridPosition);
                        }
                    }
                    
                    button.interactable = canInteract;
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"[CellController_EXT] Extension point at {gridPosition}: button interactable = {canInteract}");
                    }
                }
                else
                {
                    // For non-extension nodes, use the base class logic
                    button.interactable = CanBeInteractedWith();
                }
            }
        }
        
        /// <summary>
        /// Debug method to show extension board cell state
        /// </summary>
        [ContextMenu("Debug Extension Cell State")]
        public void DebugExtensionCellState()
        {
            Debug.Log($"[CellController_EXT] Extension Board Cell {gridPosition} State:");
            Debug.Log($"  - NodeType: {nodeType}");
            Debug.Log($"  - IsAvailable: {isAvailable}");
            Debug.Log($"  - IsUnlocked: {isUnlocked}");
            Debug.Log($"  - IsPurchased: {isPurchased}");
            Debug.Log($"  - IsExtensionPoint: {isExtensionPoint}");
            Debug.Log($"  - ExtensionBoardController: {(extensionBoardController != null ? extensionBoardController.name : "null")}");
        }
    }
}

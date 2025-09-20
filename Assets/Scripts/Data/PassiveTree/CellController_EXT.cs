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
        /// Set initial state based on node type
        /// </summary>
        public void SetInitialState()
        {
            switch (nodeType)
            {
                case NodeType.Start:
                    isUnlocked = true;
                    isAvailable = true;
                    isPurchased = true;
                    break;
                case NodeType.Extension:
                    isUnlocked = true;
                    isAvailable = true;
                    isPurchased = false;
                    isExtensionPoint = true;
                    break;
                default:
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
            
            // Handle extension board cell purchase directly
            HandleExtensionBoardCellPurchase();
            
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

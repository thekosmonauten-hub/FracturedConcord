using UnityEngine;
using UnityEngine.InputSystem;

namespace PassiveTree
{
    /// <summary>
    /// Direct world space input handler for passive tree cells
    /// Eliminates Canvas coordinate system issues by using pure Physics2D raycasting
    /// </summary>
    public class WorldSpaceInputHandler : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private LayerMask cellLayerMask = -1;
        [SerializeField] private float maxRaycastDistance = 100f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private bool showRaycastGizmos = true;
        
        // References
        private Camera mainCamera;
        private PassiveTreeManager treeManager;
        
        // Input state
        private bool isMousePressed = false;
        private Vector2 lastMousePosition;
        
        void Start()
        {
            // Get camera reference
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[WorldSpaceInputHandler] No main camera found!");
                enabled = false;
                return;
            }
            
            // Get tree manager reference
            treeManager = FindFirstObjectByType<PassiveTreeManager>();
            if (treeManager == null)
            {
                Debug.LogError("[WorldSpaceInputHandler] No PassiveTreeManager found!");
                enabled = false;
                return;
            }
            
            Debug.Log($"[WorldSpaceInputHandler] Initialized - Camera: {mainCamera.name}, TreeManager: {treeManager.name}");
        }
        
        void Update()
        {
            HandleMouseInput();
        }
        
        /// <summary>
        /// Handle mouse input using new Input System
        /// </summary>
        private void HandleMouseInput()
        {
            if (mainCamera == null) return;
            
            // Get mouse position
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0f));
            
            // Check for mouse press
            bool mousePressed = Mouse.current.leftButton.wasPressedThisFrame;
            bool mouseReleased = Mouse.current.leftButton.wasReleasedThisFrame;
            
            // Handle mouse press
            if (mousePressed)
            {
                HandleMousePress(worldPosition);
            }
            
            // Handle mouse release
            if (mouseReleased)
            {
                HandleMouseRelease(worldPosition);
            }
            
            // Store last position
            lastMousePosition = mousePosition;
        }
        
        /// <summary>
        /// Handle mouse press - find and interact with cell
        /// </summary>
        private void HandleMousePress(Vector3 worldPosition)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[WorldSpaceInputHandler] Mouse pressed at world position: {worldPosition}");
            }
            
            // Perform Physics2D raycast
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero, maxRaycastDistance, cellLayerMask);
            
            if (hit.collider != null)
            {
                CellController cellController = hit.collider.GetComponent<CellController>();
                if (cellController != null)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"[WorldSpaceInputHandler] Hit cell: {cellController.GridPosition} at {hit.point}");
                    }
                    
                    // Notify tree manager
                    treeManager.OnCellClicked(cellController.GridPosition);
                }
                else
                {
                    if (showDebugInfo)
                    {
                        Debug.LogWarning($"[WorldSpaceInputHandler] Hit object {hit.collider.name} but no CellController found");
                    }
                }
            }
            else
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[WorldSpaceInputHandler] No cell hit at world position: {worldPosition}");
                }
            }
        }
        
        /// <summary>
        /// Handle mouse release
        /// </summary>
        private void HandleMouseRelease(Vector3 worldPosition)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[WorldSpaceInputHandler] Mouse released at world position: {worldPosition}");
            }
        }
        
        /// <summary>
        /// Draw debug gizmos in scene view
        /// </summary>
        void OnDrawGizmos()
        {
            if (!showRaycastGizmos || mainCamera == null) return;
            
            // Draw mouse position
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0f));
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(worldPosition, 0.5f);
            
            // Draw raycast line
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(worldPosition, worldPosition + Vector3.forward * maxRaycastDistance);
        }
        
        /// <summary>
        /// Debug method to test raycasting at current mouse position
        /// </summary>
        [ContextMenu("Test Raycast at Mouse Position")]
        public void TestRaycastAtMousePosition()
        {
            if (mainCamera == null)
            {
                Debug.LogError("[WorldSpaceInputHandler] No camera available for testing");
                return;
            }
            
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0f));
            
            Debug.Log($"[WorldSpaceInputHandler] Testing raycast at mouse position: {mousePosition} -> world: {worldPosition}");
            
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero, maxRaycastDistance, cellLayerMask);
            
            if (hit.collider != null)
            {
                Debug.Log($"✅ Hit: {hit.collider.name} at {hit.point}");
                CellController cellController = hit.collider.GetComponent<CellController>();
                if (cellController != null)
                {
                    Debug.Log($"  - CellController: {cellController.GridPosition}");
                }
            }
            else
            {
                Debug.LogWarning("❌ No hit detected");
            }
        }
    }
}

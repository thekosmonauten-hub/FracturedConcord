using UnityEngine;
using UnityEngine.InputSystem;

namespace PassiveTree
{
    /// <summary>
    /// Simple camera controller for the passive tree
    /// Handles basic camera movement and zoom
    /// </summary>
    public class SimpleCameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float minZoom = 2f;
        [SerializeField] private float maxZoom = 10f;
        
        [Header("Mouse Drag Settings")]
        [SerializeField] private bool enableMouseDrag = true;
        [SerializeField] private float dragSensitivity = 2f;
        [SerializeField] private float dragResponseSpeed = 15f; // Higher speed for more responsive dragging
        
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string moveActionName = "Move";
        [SerializeField] private string scrollActionName = "ScrollWheel";
        [SerializeField] private string clickActionName = "Click";
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        
        private Camera cam;
        private Vector3 targetPosition;
        private float targetZoom;
        private InputAction moveAction;
        private InputAction scrollAction;
        private InputAction clickAction;
        
        // Mouse drag variables
        private bool isDragging = false;
        private Vector2 lastMousePosition;
        private Vector3 dragStartCameraPosition;
        
        void Start()
        {
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                Debug.LogError("[SimpleCameraController] No Camera component found!");
                return;
            }
            
            targetPosition = transform.position;
            targetZoom = cam.orthographicSize;
            
            // Initialize input actions
            InitializeInputActions();
            
            if (showDebugInfo)
                Debug.Log("[SimpleCameraController] Initialized");
        }
        
        void OnEnable()
        {
            // Enable input actions when the component is enabled
            if (moveAction != null) moveAction.Enable();
            if (scrollAction != null) scrollAction.Enable();
            if (clickAction != null) clickAction.Enable();
        }
        
        void OnDisable()
        {
            // Disable input actions when the component is disabled
            if (moveAction != null) moveAction.Disable();
            if (scrollAction != null) scrollAction.Disable();
            if (clickAction != null) clickAction.Disable();
        }
        
        void Update()
        {
            HandleInput();
            UpdateCamera();
        }
        
        /// <summary>
        /// Initialize input actions
        /// </summary>
        private void InitializeInputActions()
        {
            // Try to find the input actions asset if not assigned
            if (inputActions == null)
            {
                inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
                if (inputActions == null)
                {
                    Debug.LogError("[SimpleCameraController] No InputActionAsset found! Please assign one in the inspector.");
                    return;
                }
            }
            
            // Get the move action from the Player action map
            var playerMap = inputActions.FindActionMap("Player");
            if (playerMap != null)
            {
                moveAction = playerMap.FindAction(moveActionName);
                if (moveAction == null)
                {
                    Debug.LogError($"[SimpleCameraController] Move action '{moveActionName}' not found in Player action map!");
                }
            }
            else
            {
                Debug.LogError("[SimpleCameraController] Player action map not found!");
            }
            
            // Get the scroll action from the UI action map
            var uiMap = inputActions.FindActionMap("UI");
            if (uiMap != null)
            {
                scrollAction = uiMap.FindAction(scrollActionName);
                if (scrollAction == null)
                {
                    Debug.LogError($"[SimpleCameraController] Scroll action '{scrollActionName}' not found in UI action map!");
                }
                
                // Get the click action from the UI action map
                clickAction = uiMap.FindAction(clickActionName);
                if (clickAction == null)
                {
                    Debug.LogError($"[SimpleCameraController] Click action '{clickActionName}' not found in UI action map!");
                }
            }
            else
            {
                Debug.LogError("[SimpleCameraController] UI action map not found!");
            }
        }
        
        /// <summary>
        /// Handle input for camera movement and zoom
        /// </summary>
        private void HandleInput()
        {
            // Mouse drag movement
            if (enableMouseDrag)
            {
                HandleMouseDrag();
            }
            
            // Movement input using new Input System
            if (moveAction != null && moveAction.enabled)
            {
                Vector2 moveInput = moveAction.ReadValue<Vector2>();
                if (moveInput != Vector2.zero)
                {
                    Vector3 moveDirection = new Vector3(moveInput.x, moveInput.y, 0);
                    targetPosition += moveDirection.normalized * moveSpeed * Time.deltaTime;
                }
            }
            
            // Zoom input using new Input System
            if (scrollAction != null && scrollAction.enabled)
            {
                Vector2 scrollInput = scrollAction.ReadValue<Vector2>();
                if (scrollInput.y != 0f)
                {
                    targetZoom -= scrollInput.y * zoomSpeed;
                    targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
                }
            }
        }
        
        /// <summary>
        /// Handle mouse drag for camera movement using new Input System
        /// </summary>
        private void HandleMouseDrag()
        {
            // Use Mouse device directly as fallback if click action is not available
            if (Mouse.current == null) return;
            
            // Check for left mouse button press using new Input System
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                isDragging = true;
                lastMousePosition = Mouse.current.position.ReadValue();
                dragStartCameraPosition = transform.position; // Store actual camera position, not target
                
                if (showDebugInfo)
                    Debug.Log("[SimpleCameraController] Started mouse drag");
            }
            
            // Handle dragging - apply movement directly to camera transform
            if (isDragging && Mouse.current.leftButton.isPressed)
            {
                Vector2 currentMousePosition = Mouse.current.position.ReadValue();
                Vector2 mouseDelta = currentMousePosition - lastMousePosition;
                
                // Convert screen delta to world delta
                // Invert Y axis for intuitive camera movement
                Vector3 worldDelta = new Vector3(-mouseDelta.x, -mouseDelta.y, 0) * dragSensitivity * 0.01f;
                
                // Apply zoom-based sensitivity (closer zoom = more sensitive)
                float zoomFactor = cam != null ? cam.orthographicSize / 5f : 1f;
                worldDelta *= zoomFactor;
                
                // Apply movement directly to camera transform
                transform.position = dragStartCameraPosition + worldDelta;
                
                // Update target position to match current position for smooth transitions
                targetPosition = transform.position;
            }
            
            // Check for left mouse button release using new Input System
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                isDragging = false;
                
                if (showDebugInfo)
                    Debug.Log("[SimpleCameraController] Ended mouse drag");
            }
        }
        
        /// <summary>
        /// Update camera position and zoom
        /// </summary>
        private void UpdateCamera()
        {
            // Only update position if not dragging (mouse drag handles position directly)
            if (!isDragging)
            {
                // Smooth movement for keyboard input and other interactions
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);
            }
            
            // Smooth zoom
            if (cam != null)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * 5f);
            }
        }
        
        /// <summary>
        /// Focus camera on a specific position
        /// </summary>
        public void FocusOnPosition(Vector3 position)
        {
            targetPosition = new Vector3(position.x, position.y, transform.position.z);
            
            if (showDebugInfo)
                Debug.Log($"[SimpleCameraController] Focusing on position: {position}");
        }
        
        /// <summary>
        /// Set camera zoom level
        /// </summary>
        public void SetZoom(float zoom)
        {
            targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
            
            if (showDebugInfo)
                Debug.Log($"[SimpleCameraController] Setting zoom to: {targetZoom}");
        }
        
        /// <summary>
        /// Reset camera to center position
        /// </summary>
        public void ResetToCenter()
        {
            targetPosition = new Vector3(0, 0, transform.position.z);
            targetZoom = 5f;
            
            if (showDebugInfo)
                Debug.Log("[SimpleCameraController] Reset to center");
        }
        
        /// <summary>
        /// Enable or disable mouse drag functionality
        /// </summary>
        public void SetMouseDragEnabled(bool enabled)
        {
            enableMouseDrag = enabled;
            
            if (showDebugInfo)
                Debug.Log($"[SimpleCameraController] Mouse drag {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Set mouse drag sensitivity
        /// </summary>
        public void SetDragSensitivity(float sensitivity)
        {
            dragSensitivity = Mathf.Max(0.1f, sensitivity);
            
            if (showDebugInfo)
                Debug.Log($"[SimpleCameraController] Drag sensitivity set to: {dragSensitivity}");
        }
        
        /// <summary>
        /// Set drag response speed (how quickly camera responds during dragging)
        /// </summary>
        public void SetDragResponseSpeed(float speed)
        {
            dragResponseSpeed = Mathf.Max(1f, speed);
            
            if (showDebugInfo)
                Debug.Log($"[SimpleCameraController] Drag response speed set to: {dragResponseSpeed}");
        }
    }
}

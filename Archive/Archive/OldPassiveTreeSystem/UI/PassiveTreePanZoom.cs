using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Handles pan and zoom functionality for the passive tree canvas
/// Supports mouse drag to pan, mouse wheel to zoom, and touch controls
/// </summary>
public class PassiveTreePanZoom : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
    [Header("Pan Settings")]
    [SerializeField] private float panSpeed = 1f;
    [SerializeField] private bool enablePan = true;
    [SerializeField] private bool enableKeyboardControls = true;
    
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 0.5f;
    [SerializeField] private float minZoom = 0.5f;
    [SerializeField] private float maxZoom = 3f;
    [SerializeField] private bool enableZoom = true;
    
    [Header("Smooth Movement")]
    [SerializeField] private bool smoothMovement = true;
    [SerializeField] private float smoothSpeed = 10f;
    
    [Header("Bounds")]
    [SerializeField] private bool enableBounds = true;
    [SerializeField] private float maxPanDistance = 1000f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
    // References
    private RectTransform canvasRectTransform;
    private RectTransform boardContainer;
    private Vector2 lastMousePosition;
    private Vector2 targetPosition;
    private float targetZoom = 1f;
    private bool isDragging = false;
    
    // Current state
    private Vector2 currentPosition;
    private float currentZoom = 1f;
    
    // Public properties for UI access
    public float CurrentZoom => currentZoom;
    public float TargetZoom => targetZoom;
    public Vector2 CurrentPosition => currentPosition;
    public Vector2 TargetPosition => targetPosition;
    public float MinZoom => minZoom;
    public float MaxZoom => maxZoom;
    public bool IsDragging => isDragging;
    
    private void Awake()
    {
        // Get the canvas rect transform
        canvasRectTransform = GetComponent<RectTransform>();
        if (canvasRectTransform == null)
        {
            Debug.LogError("[PassiveTreePanZoom] No RectTransform found on this GameObject!");
            enabled = false;
            return;
        }
        
        // Find the board container
        boardContainer = transform.Find("BoardContainer")?.GetComponent<RectTransform>();
        if (boardContainer == null)
        {
            Debug.LogWarning("[PassiveTreePanZoom] BoardContainer not found, will search for it...");
            // Try to find it in children
            boardContainer = GetComponentInChildren<RectTransform>();
        }
        
        if (boardContainer == null)
        {
            Debug.LogError("[PassiveTreePanZoom] No board container found! Pan/Zoom will not work properly.");
            enabled = false;
            return;
        }
        
        // Initialize position and zoom
        currentPosition = boardContainer.anchoredPosition;
        targetPosition = currentPosition;
        currentZoom = boardContainer.localScale.x;
        targetZoom = currentZoom;
        
        if (showDebugInfo)
        {
            Debug.Log($"[PassiveTreePanZoom] Initialized with position: {currentPosition}, zoom: {currentZoom}");
        }
    }
    
    private void Update()
    {
        // Handle smooth movement
        if (smoothMovement)
        {
            // Smooth position
            if (Vector2.Distance(currentPosition, targetPosition) > 0.01f)
            {
                currentPosition = Vector2.Lerp(currentPosition, targetPosition, smoothSpeed * Time.deltaTime);
                boardContainer.anchoredPosition = currentPosition;
            }
            
            // Smooth zoom
            if (Mathf.Abs(currentZoom - targetZoom) > 0.01f)
            {
                currentZoom = Mathf.Lerp(currentZoom, targetZoom, smoothSpeed * Time.deltaTime);
                boardContainer.localScale = Vector3.one * currentZoom;
            }
        }
        
        // Handle keyboard controls
        HandleKeyboardInput();
    }
    
    /// <summary>
    /// Handle keyboard input for pan and zoom
    /// </summary>
    private void HandleKeyboardInput()
    {
        if (!enablePan && !enableZoom || !enableKeyboardControls) return;
        
        Vector2 panInput = Vector2.zero;
        float zoomInput = 0f;
        
        // Try new Input System first
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            // Pan with arrow keys or WASD
            if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed)
                panInput.x -= 1f;
            if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed)
                panInput.x += 1f;
            if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed)
                panInput.y += 1f;
            if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed)
                panInput.y -= 1f;
            
            // Zoom with +/- keys
            if (keyboard.numpadPlusKey.isPressed || keyboard.equalsKey.isPressed)
                zoomInput += 1f;
            if (keyboard.minusKey.isPressed)
                zoomInput -= 1f;
        }
        else
        {
            // Fallback to old Input System if new Input System is not available
            try
            {
                // Pan with arrow keys or WASD
                if (UnityEngine.Input.GetKey(KeyCode.LeftArrow) || UnityEngine.Input.GetKey(KeyCode.A))
                    panInput.x -= 1f;
                if (UnityEngine.Input.GetKey(KeyCode.RightArrow) || UnityEngine.Input.GetKey(KeyCode.D))
                    panInput.x += 1f;
                if (UnityEngine.Input.GetKey(KeyCode.UpArrow) || UnityEngine.Input.GetKey(KeyCode.W))
                    panInput.y += 1f;
                if (UnityEngine.Input.GetKey(KeyCode.DownArrow) || UnityEngine.Input.GetKey(KeyCode.S))
                    panInput.y -= 1f;
                
                // Zoom with +/- keys
                if (UnityEngine.Input.GetKey(KeyCode.Plus) || UnityEngine.Input.GetKey(KeyCode.Equals))
                    zoomInput += 1f;
                if (UnityEngine.Input.GetKey(KeyCode.Minus))
                    zoomInput -= 1f;
            }
            catch (System.InvalidOperationException)
            {
                // Input System conflict - keyboard controls disabled
                if (showDebugInfo)
                {
                    Debug.LogWarning("[PassiveTreePanZoom] Input System conflict detected. Keyboard controls disabled. Use mouse/touch controls instead.");
                }
                return;
            }
        }
        
        // Apply keyboard input
        if (panInput.magnitude > 0f && enablePan)
        {
            Vector2 newPosition = targetPosition + panInput * panSpeed * 50f * Time.deltaTime;
            SetTargetPosition(newPosition);
        }
        
        if (zoomInput != 0f && enableZoom)
        {
            float newZoom = targetZoom + zoomInput * zoomSpeed * Time.deltaTime;
            SetTargetZoom(newZoom);
        }
    }
    
    #region Mouse/Touch Event Handlers
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!enablePan) return;
        
        // Check if we're over a UI element that should block panning
        if (IsOverBlockingUI(eventData.position))
            return;
        
        isDragging = true;
        lastMousePosition = eventData.position;
        
        if (showDebugInfo)
        {
            Debug.Log($"[PassiveTreePanZoom] Started dragging at {lastMousePosition}");
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!enablePan || !isDragging) return;
        
        Vector2 delta = eventData.position - lastMousePosition;
        Vector2 panDelta = delta * panSpeed;
        
        Vector2 newPosition = targetPosition + panDelta;
        SetTargetPosition(newPosition);
        
        lastMousePosition = eventData.position;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!enablePan) return;
        
        isDragging = false;
        
        if (showDebugInfo)
        {
            Debug.Log($"[PassiveTreePanZoom] Stopped dragging at {eventData.position}");
        }
    }
    
    public void OnScroll(PointerEventData eventData)
    {
        if (!enableZoom) return;
        
        // Check if we're over a UI element that should block zooming
        if (IsOverBlockingUI(eventData.position))
            return;
        
        float zoomDelta = eventData.scrollDelta.y * zoomSpeed * 0.1f;
        float newZoom = targetZoom + zoomDelta;
        SetTargetZoom(newZoom);
        
        if (showDebugInfo)
        {
            Debug.Log($"[PassiveTreePanZoom] Scrolled: {eventData.scrollDelta.y}, new zoom: {newZoom}");
        }
    }
    
    #endregion
    
    /// <summary>
    /// Check if the pointer is over a UI element that should block pan/zoom
    /// </summary>
    private bool IsOverBlockingUI(Vector2 screenPosition)
    {
        // Check if we're over any UI elements that should block interaction
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = screenPosition }, results);
        
        foreach (var result in results)
        {
            // Check if the UI element should block pan/zoom
            if (result.gameObject.GetComponent<Button>() != null ||
                result.gameObject.GetComponent<InputField>() != null ||
                result.gameObject.GetComponent<ScrollRect>() != null)
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Set the target position with bounds checking
    /// </summary>
    public void SetTargetPosition(Vector2 newPosition)
    {
        if (enableBounds)
        {
            // Apply bounds to prevent panning too far
            newPosition.x = Mathf.Clamp(newPosition.x, -maxPanDistance, maxPanDistance);
            newPosition.y = Mathf.Clamp(newPosition.y, -maxPanDistance, maxPanDistance);
        }
        
        targetPosition = newPosition;
        
        if (!smoothMovement)
        {
            currentPosition = targetPosition;
            boardContainer.anchoredPosition = currentPosition;
        }
    }
    
    /// <summary>
    /// Set the target zoom with bounds checking
    /// </summary>
    public void SetTargetZoom(float newZoom)
    {
        targetZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);
        
        if (!smoothMovement)
        {
            currentZoom = targetZoom;
            boardContainer.localScale = Vector3.one * currentZoom;
        }
    }
    
    /// <summary>
    /// Reset to default position and zoom
    /// </summary>
    [ContextMenu("Reset View")]
    public void ResetView()
    {
        SetTargetPosition(Vector2.zero);
        SetTargetZoom(1f);
        
        if (showDebugInfo)
        {
            Debug.Log("[PassiveTreePanZoom] Reset view to center with 1x zoom");
        }
    }
    
    /// <summary>
    /// Fit all content to view
    /// </summary>
    [ContextMenu("Fit to View")]
    public void FitToView()
    {
        // This would require knowing the content bounds
        // For now, just reset to a reasonable zoom
        SetTargetZoom(0.8f);
        SetTargetPosition(Vector2.zero);
        
        if (showDebugInfo)
        {
            Debug.Log("[PassiveTreePanZoom] Fit to view applied");
        }
    }
    
    /// <summary>
    /// Get current pan and zoom state
    /// </summary>
    [ContextMenu("Show Current State")]
    public void ShowCurrentState()
    {
        Debug.Log($"[PassiveTreePanZoom] Position: {currentPosition}, Zoom: {currentZoom}");
        Debug.Log($"[PassiveTreePanZoom] Target Position: {targetPosition}, Target Zoom: {targetZoom}");
        Debug.Log($"[PassiveTreePanZoom] Is Dragging: {isDragging}");
    }
}

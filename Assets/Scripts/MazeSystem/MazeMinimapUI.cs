using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using Dexiled.MazeSystem;

/// <summary>
/// UI component for displaying the maze minimap in place of cards during maze mode.
/// Integrates with CombatScene to reuse player display and resources.
/// </summary>
public class MazeMinimapUI : MonoBehaviour
{
    [Header("UI Container")]
    [Tooltip("Parent container for the minimap (where cards normally are)")]
    public RectTransform minimapContainer;
    
    [Header("Minimap Grid")]
    [Tooltip("Grid layout container for node buttons")]
    public GridLayoutGroup minimapGrid;
    
    [Tooltip("Node button prefab (created if not assigned)")]
    public GameObject nodeButtonPrefab;
    
    [Header("Run Info Display")]
    [Tooltip("Text displaying current floor")]
    public TextMeshProUGUI floorText;
    
    [Tooltip("Text displaying run seed (optional)")]
    public TextMeshProUGUI seedText;
    
    [Tooltip("Container for active modifiers (shrines, curses)")]
    public Transform modifiersContainer;
    
    [Tooltip("Prefab for modifier display")]
    public GameObject modifierDisplayPrefab;
    
    [Header("Node Visual Settings")]
    [Tooltip("Size of each node button in the grid")]
    public Vector2 nodeSize = new Vector2(80f, 80f);
    
    [Tooltip("Spacing between nodes")]
    public Vector2 nodeSpacing = new Vector2(10f, 10f);
    
    [Header("Node Sprites/Icons")]
    [Tooltip("Icon for Start node")]
    public Sprite startNodeIcon;
    [Tooltip("Icon for Combat node")]
    public Sprite combatNodeIcon;
    [Tooltip("Icon for Chest node")]
    public Sprite chestNodeIcon;
    [Tooltip("Icon for Shrine node")]
    public Sprite shrineNodeIcon;
    [Tooltip("Icon for Trap node")]
    public Sprite trapNodeIcon;
    [Tooltip("Icon for Forge node")]
    public Sprite forgeNodeIcon;
    [Tooltip("Icon for Boss node")]
    public Sprite bossNodeIcon;
    [Tooltip("Icon for Stairs node")]
    public Sprite stairsNodeIcon;
    [Tooltip("Icon for Special node")]
    public Sprite specialNodeIcon;
    [Tooltip("Icon for unrevealed/hidden nodes")]
    public Sprite hiddenNodeIcon;
    
    [Header("Node State Colors")]
    public Color unrevealedColor = new Color(0.2f, 0.2f, 0.2f, 0.5f); // Dark, semi-transparent
    public Color revealedColor = Color.white;
    public Color visitedColor = new Color(0.5f, 0.5f, 0.5f, 0.8f); // Grayed out
    public Color selectableColor = new Color(1f, 1f, 0f, 1f); // Yellow highlight
    public Color currentPositionColor = new Color(0f, 1f, 0f, 1f); // Green
    
    // Runtime data
    private Dictionary<Vector2Int, GameObject> nodeButtons = new Dictionary<Vector2Int, GameObject>();
    private MazeRunData currentRun;
    private MazeFloor currentFloor;
    
    private void OnEnable()
    {
        // Subscribe to maze run events
        if (MazeRunManager.Instance != null)
        {
            MazeRunManager.Instance.OnRunStarted += OnRunStarted;
            MazeRunManager.Instance.OnFloorGenerated += OnFloorGenerated;
            MazeRunManager.Instance.OnNodeEntered += OnNodeEntered;
            MazeRunManager.Instance.OnRunStateChanged += OnRunStateChanged;
        }
    }
    
    private void OnDisable()
    {
        // Unsubscribe from events
        if (MazeRunManager.Instance != null)
        {
            MazeRunManager.Instance.OnRunStarted -= OnRunStarted;
            MazeRunManager.Instance.OnFloorGenerated -= OnFloorGenerated;
            MazeRunManager.Instance.OnNodeEntered -= OnNodeEntered;
            MazeRunManager.Instance.OnRunStateChanged -= OnRunStateChanged;
        }
    }
    
    private void Start()
    {
        InitializeMinimap();
        
        // Ensure EventSystem exists (required for UI interactions)
        EnsureEventSystem();
        
        // If there's already an active run, display it
        if (MazeRunManager.Instance != null && MazeRunManager.Instance.HasActiveRun())
        {
            currentRun = MazeRunManager.Instance.GetCurrentRun();
            if (currentRun != null)
            {
                currentFloor = currentRun.GetCurrentFloor();
                if (currentFloor != null)
                {
                    DisplayFloor(currentFloor);
                }
            }
        }
    }
    
    /// <summary>
    /// Ensures EventSystem and GraphicRaycaster exist for UI interactions.
    /// </summary>
    private void EnsureEventSystem()
    {
        // Check for EventSystem
        if (EventSystem.current == null)
        {
            Debug.LogWarning("[MazeMinimapUI] No EventSystem found! Creating one...");
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }
        
        // Check for Canvas and GraphicRaycaster
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                Debug.LogWarning("[MazeMinimapUI] Canvas found but no GraphicRaycaster! Adding one...");
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }
            
            // Ensure Canvas is set up correctly
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                Debug.Log($"[MazeMinimapUI] Canvas found: {canvas.name}, RenderMode: {canvas.renderMode}, Enabled: {canvas.enabled}");
            }
        }
        else
        {
            Debug.LogWarning("[MazeMinimapUI] No Canvas found in parent hierarchy! UI interactions may not work.");
        }
    }
    
    private void InitializeMinimap()
    {
        // Ensure we have a container
        if (minimapContainer == null)
        {
            minimapContainer = GetComponent<RectTransform>();
            if (minimapContainer == null)
            {
                Debug.LogError("[MazeMinimapUI] No minimap container assigned!");
                return;
            }
        }
        
        // Create grid layout if not assigned
        if (minimapGrid == null)
        {
            GameObject gridObj = new GameObject("MinimapGrid");
            gridObj.transform.SetParent(minimapContainer, false);
            minimapGrid = gridObj.AddComponent<GridLayoutGroup>();
            minimapGrid.cellSize = nodeSize;
            minimapGrid.spacing = nodeSpacing;
            minimapGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            
            // Set up rect transform
            RectTransform gridRect = gridObj.GetComponent<RectTransform>();
            if (gridRect != null)
            {
                gridRect.anchorMin = Vector2.zero;
                gridRect.anchorMax = Vector2.one;
                gridRect.sizeDelta = Vector2.zero;
                gridRect.anchoredPosition = Vector2.zero;
            }
        }
        
        // Configure grid
        if (mazeConfig != null && mazeConfig.gridSize.x > 0)
        {
            minimapGrid.constraintCount = mazeConfig.gridSize.x;
        }
        else
        {
            minimapGrid.constraintCount = 4; // Default 4 columns
        }
    }
    
    private MazeConfig mazeConfig
    {
        get
        {
            return MazeRunManager.Instance != null ? MazeRunManager.Instance.mazeConfig : null;
        }
    }
    
    #region Event Handlers
    
    private void OnRunStarted(MazeRunData run)
    {
        currentRun = run;
        if (run != null)
        {
            currentFloor = run.GetCurrentFloor();
            if (currentFloor != null)
            {
                DisplayFloor(currentFloor);
                UpdateRunInfo(run);
            }
        }
    }
    
    private void OnFloorGenerated(MazeFloor floor)
    {
        if (currentRun != null && floor != null)
        {
            currentFloor = floor;
            DisplayFloor(floor);
            UpdateRunInfo(currentRun);
        }
    }
    
    private void OnNodeEntered(MazeNode node)
    {
        // Refresh display to show updated node states
        if (currentFloor != null)
        {
            RefreshNodeDisplay(currentFloor);
            UpdateRunInfo(currentRun);
        }
    }
    
    private void OnRunStateChanged(MazeRunData run)
    {
        UpdateRunInfo(run);
        
        // Refresh node display to update current position indicator and other visual states
        // This is important when navigating between already-visited nodes
        if (currentFloor != null)
        {
            RefreshNodeDisplay(currentFloor);
        }
    }
    
    #endregion
    
    #region Display Methods
    
    /// <summary>
    /// Displays a floor's minimap with all nodes.
    /// </summary>
    public void DisplayFloor(MazeFloor floor)
    {
        if (floor == null || minimapGrid == null)
            return;
        
        currentFloor = floor;
        
        // Clear existing buttons
        ClearMinimap();
        
        // Get grid size
        int gridWidth = mazeConfig != null ? mazeConfig.gridSize.x : 4;
        int gridHeight = mazeConfig != null ? mazeConfig.gridSize.y : 4;
        
        // Create buttons for all grid positions (for layout)
        // All positions are created to maintain grid layout, but only revealed nodes will be visible
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                MazeNode node = floor.GetNode(pos);
                CreateNodeButton(pos, node);
            }
        }
        
        // Refresh display to show correct states
        RefreshNodeDisplay(floor);
        
        Debug.Log($"[MazeMinimapUI] Displayed floor {floor.floorNumber} with {floor.nodes.Count} nodes");
    }
    
    /// <summary>
    /// Creates a button for a node position (or empty space).
    /// </summary>
    private void CreateNodeButton(Vector2Int position, MazeNode node)
    {
        // Create button GameObject
        GameObject buttonObj;
        
        if (nodeButtonPrefab != null)
        {
            buttonObj = Instantiate(nodeButtonPrefab, minimapGrid.transform);
        }
        else
        {
            // Create default button if no prefab
            buttonObj = new GameObject($"Node_{position.x}_{position.y}");
            buttonObj.transform.SetParent(minimapGrid.transform, false);
            
            // Add button component
            Button button = buttonObj.AddComponent<Button>();
            Image image = buttonObj.AddComponent<Image>();
            
            // Add child image for icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(buttonObj.transform, false);
            Image iconImage = iconObj.AddComponent<Image>();
            
            RectTransform iconRect = iconImage.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.sizeDelta = Vector2.zero;
            iconRect.anchoredPosition = Vector2.zero;
            
            // Set button colors
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            button.colors = colors;
        }
        
        // NEW STRUCTURE: Button should be on root GameObject, Background and Icon are children
        // Check if Button is on root (new structure) or in Button child (old structure)
        Button btn = buttonObj.GetComponent<Button>();
        Transform buttonTransform = buttonObj.transform;
        Image buttonImage = null;
        
        if (btn == null)
        {
            // Try old structure: Button child GameObject
            Transform buttonChild = buttonObj.transform.Find("Button");
            if (buttonChild != null)
            {
                btn = buttonChild.GetComponent<Button>();
                buttonTransform = buttonChild;
                Debug.LogWarning($"[MazeMinimapUI] Using old prefab structure (Button in child). Consider updating prefab to have Button on root.");
            }
        }
        
        if (btn == null)
        {
            // No Button found - add one to root
            Debug.LogWarning($"[MazeMinimapUI] No Button component found for node at {position}. Adding Button to root GameObject.");
            btn = buttonObj.AddComponent<Button>();
            buttonTransform = buttonObj.transform;
        }
        
        // Get button's Image component (should be on Background child, or root if no Background)
        Transform backgroundTransform = buttonObj.transform.Find("Background");
        if (backgroundTransform != null)
        {
            buttonImage = backgroundTransform.GetComponent<Image>();
        }
        
        // If no Background, try to get Image from root or Button child
        if (buttonImage == null)
        {
            buttonImage = buttonTransform.GetComponent<Image>();
        }
        
        // Ensure button GameObject is active
        buttonTransform.gameObject.SetActive(true);
        
        // Set up button Image for click detection
        if (buttonImage != null)
        {
            // Ensure the button's Image allows raycasts for click detection
            buttonImage.raycastTarget = true;
            
            // Unity UI buttons need the target graphic to have some alpha (even tiny) for click detection
            // If it's fully transparent (alpha = 0), clicks won't register properly
            Color currentColor = buttonImage.color;
            if (currentColor.a == 0f || currentColor.a < 0.01f)
            {
                // Set to nearly transparent but not fully transparent - this ensures clicks work
                buttonImage.color = new Color(1f, 1f, 1f, 0.01f);
                Debug.Log($"[MazeMinimapUI] Fixed transparent button Image at {position} - set alpha to 0.01 for click detection");
            }
            
            // Ensure the Image component is enabled
            buttonImage.enabled = true;
            
            // CRITICAL: Set the Button's targetGraphic to this Image component
            // Unity UI buttons need targetGraphic to be set for onClick to work
            if (btn.targetGraphic != buttonImage)
            {
                btn.targetGraphic = buttonImage;
                Debug.Log($"[MazeMinimapUI] Set Button targetGraphic to Image component at {position}");
            }
        }
        else
        {
            Debug.LogWarning($"[MazeMinimapUI] No Image component found for button at {position}! Button clicks may not work.");
        }
        
        // Ensure the Button component itself is enabled
        btn.enabled = true;
        
        // Clear existing listeners first
        btn.onClick.RemoveAllListeners();
        
        if (node != null)
        {
            // Store position in a local variable to avoid closure issues
            Vector2Int nodePosition = position;
            
            // Add click listener for nodes (Button onClick)
            btn.onClick.AddListener(() => 
            {
                Debug.Log($"[MazeMinimapUI] Button onClick triggered for position {nodePosition}");
                OnNodeButtonClicked(nodePosition);
            });
            
            // ALSO add EventTrigger as fallback (in case Button onClick doesn't work)
            // Add to root GameObject (buttonObj) so it catches all pointer events
            EventTrigger trigger = buttonObj.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = buttonObj.AddComponent<EventTrigger>();
            }
            
            // Clear existing triggers
            trigger.triggers.Clear();
            
            // Add PointerClick event
            EventTrigger.Entry clickEntry = new EventTrigger.Entry();
            clickEntry.eventID = EventTriggerType.PointerClick;
            clickEntry.callback.AddListener((data) => 
            {
                Debug.Log($"[MazeMinimapUI] EventTrigger PointerClick triggered for position {nodePosition}");
                OnNodeButtonClicked(nodePosition);
            });
            trigger.triggers.Add(clickEntry);
            
            // Add PointerDown event for debugging
            EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
            pointerDownEntry.eventID = EventTriggerType.PointerDown;
            pointerDownEntry.callback.AddListener((data) => 
            {
                Debug.Log($"[MazeMinimapUI] EventTrigger PointerDown detected for position {nodePosition}");
            });
            trigger.triggers.Add(pointerDownEntry);
            
            // Initially set interactable based on node state (will be updated in UpdateNodeButtonVisual)
            btn.interactable = node.isSelectable && !node.visited;
            
            // Ensure EventSystem exists (required for UI interactions)
            if (EventSystem.current == null)
            {
                Debug.LogWarning("[MazeMinimapUI] No EventSystem found! Creating one...");
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<StandaloneInputModule>();
            }
            
            Debug.Log($"[MazeMinimapUI] Set up click listener (Button + EventTrigger) for node at {position} (Selectable: {node.isSelectable}, Visited: {node.visited}, ButtonInteractable: {btn.interactable})");
        }
        else
        {
            // Disable button for empty spaces
            btn.interactable = false;
            if (buttonImage != null)
            {
                buttonImage.raycastTarget = false; // Don't block raycasts for empty spaces
            }
        }
        
        // Store reference
        nodeButtons[position] = buttonObj;
        
        // Add a test component to verify pointer events are being received
        // Add to root GameObject (buttonObj) so it catches all pointer events
        if (node != null)
        {
            // Add a simple pointer event handler for debugging
            MazeNodeButtonDebugger debugger = buttonObj.GetComponent<MazeNodeButtonDebugger>();
            if (debugger == null)
            {
                debugger = buttonObj.AddComponent<MazeNodeButtonDebugger>();
            }
            debugger.Initialize(position, this);
        }
        
        // Update visual state
        UpdateNodeButtonVisual(buttonObj, node);
    }
    
    /// <summary>
    /// Updates the visual appearance of a node button based on node state.
    /// Prefab structure: MazeNodeButton (root) -> Background (grid tile), Button -> Icon (node icon)
    /// </summary>
    private void UpdateNodeButtonVisual(GameObject buttonObj, MazeNode node)
    {
        if (buttonObj == null)
            return;
        
        // NEW STRUCTURE: Button on root, Background and Icon are children
        // Find Background child (for grid tile sprite)
        Transform backgroundTransform = buttonObj.transform.Find("Background");
        Image bgImage = backgroundTransform != null ? backgroundTransform.GetComponent<Image>() : null;
        
        // Ensure Background doesn't block raycasts (clicks should go to Button on root)
        if (bgImage != null)
        {
            bgImage.raycastTarget = false; // Don't block clicks - let them pass through to Button
        }
        
        // Find Icon child (for node icon sprite)
        // Icon can be directly under root, or under a Button child (old structure)
        Transform iconTransform = buttonObj.transform.Find("Icon");
        if (iconTransform == null)
        {
            // Try old structure: Button/Icon
            Transform buttonChild = buttonObj.transform.Find("Button");
            if (buttonChild != null)
            {
                iconTransform = buttonChild.Find("Icon");
            }
        }
        Image iconImage = iconTransform != null ? iconTransform.GetComponent<Image>() : null;
        
        // Ensure Icon doesn't block raycasts (clicks should go to Button)
        if (iconImage != null)
        {
            iconImage.raycastTarget = false; // Don't block clicks - let them pass through to Button
        }
        
        // Get Button component (should be on root in new structure)
        Button btn = buttonObj.GetComponent<Button>();
        Transform buttonTransform = buttonObj.transform;
        
        // Fallback: try old structure (Button in child)
        if (btn == null)
        {
            Transform buttonChild = buttonObj.transform.Find("Button");
            if (buttonChild != null)
            {
                btn = buttonChild.GetComponent<Button>();
                buttonTransform = buttonChild;
            }
        }
        
        // Get grid tile sprite for this floor's tier (for background)
        Sprite gridTileSprite = null;
        if (currentFloor != null && mazeConfig != null)
        {
            gridTileSprite = mazeConfig.GetGridTileForFloor(currentFloor.floorNumber);
        }
        
        // Apply grid tile as background (for all buttons, including empty spaces)
        if (bgImage != null && gridTileSprite != null)
        {
            bgImage.sprite = gridTileSprite;
            bgImage.type = Image.Type.Sliced; // Use Sliced for 9-slice sprites if needed
        }
        
        // Handle empty spaces or unrevealed nodes
        if (node == null || !node.revealed)
        {
            // Empty space or unrevealed node - hide completely (or show very dimmed background)
            if (bgImage != null)
            {
                if (node == null)
                {
                    // Empty space - very dim grid tile
                    bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.2f);
                }
                else
                {
                    // Unrevealed node - hide completely
                    bgImage.color = new Color(0f, 0f, 0f, 0f);
                }
            }
            if (iconImage != null)
            {
                iconImage.color = new Color(0f, 0f, 0f, 0f); // Hide icon
                iconImage.sprite = null;
            }
            
            // Keep button GameObject active for layout, but make it invisible
            // (This maintains grid layout while hiding unrevealed nodes)
            return;
        }
        
        // Node is revealed - show it
        if (buttonObj != null)
        {
            buttonObj.SetActive(true);
        }
        
        // Set icon based on node type
        Sprite iconSprite = GetNodeIcon(node.nodeType);
        if (iconImage != null)
        {
            iconImage.sprite = iconSprite;
            iconImage.color = Color.white; // Reset to white, will be tinted below
        }
        
        // Determine node color based on state
        Color nodeColor;
        if (currentFloor != null && node.position == currentFloor.playerPosition)
        {
            nodeColor = currentPositionColor; // Current position - green
        }
        else if (node.isSelectable && !node.visited)
        {
            nodeColor = selectableColor; // Selectable - yellow
        }
        else if (node.visited)
        {
            nodeColor = visitedColor; // Visited - grayed out
        }
        else
        {
            nodeColor = revealedColor; // Revealed but not visited - white
        }
        
        // Apply color tint to background (grid tile will show through with tint)
        if (bgImage != null)
        {
            // If we have a grid tile sprite, tint it; otherwise use solid color
            if (bgImage.sprite != null)
            {
                bgImage.color = new Color(nodeColor.r, nodeColor.g, nodeColor.b, nodeColor.a * 0.8f); // Slightly transparent to show tile
            }
            else
            {
                bgImage.color = nodeColor; // Solid color if no tile
            }
        }
        
        // Icon uses full color
        if (iconImage != null)
            iconImage.color = nodeColor;
        
        // Set button interactability based on node state
        // btn and buttonTransform should already be set above
        if (btn != null)
        {
            // Button is interactable if node is selectable and not visited
            bool shouldBeInteractable = node.isSelectable && !node.visited;
            btn.interactable = shouldBeInteractable;
            
            // Ensure the button's Image component allows raycasts (for click detection)
            Image buttonImage = buttonTransform.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.raycastTarget = true; // Always allow raycasts for click detection
                
                // CRITICAL: Unity UI buttons need the targetGraphic to have some alpha (even tiny) for click detection
                // If alpha = 0, clicks won't register even with raycastTarget = true
                Color imgColor = buttonImage.color;
                if (imgColor.a == 0f || imgColor.a < 0.01f)
                {
                    buttonImage.color = new Color(imgColor.r, imgColor.g, imgColor.b, 0.01f);
                    Debug.Log($"[MazeMinimapUI] Fixed button Image alpha at {node.position} for click detection");
                }
                
                // Ensure the Button's targetGraphic is set to this Image
                if (btn.targetGraphic != buttonImage)
                {
                    btn.targetGraphic = buttonImage;
                    Debug.Log($"[MazeMinimapUI] Set Button targetGraphic to Image component at {node.position}");
                }
                
                // Ensure Image is enabled
                buttonImage.enabled = true;
            }
            else
            {
                Debug.LogWarning($"[MazeMinimapUI] Button Image component not found at {node.position}!");
            }
            
            // Ensure Button component is enabled
            btn.enabled = true;
            
            Debug.Log($"[MazeMinimapUI] Node at {node.position}: Selectable={node.isSelectable}, Visited={node.visited}, ButtonInteractable={shouldBeInteractable}, ButtonEnabled={btn.enabled}");
        }
    }
    
    /// <summary>
    /// Gets the appropriate icon sprite for a node type.
    /// </summary>
    private Sprite GetNodeIcon(MazeNodeType nodeType)
    {
        return nodeType switch
        {
            MazeNodeType.Start => startNodeIcon,
            MazeNodeType.Combat => combatNodeIcon,
            MazeNodeType.Chest => chestNodeIcon,
            MazeNodeType.Shrine => shrineNodeIcon,
            MazeNodeType.Trap => trapNodeIcon,
            MazeNodeType.Forge => forgeNodeIcon,
            MazeNodeType.Boss => bossNodeIcon,
            MazeNodeType.Stairs => stairsNodeIcon,
            MazeNodeType.Special => specialNodeIcon,
            _ => combatNodeIcon // Default
        };
    }
    
    /// <summary>
    /// Refreshes all node button visuals based on current floor state.
    /// </summary>
    private void RefreshNodeDisplay(MazeFloor floor)
    {
        if (floor == null)
            return;
        
        foreach (var kvp in nodeButtons)
        {
            Vector2Int pos = kvp.Key;
            GameObject buttonObj = kvp.Value;
            MazeNode node = floor.GetNode(pos);
            
            UpdateNodeButtonVisual(buttonObj, node);
        }
    }
    
    /// <summary>
    /// Clears all node buttons from the minimap.
    /// </summary>
    private void ClearMinimap()
    {
        foreach (var kvp in nodeButtons)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value);
            }
        }
        nodeButtons.Clear();
    }
    
    /// <summary>
    /// Updates run info display (floor, seed, modifiers).
    /// </summary>
    private void UpdateRunInfo(MazeRunData run)
    {
        if (run == null)
            return;
        
        // Update floor text
        if (floorText != null)
        {
            floorText.text = $"Floor {run.currentFloor}/{run.totalFloors}";
        }
        
        // Update seed text
        if (seedText != null)
        {
            seedText.text = $"Seed: {run.seed}";
        }
        
        // Update modifiers
        if (modifiersContainer != null && run.runModifiers != null)
        {
            // Clear existing modifier displays
            foreach (Transform child in modifiersContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create modifier displays
            foreach (var modifier in run.runModifiers)
            {
                CreateModifierDisplay(modifier);
            }
        }
    }
    
    /// <summary>
    /// Creates a UI element to display a run modifier.
    /// </summary>
    private void CreateModifierDisplay(MazeRunModifier modifier)
    {
        if (modifiersContainer == null || modifier == null)
            return;
        
        GameObject modifierObj;
        
        if (modifierDisplayPrefab != null)
        {
            modifierObj = Instantiate(modifierDisplayPrefab, modifiersContainer);
        }
        else
        {
            // Create default modifier display
            modifierObj = new GameObject($"Modifier_{modifier.modifierId}");
            modifierObj.transform.SetParent(modifiersContainer, false);
            
            // Add text component
            TextMeshProUGUI text = modifierObj.AddComponent<TextMeshProUGUI>();
            text.text = modifier.displayName;
            text.fontSize = 12f;
            
            RectTransform rect = modifierObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200f, 20f);
        }
        
        // TODO: Set modifier icon/text based on prefab structure
    }
    
    #endregion
    
    #region Button Events
    
    /// <summary>
    /// Called when a node button is clicked.
    /// </summary>
    /// <summary>
    /// Called when a node button is clicked.
    /// Public so MazeNodeButtonDebugger can call it.
    /// </summary>
    public void OnNodeButtonClicked(Vector2Int position)
    {
        Debug.Log($"[MazeMinimapUI] OnNodeButtonClicked called for position {position}");
        
        if (MazeRunManager.Instance == null)
        {
            Debug.LogError("[MazeMinimapUI] MazeRunManager.Instance is null!");
            return;
        }
        
        if (!MazeRunManager.Instance.HasActiveRun())
        {
            Debug.LogWarning("[MazeMinimapUI] No active maze run!");
            return;
        }
        
        var floor = currentFloor;
        if (floor != null)
        {
            var node = floor.GetNode(position);
            if (node != null)
            {
                Debug.Log($"[MazeMinimapUI] Node found: Type={node.nodeType}, Selectable={node.isSelectable}, Visited={node.visited}");
            }
            else
            {
                Debug.LogWarning($"[MazeMinimapUI] No node found at position {position}!");
                return;
            }
        }
        
        Debug.Log($"[MazeMinimapUI] Calling SelectNode for position {position}");
        MazeRunManager.Instance.SelectNode(position);
    }
    
    #endregion
}


using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Visual + tooltip behavior for the intermediate effect nodes on the warrant tree.
/// These nodes are passive (no drag/drop) but should surface whichever modifiers
/// are currently applied to them by nearby sockets.
/// Can be unlocked by clicking (requires adjacent unlocked node + skill point).
/// </summary>
public class WarrantEffectView : WarrantNodeView, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Dependencies")]
    [SerializeField] private WarrantBoardStateController boardState;
    [SerializeField] private WarrantLockerGrid lockerGrid;
    [SerializeField] private ItemTooltipManager tooltipManager;
    [SerializeField] private WarrantBoardRuntimeGraph runtimeGraph;

    [Header("Character Reference")]
    [SerializeField] private Character characterRef;

    [Header("Highlight Objects")]
    [SerializeField] private GameObject highlight1;
    [SerializeField] private GameObject highlight2;
    [SerializeField] private GameObject highlight3;

    private const int MaxSearchDepth = 8;
    private PointerEventData lastPointerEvent;

    private void OnEnable()
    {
        // Refresh character reference when enabled (scene loaded/object activated)
        RefreshCharacterReference();
        // Don't sync lock state here - wait for Start() to ensure board state is loaded
        
        // Subscribe to warrant changes for real-time highlight updates
        SubscribeToWarrantChanges();
    }
    
    private void OnDisable()
    {
        // Unsubscribe from warrant changes
        UnsubscribeFromWarrantChanges();
    }
    
    private void OnDestroy()
    {
        // Clean up subscriptions
        UnsubscribeFromWarrantChanges();
    }
    
    private void Start()
    {
        // Sync lock state after board state controller has loaded (in its Awake)
        // Use a small delay to ensure board state is fully loaded
        SyncLockState();
        
        // Update highlights based on current warrant state
        UpdateHighlights();
    }
    
    private void SubscribeToWarrantChanges()
    {
        if (boardState != null)
        {
            boardState.OnWarrantChanged += UpdateHighlights;
        }
    }
    
    private void UnsubscribeFromWarrantChanges()
    {
        if (boardState != null)
        {
            boardState.OnWarrantChanged -= UpdateHighlights;
        }
    }

    /// <summary>
    /// Syncs the lock state of this effect node based on the board state.
    /// Public so it can be called from WarrantBoardStateController after loading state.
    /// </summary>
    public void SyncLockState()
    {
        if (boardState == null || string.IsNullOrEmpty(NodeId))
        {
            SetLocked(true);
            Debug.LogWarning($"[WarrantEffectView] Cannot sync lock state for '{NodeId}': boardState is null or NodeId is empty");
            UpdateHighlights(); // Update highlights even if state is invalid
            return;
        }

        bool unlocked = boardState.IsNodeUnlocked(NodeId);
        SetLocked(!unlocked);
        
        // Update highlights when lock state changes
        UpdateHighlights();
        
        // Debug logging for first few nodes to track state
        if (NodeId.Contains("effect") && (NodeId.Contains("effect1") || NodeId.Contains("effect2") || NodeId.Contains("effect3")))
        {
            Debug.Log($"[WarrantEffectView] Synced lock state for '{NodeId}': unlocked={unlocked}, locked={!unlocked}");
        }
    }

    public void Configure(WarrantBoardStateController stateController, WarrantLockerGrid grid, ItemTooltipManager tooltip, WarrantBoardRuntimeGraph graph = null)
    {
        // Unsubscribe from old boardState if it exists
        if (boardState != null)
        {
            boardState.OnWarrantChanged -= UpdateHighlights;
        }
        
        boardState = stateController;
        lockerGrid = grid;
        runtimeGraph = graph;
        tooltipManager = tooltip != null ? tooltip : ItemTooltipManager.Instance;
        
        // Refresh character reference (don't cache - always get latest)
        RefreshCharacterReference();
        
        // Sync lock state after configuration (board state should be loaded by now)
        SyncLockState();
        
        // Subscribe to new boardState
        SubscribeToWarrantChanges();
        
        // Update highlights after configuration
        UpdateHighlights();
    }
    
    /// <summary>
    /// Refresh character reference from CharacterManager (always gets latest)
    /// </summary>
    private void RefreshCharacterReference()
    {
        var charManager = CharacterManager.Instance ?? FindFirstObjectByType<CharacterManager>();
        if (charManager != null && charManager.HasCharacter())
        {
            characterRef = charManager.GetCurrentCharacter();
            if (characterRef != null)
            {
                Debug.Log($"[WarrantEffectView] Refreshed character reference: {characterRef.characterName} (Level {characterRef.level}, Skill Points: {characterRef.skillPoints})");
            }
        }
        else
        {
            characterRef = null;
            Debug.LogWarning("[WarrantEffectView] Could not refresh character reference - CharacterManager not found or no character loaded.");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData == null || eventData.button != PointerEventData.InputButton.Left)
            return;

        // Left click: Try to unlock if locked
        if (IsLocked)
        {
            TryUnlockNode();
        }
    }

    private void TryUnlockNode()
    {
        if (boardState == null || runtimeGraph == null || string.IsNullOrEmpty(NodeId))
            return;

        // Always refresh character reference to get latest skill points
        var charManager = CharacterManager.Instance ?? FindFirstObjectByType<CharacterManager>();
        if (charManager == null)
        {
            Debug.LogWarning("[WarrantEffectView] Cannot unlock: CharacterManager not found.");
            return;
        }
        
        if (!charManager.HasCharacter())
        {
            Debug.LogWarning("[WarrantEffectView] Cannot unlock: No character loaded.");
            return;
        }
        
        // Get fresh character reference (don't cache - always get latest)
        Character currentCharacter = charManager.GetCurrentCharacter();
        if (currentCharacter == null)
        {
            Debug.LogWarning("[WarrantEffectView] Cannot unlock: Character is null.");
            return;
        }

        // Get current skill points from character
        int skillPoints = currentCharacter.skillPoints;
        Debug.Log($"[WarrantEffectView] Attempting to unlock {NodeId}. Current skill points: {skillPoints}");
        
        if (boardState.TryUnlockNode(NodeId, runtimeGraph, ref skillPoints))
        {
            // Update character's skill points
            currentCharacter.skillPoints = skillPoints;
            
            // Update cached reference for consistency
            characterRef = currentCharacter;
            
            // Save character
            charManager.SaveCharacter();
            
            SyncLockState();
            Debug.Log($"[WarrantEffectView] ✅ Unlocked node {NodeId}. Remaining skill points: {skillPoints}");
            
            // Refresh character warrant modifiers (unlocking effect nodes affects modifiers)
            if (charManager != null && charManager.HasCharacter())
            {
                charManager.GetCurrentCharacter().RefreshWarrantModifiers();
            }
        }
        else
        {
            if (boardState.IsNodeUnlocked(NodeId))
            {
                Debug.Log($"[WarrantEffectView] Node {NodeId} is already unlocked.");
            }
            else if (skillPoints < 1)
            {
                Debug.LogWarning($"[WarrantEffectView] ❌ Cannot unlock {NodeId}: Not enough skill points (need 1, have {skillPoints}). Character: {currentCharacter.characterName}, Level: {currentCharacter.level}");
            }
            else
            {
                Debug.LogWarning($"[WarrantEffectView] Cannot unlock {NodeId}: No adjacent unlocked nodes found.");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsLocked)
            return;

        lastPointerEvent = eventData;
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipManager?.HideTooltip();
    }

    private void ShowTooltip()
    {
        if (IsLocked)
        {
            tooltipManager?.HideTooltip();
            return;
        }

        var data = BuildTooltipData();
        if (data == null)
        {
            tooltipManager?.HideTooltip();
            return;
        }

        var position = lastPointerEvent != null ? lastPointerEvent.position : (Vector2)Input.mousePosition;
        tooltipManager?.ShowWarrantTooltip(data, position);
    }

    private WarrantTooltipData BuildTooltipData()
    {
        var definitions = GatherAffectingWarrants();
        if (definitions.Count == 0)
            return null;

        string subtitle = $"{definitions.Count} warrant{(definitions.Count == 1 ? string.Empty : "s")} contributing";
        
        // Build title from warrant names instead of node ID
        string title;
        if (definitions.Count == 1)
        {
            // Single warrant: use its display name
            var definition = definitions[0];
            title = string.IsNullOrWhiteSpace(definition.displayName) ? definition.warrantId : definition.displayName;
        }
        else
        {
            // Multiple warrants: join with " | "
            var warrantNames = definitions.Select(d => 
                string.IsNullOrWhiteSpace(d.displayName) ? d.warrantId : d.displayName
            );
            title = string.Join(" | ", warrantNames);
        }
        
        return WarrantTooltipUtility.BuildCombinedData(title, definitions, subtitle);
    }

    private List<WarrantDefinition> GatherAffectingWarrants()
    {
        var result = new List<WarrantDefinition>();
        if (RuntimeNode == null || boardState == null || lockerGrid == null)
            return result;
        
        var sockets = FindSocketsWithinDepth();

        foreach (var kvp in sockets)
        {
            // Only consider unlocked sockets
            if (!boardState.IsNodeUnlocked(kvp.NodeId))
                continue;

            var warrantId = boardState.GetWarrantAtNode(kvp.NodeId);
            if (string.IsNullOrEmpty(warrantId))
                continue;

            var definition = lockerGrid.GetDefinition(warrantId);
            if (definition == null)
                continue;

            if (kvp.Distance > definition.rangeDepth)
                continue;

            if (!result.Contains(definition))
            {
                result.Add(definition);
            }
        }

        UpdateNodeDebugName(result);
        UpdateHighlights(result.Count);
        return result;
    }
    
    /// <summary>
    /// Updates the highlight objects based on the number of warrants affecting this node.
    /// Called automatically when warrants change, or can be called manually.
    /// </summary>
    private void UpdateHighlights()
    {
        if (IsLocked)
        {
            // If locked, hide all highlights
            SetHighlightActive(highlight1, false);
            SetHighlightActive(highlight2, false);
            SetHighlightActive(highlight3, false);
            return;
        }
        
        var definitions = GatherAffectingWarrants();
        UpdateHighlights(definitions.Count);
    }
    
    /// <summary>
    /// Updates the highlight objects based on the number of warrants.
    /// </summary>
    private void UpdateHighlights(int warrantCount)
    {
        if (IsLocked)
        {
            // If locked, hide all highlights
            SetHighlightActive(highlight1, false);
            SetHighlightActive(highlight2, false);
            SetHighlightActive(highlight3, false);
            return;
        }
        
        // 1 warrant: Activate Highlight1
        // 2 warrants: Activate Highlight1 AND Highlight2
        // 2 or more warrants: Activate Highlight1, Highlight2, and Highlight3
        SetHighlightActive(highlight1, warrantCount >= 1);
        SetHighlightActive(highlight2, warrantCount >= 2);
        SetHighlightActive(highlight3, warrantCount >= 2);
    }
    
    /// <summary>
    /// Safely sets the active state of a highlight GameObject.
    /// </summary>
    private void SetHighlightActive(GameObject highlight, bool active)
    {
        if (highlight != null)
        {
            highlight.SetActive(active);
        }
    }

    private void UpdateNodeDebugName(List<WarrantDefinition> definitions)
    {
        if (definitions == null || definitions.Count == 0)
        {
            gameObject.name = $"{NodeId}_Effect";
            return;
        }

        var joined = string.Join(", ", definitions.Select(d => string.IsNullOrWhiteSpace(d.displayName) ? d.warrantId : d.displayName));
        gameObject.name = $"{NodeId}_[{joined}]";
    }

    private IEnumerable<(string NodeId, int Distance)> FindSocketsWithinDepth()
    {
        var result = new List<(string, int)>();
        if (RuntimeNode == null)
            return result;

        var visited = new HashSet<WarrantBoardRuntimeGraph.Node>();
        var queue = new Queue<(WarrantBoardRuntimeGraph.Node node, int depth)>();
        queue.Enqueue((RuntimeNode, 0));
        visited.Add(RuntimeNode);

        while (queue.Count > 0)
        {
            var (node, depth) = queue.Dequeue();
            if (depth > MaxSearchDepth)
                continue;

            if (node != RuntimeNode && (node.NodeType == WarrantNodeType.Socket || node.NodeType == WarrantNodeType.SpecialSocket))
            {
                result.Add((node.Id, depth));
            }

            foreach (var neighbor in node.Connections)
            {
                if (neighbor == null || visited.Contains(neighbor))
                    continue;

                visited.Add(neighbor);
                queue.Enqueue((neighbor, depth + 1));
            }
        }

        return result;
    }
}


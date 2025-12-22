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

    private const int MaxSearchDepth = 8;
    private PointerEventData lastPointerEvent;

    private void OnEnable()
    {
        // Refresh character reference when enabled (scene loaded/object activated)
        RefreshCharacterReference();
        // Don't sync lock state here - wait for Start() to ensure board state is loaded
    }
    
    private void Start()
    {
        // Sync lock state after board state controller has loaded (in its Awake)
        // Use a small delay to ensure board state is fully loaded
        SyncLockState();
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
            return;
        }

        bool unlocked = boardState.IsNodeUnlocked(NodeId);
        SetLocked(!unlocked);
        
        // Debug logging for first few nodes to track state
        if (NodeId.Contains("effect") && (NodeId.Contains("effect1") || NodeId.Contains("effect2") || NodeId.Contains("effect3")))
        {
            Debug.Log($"[WarrantEffectView] Synced lock state for '{NodeId}': unlocked={unlocked}, locked={!unlocked}");
        }
    }

    public void Configure(WarrantBoardStateController stateController, WarrantLockerGrid grid, ItemTooltipManager tooltip, WarrantBoardRuntimeGraph graph = null)
    {
        boardState = stateController;
        lockerGrid = grid;
        runtimeGraph = graph;
        tooltipManager = tooltip != null ? tooltip : ItemTooltipManager.Instance;
        
        // Refresh character reference (don't cache - always get latest)
        RefreshCharacterReference();
        
        // Sync lock state after configuration (board state should be loaded by now)
        SyncLockState();
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
        return WarrantTooltipUtility.BuildCombinedData($"{NodeId} Effects", definitions, subtitle);
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
        return result;
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


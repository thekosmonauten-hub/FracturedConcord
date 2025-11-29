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
        SyncLockState();
    }

    private void SyncLockState()
    {
        if (boardState == null || string.IsNullOrEmpty(NodeId))
        {
            SetLocked(true);
            return;
        }

        bool unlocked = boardState.IsNodeUnlocked(NodeId);
        SetLocked(!unlocked);
    }

    public void Configure(WarrantBoardStateController stateController, WarrantLockerGrid grid, ItemTooltipManager tooltip, WarrantBoardRuntimeGraph graph = null)
    {
        boardState = stateController;
        lockerGrid = grid;
        runtimeGraph = graph;
        tooltipManager = tooltip != null ? tooltip : ItemTooltipManager.Instance;
        
        // Get character reference for skill points
        if (characterRef == null)
        {
            var charManager = FindFirstObjectByType<CharacterManager>();
            characterRef = charManager != null ? charManager.GetCurrentCharacter() : null;
        }
        
        SyncLockState();
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

        if (characterRef == null)
        {
            var charManager = FindFirstObjectByType<CharacterManager>();
            characterRef = charManager != null ? charManager.GetCurrentCharacter() : null;
        }

        if (characterRef == null)
        {
            Debug.LogWarning("[WarrantEffectView] Cannot unlock: No character reference found.");
            return;
        }

        int skillPoints = characterRef.skillPoints;
        if (boardState.TryUnlockNode(NodeId, runtimeGraph, ref skillPoints))
        {
            characterRef.skillPoints = skillPoints;
            
            // Save character if manager exists
            var charManager = FindFirstObjectByType<CharacterManager>();
            charManager?.SaveCharacter();
            
            SyncLockState();
            Debug.Log($"[WarrantEffectView] Unlocked node {NodeId}. Remaining skill points: {skillPoints}");
        }
        else
        {
            if (boardState.IsNodeUnlocked(NodeId))
            {
                Debug.Log($"[WarrantEffectView] Node {NodeId} is already unlocked.");
            }
            else if (skillPoints < 1)
            {
                Debug.LogWarning($"[WarrantEffectView] Cannot unlock {NodeId}: Not enough skill points (need 1, have {skillPoints}).");
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


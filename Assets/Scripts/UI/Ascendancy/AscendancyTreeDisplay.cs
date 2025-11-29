using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Manages the complete Ascendancy tree display:
/// - Central splash art container
/// - Passive nodes arranged around the container
/// - Handles node spawning, positioning, and interactions
/// </summary>
public class AscendancyTreeDisplay : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject containerPrefab; // AscendancyContainerPrefab
    [SerializeField] private GameObject nodePrefab;      // AscendancyNode
    
    [Header("Layout")]
    [SerializeField] private Transform nodesContainer;   // Parent for all nodes
    [SerializeField] private bool useManualPositions = true; // Use treePosition from data
    [SerializeField] private float nodeSpacing = 80f;         // Spacing between nodes (reduced for tighter layout)
    [SerializeField] private float branchSpacing = 150f;     // Spacing between branches
    [SerializeField] private float maxDistanceFromCenter = 300f; // Max distance nodes can be from center (prevents overflow)
    
    [Header("Node Prefabs (Optional Variants)")]
    [Tooltip("Prefab for Start nodes (falls back to nodePrefab if empty)")]
    [SerializeField] private GameObject startNodePrefab;
    [Tooltip("Prefab for Minor nodes (falls back to nodePrefab if empty)")]
    [SerializeField] private GameObject minorNodePrefab;
    [Tooltip("Prefab for Major nodes (falls back to nodePrefab if empty)")]
    [SerializeField] private GameObject majorNodePrefab;
    
    [Header("Connection Lines")]
    [SerializeField] private bool drawConnections = true;
    [SerializeField] private GameObject connectionLinePrefab;
    [SerializeField] private Color connectionColor = Color.white;
    [SerializeField] private float connectionWidth = 3f;
    
    [Header("Tooltip System")]
    [SerializeField] private AscendancyTooltipController tooltipController;
    [SerializeField] private bool enableTooltips = true;
    
    // Public accessor for tooltip controller (used by SubNodeHoverHandler)
    public AscendancyTooltipController TooltipController => tooltipController;
    
    [Header("Optional: Legacy Info Panel")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TextMeshProUGUI passiveNameText;
    [SerializeField] private TextMeshProUGUI passiveDescriptionText;
    [SerializeField] private TextMeshProUGUI passiveCostText;
    [SerializeField] private Button unlockButton;
    
    [Header("Settings")]
    [SerializeField] private bool showDebugLogs = false;
    
    // Runtime data
    private AscendancyData currentAscendancy;
    private CharacterAscendancyProgress progression;
    private AscendancyContainerController containerController;
    private List<AscendancyPassiveNode> spawnedNodes = new List<AscendancyPassiveNode>();
    private AscendancyPassiveNode selectedNode;
    
    void Awake()
    {
        // Setup unlock button
        if (unlockButton != null)
        {
            unlockButton.onClick.AddListener(OnUnlockButtonClicked);
        }
        
        // Hide info panel initially
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Initialize and display an Ascendancy tree
    /// </summary>
    public void DisplayAscendancy(AscendancyData ascendancy, CharacterAscendancyProgress progress)
    {
        if (ascendancy == null)
        {
            Debug.LogError("[AscendancyTreeDisplay] Cannot display null Ascendancy!");
            return;
        }
        
        currentAscendancy = ascendancy;
        progression = progress ?? new CharacterAscendancyProgress();
        
        // Clear existing display
        ClearDisplay();
        
        // Spawn container (splash art)
        SpawnContainer();
        
        // Spawn passive nodes
        SpawnPassiveNodes();
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyTreeDisplay] Displayed {ascendancy.ascendancyName} with {ascendancy.passiveAbilities.Count} passives");
    }
    
    /// <summary>
    /// Spawn the central container with splash art
    /// </summary>
    void SpawnContainer()
    {
        if (containerPrefab == null)
        {
            Debug.LogError("[AscendancyTreeDisplay] Container prefab not assigned!");
            return;
        }
        
        GameObject containerObj = Instantiate(containerPrefab, transform);
        containerObj.name = "AscendancyContainer";
        
        containerController = containerObj.GetComponent<AscendancyContainerController>();
        if (containerController == null)
        {
            containerController = containerObj.AddComponent<AscendancyContainerController>();
        }
        
        containerController.Initialize(currentAscendancy);
    }
    
    /// <summary>
    /// Spawn passive ability nodes
    /// </summary>
    void SpawnPassiveNodes()
    {
        if (nodePrefab == null)
        {
            Debug.LogError("[AscendancyTreeDisplay] Node prefab not assigned!");
            return;
        }
        
        // Get all nodes (from branches or legacy list)
        List<AscendancyPassive> allNodes = currentAscendancy.GetAllNodes();
        
        if (allNodes == null || allNodes.Count == 0)
        {
            Debug.LogWarning($"[AscendancyTreeDisplay] No passive abilities defined for {currentAscendancy.ascendancyName}");
            return;
        }
        
        Transform parent = nodesContainer != null ? nodesContainer : transform;
        
        // Auto-generate tree structure if using branch system
        if (currentAscendancy.useBranchSystem && currentAscendancy.useAutoGeneratedPaths)
        {
            currentAscendancy.GenerateTreeStructure();
        }
        else if (!currentAscendancy.useBranchSystem && currentAscendancy.useAutoGeneratedPaths)
        {
            // Legacy auto-generation
            GenerateTreePositions();
        }
        
        // Spawn each node
        foreach (AscendancyPassive passive in allNodes)
        {
            if (passive == null)
            {
                Debug.LogWarning($"[AscendancyTreeDisplay] Null passive found!");
                continue;
            }
            
            // Get appropriate prefab based on node type
            GameObject prefabToUse = GetNodePrefab(passive.nodeType);
            
            // Spawn node
            GameObject nodeObj = Instantiate(prefabToUse, parent);
            nodeObj.name = $"Node_{passive.nodeType}_{passive.name}";
            
            // Ensure node has RectTransform (required for UI)
            RectTransform rectTransform = nodeObj.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                // If prefab is not a UI element, add RectTransform
                Debug.LogWarning($"[AscendancyTreeDisplay] Node prefab missing RectTransform! Adding it to {nodeObj.name}");
                rectTransform = nodeObj.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(80, 80); // Default size
            }
            
            // Position node
            Vector2 position = passive.treePosition; // Always use treePosition (set by branch or manual)
            
            // Clamp position to prevent overflow (scale down if too far from center)
            float distanceFromCenter = position.magnitude;
            if (distanceFromCenter > maxDistanceFromCenter && maxDistanceFromCenter > 0)
            {
                float scale = maxDistanceFromCenter / distanceFromCenter;
                position *= scale;
                
                if (showDebugLogs)
                    Debug.Log($"[AscendancyTreeDisplay] Clamped {passive.name} from {passive.treePosition} to {position} (scale: {scale:F2})");
            }
            
            if (rectTransform != null)
            {
                // Ensure pivot is centered (for proper positioning and connections)
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = position;
                
                if (showDebugLogs)
                    Debug.Log($"[AscendancyTreeDisplay] Positioned {passive.name} ({passive.nodeType}) at {position}");
                
                // Apply node scale (Major nodes are larger)
                float scale = passive.nodeType == AscendancyNodeType.Major ? 1.3f : 
                             (passive.nodeType == AscendancyNodeType.Start ? 1.5f : 1.0f);
                
                if (passive.nodeScale > 0)
                    scale = passive.nodeScale;
                
                rectTransform.localScale = Vector3.one * scale;
            }
            
            // Initialize node
            AscendancyPassiveNode node = nodeObj.GetComponent<AscendancyPassiveNode>();
            if (node == null)
            {
                node = nodeObj.AddComponent<AscendancyPassiveNode>();
            }
            
            bool isUnlocked = passive.unlockedByDefault || 
                             (progression != null && progression.IsPassiveUnlocked(passive.name));
            node.Initialize(passive, currentAscendancy, isUnlocked);
            
            // Check if available to unlock
            bool isAvailable = CanUnlockPassive(passive);
            node.SetAvailable(isAvailable && !isUnlocked);
            
            // Subscribe to events
            node.OnNodeClicked += OnNodeClicked;
            node.OnNodeHoverEnter += OnNodeHoverEnter;
            node.OnNodeHoverExit += OnNodeHoverExit;
            
            spawnedNodes.Add(node);
            
            // Handle choice nodes: spawn sub-nodes in a circle around the main node
            if (passive.isChoiceNode && passive.subNodes != null && passive.subNodes.Count > 0)
            {
                SpawnChoiceNodeSubNodes(passive, node, parent);
            }
        }
        
        // Draw connection lines between nodes
        if (drawConnections)
        {
            DrawConnectionLines();
        }
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyTreeDisplay] Spawned {spawnedNodes.Count} passive nodes");
    }
    
    /// <summary>
    /// Spawn sub-nodes in a circle around a choice node
    /// </summary>
    void SpawnChoiceNodeSubNodes(AscendancyPassive choiceNode, AscendancyPassiveNode mainNode, Transform parent)
    {
        if (choiceNode == null || mainNode == null || choiceNode.subNodes == null || choiceNode.subNodes.Count == 0)
            return;
        
        // Check if a sub-node is already selected
        int selectedIndex = -1;
        if (progression != null)
        {
            selectedIndex = progression.GetSelectedSubNodeIndex(choiceNode.name);
        }
        
        // If a sub-node is selected, apply its sprite to the main node and hide sub-nodes
        if (selectedIndex >= 0 && selectedIndex < choiceNode.subNodes.Count)
        {
            var selectedSubNode = choiceNode.subNodes[selectedIndex];
            if (selectedSubNode.icon != null)
            {
                // Apply selected sub-node's sprite to main node
                var nodeImage = mainNode.GetComponentInChildren<UnityEngine.UI.Image>();
                if (nodeImage != null)
                {
                    nodeImage.sprite = selectedSubNode.icon;
                }
            }
            // Don't spawn sub-nodes if one is selected
            return;
        }
        
        // Spawn sub-nodes in a circle around the main node
        RectTransform mainRect = mainNode.GetComponent<RectTransform>();
        if (mainRect == null) return;
        
        Vector2 mainPosition = mainRect.anchoredPosition;
        float radius = choiceNode.subNodeRadius > 0 ? choiceNode.subNodeRadius : 100f;
        
        // Calculate angle step for even distribution
        float angleStep = 360f / choiceNode.subNodes.Count;
        
        for (int i = 0; i < choiceNode.subNodes.Count; i++)
        {
            var subNode = choiceNode.subNodes[i];
            if (subNode == null) continue;
            
            // Calculate position in circle
            float angle = subNode.angleOffset > 0 ? subNode.angleOffset : (angleStep * i);
            float angleRad = angle * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * radius;
            Vector2 subNodePosition = mainPosition + offset;
            
            // Spawn sub-node
            GameObject subNodeObj = Instantiate(nodePrefab, parent);
            subNodeObj.name = $"SubNode_{choiceNode.name}_{i}_{subNode.name}";
            
            RectTransform subRect = subNodeObj.GetComponent<RectTransform>();
            if (subRect == null)
            {
                subRect = subNodeObj.AddComponent<RectTransform>();
                subRect.sizeDelta = new Vector2(60, 60); // Smaller than main node
            }
            
            subRect.pivot = new Vector2(0.5f, 0.5f);
            subRect.anchoredPosition = subNodePosition;
            subRect.localScale = Vector3.one * 0.75f; // Smaller scale for sub-nodes
            
            // Set sub-node icon
            var subNodeImage = subNodeObj.GetComponentInChildren<UnityEngine.UI.Image>();
            if (subNodeImage != null && subNode.icon != null)
            {
                subNodeImage.sprite = subNode.icon;
            }
            
            // Add click handler for sub-node selection
            var subNodeButton = subNodeObj.GetComponentInChildren<UnityEngine.UI.Button>();
            if (subNodeButton == null)
            {
                subNodeButton = subNodeObj.AddComponent<UnityEngine.UI.Button>();
            }
            
            int subNodeIndex = i; // Capture for closure
            subNodeButton.onClick.AddListener(() => OnSubNodeClicked(choiceNode, mainNode, subNodeIndex));
            
            // Add hover handlers for tooltip
            var subNodeHoverHandler = subNodeObj.GetComponent<SubNodeHoverHandler>();
            if (subNodeHoverHandler == null)
            {
                subNodeHoverHandler = subNodeObj.AddComponent<SubNodeHoverHandler>();
            }
            subNodeHoverHandler.Initialize(subNode, choiceNode.name, this);
            
            // Make sub-node slightly transparent to distinguish from main node
            if (subNodeImage != null)
            {
                var color = subNodeImage.color;
                color.a = 0.8f;
                subNodeImage.color = color;
            }
        }
    }
    
    /// <summary>
    /// Show tooltip for a sub-node
    /// </summary>
    public void ShowSubNodeTooltip(AscendancySubNode subNode, string choiceNodeName, Vector2 position)
    {
        if (enableTooltips && tooltipController != null)
        {
            tooltipController.ShowSubNodeTooltip(subNode, choiceNodeName, position);
        }
    }
    
    /// <summary>
    /// Handle sub-node click - select it and apply its sprite to main node
    /// </summary>
    void OnSubNodeClicked(AscendancyPassive choiceNode, AscendancyPassiveNode mainNode, int subNodeIndex)
    {
        if (choiceNode == null || mainNode == null || progression == null)
            return;
        
        if (choiceNode.subNodes == null || subNodeIndex < 0 || subNodeIndex >= choiceNode.subNodes.Count)
            return;
        
        var subNode = choiceNode.subNodes[subNodeIndex];
        
        // Select the sub-node
        bool success = progression.SelectSubNode(choiceNode.name, subNodeIndex, currentAscendancy);
        if (success)
        {
            // Apply sub-node's sprite to main node
            var nodeImage = mainNode.GetComponentInChildren<UnityEngine.UI.Image>();
            if (nodeImage != null && subNode.icon != null)
            {
                nodeImage.sprite = subNode.icon;
            }
            
            // Hide all sub-nodes (they'll be hidden on next refresh)
            // For now, just log - you might want to store sub-node references to hide them
            Debug.Log($"[AscendancyTreeDisplay] Selected sub-node {subNodeIndex} ('{subNode.name}') for choice node '{choiceNode.name}'");
            
            // Refresh the display to hide sub-nodes
            // You could also manually hide them here if you store references
        }
    }
    
    /// <summary>
    /// Get the appropriate node prefab based on type
    /// </summary>
    GameObject GetNodePrefab(AscendancyNodeType nodeType)
    {
        switch (nodeType)
        {
            case AscendancyNodeType.Start:
                return startNodePrefab != null ? startNodePrefab : nodePrefab;
            case AscendancyNodeType.Minor:
                return minorNodePrefab != null ? minorNodePrefab : nodePrefab;
            case AscendancyNodeType.Major:
                return majorNodePrefab != null ? majorNodePrefab : nodePrefab;
            default:
                return nodePrefab;
        }
    }
    
    /// <summary>
    /// Auto-generate tree positions for Start -> Minor -> Major pattern
    /// </summary>
    void GenerateTreePositions()
    {
        if (currentAscendancy.passiveAbilities == null) return;
        
        // Find start node
        AscendancyPassive startNode = currentAscendancy.GetStartNode();
        if (startNode != null)
        {
            startNode.treePosition = Vector2.zero; // Center
        }
        
        // Get minor and major nodes
        var minorNodes = currentAscendancy.GetMinorNodes();
        var majorNodes = currentAscendancy.GetMajorNodes();
        
        // Calculate number of branches
        int branches = currentAscendancy.numberOfBranches;
        if (branches <= 0) branches = 2;
        
        int nodesPerBranch = Mathf.CeilToInt((minorNodes.Count + majorNodes.Count) / (float)branches);
        
        // Position nodes in branches
        for (int branchIndex = 0; branchIndex < branches; branchIndex++)
        {
            float branchAngle = (360f / branches) * branchIndex - 90f; // Start from top, go clockwise
            float branchAngleRad = branchAngle * Mathf.Deg2Rad;
            
            Vector2 branchDirection = new Vector2(Mathf.Cos(branchAngleRad), Mathf.Sin(branchAngleRad));
            
            // Alternate Minor -> Major pattern
            int nodeInBranch = 0;
            
            for (int i = 0; i < nodesPerBranch && (branchIndex * nodesPerBranch + i) < (minorNodes.Count + majorNodes.Count); i++)
            {
                AscendancyPassive node = null;
                bool isMinor = (i % 2 == 0); // Alternate: Minor, Major, Minor, Major
                
                if (isMinor && minorNodes.Count > 0)
                {
                    node = minorNodes[0];
                    minorNodes.RemoveAt(0);
                }
                else if (!isMinor && majorNodes.Count > 0)
                {
                    node = majorNodes[0];
                    majorNodes.RemoveAt(0);
                }
                else if (minorNodes.Count > 0)
                {
                    node = minorNodes[0];
                    minorNodes.RemoveAt(0);
                }
                else if (majorNodes.Count > 0)
                {
                    node = majorNodes[0];
                    majorNodes.RemoveAt(0);
                }
                
                if (node != null)
                {
                    float distance = nodeSpacing * (nodeInBranch + 1);
                    node.treePosition = branchDirection * distance;
                    nodeInBranch++;
                }
            }
        }
    }
    
    /// <summary>
    /// Draw connection lines between nodes based on prerequisites
    /// </summary>
    void DrawConnectionLines()
    {
        foreach (var node in spawnedNodes)
        {
            AscendancyPassive passive = node.GetPassiveData();
            
            if (passive.prerequisitePassives == null || passive.prerequisitePassives.Count == 0)
                continue;
            
            foreach (string prereqName in passive.prerequisitePassives)
            {
                // Find prerequisite node
                var prereqNode = spawnedNodes.Find(n => n.GetPassiveData().name == prereqName);
                
                if (prereqNode != null)
                {
                    DrawLineBetweenNodes(prereqNode, node);
                }
            }
        }
    }
    
    /// <summary>
    /// Draw a line between two nodes
    /// </summary>
    void DrawLineBetweenNodes(AscendancyPassiveNode fromNode, AscendancyPassiveNode toNode)
    {
        if (fromNode == null || toNode == null) return;
        
        Transform parent = nodesContainer != null ? nodesContainer : transform;
        
        // Create line GameObject
        GameObject lineObj = new GameObject($"Connection_{fromNode.name}_to_{toNode.name}");
        lineObj.transform.SetParent(parent, false); // worldPositionStays = false
        lineObj.transform.SetAsFirstSibling(); // Behind nodes
        
        RectTransform lineRect = lineObj.AddComponent<RectTransform>();
        lineRect.pivot = new Vector2(0.5f, 0.5f); // Center pivot
        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        
        Image lineImage = lineObj.AddComponent<Image>();
        lineImage.color = connectionColor;
        lineImage.raycastTarget = false;
        
        // Get center positions of nodes (accounting for pivot)
        RectTransform fromRect = fromNode.GetComponent<RectTransform>();
        RectTransform toRect = toNode.GetComponent<RectTransform>();
        
        Vector2 fromPos = fromRect.anchoredPosition;
        Vector2 toPos = toRect.anchoredPosition;
        
        // Calculate line position and rotation
        Vector2 direction = toPos - fromPos;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Position line at midpoint between nodes
        lineRect.anchoredPosition = fromPos + direction * 0.5f;
        lineRect.sizeDelta = new Vector2(distance, connectionWidth);
        lineRect.localRotation = Quaternion.Euler(0, 0, angle);
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyTreeDisplay] Drew line from {fromNode.name} ({fromPos}) to {toNode.name} ({toPos}), distance: {distance:F1}");
    }
    
    /// <summary>
    /// Calculate position for a node (legacy - now uses treePosition from data)
    /// </summary>
    Vector2 CalculateNodePosition(int index, int totalNodes)
    {
        // Fallback circular layout if no manual positions
        float angleStep = 360f / totalNodes;
        float angle = angleStep * index;
        float angleRad = angle * Mathf.Deg2Rad;
        
        float x = Mathf.Cos(angleRad) * nodeSpacing * 2;
        float y = Mathf.Sin(angleRad) * nodeSpacing * 2;
        
        return new Vector2(x, y);
    }
    
    /// <summary>
    /// Check if a passive can be unlocked
    /// </summary>
    bool CanUnlockPassive(AscendancyPassive passive)
    {
        if (progression == null) return false;
        if (progression.IsPassiveUnlocked(passive.name)) return false;
        if (progression.availableAscendancyPoints < passive.pointCost) return false;
        
        // Check prerequisites
        if (passive.prerequisitePassives != null && passive.prerequisitePassives.Count > 0)
        {
            if (passive.requireAllPrerequisites)
            {
                foreach (string prereq in passive.prerequisitePassives)
                {
                    if (!progression.IsPassiveUnlocked(prereq))
                        return false;
                }
            }
            else
            {
                bool anyUnlocked = false;
                foreach (string prereq in passive.prerequisitePassives)
                {
                    if (progression.IsPassiveUnlocked(prereq))
                    {
                        anyUnlocked = true;
                        break;
                    }
                }

                if (!anyUnlocked)
                    return false;
            }
        }
        
        // Check node group (mutually exclusive nodes)
        if (currentAscendancy != null && !string.IsNullOrEmpty(passive.nodeGroup))
        {
            if (currentAscendancy.IsAnyNodeInGroupUnlocked(passive.nodeGroup, progression))
            {
                return false; // Another node in this group is already unlocked
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Handle node click
    /// </summary>
    void OnNodeClicked(AscendancyPassiveNode node)
    {
        selectedNode = node;
        
        AscendancyPassive passive = node.GetPassiveData();
        
        // Check if this is a choice node that needs sub-node selection
        if (passive != null && passive.isChoiceNode && progression != null)
        {
            // If choice node is unlocked but no sub-node selected, show sub-nodes
            if (progression.IsPassiveUnlocked(passive.name))
            {
                int selectedIndex = progression.GetSelectedSubNodeIndex(passive.name);
                if (selectedIndex < 0) // No selection yet
                {
                    // Sub-nodes should already be visible, just show info
                    ShowNodeInfo(node);
                    return;
                }
            }
        }
        
        ShowNodeInfo(node);
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyTreeDisplay] Node clicked: {passive.name}");
    }
    
    /// <summary>
    /// Handle node hover enter
    /// </summary>
    void OnNodeHoverEnter(AscendancyPassiveNode node)
    {
        if (node == null) return;
        
        AscendancyPassive passive = node.GetPassiveData();
        if (passive == null) return;
        
        // Show tooltip (new system)
        if (enableTooltips && tooltipController != null)
        {
            Vector2 nodePosition = node.GetComponent<RectTransform>().anchoredPosition;
            tooltipController.ShowTooltip(passive, nodePosition);
        }
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyTreeDisplay] Hovering: {passive.name}");
    }
    
    /// <summary>
    /// Handle node hover exit
    /// </summary>
    void OnNodeHoverExit(AscendancyPassiveNode node)
    {
        // Hide tooltip
        if (tooltipController != null)
        {
            tooltipController.HideTooltip();
        }
    }
    
    /// <summary>
    /// Show passive info in the panel
    /// </summary>
    void ShowNodeInfo(AscendancyPassiveNode node)
    {
        if (infoPanel == null) return;
        
        AscendancyPassive passive = node.GetPassiveData();
        
        if (passiveNameText != null)
            passiveNameText.text = passive.name;
        
        if (passiveDescriptionText != null)
            passiveDescriptionText.text = passive.description;
        
        if (passiveCostText != null)
        {
            if (node.IsUnlocked())
            {
                passiveCostText.text = "UNLOCKED";
                passiveCostText.color = Color.green;
            }
            else
            {
                passiveCostText.text = $"Cost: {passive.pointCost} Point{(passive.pointCost > 1 ? "s" : "")}";
                passiveCostText.color = node.IsAvailable() ? Color.yellow : Color.gray;
            }
        }
        
        // Update unlock button
        if (unlockButton != null)
        {
            unlockButton.gameObject.SetActive(!node.IsUnlocked() && node.IsAvailable());
        }
        
        infoPanel.SetActive(true);
    }
    
    /// <summary>
    /// Handle unlock button click
    /// </summary>
    void OnUnlockButtonClicked()
    {
        if (selectedNode == null || progression == null) return;
        
        AscendancyPassive passive = selectedNode.GetPassiveData();
        
        // Attempt to unlock
        bool unlocked = progression.UnlockPassive(passive, currentAscendancy);
        
        if (unlocked)
        {
            // Update node visual
            selectedNode.SetUnlocked(true);
            selectedNode.SetAvailable(false);
            
            // Update all other nodes' availability
            UpdateNodesAvailability();
            
            // Refresh info panel
            ShowNodeInfo(selectedNode);
            
            Debug.Log($"<color=green>[AscendancyTreeDisplay] ✓ Unlocked: {passive.name}</color>");
        }
        else
        {
            Debug.LogWarning($"[AscendancyTreeDisplay] Failed to unlock: {passive.name}");
        }
    }
    
    /// <summary>
    /// Update all nodes' availability state
    /// </summary>
    void UpdateNodesAvailability()
    {
        foreach (var node in spawnedNodes)
        {
            if (!node.IsUnlocked())
            {
                bool isAvailable = CanUnlockPassive(node.GetPassiveData());
                node.SetAvailable(isAvailable);
            }
        }
    }
    
    /// <summary>
    /// Clear the display
    /// </summary>
    void ClearDisplay()
    {
        // Destroy all spawned nodes
        foreach (var node in spawnedNodes)
        {
            if (node != null)
                Destroy(node.gameObject);
        }
        spawnedNodes.Clear();
        
        // Destroy container
        if (containerController != null)
        {
            Destroy(containerController.gameObject);
            containerController = null;
        }
        
        // Hide info panel
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
        
        selectedNode = null;
    }
    
    void OnDestroy()
    {
        if (unlockButton != null)
        {
            unlockButton.onClick.RemoveListener(OnUnlockButtonClicked);
        }
    }
}


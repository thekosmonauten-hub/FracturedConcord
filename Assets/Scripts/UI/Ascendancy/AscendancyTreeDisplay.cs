using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
    
    /// <summary>
    /// Public accessor for tooltip controller
    /// </summary>
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
    private bool allowPointAllocation = false;
    
    void Awake()
    {
        // Ensure EventSystem exists (required for UI clicks)
        EnsureEventSystem();
        
        // Auto-find references if not assigned
        AutoFindReferences();
        
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
    /// Ensure EventSystem exists in the scene (required for UI button clicks)
    /// </summary>
    void EnsureEventSystem()
    {
        if (EventSystem.current == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            Debug.Log("[AscendancyTreeDisplay] Created EventSystem - required for UI clicks");
        }
    }
    
    /// <summary>
    /// Auto-find UI references if not assigned in Inspector
    /// </summary>
    void AutoFindReferences()
    {
        // Find info panel - search for multiple common names
        if (infoPanel == null)
        {
            // Search in children first
            Transform[] allChildren = GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allChildren)
            {
                string name = child.gameObject.name;
                if (name.Contains("InfoPanel", System.StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("NodeInfoPanel", System.StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("PassiveInfoPanel", System.StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("InfoPanel", System.StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("NodeInfoPanel", System.StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("PassiveInfoPanel", System.StringComparison.OrdinalIgnoreCase))
                {
                    infoPanel = child.gameObject;
                    Debug.Log($"[AscendancyTreeDisplay] Auto-found infoPanel: {infoPanel.name}");
                    break;
                }
            }
            
            // If still not found, try direct Find
            if (infoPanel == null)
            {
                Transform found = transform.Find("InfoPanel") ?? 
                                transform.Find("NodeInfoPanel") ?? 
                                transform.Find("PassiveInfoPanel") ??
                                transform.Find("AscendancyInfoPanel");
                if (found != null)
                {
                    infoPanel = found.gameObject;
                    Debug.Log($"[AscendancyTreeDisplay] Auto-found infoPanel via Find: {infoPanel.name}");
                }
            }
        }
        
        // Find text components within info panel
        if (infoPanel != null)
        {
            // Find name text - search by multiple patterns
            if (passiveNameText == null)
            {
                // Try exact name matches first
                Transform nameText = infoPanel.transform.Find("NameText") ?? 
                                    infoPanel.transform.Find("PassiveName") ??
                                    infoPanel.transform.Find("Name") ??
                                    infoPanel.transform.Find("NodeName");
                if (nameText != null)
                {
                    passiveNameText = nameText.GetComponent<TextMeshProUGUI>();
                }
                
                // If not found, search by name pattern
                if (passiveNameText == null)
                {
                    TextMeshProUGUI[] texts = infoPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (var text in texts)
                    {
                        string name = text.gameObject.name;
                        if (name.Contains("Name", System.StringComparison.OrdinalIgnoreCase) &&
                            !name.Contains("Panel", System.StringComparison.OrdinalIgnoreCase))
                        {
                            passiveNameText = text;
                            break;
                        }
                    }
                }
                
                if (passiveNameText != null)
                    Debug.Log($"[AscendancyTreeDisplay] Auto-found passiveNameText: {passiveNameText.name}");
            }
            
            // Find description text
            if (passiveDescriptionText == null)
            {
                // Try exact name matches first
                Transform descText = infoPanel.transform.Find("DescriptionText") ?? 
                                    infoPanel.transform.Find("Description") ??
                                    infoPanel.transform.Find("Desc") ??
                                    infoPanel.transform.Find("PassiveDescription");
                if (descText != null)
                {
                    passiveDescriptionText = descText.GetComponent<TextMeshProUGUI>();
                }
                
                // If not found, search by name pattern
                if (passiveDescriptionText == null)
                {
                    TextMeshProUGUI[] texts = infoPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (var text in texts)
                    {
                        if (text != passiveNameText)
                        {
                            string name = text.gameObject.name;
                            if (name.Contains("Description", System.StringComparison.OrdinalIgnoreCase) || 
                                name.Contains("Desc", System.StringComparison.OrdinalIgnoreCase))
                            {
                                passiveDescriptionText = text;
                                break;
                            }
                        }
                    }
                }
                
                if (passiveDescriptionText != null)
                    Debug.Log($"[AscendancyTreeDisplay] Auto-found passiveDescriptionText: {passiveDescriptionText.name}");
            }
            
            // Find cost text
            if (passiveCostText == null)
            {
                // Try exact name matches first
                Transform costText = infoPanel.transform.Find("CostText") ?? 
                                    infoPanel.transform.Find("Cost") ??
                                    infoPanel.transform.Find("PointCost") ??
                                    infoPanel.transform.Find("PointsText");
                if (costText != null)
                {
                    passiveCostText = costText.GetComponent<TextMeshProUGUI>();
                }
                
                // If not found, search by name pattern
                if (passiveCostText == null)
                {
                    TextMeshProUGUI[] texts = infoPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (var text in texts)
                    {
                        if (text != passiveNameText && text != passiveDescriptionText)
                        {
                            string name = text.gameObject.name;
                            if (name.Contains("Cost", System.StringComparison.OrdinalIgnoreCase) || 
                                name.Contains("Point", System.StringComparison.OrdinalIgnoreCase))
                            {
                                passiveCostText = text;
                                break;
                            }
                        }
                    }
                }
                
                if (passiveCostText != null)
                    Debug.Log($"[AscendancyTreeDisplay] Auto-found passiveCostText: {passiveCostText.name}");
            }
        }
        
        // Find unlock button - search for buttons with "Unlock" in name
        if (unlockButton == null)
        {
            // Look in info panel first
            if (infoPanel != null)
            {
                Button[] panelButtons = infoPanel.GetComponentsInChildren<Button>(true);
                foreach (var btn in panelButtons)
                {
                    if (btn.gameObject.name.Contains("Unlock", System.StringComparison.OrdinalIgnoreCase))
                    {
                        unlockButton = btn;
                        break;
                    }
                }
                
                // If not found by name, just get first button in panel
                if (unlockButton == null && panelButtons.Length > 0)
                {
                    unlockButton = panelButtons[0];
                }
            }
            
            // If not found, search in all children
            if (unlockButton == null)
            {
                Button[] allButtons = GetComponentsInChildren<Button>(true);
                foreach (var btn in allButtons)
                {
                    string name = btn.gameObject.name;
                    if (name.Contains("Unlock", System.StringComparison.OrdinalIgnoreCase) || 
                        name.Contains("unlock", System.StringComparison.OrdinalIgnoreCase))
                    {
                        unlockButton = btn;
                        break;
                    }
                }
            }
            
            if (unlockButton != null)
            {
                Debug.Log($"[AscendancyTreeDisplay] Auto-found unlockButton: {unlockButton.name}");
            }
            else
            {
                Debug.LogWarning("[AscendancyTreeDisplay] Could not find unlock button! Unlocking will not work.");
            }
        }
    }
    
    /// <summary>
    /// Initialize and display an Ascendancy tree
    /// </summary>
    public void DisplayAscendancy(AscendancyData ascendancy, CharacterAscendancyProgress progress, bool allowAllocation = false)
    {
        if (ascendancy == null)
        {
            Debug.LogError("[AscendancyTreeDisplay] Cannot display null Ascendancy!");
            return;
        }
        
        currentAscendancy = ascendancy;
        progression = progress ?? new CharacterAscendancyProgress();
        allowPointAllocation = allowAllocation;
        
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
            
            // Check if available to unlock (only if allocation is allowed)
            bool isAvailable = allowPointAllocation && CanUnlockPassive(passive);
            
            // Initialize node (this sets up button and image)
            node.Initialize(passive, currentAscendancy, isUnlocked);
            
            // Set availability after initialization (this updates visual state)
            node.SetAvailable(isAvailable && !isUnlocked);
            
            // Force visual state update to ensure colors are correct
            // The node should already be set up, but verify button and image are accessible
            Button nodeButton = node.GetComponentInChildren<Button>(true);
            Image nodeImage = node.GetComponentInChildren<Image>(true);
            
            if (nodeButton != null && nodeImage != null)
            {
                // Ensure image has raycastTarget
                nodeImage.raycastTarget = true;
                
                // Set interactable based on allocation permission and state
                bool shouldBeInteractable = allowPointAllocation && (isUnlocked || isAvailable);
                nodeButton.interactable = shouldBeInteractable;
                
                // Update button's target graphic
                nodeButton.targetGraphic = nodeImage;
                
                if (showDebugLogs)
                    Debug.Log($"[AscendancyTreeDisplay] Node {passive.name}: Unlocked={isUnlocked}, Available={isAvailable}, Interactable={shouldBeInteractable}");
            }
            else
            {
                Debug.LogWarning($"[AscendancyTreeDisplay] Could not find Button or Image for node {passive.name}!");
            }
            
            // Subscribe to events
            node.OnNodeClicked += OnNodeClicked;
            node.OnNodeHoverEnter += OnNodeHoverEnter;
            node.OnNodeHoverExit += OnNodeHoverExit;
            
            spawnedNodes.Add(node);
            
            // If this is a choice node, spawn subnodes if available/unlocked and no selection made
            if (passive.isChoiceNode && progression != null)
            {
                int selectedIndex = progression.GetSelectedSubNodeIndex(passive.name);
                if (selectedIndex < 0) // No selection yet
                {
                    // Spawn subnodes if node is available OR unlocked
                    if (isAvailable || isUnlocked)
                    {
                        SpawnSubNodesForChoiceNode(node, passive);
                    }
                }
                else
                {
                    // Subnode already selected - update choice node visual
                    UpdateChoiceNodeVisual(node, passive, selectedIndex);
                }
            }
        }
        
        // Draw connection lines between nodes
        if (drawConnections)
        {
            DrawConnectionLines();
        }
        
        // Update all nodes' availability after spawning (ensures prerequisites are checked correctly)
        UpdateNodesAvailability();
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyTreeDisplay] Spawned {spawnedNodes.Count} passive nodes and updated availability");
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
    /// Check if a prerequisite passive is unlocked (accounts for unlockedByDefault and choice node subnode selection)
    /// </summary>
    bool IsPrerequisiteUnlocked(string prereqName)
    {
        if (currentAscendancy == null) return false;
        
        // Find the prerequisite passive in the ascendancy data
        AscendancyPassive prereqPassive = currentAscendancy.FindPassiveByName(prereqName);
        if (prereqPassive == null) return false;
        
        // Check if it's unlocked by default (start nodes)
        bool isUnlocked = prereqPassive.unlockedByDefault;
        
        // Check progression
        if (!isUnlocked && progression != null)
        {
            isUnlocked = progression.IsPassiveUnlocked(prereqName);
        }
        
        // If prerequisite is a choice node, it must have a selected subnode to be considered "complete"
        if (isUnlocked && prereqPassive.isChoiceNode && progression != null)
        {
            int selectedIndex = progression.GetSelectedSubNodeIndex(prereqName);
            if (selectedIndex < 0) // No subnode selected yet
            {
                if (showDebugLogs)
                    Debug.Log($"[AscendancyTreeDisplay] Prerequisite choice node '{prereqName}' is unlocked but has no subnode selected - prerequisite not met");
                return false; // Choice node is unlocked but no subnode selected - prerequisite not met
            }
            else
            {
                if (showDebugLogs)
                    Debug.Log($"[AscendancyTreeDisplay] Prerequisite choice node '{prereqName}' is unlocked and has subnode {selectedIndex} selected - prerequisite met");
            }
        }
        
        return isUnlocked;
    }
    
    /// <summary>
    /// Check if a passive can be unlocked
    /// </summary>
    bool CanUnlockPassive(AscendancyPassive passive)
    {
        if (progression == null) return false;
        if (progression.IsPassiveUnlocked(passive.name)) return false;
        
        // Check if already unlocked by default
        if (passive.unlockedByDefault) return false;
        
        if (progression.availableAscendancyPoints < passive.pointCost) return false;
        
        // Check prerequisites (accounting for unlockedByDefault)
        if (passive.prerequisitePassives != null && passive.prerequisitePassives.Count > 0)
        {
            if (passive.requireAllPrerequisites)
            {
                foreach (string prereq in passive.prerequisitePassives)
                {
                    if (!IsPrerequisiteUnlocked(prereq))
                    {
                        if (showDebugLogs)
                            Debug.Log($"[AscendancyTreeDisplay] Prerequisite '{prereq}' not unlocked for '{passive.name}'");
                        return false;
                    }
                }
            }
            else
            {
                bool anyUnlocked = false;
                foreach (string prereq in passive.prerequisitePassives)
                {
                    if (IsPrerequisiteUnlocked(prereq))
                    {
                        anyUnlocked = true;
                        break;
                    }
                }

                if (!anyUnlocked)
                {
                    if (showDebugLogs)
                        Debug.Log($"[AscendancyTreeDisplay] No prerequisites unlocked for '{passive.name}'");
                    return false;
                }
            }
        }
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyTreeDisplay] '{passive.name}' can be unlocked");
        return true;
    }
    
    /// <summary>
    /// Handle node click
    /// </summary>
    void OnNodeClicked(AscendancyPassiveNode node)
    {
        if (node == null) return;
        
        selectedNode = node;
        AscendancyPassive passive = node.GetPassiveData();
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyTreeDisplay] Node clicked: {passive?.name ?? "NULL"} (Unlocked: {node.IsUnlocked()}, Available: {node.IsAvailable()}, AllowAllocation: {allowPointAllocation})");
        
        // If node is available and allocation is allowed, unlock it directly
        if (allowPointAllocation && !node.IsUnlocked() && node.IsAvailable())
        {
            // Attempt to unlock directly
            if (progression != null && passive != null)
            {
                bool unlocked = progression.UnlockPassive(passive, currentAscendancy);
                
                if (unlocked)
                {
                    // Update node visual
                    node.SetUnlocked(true);
                    node.SetAvailable(false);
                    
                    // If this is a choice node, spawn subnodes if not already selected
                    if (passive.isChoiceNode)
                    {
                        int selectedIndex = progression.GetSelectedSubNodeIndex(passive.name);
                        if (selectedIndex < 0) // No selection yet, spawn subnodes
                        {
                            SpawnSubNodesForChoiceNode(node, passive);
                        }
                    }
                    
                    // Update all other nodes' availability
                    UpdateNodesAvailability();
                    
                    Debug.Log($"<color=green>[AscendancyTreeDisplay] ✓ Unlocked via click: {passive.name}</color>");
                    
                    // Still show info panel to confirm
                    ShowNodeInfo(node);
                    return;
                }
                else
                {
                    Debug.LogWarning($"[AscendancyTreeDisplay] Failed to unlock via click: {passive.name}");
                }
            }
        }
        
        // If node is unlocked and is a choice node, check if subnodes need to be spawned
        if (node.IsUnlocked() && passive != null && passive.isChoiceNode)
        {
            int selectedIndex = progression != null ? progression.GetSelectedSubNodeIndex(passive.name) : -1;
            if (selectedIndex < 0) // No selection yet, spawn subnodes
            {
                // Check if subnodes already exist
                SubNodeHoverHandler[] existingSubNodes = node.GetComponentsInChildren<SubNodeHoverHandler>(true);
                if (existingSubNodes == null || existingSubNodes.Length == 0)
                {
                    SpawnSubNodesForChoiceNode(node, passive);
                }
            }
        }
        
        // Default: Show node info
        ShowNodeInfo(node);
    }
    
    /// <summary>
    /// Handle node hover enter
    /// </summary>
    void OnNodeHoverEnter(AscendancyPassiveNode node)
    {
        if (node == null) return;
        
        // Check if we're hovering over a subnode - if so, don't show parent tooltip
        // Subnodes will show their own tooltips via SubNodeHoverHandler
        if (IsHoveringOverSubNode(node))
        {
            if (showDebugLogs)
                Debug.Log("[AscendancyTreeDisplay] Hovering over subnode - skipping parent tooltip");
            return;
        }
        
        AscendancyPassive passive = node.GetPassiveData();
        if (passive == null) return;
        
        // Show tooltip (new system)
        if (enableTooltips && tooltipController != null)
        {
            Vector2 nodePosition = node.GetComponent<RectTransform>().anchoredPosition;
            // Use effective description for tooltip
            string effectiveDescription = node.GetEffectiveDescription();
            if (!string.IsNullOrEmpty(effectiveDescription))
            {
                // Temporarily override description for tooltip
                string originalDesc = passive.description;
                passive.description = effectiveDescription;
                tooltipController.ShowTooltip(passive, nodePosition);
                passive.description = originalDesc; // Restore
            }
            else
            {
                tooltipController.ShowTooltip(passive, nodePosition);
            }
        }
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyTreeDisplay] Hovering: {passive.name}");
    }
    
    /// <summary>
    /// Check if the mouse is currently hovering over a subnode (prevents parent tooltip from showing)
    /// </summary>
    bool IsHoveringOverSubNode(AscendancyPassiveNode parentNode)
    {
        if (parentNode == null) return false;
        
        // Check if there are any active subnodes under this node
        SubNodeHoverHandler[] subNodeHandlers = parentNode.GetComponentsInChildren<SubNodeHoverHandler>(false); // Only active ones
        if (subNodeHandlers == null || subNodeHandlers.Length == 0) return false;
        
        // Check if mouse is over any subnode using EventSystem
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;
            
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            
            foreach (var result in results)
            {
                // Check if the hit object is a subnode
                SubNodeHoverHandler hitSubNode = result.gameObject.GetComponent<SubNodeHoverHandler>();
                if (hitSubNode != null)
                {
                    // Check if this subnode belongs to the parent node
                    foreach (var handler in subNodeHandlers)
                    {
                        if (handler == hitSubNode)
                        {
                            return true; // Mouse is over a subnode
                        }
                    }
                }
            }
        }
        
        return false;
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
        if (node == null)
        {
            Debug.LogWarning("[AscendancyTreeDisplay] Cannot show info for null node");
            return;
        }
        
        // Try to auto-find references if panel is null
        if (infoPanel == null)
        {
            AutoFindReferences();
        }
        
        if (infoPanel == null)
        {
            if (showDebugLogs)
                Debug.LogWarning("[AscendancyTreeDisplay] InfoPanel is null - cannot show node info");
            return;
        }
        
        AscendancyPassive passive = node.GetPassiveData();
        if (passive == null)
        {
            Debug.LogWarning("[AscendancyTreeDisplay] Node has no passive data");
            return;
        }
        
        if (passiveNameText != null)
            passiveNameText.text = passive.name;
        
        if (passiveDescriptionText != null)
        {
            // Use effective description (includes subnode override if selected)
            string description = node.GetEffectiveDescription();
            if (string.IsNullOrEmpty(description))
                description = passive.description;
            passiveDescriptionText.text = description;
        }
        
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
            bool canUnlock = !node.IsUnlocked() && node.IsAvailable() && allowPointAllocation;
            unlockButton.gameObject.SetActive(canUnlock);
            unlockButton.interactable = canUnlock;
        }
        
        infoPanel.SetActive(true);
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyTreeDisplay] Showing info for: {passive.name}");
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
            
            // If this is a choice node, spawn subnodes if not already selected
            if (passive.isChoiceNode)
            {
                int selectedIndex = progression.GetSelectedSubNodeIndex(passive.name);
                if (selectedIndex < 0) // No selection yet, spawn subnodes
                {
                    SpawnSubNodesForChoiceNode(selectedNode, passive);
                }
            }
            
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
    /// Spawn subnodes around a choice node
    /// </summary>
    void SpawnSubNodesForChoiceNode(AscendancyPassiveNode choiceNode, AscendancyPassive choicePassive)
    {
        if (choiceNode == null || choicePassive == null || !choicePassive.isChoiceNode) return;
        if (choicePassive.subNodes == null || choicePassive.subNodes.Count == 0) return;
        
        Transform parent = choiceNode.transform;
        RectTransform choiceRect = choiceNode.GetComponent<RectTransform>();
        if (choiceRect == null) return;
        
        float radius = choicePassive.subNodeRadius > 0 ? choicePassive.subNodeRadius : 100f;
        int subNodeCount = choicePassive.subNodes.Count;
        
        for (int i = 0; i < subNodeCount; i++)
        {
            AscendancySubNode subNode = choicePassive.subNodes[i];
            if (subNode == null) continue;
            
            // Calculate position around choice node
            float angle = (360f / subNodeCount) * i + subNode.angleOffset;
            float angleRad = angle * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * radius;
            
            // Create subnode GameObject
            GameObject subNodeObj = new GameObject($"SubNode_{subNode.name}");
            subNodeObj.transform.SetParent(parent, false);
            
            RectTransform subRect = subNodeObj.AddComponent<RectTransform>();
            subRect.anchoredPosition = offset;
            subRect.sizeDelta = new Vector2(60, 60); // Smaller than main node
            subRect.pivot = new Vector2(0.5f, 0.5f);
            
            // Add image for visual
            Image subImage = subNodeObj.AddComponent<Image>();
            if (subNode.icon != null)
            {
                subImage.sprite = subNode.icon;
            }
            subImage.raycastTarget = true;
            
            // Add SubNodeHoverHandler
            SubNodeHoverHandler handler = subNodeObj.AddComponent<SubNodeHoverHandler>();
            handler.Initialize(subNode, choicePassive.name, this, i);
            
            if (showDebugLogs)
                Debug.Log($"[AscendancyTreeDisplay] Spawned subnode '{subNode.name}' at angle {angle}° for choice node '{choicePassive.name}'");
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
    
    /// <summary>
    /// Show tooltip for a sub-node (used by SubNodeHoverHandler)
    /// </summary>
    public void ShowSubNodeTooltip(AscendancySubNode subNode, string choiceNodeName, Vector2 nodePosition)
    {
        if (tooltipController != null && enableTooltips)
        {
            tooltipController.ShowSubNodeTooltip(subNode, choiceNodeName, nodePosition);
        }
    }
    
    /// <summary>
    /// Get progression (for SubNodeHoverHandler)
    /// </summary>
    public CharacterAscendancyProgress GetProgression()
    {
        return progression;
    }
    
    /// <summary>
    /// Get current ascendancy (for SubNodeHoverHandler)
    /// </summary>
    public AscendancyData GetCurrentAscendancy()
    {
        return currentAscendancy;
    }
    
    /// <summary>
    /// Update choice node visual to show selected subnode's icon and description
    /// </summary>
    void UpdateChoiceNodeVisual(AscendancyPassiveNode choiceNode, AscendancyPassive choicePassive, int selectedIndex)
    {
        if (choiceNode == null || choicePassive == null || !choicePassive.isChoiceNode) return;
        if (choicePassive.subNodes == null || selectedIndex < 0 || selectedIndex >= choicePassive.subNodes.Count) return;
        
        AscendancySubNode selectedSubNode = choicePassive.subNodes[selectedIndex];
        if (selectedSubNode == null) return;
        
        // Update icon
        if (selectedSubNode.icon != null)
        {
            choiceNode.UpdateIcon(selectedSubNode.icon);
        }
        
        // Update description override
        if (!string.IsNullOrEmpty(selectedSubNode.description))
        {
            choiceNode.UpdateDescription(selectedSubNode.description);
        }
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyTreeDisplay] Updated choice node '{choicePassive.name}' visual: icon={selectedSubNode.icon != null}, description={selectedSubNode.description}");
    }
    
    /// <summary>
    /// Refresh choice node display after subnode selection (hide subnodes, show selected)
    /// </summary>
    public void RefreshChoiceNodeDisplay(string choiceNodeName)
    {
        if (progression == null || currentAscendancy == null) return;
        
        // Find the choice node
        var choiceNode = spawnedNodes.Find(n => n.GetPassiveData() != null && n.GetPassiveData().name == choiceNodeName);
        if (choiceNode == null) return;
        
        AscendancyPassive passive = choiceNode.GetPassiveData();
        if (passive == null || !passive.isChoiceNode) return;
        
        // Get selected subnode index
        int selectedIndex = progression.GetSelectedSubNodeIndex(choiceNodeName);
        
        if (showDebugLogs)
            Debug.Log($"[AscendancyTreeDisplay] Refreshing choice node '{choiceNodeName}' - selected index: {selectedIndex}");
        
        // Hide/show subnodes based on selection
        Transform choiceNodeTransform = choiceNode.transform;
        SubNodeHoverHandler[] subNodeHandlers = choiceNodeTransform.GetComponentsInChildren<SubNodeHoverHandler>(true);
        
        foreach (var handler in subNodeHandlers)
        {
            if (handler != null)
            {
                // Hide all subnodes if one is selected, or show all if none selected
                handler.gameObject.SetActive(selectedIndex < 0); // Show if no selection, hide if selected
            }
        }
        
        // Update choice node visual to show selected subnode
        if (selectedIndex >= 0)
        {
            UpdateChoiceNodeVisual(choiceNode, passive, selectedIndex);
        }
        
        // Update availability of all nodes (since a prerequisite choice node now has a selected subnode)
        UpdateNodesAvailability();
    }
    
    void OnDestroy()
    {
        if (unlockButton != null)
        {
            unlockButton.onClick.RemoveListener(OnUnlockButtonClicked);
        }
    }
}


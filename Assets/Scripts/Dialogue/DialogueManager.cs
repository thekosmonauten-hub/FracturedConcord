using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager _instance;
    public static DialogueManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DialogueManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("DialogueManager");
                    _instance = go.AddComponent<DialogueManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    [Header("UI References")]
    [SerializeField] private DialogueUI dialogueUI;
    
    [Header("Warrant System References")]
    [Tooltip("Database containing all warrant packs (e.g., warrant_starter_pack). Can be assigned in Inspector or will auto-find.")]
    [SerializeField] private WarrantPackDatabase warrantPackDatabase;
    
    [Tooltip("Database containing warrant definitions and affix data. Can be assigned in Inspector or will auto-find.")]
    [SerializeField] private WarrantDatabase warrantDatabase;
    
    [Tooltip("The WarrantLockerGrid where warrants are stored. Can be assigned in Inspector or will auto-find. " +
             "Recommended: Assign this reference (e.g., TownCanvas/Menus/PeacekeepersFactionPanel/WarrantFusionPanel/WarrantLockerGrid)")]
    [SerializeField] private WarrantLockerGrid warrantLockerGrid;
    
    [Header("Shop System References")]
    [Tooltip("Database mapping shop IDs to their UI panels. Can be assigned in Inspector or will auto-find in Resources.")]
    [SerializeField] private ShopDatabase shopDatabase;
    
    [Header("Current Dialogue State")]
    private DialogueData currentDialogue;
    public DialogueData CurrentDialogue => currentDialogue;
    private DialogueNode currentNode;
    private Stack<DialogueNode> dialogueHistory = new Stack<DialogueNode>();
    private NPCInteractable currentNPCInteractable; // Store reference to NPC that started dialogue
    
    // Paragraph management (stored here to persist across DialogueUI instances)
    public List<string> currentParagraphs = new List<string>();
    public int currentParagraphIndex = 0;
    
    // Public property to access currentNode
    public DialogueNode CurrentNode => currentNode;
    
    // Track which dialogue nodes have been seen (for conditional unlocks)
    private HashSet<string> seenNodeIds = new HashSet<string>();
    
    // Tutorial state for cross-scene persistence
    private string pendingTutorialId;
    private int pendingTutorialStepIndex;
    private string pendingTutorialIdAfterTransition;
    
    // Public property for dialogue history access
    public Stack<DialogueNode> DialogueHistory => dialogueHistory;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            // DontDestroyOnLoad only works on root GameObjects
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.LogWarning($"[DialogueManager] Cannot use DontDestroyOnLoad on {gameObject.name} - it's not a root GameObject. Parent: {transform.parent.name}");
                // Move to root to make it persistent
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        FindDialogueUI();
    }
    
    /// <summary>
    /// Find DialogueUI component in the scene (searches active and inactive GameObjects)
    /// </summary>
    private void FindDialogueUI()
    {
        if (dialogueUI != null) return;
        
        // First try: Find in active GameObjects
        dialogueUI = FindFirstObjectByType<DialogueUI>();
        
        // Second try: Find in inactive GameObjects (in case DialoguePanel is inactive)
        if (dialogueUI == null)
        {
            DialogueUI[] allDialogueUIs = FindObjectsByType<DialogueUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (allDialogueUIs != null && allDialogueUIs.Length > 0)
            {
                dialogueUI = allDialogueUIs[0];
                Debug.Log($"[DialogueManager] Found DialogueUI in inactive GameObject: {dialogueUI.gameObject.name}");
            }
        }
        
        if (dialogueUI != null)
        {
            Debug.Log($"[DialogueManager] Found DialogueUI: {dialogueUI.gameObject.name} (scene: {dialogueUI.gameObject.scene.name})");
        }
        else
        {
            Debug.LogWarning("[DialogueManager] DialogueUI not found! Will search again when dialogue starts.");
        }
    }
    
    /// <summary>
    /// Start a dialogue conversation
    /// </summary>
    public void StartDialogue(DialogueData dialogue, NPCInteractable npcInteractable = null)
    {
        if (dialogue == null)
        {
            Debug.LogWarning("[DialogueManager] Cannot start null dialogue.");
            return;
        }
        
        currentDialogue = dialogue;
        currentNPCInteractable = npcInteractable; // Store reference to NPC
        dialogueHistory.Clear();
        
        // Note: seenNodeIds persists across dialogue sessions for the same NPC
        // If you want to reset per dialogue, uncomment the line below:
        // seenNodeIds.Clear();
        
        // Special handling for Seer dialogue: Check if midpoint has been seen
        // If so, skip the initial paragraphs and go directly to midpoint
        if (dialogue != null && (dialogue.npcId == "seer" || dialogue.dialogueName == "The Seer"))
        {
            DialogueNode midpointNode = dialogue.nodes?.Find(n => n != null && n.nodeId == "midpoint");
            if (midpointNode != null && seenNodeIds.Contains("midpoint"))
            {
                Debug.Log("[DialogueManager] Seer dialogue: Midpoint already seen, starting at midpoint");
                ShowNode(midpointNode);
                return;
            }
        }
        
        // Try to find an alternative start node based on conditions (e.g., tutorial completion)
        DialogueNode startNode = GetConditionalStartNode(dialogue);
        
        // Fall back to default start node if no conditional node found
        if (startNode == null)
        {
            startNode = dialogue.GetStartNode();
            
            // If default start node has a condition that's not met, look for fallback start nodes
            if (startNode != null && startNode.condition != null && 
                startNode.condition.conditionType != DialogueCondition.ConditionType.None)
            {
                if (!EvaluateCondition(startNode.condition))
                {
                    // Default start condition failed, look for fallback (e.g., "start_no_tutorial")
                    DialogueNode fallbackNode = dialogue.nodes?.Find(n => 
                        n != null && 
                        n.nodeId != null && 
                        n.nodeId.StartsWith("start_") && 
                        n.nodeId != dialogue.startNodeId &&
                        (n.condition == null || n.condition.conditionType == DialogueCondition.ConditionType.None));
                    
                    if (fallbackNode != null)
                    {
                        Debug.Log($"[DialogueManager] Default start node condition not met, using fallback: '{fallbackNode.nodeId}'");
                        startNode = fallbackNode;
                    }
                }
            }
        }
        
        if (startNode == null)
        {
            Debug.LogError($"[DialogueManager] Dialogue '{dialogue.dialogueId}' has no start node!");
            return;
        }
        
        ShowNode(startNode);
    }
    
    /// <summary>
    /// Show a specific dialogue node
    /// </summary>
    public void ShowNode(DialogueNode node)
    {
        if (node == null)
            return;
        
        // Check condition
        if (!EvaluateCondition(node.condition))
        {
            Debug.LogWarning($"[DialogueManager] Node '{node.nodeId}' condition not met. Checking for fallback...");
            
            // If this is a start node that failed, look for fallback
            if (currentDialogue != null && node.nodeId == currentDialogue.startNodeId)
            {
                DialogueNode fallbackNode = currentDialogue.nodes?.Find(n => 
                    n != null && 
                    n.nodeId != null && 
                    n.nodeId.StartsWith("start_") && 
                    n.nodeId != currentDialogue.startNodeId &&
                    (n.condition == null || n.condition.conditionType == DialogueCondition.ConditionType.None));
                
                if (fallbackNode != null)
                {
                    Debug.Log($"[DialogueManager] Using fallback start node: '{fallbackNode.nodeId}'");
                    node = fallbackNode;
                }
                else
                {
                    Debug.LogWarning($"[DialogueManager] No fallback node found for '{node.nodeId}'. Skipping.");
                    return;
                }
            }
            else
            {
                Debug.LogWarning($"[DialogueManager] Node '{node.nodeId}' condition not met. Skipping.");
                return;
            }
        }
        
        currentNode = node;
        // Reset paragraph index when showing a new node
        currentParagraphIndex = 0;
        
        // Mark this node as seen
        if (!string.IsNullOrEmpty(node.nodeId))
        {
            seenNodeIds.Add(node.nodeId);
        }
        
        // Execute onEnter action(s)
        if (node.onEnterActions != null && node.onEnterActions.Count > 0)
        {
            Debug.Log($"[DialogueManager] Executing {node.onEnterActions.Count} onEnterActions for node '{node.nodeId}'");
            // Execute list of actions
            foreach (var action in node.onEnterActions)
            {
                if (action != null)
                {
                    Debug.Log($"[DialogueManager] Executing onEnterAction: {action.actionType}, value: '{action.actionValue}'");
                    ExecuteAction(action);
                }
            }
        }
        else if (node.onEnterAction != null)
        {
            Debug.Log($"[DialogueManager] Executing onEnterAction: {node.onEnterAction.actionType}, value: '{node.onEnterAction.actionValue}'");
            // Execute single action (backward compatibility)
            ExecuteAction(node.onEnterAction);
        }
        
        // Show in UI - ensure DialogueUI is found before displaying
        if (dialogueUI == null)
        {
            FindDialogueUI();
        }
        
        if (dialogueUI != null)
        {
            dialogueUI.DisplayNode(node);
        }
        else
        {
            Debug.LogError("[DialogueManager] DialogueUI not found! Cannot display dialogue. Please ensure DialogueUI component exists in the scene (can be on DialoguePanel in DialogueCanvas).");
        }
    }
    
    /// <summary>
    /// Select a choice and advance dialogue
    /// </summary>
    public void SelectChoice(int choiceIndex)
    {
        Debug.Log($"[DialogueManager] SelectChoice called with index: {choiceIndex}");
        Debug.Log($"[DialogueManager] currentNode is null: {currentNode == null}");
        
        if (currentNode == null)
        {
            Debug.LogError("[DialogueManager] Cannot select choice: currentNode is null!");
            return;
        }
        
        Debug.Log($"[DialogueManager] currentNode.choices is null: {currentNode.choices == null}");
        if (currentNode.choices == null)
        {
            Debug.LogError("[DialogueManager] Cannot select choice: currentNode.choices is null!");
            return;
        }
        
        Debug.Log($"[DialogueManager] currentNode.choices.Count: {currentNode.choices.Count}");
        if (choiceIndex < 0 || choiceIndex >= currentNode.choices.Count)
        {
            Debug.LogWarning($"[DialogueManager] Invalid choice index: {choiceIndex} (valid range: 0-{currentNode.choices.Count - 1})");
            return;
        }
        
        var choice = currentNode.choices[choiceIndex];
        Debug.Log($"[DialogueManager] Selected choice: '{choice.choiceText}', targetNodeId: '{choice.targetNodeId}'");
        
        // Check choice condition
        if (!EvaluateCondition(choice.condition))
        {
            Debug.LogWarning($"[DialogueManager] Choice '{choice.choiceText}' condition not met.");
            return;
        }
        
        // Check if choice action is TransitionScene - if so, we need to handle exit actions specially
        bool isTransitioningViaChoice = choice.action != null && 
                                        choice.action.actionType == DialogueAction.ActionType.TransitionScene;
        
        // Execute choice action (if any)
        ExecuteAction(choice.action);
        
        // IMPORTANT: If choice action is TransitionScene, exit actions will be handled in TransitionToScene
        // Otherwise, exit actions should only execute when:
        // 1. Clicking Continue on a node (handled in Continue() method)
        // 2. When ending dialogue (handled when choice has no target)
        // This ensures dialogue is shown before any panels/transitions open
        
        // Move to next node
        if (!string.IsNullOrEmpty(choice.targetNodeId))
        {
            Debug.Log($"[DialogueManager] Looking for target node: '{choice.targetNodeId}'");
            Debug.Log($"[DialogueManager] currentDialogue is null: {currentDialogue == null}");
            if (currentDialogue != null)
            {
                Debug.Log($"[DialogueManager] currentDialogue.dialogueId: '{currentDialogue.dialogueId}'");
            }
            
            var nextNode = currentDialogue?.GetNode(choice.targetNodeId);
            Debug.Log($"[DialogueManager] nextNode is null: {nextNode == null}");
            if (nextNode != null)
            {
                Debug.Log($"[DialogueManager] Found target node '{choice.targetNodeId}', showing it...");
                dialogueHistory.Push(currentNode);
                ShowNode(nextNode);
            }
            else
            {
                Debug.LogWarning($"[DialogueManager] Target node '{choice.targetNodeId}' not found in dialogue '{currentDialogue?.dialogueId}'!");
                Debug.LogWarning($"[DialogueManager] Available nodes: {string.Join(", ", currentDialogue?.nodes?.Select(n => n.nodeId) ?? new string[0])}");
                EndDialogue();
            }
        }
        else
        {
            Debug.Log("[DialogueManager] Choice has no targetNodeId, executing exit actions and ending dialogue...");
            
            // Store the node for exit action execution
            DialogueNode nodeBeingExited = currentNode;
            
            // If choice action was TransitionScene, exit actions were already handled in TransitionToScene
            // Otherwise, execute exit actions now (this allows tutorials to start after scene transitions)
            if (!isTransitioningViaChoice)
            {
                ExecuteExitActions(nodeBeingExited);
            }
            
            EndDialogue();
        }
    }
    
    /// <summary>
    /// Continue to next node (for non-choice nodes)
    /// </summary>
    public void Continue()
    {
        Debug.Log($"[DialogueManager] Continue() called. Current node: {currentNode?.nodeId ?? "null"}");
        
        if (currentNode == null)
        {
            EndDialogue();
            return;
        }
        
        // If node has choices, do nothing (wait for choice selection)
        if (currentNode.HasChoices)
        {
            Debug.Log($"[DialogueManager] Continue() called but node has choices. Ignoring.");
            return;
        }
        
        // Execute exit action(s) - this is called from Continue() when user clicks Continue button
        Debug.Log($"[DialogueManager] Executing exit actions for node: {currentNode.nodeId}");
        ExecuteExitActions(currentNode);
        
        // If this is an end node, close dialogue
        if (currentNode.IsEndNode)
        {
            Debug.Log($"[DialogueManager] Node {currentNode.nodeId} is an end node. Ending dialogue.");
            EndDialogue();
        }
    }
    
    /// <summary>
    /// Execute exit actions from a node (used when leaving a node)
    /// </summary>
    private void ExecuteExitActions(DialogueNode node)
    {
        if (node == null)
        {
            Debug.LogWarning("[DialogueManager] ExecuteExitActions called with null node!");
            return;
        }
        
        Debug.Log($"[DialogueManager] ExecuteExitActions for node '{node.nodeId}'. " +
                  $"onExitActions count: {node.onExitActions?.Count ?? 0}, " +
                  $"onExitAction: {(node.onExitAction != null ? node.onExitAction.actionType.ToString() : "null")}");
        
        // Check if exit action is TransitionScene - if so, delay tutorial start
        bool isTransitioning = false;
        if (node.onExitActions != null && node.onExitActions.Count > 0)
        {
            isTransitioning = node.onExitActions.Any(a => a != null && a.actionType == DialogueAction.ActionType.TransitionScene);
        }
        else if (node.onExitAction != null)
        {
            isTransitioning = node.onExitAction.actionType == DialogueAction.ActionType.TransitionScene;
        }
        
        // If transitioning and tutorial is active, prepare it for resumption
        if (isTransitioning && TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            pendingTutorialId = TutorialManager.Instance.currentTutorialId;
            pendingTutorialStepIndex = TutorialManager.Instance.CurrentStepIndex;
        }
        
        // Execute exit action(s)
        if (node.onExitActions != null && node.onExitActions.Count > 0)
        {
            // Check if TransitionScene is in the list and if StartTutorial comes after it
            bool hasTransition = node.onExitActions.Any(a => a != null && a.actionType == DialogueAction.ActionType.TransitionScene);
            int transitionIndex = -1;
            if (hasTransition)
            {
                for (int i = 0; i < node.onExitActions.Count; i++)
                {
                    if (node.onExitActions[i] != null && 
                        node.onExitActions[i].actionType == DialogueAction.ActionType.TransitionScene)
                    {
                        transitionIndex = i;
                        break;
                    }
                }
            }
            
            // Execute list of actions
            for (int i = 0; i < node.onExitActions.Count; i++)
            {
                var action = node.onExitActions[i];
                if (action == null) continue;
                
                // If this is StartTutorial and TransitionScene comes before it, skip it
                // (it will be handled after scene transition)
                if (action.actionType == DialogueAction.ActionType.StartTutorial && 
                    transitionIndex >= 0 && i > transitionIndex)
                {
                    // Tutorial will start after scene transition
                    pendingTutorialIdAfterTransition = action.actionValue;
                    continue;
                }
                
                ExecuteAction(action);
            }
        }
        else if (node.onExitAction != null)
        {
            // Execute single action (backward compatibility)
            ExecuteAction(node.onExitAction);
        }
    }
    
    private bool isTransitioningScene = false; // Track if we're transitioning scenes
    
    /// <summary>
    /// End the current dialogue
    /// </summary>
    public void EndDialogue()
    {
        // Play reverse animation on NPC if available (skip if transitioning to avoid DOTween errors)
        if (currentNPCInteractable != null && !isTransitioningScene)
        {
            currentNPCInteractable.PlayReverseAnimation();
        }
        else if (currentNPCInteractable != null && isTransitioningScene)
        {
            // Kill any active tweens on the NPC to prevent DOTween errors during scene transition
            currentNPCInteractable.KillAllAnimations();
            Debug.Log("[DialogueManager] Skipping reverse animation and killing tweens because scene is transitioning.");
        }
        
        // Clear NPC reference
        currentNPCInteractable = null;
        if (dialogueUI != null)
        {
            dialogueUI.HideDialogue();
        }
        
        currentDialogue = null;
        currentNode = null;
        dialogueHistory.Clear();
    }
    
    /// <summary>
    /// Go back to previous node
    /// </summary>
    public void GoBack()
    {
        if (dialogueHistory.Count > 0)
        {
            var previousNode = dialogueHistory.Pop();
            ShowNode(previousNode);
        }
    }
    
    private bool EvaluateCondition(DialogueCondition condition)
    {
        if (condition == null || condition.conditionType == DialogueCondition.ConditionType.None)
            return true;
        
        switch (condition.conditionType)
        {
            case DialogueCondition.ConditionType.DialogueNodeSeen:
                // Check if a specific dialogue node has been seen
                if (!string.IsNullOrEmpty(condition.conditionValue))
                {
                    return seenNodeIds.Contains(condition.conditionValue);
                }
                return false;
            
            case DialogueCondition.ConditionType.TutorialCompleted:
                // Check if a tutorial has been completed
                if (!string.IsNullOrEmpty(condition.conditionValue))
                {
                    var character = CharacterManager.Instance != null ? CharacterManager.Instance.GetCurrentCharacter() : null;
                    if (character != null)
                    {
                        return character.HasCompletedTutorial(condition.conditionValue);
                    }
                }
                return false;
            
            case DialogueCondition.ConditionType.QuestCompleted:
                // TODO: Implement when quest system is integrated
                return true;
            
            case DialogueCondition.ConditionType.QuestActive:
                // TODO: Implement when quest system is integrated
                return true;
            
            case DialogueCondition.ConditionType.LevelRequirement:
                // TODO: Implement level checking
                if (CharacterManager.Instance != null && CharacterManager.Instance.currentCharacter != null)
                {
                    return CharacterManager.Instance.currentCharacter.level >= condition.intValue;
                }
                return false;
            
            case DialogueCondition.ConditionType.ItemOwned:
                // TODO: Implement when item system is integrated
                return true;
            
            case DialogueCondition.ConditionType.Custom:
                // TODO: Implement custom condition logic
                return true;
            
            default:
                return true;
        }
    }
    
    /// <summary>
    /// Give a warrant pack to the player (e.g., "warrant_starter_pack" with 3 warrants)
    /// </summary>
    private void GiveWarrantPack(string packId, int quantity)
    {
        if (string.IsNullOrEmpty(packId))
        {
            Debug.LogWarning("[DialogueManager] GiveWarrantPack called with empty packId.");
            return;
        }
        
        // Use assigned reference or find WarrantPackDatabase
        WarrantPackDatabase packDatabase = warrantPackDatabase;
        if (packDatabase == null)
        {
            Debug.Log("[DialogueManager] WarrantPackDatabase not assigned in Inspector, searching for it...");
            packDatabase = FindFirstObjectByType<WarrantPackDatabase>();
            if (packDatabase == null)
            {
                // Try to find it in resources (try multiple possible paths)
                packDatabase = Resources.Load<WarrantPackDatabase>("QuestRewards/Joreg/WarrantPackDatabase");
                if (packDatabase == null)
                {
                    packDatabase = Resources.Load<WarrantPackDatabase>("WarrantPackDatabase");
                }
                if (packDatabase == null)
                {
                    // Try searching all resources
                    WarrantPackDatabase[] allDatabases = Resources.LoadAll<WarrantPackDatabase>("");
                    if (allDatabases != null && allDatabases.Length > 0)
                    {
                        packDatabase = allDatabases[0];
                        Debug.Log($"[DialogueManager] Found WarrantPackDatabase at non-standard path, using first available: {packDatabase.name}");
                    }
                }
            }
        }
        
        if (packDatabase == null)
        {
            Debug.LogError("[DialogueManager] WarrantPackDatabase not found. Cannot give warrant pack. " +
                         "Please assign it in the Inspector on DialogueManager, or ensure it exists in Resources folder (e.g., Resources/QuestRewards/Joreg/WarrantPackDatabase.asset)");
            return;
        }
        
        Debug.Log($"[DialogueManager] Using WarrantPackDatabase: {packDatabase.name}");
        
        // Get the pack
        WarrantPack pack = packDatabase.GetPackById(packId);
        if (pack == null)
        {
            Debug.LogWarning($"[DialogueManager] Warrant pack '{packId}' not found in database.");
            return;
        }
        
        if (!pack.IsValid())
        {
            Debug.LogWarning($"[DialogueManager] Warrant pack '{packId}' is invalid (missing ID or warrants).");
            return;
        }
        
        // Use assigned reference or find WarrantDatabase
        WarrantDatabase warrantDb = warrantDatabase;
        if (warrantDb == null)
        {
            // Try to get it from WarrantPackDatabase if available
            warrantDb = packDatabase.GetWarrantDatabase();
            if (warrantDb != null)
            {
                Debug.Log("[DialogueManager] Using WarrantDatabase from WarrantPackDatabase.");
            }
        }
        
        if (warrantDb == null)
        {
            Debug.Log("[DialogueManager] WarrantDatabase not assigned in Inspector, searching for it...");
            warrantDb = FindFirstObjectByType<WarrantDatabase>();
            if (warrantDb == null)
            {
                // Try to find it in resources (try multiple possible paths)
                warrantDb = Resources.Load<WarrantDatabase>("WarrantTree/WarrantDatabase");
                if (warrantDb == null)
                {
                    warrantDb = Resources.Load<WarrantDatabase>("WarrantDatabase");
                }
                if (warrantDb == null)
                {
                    // Try searching all resources
                    WarrantDatabase[] allDatabases = Resources.LoadAll<WarrantDatabase>("");
                    if (allDatabases != null && allDatabases.Length > 0)
                    {
                        warrantDb = allDatabases[0];
                        Debug.Log($"[DialogueManager] Found WarrantDatabase at non-standard path, using first available: {warrantDb.name}");
                    }
                }
            }
        }
        
        if (warrantDb == null)
        {
            Debug.LogError("[DialogueManager] WarrantDatabase not found. Cannot give warrant pack. " +
                         "Please assign it in the Inspector on DialogueManager, or ensure it exists in Resources folder (e.g., Resources/WarrantTree/WarrantDatabase.asset)");
            return;
        }
        
        Debug.Log($"[DialogueManager] Using WarrantDatabase: {warrantDb.name}");
        
        // Use assigned reference or find WarrantLockerGrid
        WarrantLockerGrid lockerGrid = warrantLockerGrid;
        if (lockerGrid == null)
        {
            Debug.Log("[DialogueManager] WarrantLockerGrid not assigned in Inspector, searching for it in scene (including inactive)...");
            // Try finding active first
            lockerGrid = FindFirstObjectByType<WarrantLockerGrid>();
            if (lockerGrid == null)
            {
                // Try finding inactive objects as well (for newer Unity versions)
                #if UNITY_2021_2_OR_NEWER
                WarrantLockerGrid[] allLockerGrids = FindObjectsByType<WarrantLockerGrid>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                if (allLockerGrids != null && allLockerGrids.Length > 0)
                {
                    lockerGrid = allLockerGrids[0];
                    Debug.Log($"[DialogueManager] Found WarrantLockerGrid (inactive): {lockerGrid.name}");
                }
                #else
                // Fallback for older Unity versions
                WarrantLockerGrid[] allLockerGrids = FindObjectsByType<WarrantLockerGrid>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                if (allLockerGrids != null && allLockerGrids.Length > 0)
                {
                    lockerGrid = allLockerGrids[0];
                    Debug.Log($"[DialogueManager] Found WarrantLockerGrid (inactive): {lockerGrid.name}");
                }
                #endif
            }
            
            if (lockerGrid == null)
            {
                // Try searching by name as last resort
                GameObject lockerObj = GameObject.Find("WarrantLockerGrid");
                if (lockerObj != null)
                {
                    lockerGrid = lockerObj.GetComponent<WarrantLockerGrid>();
                    if (lockerGrid != null)
                    {
                        Debug.Log($"[DialogueManager] Found WarrantLockerGrid by name: {lockerObj.name}");
                    }
                }
            }
        }
        
        if (lockerGrid == null)
        {
            Debug.LogError("[DialogueManager] WarrantLockerGrid not found. Cannot add warrants to locker. " +
                         "Please assign it in the Inspector on DialogueManager (e.g., TownCanvas/Menus/PeacekeepersFactionPanel/WarrantFusionPanel/WarrantLockerGrid), " +
                         "or ensure it exists in the scene.");
            return;
        }
        
        Debug.Log($"[DialogueManager] Using WarrantLockerGrid: {lockerGrid.name} (GameObject active: {lockerGrid.gameObject.activeSelf}, activeInHierarchy: {lockerGrid.gameObject.activeInHierarchy})");
        
        // Give warrants from the pack (quantity determines how many times to give the full pack)
        int totalWarrantsGiven = 0;
        for (int packIteration = 0; packIteration < quantity; packIteration++)
        {
            foreach (var blueprint in pack.warrantBlueprints)
            {
                if (blueprint == null)
                {
                    Debug.LogWarning($"[DialogueManager] Warrant pack '{packId}' contains a null blueprint. Skipping.");
                    continue;
                }
                
                // Roll and add the warrant
                WarrantDefinition rolledInstance = WarrantRollingUtility.RollAndAddToLocker(
                    blueprint,
                    warrantDb,
                    lockerGrid,
                    minAffixes: pack.minAffixes,
                    maxAffixes: pack.maxAffixes
                );
                
                if (rolledInstance != null)
                {
                    totalWarrantsGiven++;
                    Debug.Log($"[DialogueManager] Gave warrant '{rolledInstance.warrantId}' from pack '{packId}' to player.");
                }
                else
                {
                    Debug.LogWarning($"[DialogueManager] Failed to roll warrant from blueprint '{blueprint.warrantId}' in pack '{packId}'.");
                }
            }
        }
        
        // Show notification popup if warrants were given
        if (totalWarrantsGiven > 0)
        {
            ShowWarrantPackNotification(pack, totalWarrantsGiven);
        }
        
        Debug.Log($"[DialogueManager] Successfully gave {totalWarrantsGiven} warrant(s) from pack '{packId}' (x{quantity}) to player.");
    }
    
    /// <summary>
    /// Show a notification when a warrant pack is given to the player
    /// </summary>
    private void ShowWarrantPackNotification(WarrantPack pack, int warrantCount)
    {
        Debug.Log($"[DialogueManager] ShowWarrantPackNotification called: pack='{pack.packId}', count={warrantCount}");
        
        if (NotificationManager.Instance != null)
        {
            string packName = !string.IsNullOrEmpty(pack.displayName) ? pack.displayName : pack.packId;
            Debug.Log($"[DialogueManager] Calling NotificationManager.ShowWarrantPackNotification: '{packName}', {warrantCount} warrants");
            
            // Delay notification slightly to ensure it appears after dialogue UI updates
            StartCoroutine(DelayedNotification(packName, warrantCount));
        }
        else
        {
            Debug.LogWarning("[DialogueManager] NotificationManager.Instance is null. Cannot show warrant pack notification. " +
                           "Make sure NotificationManager exists in the scene or is created automatically.");
        }
    }
    
    /// <summary>
    /// Delay notification slightly to ensure UI is ready
    /// </summary>
    /// <summary>
    /// Delayed end dialogue coroutine - waits a frame to ensure shop opens before closing dialogue
    /// </summary>
    private System.Collections.IEnumerator DelayedEndDialogueCoroutine()
    {
        yield return null; // Wait one frame
        EndDialogue();
    }
    
    private System.Collections.IEnumerator DelayedNotification(string packName, int warrantCount)
    {
        yield return null; // Wait one frame for UI to update
        yield return null; // Wait another frame for notification canvas to be ready
        
        NotificationManager.Instance.ShowWarrantPackNotification(packName, warrantCount);
    }
    
    private void ExecuteAction(DialogueAction action)
    {
        if (action == null || action.actionType == DialogueAction.ActionType.None)
            return;
        
        switch (action.actionType)
        {
            case DialogueAction.ActionType.MarkNodeSeen:
                // Mark a dialogue node as seen (for unlocking conditional choices)
                if (!string.IsNullOrEmpty(action.actionValue))
                {
                    seenNodeIds.Add(action.actionValue);
                    Debug.Log($"[DialogueManager] Marked node '{action.actionValue}' as seen.");
                }
                break;
            
            case DialogueAction.ActionType.OpenPanel:
                // Open NPC panel (Seer, Forge, etc.)
                OpenNPCPanel(action.actionValue);
                break;
            
            case DialogueAction.ActionType.StartQuest:
                // TODO: Implement when quest system is integrated
                Debug.Log($"[DialogueManager] StartQuest action not yet implemented: {action.actionValue}");
                break;
            
            case DialogueAction.ActionType.CompleteQuest:
                // TODO: Implement when quest system is integrated
                Debug.Log($"[DialogueManager] CompleteQuest action not yet implemented: {action.actionValue}");
                break;
            
            case DialogueAction.ActionType.GiveItem:
                // Handle warrant packs (e.g., "warrant_starter_pack")
                GiveWarrantPack(action.actionValue, action.intValue);
                break;
            
            case DialogueAction.ActionType.RemoveItem:
                // TODO: Implement when item system is integrated
                Debug.Log($"[DialogueManager] RemoveItem action not yet implemented: {action.actionValue} x{action.intValue}");
                break;
            
            case DialogueAction.ActionType.OpenShop:
                // Open shop panel (e.g., "MazeVendor" opens the maze vendor shop)
                Debug.Log($"[DialogueManager] Executing OpenShop action with value: {action.actionValue}");
                OpenShop(action.actionValue);
                // Close dialogue when opening shop (after a brief delay to ensure shop opens first)
                StartCoroutine(DelayedEndDialogueCoroutine());
                break;
            
            case DialogueAction.ActionType.TransitionScene:
                // Transition to a different scene
                // If next action is StartTutorial, store it to execute after transition
                TransitionToScene(action.actionValue);
                break;
            
            case DialogueAction.ActionType.StartTutorial:
                // Start a tutorial sequence
                StartTutorial(action.actionValue);
                break;
            
            case DialogueAction.ActionType.Custom:
                // TODO: Implement custom action logic
                Debug.Log($"[DialogueManager] Custom action: {action.actionValue}");
                break;
            
            default:
                break;
        }
    }
    
    private void TransitionToScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[DialogueManager] Cannot transition to empty scene name.");
            return;
        }
        
        // Check if there's a StartTutorial action coming up in the same action list
        // This happens when actions are chained (e.g., TransitionScene then StartTutorial)
        // OR when StartTutorial is in exit actions and we're transitioning via choice action
        bool hasTutorialActionAfterTransition = false;
        string tutorialIdToStart = null;
        
        if (currentNode != null)
        {
            // Check exit actions list for StartTutorial
            if (currentNode.onExitActions != null && currentNode.onExitActions.Count > 0)
            {
                Debug.Log($"[DialogueManager] Checking {currentNode.onExitActions.Count} exit actions for StartTutorial...");
                
                int transitionIndex = -1;
                for (int i = 0; i < currentNode.onExitActions.Count; i++)
                {
                    var action = currentNode.onExitActions[i];
                    if (action == null) continue;
                    
                    if (action.actionType == DialogueAction.ActionType.TransitionScene)
                    {
                        transitionIndex = i;
                        Debug.Log($"[DialogueManager] Found TransitionScene action at index {i}");
                    }
                    else if (action.actionType == DialogueAction.ActionType.StartTutorial)
                    {
                        // Found StartTutorial - if there's a TransitionScene before it, queue it for after transition
                        // Otherwise, if we're transitioning via choice action, also queue it
                        if (transitionIndex >= 0 || true) // Always queue if we're transitioning
                        {
                            hasTutorialActionAfterTransition = true;
                            tutorialIdToStart = action.actionValue;
                            Debug.Log($"[DialogueManager] Found StartTutorial action at index {i} with tutorialId: '{tutorialIdToStart}'");
                            break;
                        }
                    }
                }
            }
            
            // Also check single exit action (backward compatibility)
            if (!hasTutorialActionAfterTransition && currentNode.onExitAction != null)
            {
                if (currentNode.onExitAction.actionType == DialogueAction.ActionType.StartTutorial)
                {
                    hasTutorialActionAfterTransition = true;
                    tutorialIdToStart = currentNode.onExitAction.actionValue;
                    Debug.Log($"[DialogueManager] Found StartTutorial in onExitAction with tutorialId: '{tutorialIdToStart}'");
                }
            }
        }
        
        // If there's an active tutorial, prepare it to resume after scene load
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            // Store tutorial state for resumption after scene load
            pendingTutorialId = TutorialManager.Instance.currentTutorialId;
            pendingTutorialStepIndex = TutorialManager.Instance.CurrentStepIndex;
            
            TutorialManager.Instance.PrepareTutorialForSceneTransition(
                pendingTutorialId, 
                pendingTutorialStepIndex
            );
        }
        // If a new tutorial is pending to start after transition, prepare it
        else if (hasTutorialActionAfterTransition && !string.IsNullOrEmpty(tutorialIdToStart))
        {
            pendingTutorialIdAfterTransition = tutorialIdToStart;
            TutorialManager.Instance?.PrepareTutorialForSceneTransition(
                tutorialIdToStart,
                0 // Start from beginning
            );
        }
        else if (!string.IsNullOrEmpty(pendingTutorialIdAfterTransition))
        {
            TutorialManager.Instance?.PrepareTutorialForSceneTransition(
                pendingTutorialIdAfterTransition,
                0 // Start from beginning
            );
        }
        
        // Use TransitionManager if available, otherwise use SceneManager directly
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.TransitionToScene(sceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
        
        // Close dialogue before transitioning
        EndDialogue();
    }
    
    private void OnEnable()
    {
        // Subscribe to scene loaded event to start tutorial after transition
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        Debug.Log($"[DialogueManager] Scene loaded: {scene.name}");
        
        // Reset transition flag after scene loads
        isTransitioningScene = false;
        
        // NOTE: Tutorial starting is now handled entirely by TutorialManager.OnSceneLoaded
        // to avoid duplicate starts. DialogueManager just sets up the pending state.
        // No need to start tutorial here - TutorialManager will handle it via PrepareTutorialForSceneTransition
    }
    
    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void StartTutorial(string tutorialId)
    {
        if (string.IsNullOrEmpty(tutorialId))
        {
            Debug.LogWarning("[DialogueManager] Cannot start tutorial with empty ID.");
            return;
        }
        
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.StartTutorial(tutorialId);
        }
        else
        {
            Debug.LogWarning("[DialogueManager] TutorialManager not found. Cannot start tutorial.");
        }
    }
    
    private void OpenNPCPanel(string panelName)
    {
        Debug.Log($"[DialogueManager] Opening panel: {panelName}");
        
        // Handle different panel types
        switch (panelName.ToLower())
        {
            case "warrantfusion":
            case "warrantfusionpanel":
            case "fusion":
                OpenWarrantFusionPanel();
                break;
            
            case "warrantlocker":
            case "locker":
                OpenWarrantLockerPanel();
                break;
            
            default:
                Debug.LogWarning($"[DialogueManager] Unknown panel name: {panelName}. Supported panels: 'WarrantFusion', 'WarrantLocker'");
                // Try to find by GameObject name as fallback
                GameObject panelObj = GameObject.Find(panelName);
                if (panelObj != null)
                {
                    panelObj.SetActive(true);
                    Debug.Log($"[DialogueManager] Found panel by name '{panelName}' and activated it.");
                }
                else
                {
                    Debug.LogWarning($"[DialogueManager] Could not find panel '{panelName}' by name.");
                }
                break;
        }
    }
    
    /// <summary>
    /// Opens the Warrant Fusion Panel (3→1 fusion interface)
    /// </summary>
    private void OpenWarrantFusionPanel()
    {
        Debug.Log("[DialogueManager] OpenWarrantFusionPanel() called. Searching for WarrantFusionUI...");
        
        // Try to find WarrantFusionUI component (including inactive)
        WarrantFusionUI fusionUI = FindFirstObjectByType<WarrantFusionUI>();
        if (fusionUI == null)
        {
            // Try finding inactive ones
            WarrantFusionUI[] allFusionUIs = FindObjectsByType<WarrantFusionUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (allFusionUIs != null && allFusionUIs.Length > 0)
            {
                fusionUI = allFusionUIs[0];
                Debug.Log($"[DialogueManager] Found inactive WarrantFusionUI: {fusionUI.name}");
            }
        }
        
        if (fusionUI != null)
        {
            Debug.Log($"[DialogueManager] Found WarrantFusionUI on '{fusionUI.name}'. Calling ShowPanel()...");
            fusionUI.ShowPanel();
            Debug.Log("[DialogueManager] ✓ Opened WarrantFusionPanel via WarrantFusionUI.ShowPanel().");
            return;
        }
        
        Debug.LogWarning("[DialogueManager] WarrantFusionUI component not found. Trying to find by GameObject name...");
        
        // Fallback: Try to find by GameObject name (including inactive)
        GameObject fusionPanel = GameObject.Find("WarrantFusionPanel");
        if (fusionPanel == null)
        {
            fusionPanel = GameObject.Find("WarrantFusion");
        }
        
        // Also try searching in inactive GameObjects
        if (fusionPanel == null)
        {
            Transform[] allTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Transform t in allTransforms)
            {
                if (t.name == "WarrantFusionPanel" || t.name == "WarrantFusion")
                {
                    fusionPanel = t.gameObject;
                    break;
                }
            }
        }
        
        if (fusionPanel != null)
        {
            Debug.Log($"[DialogueManager] Found panel GameObject: '{fusionPanel.name}'. Activating...");
            fusionPanel.SetActive(true);
            
            // Also try to get WarrantFusionUI and call ShowPanel if it exists
            WarrantFusionUI ui = fusionPanel.GetComponent<WarrantFusionUI>();
            if (ui != null)
            {
                ui.ShowPanel();
                Debug.Log("[DialogueManager] ✓ Also called ShowPanel() on WarrantFusionUI component.");
            }
            
            Debug.Log("[DialogueManager] ✓ Opened WarrantFusionPanel by GameObject name.");
        }
        else
        {
            Debug.LogError("[DialogueManager] ✗ Could not find WarrantFusionPanel! Searched for:\n" +
                          "  - WarrantFusionUI component\n" +
                          "  - GameObject named 'WarrantFusionPanel'\n" +
                          "  - GameObject named 'WarrantFusion'\n" +
                          "Make sure WarrantFusionPanel exists in the scene with WarrantFusionUI component.");
        }
    }
    
    /// <summary>
    /// Opens the Warrant Locker Panel
    /// </summary>
    private void OpenWarrantLockerPanel()
    {
        // Try to find WarrantLockerPanelManager
        WarrantLockerPanelManager lockerManager = FindFirstObjectByType<WarrantLockerPanelManager>();
        if (lockerManager != null)
        {
            lockerManager.ShowPanel();
            Debug.Log("[DialogueManager] Opened WarrantLockerPanel via WarrantLockerPanelManager.");
            return;
        }
        
        // Fallback: Try to find WarrantLockerGrid directly
        WarrantLockerGrid lockerGrid = FindFirstObjectByType<WarrantLockerGrid>();
        if (lockerGrid != null && lockerGrid.gameObject != null)
        {
            lockerGrid.gameObject.SetActive(true);
            Debug.Log("[DialogueManager] Opened WarrantLockerPanel by activating WarrantLockerGrid.");
        }
        else
        {
            Debug.LogWarning("[DialogueManager] Could not find WarrantLockerPanel. Make sure WarrantLockerPanelManager or WarrantLockerGrid exists in the scene.");
        }
    }
    
    /// <summary>
    /// Opens a shop panel based on shop ID using the ShopDatabase mapping
    /// </summary>
    private void OpenShop(string shopId)
    {
        if (string.IsNullOrEmpty(shopId))
        {
            Debug.LogWarning("[DialogueManager] OpenShop called with empty shopId.");
            return;
        }
        
        Debug.Log($"[DialogueManager] Opening shop: {shopId}");
        
        // Special case: SeerShop opens card generation UI
        if (shopId.Equals("SeerShop", System.StringComparison.OrdinalIgnoreCase))
        {
            OpenSeerCardGeneration();
            return;
        }
        
        // Ensure shop database is loaded
        if (shopDatabase == null)
        {
            LoadShopDatabase();
        }
        
        if (shopDatabase == null)
        {
            Debug.LogError("[DialogueManager] ShopDatabase not found! Cannot open shop. Please create and assign a ShopDatabase asset.");
            // Fallback to legacy method for backward compatibility
            OpenMazeVendorShop();
            return;
        }
        
        // Get shop mapping from database
        ShopDatabase.ShopMapping mapping = shopDatabase.GetShopMapping(shopId);
        if (mapping == null)
        {
            Debug.LogWarning($"[DialogueManager] Shop ID '{shopId}' not found in ShopDatabase. Please add it to the database or check the spelling.");
            // Fallback to legacy method for backward compatibility
            if (shopId.ToLower() == "mazevendor" || shopId.ToLower() == "blinket" || shopId.ToLower() == "vendor")
            {
                Debug.Log("[DialogueManager] Falling back to legacy OpenMazeVendorShop() method.");
                OpenMazeVendorShop();
            }
            return;
        }
        
        // Open shop based on access method
        bool opened = false;
        switch (mapping.accessMethod)
        {
            case ShopDatabase.PanelAccessMethod.MazeHubController:
                opened = OpenShopViaMazeHubController(mapping);
                break;
                
            case ShopDatabase.PanelAccessMethod.DirectPanel:
                opened = OpenShopViaDirectPanel(mapping);
                break;
                
            case ShopDatabase.PanelAccessMethod.ComponentType:
                opened = OpenShopViaComponentType(mapping);
                break;
                
            case ShopDatabase.PanelAccessMethod.GameObjectName:
                opened = OpenShopViaGameObjectName(mapping);
                break;
        }
        
        if (!opened)
        {
            Debug.LogWarning($"[DialogueManager] Failed to open shop '{shopId}' using method {mapping.accessMethod}. Check the mapping configuration.");
        }
    }
    
    /// <summary>
    /// Opens the Seer's card generation UI
    /// </summary>
    private void OpenSeerCardGeneration()
    {
        Debug.Log("[DialogueManager] Opening Seer card generation UI...");
        
        // Try to find SeerCardGenerationUI component in the scene
        SeerCardGenerationUI cardGenUI = FindFirstObjectByType<SeerCardGenerationUI>(FindObjectsInactive.Include);
        
        if (cardGenUI == null)
        {
            Debug.LogWarning("[DialogueManager] SeerCardGenerationUI not found in scene! Please ensure the UI GameObject is present.");
            return;
        }
        
        cardGenUI.Open();
        Debug.Log("[DialogueManager] Seer card generation UI opened successfully.");
    }
    
    /// <summary>
    /// Loads the ShopDatabase from Resources if not assigned
    /// </summary>
    private void LoadShopDatabase()
    {
        if (shopDatabase != null)
            return;
        
        shopDatabase = Resources.Load<ShopDatabase>("ShopDatabase");
        if (shopDatabase != null)
        {
            Debug.Log("[DialogueManager] Auto-loaded ShopDatabase from Resources.");
        }
        else
        {
            // Try to find any ShopDatabase asset
            #if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ShopDatabase");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                shopDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<ShopDatabase>(path);
                if (shopDatabase != null)
                {
                    Debug.Log($"[DialogueManager] Found ShopDatabase at: {path}");
                }
            }
            #endif
        }
    }
    
    /// <summary>
    /// Open shop via MazeHubController field access
    /// </summary>
    private bool OpenShopViaMazeHubController(ShopDatabase.ShopMapping mapping)
    {
        Dexiled.MazeSystem.MazeHubController hubController = FindFirstObjectByType<Dexiled.MazeSystem.MazeHubController>();
        if (hubController == null)
        {
            Debug.LogWarning("[DialogueManager] MazeHubController not found in scene!");
            return false;
        }
        
        // Use reflection to access the panel field by name
        var panelField = typeof(Dexiled.MazeSystem.MazeHubController).GetField(
            mapping.panelFieldName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        
        if (panelField == null)
        {
            Debug.LogWarning($"[DialogueManager] Panel field '{mapping.panelFieldName}' not found in MazeHubController!");
            return false;
        }
        
        GameObject panel = panelField.GetValue(hubController) as GameObject;
        if (panel == null)
        {
            Debug.LogWarning($"[DialogueManager] Panel field '{mapping.panelFieldName}' is null in MazeHubController! Check Inspector assignment.");
            return false;
        }
        
        Debug.Log($"[DialogueManager] Opening shop via MazeHubController.{mapping.panelFieldName}");
        hubController.ShowPanel(panel);
        return true;
    }
    
    /// <summary>
    /// Open shop via direct GameObject reference
    /// </summary>
    private bool OpenShopViaDirectPanel(ShopDatabase.ShopMapping mapping)
    {
        if (mapping.panelGameObject == null)
        {
            Debug.LogWarning($"[DialogueManager] Panel GameObject is null for shop mapping!");
            return false;
        }
        
        Debug.Log($"[DialogueManager] Opening shop via direct GameObject: {mapping.panelGameObject.name}");
        mapping.panelGameObject.SetActive(true);
        return true;
    }
    
    /// <summary>
    /// Open shop via component type name
    /// </summary>
    private bool OpenShopViaComponentType(ShopDatabase.ShopMapping mapping)
    {
        if (string.IsNullOrEmpty(mapping.componentTypeName))
        {
            Debug.LogWarning($"[DialogueManager] Component type name is empty for shop mapping!");
            return false;
        }
        
        // Try to find component by type name using reflection
        System.Type componentType = System.Type.GetType(mapping.componentTypeName);
        if (componentType == null)
        {
            // Try with assembly-qualified name
            componentType = System.Type.GetType($"{mapping.componentTypeName}, Assembly-CSharp");
        }
        
        if (componentType == null)
        {
            Debug.LogWarning($"[DialogueManager] Could not find component type: {mapping.componentTypeName}");
            return false;
        }
        
        // Use Object.FindObjectsByType with Type parameter (works at runtime)
        Object[] foundObjects = Object.FindObjectsByType(componentType, FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (foundObjects == null || foundObjects.Length == 0)
        {
            Debug.LogWarning($"[DialogueManager] Component of type '{mapping.componentTypeName}' not found in scene!");
            return false;
        }
        
        Component component = foundObjects[0] as Component;
        if (component == null || component.gameObject == null)
        {
            Debug.LogWarning($"[DialogueManager] Found object but it's not a Component!");
            return false;
        }
        
        Debug.Log($"[DialogueManager] Opening shop via component type: {mapping.componentTypeName}");
        component.gameObject.SetActive(true);
        return true;
    }
    
    /// <summary>
    /// Open shop via GameObject name search
    /// </summary>
    private bool OpenShopViaGameObjectName(ShopDatabase.ShopMapping mapping)
    {
        if (mapping.gameObjectNames == null || mapping.gameObjectNames.Count == 0)
        {
            Debug.LogWarning($"[DialogueManager] GameObject names list is empty for shop mapping!");
            return false;
        }
        
        GameObject foundPanel = null;
        foreach (string name in mapping.gameObjectNames)
        {
            if (string.IsNullOrEmpty(name))
                continue;
            
            foundPanel = GameObject.Find(name);
            if (foundPanel != null)
            {
                Debug.Log($"[DialogueManager] Opening shop via GameObject name: {name}");
                foundPanel.SetActive(true);
                return true;
            }
        }
        
        Debug.LogWarning($"[DialogueManager] Could not find GameObject with any of these names: {string.Join(", ", mapping.gameObjectNames)}");
        return false;
    }
    
    /// <summary>
    /// Opens the Maze Vendor shop panel (Blinket's shop)
    /// </summary>
    private void OpenMazeVendorShop()
    {
        Debug.Log("[DialogueManager] OpenMazeVendorShop() called - searching for vendor panel...");
        
        // Try to find MazeHubController first (it manages the vendor panel)
        Dexiled.MazeSystem.MazeHubController hubController = FindFirstObjectByType<Dexiled.MazeSystem.MazeHubController>();
        if (hubController != null)
        {
            Debug.Log("[DialogueManager] Found MazeHubController");
            
            // vendorPanel is public, so we can access it directly
            if (hubController.vendorPanel != null)
            {
                Debug.Log($"[DialogueManager] Found vendorPanel GameObject: {hubController.vendorPanel.name}, calling ShowPanel()...");
                // Use the public ShowPanel method
                hubController.ShowPanel(hubController.vendorPanel);
                Debug.Log("[DialogueManager] ✓ Opened Maze Vendor shop via MazeHubController.ShowPanel().");
                return;
            }
            else
            {
                Debug.LogWarning("[DialogueManager] vendorPanel is null in MazeHubController! Check the Inspector assignment.");
            }
        }
        else
        {
            Debug.LogWarning("[DialogueManager] MazeHubController not found in scene!");
        }
        
        // Fallback: Try to find MazeVendorUI directly
        Dexiled.MazeSystem.MazeVendorUI vendorUI = FindFirstObjectByType<Dexiled.MazeSystem.MazeVendorUI>();
        if (vendorUI != null && vendorUI.gameObject != null)
        {
            vendorUI.gameObject.SetActive(true);
            Debug.Log("[DialogueManager] Opened Maze Vendor shop by activating MazeVendorUI.");
            return;
        }
        
        // Fallback: Try to find vendor panel by name
        GameObject vendorPanelObj = GameObject.Find("VendorPanel");
        if (vendorPanelObj == null)
        {
            vendorPanelObj = GameObject.Find("MazeVendorPanel");
        }
        if (vendorPanelObj == null)
        {
            vendorPanelObj = GameObject.Find("BlinketShopPanel");
        }
        
        if (vendorPanelObj != null)
        {
            vendorPanelObj.SetActive(true);
            Debug.Log("[DialogueManager] Opened Maze Vendor shop by GameObject name.");
            return;
        }
        
        Debug.LogWarning("[DialogueManager] Could not find Maze Vendor shop panel. Make sure MazeHubController or MazeVendorUI exists in the scene.");
    }
    
    /// <summary>
    /// Get a start node based on conditions (e.g., tutorial completion).
    /// Checks all nodes in the dialogue and returns the first one that matches conditions.
    /// Useful for having different start nodes based on game state.
    /// </summary>
    private DialogueNode GetConditionalStartNode(DialogueData dialogue)
    {
        if (dialogue == null || dialogue.nodes == null)
            return null;
        
        // Look for nodes that are conditional starts based on naming patterns
        // Common patterns: "start_tutorial", "start_post_tutorial", "post_tutorial", "post_fusion", etc.
        // Exclude nodes like "main" which are part of normal dialogue flow
        foreach (var node in dialogue.nodes)
        {
            if (node == null || string.IsNullOrEmpty(node.nodeId))
                continue;
            
            // Skip the actual start node
            string nodeIdLower = node.nodeId.ToLower();
            if (nodeIdLower == dialogue.startNodeId?.ToLower())
                continue;
            
            // Check if this node's ID suggests it's a conditional start node
            // Patterns: "start_*" (except "start_main"), "post_*", etc.
            bool isConditionalStart = false;
            
            // Pattern 1: Nodes starting with "start_" (but not "start_main")
            if (nodeIdLower.StartsWith("start_") && nodeIdLower != "start_main")
            {
                isConditionalStart = true;
            }
            // Pattern 2: Nodes starting with "post_" (conditional starts after tutorials/fusion)
            else if (nodeIdLower.StartsWith("post_"))
            {
                isConditionalStart = true;
            }
            
            if (isConditionalStart)
            {
                // Only consider nodes with actual conditions (not "None")
                // Nodes with no condition should be reached through normal dialogue flow
                if (node.condition != null && node.condition.conditionType != DialogueCondition.ConditionType.None)
                {
                    // Check if this node's condition is met
                    if (EvaluateCondition(node.condition))
                    {
                        Debug.Log($"[DialogueManager] Using conditional start node: '{node.nodeId}' (condition met)");
                        return node;
                    }
                }
            }
        }
        
        return null;
    }
    
    public bool IsDialogueActive => currentDialogue != null && currentNode != null;
    
    /// <summary>
    /// Public method to evaluate choice conditions (for UI filtering)
    /// </summary>
    public bool EvaluateChoiceCondition(DialogueChoice choice)
    {
        if (choice == null)
            return false;
        
        return EvaluateCondition(choice.condition);
    }
}


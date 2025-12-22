# NPC Dialogue System - Setup Guide

## Overview

This guide covers implementing an NPC dialogue system for Dexiled that integrates with the TownScene NPCs (Seer, Questgiver, Forge, PeacekeepersFaction). The system uses ScriptableObjects for dialogue data (consistent with your Warrant/Enemy database patterns) and Unity's legacy UI (uGUI) to match your existing UI architecture.

## Architecture Overview

### Core Components

1. **DialogueData** (ScriptableObject) - Dialogue tree data
2. **DialogueNode** - Individual dialogue lines with branching
3. **DialogueManager** (Singleton) - Manages dialogue state and display
4. **DialogueUI** (Unity Legacy UI) - UI panel for displaying dialogue
5. **NPCInteractable** - Component for NPCs that can be clicked to start dialogue

### Data Flow

```
NPC Click → NPCInteractable → DialogueManager → DialogueUI
                ↓
         DialogueData (ScriptableObject)
```

## Step 1: Create Dialogue Data Structure

### DialogueNode.cs

```csharp
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueNode
{
    [Header("Content")]
    public string nodeId;
    public string speakerName;
    [TextArea(3, 8)]
    public string dialogueText;
    public Sprite speakerPortrait;
    
    [Header("Choices")]
    public List<DialogueChoice> choices = new List<DialogueChoice>();
    
    [Header("Actions")]
    public DialogueAction onEnterAction;
    public DialogueAction onExitAction;
    
    [Header("Conditions")]
    public DialogueCondition condition;
    
    public bool HasChoices => choices != null && choices.Count > 0;
    public bool IsEndNode => !HasChoices;
}

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;
    public string targetNodeId; // Next node to go to
    public DialogueCondition condition; // Optional: only show if condition met
    public DialogueAction action; // Optional: execute action when chosen
}

[System.Serializable]
public class DialogueCondition
{
    public enum ConditionType
    {
        None,
        QuestCompleted,
        QuestActive,
        LevelRequirement,
        ItemOwned,
        Custom
    }
    
    public ConditionType conditionType;
    public string conditionValue; // Quest ID, Item ID, etc.
    public int intValue; // For level requirements
}

[System.Serializable]
public class DialogueAction
{
    public enum ActionType
    {
        None,
        StartQuest,
        CompleteQuest,
        GiveItem,
        RemoveItem,
        OpenShop,
        OpenPanel, // Open Seer/Forge/etc. panel
        Custom
    }
    
    public ActionType actionType;
    public string actionValue; // Quest ID, Item ID, Panel name, etc.
    public int intValue; // For item quantities
}
```

### DialogueData.cs

```csharp
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Dexiled/Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Dialogue Info")]
    public string dialogueId;
    public string dialogueName;
    public string npcId; // Links to NPC (e.g., "seer", "questgiver")
    
    [Header("Dialogue Tree")]
    public string startNodeId; // Entry point
    public List<DialogueNode> nodes = new List<DialogueNode>();
    
    private Dictionary<string, DialogueNode> nodeLookup;
    
    public DialogueNode GetNode(string nodeId)
    {
        if (nodeLookup == null)
        {
            BuildLookup();
        }
        
        nodeLookup.TryGetValue(nodeId, out var node);
        return node;
    }
    
    public DialogueNode GetStartNode()
    {
        if (string.IsNullOrEmpty(startNodeId))
            return nodes.Count > 0 ? nodes[0] : null;
        
        return GetNode(startNodeId);
    }
    
    private void BuildLookup()
    {
        nodeLookup = new Dictionary<string, DialogueNode>();
        foreach (var node in nodes)
        {
            if (node != null && !string.IsNullOrEmpty(node.nodeId))
            {
                nodeLookup[node.nodeId] = node;
            }
        }
    }
    
    private void OnValidate()
    {
        nodeLookup = null; // Rebuild on next access
    }
}
```

## Step 2: Create DialogueManager (Singleton)

### DialogueManager.cs

```csharp
using System.Collections.Generic;
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
    
    [Header("Current Dialogue State")]
    private DialogueData currentDialogue;
    private DialogueNode currentNode;
    private Stack<DialogueNode> dialogueHistory = new Stack<DialogueNode>();
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (dialogueUI == null)
        {
            dialogueUI = FindFirstObjectByType<DialogueUI>();
        }
    }
    
    /// <summary>
    /// Start a dialogue conversation
    /// </summary>
    public void StartDialogue(DialogueData dialogue)
    {
        if (dialogue == null)
        {
            Debug.LogWarning("[DialogueManager] Cannot start null dialogue.");
            return;
        }
        
        currentDialogue = dialogue;
        dialogueHistory.Clear();
        
        var startNode = dialogue.GetStartNode();
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
            Debug.LogWarning($"[DialogueManager] Node '{node.nodeId}' condition not met. Skipping.");
            return;
        }
        
        currentNode = node;
        
        // Execute onEnter action
        ExecuteAction(node.onEnterAction);
        
        // Show in UI
        if (dialogueUI != null)
        {
            dialogueUI.DisplayNode(node);
        }
        else
        {
            Debug.LogWarning("[DialogueManager] DialogueUI not found! Cannot display dialogue.");
        }
    }
    
    /// <summary>
    /// Select a choice and advance dialogue
    /// </summary>
    public void SelectChoice(int choiceIndex)
    {
        if (currentNode == null || currentNode.choices == null)
            return;
        
        if (choiceIndex < 0 || choiceIndex >= currentNode.choices.Count)
            return;
        
        var choice = currentNode.choices[choiceIndex];
        
        // Check choice condition
        if (!EvaluateCondition(choice.condition))
        {
            Debug.LogWarning($"[DialogueManager] Choice '{choice.choiceText}' condition not met.");
            return;
        }
        
        // Execute choice action
        ExecuteAction(choice.action);
        
        // Execute exit action from current node
        ExecuteAction(currentNode.onExitAction);
        
        // Move to next node
        if (!string.IsNullOrEmpty(choice.targetNodeId))
        {
            var nextNode = currentDialogue?.GetNode(choice.targetNodeId);
            if (nextNode != null)
            {
                dialogueHistory.Push(currentNode);
                ShowNode(nextNode);
            }
            else
            {
                Debug.LogWarning($"[DialogueManager] Target node '{choice.targetNodeId}' not found!");
                EndDialogue();
            }
        }
        else
        {
            EndDialogue();
        }
    }
    
    /// <summary>
    /// Continue to next node (for non-choice nodes)
    /// </summary>
    public void Continue()
    {
        if (currentNode == null)
        {
            EndDialogue();
            return;
        }
        
        // If node has choices, do nothing (wait for choice selection)
        if (currentNode.HasChoices)
            return;
        
        // Execute exit action
        ExecuteAction(currentNode.onExitAction);
        
        // If this is an end node, close dialogue
        if (currentNode.IsEndNode)
        {
            EndDialogue();
        }
    }
    
    /// <summary>
    /// End the current dialogue
    /// </summary>
    public void EndDialogue()
    {
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
        
        // TODO: Implement condition checking
        // - QuestCompleted, QuestActive, LevelRequirement, ItemOwned, Custom
        // For now, return true (always pass)
        return true;
    }
    
    private void ExecuteAction(DialogueAction action)
    {
        if (action == null || action.actionType == DialogueAction.ActionType.None)
            return;
        
        // TODO: Implement action execution
        // - StartQuest, CompleteQuest, GiveItem, RemoveItem, OpenShop, OpenPanel, Custom
        switch (action.actionType)
        {
            case DialogueAction.ActionType.OpenPanel:
                // Open NPC panel (Seer, Forge, etc.)
                OpenNPCPanel(action.actionValue);
                break;
            // Add other action types as needed
        }
    }
    
    private void OpenNPCPanel(string panelName)
    {
        // TODO: Integrate with TownScene NPC panels
        Debug.Log($"[DialogueManager] Opening panel: {panelName}");
        // Example: Find panel by name and show it
    }
    
    public bool IsDialogueActive => currentDialogue != null && currentNode != null;
}
```

## Step 3: Create DialogueUI (Unity Legacy UI)

### DialogueUI.cs

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for displaying dialogue. Uses Unity's legacy UI system (uGUI).
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    
    [Header("Dialogue Display")]
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image speakerPortrait;
    
    [Header("Choices")]
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private GameObject choiceButtonPrefab;
    
    [Header("Navigation")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backButton;
    
    [Header("Settings")]
    [SerializeField] private float typewriterSpeed = 0.05f; // Seconds per character
    [SerializeField] private bool useTypewriterEffect = true;
    
    private DialogueNode currentNode;
    private bool isTyping = false;
    private Coroutine typewriterCoroutine;
    
    private void Awake()
    {
        InitializeUI();
    }
    
    private void Start()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }
    
    private void InitializeUI()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }
    }
    
    private void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseClicked);
        }
        
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackClicked);
        }
    }
    
    public void DisplayNode(DialogueNode node)
    {
        if (node == null)
            return;
        
        currentNode = node;
        
        // Show panel
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 1f;
            panelCanvasGroup.blocksRaycasts = true;
            panelCanvasGroup.interactable = true;
        }
        
        // Update speaker info
        if (speakerNameText != null)
        {
            speakerNameText.text = node.speakerName ?? "Unknown";
        }
        
        if (speakerPortrait != null)
        {
            speakerPortrait.sprite = node.speakerPortrait;
            speakerPortrait.enabled = node.speakerPortrait != null;
        }
        
        // Display dialogue text
        if (dialogueText != null)
        {
            if (useTypewriterEffect && !string.IsNullOrEmpty(node.dialogueText))
            {
                StartTypewriter(node.dialogueText);
            }
            else
            {
                dialogueText.text = node.dialogueText ?? "";
                isTyping = false;
            }
        }
        
        // Display choices or continue button
        if (node.HasChoices)
        {
            ShowChoices(node.choices);
            if (continueButton != null) continueButton.gameObject.SetActive(false);
        }
        else
        {
            ClearChoices();
            if (continueButton != null) continueButton.gameObject.SetActive(node.IsEndNode == false);
        }
        
        // Show/hide back button
        if (backButton != null)
        {
            backButton.gameObject.SetActive(DialogueManager.Instance != null && 
                                           DialogueManager.Instance.dialogueHistory.Count > 0);
        }
    }
    
    private void StartTypewriter(string text)
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }
        
        isTyping = true;
        typewriterCoroutine = StartCoroutine(TypewriterCoroutine(text));
    }
    
    private System.Collections.IEnumerator TypewriterCoroutine(string text)
    {
        if (dialogueText == null)
            yield break;
        
        dialogueText.text = "";
        
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        isTyping = false;
        typewriterCoroutine = null;
    }
    
    private void ShowChoices(List<DialogueChoice> choices)
    {
        ClearChoices();
        
        if (choicesContainer == null || choiceButtonPrefab == null)
            return;
        
        if (choices == null || choices.Count == 0)
            return;
        
        for (int i = 0; i < choices.Count; i++)
        {
            var choice = choices[i];
            
            // Check condition (optional - can be done in DialogueManager too)
            // For now, show all choices
            
            GameObject choiceObj = Instantiate(choiceButtonPrefab, choicesContainer);
            Button choiceButton = choiceObj.GetComponent<Button>();
            TextMeshProUGUI choiceText = choiceObj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (choiceText != null)
            {
                choiceText.text = choice.choiceText;
            }
            
            if (choiceButton != null)
            {
                int choiceIndex = i; // Capture for closure
                choiceButton.onClick.AddListener(() => OnChoiceSelected(choiceIndex));
            }
        }
    }
    
    private void ClearChoices()
    {
        if (choicesContainer == null)
            return;
        
        foreach (Transform child in choicesContainer)
        {
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }
    }
    
    private void OnChoiceSelected(int choiceIndex)
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.SelectChoice(choiceIndex);
        }
    }
    
    private void OnContinueClicked()
    {
        if (isTyping)
        {
            // Skip typewriter effect
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
            }
            if (dialogueText != null && currentNode != null)
            {
                dialogueText.text = currentNode.dialogueText ?? "";
            }
            isTyping = false;
            return;
        }
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.Continue();
        }
    }
    
    private void OnCloseClicked()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.EndDialogue();
        }
    }
    
    private void OnBackClicked()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.GoBack();
        }
    }
    
    public void HideDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.blocksRaycasts = false;
            panelCanvasGroup.interactable = false;
        }
        
        currentNode = null;
        ClearChoices();
        
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
        isTyping = false;
    }
}
```

## Step 4: Create NPCInteractable Component

### NPCInteractable.cs

```csharp
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Component for NPCs that can be clicked to start dialogue.
/// </summary>
public class NPCInteractable : MonoBehaviour, IPointerClickHandler
{
    [Header("NPC Info")]
    [SerializeField] private string npcId;
    [SerializeField] private string npcName;
    [SerializeField] private Sprite npcPortrait;
    
    [Header("Dialogue")]
    [SerializeField] private DialogueData dialogueData;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject hoverIndicator;
    [SerializeField] private bool showHoverEffect = true;
    
    private void Start()
    {
        if (hoverIndicator != null)
        {
            hoverIndicator.SetActive(false);
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        StartDialogue();
    }
    
    public void StartDialogue()
    {
        if (dialogueData == null)
        {
            Debug.LogWarning($"[NPCInteractable] No dialogue data assigned to NPC '{npcName}' ({npcId})");
            return;
        }
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogueData);
        }
        else
        {
            Debug.LogError("[NPCInteractable] DialogueManager.Instance is null! Cannot start dialogue.");
        }
    }
    
    // Optional: Add hover effects
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (showHoverEffect && hoverIndicator != null)
        {
            hoverIndicator.SetActive(true);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverIndicator != null)
        {
            hoverIndicator.SetActive(false);
        }
    }
    
    // Public setters for runtime assignment
    public void SetDialogueData(DialogueData dialogue)
    {
        dialogueData = dialogue;
    }
    
    public string GetNPCId() => npcId;
    public string GetNPCName() => npcName;
}
```

## Step 5: Unity Setup Instructions

### Create Dialogue UI Prefab (Recommended)

**Option 1: Create as Prefab (Recommended for Reusability)**

1. **Create DialoguePanel GameObject in Scene:**
   - Right-click in Hierarchy → Create Empty
   - Rename to `DialoguePanel`
   - Add Component: `DialogueUI`
   - Add Component: `CanvasGroup` (for fade effects)

2. **Build UI Hierarchy:**
   ```
   DialoguePanel (GameObject with DialogueUI + CanvasGroup)
   ├── Background (Image) - Semi-transparent dark overlay
   │   └── RectTransform: Stretch to fill screen
   ├── DialogueContainer (Image + VerticalLayoutGroup)
   │   ├── RectTransform: Center, Width: 800, Height: 400
   │   ├── Header (HorizontalLayoutGroup)
   │   │   ├── SpeakerPortrait (Image) - 128x128
   │   │   └── SpeakerNameText (TextMeshProUGUI)
   │   ├── DialogueText (TextMeshProUGUI) - Main dialogue text area
   │   └── ChoicesContainer (VerticalLayoutGroup) - For choice buttons
   └── NavigationButtons (HorizontalLayoutGroup)
       ├── ContinueButton (Button + TextMeshProUGUI)
       ├── BackButton (Button + TextMeshProUGUI)
       └── CloseButton (Button + TextMeshProUGUI)
   ```

3. **Detailed Setup Steps:**
   
   **Background:**
   - Add `Image` component
   - Color: Black with 0.7 alpha
   - RectTransform: Anchor to stretch (Alt+Shift+Click bottom-right preset)
   - Left/Right/Top/Bottom: 0
   
   **DialogueContainer:**
   - Add `Image` component (panel background)
   - Add `Vertical Layout Group` component
   - RectTransform: Center, Width: 800, Height: 400
   - Padding: 20 all sides
   - Spacing: 15
   
   **Header:**
   - Add `Horizontal Layout Group`
   - Child Alignment: Middle Left
   - Spacing: 15
   - Padding: 10 all sides
   
   **SpeakerPortrait:**
   - Add `Image` component
   - RectTransform: Width: 128, Height: 128
   - Preserve Aspect: ✓
   
   **SpeakerNameText:**
   - Add `TextMeshProUGUI` component
   - Font Size: 24
   - Font Style: Bold
   - Alignment: Middle Left
   
   **DialogueText:**
   - Add `TextMeshProUGUI` component
   - RectTransform: Flexible width/height
   - Font Size: 18
   - Alignment: Top Left
   - Enable Word Wrapping
   
   **ChoicesContainer:**
   - Add `Vertical Layout Group`
   - Spacing: 10
   - Child Alignment: Upper Center
   
   **NavigationButtons:**
   - Add `Horizontal Layout Group`
   - Spacing: 10
   - Child Alignment: Middle Center
   
   **Buttons (Continue/Back/Close):**
   - Add `Button` component
   - Add `Image` component (button background)
   - Add child `TextMeshProUGUI` for button text
   - RectTransform: Width: 120, Height: 40

4. **Wire Up DialogueUI Component:**
   - Select `DialoguePanel` GameObject
   - In `DialogueUI` component inspector:
     - **Dialogue Panel**: Drag `DialoguePanel` GameObject
     - **Panel Canvas Group**: Drag `DialoguePanel` (has CanvasGroup)
     - **Speaker Name Text**: Drag `SpeakerNameText`
     - **Dialogue Text**: Drag `DialogueText`
     - **Speaker Portrait**: Drag `SpeakerPortrait`
     - **Choices Container**: Drag `ChoicesContainer`
     - **Choice Button Prefab**: (see step 5 below)
     - **Continue Button**: Drag `ContinueButton`
     - **Back Button**: Drag `BackButton`
     - **Close Button**: Drag `CloseButton`

5. **Save as Prefab:**
   - Drag `DialoguePanel` from Hierarchy to `Assets/Prefabs/UI/`
   - Name it `DialoguePanel.prefab`
   - You can now delete it from the scene (it's saved as prefab)
   - Instantiate it in scenes where you need dialogue

### Create Choice Button Prefab

1. **Create Choice Button:**
   - Right-click in Hierarchy → UI → Button
   - Rename to `DialogueChoiceButton`
   - Remove default "Text" child (we'll use TextMeshPro)
   - Add child GameObject: Right-click button → UI → Text - TextMeshPro
   - Rename child to `ChoiceText`
   - Set TextMeshProUGUI:
     - Font Size: 16
     - Alignment: Center
     - Auto Size: ✓
     - Text: "Choice Text"

2. **Button Setup:**
   - RectTransform: Width: 600, Height: 50
   - Button Colors:
     - Normal: White
     - Highlighted: Light Gray
     - Pressed: Gray
     - Selected: Light Blue

3. **Save as Prefab:**
   - Drag to `Assets/Prefabs/UI/DialogueChoiceButton.prefab`
   - Assign this prefab to `DialogueUI` → **Choice Button Prefab** field

### Alternative: Create Dialogue UI in Scene (No Prefab)

If you prefer to create the UI directly in each scene:

1. Follow the same hierarchy structure above
2. Make sure it's a child of your main Canvas
3. Wire up all references in `DialogueUI` component
4. The UI will be scene-specific (not reusable)

### Create NPC GameObjects

For each NPC in TownScene:

1. **Create NPC GameObject:**
   - Add `NPCInteractable` component
   - Add `Image` component (for click detection)
   - Set `npcId`, `npcName`, `npcPortrait`
   - Assign `DialogueData` ScriptableObject

2. **Optional Visual Feedback:**
   - Add hover indicator (highlight, glow, etc.)
   - Add NPC sprite/portrait

### Create DialogueData Assets

1. **Create DialogueData ScriptableObject:**
   - Right-click in Project → Create → Dexiled → Dialogue → Dialogue Data
   - Set `dialogueId`, `dialogueName`, `npcId`

2. **Add Dialogue Nodes:**
   - Click "Add Node" or manually add to nodes list
   - Set `nodeId`, `speakerName`, `dialogueText`
   - Add choices if needed
   - Link choices to target nodes

3. **Example Structure:**
   ```
   Start Node (nodeId: "start")
   ├── Choice 1: "Ask about quests" → Quest Node
   ├── Choice 2: "Open shop" → Shop Node (with OpenShop action)
   └── Choice 3: "Goodbye" → End Node
   ```

## Step 6: Integration with TownScene NPCs

### Seer NPC Example

```csharp
// In TownScene, create SeerNPC GameObject
// Add NPCInteractable component:
// - npcId: "seer"
// - npcName: "The Seer"
// - dialogueData: [Assign SeerDialogueData asset]

// In DialogueAction, use:
// - actionType: OpenPanel
// - actionValue: "SeerPanel" or "CardGeneration" or "CardVendor"
```

### Questgiver NPC Example

```csharp
// DialogueAction for starting quest:
// - actionType: StartQuest
// - actionValue: "quest_001" (quest ID)

// DialogueCondition for quest availability:
// - conditionType: QuestCompleted
// - conditionValue: "quest_001"
// - This choice only shows if quest_001 is completed
```

## Step 7: Future Enhancements

### Quest System Integration

Extend `DialogueCondition` and `DialogueAction` to integrate with your quest system:

```csharp
// In DialogueManager.ExecuteAction:
case DialogueAction.ActionType.StartQuest:
    QuestManager.Instance?.StartQuest(action.actionValue);
    break;

case DialogueAction.ActionType.CompleteQuest:
    QuestManager.Instance?.CompleteQuest(action.actionValue);
    break;
```

### Shop Integration

```csharp
case DialogueAction.ActionType.OpenShop:
    ShopManager.Instance?.OpenShop(action.actionValue);
    break;
```

### Save/Load Dialogue State

Track which dialogues have been seen, choices made, etc. for persistent dialogue state.

## Best Practices

1. **Node IDs**: Use descriptive IDs like "start", "quest_intro", "shop_greeting"
2. **Speaker Names**: Keep consistent across nodes for same NPC
3. **Portraits**: Reuse portrait sprites across nodes for same NPC
4. **Branching**: Keep dialogue trees reasonably sized (5-10 nodes per conversation)
5. **Actions**: Use actions for side effects (opening panels, giving items) rather than dialogue text
6. **Conditions**: Use conditions to show/hide choices based on game state

## Testing

1. Create a simple test dialogue with 2-3 nodes
2. Assign to an NPC in TownScene
3. Click NPC to start dialogue
4. Test choices, continue button, back button
5. Test typewriter effect (if enabled)
6. Test dialogue closing

## File Structure

```
Assets/
├── Scripts/
│   └── Dialogue/
│       ├── DialogueData.cs
│       ├── DialogueNode.cs
│       ├── DialogueManager.cs
│       ├── DialogueUI.cs
│       └── NPCInteractable.cs
├── Data/
│   └── Dialogue/
│       ├── SeerDialogue.asset
│       ├── QuestgiverDialogue.asset
│       └── ForgeDialogue.asset
└── Prefabs/
    └── UI/
        └── DialogueChoiceButton.prefab
```

This system is designed to be:
- **Data-driven**: Easy to author dialogue without code changes
- **Extensible**: Easy to add new action/condition types
- **Consistent**: Matches your existing architecture patterns
- **Flexible**: Supports branching, conditions, and actions


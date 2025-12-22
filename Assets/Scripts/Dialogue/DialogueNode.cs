using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueNode
{
    [Header("Content")]
    public string nodeId;
    public string speakerName;
    
    [Header("Dialogue Text")]
    [Tooltip("List of paragraphs to display sequentially. Each paragraph will be shown one at a time.")]
    public List<string> paragraphs = new List<string>();
    
    [Tooltip("Legacy single text field (for backward compatibility). If paragraphs list is empty, this will be used.")]
    [TextArea(3, 8)]
    public string dialogueText;
    
    public Sprite speakerPortrait;
    
    [Header("Choices")]
    public List<DialogueChoice> choices = new List<DialogueChoice>();
    
    [Header("Actions")]
    [Tooltip("Action(s) to execute when this node is entered")]
    public DialogueAction onEnterAction;
    
    [Tooltip("List of actions to execute when this node is entered (alternative to single onEnterAction)")]
    public List<DialogueAction> onEnterActions = new List<DialogueAction>();
    
    [Tooltip("Action(s) to execute when this node is exited")]
    public DialogueAction onExitAction;
    
    [Tooltip("List of actions to execute when this node is exited (alternative to single onExitAction)")]
    public List<DialogueAction> onExitActions = new List<DialogueAction>();
    
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
        DialogueNodeSeen, // Check if a specific dialogue node has been seen
        TutorialCompleted, // Check if a tutorial has been completed (conditionValue = tutorial ID)
        MultipleTutorialsCompleted, // Check if multiple tutorials are completed (conditionValue = comma-separated tutorial IDs)
        Custom
    }
    
    public enum LogicOperator
    {
        AND, // All conditions must be true
        OR   // At least one condition must be true
    }
    
    public ConditionType conditionType;
    public string conditionValue; // Quest ID, Item ID, comma-separated tutorial IDs, etc.
    public int intValue; // For level requirements
    
    [Tooltip("Additional conditions to check (for AND/OR logic). Only used if additionalConditions list is not empty.")]
    public List<DialogueCondition> additionalConditions = new List<DialogueCondition>();
    
    [Tooltip("Logic operator for combining multiple conditions: AND (all must be true) or OR (at least one must be true).")]
    public LogicOperator logicOperator = LogicOperator.AND;
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
        MarkNodeSeen, // Mark a dialogue node as seen (for unlocking conditional choices)
        TransitionScene, // Transition to a different scene (actionValue = scene name)
        StartTutorial, // Start a tutorial sequence (actionValue = tutorial ID)
        CompleteTutorial, // Mark a tutorial as completed (actionValue = tutorial ID)
        Custom
    }
    
    public ActionType actionType;
    public string actionValue; // Quest ID, Item ID, Panel name, etc.
    public int intValue; // For item quantities
}


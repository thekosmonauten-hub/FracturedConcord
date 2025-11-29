using UnityEngine;

/// <summary>
/// Represents a single step in a tutorial sequence.
/// Each step can highlight UI elements, display text, and wait for player interaction.
/// </summary>
[System.Serializable]
public class TutorialStep
{
    [Header("Step Info")]
    [Tooltip("Unique ID for this step (e.g., 'highlight_socket_1')")]
    public string stepId;
    
    [Tooltip("Tutorial text to display for this step")]
    [TextArea(3, 6)]
    public string tutorialText;
    
    [Header("Target Highlighting")]
    [Tooltip("Find target by name/path or component property. Examples:\n" +
             "- 'Socket_Anchor' (GameObject name)\n" +
             "- 'Canvas/Panel/Button' (hierarchical path)\n" +
             "- 'WarrantNodeView:Anchor' (component property - finds WarrantNodeView with NodeId='Anchor')\n\n" +
             "For multiple targets, separate paths with semicolons (;):\n" +
             "- 'WarrantNodeView:Anchor;WarrantNodeView:TopLeft'")]
    public string targetObjectPath;
    
    [Tooltip("Alternative: List of target paths (one per line). If this is populated, it overrides targetObjectPath.")]
    public string[] targetObjectPaths;
    
    [Tooltip("Highlight color (default: yellow with transparency)")]
    public Color highlightColor = new Color(1f, 1f, 0f, 0.3f);
    
    [Tooltip("Show a pulsing animation on the highlight")]
    public bool usePulseAnimation = true;
    
    [Header("Completion")]
    [Tooltip("How to complete this step")]
    public CompletionType completionType = CompletionType.ClickTarget;
    
    [Tooltip("If completionType is ClickTarget, find click target by name/path or component property.\n" +
             "Use same format as Target Object Path (e.g., 'WarrantNodeView:Anchor')")]
    public string clickTargetPath;
    
    [Tooltip("If completionType is WaitForTime, wait this many seconds")]
    public float waitTime = 2f;
    
    [Tooltip("If completionType is WaitForCondition, check this condition (custom implementation)")]
    public string conditionName;
    
    [Header("Actions")]
    [Tooltip("Action to execute when this step starts")]
    public TutorialAction onStartAction;
    
    [Tooltip("Action to execute when this step completes")]
    public TutorialAction onCompleteAction;
    
    public enum CompletionType
    {
        ClickTarget,      // Player must click the target object
        WaitForTime,      // Automatically advance after a delay
        WaitForCondition, // Wait for a custom condition to be met
        Manual            // Manually advance via TutorialManager.NextStep()
    }
}

/// <summary>
/// Actions that can be executed during tutorial steps
/// </summary>
[System.Serializable]
public class TutorialAction
{
    public enum ActionType
    {
        None,
        ShowPanel,        // Show a UI panel (actionValue = panel name/path)
        HidePanel,        // Hide a UI panel
        EnableObject,     // Enable a GameObject
        DisableObject,    // Disable a GameObject
        TransitionScene,  // Transition to a scene (actionValue = scene name)
        StartDialogue,    // Start a dialogue conversation (actionValue = dialogue ID or NPC ID)
        PlaySound,        // Play a sound effect
        TriggerAnimation, // Trigger an animation
        Custom            // Custom action (implement in TutorialManager)
    }
    
    public ActionType actionType;
    public string actionValue; // Panel name, object path, sound name, etc.
    public float floatValue;   // For delays, volumes, etc.
}


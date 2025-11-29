using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject that defines a complete tutorial sequence.
/// Create these assets in Resources/Tutorials/ or similar.
/// </summary>
[CreateAssetMenu(fileName = "New Tutorial", menuName = "Dexiled/Tutorial Data")]
public class TutorialData : ScriptableObject
{
    [Header("Tutorial Info")]
    [Tooltip("Unique ID for this tutorial (e.g., 'warrant_tutorial')")]
    public string tutorialId;
    
    [Tooltip("Display name for this tutorial")]
    public string tutorialName;
    
    [Header("Steps")]
    [Tooltip("List of tutorial steps in order")]
    public List<TutorialStep> steps = new List<TutorialStep>();
    
    [Header("Settings")]
    [Tooltip("Can this tutorial be skipped?")]
    public bool canSkip = false;
    
    [Tooltip("Should tutorial automatically start when scene loads?")]
    public bool autoStart = false;
    
    [Tooltip("Scene name where this tutorial should run (optional, for validation)")]
    public string targetScene;
    
    [Header("Completion")]
    [Tooltip("Action to execute when tutorial completes (e.g., return to dialogue, transition scene)")]
    public TutorialAction onTutorialComplete;
}



using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Dexiled.MazeSystem;

/// <summary>
/// Controller component for CombatScene that handles switching between combat mode and maze mode.
/// 
/// MAZE MODE (exploring/navigating):
/// - Minimap visible (replaces cards in bottom-center)
/// - Cards hidden
/// - Enemies hidden
/// - Player display visible (character, health/mana, resources)
/// - Top bar shows "FLOOR: X/Y"
/// 
/// COMBAT MODE (in battle):
/// - Cards visible (replaces minimap in bottom-center)
/// - Minimap hidden
/// - Enemies visible
/// - Player display visible (same as maze mode)
/// - Top bar shows "WAVE: X/Y"
/// 
/// Automatically switches based on whether a maze run is active and if combat is in progress.
/// </summary>
public class MazeSceneController : MonoBehaviour
{
    [Header("Mode Detection")]
    [Tooltip("Automatically detect maze mode on Start")]
    public bool autoDetectMode = true;
    
    [Header("Combat Mode UI (to hide in maze mode)")]
    [Tooltip("Card hand container/scroll view (Unity UI or UXML)")]
    public GameObject cardHandContainer;
    
    [Tooltip("Enemy display panels (hide in maze mode)")]
    public GameObject[] enemyPanels;
    
    [Tooltip("End Turn button (may hide in maze mode)")]
    public GameObject endTurnButton;
    
    [Tooltip("Wave indicator text (change to Floor in maze mode)")]
    public TextMeshProUGUI waveIndicatorText;
    
    [Header("Maze Mode UI")]
    [Tooltip("Minimap UI component (shown in maze mode)")]
    public MazeMinimapUI minimapUI;
    
    [Tooltip("Minimap container (where cards normally are)")]
    public GameObject minimapContainer;
    
    [Header("Always Visible (player resources)")]
    [Tooltip("Player display (always visible)")]
    public GameObject playerDisplay;
    
    [Tooltip("Player resources (health/mana bars) - always visible")]
    public GameObject playerResourcesPanel;
    
    [Header("Maze Background")]
    [Tooltip("Background image component (Unity UI Image) that displays the room background")]
    public UnityEngine.UI.Image backgroundImage;
    
    [Tooltip("If background image is not assigned, will try to find it automatically")]
    public bool autoFindBackgroundImage = true;
    
    private bool isMazeMode = false;
    private Sprite currentBackground;
    
    private void Start()
    {
        // In MazeScene, check if we need to start a maze run
        if (autoDetectMode)
        {
            // Check if we're in MazeScene (either via encounter or directly)
            bool isMazeScene = IsMazeScene();
            bool isMazeEncounter = IsMazeEncounter();
            
            // If we're in MazeScene (directly or via encounter) and no run is active, start one
            // BUT: Check if we're returning from combat first (has MazeRunId in PlayerPrefs)
            string returningFromCombat = PlayerPrefs.GetString("MazeRunId", "");
            bool isReturningFromCombat = !string.IsNullOrEmpty(returningFromCombat);
            bool shouldHaveActiveRun = PlayerPrefs.GetInt("MazeRunActive", 0) == 1;
            
            if (isMazeScene && (MazeRunManager.Instance == null || !MazeRunManager.Instance.HasActiveRun()))
            {
                if (isReturningFromCombat || shouldHaveActiveRun)
                {
                    // We're returning from combat but the run was lost - this is a serious error
                    Debug.LogError($"[MazeSceneController] Returning from combat but no active run found! RunId: {returningFromCombat}, ShouldHaveActiveRun: {shouldHaveActiveRun}");
                    Debug.LogError("[MazeSceneController] The run should have been preserved! This indicates a problem with DontDestroyOnLoad or the run being cleared.");
                    
                    // Clear the flags since we can't restore
                    PlayerPrefs.DeleteKey("MazeRunId");
                    PlayerPrefs.DeleteKey("MazeRunSeed");
                    PlayerPrefs.DeleteKey("MazeRunActive");
                    PlayerPrefs.Save();
                    
                    Debug.LogError("[MazeSceneController] Cannot continue without run data. Please restart the maze run.");
                    // Don't start a new run automatically - this would hide the problem
                    return;
                }
                
                // Not returning from combat - safe to start a new run
                Debug.Log("[MazeSceneController] MazeScene detected but no active run. Starting new maze run with random seed...");
                
                // Start maze run if manager exists
                if (MazeRunManager.Instance != null)
                {
                    // Generate random seed for testing
                    int randomSeed = UnityEngine.Random.Range(1, int.MaxValue);
                    MazeRunManager.Instance.StartRun(randomSeed);
                    Debug.Log($"[MazeSceneController] Started maze run with seed: {randomSeed}");
                }
                else
                {
                    Debug.LogError("[MazeSceneController] MazeRunManager.Instance is null! Cannot start maze run.");
                }
            }
            else if (isMazeScene && isReturningFromCombat)
            {
                // We have an active run and we're returning from combat - verify it matches
                var currentRun = MazeRunManager.Instance?.GetCurrentRun();
                if (currentRun != null && currentRun.runId == returningFromCombat)
                {
                    Debug.Log($"[MazeSceneController] Successfully returned from combat. Run {currentRun.runId} is still active (seed: {currentRun.seed})");
                    // Clear the flags
                    PlayerPrefs.DeleteKey("MazeRunId");
                    PlayerPrefs.DeleteKey("MazeRunSeed");
                    PlayerPrefs.DeleteKey("MazeRunActive");
                    PlayerPrefs.Save();
                }
                else
                {
                    Debug.LogWarning($"[MazeSceneController] Run ID mismatch! Expected: {returningFromCombat}, Got: {currentRun?.runId ?? "null"}");
                }
            }
            
            // Prevent CombatDisplayManager from auto-starting combat
            PreventAutoCombat();
            
            // Wait a frame for everything to initialize, then switch to maze mode
            StartCoroutine(DelayedModeSwitch());
        }
    }
    
    /// <summary>
    /// Delays mode switching by one frame to ensure all components are initialized.
    /// </summary>
    private IEnumerator DelayedModeSwitch()
    {
        yield return null; // Wait one frame
        
        // Check and switch to maze mode
        CheckAndSwitchMode();
        
        // If we just started a run, refresh the minimap display
        if (MazeRunManager.Instance != null && MazeRunManager.Instance.HasActiveRun() && minimapUI != null)
        {
            var run = MazeRunManager.Instance.GetCurrentRun();
            if (run != null)
            {
                var floor = run.GetCurrentFloor();
                if (floor != null)
                {
                    minimapUI.DisplayFloor(floor);
                    Debug.Log("[MazeSceneController] Minimap refreshed after run start.");
                }
            }
        }
    }
    
    /// <summary>
    /// Checks if we're currently in the MazeScene (by checking the active scene name).
    /// </summary>
    private bool IsMazeScene()
    {
        // Check current scene name
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return currentSceneName == "MazeScene";
    }
    
    /// <summary>
    /// Checks if the current encounter is a maze encounter (sceneName = "MazeScene").
    /// </summary>
    private bool IsMazeEncounter()
    {
        if (EncounterManager.Instance == null)
            return false;
        
        var encounter = EncounterManager.Instance.GetCurrentEncounter();
        if (encounter == null)
            return false;
        
        // Check if scene name is MazeScene
        return encounter.sceneName == "MazeScene";
    }
    
    /// <summary>
    /// Prevents CombatDisplayManager from auto-starting combat in maze mode.
    /// Also disables enemy spawning for maze encounters.
    /// </summary>
    private void PreventAutoCombat()
    {
        CombatDisplayManager combatManager = FindFirstObjectByType<CombatDisplayManager>();
        if (combatManager != null)
        {
            // Disable auto-start combat for maze scenes
            if (IsMazeScene())
            {
                combatManager.autoStartCombat = false;
                combatManager.createTestEnemies = false; // Prevent test enemies from spawning
                Debug.Log("[MazeSceneController] Disabled auto-start combat and test enemies for maze scene.");
            }
        }
    }
    
    /// <summary>
    /// Checks if we should be in maze mode and switches accordingly.
    /// 
    /// Logic:
    /// - If maze run active AND not in combat → Maze Mode (minimap visible, cards hidden)
    /// - If in combat → Combat Mode (cards visible, minimap hidden)
    /// - Otherwise → Combat Mode (default)
    /// </summary>
    public void CheckAndSwitchMode()
    {
        // Check if we're currently in combat (would need combat state check)
        // For now, assume if no active combat, we're in maze mode if run is active
        bool isInCombat = IsInCombat();
        bool shouldBeMazeMode = !isInCombat && 
                                MazeRunManager.Instance != null && 
                                MazeRunManager.Instance.HasActiveRun();
        
        if (shouldBeMazeMode != isMazeMode)
        {
            SwitchToMode(shouldBeMazeMode);
        }
    }
    
    /// <summary>
    /// Checks if combat is currently active.
    /// TODO: Integrate with CombatDisplayManager or CombatState to check actual combat state.
    /// </summary>
    private bool IsInCombat()
    {
        // Check for maze combat context (set when entering combat from maze)
        string mazeContext = PlayerPrefs.GetString("MazeCombatContext", "");
        bool hasMazeContext = !string.IsNullOrEmpty(mazeContext);
        
        // If we have maze context, we might be in combat
        // But we need to check actual combat state
        // For now, we'll check if CombatDisplayManager exists and is active
        CombatDisplayManager combatManager = FindFirstObjectByType<CombatDisplayManager>();
        if (combatManager != null)
        {
            // Check combat state - you'll need to expose this from CombatDisplayManager
            // For now, assume if combat manager exists and is in a combat state, we're in combat
            return hasMazeContext; // Simplified - will need actual state check
        }
        
        return false;
    }
    
    /// <summary>
    /// Switches between combat mode and maze mode.
    /// </summary>
    public void SwitchToMode(bool mazeMode)
    {
        isMazeMode = mazeMode;
        
        if (mazeMode)
        {
            EnableMazeMode();
        }
        else
        {
            EnableCombatMode();
        }
        
        Debug.Log($"[MazeSceneController] Switched to {(mazeMode ? "Maze" : "Combat")} mode");
    }
    
    /// <summary>
    /// Enables maze mode: hide cards/enemies, show minimap, keep player visible.
    /// </summary>
    private void EnableMazeMode()
    {
        // Hide combat UI elements
        if (cardHandContainer != null)
            cardHandContainer.SetActive(false);
        
        if (enemyPanels != null)
        {
            foreach (var panel in enemyPanels)
            {
                if (panel != null)
                    panel.SetActive(false);
            }
        }
        
        if (endTurnButton != null)
            endTurnButton.SetActive(false);
        
        // Reactivate MinimapCanvas when entering maze mode
        GameObject minimapCanvasObj = GameObject.Find("MinimapCanvas");
        if (minimapCanvasObj != null)
        {
            minimapCanvasObj.SetActive(true);
            Debug.Log("[MazeSceneController] Reactivated MinimapCanvas when entering maze mode.");
        }
        else
        {
            // Fallback: find canvas via minimap UI
            if (minimapUI != null)
            {
                Canvas canvas = minimapUI.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    canvas.gameObject.SetActive(true);
                    Debug.Log($"[MazeSceneController] Reactivated minimap Canvas '{canvas.name}' when entering maze mode.");
                }
            }
        }
        
        // Show minimap
        if (minimapContainer != null)
            minimapContainer.SetActive(true);
        
        if (minimapUI != null)
        {
            minimapUI.gameObject.SetActive(true);
            
            // Initialize minimap if run exists
            if (MazeRunManager.Instance != null && MazeRunManager.Instance.HasActiveRun())
            {
                var run = MazeRunManager.Instance.GetCurrentRun();
                if (run != null)
                {
                    var floor = run.GetCurrentFloor();
                    if (floor != null && minimapUI != null)
                    {
                        minimapUI.DisplayFloor(floor);
                    }
                }
            }
        }
        
        // Keep player display visible
        if (playerDisplay != null)
            playerDisplay.SetActive(true);
        
        if (playerResourcesPanel != null)
            playerResourcesPanel.SetActive(true);
        
        // Update top bar text
        if (waveIndicatorText != null && MazeRunManager.Instance != null && MazeRunManager.Instance.HasActiveRun())
        {
            var run = MazeRunManager.Instance.GetCurrentRun();
            if (run != null)
            {
                waveIndicatorText.text = $"FLOOR: {run.currentFloor}/{run.totalFloors}";
            }
        }
        
        // Set initial background when entering maze mode
        SetInitialBackground();
    }
    
    /// <summary>
    /// Enables combat mode: show cards/enemies, hide minimap.
    /// </summary>
    private void EnableCombatMode()
    {
        // Show combat UI elements
        if (cardHandContainer != null)
            cardHandContainer.SetActive(true);
        
        if (enemyPanels != null)
        {
            foreach (var panel in enemyPanels)
            {
                if (panel != null)
                    panel.SetActive(true);
            }
        }
        
        if (endTurnButton != null)
            endTurnButton.SetActive(true);
        
        // Deactivate MinimapCanvas when entering combat mode
        GameObject minimapCanvasObj = GameObject.Find("MinimapCanvas");
        if (minimapCanvasObj != null)
        {
            minimapCanvasObj.SetActive(false);
            Debug.Log("[MazeSceneController] Deactivated MinimapCanvas when entering combat mode.");
        }
        else
        {
            // Fallback: find canvas via minimap UI
            if (minimapUI != null)
            {
                Canvas canvas = minimapUI.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    canvas.gameObject.SetActive(false);
                    Debug.Log($"[MazeSceneController] Deactivated minimap Canvas '{canvas.name}' when entering combat mode.");
                }
            }
        }
        
        // Hide minimap
        if (minimapContainer != null)
            minimapContainer.SetActive(false);
        
        if (minimapUI != null)
            minimapUI.gameObject.SetActive(false);
        
        // Keep player display visible
        if (playerDisplay != null)
            playerDisplay.SetActive(true);
        
        if (playerResourcesPanel != null)
            playerResourcesPanel.SetActive(true);
        
        // Reset top bar text
        if (waveIndicatorText != null)
        {
            waveIndicatorText.text = "WAVE: 1/1"; // Default, will be updated by combat system
        }
    }
    
    /// <summary>
    /// Called when returning from combat to maze.
    /// Refreshes the minimap display.
    /// </summary>
    public void OnReturnFromCombat()
    {
        if (isMazeMode && minimapUI != null && MazeRunManager.Instance != null)
        {
            var run = MazeRunManager.Instance.GetCurrentRun();
            if (run != null)
            {
                var floor = run.GetCurrentFloor();
                if (floor != null)
                {
                    minimapUI.DisplayFloor(floor);
                    CheckAndSwitchMode(); // Ensure we're still in maze mode
                }
            }
        }
    }
    
    private void OnEnable()
    {
        // Subscribe to node entered events for background changes
        if (MazeRunManager.Instance != null)
        {
            MazeRunManager.Instance.OnNodeEntered += OnNodeEntered;
        }
        
        // Re-check mode when scene becomes active (e.g., returning from combat)
        if (autoDetectMode)
        {
            CheckAndSwitchMode();
        }
    }
    
    private void OnDisable()
    {
        // Unsubscribe from events
        if (MazeRunManager.Instance != null)
        {
            MazeRunManager.Instance.OnNodeEntered -= OnNodeEntered;
        }
    }
    
    /// <summary>
    /// Called when player enters a new node (first visit only).
    /// Changes the background image randomly.
    /// Note: This event is only triggered for new visits, not when navigating between completed nodes.
    /// </summary>
    private void OnNodeEntered(MazeNode node)
    {
        if (node == null) return;
        
        // Only change background for first-time visits
        // This event is only triggered when node.MarkVisited() is called for the first time
        ChangeBackgroundForNode(node);
        Debug.Log($"[MazeSceneController] First visit to node {node.nodeId} - background changed");
    }
    
    /// <summary>
    /// Changes the background image based on the current floor/node.
    /// </summary>
    private void ChangeBackgroundForNode(MazeNode node)
    {
        if (backgroundImage == null)
        {
            if (autoFindBackgroundImage)
            {
                // Try to find background image automatically
                backgroundImage = FindFirstObjectByType<UnityEngine.UI.Image>();
                if (backgroundImage != null && backgroundImage.name.ToLower().Contains("background"))
                {
                    Debug.Log("[MazeSceneController] Auto-found background image: " + backgroundImage.name);
                }
                else
                {
                    // Try to find by tag or common names
                    GameObject bgObj = GameObject.FindGameObjectWithTag("Background");
                    if (bgObj != null)
                    {
                        backgroundImage = bgObj.GetComponent<UnityEngine.UI.Image>();
                    }
                }
            }
            
            if (backgroundImage == null)
            {
                Debug.LogWarning("[MazeSceneController] No background image assigned! Backgrounds will not change.");
                return;
            }
        }
        
        // Get random background from config
        if (MazeRunManager.Instance != null && MazeRunManager.Instance.mazeConfig != null)
        {
            var run = MazeRunManager.Instance.GetCurrentRun();
            if (run != null)
            {
                Sprite newBackground = MazeRunManager.Instance.mazeConfig.GetRandomBackgroundForFloor(run.currentFloor);
                if (newBackground != null)
                {
                    backgroundImage.sprite = newBackground;
                    currentBackground = newBackground;
                    Debug.Log($"[MazeSceneController] Changed background to: {newBackground.name} (Floor {run.currentFloor})");
                }
                else
                {
                    Debug.LogWarning("[MazeSceneController] No background images configured in MazeConfig!");
                }
            }
        }
    }
    
    /// <summary>
    /// Sets the initial background when entering maze mode.
    /// </summary>
    private void SetInitialBackground()
    {
        if (MazeRunManager.Instance != null && MazeRunManager.Instance.HasActiveRun())
        {
            var run = MazeRunManager.Instance.GetCurrentRun();
            if (run != null)
            {
                ChangeBackgroundForNode(null); // Will use current floor
            }
        }
    }
}


using UnityEngine;
using UnityEngine.UI;

namespace Dexiled.MazeSystem
{
    /// <summary>
    /// Component to trigger a maze run from an EncounterNode or UI button.
    /// Can be attached to EncounterButton or a standalone button.
    /// 
    /// Option 1: Use with EncounterButton/EncounterData
    /// - Create EncounterDataAsset with sceneName = "MazeScene"
    /// - EncounterManager will load MazeScene
    /// - This component will start the maze run when scene loads
    /// 
    /// Option 2: Direct button (standalone)
    /// - Attach to any button
    /// - Starts maze run and loads MazeScene directly
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class MazeRunEntry : MonoBehaviour
    {
        [Header("Maze Settings")]
        [Tooltip("Optional: Specific seed for reproducible runs. Leave at 0 for random seed.")]
        public int runSeed = 0;
        
        [Tooltip("Optional: Override maze config. Leave null to use default from MazeRunManager.")]
        public MazeConfig overrideConfig;
        
        [Header("Entry Method")]
        [Tooltip("If true, works through EncounterManager using EncounterData.sceneName")]
        public bool useEncounterSystem = false;
        
        [Tooltip("Encounter ID to use (if useEncounterSystem is true). Must have sceneName = 'MazeScene'")]
        public int encounterID = 0;
        
        private Button button;
        
        private void Awake()
        {
            button = GetComponent<Button>();
        }
        
        private void Start()
        {
            if (button != null)
            {
                button.onClick.AddListener(StartMazeRun);
            }
        }
        
        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(StartMazeRun);
            }
        }
        
        /// <summary>
        /// Starts a new maze run. Called by button click or programmatically.
        /// Can work through EncounterManager or directly.
        /// </summary>
        public void StartMazeRun()
        {
            if (MazeRunManager.Instance == null)
            {
                Debug.LogError("[MazeRunEntry] MazeRunManager.Instance is null! Cannot start maze run.");
                return;
            }
            
            // Apply override config if provided
            if (overrideConfig != null)
            {
                MazeRunManager.Instance.mazeConfig = overrideConfig;
            }
            
            if (useEncounterSystem && encounterID > 0)
            {
                // Work through EncounterManager
                // EncounterManager will load MazeScene based on EncounterData.sceneName
                // Then MazeSceneController will detect and start the run
                
                if (EncounterManager.Instance != null)
                {
                    // Initialize maze run before loading scene
                    int seed = runSeed > 0 ? runSeed : 0;
                    MazeRunManager.Instance.StartRun(seed > 0 ? (int?)seed : null);
                    
                    // Start encounter (loads scene from EncounterData.sceneName)
                    EncounterManager.Instance.StartEncounter(encounterID);
                    
                    Debug.Log($"[MazeRunEntry] Starting maze run through EncounterSystem (ID: {encounterID}, Seed: {seed})");
                }
                else
                {
                    Debug.LogError("[MazeRunEntry] EncounterManager.Instance is null! Cannot use encounter system.");
                }
            }
            else
            {
                // Direct approach: start run and load MazeScene
                int seed = runSeed > 0 ? runSeed : 0;
                MazeRunManager.Instance.StartRun(seed > 0 ? (int?)seed : null);
                
                Debug.Log($"[MazeRunEntry] Starting maze run directly (Seed: {seed})");
                // MazeRunManager.StartRun() already loads MazeScene
            }
        }
    }
}


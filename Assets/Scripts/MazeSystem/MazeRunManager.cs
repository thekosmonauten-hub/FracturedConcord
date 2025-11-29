using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Dexiled.MazeSystem
{
    /// <summary>
    /// Singleton manager for maze runs (Ascendancy trials).
    /// Handles run state, floor generation, and node interactions.
    /// </summary>
    public class MazeRunManager : MonoBehaviour
    {
        private static MazeRunManager _instance;
        public static MazeRunManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<MazeRunManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("MazeRunManager");
                        _instance = go.AddComponent<MazeRunManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        [Header("Configuration")]
        [Tooltip("Maze configuration asset (node weights, floor settings, etc.)")]
        public MazeConfig mazeConfig;
        
        [Header("Run State")]
        [SerializeField] private MazeRunData currentRun;
        
        [Header("Scene Settings")]
        [Tooltip("Scene name for maze UI (minimap)")]
        public string mazeSceneName = "MazeScene";
        
        [Tooltip("Scene name for combat (reused from existing system)")]
        public string combatSceneName = "CombatScene";
        
        // Events
        public event System.Action<MazeRunData> OnRunStarted;
        public event System.Action<MazeRunData> OnRunCompleted;
        public event System.Action<MazeRunData> OnRunFailed;
        public event System.Action<MazeFloor> OnFloorGenerated;
        public event System.Action<MazeNode> OnNodeEntered;
        public event System.Action<MazeRunData> OnRunStateChanged;
        
        // Combat context (temporary encounter for combat scene)
        private EncounterData temporaryCombatEncounter;
        
        // Store random state before combat to restore after
        private UnityEngine.Random.State randomStateBeforeCombat;
        
        private void Awake()
        {
            // Singleton pattern
            if (_instance == null)
            {
                _instance = this;
                
                // Ensure GameObject is at root level for DontDestroyOnLoad to work
                if (transform.parent != null)
                {
                    Debug.LogWarning("[MazeRunManager] GameObject is not at root level! Moving to root for DontDestroyOnLoad to work properly.");
                    transform.SetParent(null);
                }
                
                DontDestroyOnLoad(gameObject);
                
                // Load config from Resources if not assigned
                if (mazeConfig == null)
                {
                    mazeConfig = Resources.Load<MazeConfig>("Maze/DefaultMazeConfig");
                    if (mazeConfig == null)
                    {
                        Debug.LogWarning("[MazeRunManager] No maze config found! Using defaults.");
                        CreateDefaultConfig();
                    }
                }
                
                Debug.Log("[MazeRunManager] Singleton initialized. Run will persist across scenes.");
            }
            else if (_instance != this)
            {
                Debug.Log("[MazeRunManager] Duplicate instance detected. Destroying duplicate.");
                Destroy(gameObject);
            }
            else
            {
                // This is the existing instance - verify the run is still active
                Debug.Log($"[MazeRunManager] Existing instance found. HasActiveRun: {HasActiveRun()}, CurrentRun: {currentRun?.runId ?? "null"}");
            }
        }
        
        private void CreateDefaultConfig()
        {
            // Create a default config at runtime if none exists
            mazeConfig = ScriptableObject.CreateInstance<MazeConfig>();
            mazeConfig.totalFloors = 8;
            mazeConfig.minNodesPerFloor = 12;
            mazeConfig.maxNodesPerFloor = 16;
            mazeConfig.gridSize = new Vector2Int(4, 4);
            mazeConfig.bossFloors = new List<int> { 2, 4, 6 };
        }
        
        #region Run Management
        
        /// <summary>
        /// Starts a new maze run.
        /// </summary>
        public void StartRun(int? seed = null)
        {
            // Prevent starting a new run if one is already active
            if (currentRun != null && currentRun.isActive)
            {
                Debug.LogWarning($"[MazeRunManager] Cannot start new run: run {currentRun.runId} is already active (Seed: {currentRun.seed}). End the current run first.");
                return;
            }
            
            // Generate seed if not provided
            // IMPORTANT: Seed is only set once when run starts, and persists until run ends
            int runSeed = seed ?? UnityEngine.Random.Range(1, 1000000);
            UnityEngine.Random.InitState(runSeed);
            
            // Create run data
            int totalFloors = mazeConfig != null ? mazeConfig.totalFloors : 8;
            currentRun = new MazeRunData(runSeed, totalFloors);
            currentRun.isActive = true;
            
            Debug.Log($"[MazeRunManager] Starting new maze run (Seed: {runSeed}, Floors: {totalFloors})");
            Debug.Log($"[MazeRunManager] Seed {runSeed} will persist until run ends (victory/defeat/exit)");
            
            // Generate first floor
            GenerateFloor(1);
            
            OnRunStarted?.Invoke(currentRun);
            
            // Only load maze scene if we're not already in it
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (currentSceneName != mazeSceneName)
            {
                Debug.Log($"[MazeRunManager] Loading maze scene (currently in {currentSceneName})");
                SceneManager.LoadScene(mazeSceneName);
            }
            else
            {
                Debug.Log($"[MazeRunManager] Already in {mazeSceneName}, skipping scene load. MazeSceneController will handle UI initialization.");
            }
        }
        
        /// <summary>
        /// Gets the current active run.
        /// </summary>
        public MazeRunData GetCurrentRun()
        {
            return currentRun;
        }
        
        /// <summary>
        /// Checks if there's an active run.
        /// </summary>
        public bool HasActiveRun()
        {
            return currentRun != null && currentRun.isActive;
        }
        
        /// <summary>
        /// Completes the current run.
        /// </summary>
        public void CompleteRun()
        {
            if (currentRun != null)
            {
                Debug.Log($"[MazeRunManager] Completing run {currentRun.runId} (Seed: {currentRun.seed}). Seed will be cleared and new seed will be rolled on next StartRun().");
                currentRun.isActive = false;
                currentRun.isCompleted = true;
                
                // Check and apply first clear bonus
                ApplyFirstClearBonus();
                
                OnRunCompleted?.Invoke(currentRun);
                
                // Clear the current run so a new one can be started
                currentRun = null;
                Debug.Log("[MazeRunManager] Run completed! Ready for new run.");
            }
        }
        
        /// <summary>
        /// Applies first clear bonus if this is the first time completing this difficulty.
        /// </summary>
        private void ApplyFirstClearBonus()
        {
            // Get difficulty name from PlayerPrefs (set by MazeHubController)
            string difficultyName = PlayerPrefs.GetString("MazeDifficulty_Name", "");
            if (string.IsNullOrEmpty(difficultyName))
            {
                Debug.Log("[MazeRunManager] No difficulty name found. First clear bonus not applied.");
                return;
            }
            
            // Check if this difficulty has been cleared before
            string clearedKey = $"MazeDifficulty_Cleared_{difficultyName}";
            bool hasBeenCleared = PlayerPrefs.GetInt(clearedKey, 0) == 1;
            
            if (hasBeenCleared)
            {
                Debug.Log($"[MazeRunManager] Difficulty '{difficultyName}' already cleared. No first clear bonus.");
                return;
            }
            
            // Mark as cleared
            PlayerPrefs.SetInt(clearedKey, 1);
            PlayerPrefs.Save();
            
            // Get difficulty config to access bonus values
            // We need to find the config - for now, we'll use PlayerPrefs to store bonus values
            // Alternatively, we could store a reference to the config in MazeRunData
            int attunementPoints = PlayerPrefs.GetInt($"MazeDifficulty_FirstClear_Attunement_{difficultyName}", 0);
            int currencyBonus = PlayerPrefs.GetInt($"MazeDifficulty_FirstClear_Currency_{difficultyName}", 0);
            string currencyTypeStr = PlayerPrefs.GetString($"MazeDifficulty_FirstClear_CurrencyType_{difficultyName}", "");
            
            if (attunementPoints > 0)
            {
                // Award Attunement points (Ascendancy points)
                AwardAttunementPoints(attunementPoints);
                Debug.Log($"[MazeRunManager] First clear bonus: Awarded {attunementPoints} Attunement points!");
            }
            
            if (currencyBonus > 0 && !string.IsNullOrEmpty(currencyTypeStr))
            {
                // Award currency bonus
                if (System.Enum.TryParse<Dexiled.Data.Items.CurrencyType>(currencyTypeStr, out var currencyType))
                {
                    AwardMazeCurrency(currencyType, currencyBonus);
                    Debug.Log($"[MazeRunManager] First clear bonus: Awarded {currencyBonus} {currencyType}!");
                }
            }
        }
        
        /// <summary>
        /// Awards Attunement points (Ascendancy points) to the character.
        /// </summary>
        private void AwardAttunementPoints(int points)
        {
            // Try to get CharacterAscendancyProgress from CharacterManager
            // Note: This assumes CharacterAscendancyProgress is stored somewhere accessible
            // If it's stored differently, adjust this method accordingly
            
            var characterManager = CharacterManager.Instance;
            if (characterManager != null && characterManager.HasCharacter())
            {
                var character = characterManager.GetCurrentCharacter();
                
                // Try to get or create Ascendancy progress
                // For now, we'll store it in PlayerPrefs and let the Ascendancy system read it
                string key = $"Character_{character.characterName}_AscendancyPoints";
                int currentPoints = PlayerPrefs.GetInt(key, 0);
                PlayerPrefs.SetInt(key, currentPoints + points);
                PlayerPrefs.Save();
                
                Debug.Log($"[MazeRunManager] Awarded {points} Attunement points to {character.characterName} (Total: {currentPoints + points})");
            }
            else
            {
                Debug.LogWarning("[MazeRunManager] Cannot award Attunement points: No character loaded!");
            }
        }
        
        /// <summary>
        /// Awards maze currency to the character.
        /// </summary>
        private void AwardMazeCurrency(Dexiled.Data.Items.CurrencyType currencyType, int amount)
        {
            var currencyManager = MazeCurrencyManager.Instance;
            if (currencyManager != null)
            {
                currencyManager.AddCurrency(currencyType, amount);
            }
            else
            {
                Debug.LogWarning("[MazeRunManager] Cannot award maze currency: MazeCurrencyManager not found!");
            }
        }
        
        /// <summary>
        /// Fails the current run.
        /// </summary>
        public void FailRun()
        {
            if (currentRun != null)
            {
                Debug.Log($"[MazeRunManager] Failing run {currentRun.runId} (Seed: {currentRun.seed}). Seed will be cleared and new seed will be rolled on next StartRun().");
                currentRun.isActive = false;
                currentRun.isFailed = true;
                OnRunFailed?.Invoke(currentRun);
                
                // Clear the current run so a new one can be started
                currentRun = null;
                Debug.Log("[MazeRunManager] Run failed! Ready for new run.");
            }
        }
        
        /// <summary>
        /// Abandons the current run (player exits maze).
        /// </summary>
        public void AbandonRun()
        {
            if (currentRun != null && currentRun.isActive)
            {
                Debug.Log($"[MazeRunManager] Abandoning run {currentRun.runId} (Seed: {currentRun.seed}). Seed will be cleared and new seed will be rolled on next StartRun().");
                currentRun.isActive = false;
                
                // Clear the current run so a new one can be started
                currentRun = null;
                Debug.Log("[MazeRunManager] Run abandoned! Ready for new run.");
            }
        }
        
        #endregion
        
        #region Floor Generation
        
        /// <summary>
        /// Generates a floor with 12-16 nodes and ensures connectivity.
        /// </summary>
        public MazeFloor GenerateFloor(int floorNumber)
        {
            if (mazeConfig == null)
            {
                Debug.LogError("[MazeRunManager] Cannot generate floor: mazeConfig is null!");
                return null;
            }
            
            MazeFloor floor = new MazeFloor(floorNumber);
            
            // Determine node count
            int nodeCount = UnityEngine.Random.Range(mazeConfig.minNodesPerFloor, mazeConfig.maxNodesPerFloor + 1);
            Vector2Int gridSize = mazeConfig.gridSize;
            int maxCells = gridSize.x * gridSize.y;
            
            if (nodeCount > maxCells)
            {
                nodeCount = maxCells;
                Debug.LogWarning($"[MazeRunManager] Node count ({nodeCount}) exceeds grid capacity ({maxCells}). Capping.");
            }
            
            // Generate nodes until we have valid connectivity
            int attempts = 0;
            bool valid = false;
            
            while (!valid && attempts < 10)
            {
                floor = new MazeFloor(floorNumber);
                GenerateNodes(floor, nodeCount, gridSize);
                
                // Enforce Start and Stairs nodes
                EnsureStartAndStairs(floor, gridSize);
                
                // Validate connectivity
                valid = floor.ValidateConnectivity();
                attempts++;
            }
            
            if (!valid)
            {
                Debug.LogError($"[MazeRunManager] Failed to generate valid floor after {attempts} attempts!");
                return null;
            }
            
            // Place player at start
            floor.SetPlayerPosition(floor.startPosition);
            
            // Ensure start node exists and is properly initialized
            var startNode = floor.GetNode(floor.startPosition);
            if (startNode == null)
            {
                Debug.LogError($"[MazeRunManager] Start node at {floor.startPosition} not found! Creating fallback.");
                // Create start node if missing
                startNode = new MazeNode($"{floor.floorNumber}_start", MazeNodeType.Start, floor.startPosition);
                floor.AddNode(startNode);
            }
            
            // Mark start node as visited and revealed
            startNode.Reveal();
            startNode.MarkVisited();
            startNode.SetSelectable(false); // Visited nodes aren't selectable
            
            // Reveal only adjacent nodes on paths to stairs (not branches/dead ends)
            floor.RevealAdjacentNodes(floor.startPosition);
            
            // Update selectable nodes to show only path nodes
            floor.UpdateSelectableNodes();
            
            Debug.Log($"[MazeRunManager] Start node initialized at {floor.startPosition}. Node type: {startNode.nodeType}, Visited: {startNode.visited}, Revealed: {startNode.revealed}");
            
            // Add boss node before stairs if needed
            if (mazeConfig.ShouldSpawnBoss(floorNumber))
            {
                AddBossNodeBeforeStairs(floor);
            }
            
            // Add to run
            if (currentRun != null)
            {
                currentRun.floors.Add(floor);
            }
            
            OnFloorGenerated?.Invoke(floor);
            Debug.Log($"[MazeRunManager] Generated floor {floorNumber} with {floor.nodes.Count} nodes (Start: {floor.startPosition}, Stairs: {floor.stairsPosition})");
            
            return floor;
        }
        
        /// <summary>
        /// Generates random nodes on the floor grid, ensuring connectivity.
        /// Uses a spanning tree approach: start with one node, then add adjacent nodes.
        /// </summary>
        private void GenerateNodes(MazeFloor floor, int nodeCount, Vector2Int gridSize)
        {
            HashSet<Vector2Int> usedPositions = new HashSet<Vector2Int>();
            HashSet<Vector2Int> connectedPositions = new HashSet<Vector2Int>();
            List<Vector2Int> frontier = new List<Vector2Int>(); // Positions we can expand from
            
            // Start with a random position
            Vector2Int startPos = new Vector2Int(
                UnityEngine.Random.Range(0, gridSize.x),
                UnityEngine.Random.Range(0, gridSize.y)
            );
            
            usedPositions.Add(startPos);
            connectedPositions.Add(startPos);
            frontier.Add(startPos);
            
            // Create first node
            MazeNodeType nodeType = RollNodeType();
            MazeNode firstNode = new MazeNode($"{floor.floorNumber}_{startPos.x}_{startPos.y}", nodeType, startPos);
            SetupNodeData(firstNode, floor.floorNumber);
            floor.AddNode(firstNode);
            
            Vector2Int[] directions = {
                new Vector2Int(0, 1),   // Up
                new Vector2Int(0, -1),  // Down
                new Vector2Int(1, 0),    // Right
                new Vector2Int(-1, 0)    // Left
            };
            
            // Generate remaining nodes ensuring connectivity
            int nodesGenerated = 1;
            while (nodesGenerated < nodeCount && frontier.Count > 0)
            {
                // Pick a random position from frontier to expand from
                int frontierIndex = UnityEngine.Random.Range(0, frontier.Count);
                Vector2Int expandFrom = frontier[frontierIndex];
                
                // Try to find an adjacent position that's not used
                List<Vector2Int> candidates = new List<Vector2Int>();
                foreach (var dir in directions)
                {
                    Vector2Int candidate = expandFrom + dir;
                    if (candidate.x >= 0 && candidate.x < gridSize.x &&
                        candidate.y >= 0 && candidate.y < gridSize.y &&
                        !usedPositions.Contains(candidate))
                    {
                        candidates.Add(candidate);
                    }
                }
                
                if (candidates.Count > 0)
                {
                    // Pick a random candidate
                    Vector2Int newPos = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                    usedPositions.Add(newPos);
                    connectedPositions.Add(newPos);
                    frontier.Add(newPos);
                    
                    // Create node
                    nodeType = RollNodeType();
                    MazeNode node = new MazeNode($"{floor.floorNumber}_{newPos.x}_{newPos.y}", nodeType, newPos);
                    SetupNodeData(node, floor.floorNumber);
                    floor.AddNode(node);
                    
                    nodesGenerated++;
                }
                else
                {
                    // No valid adjacent positions from this frontier point, remove it
                    frontier.RemoveAt(frontierIndex);
                }
            }
            
            // If we haven't generated enough nodes, fill remaining with random positions
            // (they might not be connected, but we'll validate connectivity later)
            while (nodesGenerated < nodeCount)
            {
                Vector2Int pos = new Vector2Int(
                    UnityEngine.Random.Range(0, gridSize.x),
                    UnityEngine.Random.Range(0, gridSize.y)
                );
                
                if (!usedPositions.Contains(pos))
                {
                    usedPositions.Add(pos);
                    nodesGenerated++;
                    
                    nodeType = RollNodeType();
                    MazeNode node = new MazeNode($"{floor.floorNumber}_{pos.x}_{pos.y}", nodeType, pos);
                    SetupNodeData(node, floor.floorNumber);
                    floor.AddNode(node);
                }
            }
            
            Debug.Log($"[MazeRunManager] Generated {nodesGenerated} nodes ({connectedPositions.Count} connected, {nodesGenerated - connectedPositions.Count} additional)");
        }
        
        /// <summary>
        /// Ensures Start and Stairs nodes exist and are far apart.
        /// </summary>
        private void EnsureStartAndStairs(MazeFloor floor, Vector2Int gridSize)
        {
            var nodes = floor.nodes.Values.ToList();
            if (nodes.Count < 2)
            {
                Debug.LogError("[MazeRunManager] Not enough nodes to place Start and Stairs!");
                return;
            }
            
            // Remove any existing Start/Stairs nodes
            var nodesToRemove = nodes.Where(n => n.nodeType == MazeNodeType.Start || n.nodeType == MazeNodeType.Stairs).ToList();
            foreach (var node in nodesToRemove)
            {
                floor.nodes.Remove(node.position);
                floor.nodePositions.Remove(node.position);
            }
            
            // Pick Start position (prefer edge/corner)
            var startNode = nodes[0];
            startNode.nodeType = MazeNodeType.Start;
            floor.startPosition = startNode.position;
            floor.nodes[startNode.position] = startNode;
            
            // Pick Stairs position (as far from start as possible)
            MazeNode stairsNode = null;
            float maxDistance = 0f;
            
            foreach (var node in nodes)
            {
                if (node == startNode) continue;
                
                float distance = Vector2Int.Distance(startNode.position, node.position);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    stairsNode = node;
                }
            }
            
            if (stairsNode != null)
            {
                stairsNode.nodeType = MazeNodeType.Stairs;
                floor.stairsPosition = stairsNode.position;
                floor.nodes[stairsNode.position] = stairsNode;
            }
            else
            {
                // Fallback: create stairs at opposite corner
                Vector2Int stairsPos = new Vector2Int(
                    gridSize.x - 1 - startNode.position.x,
                    gridSize.y - 1 - startNode.position.y
                );
                
                if (!floor.nodes.ContainsKey(stairsPos))
                {
                    stairsNode = new MazeNode($"{floor.floorNumber}_stairs", MazeNodeType.Stairs, stairsPos);
                    floor.AddNode(stairsNode);
                }
                else
                {
                    stairsNode = floor.nodes[stairsPos];
                    stairsNode.nodeType = MazeNodeType.Stairs;
                }
                
                floor.stairsPosition = stairsPos;
            }
        }
        
        /// <summary>
        /// Adds a boss node immediately before the stairs.
        /// </summary>
        private void AddBossNodeBeforeStairs(MazeFloor floor)
        {
            // Find position adjacent to stairs
            Vector2Int stairsPos = floor.stairsPosition;
            Vector2Int[] directions = {
                new Vector2Int(0, 1),
                new Vector2Int(0, -1),
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0)
            };
            
            foreach (var dir in directions)
            {
                Vector2Int bossPos = stairsPos + dir;
                var existingNode = floor.GetNode(bossPos);
                
                if (existingNode != null && existingNode.nodeType != MazeNodeType.Start)
                {
                    // Convert this node to boss
                    existingNode.nodeType = MazeNodeType.Boss;
                    floor.bossPosition = bossPos;
                    
                    // Set boss data
                    if (existingNode.nodeData == null)
                        existingNode.nodeData = new MazeNodeData();
                    existingNode.nodeData.retreatThreshold = mazeConfig != null ? mazeConfig.bossRetreatThreshold : 0.45f;
                    
                    break;
                }
                else if (existingNode == null)
                {
                    // Create new boss node
                    MazeNode bossNode = new MazeNode($"{floor.floorNumber}_boss", MazeNodeType.Boss, bossPos);
                    bossNode.nodeData = new MazeNodeData();
                    bossNode.nodeData.retreatThreshold = mazeConfig != null ? mazeConfig.bossRetreatThreshold : 0.45f;
                    floor.AddNode(bossNode);
                    floor.bossPosition = bossPos;
                    break;
                }
            }
        }
        
        /// <summary>
        /// Rolls a random node type based on weights.
        /// </summary>
        private MazeNodeType RollNodeType()
        {
            if (mazeConfig == null)
                return MazeNodeType.Combat;
            
            int totalWeight = mazeConfig.GetTotalNodeWeight();
            int roll = UnityEngine.Random.Range(0, totalWeight);
            
            if (roll < mazeConfig.combatWeight)
                return MazeNodeType.Combat;
            roll -= mazeConfig.combatWeight;
            
            if (roll < mazeConfig.chestWeight)
                return MazeNodeType.Chest;
            roll -= mazeConfig.chestWeight;
            
            if (roll < mazeConfig.shrineWeight)
                return MazeNodeType.Shrine;
            roll -= mazeConfig.shrineWeight;
            
            if (roll < mazeConfig.trapWeight)
                return MazeNodeType.Trap;
            roll -= mazeConfig.trapWeight;
            
            if (roll < mazeConfig.forgeWeight)
                return MazeNodeType.Forge;
            
            return MazeNodeType.Special;
        }
        
        /// <summary>
        /// Sets up node-specific data based on type and floor.
        /// </summary>
        private void SetupNodeData(MazeNode node, int floorNumber)
        {
            if (node.nodeData == null)
                node.nodeData = new MazeNodeData();
            
            switch (node.nodeType)
            {
                case MazeNodeType.Combat:
                    node.nodeData.enemyTier = mazeConfig != null ? 
                        mazeConfig.GetEnemyTierForFloor(floorNumber) : EnemyTier.Normal;
                    node.nodeData.enemyCount = UnityEngine.Random.Range(2, 4);
                    break;
                
                case MazeNodeType.Shrine:
                    string[] shrineTypes = { "Cohesion", "Echoes", "Anchored Form", "Sealwrights" };
                    node.nodeData.shrineType = shrineTypes[UnityEngine.Random.Range(0, shrineTypes.Length)];
                    node.nodeData.buffStrength = 1f;
                    break;
                
                case MazeNodeType.Chest:
                    node.nodeData.lootTier = floorNumber;
                    break;
                
                case MazeNodeType.Trap:
                    node.nodeData.trapType = "Entropy";
                    node.nodeData.trapDifficulty = 10 + floorNumber;
                    break;
            }
        }
        
        #endregion
        
        #region Node Interactions
        
        /// <summary>
        /// Called when player clicks/selects a node.
        /// </summary>
        public void SelectNode(Vector2Int nodePosition)
        {
            if (currentRun == null || !currentRun.isActive)
            {
                Debug.LogWarning("[MazeRunManager] Cannot select node: no active run!");
                return;
            }
            
            MazeFloor currentFloor = currentRun.GetCurrentFloor();
            if (currentFloor == null)
            {
                Debug.LogWarning("[MazeRunManager] Cannot select node: no current floor!");
                return;
            }
            
            MazeNode node = currentFloor.GetNode(nodePosition);
            if (node == null)
            {
                Debug.LogWarning($"[MazeRunManager] Node at {nodePosition} not found!");
                return;
            }
            
            // Validate adjacency
            Vector2Int playerPos = currentFloor.playerPosition;
            int distance = Mathf.Abs(nodePosition.x - playerPos.x) + Mathf.Abs(nodePosition.y - playerPos.y);
            
            if (distance != 1)
            {
                Debug.LogWarning($"[MazeRunManager] Node at {nodePosition} is not adjacent to player at {playerPos}!");
                return;
            }
            
            // Check if this is a new visit or navigation between completed nodes
            bool isNewVisit = !node.visited;
            
            // Move player to node
            currentFloor.SetPlayerPosition(nodePosition);
            
            // Only mark as visited and handle node interaction if this is a new visit
            if (isNewVisit)
            {
                node.MarkVisited();
                node.Reveal(); // Ensure visited node is revealed
                
                // Reveal only adjacent nodes on paths to stairs (not branches/dead ends)
                currentFloor.RevealAdjacentNodes(nodePosition);
                
                // Update selectable nodes - allows free navigation between visited nodes
                currentFloor.UpdateSelectableNodes();
                
                // Only trigger node entered event and interaction for new visits
                // This prevents background changes and duplicate interactions when navigating between completed nodes
                OnNodeEntered?.Invoke(node);
                OnRunStateChanged?.Invoke(currentRun);
                
                // Handle node type (only for first visit)
                HandleNodeInteraction(node);
            }
            else
            {
                // Just updating navigation between completed nodes
                // Update selectable nodes to reflect new position
                currentFloor.UpdateSelectableNodes();
                
                // Trigger state change (for UI updates) but NOT node entered (to avoid background changes)
                OnRunStateChanged?.Invoke(currentRun);
                
                Debug.Log($"[MazeRunManager] Navigated to already-visited node at {nodePosition} (no background change)");
            }
        }
        
        /// <summary>
        /// Handles different node types (combat, shrine, chest, etc.).
        /// </summary>
        private void HandleNodeInteraction(MazeNode node)
        {
            switch (node.nodeType)
            {
                case MazeNodeType.Combat:
                    StartCombat(node);
                    break;
                
                case MazeNodeType.Chest:
                    GiveChestLoot(node);
                    break;
                
                case MazeNodeType.Shrine:
                    ApplyShrineBuff(node);
                    break;
                
                case MazeNodeType.Boss:
                    StartBossCombat(node);
                    break;
                
                case MazeNodeType.Stairs:
                    HandleStairs(node);
                    break;
                
                case MazeNodeType.Trap:
                    TriggerTrap(node);
                    break;
                
                case MazeNodeType.Forge:
                    OpenForge(node);
                    break;
                
                case MazeNodeType.Special:
                    HandleSpecialNode(node);
                    break;
                
                default:
                    Debug.Log($"[MazeRunManager] Node type {node.nodeType} handled (no action needed).");
                    break;
            }
        }
        
        /// <summary>
        /// Starts a combat encounter for a combat node.
        /// </summary>
        private void StartCombat(MazeNode node)
        {
            if (currentRun == null || !currentRun.isActive)
            {
                Debug.LogError($"[MazeRunManager] Cannot start combat: no active run! RunId: {currentRun?.runId ?? "null"}");
                return;
            }
            
            Debug.Log($"[MazeRunManager] Starting combat at node {node.nodeId} (RunId: {currentRun.runId}, Seed: {currentRun.seed})");
            
            // Save current random state before entering combat
            // This ensures the maze generation stays consistent when returning
            randomStateBeforeCombat = UnityEngine.Random.state;
            Debug.Log($"[MazeRunManager] Saved random state before combat (seed: {currentRun.seed})");
            
            // Create temporary encounter data for combat scene
            // This allows CombatSceneManager to work with maze combat
            temporaryCombatEncounter = new EncounterData(-1, "Maze Combat", combatSceneName);
            temporaryCombatEncounter.areaLevel = currentRun.currentFloor;
            
            // Store maze context for combat scene
            PlayerPrefs.SetString("MazeCombatContext", node.nodeId);
            PlayerPrefs.SetString("MazeRunId", currentRun.runId);
            PlayerPrefs.SetInt("MazeRunSeed", currentRun.seed); // Store seed for verification
            PlayerPrefs.SetInt("MazeRunActive", 1); // Flag to indicate run should be active
            PlayerPrefs.SetInt("MazeCombatFloor", currentRun.currentFloor); // Store floor for background randomization in combat scene
            PlayerPrefs.Save();
            
            Debug.Log($"[MazeRunManager] Stored maze context. RunId: {currentRun.runId}, Seed: {currentRun.seed}, Floor: {currentRun.currentFloor}");
            
            // Hide minimap canvas before transitioning to combat
            // Try multiple methods to find and deactivate the minimap canvas
            bool minimapCanvasHidden = false;
            
            // Method 1: Find MinimapCanvas by name
            GameObject minimapCanvasObj = GameObject.Find("MinimapCanvas");
            if (minimapCanvasObj != null)
            {
                minimapCanvasObj.SetActive(false);
                minimapCanvasHidden = true;
                Debug.Log("[MazeRunManager] Deactivated MinimapCanvas by name before combat transition.");
            }
            else
            {
                // Method 2: Find via MazeSceneController
                var mazeSceneController = FindFirstObjectByType<MazeSceneController>();
                if (mazeSceneController != null)
                {
                    mazeSceneController.SwitchToMode(false); // Switch to combat mode (hides minimap)
                    minimapCanvasHidden = true;
                    Debug.Log("[MazeRunManager] Hid minimap via MazeSceneController before combat transition.");
                }
                else
                {
                    // Method 3: Find via MazeMinimapUI's parent Canvas
                    var minimapUI = FindFirstObjectByType<MazeMinimapUI>();
                    if (minimapUI != null)
                    {
                        // Find the Canvas in the parent hierarchy
                        Canvas canvas = minimapUI.GetComponentInParent<Canvas>();
                        if (canvas != null)
                        {
                            canvas.gameObject.SetActive(false);
                            minimapCanvasHidden = true;
                            Debug.Log($"[MazeRunManager] Deactivated minimap Canvas '{canvas.name}' via MazeMinimapUI before combat transition.");
                        }
                        else
                        {
                            // Fallback: hide the minimap UI GameObject itself
                            minimapUI.gameObject.SetActive(false);
                            Debug.Log("[MazeRunManager] Hid minimap UI GameObject directly before combat transition.");
                        }
                    }
                }
            }
            
            if (!minimapCanvasHidden)
            {
                Debug.LogWarning("[MazeRunManager] Could not find MinimapCanvas to deactivate before combat transition.");
            }
            
            // Load combat scene
            SceneManager.LoadScene(combatSceneName);
        }
        
        /// <summary>
        /// Starts a boss combat encounter.
        /// </summary>
        private void StartBossCombat(MazeNode node)
        {
            Debug.Log($"[MazeRunManager] Starting boss combat at node {node.nodeId}");
            
            // Save current random state before entering combat
            randomStateBeforeCombat = UnityEngine.Random.state;
            Debug.Log($"[MazeRunManager] Saved random state before boss combat (seed: {currentRun.seed})");
            
            temporaryCombatEncounter = new EncounterData(-2, "Maze Boss", combatSceneName);
            temporaryCombatEncounter.areaLevel = currentRun.currentFloor;
            
            // Mark boss as encountered
            currentRun.MarkBossEncountered(currentRun.currentFloor);
            
            PlayerPrefs.SetString("MazeCombatContext", node.nodeId);
            PlayerPrefs.SetString("MazeRunId", currentRun.runId);
            PlayerPrefs.SetString("MazeBossCombat", "true");
            PlayerPrefs.SetInt("MazeRunSeed", currentRun.seed); // Store seed for verification
            PlayerPrefs.Save();
            
            SceneManager.LoadScene(combatSceneName);
        }
        
        /// <summary>
        /// Handles stairs node (boss encounter if first time, then next floor).
        /// </summary>
        private void HandleStairs(MazeNode node)
        {
            MazeFloor floor = currentRun.GetCurrentFloor();
            
            // Check if boss should spawn (first time reaching stairs)
            if (mazeConfig != null && mazeConfig.ShouldSpawnBoss(currentRun.currentFloor))
            {
                if (floor.bossPosition.HasValue)
                {
                    var bossNode = floor.GetNode(floor.bossPosition.Value);
                    if (bossNode != null && !bossNode.visited && !currentRun.WasBossEncountered(currentRun.currentFloor))
                    {
                        Debug.Log("[MazeRunManager] Boss should spawn before stairs! Boss node should have been visited first.");
                    }
                }
            }
            
            // Move to next floor
            if (currentRun.currentFloor < currentRun.totalFloors)
            {
                currentRun.currentFloor++;
                GenerateFloor(currentRun.currentFloor);
                Debug.Log($"[MazeRunManager] Moved to floor {currentRun.currentFloor}");
            }
            else
            {
                // Final floor - start final boss
                Debug.Log("[MazeRunManager] Final floor reached! Starting final boss...");
                // TODO: Final boss logic
            }
        }
        
        /// <summary>
        /// Gives loot from a chest node.
        /// </summary>
        private void GiveChestLoot(MazeNode node)
        {
            Debug.Log($"[MazeRunManager] Opening chest at node {node.nodeId}");
            // TODO: Implement loot system integration
        }
        
        /// <summary>
        /// Applies a shrine buff for the rest of the run.
        /// </summary>
        private void ApplyShrineBuff(MazeNode node)
        {
            if (node.nodeData == null || string.IsNullOrEmpty(node.nodeData.shrineType))
                return;
            
            Debug.Log($"[MazeRunManager] Activating shrine: {node.nodeData.shrineType}");
            
            MazeRunModifier modifier = new MazeRunModifier(
                $"shrine_{node.nodeData.shrineType.ToLower().Replace(" ", "_")}",
                $"Shrine of {node.nodeData.shrineType}",
                GetShrineDescription(node.nodeData.shrineType),
                ModifierType.Shrine,
                node.nodeData.buffStrength
            );
            
            currentRun.AddRunModifier(modifier);
            OnRunStateChanged?.Invoke(currentRun);
            
            Debug.Log($"[MazeRunManager] Added run modifier: {modifier.displayName}");
        }
        
        private string GetShrineDescription(string shrineType)
        {
            return shrineType switch
            {
                "Cohesion" => "Your deck draws more consistently; the Maze likes your intent.",
                "Echoes" => "Cards replay under a Law-echo effect.",
                "Anchored Form" => "Enemies deal less entropy-type damage.",
                "Sealwrights" => "Warrant power increases slightly.",
                _ => "Maze blessing active."
            };
        }
        
        /// <summary>
        /// Triggers a trap node.
        /// </summary>
        private void TriggerTrap(MazeNode node)
        {
            Debug.Log($"[MazeRunManager] Trap triggered: {node.nodeData?.trapType ?? "Unknown"}");
            // TODO: Implement trap effects (curses, damage, etc.)
        }
        
        /// <summary>
        /// Opens forge node (card fusion/upgrade).
        /// </summary>
        private void OpenForge(MazeNode node)
        {
            Debug.Log($"[MazeRunManager] Opening forge at node {node.nodeId}");
            // TODO: Integrate with card fusion/upgrade system
        }
        
        /// <summary>
        /// Handles special node interactions.
        /// </summary>
        private void HandleSpecialNode(MazeNode node)
        {
            Debug.Log($"[MazeRunManager] Special node interaction at {node.nodeId}");
            // TODO: Implement special nodes (Market, Still-Point, etc.)
        }
        
        #endregion
        
        #region Combat Integration
        
        /// <summary>
        /// Called when combat is completed (victory).
        /// Returns to maze scene.
        /// </summary>
        public void OnCombatVictory()
        {
            Debug.Log("[MazeRunManager] Combat victory! Returning to maze...");
            
            // Verify run is still active before returning
            if (currentRun == null || !currentRun.isActive)
            {
                Debug.LogError($"[MazeRunManager] Cannot return from combat: run is not active! RunId: {currentRun?.runId ?? "null"}");
                // Clear flags anyway
                PlayerPrefs.DeleteKey("MazeCombatContext");
                PlayerPrefs.DeleteKey("MazeBossCombat");
                PlayerPrefs.DeleteKey("MazeRunId");
                PlayerPrefs.DeleteKey("MazeRunSeed");
                PlayerPrefs.DeleteKey("MazeRunActive");
                PlayerPrefs.Save();
                return;
            }
            
            Debug.Log($"[MazeRunManager] Run is still active. RunId: {currentRun.runId}, Seed: {currentRun.seed}, Floor: {currentRun.currentFloor}");
            
            // Restore random state to preserve maze generation consistency
            // Verify seed matches (safety check)
            int storedSeed = PlayerPrefs.GetInt("MazeRunSeed", -1);
            if (storedSeed == currentRun.seed)
            {
                // Restore the random state we saved before combat
                UnityEngine.Random.state = randomStateBeforeCombat;
                Debug.Log($"[MazeRunManager] Restored random state after combat (seed: {currentRun.seed})");
            }
            else
            {
                // Fallback: reinitialize with the run's seed
                Debug.LogWarning($"[MazeRunManager] Seed mismatch! Stored: {storedSeed}, Current: {currentRun.seed}. Reinitializing with current seed.");
                UnityEngine.Random.InitState(currentRun.seed);
            }
            
            // Reactivate minimap canvas when returning from combat
            // Note: This will be in the maze scene, so we can find it
            // However, the scene might not be loaded yet, so we'll also handle it in MazeSceneController
            
            // Try to find and reactivate MinimapCanvas (scene might be loaded already)
            GameObject minimapCanvasObj = GameObject.Find("MinimapCanvas");
            if (minimapCanvasObj != null)
            {
                minimapCanvasObj.SetActive(true);
                Debug.Log("[MazeRunManager] Reactivated MinimapCanvas after combat victory.");
            }
            else
            {
                // Canvas will be reactivated when maze scene loads via MazeSceneController
                Debug.Log("[MazeRunManager] MinimapCanvas not found yet (scene not loaded). Will be reactivated when maze scene loads.");
            }
            
            // Clear temporary encounter (but keep MazeRunId for verification when scene loads)
            temporaryCombatEncounter = null;
            PlayerPrefs.DeleteKey("MazeCombatContext");
            PlayerPrefs.DeleteKey("MazeBossCombat");
            PlayerPrefs.DeleteKey("MazeRunSeed");
            // Keep MazeRunId and MazeRunActive until scene verifies
            PlayerPrefs.Save();
            
            Debug.Log($"[MazeRunManager] Loading maze scene. Run will be verified on scene load. RunId: {currentRun.runId}");
            
            // Return to maze scene
            SceneManager.LoadScene(mazeSceneName);
        }
        
        /// <summary>
        /// Called when combat is lost.
        /// Fails the run.
        /// </summary>
        public void OnCombatDefeat()
        {
            Debug.Log("[MazeRunManager] Combat defeat! Run failed...");
            
            FailRun();
            
            // Clear temporary encounter
            temporaryCombatEncounter = null;
            PlayerPrefs.DeleteKey("MazeCombatContext");
            PlayerPrefs.DeleteKey("MazeBossCombat");
            PlayerPrefs.Save();
            
            // Return to main UI
            if (EncounterManager.Instance != null)
            {
                EncounterManager.Instance.ReturnToMainUI();
            }
            else
            {
                SceneManager.LoadScene("MainGameUI");
            }
        }
        
        #endregion
    }
}


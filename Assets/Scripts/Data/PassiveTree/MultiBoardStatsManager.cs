using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PassiveTree
{
    /// <summary>
    /// Manages stats from multiple boards (core board + extension boards)
    /// Consolidates all stats from all boards into a single source of truth
    /// </summary>
    public class MultiBoardStatsManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool autoUpdateOnStart = true;
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private float updateInterval = 0.1f; // Check for changes every 0.1 seconds
        [SerializeField] private bool forcePeriodicRefresh = true; // Force refresh periodically to catch missed updates
        [SerializeField] private float periodicRefreshInterval = 1.0f; // Force refresh every 1 second
        
        [Header("Board Management")]
        [SerializeField] private List<BoardJSONData> managedBoards = new List<BoardJSONData>();
        [SerializeField] private List<ExtensionBoardController> extensionBoards = new List<ExtensionBoardController>();
        
        [Header("Consolidated Data")]
        [SerializeField] private Dictionary<string, float> globalConsolidatedStats = new Dictionary<string, float>();
        [SerializeField] private int totalAllocatedNodes = 0;
        [SerializeField] private int totalBoards = 0;
        
        [Header("Events")]
        public System.Action<Dictionary<string, float>> OnGlobalStatsUpdated;
        public System.Action<int> OnTotalAllocatedNodesChanged;
        
        // Cached data for performance
        private Dictionary<string, float> lastGlobalStats = new Dictionary<string, float>();
        private bool isInitialized = false;
        private Coroutine updateCoroutine;
        private Coroutine periodicRefreshCoroutine;
        private float lastRefreshTime = 0f;
        
        void Start()
        {
            if (autoUpdateOnStart)
            {
                InitializeMultiBoardSystem();
            }
        }
        
        void OnDestroy()
        {
            if (updateCoroutine != null)
            {
                StopCoroutine(updateCoroutine);
            }
            
            if (periodicRefreshCoroutine != null)
            {
                StopCoroutine(periodicRefreshCoroutine);
            }
            
            // Unsubscribe from all board events
            UnsubscribeFromAllBoards();
        }
        
        /// <summary>
        /// Initialize the multi-board system
        /// </summary>
        public void InitializeMultiBoardSystem()
        {
            if (enableDebugLogging)
            {
                Debug.Log("[MultiBoardStatsManager] Initializing multi-board system");
            }
            
            // Find all boards
            FindAllBoards();
            
            // Subscribe to board events
            SubscribeToAllBoards();
            
            // Start update coroutine
            if (updateCoroutine != null)
            {
                StopCoroutine(updateCoroutine);
            }
            updateCoroutine = StartCoroutine(UpdateStatsCoroutine());
            
            // Start periodic refresh coroutine if enabled
            if (forcePeriodicRefresh)
            {
                if (periodicRefreshCoroutine != null)
                {
                    StopCoroutine(periodicRefreshCoroutine);
                }
                periodicRefreshCoroutine = StartCoroutine(PeriodicRefreshCoroutine());
            }
            
            // Initial consolidation
            ConsolidateAllBoardStats();
            
            isInitialized = true;
            
            if (enableDebugLogging)
            {
                Debug.Log($"[MultiBoardStatsManager] Multi-board system initialized - {totalBoards} boards, {totalAllocatedNodes} allocated nodes");
            }
        }
        
        /// <summary>
        /// Find all boards in the scene
        /// </summary>
        private void FindAllBoards()
        {
            managedBoards.Clear();
            extensionBoards.Clear();
            
            // Find all BoardJSONData components (core board + any extension boards with BoardJSONData)
            BoardJSONData[] allBoardJSONData = FindObjectsOfType<BoardJSONData>();
            foreach (var boardData in allBoardJSONData)
            {
                if (boardData != null)
                {
                    managedBoards.Add(boardData);
                    if (enableDebugLogging)
                    {
                        Debug.Log($"[MultiBoardStatsManager] Found BoardJSONData: {boardData.gameObject.name}");
                    }
                }
            }
            
            // Find all ExtensionBoardController components
            ExtensionBoardController[] allExtensionBoards = FindObjectsOfType<ExtensionBoardController>();
            foreach (var extensionBoard in allExtensionBoards)
            {
                if (extensionBoard != null)
                {
                    extensionBoards.Add(extensionBoard);
                    if (enableDebugLogging)
                    {
                        Debug.Log($"[MultiBoardStatsManager] Found ExtensionBoard: {extensionBoard.gameObject.name}");
                    }
                }
            }
            
            totalBoards = managedBoards.Count + extensionBoards.Count;
            
            if (enableDebugLogging)
            {
                Debug.Log($"[MultiBoardStatsManager] Found {managedBoards.Count} boards with BoardJSONData, {extensionBoards.Count} extension boards");
            }
        }
        
        /// <summary>
        /// Subscribe to all board events
        /// </summary>
        private void SubscribeToAllBoards()
        {
            // Subscribe to BoardJSONData events
            foreach (var boardData in managedBoards)
            {
                if (boardData != null)
                {
                    boardData.OnStatsUpdated += OnBoardStatsUpdated;
                    if (enableDebugLogging)
                    {
                        Debug.Log($"[MultiBoardStatsManager] Subscribed to board: {boardData.gameObject.name}");
                    }
                }
            }
            
            // Subscribe to PassiveTreeManager events for extension boards
            PassiveTree.PassiveTreeManager.OnNodeAllocated += OnPassiveTreeNodeAllocated;
            PassiveTree.PassiveTreeManager.OnNodeDeallocated += OnPassiveTreeNodeDeallocated;
        }
        
        /// <summary>
        /// Unsubscribe from all board events
        /// </summary>
        private void UnsubscribeFromAllBoards()
        {
            // Unsubscribe from BoardJSONData events
            foreach (var boardData in managedBoards)
            {
                if (boardData != null)
                {
                    boardData.OnStatsUpdated -= OnBoardStatsUpdated;
                }
            }
            
            // Unsubscribe from PassiveTreeManager events
            PassiveTree.PassiveTreeManager.OnNodeAllocated -= OnPassiveTreeNodeAllocated;
            PassiveTree.PassiveTreeManager.OnNodeDeallocated -= OnPassiveTreeNodeDeallocated;
        }
        
        /// <summary>
        /// Handle stats updates from individual boards
        /// </summary>
        private void OnBoardStatsUpdated(Dictionary<string, float> boardStats)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[MultiBoardStatsManager] Board stats updated: {boardStats.Count} stat types");
            }
            
            // Consolidate all board stats
            ConsolidateAllBoardStats();
        }
        
        /// <summary>
        /// Handle node allocation from PassiveTreeManager
        /// </summary>
        private void OnPassiveTreeNodeAllocated(Vector2Int position, CellController cell)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[MultiBoardStatsManager] PassiveTreeManager node allocated at {position}");
            }
            
            // Always refresh all boards when any node is allocated
            // This ensures we catch updates from both core and extension boards
            RefreshAllBoardsImmediately();
        }
        
        /// <summary>
        /// Handle node deallocation from PassiveTreeManager
        /// </summary>
        private void OnPassiveTreeNodeDeallocated(Vector2Int position, CellController cell)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[MultiBoardStatsManager] PassiveTreeManager node deallocated at {position}");
            }
            
            // Always refresh all boards when any node is deallocated
            // This ensures we catch updates from both core and extension boards
            RefreshAllBoardsImmediately();
        }
        
        /// <summary>
        /// Check if a cell belongs to an extension board
        /// </summary>
        private bool IsExtensionBoardNode(CellController cell)
        {
            // Check if the cell is a child of an ExtensionBoardController
            Transform current = cell.transform;
            while (current != null)
            {
                if (current.GetComponent<ExtensionBoardController>() != null)
                {
                    return true;
                }
                current = current.parent;
            }
            return false;
        }
        
        /// <summary>
        /// Refresh all boards immediately (core + extension boards)
        /// </summary>
        private void RefreshAllBoardsImmediately()
        {
            if (enableDebugLogging)
            {
                Debug.Log("[MultiBoardStatsManager] Refreshing all boards immediately");
            }
            
            // Force refresh all boards
            FindAllBoards();
            ConsolidateAllBoardStats();
            
            // Force trigger events to ensure UI updates
            OnGlobalStatsUpdated?.Invoke(globalConsolidatedStats);
        }
        
        /// <summary>
        /// Consolidate stats from all boards
        /// </summary>
        public void ConsolidateAllBoardStats()
        {
            globalConsolidatedStats.Clear();
            totalAllocatedNodes = 0;
            
            if (enableDebugLogging)
            {
                Debug.Log("[MultiBoardStatsManager] Consolidating stats from all boards");
            }
            
            // Consolidate stats from boards with BoardJSONData
            foreach (var boardData in managedBoards)
            {
                if (boardData != null)
                {
                    var boardStats = boardData.GetConsolidatedStats();
                    var boardAllocatedNodes = boardData.GetAllocatedNodeCount();
                    
                    if (enableDebugLogging)
                    {
                        Debug.Log($"[MultiBoardStatsManager] Board {boardData.gameObject.name}: {boardStats.Count} stats, {boardAllocatedNodes} nodes");
                    }
                    
                    // Add stats to global consolidation
                    foreach (var stat in boardStats)
                    {
                        if (globalConsolidatedStats.ContainsKey(stat.Key))
                        {
                            globalConsolidatedStats[stat.Key] += stat.Value;
                        }
                        else
                        {
                            globalConsolidatedStats[stat.Key] = stat.Value;
                        }
                    }
                    
                    totalAllocatedNodes += boardAllocatedNodes;
                }
            }
            
            // Consolidate stats from extension boards (direct CellJsonData access)
            foreach (var extensionBoard in extensionBoards)
            {
                if (extensionBoard != null)
                {
                    var extensionStats = GetExtensionBoardStats(extensionBoard);
                    var extensionAllocatedNodes = GetExtensionBoardAllocatedNodes(extensionBoard);
                    
                    if (enableDebugLogging)
                    {
                        Debug.Log($"[MultiBoardStatsManager] Extension board {extensionBoard.gameObject.name}: {extensionStats.Count} stats, {extensionAllocatedNodes} nodes");
                    }
                    
                    // Add stats to global consolidation
                    foreach (var stat in extensionStats)
                    {
                        if (globalConsolidatedStats.ContainsKey(stat.Key))
                        {
                            globalConsolidatedStats[stat.Key] += stat.Value;
                        }
                        else
                        {
                            globalConsolidatedStats[stat.Key] = stat.Value;
                        }
                    }
                    
                    totalAllocatedNodes += extensionAllocatedNodes;
                }
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"[MultiBoardStatsManager] Global consolidation complete: {globalConsolidatedStats.Count} unique stat types, {totalAllocatedNodes} total allocated nodes");
            }
            
            // Check if stats have changed
            bool statsChanged = !AreStatsEqual(globalConsolidatedStats, lastGlobalStats);
            if (statsChanged)
            {
                lastGlobalStats = new Dictionary<string, float>(globalConsolidatedStats);
                OnGlobalStatsUpdated?.Invoke(globalConsolidatedStats);
                OnTotalAllocatedNodesChanged?.Invoke(totalAllocatedNodes);
            }
        }
        
        /// <summary>
        /// Get stats from an extension board
        /// </summary>
        private Dictionary<string, float> GetExtensionBoardStats(ExtensionBoardController extensionBoard)
        {
            var stats = new Dictionary<string, float>();
            
            try
            {
                // Get all cells from the extension board
                var allCells = extensionBoard.GetAllCells();
                
                foreach (var cell in allCells.Values)
                {
                    if (cell != null && cell.IsPurchased)
                    {
                        var cellJsonData = cell.GetComponent<CellJsonData>();
                        if (cellJsonData != null && cellJsonData.HasJsonData())
                        {
                            var cellStats = ExtractStatsFromCell(cellJsonData);
                            
                            // Add stats to extension board total
                            foreach (var stat in cellStats)
                            {
                                if (stats.ContainsKey(stat.Key))
                                {
                                    stats[stat.Key] += stat.Value;
                                }
                                else
                                {
                                    stats[stat.Key] = stat.Value;
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MultiBoardStatsManager] Error getting extension board stats: {e.Message}");
            }
            
            return stats;
        }
        
        /// <summary>
        /// Get allocated nodes count from an extension board
        /// </summary>
        private int GetExtensionBoardAllocatedNodes(ExtensionBoardController extensionBoard)
        {
            int count = 0;
            
            try
            {
                var allCells = extensionBoard.GetAllCells();
                foreach (var cell in allCells.Values)
                {
                    if (cell != null && cell.IsPurchased)
                    {
                        count++;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MultiBoardStatsManager] Error counting extension board nodes: {e.Message}");
            }
            
            return count;
        }
        
        /// <summary>
        /// Extract stats from a CellJsonData component (same as BoardJSONData)
        /// </summary>
        private Dictionary<string, float> ExtractStatsFromCell(CellJsonData cellJsonData)
        {
            var stats = new Dictionary<string, float>();
            
            if (cellJsonData.NodeStats == null)
            {
                return stats;
            }
            
            var nodeStats = cellJsonData.NodeStats;
            
            // Core Attributes
            if (nodeStats.strength != 0) stats["Strength"] = nodeStats.strength;
            if (nodeStats.dexterity != 0) stats["Dexterity"] = nodeStats.dexterity;
            if (nodeStats.intelligence != 0) stats["Intelligence"] = nodeStats.intelligence;
            
            // Combat Resources
            if (nodeStats.maxHealthIncrease != 0) stats["Max Health"] = nodeStats.maxHealthIncrease;
            if (nodeStats.maxEnergyShieldIncrease != 0) stats["Max Energy Shield"] = nodeStats.maxEnergyShieldIncrease;
            if (nodeStats.maxMana != 0) stats["Max Mana"] = nodeStats.maxMana;
            if (nodeStats.maxReliance != 0) stats["Max Reliance"] = nodeStats.maxReliance;
            
            // Combat Stats
            if (nodeStats.attackPower != 0) stats["Attack Power"] = nodeStats.attackPower;
            if (nodeStats.defense != 0) stats["Defense"] = nodeStats.defense;
            if (nodeStats.criticalChance != 0) stats["Critical Chance"] = nodeStats.criticalChance;
            if (nodeStats.criticalMultiplier != 0) stats["Critical Multiplier"] = nodeStats.criticalMultiplier;
            if (nodeStats.accuracy != 0) stats["Accuracy"] = nodeStats.accuracy;
            
            // Damage Modifiers (Increased)
            if (nodeStats.increasedPhysicalDamage != 0) stats["Physical Damage"] = nodeStats.increasedPhysicalDamage;
            if (nodeStats.increasedFireDamage != 0) stats["Fire Damage"] = nodeStats.increasedFireDamage;
            if (nodeStats.increasedColdDamage != 0) stats["Cold Damage"] = nodeStats.increasedColdDamage;
            if (nodeStats.increasedLightningDamage != 0) stats["Lightning Damage"] = nodeStats.increasedLightningDamage;
            if (nodeStats.increasedChaosDamage != 0) stats["Chaos Damage"] = nodeStats.increasedChaosDamage;
            if (nodeStats.increasedElementalDamage != 0) stats["Elemental Damage"] = nodeStats.increasedElementalDamage;
            if (nodeStats.increasedSpellDamage != 0) stats["Spell Damage"] = nodeStats.increasedSpellDamage;
            if (nodeStats.increasedAttackDamage != 0) stats["Attack Damage"] = nodeStats.increasedAttackDamage;
            
            // Added Damage
            if (nodeStats.addedPhysicalDamage != 0) stats["Added Physical Damage"] = nodeStats.addedPhysicalDamage;
            if (nodeStats.addedFireDamage != 0) stats["Added Fire Damage"] = nodeStats.addedFireDamage;
            if (nodeStats.addedColdDamage != 0) stats["Added Cold Damage"] = nodeStats.addedColdDamage;
            if (nodeStats.addedLightningDamage != 0) stats["Added Lightning Damage"] = nodeStats.addedLightningDamage;
            if (nodeStats.addedChaosDamage != 0) stats["Added Chaos Damage"] = nodeStats.addedChaosDamage;
            if (nodeStats.addedElementalDamage != 0) stats["Added Elemental Damage"] = nodeStats.addedElementalDamage;
            if (nodeStats.addedSpellDamage != 0) stats["Added Spell Damage"] = nodeStats.addedSpellDamage;
            if (nodeStats.addedAttackDamage != 0) stats["Added Attack Damage"] = nodeStats.addedAttackDamage;
            
            // Resistances
            if (nodeStats.physicalResistance != 0) stats["Physical Resistance"] = nodeStats.physicalResistance;
            if (nodeStats.fireResistance != 0) stats["Fire Resistance"] = nodeStats.fireResistance;
            if (nodeStats.coldResistance != 0) stats["Cold Resistance"] = nodeStats.coldResistance;
            if (nodeStats.lightningResistance != 0) stats["Lightning Resistance"] = nodeStats.lightningResistance;
            if (nodeStats.chaosResistance != 0) stats["Chaos Resistance"] = nodeStats.chaosResistance;
            if (nodeStats.elementalResistance != 0) stats["Elemental Resistance"] = nodeStats.elementalResistance;
            if (nodeStats.allResistance != 0) stats["All Resistance"] = nodeStats.allResistance;
            
            // Defense Stats
            if (nodeStats.armour != 0) stats["Armour"] = nodeStats.armour;
            if (nodeStats.evasion != 0) stats["Evasion"] = nodeStats.evasion;
            if (nodeStats.energyShield != 0) stats["Energy Shield"] = nodeStats.energyShield;
            if (nodeStats.blockChance != 0) stats["Block Chance"] = nodeStats.blockChance;
            if (nodeStats.dodgeChance != 0) stats["Dodge Chance"] = nodeStats.dodgeChance;
            if (nodeStats.spellDodgeChance != 0) stats["Spell Dodge Chance"] = nodeStats.spellDodgeChance;
            if (nodeStats.spellBlockChance != 0) stats["Spell Block Chance"] = nodeStats.spellBlockChance;
            
            // Recovery Stats
            if (nodeStats.lifeRegeneration != 0) stats["Life Regeneration"] = nodeStats.lifeRegeneration;
            if (nodeStats.energyShieldRegeneration != 0) stats["Energy Shield Regeneration"] = nodeStats.energyShieldRegeneration;
            if (nodeStats.manaRegeneration != 0) stats["Mana Regeneration"] = nodeStats.manaRegeneration;
            if (nodeStats.relianceRegeneration != 0) stats["Reliance Regeneration"] = nodeStats.relianceRegeneration;
            if (nodeStats.lifeLeech != 0) stats["Life Leech"] = nodeStats.lifeLeech;
            if (nodeStats.manaLeech != 0) stats["Mana Leech"] = nodeStats.manaLeech;
            if (nodeStats.energyShieldLeech != 0) stats["Energy Shield Leech"] = nodeStats.energyShieldLeech;
            
            // Combat Mechanics
            if (nodeStats.attackSpeed != 0) stats["Attack Speed"] = nodeStats.attackSpeed;
            if (nodeStats.castSpeed != 0) stats["Cast Speed"] = nodeStats.castSpeed;
            if (nodeStats.movementSpeed != 0) stats["Movement Speed"] = nodeStats.movementSpeed;
            if (nodeStats.attackRange != 0) stats["Attack Range"] = nodeStats.attackRange;
            if (nodeStats.projectileSpeed != 0) stats["Projectile Speed"] = nodeStats.projectileSpeed;
            if (nodeStats.areaOfEffect != 0) stats["Area of Effect"] = nodeStats.areaOfEffect;
            if (nodeStats.skillEffectDuration != 0) stats["Skill Effect Duration"] = nodeStats.skillEffectDuration;
            if (nodeStats.statusEffectDuration != 0) stats["Status Effect Duration"] = nodeStats.statusEffectDuration;
            
            // Card System Stats
            if (nodeStats.cardsDrawnPerTurn != 0) stats["Cards Drawn Per Turn"] = nodeStats.cardsDrawnPerTurn;
            if (nodeStats.maxHandSize != 0) stats["Max Hand Size"] = nodeStats.maxHandSize;
            if (nodeStats.cardDrawChance != 0) stats["Card Draw Chance"] = nodeStats.cardDrawChance;
            if (nodeStats.cardRetentionChance != 0) stats["Card Retention Chance"] = nodeStats.cardRetentionChance;
            if (nodeStats.cardUpgradeChance != 0) stats["Card Upgrade Chance"] = nodeStats.cardUpgradeChance;
            if (nodeStats.discardPower != 0) stats["Discard Power"] = nodeStats.discardPower;
            if (nodeStats.manaPerTurn != 0) stats["Mana Per Turn"] = nodeStats.manaPerTurn;
            
            // Legacy/Backward Compatibility Fields
            if (nodeStats.armorIncrease != 0) stats["Armor"] = nodeStats.armorIncrease;
            if (nodeStats.increasedEvasion != 0) stats["Evasion"] = nodeStats.increasedEvasion;
            if (nodeStats.elementalResist != 0) stats["Elemental Resistance"] = nodeStats.elementalResist;
            if (nodeStats.spellPowerIncrease != 0) stats["Spell Power"] = nodeStats.spellPowerIncrease;
            if (nodeStats.critChanceIncrease != 0) stats["Critical Strike Chance"] = nodeStats.critChanceIncrease;
            if (nodeStats.critMultiplierIncrease != 0) stats["Critical Strike Multiplier"] = nodeStats.critMultiplierIncrease;
            
            // Elemental Legacy Stats - Fire
            if (nodeStats.fireIncrease != 0) stats["Fire Damage (Legacy)"] = nodeStats.fireIncrease;
            if (nodeStats.fire != 0) stats["Fire Damage (Legacy)"] = nodeStats.fire;
            if (nodeStats.addedPhysicalAsFire != 0) stats["Physical Damage as Fire"] = nodeStats.addedPhysicalAsFire;
            if (nodeStats.addedFireAsCold != 0) stats["Fire Damage as Cold"] = nodeStats.addedFireAsCold;
            
            // Elemental Legacy Stats - Cold
            if (nodeStats.coldIncrease != 0) stats["Cold Damage (Legacy)"] = nodeStats.coldIncrease;
            if (nodeStats.cold != 0) stats["Cold Damage (Legacy)"] = nodeStats.cold;
            if (nodeStats.addedPhysicalAsCold != 0) stats["Physical Damage as Cold"] = nodeStats.addedPhysicalAsCold;
            if (nodeStats.addedColdAsFire != 0) stats["Cold Damage as Fire"] = nodeStats.addedColdAsFire;
            
            // Elemental Legacy Stats - Lightning
            if (nodeStats.lightningIncrease != 0) stats["Lightning Damage (Legacy)"] = nodeStats.lightningIncrease;
            if (nodeStats.lightning != 0) stats["Lightning Damage (Legacy)"] = nodeStats.lightning;
            if (nodeStats.addedPhysicalAsLightning != 0) stats["Physical Damage as Lightning"] = nodeStats.addedPhysicalAsLightning;
            if (nodeStats.addedLightningAsFire != 0) stats["Lightning Damage as Fire"] = nodeStats.addedLightningAsFire;
            
            // Elemental Legacy Stats - Physical
            if (nodeStats.physicalIncrease != 0) stats["Physical Damage (Legacy)"] = nodeStats.physicalIncrease;
            if (nodeStats.physical != 0) stats["Physical Damage (Legacy)"] = nodeStats.physical;
            
            // Elemental Legacy Stats - Chaos
            if (nodeStats.chaosIncrease != 0) stats["Chaos Damage (Legacy)"] = nodeStats.chaosIncrease;
            if (nodeStats.chaos != 0) stats["Chaos Damage (Legacy)"] = nodeStats.chaos;
            
            return stats;
        }
        
        /// <summary>
        /// Check if two stat dictionaries are equal
        /// </summary>
        private bool AreStatsEqual(Dictionary<string, float> stats1, Dictionary<string, float> stats2)
        {
            if (stats1.Count != stats2.Count) return false;
            
            foreach (var kvp in stats1)
            {
                if (!stats2.ContainsKey(kvp.Key) || Mathf.Abs(stats2[kvp.Key] - kvp.Value) > 0.001f)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Update coroutine to periodically check for changes
        /// </summary>
        private System.Collections.IEnumerator UpdateStatsCoroutine()
        {
            while (true)
            {
                // Periodically consolidate stats to catch any missed updates
                ConsolidateAllBoardStats();
                lastRefreshTime = Time.time;
                yield return new WaitForSeconds(updateInterval);
            }
        }
        
        /// <summary>
        /// Periodic refresh coroutine to force refresh periodically
        /// </summary>
        private System.Collections.IEnumerator PeriodicRefreshCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(periodicRefreshInterval);
                
                if (enableDebugLogging)
                {
                    Debug.Log($"[MultiBoardStatsManager] Periodic refresh triggered (every {periodicRefreshInterval}s)");
                }
                
                // Force refresh all boards
                FindAllBoards();
                ConsolidateAllBoardStats();
                
                // Force trigger events to ensure UI updates
                OnGlobalStatsUpdated?.Invoke(globalConsolidatedStats);
            }
        }
        
        /// <summary>
        /// Get the global consolidated stats
        /// </summary>
        public Dictionary<string, float> GetGlobalConsolidatedStats()
        {
            return new Dictionary<string, float>(globalConsolidatedStats);
        }
        
        /// <summary>
        /// Get the total number of allocated nodes across all boards
        /// </summary>
        public int GetTotalAllocatedNodeCount()
        {
            return totalAllocatedNodes;
        }
        
        /// <summary>
        /// Get the total number of boards
        /// </summary>
        public int GetTotalBoardCount()
        {
            return totalBoards;
        }
        
        /// <summary>
        /// Check if there are any allocated nodes across all boards
        /// </summary>
        public bool HasAnyAllocatedNodes()
        {
            return totalAllocatedNodes > 0;
        }
        
        /// <summary>
        /// Force refresh of all data
        /// </summary>
        [ContextMenu("Force Refresh All Data")]
        public void ForceRefreshAllData()
        {
            if (enableDebugLogging)
            {
                Debug.Log("[MultiBoardStatsManager] Force refreshing all data");
            }
            
            FindAllBoards();
            ConsolidateAllBoardStats();
        }
        
        /// <summary>
        /// Force immediate refresh and update display
        /// </summary>
        [ContextMenu("Force Immediate Refresh")]
        public void ForceImmediateRefresh()
        {
            Debug.Log("[MultiBoardStatsManager] === FORCE IMMEDIATE REFRESH ===");
            
            // Force refresh all data
            FindAllBoards();
            ConsolidateAllBoardStats();
            
            // Force trigger the global stats updated event
            OnGlobalStatsUpdated?.Invoke(globalConsolidatedStats);
            
            Debug.Log($"[MultiBoardStatsManager] Immediate refresh complete - {globalConsolidatedStats.Count} stats, {totalAllocatedNodes} allocated nodes across {totalBoards} boards");
        }
        
        /// <summary>
        /// Debug method to show all boards
        /// </summary>
        [ContextMenu("Debug All Boards")]
        public void DebugAllBoards()
        {
            Debug.Log("[MultiBoardStatsManager] === DEBUGGING ALL BOARDS ===");
            Debug.Log($"[MultiBoardStatsManager] Total boards: {totalBoards}");
            Debug.Log($"[MultiBoardStatsManager] Boards with BoardJSONData: {managedBoards.Count}");
            Debug.Log($"[MultiBoardStatsManager] Extension boards: {extensionBoards.Count}");
            
            foreach (var board in managedBoards)
            {
                if (board != null)
                {
                    Debug.Log($"[MultiBoardStatsManager] BoardJSONData: {board.gameObject.name} - {board.GetAllocatedNodeCount()} nodes");
                }
            }
            
            foreach (var extensionBoard in extensionBoards)
            {
                if (extensionBoard != null)
                {
                    int allocatedNodes = GetExtensionBoardAllocatedNodes(extensionBoard);
                    Debug.Log($"[MultiBoardStatsManager] Extension board: {extensionBoard.gameObject.name} - {allocatedNodes} nodes");
                }
            }
        }
        
        /// <summary>
        /// Debug method to show global consolidated stats
        /// </summary>
        [ContextMenu("Debug Global Stats")]
        public void DebugGlobalStats()
        {
            Debug.Log("[MultiBoardStatsManager] === DEBUGGING GLOBAL STATS ===");
            Debug.Log($"[MultiBoardStatsManager] Total stat types: {globalConsolidatedStats.Count}");
            Debug.Log($"[MultiBoardStatsManager] Total allocated nodes: {totalAllocatedNodes}");
            
            foreach (var stat in globalConsolidatedStats.OrderBy(s => s.Key))
            {
                string suffix = GetStatSuffix(stat.Key);
                Debug.Log($"[MultiBoardStatsManager] {stat.Key}: +{stat.Value}{suffix}");
            }
        }
        
        /// <summary>
        /// Check if the auto-update system is working properly
        /// </summary>
        [ContextMenu("Check Auto-Update Status")]
        public void CheckAutoUpdateStatus()
        {
            Debug.Log("[MultiBoardStatsManager] === AUTO-UPDATE STATUS CHECK ===");
            Debug.Log($"[MultiBoardStatsManager] Is Initialized: {isInitialized}");
            Debug.Log($"[MultiBoardStatsManager] Update Coroutine Running: {(updateCoroutine != null ? "Yes" : "No")}");
            Debug.Log($"[MultiBoardStatsManager] Periodic Refresh Running: {(periodicRefreshCoroutine != null ? "Yes" : "No")}");
            Debug.Log($"[MultiBoardStatsManager] Force Periodic Refresh: {forcePeriodicRefresh}");
            Debug.Log($"[MultiBoardStatsManager] Update Interval: {updateInterval}s");
            Debug.Log($"[MultiBoardStatsManager] Periodic Refresh Interval: {periodicRefreshInterval}s");
            Debug.Log($"[MultiBoardStatsManager] Last Refresh Time: {lastRefreshTime}");
            Debug.Log($"[MultiBoardStatsManager] Time Since Last Refresh: {Time.time - lastRefreshTime}s");
            Debug.Log($"[MultiBoardStatsManager] Total Boards: {totalBoards}");
            Debug.Log($"[MultiBoardStatsManager] Total Allocated Nodes: {totalAllocatedNodes}");
            Debug.Log($"[MultiBoardStatsManager] Global Stats Count: {globalConsolidatedStats.Count}");
        }
        
        /// <summary>
        /// Get the suffix for a stat name
        /// </summary>
        private string GetStatSuffix(string statName)
        {
            if (statName.Contains("Damage") || statName.Contains("Speed") || statName.Contains("Chance") || 
                statName.Contains("Resistance") || statName.Contains("Leech") || statName.Contains("Range"))
                return "%";
            return "";
        }
    }
}

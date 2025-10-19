using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PassiveTree
{
    /// <summary>
    /// Consolidates JSON data and stats from all allocated cells in a board
    /// This provides a single source of truth for all board-level stats
    /// </summary>
    public class BoardJSONData : MonoBehaviour
    {
        [Header("Board Configuration")]
        [SerializeField] private string boardId = "core_board";
        [SerializeField] private bool autoUpdateOnStart = true;
        [SerializeField] private bool enableDebugLogging = true;
        
        [Header("Consolidated Data")]
        [SerializeField] private List<CellJsonData> allocatedCells = new List<CellJsonData>();
        [SerializeField] private Dictionary<string, float> consolidatedStats = new Dictionary<string, float>();
        [SerializeField] private int totalAllocatedNodes = 0;
        
        [Header("Events")]
        public System.Action<Dictionary<string, float>> OnStatsUpdated;
        public System.Action<int> OnAllocatedNodesChanged;
        
        // Cached data for performance
        private Dictionary<string, float> lastConsolidatedStats = new Dictionary<string, float>();
        private bool isInitialized = false;
        
        void Start()
        {
            if (autoUpdateOnStart)
            {
                InitializeBoardData();
            }
            
            // Subscribe to PassiveTreeManager events for automatic updates
            SubscribeToPassiveTreeEvents();
        }
        
        void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            UnsubscribeFromPassiveTreeEvents();
        }
        
        /// <summary>
        /// Initialize the board data and start monitoring for changes
        /// </summary>
        public void InitializeBoardData()
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[BoardJSONData] Initializing board data for {boardId}");
            }
            
            RefreshAllocatedCells();
            ConsolidateStats();
            isInitialized = true;
            
            if (enableDebugLogging)
            {
                Debug.Log($"[BoardJSONData] Board data initialized - {totalAllocatedNodes} allocated nodes, {consolidatedStats.Count} stat types");
            }
        }
        
        /// <summary>
        /// Refresh the list of allocated cells from all CellJsonData components in this board
        /// </summary>
        public void RefreshAllocatedCells()
        {
            allocatedCells.Clear();
            
            // Find all CellJsonData components in this board
            CellJsonData[] allCellJsonData = GetComponentsInChildren<CellJsonData>();
            
            if (enableDebugLogging)
            {
                Debug.Log($"[BoardJSONData] Found {allCellJsonData.Length} CellJsonData components in board {boardId}");
            }
            
            foreach (var cellJsonData in allCellJsonData)
            {
                if (cellJsonData != null && IsCellAllocated(cellJsonData))
                {
                    allocatedCells.Add(cellJsonData);
                    
                    if (enableDebugLogging)
                    {
                        Debug.Log($"[BoardJSONData] Found allocated cell: {cellJsonData.gameObject.name} - {cellJsonData.NodeName}");
                    }
                }
            }
            
            totalAllocatedNodes = allocatedCells.Count;
            
            if (enableDebugLogging)
            {
                Debug.Log($"[BoardJSONData] Found {totalAllocatedNodes} allocated cells in board {boardId}");
            }
            
            // Trigger event
            OnAllocatedNodesChanged?.Invoke(totalAllocatedNodes);
        }
        
        /// <summary>
        /// Check if a cell is allocated (purchased)
        /// </summary>
        private bool IsCellAllocated(CellJsonData cellJsonData)
        {
            // Check if the cell has a CellController and is purchased
            var cellController = cellJsonData.GetComponent<CellController>();
            if (cellController != null)
            {
                return cellController.IsPurchased;
            }
            
            // Fallback: check if the cell has JSON data (assumes allocated if it has data)
            return cellJsonData.HasJsonData();
        }
        
        /// <summary>
        /// Consolidate all stats from allocated cells
        /// </summary>
        public void ConsolidateStats()
        {
            consolidatedStats.Clear();
            
            if (enableDebugLogging)
            {
                Debug.Log($"[BoardJSONData] Consolidating stats from {allocatedCells.Count} allocated cells");
            }
            
            foreach (var cellJsonData in allocatedCells)
            {
                if (cellJsonData != null && cellJsonData.HasJsonData())
                {
                    var cellStats = ExtractStatsFromCell(cellJsonData);
                    
                    if (enableDebugLogging)
                    {
                        Debug.Log($"[BoardJSONData] Cell {cellJsonData.NodeName} has {cellStats.Count} stats");
                    }
                    
                    // Add stats to consolidated dictionary
                    foreach (var stat in cellStats)
                    {
                        if (consolidatedStats.ContainsKey(stat.Key))
                        {
                            consolidatedStats[stat.Key] += stat.Value;
                        }
                        else
                        {
                            consolidatedStats[stat.Key] = stat.Value;
                        }
                    }
                }
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"[BoardJSONData] Consolidated {consolidatedStats.Count} unique stat types");
                foreach (var stat in consolidatedStats)
                {
                    Debug.Log($"[BoardJSONData]   {stat.Key}: {stat.Value}");
                }
            }
            
            // Check if stats have changed
            bool statsChanged = !AreStatsEqual(consolidatedStats, lastConsolidatedStats);
            if (statsChanged)
            {
                lastConsolidatedStats = new Dictionary<string, float>(consolidatedStats);
                OnStatsUpdated?.Invoke(consolidatedStats);
            }
        }
        
        /// <summary>
        /// Extract stats from a single cell's JsonStats
        /// </summary>
        private Dictionary<string, float> ExtractStatsFromCell(CellJsonData cellJsonData)
        {
            var stats = new Dictionary<string, float>();
            
            if (cellJsonData.NodeStats == null)
            {
                if (enableDebugLogging)
                {
                    Debug.LogWarning($"[BoardJSONData] Cell {cellJsonData.NodeName} has no NodeStats");
                }
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
        /// Force refresh of all data
        /// </summary>
        [ContextMenu("Force Refresh All Data")]
        public void ForceRefreshAllData()
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[BoardJSONData] Force refreshing all data for board {boardId}");
            }
            
            RefreshAllocatedCells();
            ConsolidateStats();
        }
        
        /// <summary>
        /// Force immediate refresh and update display
        /// </summary>
        [ContextMenu("Force Immediate Refresh")]
        public void ForceImmediateRefresh()
        {
            Debug.Log($"[BoardJSONData] === FORCE IMMEDIATE REFRESH ===");
            
            // Force refresh all data
            RefreshAllocatedCells();
            ConsolidateStats();
            
            // Force trigger the stats updated event
            OnStatsUpdated?.Invoke(consolidatedStats);
            
            Debug.Log($"[BoardJSONData] Immediate refresh complete - {consolidatedStats.Count} stats, {totalAllocatedNodes} allocated nodes");
        }
        
        /// <summary>
        /// Get the current consolidated stats
        /// </summary>
        public Dictionary<string, float> GetConsolidatedStats()
        {
            return new Dictionary<string, float>(consolidatedStats);
        }
        
        /// <summary>
        /// Get the number of allocated nodes
        /// </summary>
        public int GetAllocatedNodeCount()
        {
            return totalAllocatedNodes;
        }
        
        /// <summary>
        /// Get all allocated cells
        /// </summary>
        public List<CellJsonData> GetAllocatedCells()
        {
            return new List<CellJsonData>(allocatedCells);
        }
        
        /// <summary>
        /// Check if the board has any allocated nodes
        /// </summary>
        public bool HasAllocatedNodes()
        {
            return totalAllocatedNodes > 0;
        }
        
        /// <summary>
        /// Get a formatted summary of all stats
        /// </summary>
        public string GetStatsSummary()
        {
            if (consolidatedStats.Count == 0)
            {
                return "No stats allocated";
            }
            
            var summary = new System.Text.StringBuilder();
            summary.AppendLine($"Board: {boardId}");
            summary.AppendLine($"Allocated Nodes: {totalAllocatedNodes}");
            summary.AppendLine($"Stat Types: {consolidatedStats.Count}");
            summary.AppendLine();
            
            // Group stats by category
            var categories = new Dictionary<string, List<KeyValuePair<string, float>>>
            {
                { "Attributes", new List<KeyValuePair<string, float>>() },
                { "Combat Resources", new List<KeyValuePair<string, float>>() },
                { "Damage", new List<KeyValuePair<string, float>>() },
                { "Resistances", new List<KeyValuePair<string, float>>() },
                { "Defense", new List<KeyValuePair<string, float>>() },
                { "Recovery", new List<KeyValuePair<string, float>>() },
                { "Combat Mechanics", new List<KeyValuePair<string, float>>() },
                { "Card System", new List<KeyValuePair<string, float>>() },
                { "Other", new List<KeyValuePair<string, float>>() }
            };
            
            foreach (var stat in consolidatedStats)
            {
                string category = GetStatCategory(stat.Key);
                if (categories.ContainsKey(category))
                {
                    categories[category].Add(stat);
                }
                else
                {
                    categories["Other"].Add(stat);
                }
            }
            
            // Format each category
            foreach (var category in categories)
            {
                if (category.Value.Count > 0)
                {
                    summary.AppendLine($"{category.Key}:");
                    foreach (var stat in category.Value.OrderBy(s => s.Key))
                    {
                        string suffix = GetStatSuffix(stat.Key);
                        summary.AppendLine($"  {stat.Key}: +{stat.Value}{suffix}");
                    }
                    summary.AppendLine();
                }
            }
            
            return summary.ToString();
        }
        
        /// <summary>
        /// Get the category for a stat name
        /// </summary>
        private string GetStatCategory(string statName)
        {
            if (statName.Contains("Strength") || statName.Contains("Dexterity") || statName.Contains("Intelligence"))
                return "Attributes";
            if (statName.Contains("Max Health") || statName.Contains("Max Mana") || statName.Contains("Max Energy Shield") || statName.Contains("Max Reliance"))
                return "Combat Resources";
            if (statName.Contains("Damage"))
                return "Damage";
            if (statName.Contains("Resistance"))
                return "Resistances";
            if (statName.Contains("Armour") || statName.Contains("Evasion") || statName.Contains("Block") || statName.Contains("Dodge"))
                return "Defense";
            if (statName.Contains("Regeneration") || statName.Contains("Leech"))
                return "Recovery";
            if (statName.Contains("Speed") || statName.Contains("Range") || statName.Contains("Effect"))
                return "Combat Mechanics";
            if (statName.Contains("Card") || statName.Contains("Mana Per Turn"))
                return "Card System";
            return "Other";
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
        
        /// <summary>
        /// Debug method to show all allocated cells
        /// </summary>
        [ContextMenu("Debug Allocated Cells")]
        public void DebugAllocatedCells()
        {
            Debug.Log($"[BoardJSONData] === DEBUGGING ALLOCATED CELLS ===");
            Debug.Log($"[BoardJSONData] Board ID: {boardId}");
            Debug.Log($"[BoardJSONData] Total allocated cells: {totalAllocatedNodes}");
            
            foreach (var cell in allocatedCells)
            {
                if (cell != null)
                {
                    Debug.Log($"[BoardJSONData] Cell: {cell.gameObject.name} - {cell.NodeName} (Type: {cell.NodeType})");
                }
            }
        }
        
        /// <summary>
        /// Debug method to show all consolidated stats
        /// </summary>
        [ContextMenu("Debug Consolidated Stats")]
        public void DebugConsolidatedStats()
        {
            Debug.Log($"[BoardJSONData] === DEBUGGING CONSOLIDATED STATS ===");
            Debug.Log($"[BoardJSONData] Board ID: {boardId}");
            Debug.Log($"[BoardJSONData] Total stat types: {consolidatedStats.Count}");
            
            foreach (var stat in consolidatedStats.OrderBy(s => s.Key))
            {
                string suffix = GetStatSuffix(stat.Key);
                Debug.Log($"[BoardJSONData] {stat.Key}: +{stat.Value}{suffix}");
            }
        }
        
        /// <summary>
        /// Update stats when a node is allocated (called by external systems)
        /// </summary>
        public void OnNodeAllocated(CellJsonData cellJsonData)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[BoardJSONData] Node allocated: {cellJsonData.NodeName}");
            }
            
            RefreshAllocatedCells();
            ConsolidateStats();
        }
        
        /// <summary>
        /// Update stats when a node is deallocated (called by external systems)
        /// </summary>
        public void OnNodeDeallocated(CellJsonData cellJsonData)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[BoardJSONData] Node deallocated: {cellJsonData.NodeName}");
            }
            
            RefreshAllocatedCells();
            ConsolidateStats();
        }
        
        /// <summary>
        /// Subscribe to PassiveTreeManager events for automatic updates
        /// </summary>
        private void SubscribeToPassiveTreeEvents()
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[BoardJSONData] Subscribing to PassiveTreeManager events");
            }
            
            PassiveTree.PassiveTreeManager.OnNodeAllocated += OnPassiveTreeNodeAllocated;
            PassiveTree.PassiveTreeManager.OnNodeDeallocated += OnPassiveTreeNodeDeallocated;
        }
        
        /// <summary>
        /// Unsubscribe from PassiveTreeManager events
        /// </summary>
        private void UnsubscribeFromPassiveTreeEvents()
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[BoardJSONData] Unsubscribing from PassiveTreeManager events");
            }
            
            PassiveTree.PassiveTreeManager.OnNodeAllocated -= OnPassiveTreeNodeAllocated;
            PassiveTree.PassiveTreeManager.OnNodeDeallocated -= OnPassiveTreeNodeDeallocated;
        }
        
        /// <summary>
        /// Handle node allocation from PassiveTreeManager
        /// </summary>
        private void OnPassiveTreeNodeAllocated(Vector2Int position, CellController cell)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[BoardJSONData] PassiveTreeManager node allocated at {position}");
            }
            
            // Find the CellJsonData component for this cell
            var cellJsonData = cell.GetComponent<CellJsonData>();
            if (cellJsonData != null)
            {
                OnNodeAllocated(cellJsonData);
            }
            else
            {
                if (enableDebugLogging)
                {
                    Debug.LogWarning($"[BoardJSONData] No CellJsonData found on allocated cell at {position}");
                }
            }
        }
        
        /// <summary>
        /// Handle node deallocation from PassiveTreeManager
        /// </summary>
        private void OnPassiveTreeNodeDeallocated(Vector2Int position, CellController cell)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[BoardJSONData] PassiveTreeManager node deallocated at {position}");
            }
            
            // Find the CellJsonData component for this cell
            var cellJsonData = cell.GetComponent<CellJsonData>();
            if (cellJsonData != null)
            {
                OnNodeDeallocated(cellJsonData);
            }
            else
            {
                if (enableDebugLogging)
                {
                    Debug.LogWarning($"[BoardJSONData] No CellJsonData found on deallocated cell at {position}");
                }
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace PassiveTree.UI
{
    /// <summary>
    /// Static stats summary panel that displays all accumulated stats from allocated passive nodes
    /// </summary>
    public class StatsSummaryPanel : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] public GameObject statsPanel;
        [SerializeField] public Button toggleButton;
        [SerializeField] public TextMeshProUGUI toggleButtonText;
        [SerializeField] public TextMeshProUGUI statsContentText;
        [SerializeField] public ScrollRect statsScrollRect;
        [SerializeField] public TextMeshProUGUI titleText;

        [Header("Styling")]
        [SerializeField] private Color titleColor = Color.white;
        [SerializeField] private Color statColor = Color.white;
        [SerializeField] private Color categoryColor = Color.yellow;
        [SerializeField] private Color valueColor = Color.green;
        [SerializeField] private int titleFontSize = 24;
        [SerializeField] private int statFontSize = 18;
        [SerializeField] private int categoryFontSize = 20;

        [Header("Configuration")]
        [SerializeField] private bool startVisible = false;
        [SerializeField] private float updateInterval = 0.5f; // Update every 0.5 seconds
        [SerializeField] private string toggleButtonTextShow = "Show Stats";
        [SerializeField] private string toggleButtonTextHide = "Hide Stats";

        private bool isVisible = false;
        private Coroutine updateCoroutine;
        private PassiveTreeManager passiveTreeManager;
        private BoardPositioningManager boardPositioningManager;
        private BoardJSONData boardJSONData;
        private MultiBoardStatsManager multiBoardStatsManager;

        // Cached stats to avoid recalculating every frame
        private Dictionary<string, float> cachedStats = new Dictionary<string, float>();
        private float lastUpdateTime = 0f;

        void Awake()
        {
            // Ensure this component persists
            DontDestroyOnLoad(gameObject);
            
            // Initialize visibility
            isVisible = startVisible;
            UpdatePanelVisibility();
            
            // Setup button
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(TogglePanel);
            }
            
            UpdateToggleButtonText();
            
            Debug.Log("[StatsSummaryPanel] Awake - Component initialized and set to persist");
        }

        void Start()
        {
            Debug.Log("[StatsSummaryPanel] Start - Component starting initialization");
            
            // Find managers
            passiveTreeManager = FindObjectOfType<PassiveTreeManager>();
            boardPositioningManager = FindObjectOfType<BoardPositioningManager>();
            boardJSONData = FindObjectOfType<BoardJSONData>();
            multiBoardStatsManager = FindObjectOfType<MultiBoardStatsManager>();
            
            if (passiveTreeManager == null)
            {
                Debug.LogWarning("[StatsSummaryPanel] PassiveTreeManager not found in scene");
            }
            else
            {
                Debug.Log("[StatsSummaryPanel] ✅ Found PassiveTreeManager");
            }
            
            if (boardPositioningManager == null)
            {
                Debug.LogWarning("[StatsSummaryPanel] BoardPositioningManager not found in scene");
            }
            else
            {
                Debug.Log("[StatsSummaryPanel] ✅ Found BoardPositioningManager");
            }
            
            if (boardJSONData == null)
            {
                Debug.LogWarning("[StatsSummaryPanel] BoardJSONData not found in scene");
            }
            else
            {
                Debug.Log("[StatsSummaryPanel] ✅ Found BoardJSONData");
                // Subscribe to stats updates
                boardJSONData.OnStatsUpdated += OnStatsUpdated;
            }
            
            if (multiBoardStatsManager == null)
            {
                Debug.LogWarning("[StatsSummaryPanel] MultiBoardStatsManager not found in scene");
            }
            else
            {
                Debug.Log("[StatsSummaryPanel] ✅ Found MultiBoardStatsManager");
                // Subscribe to global stats updates
                multiBoardStatsManager.OnGlobalStatsUpdated += OnGlobalStatsUpdated;
            }
            
            // Note: BoardJSONData will handle PassiveTreeManager events directly
            // No need to subscribe here since BoardJSONData does it automatically
            
            // Start update coroutine
            if (updateCoroutine != null)
            {
                StopCoroutine(updateCoroutine);
            }
            updateCoroutine = StartCoroutine(UpdateStatsCoroutine());
            
            Debug.Log("[StatsSummaryPanel] ✅ Stats Summary Panel initialized and running");
        }

        void OnDestroy()
        {
            if (updateCoroutine != null)
            {
                StopCoroutine(updateCoroutine);
            }
            
            // Unsubscribe from events
            if (boardJSONData != null)
            {
                boardJSONData.OnStatsUpdated -= OnStatsUpdated;
            }
            
            if (multiBoardStatsManager != null)
            {
                multiBoardStatsManager.OnGlobalStatsUpdated -= OnGlobalStatsUpdated;
            }
            
            // Note: BoardJSONData handles PassiveTreeManager events directly
            // No need to unsubscribe here
        }

        /// <summary>
        /// Toggle the stats panel visibility
        /// </summary>
        public void TogglePanel()
        {
            isVisible = !isVisible;
            UpdatePanelVisibility();
            UpdateToggleButtonText();
            Debug.Log($"[StatsSummaryPanel] Panel toggled: {(isVisible ? "Visible" : "Hidden")}");
        }

        /// <summary>
        /// Show the stats panel
        /// </summary>
        public void ShowPanel()
        {
            isVisible = true;
            UpdatePanelVisibility();
            UpdateToggleButtonText();
            Debug.Log("[StatsSummaryPanel] Panel shown");
        }

        /// <summary>
        /// Hide the stats panel
        /// </summary>
        public void HidePanel()
        {
            isVisible = false;
            UpdatePanelVisibility();
            UpdateToggleButtonText();
            Debug.Log("[StatsSummaryPanel] Panel hidden");
        }

        /// <summary>
        /// Force update the stats display
        /// </summary>
        public void ForceUpdateStats()
        {
            Debug.Log("[StatsSummaryPanel] Force updating stats display");
            UpdateStatsDisplay();
        }
        
        /// <summary>
        /// Force show panel and update stats - useful for debugging
        /// </summary>
        [ContextMenu("Force Show and Update Stats")]
        public void ForceShowAndUpdateStats()
        {
            Debug.Log("[StatsSummaryPanel] Force showing panel and updating stats");
            isVisible = true;
            UpdatePanelVisibility();
            UpdateToggleButtonText();
            ForceUpdateStats();
        }
        
        /// <summary>
        /// Event handler for when stats are updated from BoardJSONData
        /// </summary>
        private void OnStatsUpdated(Dictionary<string, float> newStats)
        {
            Debug.Log($"[StatsSummaryPanel] Stats updated from BoardJSONData: {newStats.Count} stat types");
            
            // Force update the display immediately
            if (isVisible)
            {
                UpdateStatsDisplay();
            }
        }
        
        /// <summary>
        /// Event handler for when global stats are updated from MultiBoardStatsManager
        /// </summary>
        private void OnGlobalStatsUpdated(Dictionary<string, float> newStats)
        {
            Debug.Log($"[StatsSummaryPanel] Global stats updated from MultiBoardStatsManager: {newStats.Count} stat types");
            
            // Force update the display immediately
            if (isVisible)
            {
                UpdateStatsDisplay();
            }
        }
        
        // Note: BoardJSONData handles PassiveTreeManager events directly
        // No need for these event handlers here
        
        /// <summary>
        /// Debug method to check what's happening with stats calculation
        /// </summary>
        [ContextMenu("Debug Stats Calculation")]
        public void DebugStatsCalculation()
        {
            Debug.Log("[StatsSummaryPanel] === DEBUGGING STATS CALCULATION ===");
            
            // Check if managers are found
            Debug.Log($"PassiveTreeManager: {(passiveTreeManager != null ? "Found" : "NULL")}");
            Debug.Log($"BoardPositioningManager: {(boardPositioningManager != null ? "Found" : "NULL")}");
            
            // Force calculate stats
            var stats = CalculateAllStats();
            Debug.Log($"Calculated {stats.Count} total stats:");
            foreach (var stat in stats)
            {
                Debug.Log($"  {stat.Key}: {stat.Value}");
            }
            
            // Force update display
            ForceUpdateStats();
        }

        private void UpdatePanelVisibility()
        {
            if (statsPanel != null)
            {
                statsPanel.SetActive(isVisible);
            }
        }

        private void UpdateToggleButtonText()
        {
            if (toggleButtonText != null)
            {
                toggleButtonText.text = isVisible ? toggleButtonTextHide : toggleButtonTextShow;
            }
        }

        private System.Collections.IEnumerator UpdateStatsCoroutine()
        {
            while (true)
            {
                if (isVisible)
                {
                    UpdateStatsDisplay();
                }
                yield return new WaitForSeconds(updateInterval);
            }
        }

        private void UpdateStatsDisplay()
        {
            Debug.Log($"[StatsSummaryPanel] UpdateStatsDisplay called - isVisible: {isVisible}, statsContentText: {(statsContentText != null ? "Found" : "NULL")}");
            
            if (!isVisible || statsContentText == null) 
            {
                Debug.Log($"[StatsSummaryPanel] UpdateStatsDisplay returning early - isVisible: {isVisible}, statsContentText: {(statsContentText != null ? "Found" : "NULL")}");
                return;
            }

            // Calculate current stats
            Dictionary<string, float> currentStats = CalculateAllStats();
            Debug.Log($"[StatsSummaryPanel] CalculateAllStats returned {currentStats.Count} stats");
            
            // Update display
            string statsText = FormatStatsText(currentStats);
            Debug.Log($"[StatsSummaryPanel] FormatStatsText returned: {statsText}");
            statsContentText.text = statsText;
            
            // Update title
            if (titleText != null)
            {
                titleText.text = "Passive Tree Stats";
                titleText.color = titleColor;
                titleText.fontSize = titleFontSize;
            }
            
            // Cache stats for comparison
            cachedStats = new Dictionary<string, float>(currentStats);
            lastUpdateTime = Time.time;
        }

        private Dictionary<string, float> CalculateAllStats()
        {
            var allStats = new Dictionary<string, float>();
            
            try
            {
                // Use MultiBoardStatsManager if available (preferred method for multi-board support)
                if (multiBoardStatsManager != null)
                {
                    allStats = multiBoardStatsManager.GetGlobalConsolidatedStats();
                    Debug.Log($"[StatsSummaryPanel] Using MultiBoardStatsManager - found {allStats.Count} global consolidated stats");
                }
                // Fallback to BoardJSONData if available
                else if (boardJSONData != null)
                {
                    allStats = boardJSONData.GetConsolidatedStats();
                    Debug.Log($"[StatsSummaryPanel] Using BoardJSONData - found {allStats.Count} consolidated stats");
                }
                else
                {
                    // Fallback to old method if neither manager available
                    Debug.LogWarning("[StatsSummaryPanel] No stats managers found, using fallback method");
                    var allocatedNodes = GetAllAllocatedNodes();
                    Debug.Log($"[StatsSummaryPanel] Found {allocatedNodes.Count} allocated nodes");
                    
                    foreach (var node in allocatedNodes)
                    {
                        Debug.Log($"[StatsSummaryPanel] Processing node: {node.name} at {node.GridPosition}");
                        var nodeStats = ExtractStatsFromNode(node);
                        Debug.Log($"[StatsSummaryPanel] Extracted {nodeStats.Count} stats from node");
                        
                        foreach (var stat in nodeStats)
                        {
                            if (allStats.ContainsKey(stat.Key))
                            {
                                allStats[stat.Key] += stat.Value;
                            }
                            else
                            {
                                allStats[stat.Key] = stat.Value;
                            }
                        }
                    }
                    
                    Debug.Log($"[StatsSummaryPanel] Total stats calculated: {allStats.Count}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[StatsSummaryPanel] Error calculating stats: {e.Message}");
            }
            
            return allStats;
        }

        private List<CellController> GetAllAllocatedNodes()
        {
            var allocatedNodes = new List<CellController>();
            
            try
            {
                // Get core board nodes
                if (passiveTreeManager != null)
                {
                    var allCells = passiveTreeManager.GetAllCells();
                    Debug.Log($"[StatsSummaryPanel] Core board has {allCells.Count} total cells");
                    
                    int purchasedCount = 0;
                    foreach (var cell in allCells.Values)
                    {
                        if (cell != null)
                        {
                            if (cell.IsPurchased)
                            {
                                allocatedNodes.Add(cell);
                                purchasedCount++;
                                Debug.Log($"[StatsSummaryPanel] Found purchased core cell: {cell.name} at {cell.GridPosition}");
                            }
                        }
                    }
                    Debug.Log($"[StatsSummaryPanel] Core board has {purchasedCount} purchased cells");
                }
                else
                {
                    Debug.LogWarning("[StatsSummaryPanel] PassiveTreeManager is null");
                }
                
                // Get extension board nodes
                if (boardPositioningManager != null)
                {
                    var extensionBoards = FindObjectsOfType<ExtensionBoardController>();
                    Debug.Log($"[StatsSummaryPanel] Found {extensionBoards.Length} extension boards");
                    
                    foreach (var board in extensionBoards)
                    {
                        var allCells = board.GetAllCells();
                        Debug.Log($"[StatsSummaryPanel] Extension board {board.name} has {allCells.Count} total cells");
                        
                        int purchasedCount = 0;
                        foreach (var cell in allCells.Values)
                        {
                            if (cell != null)
                            {
                                if (cell.IsPurchased)
                                {
                                    allocatedNodes.Add(cell);
                                    purchasedCount++;
                                    Debug.Log($"[StatsSummaryPanel] Found purchased extension cell: {cell.name} at {cell.GridPosition}");
                                }
                            }
                        }
                        Debug.Log($"[StatsSummaryPanel] Extension board {board.name} has {purchasedCount} purchased cells");
                    }
                }
                else
                {
                    Debug.LogWarning("[StatsSummaryPanel] BoardPositioningManager is null");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[StatsSummaryPanel] Error getting allocated nodes: {e.Message}");
            }
            
            Debug.Log($"[StatsSummaryPanel] Total allocated nodes found: {allocatedNodes.Count}");
            return allocatedNodes;
        }

        private Dictionary<string, float> ExtractStatsFromNode(CellController node)
        {
            var stats = new Dictionary<string, float>();
            
            try
            {
                // Get the node's JSON data from CellJsonData component
                var cellJsonData = node.GetComponent<CellJsonData>();
                if (cellJsonData == null)
                {
                    Debug.LogWarning($"[StatsSummaryPanel] No CellJsonData found on node {node.name}");
                    return stats;
                }
                
                if (!cellJsonData.HasJsonData())
                {
                    Debug.LogWarning($"[StatsSummaryPanel] Node {node.name} has no JSON data");
                    return stats;
                }
                
                Debug.Log($"[StatsSummaryPanel] Processing JSON data for node {node.name}");
                
                // Get the JSON node data
                var jsonNodeData = cellJsonData.GetJsonNodeData();
                if (jsonNodeData == null)
                {
                    Debug.LogWarning($"[StatsSummaryPanel] No JsonNodeData for node {node.name}");
                    return stats;
                }
                
                Debug.Log($"[StatsSummaryPanel] JsonNodeData found for node {node.name}. Name: {jsonNodeData.name}, Description: {jsonNodeData.description}");
                
                if (jsonNodeData.stats == null)
                {
                    Debug.LogWarning($"[StatsSummaryPanel] No stats in JsonNodeData for node {node.name}");
                    return stats;
                }
                
                // Extract stats from the JsonStats object
                var nodeStats = jsonNodeData.stats;
                
                // Debug: Check if JsonStats object is null or empty
                if (nodeStats == null)
                {
                    Debug.LogWarning($"[StatsSummaryPanel] JsonStats object is NULL for node {node.name}");
                    return stats;
                }
                
                Debug.Log($"[StatsSummaryPanel] JsonStats object found for node {node.name}. Type: {nodeStats.GetType().Name}");
                
                // Debug: Check a few specific properties to see if they have values
                Debug.Log($"[StatsSummaryPanel] Sample values - strength: {nodeStats.strength}, dexterity: {nodeStats.dexterity}, intelligence: {nodeStats.intelligence}");
                Debug.Log($"[StatsSummaryPanel] Sample values - increasedPhysicalDamage: {nodeStats.increasedPhysicalDamage}, increasedFireDamage: {nodeStats.increasedFireDamage}");
                
                // Debug: Check if any stats have non-zero values
                bool hasNonZeroStats = false;
                if (nodeStats.strength != 0 || nodeStats.dexterity != 0 || nodeStats.intelligence != 0 ||
                    nodeStats.increasedPhysicalDamage != 0 || nodeStats.increasedFireDamage != 0 ||
                    nodeStats.increasedColdDamage != 0 || nodeStats.increasedLightningDamage != 0)
                {
                    hasNonZeroStats = true;
                }
                Debug.Log($"[StatsSummaryPanel] Has non-zero stats: {hasNonZeroStats}");
                
                // If no stats found, let's check if the JsonStats object is properly initialized
                if (!hasNonZeroStats)
                {
                    Debug.LogWarning($"[StatsSummaryPanel] No non-zero stats found in JsonStats for node {node.name}");
                    Debug.Log($"[StatsSummaryPanel] JsonStats object details:");
                    Debug.Log($"  - Type: {nodeStats.GetType().FullName}");
                    Debug.Log($"  - Is null: {nodeStats == null}");
                    
                    // Check a few more specific stats to see if they have any values
                    Debug.Log($"  - maxHealthIncrease: {nodeStats.maxHealthIncrease}");
                    Debug.Log($"  - maxEnergyShieldIncrease: {nodeStats.maxEnergyShieldIncrease}");
                    Debug.Log($"  - attackPower: {nodeStats.attackPower}");
                    Debug.Log($"  - defence: {nodeStats.defense}");
                }
                
                // Map JsonStats properties to display names
                var statMappings = new Dictionary<string, string>
                {
                    // Core Attributes
                    { "strength", "Strength" },
                    { "dexterity", "Dexterity" },
                    { "intelligence", "Intelligence" },
                    
                    // Combat Resources
                    { "maxHealth", "Max Health" },
                    { "maxMana", "Max Mana" },
                    { "maxReliance", "Max Reliance" },
                    { "maxEnergyShield", "Max Energy Shield" },
                    
                    // Damage Modifiers
                    { "increasedPhysicalDamage", "Physical Damage" },
                    { "increasedFireDamage", "Fire Damage" },
                    { "increasedColdDamage", "Cold Damage" },
                    { "increasedLightningDamage", "Lightning Damage" },
                    { "increasedChaosDamage", "Chaos Damage" },
                    { "increasedElementalDamage", "Elemental Damage" },
                    { "increasedSpellDamage", "Spell Damage" },
                    { "increasedAttackDamage", "Attack Damage" },
                    
                    // Resistances
                    { "physicalResistance", "Physical Resistance" },
                    { "fireResistance", "Fire Resistance" },
                    { "coldResistance", "Cold Resistance" },
                    { "lightningResistance", "Lightning Resistance" },
                    { "chaosResistance", "Chaos Resistance" },
                    { "elementalResistance", "Elemental Resistance" },
                    { "allResistance", "All Resistance" },
                    
                    // Ailment Magnitudes
                    { "increasedIgniteMagnitude", "Ignite Magnitude" },
                    { "increasedShockMagnitude", "Shock Magnitude" },
                    { "increasedChillMagnitude", "Chill Magnitude" },
                    { "increasedFreezeMagnitude", "Freeze Magnitude" },
                    { "increasedBleedMagnitude", "Bleed Magnitude" },
                    { "increasedPoisonMagnitude", "Poison Magnitude" },
                    
                    // Combat Mechanics
                    { "movementSpeed", "Movement Speed" },
                    { "attackSpeed", "Attack Speed" },
                    { "castSpeed", "Cast Speed" },
                    { "attackRange", "Attack Range" },
                    { "projectileSpeed", "Projectile Speed" },
                    { "areaOfEffect", "Area of Effect" },
                    
                    // Defense Stats
                    { "armour", "Armour" },
                    { "evasion", "Evasion" },
                    { "energyShield", "Energy Shield" },
                    { "blockChance", "Block Chance" },
                    { "dodgeChance", "Dodge Chance" },
                    { "spellDodgeChance", "Spell Dodge Chance" },
                    { "spellBlockChance", "Spell Block Chance" },
                    
                    // Recovery Stats
                    { "lifeRegeneration", "Life Regeneration" },
                    { "energyShieldRegeneration", "Energy Shield Regeneration" },
                    { "manaRegeneration", "Mana Regeneration" },
                    { "relianceRegeneration", "Reliance Regeneration" },
                    { "lifeLeech", "Life Leech" },
                    { "manaLeech", "Mana Leech" },
                    { "energyShieldLeech", "Energy Shield Leech" },
                    
                    // Card System Stats
                    { "cardsDrawnPerTurn", "Cards Drawn Per Turn" },
                    { "maxHandSize", "Max Hand Size" },
                    { "cardDrawChance", "Card Draw Chance" },
                    { "cardRetentionChance", "Card Retention Chance" },
                    { "cardUpgradeChance", "Card Upgrade Chance" },
                    { "discardPower", "Discard Power" }
                };
                
                // Extract stats using reflection to get all properties
                var statsType = nodeStats.GetType();
                var properties = statsType.GetProperties();
                Debug.Log($"[StatsSummaryPanel] Found {properties.Length} properties in JsonStats for node {node.name}");
                
                // Debug: List all property names and their values
                Debug.Log($"[StatsSummaryPanel] All properties for node {node.name}:");
                foreach (var prop in properties)
                {
                    try
                    {
                        var value = prop.GetValue(nodeStats);
                        Debug.Log($"[StatsSummaryPanel]   {prop.Name} ({prop.PropertyType.Name}) = {value}");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[StatsSummaryPanel]   {prop.Name} - Error getting value: {e.Message}");
                    }
                }
                
                // Debug: Show what's in statMappings
                Debug.Log($"[StatsSummaryPanel] StatMappings contains {statMappings.Count} entries:");
                foreach (var mapping in statMappings)
                {
                    Debug.Log($"[StatsSummaryPanel]   {mapping.Key} -> {mapping.Value}");
                }
                
                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(float) || property.PropertyType == typeof(int))
                    {
                        float value = 0f;
                        if (property.PropertyType == typeof(float))
                        {
                            value = (float)property.GetValue(nodeStats);
                        }
                        else if (property.PropertyType == typeof(int))
                        {
                            value = (int)property.GetValue(nodeStats);
                        }
                        
                        if (Mathf.Abs(value) > 0.001f && statMappings.ContainsKey(property.Name))
                        {
                            string displayName = statMappings[property.Name];
                            if (!stats.ContainsKey(displayName))
                            {
                                stats[displayName] = 0f;
                            }
                            stats[displayName] += value;
                            Debug.Log($"[StatsSummaryPanel] Found stat: {property.Name} = {value} -> {displayName} (Total: {stats[displayName]})");
                        }
                        else if (value > 0)
                        {
                            Debug.Log($"[StatsSummaryPanel] Found unmapped stat: {property.Name} = {value}");
                        }
                    }
                }
                
                Debug.Log($"[StatsSummaryPanel] Extracted {stats.Count} stats from node {node.name}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[StatsSummaryPanel] Error extracting stats from node: {e.Message}");
            }
            
            return stats;
        }

        private string FormatStatsText(Dictionary<string, float> stats)
        {
            if (stats.Count == 0)
            {
                return "<color=#888888>No stats allocated</color>";
            }
            
            var sb = new System.Text.StringBuilder();
            
            // Group stats by category
            var categories = new Dictionary<string, List<KeyValuePair<string, float>>>
            {
                { "Damage", new List<KeyValuePair<string, float>>() },
                { "Attributes", new List<KeyValuePair<string, float>>() },
                { "Resistances", new List<KeyValuePair<string, float>>() },
                { "Magnitudes", new List<KeyValuePair<string, float>>() },
                { "Speed", new List<KeyValuePair<string, float>>() },
                { "Other", new List<KeyValuePair<string, float>>() }
            };
            
            foreach (var stat in stats)
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
                    sb.AppendLine($"<size={categoryFontSize}><color=#{ColorUtility.ToHtmlStringRGB(categoryColor)}>{category.Key}</color></size>");
                    
                    foreach (var stat in category.Value.OrderBy(s => s.Key))
                    {
                        string suffix = GetStatSuffix(stat.Key);
                        string color = GetStatColor(stat.Key);
                        sb.AppendLine($"<size={statFontSize}><color=#{color}>{stat.Key}: +{stat.Value}{suffix}</color></size>");
                    }
                    sb.AppendLine();
                }
            }
            
            return sb.ToString();
        }

        private string GetStatCategory(string statName)
        {
            if (statName.Contains("Damage")) return "Damage";
            if (statName.Contains("Intelligence") || statName.Contains("Strength") || statName.Contains("Dexterity")) return "Attributes";
            if (statName.Contains("Resistance")) return "Resistances";
            if (statName.Contains("Magnitude")) return "Magnitudes";
            if (statName.Contains("Speed")) return "Speed";
            return "Other";
        }

        private string GetStatSuffix(string statName)
        {
            if (statName.Contains("Damage") || statName.Contains("Magnitude") || statName.Contains("Speed"))
                return "%";
            return "";
        }

        private string GetStatColor(string statName)
        {
            if (statName.Contains("Fire")) return ColorUtility.ToHtmlStringRGB(Color.red);
            if (statName.Contains("Cold")) return ColorUtility.ToHtmlStringRGB(Color.cyan);
            if (statName.Contains("Lightning")) return ColorUtility.ToHtmlStringRGB(Color.yellow);
            if (statName.Contains("Physical")) return ColorUtility.ToHtmlStringRGB(Color.gray);
            if (statName.Contains("Intelligence")) return ColorUtility.ToHtmlStringRGB(Color.blue);
            if (statName.Contains("Strength")) return ColorUtility.ToHtmlStringRGB(Color.red);
            if (statName.Contains("Dexterity")) return ColorUtility.ToHtmlStringRGB(Color.green);
            return ColorUtility.ToHtmlStringRGB(statColor);
        }

        /// <summary>
        /// Test method to show sample stats
        /// </summary>
        [ContextMenu("Test Stats Display")]
        public void TestStatsDisplay()
        {
            var testStats = new Dictionary<string, float>
            {
                { "Fire Damage", 25.5f },
                { "Intelligence", 15f },
                { "Health", 100f },
                { "Ignite Magnitude", 30f },
                { "Movement Speed", 10f }
            };
            
            string testText = FormatStatsText(testStats);
            if (statsContentText != null)
            {
                statsContentText.text = testText;
            }
            
            Debug.Log("[StatsSummaryPanel] Testing stats display with sample data");
        }
        
        /// <summary>
        /// Check if the panel is still active and working
        /// </summary>
        [ContextMenu("Check Panel Status")]
        public void CheckPanelStatus()
        {
            Debug.Log($"[StatsSummaryPanel] Panel Status Check:");
            Debug.Log($"- GameObject Active: {gameObject.activeInHierarchy}");
            Debug.Log($"- Component Enabled: {enabled}");
            Debug.Log($"- Stats Panel Active: {(statsPanel != null ? statsPanel.activeInHierarchy : "NULL")}");
            Debug.Log($"- Toggle Button: {(toggleButton != null ? "Found" : "NULL")}");
            Debug.Log($"- Stats Content Text: {(statsContentText != null ? "Found" : "NULL")}");
            Debug.Log($"- Is Visible: {isVisible}");
            Debug.Log($"- Update Coroutine Running: {(updateCoroutine != null ? "Yes" : "No")}");
            
            // Test the stats calculation
            var testStats = CalculateAllStats();
            Debug.Log($"- Test stats calculation: {testStats.Count} stats found");
            foreach (var stat in testStats)
            {
                Debug.Log($"  {stat.Key}: {stat.Value}");
            }
        }

        /// <summary>
        /// Debug method to check JSON data on all cells
        /// </summary>
        [ContextMenu("Debug JSON Data on All Cells")]
        public void DebugJsonDataOnAllCells()
        {
            Debug.Log("[StatsSummaryPanel] === DEBUGGING JSON DATA ON ALL CELLS ===");
            
            var allocatedNodes = GetAllAllocatedNodes();
            Debug.Log($"[StatsSummaryPanel] Found {allocatedNodes.Count} allocated nodes to check");
            
            int cellsWithJsonData = 0;
            int cellsWithStats = 0;
            
            foreach (var node in allocatedNodes)
            {
                var cellJsonData = node.GetComponent<CellJsonData>();
                if (cellJsonData != null)
                {
                    cellsWithJsonData++;
                    Debug.Log($"[StatsSummaryPanel] Cell {node.name} has CellJsonData component");
                    
                    if (cellJsonData.HasJsonData())
                    {
                        Debug.Log($"[StatsSummaryPanel] Cell {node.name} has JSON data: {cellJsonData.NodeName}");
                        
                        var jsonNodeData = cellJsonData.GetJsonNodeData();
                        if (jsonNodeData != null && jsonNodeData.stats != null)
                        {
                            var nodeStats = jsonNodeData.stats;
                            
                            // Check if this node has any non-zero stats
                            bool hasStats = false;
                            if (nodeStats.strength != 0 || nodeStats.dexterity != 0 || nodeStats.intelligence != 0 ||
                                nodeStats.increasedPhysicalDamage != 0 || nodeStats.increasedFireDamage != 0 ||
                                nodeStats.maxHealthIncrease != 0 || nodeStats.attackPower != 0)
                            {
                                hasStats = true;
                                cellsWithStats++;
                                Debug.Log($"[StatsSummaryPanel] ✅ Cell {node.name} has stats: strength={nodeStats.strength}, dexterity={nodeStats.dexterity}, intelligence={nodeStats.intelligence}");
                            }
                            else
                            {
                                Debug.Log($"[StatsSummaryPanel] ❌ Cell {node.name} has no stats (all values are 0)");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"[StatsSummaryPanel] Cell {node.name} has JSON data but no stats object");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[StatsSummaryPanel] Cell {node.name} has CellJsonData but no JSON data loaded");
                    }
                }
                else
                {
                    Debug.LogWarning($"[StatsSummaryPanel] Cell {node.name} has no CellJsonData component");
                }
            }
            
            Debug.Log($"[StatsSummaryPanel] Summary: {cellsWithJsonData}/{allocatedNodes.Count} cells have JSON data, {cellsWithStats}/{allocatedNodes.Count} cells have stats");
        }
    }
}

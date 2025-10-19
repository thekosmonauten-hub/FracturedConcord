using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using PassiveTree;

namespace PassiveTreeEditor
{
    /// <summary>
    /// Serializable preset for stat templates
    /// </summary>
    [System.Serializable]
    public class StatTemplatePreset
    {
        public string name;
        public JsonStats stats;
        
        public StatTemplatePreset(string presetName, JsonStats presetStats)
        {
            name = presetName;
            stats = presetStats;
        }
    }

    /// <summary>
    /// Custom editor window for managing CellJsonData on prefabs
    /// Provides a streamlined interface for adjusting JSON data without manual scrolling
    /// </summary>
    public class PassiveTreeJsonDataEditor : EditorWindow
    {
        [Header("Board Selection")]
        private GameObject selectedBoard;
        private List<CellJsonData> allCellJsonData = new List<CellJsonData>();
        private List<CellJsonData> filteredCellJsonData = new List<CellJsonData>();
        
        [Header("Search and Filter")]
        private string searchFilter = "";
        private string nodeTypeFilter = "All";
        private bool showOnlyAllocated = false;
        private bool showOnlyWithStats = false;
        
        [Header("Bulk Operations")]
        private bool bulkMode = false;
        private List<CellJsonData> selectedCells = new List<CellJsonData>();
        
        [Header("Stat Editing")]
        private Vector2 scrollPosition;
        private CellJsonData selectedCell;
        private JsonStats editingStats;
        private bool isEditing = false;
        
        [Header("UI Settings")]
        private int selectedTab = 0;
        private string[] tabNames = { "Cell Browser", "Grid View", "Stat Editor", "Bulk Operations", "Search & Filter" };
        
        [Header("Grid View Settings")]
        private bool showGrid = true;
        private Vector2 gridScrollPosition;
        private float cellSize = 60f;
        private CellJsonData[,] gridCells = new CellJsonData[7, 7];
        
        [Header("Stat Template Settings")]
        private JsonStats statTemplate = new JsonStats();
        private bool showTemplateEditor = false;
        private Vector2 templateScrollPosition;
        
        [Header("Template Presets")]
        private List<StatTemplatePreset> templatePresets = new List<StatTemplatePreset>();
        private int selectedPresetIndex = 0;
        private string newPresetName = "";
        private bool showPresetEditor = false;
        
        [Header("Data Source")]
        private bool usePrefabData = true; // Toggle between scene data and prefab data
        
        [Header("Node Name Generation")]
        private bool autoGenerateNodeNames = true;
        private string nodeNamePrefix = "Cell";
        private bool includePositionInName = true;
        private bool includeStatsInName = true;
        
        [MenuItem("Tools/Passive Tree/JSON Data Editor")]
        public static void ShowWindow()
        {
            PassiveTreeJsonDataEditor window = GetWindow<PassiveTreeJsonDataEditor>("Passive Tree JSON Editor");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }
        
        [MenuItem("Tools/Passive Tree/Reset Template Presets")]
        static void ResetTemplatePresetsMenu()
        {
            var window = GetWindow<PassiveTreeJsonDataEditor>();
            window.ResetTemplatePresets();
        }
        
        [MenuItem("Tools/Passive Tree/Export Template Presets")]
        static void ExportTemplatePresetsMenu()
        {
            var window = GetWindow<PassiveTreeJsonDataEditor>();
            window.ExportTemplatePresets();
        }
        
        [MenuItem("Tools/Passive Tree/Save Node Names to Prefabs")]
        static void SaveNodeNamesToPrefabsMenu()
        {
            var window = GetWindow<PassiveTreeJsonDataEditor>();
            window.SaveNodeNamesToPrefabs();
        }
        
        void OnEnable()
        {
            if (usePrefabData)
            {
                RefreshCellDataFromPrefabs();
            }
            else
            {
                RefreshCellData();
            }
            InitializeDefaultPresets();
        }
        
        void InitializeDefaultPresets()
        {
            if (templatePresets.Count == 0)
            {
                // Warrior preset
                var warriorStats = new JsonStats();
                warriorStats.strength = 10;
                warriorStats.attackPower = 15;
                warriorStats.increasedPhysicalDamage = 20;
                warriorStats.armour = 25;
                templatePresets.Add(new StatTemplatePreset("Warrior", warriorStats));
                
                // Mage preset
                var mageStats = new JsonStats();
                mageStats.intelligence = 15;
                mageStats.increasedSpellDamage = 25;
                mageStats.maxMana = 50;
                mageStats.increasedFireDamage = 15;
                templatePresets.Add(new StatTemplatePreset("Mage", mageStats));
                
                // Rogue preset
                var rogueStats = new JsonStats();
                rogueStats.dexterity = 12;
                rogueStats.criticalChance = 10;
                rogueStats.criticalMultiplier = 25;
                rogueStats.evasion = 20;
                templatePresets.Add(new StatTemplatePreset("Rogue", rogueStats));
                
                // Tank preset
                var tankStats = new JsonStats();
                tankStats.maxHealthIncrease = 30;
                tankStats.armour = 40;
                tankStats.physicalResistance = 15;
                tankStats.lifeRegeneration = 5;
                templatePresets.Add(new StatTemplatePreset("Tank", tankStats));
                
                // Fire Elemental preset
                var fireStats = new JsonStats();
                fireStats.increasedFireDamage = 30;
                fireStats.addedFireDamage = 20;
                fireStats.fireResistance = 25;
                fireStats.chanceToIgnite = 15;
                templatePresets.Add(new StatTemplatePreset("Fire Elemental", fireStats));
                
                // Cold Elemental preset
                var coldStats = new JsonStats();
                coldStats.increasedColdDamage = 30;
                coldStats.addedColdDamage = 20;
                coldStats.coldResistance = 25;
                coldStats.chanceToFreeze = 15;
                templatePresets.Add(new StatTemplatePreset("Cold Elemental", coldStats));
                
                // Lightning Elemental preset
                var lightningStats = new JsonStats();
                lightningStats.increasedLightningDamage = 30;
                lightningStats.addedLightningDamage = 20;
                lightningStats.lightningResistance = 25;
                lightningStats.chanceToShock = 15;
                templatePresets.Add(new StatTemplatePreset("Lightning Elemental", lightningStats));
                
                // Card System preset
                var cardStats = new JsonStats();
                cardStats.cardsDrawnPerTurn = 2;
                cardStats.maxHandSize = 3;
                cardStats.cardDrawChance = 25;
                cardStats.manaPerTurn = 5;
                templatePresets.Add(new StatTemplatePreset("Card System", cardStats));
            }
        }
        
        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            
            // Header
            DrawHeader();
            
            // Tab selection
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
            EditorGUILayout.Space();
            
            // Tab content
            switch (selectedTab)
            {
                case 0:
                    DrawCellBrowser();
                    break;
                case 1:
                    DrawGridView();
                    break;
                case 2:
                    DrawStatEditor();
                    break;
                case 3:
                    DrawBulkOperations();
                    break;
                case 4:
                    DrawSearchAndFilter();
                    break;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("Passive Tree JSON Data Editor", EditorStyles.boldLabel);
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Save to Prefabs", GUILayout.Width(120)))
            {
                SaveAllChangesToPrefabs();
            }
            
            if (GUILayout.Button("Save Names", GUILayout.Width(100)))
            {
                SaveNodeNamesToPrefabs();
            }
            
            if (GUILayout.Button("Refresh", GUILayout.Width(80)))
            {
                if (usePrefabData)
                {
                    RefreshCellDataFromPrefabs();
                }
                else
                {
                    RefreshCellData();
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Data source toggle
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Data Source:", GUILayout.Width(80));
            bool newUsePrefabData = EditorGUILayout.Toggle("Use Prefab Data", usePrefabData);
            if (newUsePrefabData != usePrefabData)
            {
                usePrefabData = newUsePrefabData;
                if (usePrefabData)
                {
                    RefreshCellDataFromPrefabs();
                }
                else
                {
                    RefreshCellData();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
        }
        
        void DrawCellBrowser()
        {
            EditorGUILayout.LabelField("Cell Browser", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Board selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Board:", GUILayout.Width(50));
            GameObject newBoard = (GameObject)EditorGUILayout.ObjectField(selectedBoard, typeof(GameObject), true);
            if (newBoard != selectedBoard)
            {
                selectedBoard = newBoard;
                if (usePrefabData)
                {
                    RefreshCellDataFromPrefabs();
                }
                else
                {
                    RefreshCellData();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Cell list
            if (filteredCellJsonData.Count == 0)
            {
                EditorGUILayout.HelpBox("No cells found. Select a board or adjust filters.", MessageType.Info);
                return;
            }
            
            EditorGUILayout.LabelField($"Found {filteredCellJsonData.Count} cells", EditorStyles.miniLabel);
            EditorGUILayout.Space();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (var cellData in filteredCellJsonData)
            {
                if (cellData == null) continue;
                
                DrawCellItem(cellData);
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        void DrawGridView()
        {
            EditorGUILayout.LabelField("Grid View", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Board selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Board:", GUILayout.Width(50));
            GameObject newBoard = (GameObject)EditorGUILayout.ObjectField(selectedBoard, typeof(GameObject), true);
            if (newBoard != selectedBoard)
            {
                selectedBoard = newBoard;
                if (usePrefabData)
                {
                    RefreshCellDataFromPrefabs();
                }
                else
                {
                    RefreshCellData();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Grid settings
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Cell Size:", GUILayout.Width(70));
            cellSize = EditorGUILayout.Slider(cellSize, 40f, 100f);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Grid display
            if (allCellJsonData.Count == 0)
            {
                EditorGUILayout.HelpBox("No cells found. Select a board to see the grid.", MessageType.Info);
                return;
            }
            
            EditorGUILayout.LabelField($"Grid View - {allCellJsonData.Count} cells", EditorStyles.miniLabel);
            
            // Legend
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Legend:", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Start:", EditorStyles.miniLabel, GUILayout.Width(50));
            EditorGUILayout.LabelField("Green", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Travel:", EditorStyles.miniLabel, GUILayout.Width(50));
            EditorGUILayout.LabelField("White", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Notable:", EditorStyles.miniLabel, GUILayout.Width(50));
            EditorGUILayout.LabelField("Yellow", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Keystone:", EditorStyles.miniLabel, GUILayout.Width(50));
            EditorGUILayout.LabelField("Red", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Extension:", EditorStyles.miniLabel, GUILayout.Width(50));
            EditorGUILayout.LabelField("Cyan", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Create scrollable area for the grid
            gridScrollPosition = EditorGUILayout.BeginScrollView(gridScrollPosition, GUILayout.Height(500));
            
            // Draw 7x7 grid
            for (int row = 0; row < 7; row++)
            {
                EditorGUILayout.BeginHorizontal();
                
                for (int col = 0; col < 7; col++)
                {
                    DrawGridCell(row, col);
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        void DrawGridCell(int row, int col)
        {
            CellJsonData cellData = gridCells[row, col];
            
            // Determine cell style based on node type and selection
            string cellStyle = "box";
            if (cellData != null)
            {
                // Color code by node type
                switch (cellData.NodeType.ToLower())
                {
                    case "start":
                        cellStyle = "button";
                        break;
                    case "travel":
                        cellStyle = "box";
                        break;
                    case "notable":
                        cellStyle = "button";
                        break;
                    case "keystone":
                        cellStyle = "button";
                        break;
                    case "extension":
                        cellStyle = "box";
                        break;
                }
                
                // Highlight if selected
                if (selectedCell == cellData)
                {
                    cellStyle = "button";
                }
            }
            
            // Create a box for each cell
            EditorGUILayout.BeginVertical(cellStyle, GUILayout.Width(cellSize), GUILayout.Height(cellSize));
            
            if (cellData != null)
            {
                // Cell is occupied
                EditorGUILayout.LabelField($"{row},{col}", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.LabelField(cellData.NodeName, EditorStyles.centeredGreyMiniLabel, GUILayout.Height(20));
                
                // Color code the node type
                GUIStyle typeStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                switch (cellData.NodeType.ToLower())
                {
                    case "start":
                        typeStyle.normal.textColor = Color.green;
                        break;
                    case "travel":
                        typeStyle.normal.textColor = Color.white;
                        break;
                    case "notable":
                        typeStyle.normal.textColor = Color.yellow;
                        break;
                    case "keystone":
                        typeStyle.normal.textColor = Color.red;
                        break;
                    case "extension":
                        typeStyle.normal.textColor = Color.cyan;
                        break;
                }
                EditorGUILayout.LabelField(cellData.NodeType, typeStyle);
                
                // Show stats preview
                var stats = GetStatsPreview(cellData.NodeStats);
                if (stats.Count > 0)
                {
                    EditorGUILayout.LabelField(string.Join(", ", stats.Take(2)), EditorStyles.centeredGreyMiniLabel);
                }
                
                // Action buttons
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Edit", GUILayout.Width(30), GUILayout.Height(20)))
                {
                    selectedCell = cellData;
                    editingStats = cellData.NodeStats;
                    isEditing = true;
                    selectedTab = 2; // Switch to stat editor
                }
                
                if (bulkMode)
                {
                    bool isSelected = selectedCells.Contains(cellData);
                    if (GUILayout.Button(isSelected ? "X" : "+", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        if (isSelected)
                            selectedCells.Remove(cellData);
                        else
                            selectedCells.Add(cellData);
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                // Empty cell
                EditorGUILayout.LabelField($"{row},{col}", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.LabelField("Empty", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        void DrawCellItem(CellJsonData cellData)
        {
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            
            // Cell info
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField($"Cell: {cellData.gameObject.name}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Position: {cellData.NodePosition}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Type: {cellData.NodeType}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Name: {cellData.NodeName}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            
            GUILayout.FlexibleSpace();
            
            // Action buttons
            EditorGUILayout.BeginVertical();
            
            if (GUILayout.Button("Edit Stats", GUILayout.Width(80)))
            {
                selectedCell = cellData;
                editingStats = cellData.NodeStats;
                isEditing = true;
                selectedTab = 1; // Switch to stat editor
            }
            
            if (bulkMode)
            {
                bool isSelected = selectedCells.Contains(cellData);
                if (GUILayout.Button(isSelected ? "Deselect" : "Select", GUILayout.Width(80)))
                {
                    if (isSelected)
                        selectedCells.Remove(cellData);
                    else
                        selectedCells.Add(cellData);
                }
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            
            // Stats preview
            if (cellData.NodeStats != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Stats:", EditorStyles.miniLabel);
                
                var stats = GetStatsPreview(cellData.NodeStats);
                if (stats.Count > 0)
                {
                    EditorGUILayout.LabelField(string.Join(", ", stats.Take(3)), EditorStyles.miniLabel);
                    if (stats.Count > 3)
                    {
                        EditorGUILayout.LabelField($"... and {stats.Count - 3} more", EditorStyles.miniLabel);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No stats", EditorStyles.miniLabel);
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        
        void DrawStatEditor()
        {
            if (selectedCell == null)
            {
                EditorGUILayout.HelpBox("No cell selected. Go to Cell Browser tab to select a cell.", MessageType.Info);
                return;
            }
            
            EditorGUILayout.LabelField("Stat Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Cell info
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Editing: {selectedCell.gameObject.name}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Position: {selectedCell.NodePosition} | Type: {selectedCell.NodeType}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            if (editingStats == null)
            {
                EditorGUILayout.HelpBox("No stats data available for this cell.", MessageType.Warning);
                return;
            }
            
            // Stat editing
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            EditorGUILayout.LabelField("Core Attributes", EditorStyles.boldLabel);
            editingStats.strength = EditorGUILayout.IntField("Strength", editingStats.strength);
            editingStats.dexterity = EditorGUILayout.IntField("Dexterity", editingStats.dexterity);
            editingStats.intelligence = EditorGUILayout.IntField("Intelligence", editingStats.intelligence);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Combat Resources", EditorStyles.boldLabel);
            editingStats.maxHealthIncrease = EditorGUILayout.IntField("Max Health Increase", editingStats.maxHealthIncrease);
            editingStats.maxEnergyShieldIncrease = EditorGUILayout.IntField("Max Energy Shield Increase", editingStats.maxEnergyShieldIncrease);
            editingStats.maxMana = EditorGUILayout.IntField("Max Mana", editingStats.maxMana);
            editingStats.maxReliance = EditorGUILayout.IntField("Max Reliance", editingStats.maxReliance);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Combat Stats", EditorStyles.boldLabel);
            editingStats.attackPower = EditorGUILayout.IntField("Attack Power", editingStats.attackPower);
            editingStats.defense = EditorGUILayout.IntField("Defense", editingStats.defense);
            editingStats.criticalChance = EditorGUILayout.FloatField("Critical Chance", editingStats.criticalChance);
            editingStats.criticalMultiplier = EditorGUILayout.FloatField("Critical Multiplier", editingStats.criticalMultiplier);
            editingStats.accuracy = EditorGUILayout.FloatField("Accuracy", editingStats.accuracy);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Damage Modifiers (Increased)", EditorStyles.boldLabel);
            editingStats.increasedPhysicalDamage = EditorGUILayout.FloatField("Physical Damage", editingStats.increasedPhysicalDamage);
            editingStats.increasedFireDamage = EditorGUILayout.FloatField("Fire Damage", editingStats.increasedFireDamage);
            editingStats.increasedColdDamage = EditorGUILayout.FloatField("Cold Damage", editingStats.increasedColdDamage);
            editingStats.increasedLightningDamage = EditorGUILayout.FloatField("Lightning Damage", editingStats.increasedLightningDamage);
            editingStats.increasedChaosDamage = EditorGUILayout.FloatField("Chaos Damage", editingStats.increasedChaosDamage);
            editingStats.increasedElementalDamage = EditorGUILayout.FloatField("Elemental Damage", editingStats.increasedElementalDamage);
            editingStats.increasedSpellDamage = EditorGUILayout.FloatField("Spell Damage", editingStats.increasedSpellDamage);
            editingStats.increasedAttackDamage = EditorGUILayout.FloatField("Attack Damage", editingStats.increasedAttackDamage);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Added Damage", EditorStyles.boldLabel);
            editingStats.addedPhysicalDamage = EditorGUILayout.FloatField("Added Physical Damage", editingStats.addedPhysicalDamage);
            editingStats.addedFireDamage = EditorGUILayout.FloatField("Added Fire Damage", editingStats.addedFireDamage);
            editingStats.addedColdDamage = EditorGUILayout.FloatField("Added Cold Damage", editingStats.addedColdDamage);
            editingStats.addedLightningDamage = EditorGUILayout.FloatField("Added Lightning Damage", editingStats.addedLightningDamage);
            editingStats.addedChaosDamage = EditorGUILayout.FloatField("Added Chaos Damage", editingStats.addedChaosDamage);
            editingStats.addedElementalDamage = EditorGUILayout.FloatField("Added Elemental Damage", editingStats.addedElementalDamage);
            editingStats.addedSpellDamage = EditorGUILayout.FloatField("Added Spell Damage", editingStats.addedSpellDamage);
            editingStats.addedAttackDamage = EditorGUILayout.FloatField("Added Attack Damage", editingStats.addedAttackDamage);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Elemental Conversions", EditorStyles.boldLabel);
            editingStats.addedPhysicalAsFire = EditorGUILayout.IntField("Physical Damage as Fire", editingStats.addedPhysicalAsFire);
            editingStats.addedPhysicalAsCold = EditorGUILayout.IntField("Physical Damage as Cold", editingStats.addedPhysicalAsCold);
            editingStats.addedPhysicalAsLightning = EditorGUILayout.IntField("Physical Damage as Lightning", editingStats.addedPhysicalAsLightning);
            editingStats.addedFireAsCold = EditorGUILayout.IntField("Fire Damage as Cold", editingStats.addedFireAsCold);
            editingStats.addedColdAsFire = EditorGUILayout.IntField("Cold Damage as Fire", editingStats.addedColdAsFire);
            editingStats.addedLightningAsFire = EditorGUILayout.IntField("Lightning Damage as Fire", editingStats.addedLightningAsFire);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Resistances", EditorStyles.boldLabel);
            editingStats.physicalResistance = EditorGUILayout.FloatField("Physical Resistance", editingStats.physicalResistance);
            editingStats.fireResistance = EditorGUILayout.FloatField("Fire Resistance", editingStats.fireResistance);
            editingStats.coldResistance = EditorGUILayout.FloatField("Cold Resistance", editingStats.coldResistance);
            editingStats.lightningResistance = EditorGUILayout.FloatField("Lightning Resistance", editingStats.lightningResistance);
            editingStats.chaosResistance = EditorGUILayout.FloatField("Chaos Resistance", editingStats.chaosResistance);
            editingStats.elementalResistance = EditorGUILayout.FloatField("Elemental Resistance", editingStats.elementalResistance);
            editingStats.allResistance = EditorGUILayout.FloatField("All Resistance", editingStats.allResistance);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Defense Stats", EditorStyles.boldLabel);
            editingStats.armour = EditorGUILayout.IntField("Armour", editingStats.armour);
            editingStats.evasion = EditorGUILayout.FloatField("Evasion", editingStats.evasion);
            editingStats.energyShield = EditorGUILayout.IntField("Energy Shield", editingStats.energyShield);
            editingStats.blockChance = EditorGUILayout.FloatField("Block Chance", editingStats.blockChance);
            editingStats.dodgeChance = EditorGUILayout.FloatField("Dodge Chance", editingStats.dodgeChance);
            editingStats.spellDodgeChance = EditorGUILayout.FloatField("Spell Dodge Chance", editingStats.spellDodgeChance);
            editingStats.spellBlockChance = EditorGUILayout.FloatField("Spell Block Chance", editingStats.spellBlockChance);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Recovery Stats", EditorStyles.boldLabel);
            editingStats.lifeRegeneration = EditorGUILayout.FloatField("Life Regeneration", editingStats.lifeRegeneration);
            editingStats.energyShieldRegeneration = EditorGUILayout.FloatField("Energy Shield Regeneration", editingStats.energyShieldRegeneration);
            editingStats.manaRegeneration = EditorGUILayout.FloatField("Mana Regeneration", editingStats.manaRegeneration);
            editingStats.relianceRegeneration = EditorGUILayout.FloatField("Reliance Regeneration", editingStats.relianceRegeneration);
            editingStats.lifeLeech = EditorGUILayout.FloatField("Life Leech", editingStats.lifeLeech);
            editingStats.manaLeech = EditorGUILayout.FloatField("Mana Leech", editingStats.manaLeech);
            editingStats.energyShieldLeech = EditorGUILayout.FloatField("Energy Shield Leech", editingStats.energyShieldLeech);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Combat Mechanics", EditorStyles.boldLabel);
            editingStats.attackSpeed = EditorGUILayout.FloatField("Attack Speed", editingStats.attackSpeed);
            editingStats.castSpeed = EditorGUILayout.FloatField("Cast Speed", editingStats.castSpeed);
            editingStats.movementSpeed = EditorGUILayout.FloatField("Movement Speed", editingStats.movementSpeed);
            editingStats.attackRange = EditorGUILayout.FloatField("Attack Range", editingStats.attackRange);
            editingStats.projectileSpeed = EditorGUILayout.FloatField("Projectile Speed", editingStats.projectileSpeed);
            editingStats.areaOfEffect = EditorGUILayout.FloatField("Area of Effect", editingStats.areaOfEffect);
            editingStats.skillEffectDuration = EditorGUILayout.FloatField("Skill Effect Duration", editingStats.skillEffectDuration);
            editingStats.statusEffectDuration = EditorGUILayout.FloatField("Status Effect Duration", editingStats.statusEffectDuration);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Card System Stats", EditorStyles.boldLabel);
            editingStats.cardsDrawnPerTurn = EditorGUILayout.IntField("Cards Drawn Per Turn", editingStats.cardsDrawnPerTurn);
            editingStats.maxHandSize = EditorGUILayout.IntField("Max Hand Size", editingStats.maxHandSize);
            editingStats.cardDrawChance = EditorGUILayout.FloatField("Card Draw Chance", editingStats.cardDrawChance);
            editingStats.cardRetentionChance = EditorGUILayout.FloatField("Card Retention Chance", editingStats.cardRetentionChance);
            editingStats.cardUpgradeChance = EditorGUILayout.FloatField("Card Upgrade Chance", editingStats.cardUpgradeChance);
            editingStats.discardPower = EditorGUILayout.FloatField("Discard Power", editingStats.discardPower);
            editingStats.manaPerTurn = EditorGUILayout.FloatField("Mana Per Turn", editingStats.manaPerTurn);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Legacy Stats", EditorStyles.boldLabel);
            editingStats.armorIncrease = EditorGUILayout.IntField("Armor Increase", editingStats.armorIncrease);
            editingStats.increasedEvasion = EditorGUILayout.FloatField("Increased Evasion", editingStats.increasedEvasion);
            editingStats.elementalResist = EditorGUILayout.FloatField("Elemental Resist", editingStats.elementalResist);
            editingStats.spellPowerIncrease = EditorGUILayout.FloatField("Spell Power Increase", editingStats.spellPowerIncrease);
            editingStats.critChanceIncrease = EditorGUILayout.FloatField("Crit Chance Increase", editingStats.critChanceIncrease);
            editingStats.critMultiplierIncrease = EditorGUILayout.FloatField("Crit Multiplier Increase", editingStats.critMultiplierIncrease);
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            
            // Action buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Apply Changes", GUILayout.Height(30)))
            {
                ApplyChanges();
            }
            
            if (GUILayout.Button("Reset", GUILayout.Height(30)))
            {
                ResetChanges();
            }
            
            if (GUILayout.Button("Close", GUILayout.Height(30)))
            {
                selectedCell = null;
                isEditing = false;
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        void DrawBulkOperations()
        {
            EditorGUILayout.LabelField("Bulk Operations", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Stat Template Editor
            EditorGUILayout.LabelField("Stat Template", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Template Presets section
            EditorGUILayout.LabelField("Template Presets", EditorStyles.miniBoldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Select Preset:", GUILayout.Width(80));
            selectedPresetIndex = EditorGUILayout.Popup(selectedPresetIndex, templatePresets.Select(p => p.name).ToArray());
            
            if (GUILayout.Button("Load Preset", GUILayout.Width(80)))
            {
                if (selectedPresetIndex >= 0 && selectedPresetIndex < templatePresets.Count)
                {
                    statTemplate = templatePresets[selectedPresetIndex].stats;
                    Debug.Log($"[PassiveTreeJsonDataEditor] Loaded preset: {templatePresets[selectedPresetIndex].name}");
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            newPresetName = EditorGUILayout.TextField("Save as:", newPresetName);
            if (GUILayout.Button("Save Preset", GUILayout.Width(80)))
            {
                if (!string.IsNullOrEmpty(newPresetName))
                {
                    var newPreset = new StatTemplatePreset(newPresetName, statTemplate);
                    templatePresets.Add(newPreset);
                    selectedPresetIndex = templatePresets.Count - 1;
                    newPresetName = "";
                    Debug.Log($"[PassiveTreeJsonDataEditor] Saved preset: {newPreset.name}");
                }
            }
            if (GUILayout.Button("Delete Preset", GUILayout.Width(90)))
            {
                if (selectedPresetIndex >= 0 && selectedPresetIndex < templatePresets.Count)
                {
                    string deletedName = templatePresets[selectedPresetIndex].name;
                    templatePresets.RemoveAt(selectedPresetIndex);
                    if (selectedPresetIndex >= templatePresets.Count)
                        selectedPresetIndex = Mathf.Max(0, templatePresets.Count - 1);
                    Debug.Log($"[PassiveTreeJsonDataEditor] Deleted preset: {deletedName}");
                }
            }
            EditorGUILayout.EndHorizontal();
            
            showTemplateEditor = EditorGUILayout.Foldout(showTemplateEditor, "Edit Stat Template");
            if (showTemplateEditor)
            {
                DrawStatTemplateEditor();
            }
            
            EditorGUILayout.Space();
            
            // Node Name Generation Settings
            EditorGUILayout.LabelField("Node Name Generation", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            autoGenerateNodeNames = EditorGUILayout.Toggle("Auto Generate Node Names", autoGenerateNodeNames);
            
            if (autoGenerateNodeNames)
            {
                EditorGUILayout.BeginVertical("box");
                
                nodeNamePrefix = EditorGUILayout.TextField("Name Prefix", nodeNamePrefix);
                includePositionInName = EditorGUILayout.Toggle("Include Position in Name", includePositionInName);
                includeStatsInName = EditorGUILayout.Toggle("Include Stats in Name", includeStatsInName);
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);
                string previewName = GenerateNodeName(statTemplate, new Vector2Int(3, 4));
                EditorGUILayout.LabelField($"Example: {previewName}", EditorStyles.miniLabel);
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Space();
            
            // Bulk mode toggle
            bulkMode = EditorGUILayout.Toggle("Bulk Mode", bulkMode);
            
            if (bulkMode)
            {
                EditorGUILayout.HelpBox("Bulk mode is enabled. You can select multiple cells in the Cell Browser or Grid View tab.", MessageType.Info);
                EditorGUILayout.Space();
                
                EditorGUILayout.LabelField($"Selected Cells: {selectedCells.Count}", EditorStyles.boldLabel);
                
                if (selectedCells.Count > 0)
                {
                    EditorGUILayout.BeginVertical("box");
                    foreach (var cell in selectedCells)
                    {
                        if (cell != null)
                        {
                            EditorGUILayout.LabelField($"• {cell.gameObject.name} - {cell.NodeName}");
                        }
                    }
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.Space();
                    
                    // Bulk operations
                    EditorGUILayout.BeginHorizontal();
                    
                    if (GUILayout.Button("Clear Selection"))
                    {
                        selectedCells.Clear();
                    }
                    
                    if (GUILayout.Button("Select All"))
                    {
                        selectedCells.Clear();
                        selectedCells.AddRange(filteredCellJsonData);
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.Space();
                    
                    // Bulk stat operations
                    EditorGUILayout.LabelField("Bulk Stat Operations", EditorStyles.boldLabel);
                    
                    if (GUILayout.Button("Apply Stat Template", GUILayout.Height(30)))
                    {
                        ApplyStatTemplate();
                    }
                    
                    if (GUILayout.Button("Clear All Stats", GUILayout.Height(30)))
                    {
                        ClearAllStats();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Enable bulk mode to perform operations on multiple cells.", MessageType.Info);
            }
        }
        
        void DrawStatTemplateEditor()
        {
            EditorGUILayout.BeginVertical("box");
            
            templateScrollPosition = EditorGUILayout.BeginScrollView(templateScrollPosition, GUILayout.Height(300));
            
            EditorGUILayout.LabelField("Core Attributes", EditorStyles.boldLabel);
            statTemplate.strength = EditorGUILayout.IntField("Strength", statTemplate.strength);
            statTemplate.dexterity = EditorGUILayout.IntField("Dexterity", statTemplate.dexterity);
            statTemplate.intelligence = EditorGUILayout.IntField("Intelligence", statTemplate.intelligence);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Combat Resources", EditorStyles.boldLabel);
            statTemplate.maxHealthIncrease = EditorGUILayout.IntField("Max Health Increase", statTemplate.maxHealthIncrease);
            statTemplate.maxEnergyShieldIncrease = EditorGUILayout.IntField("Max Energy Shield Increase", statTemplate.maxEnergyShieldIncrease);
            statTemplate.maxMana = EditorGUILayout.IntField("Max Mana", statTemplate.maxMana);
            statTemplate.maxReliance = EditorGUILayout.IntField("Max Reliance", statTemplate.maxReliance);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Combat Stats", EditorStyles.boldLabel);
            statTemplate.attackPower = EditorGUILayout.IntField("Attack Power", statTemplate.attackPower);
            statTemplate.defense = EditorGUILayout.IntField("Defense", statTemplate.defense);
            statTemplate.criticalChance = EditorGUILayout.FloatField("Critical Chance", statTemplate.criticalChance);
            statTemplate.criticalMultiplier = EditorGUILayout.FloatField("Critical Multiplier", statTemplate.criticalMultiplier);
            statTemplate.accuracy = EditorGUILayout.FloatField("Accuracy", statTemplate.accuracy);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Damage Modifiers (Increased)", EditorStyles.boldLabel);
            statTemplate.increasedPhysicalDamage = EditorGUILayout.FloatField("Physical Damage", statTemplate.increasedPhysicalDamage);
            statTemplate.increasedFireDamage = EditorGUILayout.FloatField("Fire Damage", statTemplate.increasedFireDamage);
            statTemplate.increasedColdDamage = EditorGUILayout.FloatField("Cold Damage", statTemplate.increasedColdDamage);
            statTemplate.increasedLightningDamage = EditorGUILayout.FloatField("Lightning Damage", statTemplate.increasedLightningDamage);
            statTemplate.increasedChaosDamage = EditorGUILayout.FloatField("Chaos Damage", statTemplate.increasedChaosDamage);
            statTemplate.increasedElementalDamage = EditorGUILayout.FloatField("Elemental Damage", statTemplate.increasedElementalDamage);
            statTemplate.increasedSpellDamage = EditorGUILayout.FloatField("Spell Damage", statTemplate.increasedSpellDamage);
            statTemplate.increasedAttackDamage = EditorGUILayout.FloatField("Attack Damage", statTemplate.increasedAttackDamage);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Added Damage", EditorStyles.boldLabel);
            statTemplate.addedPhysicalDamage = EditorGUILayout.FloatField("Added Physical Damage", statTemplate.addedPhysicalDamage);
            statTemplate.addedFireDamage = EditorGUILayout.FloatField("Added Fire Damage", statTemplate.addedFireDamage);
            statTemplate.addedColdDamage = EditorGUILayout.FloatField("Added Cold Damage", statTemplate.addedColdDamage);
            statTemplate.addedLightningDamage = EditorGUILayout.FloatField("Added Lightning Damage", statTemplate.addedLightningDamage);
            statTemplate.addedChaosDamage = EditorGUILayout.FloatField("Added Chaos Damage", statTemplate.addedChaosDamage);
            statTemplate.addedElementalDamage = EditorGUILayout.FloatField("Added Elemental Damage", statTemplate.addedElementalDamage);
            statTemplate.addedSpellDamage = EditorGUILayout.FloatField("Added Spell Damage", statTemplate.addedSpellDamage);
            statTemplate.addedAttackDamage = EditorGUILayout.FloatField("Added Attack Damage", statTemplate.addedAttackDamage);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Elemental Conversions", EditorStyles.boldLabel);
            statTemplate.addedPhysicalAsFire = EditorGUILayout.IntField("Physical Damage as Fire", statTemplate.addedPhysicalAsFire);
            statTemplate.addedPhysicalAsCold = EditorGUILayout.IntField("Physical Damage as Cold", statTemplate.addedPhysicalAsCold);
            statTemplate.addedPhysicalAsLightning = EditorGUILayout.IntField("Physical Damage as Lightning", statTemplate.addedPhysicalAsLightning);
            statTemplate.addedFireAsCold = EditorGUILayout.IntField("Fire Damage as Cold", statTemplate.addedFireAsCold);
            statTemplate.addedColdAsFire = EditorGUILayout.IntField("Cold Damage as Fire", statTemplate.addedColdAsFire);
            statTemplate.addedLightningAsFire = EditorGUILayout.IntField("Lightning Damage as Fire", statTemplate.addedLightningAsFire);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Resistances", EditorStyles.boldLabel);
            statTemplate.physicalResistance = EditorGUILayout.FloatField("Physical Resistance", statTemplate.physicalResistance);
            statTemplate.fireResistance = EditorGUILayout.FloatField("Fire Resistance", statTemplate.fireResistance);
            statTemplate.coldResistance = EditorGUILayout.FloatField("Cold Resistance", statTemplate.coldResistance);
            statTemplate.lightningResistance = EditorGUILayout.FloatField("Lightning Resistance", statTemplate.lightningResistance);
            statTemplate.chaosResistance = EditorGUILayout.FloatField("Chaos Resistance", statTemplate.chaosResistance);
            statTemplate.elementalResistance = EditorGUILayout.FloatField("Elemental Resistance", statTemplate.elementalResistance);
            statTemplate.allResistance = EditorGUILayout.FloatField("All Resistance", statTemplate.allResistance);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Defense Stats", EditorStyles.boldLabel);
            statTemplate.armour = EditorGUILayout.IntField("Armour", statTemplate.armour);
            statTemplate.evasion = EditorGUILayout.FloatField("Evasion", statTemplate.evasion);
            statTemplate.energyShield = EditorGUILayout.IntField("Energy Shield", statTemplate.energyShield);
            statTemplate.blockChance = EditorGUILayout.FloatField("Block Chance", statTemplate.blockChance);
            statTemplate.dodgeChance = EditorGUILayout.FloatField("Dodge Chance", statTemplate.dodgeChance);
            statTemplate.spellDodgeChance = EditorGUILayout.FloatField("Spell Dodge Chance", statTemplate.spellDodgeChance);
            statTemplate.spellBlockChance = EditorGUILayout.FloatField("Spell Block Chance", statTemplate.spellBlockChance);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Recovery Stats", EditorStyles.boldLabel);
            statTemplate.lifeRegeneration = EditorGUILayout.FloatField("Life Regeneration", statTemplate.lifeRegeneration);
            statTemplate.energyShieldRegeneration = EditorGUILayout.FloatField("Energy Shield Regeneration", statTemplate.energyShieldRegeneration);
            statTemplate.manaRegeneration = EditorGUILayout.FloatField("Mana Regeneration", statTemplate.manaRegeneration);
            statTemplate.relianceRegeneration = EditorGUILayout.FloatField("Reliance Regeneration", statTemplate.relianceRegeneration);
            statTemplate.lifeLeech = EditorGUILayout.FloatField("Life Leech", statTemplate.lifeLeech);
            statTemplate.manaLeech = EditorGUILayout.FloatField("Mana Leech", statTemplate.manaLeech);
            statTemplate.energyShieldLeech = EditorGUILayout.FloatField("Energy Shield Leech", statTemplate.energyShieldLeech);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Combat Mechanics", EditorStyles.boldLabel);
            statTemplate.attackSpeed = EditorGUILayout.FloatField("Attack Speed", statTemplate.attackSpeed);
            statTemplate.castSpeed = EditorGUILayout.FloatField("Cast Speed", statTemplate.castSpeed);
            statTemplate.movementSpeed = EditorGUILayout.FloatField("Movement Speed", statTemplate.movementSpeed);
            statTemplate.attackRange = EditorGUILayout.FloatField("Attack Range", statTemplate.attackRange);
            statTemplate.projectileSpeed = EditorGUILayout.FloatField("Projectile Speed", statTemplate.projectileSpeed);
            statTemplate.areaOfEffect = EditorGUILayout.FloatField("Area of Effect", statTemplate.areaOfEffect);
            statTemplate.skillEffectDuration = EditorGUILayout.FloatField("Skill Effect Duration", statTemplate.skillEffectDuration);
            statTemplate.statusEffectDuration = EditorGUILayout.FloatField("Status Effect Duration", statTemplate.statusEffectDuration);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Card System Stats", EditorStyles.boldLabel);
            statTemplate.cardsDrawnPerTurn = EditorGUILayout.IntField("Cards Drawn Per Turn", statTemplate.cardsDrawnPerTurn);
            statTemplate.maxHandSize = EditorGUILayout.IntField("Max Hand Size", statTemplate.maxHandSize);
            statTemplate.cardDrawChance = EditorGUILayout.FloatField("Card Draw Chance", statTemplate.cardDrawChance);
            statTemplate.cardRetentionChance = EditorGUILayout.FloatField("Card Retention Chance", statTemplate.cardRetentionChance);
            statTemplate.cardUpgradeChance = EditorGUILayout.FloatField("Card Upgrade Chance", statTemplate.cardUpgradeChance);
            statTemplate.discardPower = EditorGUILayout.FloatField("Discard Power", statTemplate.discardPower);
            statTemplate.manaPerTurn = EditorGUILayout.FloatField("Mana Per Turn", statTemplate.manaPerTurn);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Legacy Stats", EditorStyles.boldLabel);
            statTemplate.armorIncrease = EditorGUILayout.IntField("Armor Increase", statTemplate.armorIncrease);
            statTemplate.increasedEvasion = EditorGUILayout.FloatField("Increased Evasion", statTemplate.increasedEvasion);
            statTemplate.elementalResist = EditorGUILayout.FloatField("Elemental Resist", statTemplate.elementalResist);
            statTemplate.spellPowerIncrease = EditorGUILayout.FloatField("Spell Power Increase", statTemplate.spellPowerIncrease);
            statTemplate.critChanceIncrease = EditorGUILayout.FloatField("Crit Chance Increase", statTemplate.critChanceIncrease);
            statTemplate.critMultiplierIncrease = EditorGUILayout.FloatField("Crit Multiplier Increase", statTemplate.critMultiplierIncrease);
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            
            // Template actions
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Reset Template", GUILayout.Height(25)))
            {
                statTemplate = new JsonStats();
            }
            
            if (GUILayout.Button("Load from Selected Cell", GUILayout.Height(25)))
            {
                if (selectedCell != null && selectedCell.NodeStats != null)
                {
                    statTemplate = selectedCell.NodeStats;
                }
                else
                {
                    Debug.LogWarning("No cell selected or cell has no stats to load from.");
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        void DrawSearchAndFilter()
        {
            EditorGUILayout.LabelField("Search and Filter", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Search filter
            EditorGUILayout.LabelField("Search Filter", EditorStyles.boldLabel);
            string newSearchFilter = EditorGUILayout.TextField("Search by name or position:", searchFilter);
            if (newSearchFilter != searchFilter)
            {
                searchFilter = newSearchFilter;
                ApplyFilters();
            }
            
            EditorGUILayout.Space();
            
            // Node type filter
            EditorGUILayout.LabelField("Node Type Filter", EditorStyles.boldLabel);
            string[] nodeTypes = { "All", "Start", "Travel", "Notable", "Keystone", "Extension" };
            int currentIndex = System.Array.IndexOf(nodeTypes, nodeTypeFilter);
            int newIndex = EditorGUILayout.Popup("Node Type:", currentIndex, nodeTypes);
            if (newIndex != currentIndex)
            {
                nodeTypeFilter = nodeTypes[newIndex];
                ApplyFilters();
            }
            
            EditorGUILayout.Space();
            
            // Additional filters
            EditorGUILayout.LabelField("Additional Filters", EditorStyles.boldLabel);
            
            bool newShowOnlyAllocated = EditorGUILayout.Toggle("Show only allocated nodes", showOnlyAllocated);
            if (newShowOnlyAllocated != showOnlyAllocated)
            {
                showOnlyAllocated = newShowOnlyAllocated;
                ApplyFilters();
            }
            
            bool newShowOnlyWithStats = EditorGUILayout.Toggle("Show only nodes with stats", showOnlyWithStats);
            if (newShowOnlyWithStats != showOnlyWithStats)
            {
                showOnlyWithStats = newShowOnlyWithStats;
                ApplyFilters();
            }
            
            EditorGUILayout.Space();
            
            // Filter results
            EditorGUILayout.LabelField($"Filter Results: {filteredCellJsonData.Count} cells", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Clear All Filters"))
            {
                searchFilter = "";
                nodeTypeFilter = "All";
                showOnlyAllocated = false;
                showOnlyWithStats = false;
                ApplyFilters();
            }
        }
        
        void RefreshCellData()
        {
            allCellJsonData.Clear();
            filteredCellJsonData.Clear();
            
            // Clear the grid
            for (int row = 0; row < 7; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    gridCells[row, col] = null;
                }
            }
            
            if (selectedBoard != null)
            {
                CellJsonData[] cells = selectedBoard.GetComponentsInChildren<CellJsonData>();
                allCellJsonData.AddRange(cells);
                
                // Populate the grid based on cell positions
                foreach (var cell in cells)
                {
                    if (cell != null)
                    {
                        Vector2Int pos = cell.NodePosition;
                        if (pos.x >= 0 && pos.x < 7 && pos.y >= 0 && pos.y < 7)
                        {
                            gridCells[pos.x, pos.y] = cell;
                        }
                    }
                }
            }
            else
            {
                // Find all CellJsonData in the scene
                CellJsonData[] allCells = FindObjectsOfType<CellJsonData>();
                allCellJsonData.AddRange(allCells);
                
                // Populate the grid based on cell positions
                foreach (var cell in allCells)
                {
                    if (cell != null)
                    {
                        Vector2Int pos = cell.NodePosition;
                        if (pos.x >= 0 && pos.x < 7 && pos.y >= 0 && pos.y < 7)
                        {
                            gridCells[pos.x, pos.y] = cell;
                        }
                    }
                }
            }
            
            ApplyFilters();
        }
        
        void RefreshCellDataFromPrefabs()
        {
            allCellJsonData.Clear();
            filteredCellJsonData.Clear();
            
            // Clear the grid
            for (int row = 0; row < 7; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    gridCells[row, col] = null;
                }
            }
            
            if (selectedBoard != null)
            {
                CellJsonData[] sceneCells = selectedBoard.GetComponentsInChildren<CellJsonData>();
                
                foreach (var sceneCell in sceneCells)
                {
                    if (sceneCell != null)
                    {
                        // Get the prefab asset for this cell
                        var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(sceneCell.gameObject);
                        if (prefabAsset != null)
                        {
                            // Get the CellJsonData from the prefab asset
                            var prefabCellData = prefabAsset.GetComponent<CellJsonData>();
                            if (prefabCellData != null)
                            {
                                allCellJsonData.Add(prefabCellData);
                                
                                // Populate the grid based on cell positions
                                Vector2Int pos = prefabCellData.NodePosition;
                                if (pos.x >= 0 && pos.x < 7 && pos.y >= 0 && pos.y < 7)
                                {
                                    gridCells[pos.x, pos.y] = prefabCellData;
                                }
                            }
                        }
                        else
                        {
                            // If not a prefab instance, use the scene data
                            allCellJsonData.Add(sceneCell);
                            
                            Vector2Int pos = sceneCell.NodePosition;
                            if (pos.x >= 0 && pos.x < 7 && pos.y >= 0 && pos.y < 7)
                            {
                                gridCells[pos.x, pos.y] = sceneCell;
                            }
                        }
                    }
                }
                
                Debug.Log($"Found {allCellJsonData.Count} cells with JSON data from prefabs in {selectedBoard.name}");
            }
            else
            {
                Debug.LogWarning("No board selected. Please select a board GameObject.");
            }
            
            ApplyFilters();
        }
        
        void ApplyFilters()
        {
            filteredCellJsonData.Clear();
            
            foreach (var cell in allCellJsonData)
            {
                if (cell == null) continue;
                
                // Search filter
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    string searchLower = searchFilter.ToLower();
                    if (!cell.gameObject.name.ToLower().Contains(searchLower) &&
                        !cell.NodeName.ToLower().Contains(searchLower) &&
                        !cell.NodePosition.ToString().Contains(searchFilter))
                    {
                        continue;
                    }
                }
                
                // Node type filter
                if (nodeTypeFilter != "All" && cell.NodeType != nodeTypeFilter)
                {
                    continue;
                }
                
                // Allocated filter
                if (showOnlyAllocated)
                {
                    var cellController = cell.GetComponent<CellController>();
                    if (cellController == null || !cellController.IsPurchased)
                    {
                        continue;
                    }
                }
                
                // Stats filter
                if (showOnlyWithStats)
                {
                    if (cell.NodeStats == null || !HasAnyStats(cell.NodeStats))
                    {
                        continue;
                    }
                }
                
                filteredCellJsonData.Add(cell);
            }
        }
        
        bool HasAnyStats(JsonStats stats)
        {
            if (stats == null) return false;
            
            // Check a few key stats
            return stats.strength != 0 || stats.dexterity != 0 || stats.intelligence != 0 ||
                   stats.increasedPhysicalDamage != 0 || stats.increasedFireDamage != 0 ||
                   stats.addedPhysicalAsFire != 0 || stats.armorIncrease != 0;
        }
        
        List<string> GetStatsPreview(JsonStats stats)
        {
            var preview = new List<string>();
            
            if (stats.strength != 0) preview.Add($"Str: {stats.strength}");
            if (stats.dexterity != 0) preview.Add($"Dex: {stats.dexterity}");
            if (stats.intelligence != 0) preview.Add($"Int: {stats.intelligence}");
            if (stats.increasedPhysicalDamage != 0) preview.Add($"Phys: {stats.increasedPhysicalDamage}%");
            if (stats.addedPhysicalAsFire != 0) preview.Add($"Phys→Fire: {stats.addedPhysicalAsFire}%");
            
            return preview;
        }
        
        void ApplyChanges()
        {
            if (selectedCell == null || editingStats == null) return;
            
            // Apply the changes to the selected cell using the new method
            selectedCell.UpdateNodeStats(editingStats);
            
            // Mark the object as dirty
            EditorUtility.SetDirty(selectedCell);
            EditorUtility.SetDirty(selectedCell.gameObject);
            
            // Save changes to prefab if this is a prefab instance
            SaveChangesToPrefab(selectedCell);
            
            Debug.Log($"Applied stat changes to {selectedCell.gameObject.name}");
        }
        
        void SaveChangesToPrefab(CellJsonData cellData)
        {
            if (cellData == null) return;
            
            if (usePrefabData)
            {
                // When using prefab data, we're already working with prefab assets
                // Just mark as dirty and save
                EditorUtility.SetDirty(cellData);
                EditorUtility.SetDirty(cellData.gameObject);
                Debug.Log($"Saved changes to prefab asset: {cellData.gameObject.name} (Stats + Name)");
            }
            else
            {
                // Check if this is a prefab instance
                var prefabInstance = PrefabUtility.GetCorrespondingObjectFromSource(cellData.gameObject);
                if (prefabInstance != null)
                {
                    // This is a prefab instance, save changes to the prefab
                    var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(cellData.gameObject);
                    if (prefabAsset != null)
                    {
                        // Apply changes to the prefab asset
                        var prefabCellData = prefabAsset.GetComponent<CellJsonData>();
                        if (prefabCellData != null)
                        {
                            // Save stats
                            prefabCellData.UpdateNodeStats(cellData.NodeStats);
                            
                            // Save node name if it was manually set
                            if (cellData.NodeName != prefabCellData.NodeName)
                            {
                                prefabCellData.SetNodeName(cellData.NodeName);
                            }
                            
                            // Save GameObject name if it changed
                            if (cellData.gameObject.name != prefabAsset.name)
                            {
                                prefabAsset.name = cellData.gameObject.name;
                            }
                            
                            EditorUtility.SetDirty(prefabAsset);
                            
                            Debug.Log($"Saved changes to prefab asset: {prefabAsset.name} (Stats + Name)");
                        }
                    }
                }
                else
                {
                    // This is not a prefab instance, just mark as dirty
                    EditorUtility.SetDirty(cellData);
                    Debug.Log($"Marked {cellData.gameObject.name} as dirty (not a prefab instance)");
                }
            }
        }
        
        void ResetChanges()
        {
            if (selectedCell == null) return;
            
            // Reset to original values
            editingStats = selectedCell.NodeStats;
        }
        
        void ApplyStatTemplate()
        {
            if (selectedCells.Count == 0) return;
            
            foreach (var cell in selectedCells)
            {
                if (cell != null)
                {
                    cell.UpdateNodeStats(statTemplate);
                    
                    // Generate new node name if enabled
                    if (autoGenerateNodeNames)
                    {
                        string newName = GenerateNodeName(statTemplate, cell.NodePosition);
                        cell.SetNodeName(newName);
                    }
                    
                    EditorUtility.SetDirty(cell);
                    SaveChangesToPrefab(cell);
                }
            }
            
            Debug.Log($"Applied stat template to {selectedCells.Count} cells");
        }
        
        string GenerateNodeName(JsonStats stats, Vector2Int position)
        {
            string name = nodeNamePrefix;
            
            // Add position if enabled
            if (includePositionInName)
            {
                name += $"_{position.x}_{position.y}";
            }
            
            // Add stats if enabled
            if (includeStatsInName)
            {
                var statNames = GetStatNames(stats);
                if (statNames.Count > 0)
                {
                    name += "_" + string.Join(" & ", statNames);
                }
            }
            
            return name;
        }
        
        List<string> GetStatNames(JsonStats stats)
        {
            var statNames = new List<string>();
            
            // Core Attributes
            if (stats.strength != 0) statNames.Add("Strength");
            if (stats.dexterity != 0) statNames.Add("Dexterity");
            if (stats.intelligence != 0) statNames.Add("Intelligence");
            
            // Combat Resources
            if (stats.maxHealthIncrease != 0) statNames.Add("Max Health");
            if (stats.maxEnergyShieldIncrease != 0) statNames.Add("Max Energy Shield");
            if (stats.maxMana != 0) statNames.Add("Max Mana");
            if (stats.maxReliance != 0) statNames.Add("Max Reliance");
            
            // Combat Stats
            if (stats.attackPower != 0) statNames.Add("Attack Power");
            if (stats.defense != 0) statNames.Add("Defense");
            if (stats.criticalChance != 0) statNames.Add("Critical Chance");
            if (stats.criticalMultiplier != 0) statNames.Add("Critical Multiplier");
            if (stats.accuracy != 0) statNames.Add("Accuracy");
            
            // Damage Modifiers (Increased)
            if (stats.increasedPhysicalDamage != 0) statNames.Add("Increased Physical Damage");
            if (stats.increasedFireDamage != 0) statNames.Add("Increased Fire Damage");
            if (stats.increasedColdDamage != 0) statNames.Add("Increased Cold Damage");
            if (stats.increasedLightningDamage != 0) statNames.Add("Increased Lightning Damage");
            if (stats.increasedChaosDamage != 0) statNames.Add("Increased Chaos Damage");
            if (stats.increasedElementalDamage != 0) statNames.Add("Increased Elemental Damage");
            if (stats.increasedSpellDamage != 0) statNames.Add("Increased Spell Damage");
            if (stats.increasedAttackDamage != 0) statNames.Add("Increased Attack Damage");
            
            // Added Damage
            if (stats.addedPhysicalDamage != 0) statNames.Add("Added Physical Damage");
            if (stats.addedFireDamage != 0) statNames.Add("Added Fire Damage");
            if (stats.addedColdDamage != 0) statNames.Add("Added Cold Damage");
            if (stats.addedLightningDamage != 0) statNames.Add("Added Lightning Damage");
            if (stats.addedChaosDamage != 0) statNames.Add("Added Chaos Damage");
            if (stats.addedElementalDamage != 0) statNames.Add("Added Elemental Damage");
            if (stats.addedSpellDamage != 0) statNames.Add("Added Spell Damage");
            if (stats.addedAttackDamage != 0) statNames.Add("Added Attack Damage");
            
            // Elemental Conversions
            if (stats.addedPhysicalAsFire != 0) statNames.Add("Physical as Fire");
            if (stats.addedPhysicalAsCold != 0) statNames.Add("Physical as Cold");
            if (stats.addedPhysicalAsLightning != 0) statNames.Add("Physical as Lightning");
            if (stats.addedFireAsCold != 0) statNames.Add("Fire as Cold");
            if (stats.addedColdAsFire != 0) statNames.Add("Cold as Fire");
            if (stats.addedLightningAsFire != 0) statNames.Add("Lightning as Fire");
            
            // Resistances
            if (stats.physicalResistance != 0) statNames.Add("Physical Resistance");
            if (stats.fireResistance != 0) statNames.Add("Fire Resistance");
            if (stats.coldResistance != 0) statNames.Add("Cold Resistance");
            if (stats.lightningResistance != 0) statNames.Add("Lightning Resistance");
            if (stats.chaosResistance != 0) statNames.Add("Chaos Resistance");
            if (stats.elementalResistance != 0) statNames.Add("Elemental Resistance");
            if (stats.allResistance != 0) statNames.Add("All Resistance");
            
            // Defense Stats
            if (stats.armour != 0) statNames.Add("Armour");
            if (stats.evasion != 0) statNames.Add("Evasion");
            if (stats.energyShield != 0) statNames.Add("Energy Shield");
            if (stats.blockChance != 0) statNames.Add("Block Chance");
            if (stats.dodgeChance != 0) statNames.Add("Dodge Chance");
            if (stats.spellDodgeChance != 0) statNames.Add("Spell Dodge");
            if (stats.spellBlockChance != 0) statNames.Add("Spell Block");
            
            // Recovery Stats
            if (stats.lifeRegeneration != 0) statNames.Add("Life Regen");
            if (stats.energyShieldRegeneration != 0) statNames.Add("Energy Shield Regen");
            if (stats.manaRegeneration != 0) statNames.Add("Mana Regen");
            if (stats.relianceRegeneration != 0) statNames.Add("Reliance Regen");
            if (stats.lifeLeech != 0) statNames.Add("Life Leech");
            if (stats.manaLeech != 0) statNames.Add("Mana Leech");
            if (stats.energyShieldLeech != 0) statNames.Add("Energy Shield Leech");
            
            // Combat Mechanics
            if (stats.attackSpeed != 0) statNames.Add("Attack Speed");
            if (stats.castSpeed != 0) statNames.Add("Cast Speed");
            if (stats.movementSpeed != 0) statNames.Add("Movement Speed");
            if (stats.attackRange != 0) statNames.Add("Attack Range");
            if (stats.projectileSpeed != 0) statNames.Add("Projectile Speed");
            if (stats.areaOfEffect != 0) statNames.Add("Area of Effect");
            if (stats.skillEffectDuration != 0) statNames.Add("Skill Duration");
            if (stats.statusEffectDuration != 0) statNames.Add("Status Duration");
            
            // Card System Stats
            if (stats.cardsDrawnPerTurn != 0) statNames.Add("Cards Drawn");
            if (stats.maxHandSize != 0) statNames.Add("Max Hand Size");
            if (stats.cardDrawChance != 0) statNames.Add("Card Draw Chance");
            if (stats.cardRetentionChance != 0) statNames.Add("Card Retention");
            if (stats.cardUpgradeChance != 0) statNames.Add("Card Upgrade");
            if (stats.discardPower != 0) statNames.Add("Discard Power");
            if (stats.manaPerTurn != 0) statNames.Add("Mana Per Turn");
            
            // Legacy Stats
            if (stats.armorIncrease != 0) statNames.Add("Armor Increase");
            if (stats.increasedEvasion != 0) statNames.Add("Evasion Increase");
            if (stats.elementalResist != 0) statNames.Add("Elemental Resist");
            if (stats.spellPowerIncrease != 0) statNames.Add("Spell Power");
            if (stats.critChanceIncrease != 0) statNames.Add("Crit Chance");
            if (stats.critMultiplierIncrease != 0) statNames.Add("Crit Multiplier");
            
            // Limit to first 3 stats to keep names manageable
            if (statNames.Count > 3)
            {
                statNames = statNames.Take(3).ToList();
            }
            
            return statNames;
        }
        
        void ClearAllStats()
        {
            if (selectedCells.Count == 0) return;
            
            foreach (var cell in selectedCells)
            {
                if (cell != null)
                {
                    cell.UpdateNodeStats(new JsonStats());
                    EditorUtility.SetDirty(cell);
                    SaveChangesToPrefab(cell);
                }
            }
            
            Debug.Log($"Cleared stats from {selectedCells.Count} cells");
        }
        
        [ContextMenu("Save All Changes to Prefabs")]
        public void SaveAllChangesToPrefabs()
        {
            if (allCellJsonData.Count == 0)
            {
                Debug.LogWarning("No cells found. Select a board first.");
                return;
            }
            
            int savedCount = 0;
            foreach (var cell in allCellJsonData)
            {
                if (cell != null)
                {
                    SaveChangesToPrefab(cell);
                    savedCount++;
                }
            }
            
            Debug.Log($"Saved changes to {savedCount} prefab assets");
        }
        
        [ContextMenu("Save Node Names to Prefabs")]
        public void SaveNodeNamesToPrefabs()
        {
            if (allCellJsonData.Count == 0)
            {
                Debug.LogWarning("No cells found. Select a board first.");
                return;
            }
            
            int savedCount = 0;
            foreach (var cell in allCellJsonData)
            {
                if (cell != null && !string.IsNullOrEmpty(cell.NodeName))
                {
                    var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(cell.gameObject);
                    if (prefabAsset != null)
                    {
                        var prefabCellData = prefabAsset.GetComponent<CellJsonData>();
                        if (prefabCellData != null)
                        {
                            prefabCellData.SetNodeName(cell.NodeName);
                            prefabAsset.name = cell.gameObject.name;
                            EditorUtility.SetDirty(prefabAsset);
                            savedCount++;
                        }
                    }
                }
            }
            
            Debug.Log($"Saved node names to {savedCount} prefab assets");
        }
        
        [ContextMenu("Check Prefab Status")]
        public void CheckPrefabStatus()
        {
            if (allCellJsonData.Count == 0)
            {
                Debug.LogWarning("No cells found. Select a board first.");
                return;
            }
            
            int prefabInstances = 0;
            int regularObjects = 0;
            
            foreach (var cell in allCellJsonData)
            {
                if (cell != null)
                {
                    var prefabInstance = PrefabUtility.GetCorrespondingObjectFromSource(cell.gameObject);
                    if (prefabInstance != null)
                    {
                        prefabInstances++;
                    }
                    else
                    {
                        regularObjects++;
                    }
                }
            }
            
            Debug.Log($"Prefab Status: {prefabInstances} prefab instances, {regularObjects} regular objects");
        }
        
        void ResetTemplatePresets()
        {
            templatePresets.Clear();
            InitializeDefaultPresets();
            Debug.Log("[PassiveTreeJsonDataEditor] Template presets reset to defaults");
        }
        
        void ExportTemplatePresets()
        {
            string presetInfo = "Template Presets:\n";
            for (int i = 0; i < templatePresets.Count; i++)
            {
                var preset = templatePresets[i];
                presetInfo += $"{i + 1}. {preset.name}\n";
                presetInfo += $"   - Stats: {GetStatSummary(preset.stats)}\n";
            }
            Debug.Log(presetInfo);
        }
        
        string GetStatSummary(JsonStats stats)
        {
            var statNames = new List<string>();
            if (stats.strength != 0) statNames.Add($"Str:{stats.strength}");
            if (stats.dexterity != 0) statNames.Add($"Dex:{stats.dexterity}");
            if (stats.intelligence != 0) statNames.Add($"Int:{stats.intelligence}");
            if (stats.maxHealthIncrease != 0) statNames.Add($"HP:{stats.maxHealthIncrease}%");
            if (stats.increasedFireDamage != 0) statNames.Add($"Fire:{stats.increasedFireDamage}%");
            if (stats.increasedColdDamage != 0) statNames.Add($"Cold:{stats.increasedColdDamage}%");
            if (stats.increasedLightningDamage != 0) statNames.Add($"Lightning:{stats.increasedLightningDamage}%");
            if (stats.criticalChance != 0) statNames.Add($"Crit:{stats.criticalChance}%");
            if (stats.armour != 0) statNames.Add($"Armour:{stats.armour}");
            if (stats.evasion != 0) statNames.Add($"Evasion:{stats.evasion}");
            
            return statNames.Count > 0 ? string.Join(", ", statNames) : "No stats";
        }
    }
}

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class StatsPanel : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    // UI Elements
    private Label characterNameLabel;
    private Label characterClassLabel;
    private Label characterLevelLabel;
    private ProgressBar experienceBar;
    private MultiColumnListView attributeListView;
    private MultiColumnListView resourceListView;
    private MultiColumnListView damageListView;
    private MultiColumnListView resistanceListView;
    
    // Data sources
    private List<AttributeData> attributeDataList = new List<AttributeData>();
    private List<ResourceData> resourceDataList = new List<ResourceData>();
    private List<DamageData> damageDataList = new List<DamageData>();
    private List<ResistanceData> resistanceDataList = new List<ResistanceData>();

    [MenuItem("Window/UI Toolkit/StatsPanel")]
    public static void ShowExample()
    {
        StatsPanel wnd = GetWindow<StatsPanel>();
        wnd.titleContent = new GUIContent("StatsPanel");
        
        // Try to auto-assign the UXML if it's not set
        if (wnd.m_VisualTreeAsset == null)
        {
            wnd.m_VisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/CharacterStats/StatsPanel.uxml");
            if (wnd.m_VisualTreeAsset != null)
            {
                Debug.Log("[StatsPanel] Auto-assigned StatsPanel.uxml");
                
                // Also try to auto-assign the USS file
                var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/CharacterStats/StatsPanel.uss");
                if (styleSheet != null)
                {
                    Debug.Log("[StatsPanel] Auto-assigned StatsPanel.uss");
                }
                
                // Force the window to recreate its GUI since we just assigned the asset
                wnd.rootVisualElement.Clear();
                wnd.CreateGUI();
            }
            else
            {
                Debug.LogError("[StatsPanel] Could not find StatsPanel.uxml at Assets/UI/CharacterStats/StatsPanel.uxml");
            }
        }
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Check if VisualTreeAsset is assigned
        if (m_VisualTreeAsset == null)
        {
            Debug.LogError("[StatsPanel] VisualTreeAsset is null! Please assign the StatsPanel.uxml file in the inspector.");
            
            // Create a simple fallback UI
            CreateFallbackUI(root);
            return;
        }

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);
        
        // Get references to UI elements
        SetupUIReferences();
        
        // Debug: Check if CharacterManager exists
        Debug.Log($"[StatsPanel] CharacterManager.Instance: {CharacterManager.Instance}");
        
        // Try to get real character data, fall back to test data
        if (CharacterManager.Instance != null)
        {
            Debug.Log($"[StatsPanel] CharacterManager found, HasCharacter: {CharacterManager.Instance.HasCharacter()}");
            
            if (CharacterManager.Instance.HasCharacter())
            {
                Character character = CharacterManager.Instance.GetCurrentCharacter();
                Debug.Log($"[StatsPanel] Character loaded: {character?.characterName} (Level {character?.level})");
                UpdateWithCharacterData(character);
            }
            else
            {
                Debug.Log("[StatsPanel] No character found, using test data");
                PopulateWithTestData();
            }
        }
        else
        {
            Debug.Log("[StatsPanel] CharacterManager not found, using test data");
            PopulateWithTestData();
        }
        
        // Setup the MultiColumnListView
        SetupAttributeListView();
        SetupResourceListView();
        SetupDamageListView();
        SetupResistanceListView();
    }
    
    private void CreateFallbackUI(VisualElement root)
    {
        Debug.Log("[StatsPanel] Creating fallback UI");
        
        // Create a simple fallback UI
        var container = new VisualElement();
        container.style.paddingLeft = 20;
        container.style.paddingRight = 20;
        container.style.paddingTop = 20;
        container.style.paddingBottom = 20;
        container.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        
        var header = new Label("StatsPanel - Configuration Error");
        header.style.fontSize = 18;
        header.style.color = Color.white;
        header.style.marginBottom = 20;
        container.Add(header);
        
        var errorLabel = new Label("VisualTreeAsset is not assigned!\n\nTo fix this:\n1. Select this StatsPanel window\n2. In the Inspector, assign the 'StatsPanel.uxml' file to the 'M Visual Tree Asset' field\n3. Close and reopen this window");
        errorLabel.style.color = Color.yellow;
        errorLabel.style.whiteSpace = WhiteSpace.Normal;
        container.Add(errorLabel);
        
        root.Add(container);
    }
    
    private void SetupUIReferences()
    {
        var root = rootVisualElement;
        
        // Get UI element references
        characterNameLabel = root.Q<Label>("CharacterName");
        characterClassLabel = root.Q<Label>("CharacterClass");
        characterLevelLabel = root.Q<Label>("CharacterLevel");
        experienceBar = root.Q<ProgressBar>("ExperienceBar");
        attributeListView = root.Q<MultiColumnListView>("AttributeColumns");
        resourceListView = root.Q<MultiColumnListView>("ResourceColumns");
        damageListView = root.Q<MultiColumnListView>("DamageColumns");
        resistanceListView = root.Q<MultiColumnListView>("ResistanceColumns");
        
        // Debug: Check if UI elements were found
        Debug.Log($"[StatsPanel] UI References - Name: {characterNameLabel != null}, Class: {characterClassLabel != null}, Level: {characterLevelLabel != null}, Exp: {experienceBar != null}");
        Debug.Log($"[StatsPanel] ListView References - Attributes: {attributeListView != null}, Resources: {resourceListView != null}, Damage: {damageListView != null}, Resistances: {resistanceListView != null}");
        
        // If any critical UI elements are missing, log warnings
        if (characterNameLabel == null) Debug.LogWarning("[StatsPanel] CharacterName label not found in UXML!");
        if (characterClassLabel == null) Debug.LogWarning("[StatsPanel] CharacterClass label not found in UXML!");
        if (characterLevelLabel == null) Debug.LogWarning("[StatsPanel] CharacterLevel label not found in UXML!");
        if (experienceBar == null) Debug.LogWarning("[StatsPanel] ExperienceBar not found in UXML!");
        if (attributeListView == null) Debug.LogError("[StatsPanel] AttributeColumns MultiColumnListView not found in UXML!");
        if (resourceListView == null) Debug.LogError("[StatsPanel] ResourceColumns MultiColumnListView not found in UXML!");
        if (damageListView == null) Debug.LogError("[StatsPanel] DamageColumns MultiColumnListView not found in UXML!");
        if (resistanceListView == null) Debug.LogError("[StatsPanel] ResistanceColumns MultiColumnListView not found in UXML!");
    }
    
    private void PopulateWithTestData()
    {
        Debug.Log("[StatsPanel] Populating with test data");
        
        // Set character info
        if (characterNameLabel != null)
        {
            characterNameLabel.text = "Test Character";
            Debug.Log("[StatsPanel] Set test character name");
        }
        else
        {
            Debug.LogWarning("[StatsPanel] CharacterName label not found!");
        }
            
        if (characterClassLabel != null)
        {
            characterClassLabel.text = "Marauder";
            Debug.Log("[StatsPanel] Set test character class");
        }
        else
        {
            Debug.LogWarning("[StatsPanel] CharacterClass label not found!");
        }
            
        if (characterLevelLabel != null)
        {
            characterLevelLabel.text = "Level 5";
            Debug.Log("[StatsPanel] Set test character level");
        }
        else
        {
            Debug.LogWarning("[StatsPanel] CharacterLevel label not found!");
        }
            
        if (experienceBar != null)
        {
            experienceBar.value = 65f; // 65% experience
            experienceBar.title = "65/100";
            Debug.Log("[StatsPanel] Set test experience bar");
        }
        else
        {
            Debug.LogWarning("[StatsPanel] ExperienceBar not found!");
        }
        
        // Create attribute data
        attributeDataList.Clear();
        attributeDataList.Add(new AttributeData("32", "14", "14"));
        attributeDataList.Add(new AttributeData("+0", "+0", "+0"));
        
        // Create resource data
        resourceDataList.Clear();
        resourceDataList.Add(new ResourceData("420/420", "3/3", "28/28", "75%"));
        resourceDataList.Add(new ResourceData("+50", "+10", "+15", "+5%"));
        
        // Create damage data
        damageDataList.Clear();
        damageDataList.Add(new DamageData("Physical", "50", "110%", "30%"));
        damageDataList.Add(new DamageData("Fire", "50", "110%", "30%"));
        damageDataList.Add(new DamageData("Cold", "50", "110%", "30%"));
        damageDataList.Add(new DamageData("Lightning", "50", "110%", "30%"));
        damageDataList.Add(new DamageData("Chaos", "50", "110%", "30%"));
        
        // Create resistance data
        resistanceDataList.Clear();
        resistanceDataList.Add(new ResistanceData("75/75", "60/60", "45/45", "25/25"));
        resistanceDataList.Add(new ResistanceData("+10", "+15", "+20", "+5"));
        
        Debug.Log($"[StatsPanel] Created test data - Attributes: {attributeDataList.Count}, Resources: {resourceDataList.Count}, Damage: {damageDataList.Count}, Resistances: {resistanceDataList.Count}");
    }
    
    private void SetupAttributeListView()
    {
        if (attributeListView == null) 
        {
            Debug.LogError("[StatsPanel] AttributeListView is null!");
            return;
        }
        
        Debug.Log($"[StatsPanel] Setting up AttributeListView with {attributeDataList.Count} items");
        
        // Set the items source
        attributeListView.itemsSource = attributeDataList;
        
        // Set up column bindings
        SetupColumnBindings(attributeListView, "Attribute");
        
        // Refresh the list view
        attributeListView.Rebuild();
        
        Debug.Log("[StatsPanel] AttributeListView setup complete");
    }
    
    private void SetupResourceListView()
    {
        if (resourceListView == null) 
        {
            Debug.LogError("[StatsPanel] ResourceListView is null!");
            return;
        }
        
        Debug.Log($"[StatsPanel] Setting up ResourceListView with {resourceDataList.Count} items");
        
        // Set the items source
        resourceListView.itemsSource = resourceDataList;
        
        // Set up column bindings
        SetupColumnBindings(resourceListView, "Resource");
        
        // Refresh the list view
        resourceListView.Rebuild();
        
        Debug.Log("[StatsPanel] ResourceListView setup complete");
    }
    
    private void SetupDamageListView()
    {
        if (damageListView == null) 
        {
            Debug.LogError("[StatsPanel] DamageListView is null!");
            return;
        }
        
        Debug.Log($"[StatsPanel] Setting up DamageListView with {damageDataList.Count} items");
        
        // Set the items source
        damageListView.itemsSource = damageDataList;
        
        // Set up column bindings
        SetupColumnBindings(damageListView, "Damage");
        
        // Refresh the list view
        damageListView.Rebuild();
        
        Debug.Log("[StatsPanel] DamageListView setup complete");
    }
    
    private void SetupResistanceListView()
    {
        if (resistanceListView == null) 
        {
            Debug.LogError("[StatsPanel] ResistanceListView is null!");
            return;
        }
        
        Debug.Log($"[StatsPanel] Setting up ResistanceListView with {resistanceDataList.Count} items");
        
        // Set the items source
        resistanceListView.itemsSource = resistanceDataList;
        
        // Set up column bindings
        SetupColumnBindings(resistanceListView, "Resistance");
        
        // Refresh the list view
        resistanceListView.Rebuild();
        
        Debug.Log("[StatsPanel] ResistanceListView setup complete");
    }
    
    private void SetupColumnBindings(MultiColumnListView listView, string prefix)
    {
        if (listView == null) return;

        // Get the columns
        var columns = listView.columns;
        Debug.Log($"[StatsPanel] Found {columns.Count} columns for {prefix}");
        
        // Set up manual cell creation and binding for each column
        foreach (var column in columns)
        {
            Debug.Log($"[StatsPanel] Setting up column: {column.name} ({column.title})");
            
            // Create cells manually
            column.makeCell = () => new Label();
            
            // Bind cells manually
            column.bindCell = (element, index) =>
            {
                if (element is Label label)
                {
                    BindCellData(label, column.name, index, prefix);
                }
            };
        }
    }
    
    private void BindCellData(Label label, string columnName, int index, string prefix)
    {
        if (label == null) return;

        // Apply column-specific CSS classes first
        ApplyColumnStyling(label, columnName, prefix);

        switch (prefix)
        {
            case "Attribute":
                if (index < attributeDataList.Count)
                {
                    var data = attributeDataList[index];
                    switch (columnName)
                    {
                        case "AttributeStrength": label.text = data.strength; break;
                        case "AttributeDexterity": label.text = data.dexterity; break;
                        case "AttributeIntelligence": label.text = data.intelligence; break;
                    }
                }
                break;
                
            case "Resource":
                if (index < resourceDataList.Count)
                {
                    var data = resourceDataList[index];
                    switch (columnName)
                    {
                        case "ResourceHealth": label.text = data.health; break;
                        case "ResourceMana": label.text = data.mana; break;
                        case "ResourceEnergyShield": label.text = data.energyShield; break;
                        case "ResourceReliance": label.text = data.reliance; break;
                    }
                }
                break;
                
            case "Damage":
                if (index < damageDataList.Count)
                {
                    var data = damageDataList[index];
                    switch (columnName)
                    {
                        case "DamageType": label.text = data.type; break;
                        case "DamageFlat": label.text = data.flat; break;
                        case "DamageIncreased": label.text = data.increased; break;
                        case "DamageMore": label.text = data.more; break;
                    }
                }
                break;
                
            case "Resistance":
                if (index < resistanceDataList.Count)
                {
                    var data = resistanceDataList[index];
                    switch (columnName)
                    {
                        case "ResistanceFire": label.text = data.fire; break;
                        case "ResistanceCold": label.text = data.cold; break;
                        case "ResistanceLightning": label.text = data.lightning; break;
                        case "ResistanceChaos": label.text = data.chaos; break;
                    }
                }
                break;
        }
    }

    // Apply column-specific CSS classes
    private void ApplyColumnStyling(Label label, string columnName, string prefix)
    {
        // Remove any existing column-specific classes
        label.RemoveFromClassList("attribute-strength");
        label.RemoveFromClassList("attribute-dexterity");
        label.RemoveFromClassList("attribute-intelligence");
        label.RemoveFromClassList("resource-health");
        label.RemoveFromClassList("resource-mana");
        label.RemoveFromClassList("resource-energy-shield");
        label.RemoveFromClassList("resource-reliance");
        label.RemoveFromClassList("damage-type");
        label.RemoveFromClassList("damage-flat");
        label.RemoveFromClassList("damage-increased");
        label.RemoveFromClassList("damage-more");
        label.RemoveFromClassList("resistance-fire");
        label.RemoveFromClassList("resistance-cold");
        label.RemoveFromClassList("resistance-lightning");
        label.RemoveFromClassList("resistance-chaos");

        // Apply appropriate class based on column name
        switch (columnName)
        {
            // Attribute columns
            case "AttributeStrength":
                label.AddToClassList("attribute-strength");
                break;
            case "AttributeDexterity":
                label.AddToClassList("attribute-dexterity");
                break;
            case "AttributeIntelligence":
                label.AddToClassList("attribute-intelligence");
                break;

            // Resource columns
            case "ResourceHealth":
                label.AddToClassList("resource-health");
                break;
            case "ResourceMana":
                label.AddToClassList("resource-mana");
                break;
            case "ResourceEnergyShield":
                label.AddToClassList("resource-energy-shield");
                break;
            case "ResourceReliance":
                label.AddToClassList("resource-reliance");
                break;

            // Damage columns
            case "DamageType":
                label.AddToClassList("damage-type");
                break;
            case "DamageFlat":
                label.AddToClassList("damage-flat");
                break;
            case "DamageIncreased":
                label.AddToClassList("damage-increased");
                break;
            case "DamageMore":
                label.AddToClassList("damage-more");
                break;

            // Resistance columns
            case "ResistanceFire":
                label.AddToClassList("resistance-fire");
                break;
            case "ResistanceCold":
                label.AddToClassList("resistance-cold");
                break;
            case "ResistanceLightning":
                label.AddToClassList("resistance-lightning");
                break;
            case "ResistanceChaos":
                label.AddToClassList("resistance-chaos");
                break;
        }
    }
    
    // Method to update with real character data
    public void UpdateWithCharacterData(Character character)
    {
        if (character == null) 
        {
            Debug.LogWarning("[StatsPanel] Character is null in UpdateWithCharacterData");
            return;
        }
        
        Debug.Log($"[StatsPanel] Updating with character: {character.characterName} (Level {character.level})");
        
        // Debug character data to understand why it's empty
        Debug.Log($"[StatsPanel] Character Debug - Name: '{character.characterName}', Class: '{character.characterClass}', Level: {character.level}");
        Debug.Log($"[StatsPanel] Character Debug - STR: {character.strength}, DEX: {character.dexterity}, INT: {character.intelligence}");
        Debug.Log($"[StatsPanel] Character Debug - Health: {character.currentHealth}/{character.maxHealth}, Mana: {character.mana}/{character.maxMana}");
        
        // Check if character data is valid
        if (string.IsNullOrEmpty(character.characterName) || character.level <= 0)
        {
            Debug.LogWarning("[StatsPanel] Character data appears to be empty or invalid, trying to load from PlayerPrefs...");
            
            // Try to load character from PlayerPrefs
            TryLoadCharacterFromPlayerPrefs();
            
            // If still empty, use test data
            if (string.IsNullOrEmpty(character.characterName) || character.level <= 0)
            {
                Debug.LogWarning("[StatsPanel] Still no valid character data, using test data instead");
                PopulateWithTestData();
                return;
            }
        }
        
        // Update character info
        if (characterNameLabel != null)
        {
            characterNameLabel.text = character.characterName;
            Debug.Log($"[StatsPanel] Set character name: {character.characterName}");
        }
        else
        {
            Debug.LogWarning("[StatsPanel] CharacterName label is null!");
        }
            
        if (characterClassLabel != null)
        {
            characterClassLabel.text = character.characterClass;
            Debug.Log($"[StatsPanel] Set character class: {character.characterClass}");
        }
        else
        {
            Debug.LogWarning("[StatsPanel] CharacterClass label is null!");
        }
            
        if (characterLevelLabel != null)
        {
            characterLevelLabel.text = $"Level {character.level}";
            Debug.Log($"[StatsPanel] Set character level: {character.level}");
        }
        else
        {
            Debug.LogWarning("[StatsPanel] CharacterLevel label is null!");
        }
            
        if (experienceBar != null)
        {
            int requiredExp = character.GetRequiredExperience();
            float expPercentage = (float)character.experience / requiredExp * 100f;
            experienceBar.value = expPercentage;
            experienceBar.title = $"{character.experience}/{requiredExp}";
            Debug.Log($"[StatsPanel] Set experience: {character.experience}/{requiredExp} ({expPercentage:F1}%)");
        }
        else
        {
            Debug.LogWarning("[StatsPanel] ExperienceBar is null!");
        }
        
        // Update attribute data
        UpdateAttributeData(character);
        
        // Refresh the list view
        if (attributeListView != null)
        {
            attributeListView.itemsSource = attributeDataList;
            attributeListView.Rebuild();
            Debug.Log($"[StatsPanel] Refreshed list view with {attributeDataList.Count} items");
        }
        else
        {
            Debug.LogError("[StatsPanel] AttributeListView is null!");
        }
    }
    
    // Try to load character from PlayerPrefs if CharacterManager has empty data
    private void TryLoadCharacterFromPlayerPrefs()
    {
        string currentCharacterName = PlayerPrefs.GetString("CurrentCharacter", "");
        
        if (!string.IsNullOrEmpty(currentCharacterName))
        {
            Debug.Log($"[StatsPanel] Found character name in PlayerPrefs: {currentCharacterName}");
            
            // Try to load the character using CharacterManager
            CharacterManager.Instance.LoadCharacter(currentCharacterName);
            
            // Get the updated character
            Character updatedCharacter = CharacterManager.Instance.GetCurrentCharacter();
            
            if (updatedCharacter != null && !string.IsNullOrEmpty(updatedCharacter.characterName))
            {
                Debug.Log($"[StatsPanel] Successfully loaded character: {updatedCharacter.characterName} (Level {updatedCharacter.level})");
                
                // Update the UI with the loaded character
                UpdateWithCharacterData(updatedCharacter);
            }
            else
            {
                Debug.LogWarning($"[StatsPanel] Failed to load character: {currentCharacterName}");
            }
        }
        else
        {
            Debug.LogWarning("[StatsPanel] No character name found in PlayerPrefs");
        }
    }
    
    // Get base stats for a character class
    private (int str, int dex, int intel) GetBaseStatsForClass(string characterClass)
    {
        switch (characterClass.ToLower())
        {
            // Primary Classes (Single Attribute Focus)
            case "marauder":
                return (32, 14, 14);
            case "ranger":
                return (14, 32, 14);
            case "witch":
                return (14, 14, 32);
            
            // Hybrid Classes (Dual Attribute Focus)
            case "brawler":
                return (23, 23, 14);
            case "thief":
                return (14, 23, 23);
            case "apostle":
                return (23, 14, 23);
            
            default:
                return (14, 14, 14); // Default fallback
        }
    }
    
    private void UpdateAttributeData(Character character)
    {
        Debug.Log($"[StatsPanel] Updating attribute data for {character.characterName}");
        
        attributeDataList.Clear();
        
        // Get base stats for this character class
        var (baseStr, baseDex, baseInt) = GetBaseStatsForClass(character.characterClass);
        
        // Calculate level-up gains
        var levelGains = GetLevelUpGains(character.characterClass);
        int levelsGained = character.level - 1;
        
        // Calculate what the base stats should be at this level
        int expectedBaseStr = baseStr + (levelGains.str * levelsGained);
        int expectedBaseDex = baseDex + (levelGains.dex * levelsGained);
        int expectedBaseInt = baseInt + (levelGains.intel * levelsGained);
        
        // Calculate bonuses (current - expected base)
        int strBonus = character.strength - expectedBaseStr;
        int dexBonus = character.dexterity - expectedBaseDex;
        int intBonus = character.intelligence - expectedBaseInt;
        
        Debug.Log($"[StatsPanel] Stats - Current: STR={character.strength}, DEX={character.dexterity}, INT={character.intelligence}");
        Debug.Log($"[StatsPanel] Stats - Expected Base: STR={expectedBaseStr}, DEX={expectedBaseDex}, INT={expectedBaseInt}");
        Debug.Log($"[StatsPanel] Stats - Bonuses: STR={strBonus}, DEX={dexBonus}, INT={intBonus}");
        
        // Row 1: Current total values
        attributeDataList.Add(new AttributeData(
            character.strength.ToString(),
            character.dexterity.ToString(),
            character.intelligence.ToString()
        ));
        
        // Row 2: Expected base values at this level
        attributeDataList.Add(new AttributeData(
            expectedBaseStr.ToString(),
            expectedBaseDex.ToString(),
            expectedBaseInt.ToString()
        ));
        
        // Row 3: Bonuses from items/equipment
        attributeDataList.Add(new AttributeData(
            strBonus > 0 ? $"+{strBonus}" : strBonus < 0 ? $"{strBonus}" : "+0",
            dexBonus > 0 ? $"+{dexBonus}" : dexBonus < 0 ? $"{dexBonus}" : "+0",
            intBonus > 0 ? $"+{intBonus}" : intBonus < 0 ? $"{intBonus}" : "+0"
        ));
        
        // Row 4: Health/Mana/Energy Shield
        attributeDataList.Add(new AttributeData(
            $"Health: {character.currentHealth}/{character.maxHealth}",
            $"Mana: {character.mana}/{character.maxMana}",
            $"ES: {character.currentEnergyShield}/{character.maxEnergyShield}"
        ));
        
        // Row 5: Combat stats
        attributeDataList.Add(new AttributeData(
            $"Attack: {character.attackPower}",
            $"Defense: {character.defense}",
            $"Crit: {character.criticalChance:F1}%"
        ));
        
        Debug.Log($"[StatsPanel] Created {attributeDataList.Count} attribute data rows");
    }
    
    // Get level-up attribute gains based on class
    private (int str, int dex, int intel) GetLevelUpGains(string characterClass)
    {
        switch (characterClass.ToLower())
        {
            // Primary Classes
            case "marauder":
                return (3, 1, 1); // +3 STR, +1 DEX, +1 INT
            case "ranger":
                return (1, 3, 1); // +1 STR, +3 DEX, +1 INT
            case "witch":
                return (1, 1, 3); // +1 STR, +1 DEX, +3 INT
            
            // Hybrid Classes
            case "brawler":
                return (2, 2, 1); // +2 STR, +2 DEX, +1 INT
            case "thief":
                return (1, 2, 2); // +1 STR, +2 DEX, +2 INT
            case "apostle":
                return (2, 1, 2); // +2 STR, +1 DEX, +2 INT
            
            default:
                return (1, 1, 1); // Default fallback
        }
    }
}

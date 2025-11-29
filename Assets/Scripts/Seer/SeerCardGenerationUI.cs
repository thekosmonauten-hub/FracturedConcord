using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Dexiled.Data.Items;

/// <summary>
/// UI for the Seer's card generation system.
/// Allows players to select orbs and spirits to generate cards.
/// </summary>
public class SeerCardGenerationUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button scryButton;
    
    [Header("Orb Selection")]
    [SerializeField] private Transform orbContainer; // Parent for orb UI items (alternative to individual fields)
    [SerializeField] private GameObject orbItemPrefab; // Prefab for orb selection items
    
    // Legacy fields (kept for backward compatibility)
    [SerializeField] private TMP_Text infusionOrbCountText;
    [SerializeField] private TMP_Text infusionOrbChanceText;
    [SerializeField] private Button infusionOrbPlusButton;
    [SerializeField] private Button infusionOrbMinusButton;
    
    [SerializeField] private TMP_Text perfectionOrbCountText;
    [SerializeField] private TMP_Text perfectionOrbChanceText;
    [SerializeField] private Button perfectionOrbPlusButton;
    [SerializeField] private Button perfectionOrbMinusButton;
    
    [Header("Spirit Selection")]
    [SerializeField] private Transform spiritContainer; // Parent for spirit UI items
    [SerializeField] private GameObject spiritItemPrefab; // Prefab for individual spirit selection
    
    [Header("Player Currency Display")]
    [SerializeField] private Transform generationOrbContainer; // Optional: Container for Generation Orb display prefab
    [SerializeField] private GameObject generationOrbDisplayPrefab; // Optional: Prefab for Generation Orb display (no buttons)
    [SerializeField] private TMP_Text generationOrbCountText; // Legacy: Direct text field (used if prefab not provided)
    
    [Header("Output Display")]
    [SerializeField] private GameObject outputCardDisplay; // Where generated card is shown
    [SerializeField] private TMP_Text outputCardNameText;
    [SerializeField] private TMP_Text outputCardRarityText;
    [SerializeField] private TMP_Text outputCardElementText;
    [SerializeField] private Image outputCardImage;
    
    [Header("Summary")]
    [SerializeField] private TMP_Text summaryText;
    
    // Runtime data
    private int selectedInfusionOrbs = 0;
    private int selectedPerfectionOrbs = 0;
    private Dictionary<CurrencyType, int> selectedSpirits = new Dictionary<CurrencyType, int>();
    
    // Available spirits (mapped from CurrencyType)
    private static readonly CurrencyType[] AVAILABLE_SPIRITS = new CurrencyType[]
    {
        CurrencyType.FireSpirit,
        CurrencyType.ColdSpirit,
        CurrencyType.LightningSpirit,
        CurrencyType.PhysicalSpirit,
        CurrencyType.ChaosSpirit,
        CurrencyType.LifeSpirit,
        CurrencyType.DefenseSpirit,
        CurrencyType.CritSpirit,
        CurrencyType.DivineSpirit
    };
    
    private CurrencyDatabase currencyDatabase;
    private LootManager lootManager;
    private CharacterManager characterManager;
    
    private void Awake()
    {
        currencyDatabase = Resources.Load<CurrencyDatabase>("CurrencyDatabase");
        lootManager = LootManager.Instance;
        characterManager = CharacterManager.Instance;
        
        // Initialize spirit dictionary
        foreach (var spirit in AVAILABLE_SPIRITS)
        {
            selectedSpirits[spirit] = 0;
        }
    }
    
    private void OnEnable()
    {
        RefreshUI();
    }
    
    private void Start()
    {
        SetupButtons();
        RefreshUI();
    }
    
    private void SetupButtons()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);
        
        if (scryButton != null)
            scryButton.onClick.AddListener(OnScryClicked);
        
        // Setup orb UI (prefer prefab-based if available, otherwise use legacy buttons)
        if (orbContainer != null && orbItemPrefab != null)
        {
            SetupOrbUI();
        }
        else
        {
            // Legacy button setup
            if (infusionOrbPlusButton != null)
                infusionOrbPlusButton.onClick.AddListener(() => AdjustInfusionOrbs(1));
            
            if (infusionOrbMinusButton != null)
                infusionOrbMinusButton.onClick.AddListener(() => AdjustInfusionOrbs(-1));
            
            if (perfectionOrbPlusButton != null)
                perfectionOrbPlusButton.onClick.AddListener(() => AdjustPerfectionOrbs(1));
            
            if (perfectionOrbMinusButton != null)
                perfectionOrbMinusButton.onClick.AddListener(() => AdjustPerfectionOrbs(-1));
        }
        
        // Setup spirit UI if container exists
        if (spiritContainer != null && spiritItemPrefab != null)
        {
            SetupSpiritUI();
        }
        
        // Setup Generation Orb display if prefab provided
        if (generationOrbContainer != null && generationOrbDisplayPrefab != null)
        {
            SetupGenerationOrbDisplay();
        }
    }
    
    private void SetupGenerationOrbDisplay()
    {
        if (generationOrbContainer == null || generationOrbDisplayPrefab == null) return;
        
        // Clear existing display
        foreach (Transform child in generationOrbContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create Generation Orb display
        GameObject displayItem = Instantiate(generationOrbDisplayPrefab, generationOrbContainer);
        SetupGenerationOrbDisplayItem(displayItem);
    }
    
    private void SetupGenerationOrbDisplayItem(GameObject displayItem)
    {
        // Find components (trying multiple naming patterns)
        TMP_Text countText = FindChildComponent<TMP_Text>(displayItem, "CountText", "Count", "QuantityText");
        TMP_Text nameText = FindChildComponent<TMP_Text>(displayItem, "NameText", "Name", "LabelText");
        Image iconImage = FindChildComponent<Image>(displayItem, "IconImage", "Icon", "SpriteImage");
        
        // Get currency data from CurrencyDatabase
        if (currencyDatabase != null)
        {
            CurrencyData currencyData = currencyDatabase.GetCurrency(CurrencyType.OrbOfGeneration);
            if (currencyData != null)
            {
                if (nameText != null)
                    nameText.text = currencyData.currencyName;
                if (iconImage != null && currencyData.currencySprite != null)
                    iconImage.sprite = currencyData.currencySprite;
            }
            else
            {
                Debug.LogWarning($"[SeerCardGenerationUI] CurrencyData not found for OrbOfGeneration in CurrencyDatabase!");
            }
        }
        else
        {
            Debug.LogWarning("[SeerCardGenerationUI] CurrencyDatabase is null! Cannot populate Generation Orb display.");
        }
        
        // Store reference for updates
        GenerationOrbDisplayData displayData = displayItem.GetComponent<GenerationOrbDisplayData>();
        if (displayData == null)
            displayData = displayItem.AddComponent<GenerationOrbDisplayData>();
        
        displayData.countText = countText;
    }
    
    private void SetupOrbUI()
    {
        if (orbContainer == null || orbItemPrefab == null) return;
        
        // Clear existing orb items
        foreach (Transform child in orbContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create UI for Infusion Orb
        GameObject infusionItem = Instantiate(orbItemPrefab, orbContainer);
        SetupOrbItem(infusionItem, CurrencyType.OrbOfInfusion, () => AdjustInfusionOrbs(1), () => AdjustInfusionOrbs(-1));
        
        // Create UI for Perfection Orb
        GameObject perfectionItem = Instantiate(orbItemPrefab, orbContainer);
        SetupOrbItem(perfectionItem, CurrencyType.OrbOfPerfection, () => AdjustPerfectionOrbs(1), () => AdjustPerfectionOrbs(-1));
    }
    
    private void SetupOrbItem(GameObject orbItem, CurrencyType orbType, System.Action onPlus, System.Action onMinus)
    {
        // Find components (trying multiple naming patterns)
        Button plusButton = FindChildComponent<Button>(orbItem, "PlusButton", "+Button", "AddButton");
        Button minusButton = FindChildComponent<Button>(orbItem, "MinusButton", "-Button", "RemoveButton");
        TMP_Text countText = FindChildComponent<TMP_Text>(orbItem, "CountText", "Count", "QuantityText");
        TMP_Text chanceText = FindChildComponent<TMP_Text>(orbItem, "ChanceText", "Chance", "PercentText");
        TMP_Text nameText = FindChildComponent<TMP_Text>(orbItem, "NameText", "Name", "LabelText");
        Image iconImage = FindChildComponent<Image>(orbItem, "IconImage", "Icon", "SpriteImage");
        
        // Get currency data from CurrencyDatabase
        if (currencyDatabase != null)
        {
            CurrencyData currencyData = currencyDatabase.GetCurrency(orbType);
            if (currencyData != null)
            {
                if (nameText != null)
                    nameText.text = currencyData.currencyName;
                if (iconImage != null && currencyData.currencySprite != null)
                    iconImage.sprite = currencyData.currencySprite;
            }
            else
            {
                Debug.LogWarning($"[SeerCardGenerationUI] CurrencyData not found for {orbType} in CurrencyDatabase!");
            }
        }
        else
        {
            Debug.LogWarning("[SeerCardGenerationUI] CurrencyDatabase is null! Cannot populate orb item data.");
        }
        
        // Setup buttons
        if (plusButton != null)
            plusButton.onClick.AddListener(() => onPlus());
        if (minusButton != null)
            minusButton.onClick.AddListener(() => onMinus());
        
        // Store references for updates
        OrbItemData itemData = orbItem.GetComponent<OrbItemData>();
        if (itemData == null)
            itemData = orbItem.AddComponent<OrbItemData>();
        
        itemData.countText = countText;
        itemData.chanceText = chanceText;
        itemData.orbType = orbType;
    }
    
    private void SetupSpiritUI()
    {
        // Clear existing spirit items
        foreach (Transform child in spiritContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create UI for each available spirit
        foreach (var spiritType in AVAILABLE_SPIRITS)
        {
            GameObject spiritItem = Instantiate(spiritItemPrefab, spiritContainer);
            SetupSpiritItem(spiritItem, spiritType);
        }
    }
    
    private void SetupSpiritItem(GameObject spiritItem, CurrencyType spiritType)
    {
        // Find components (assuming prefab has standard naming)
        // Try multiple common naming patterns
        Button plusButton = FindChildComponent<Button>(spiritItem, "PlusButton", "+Button", "AddButton");
        Button minusButton = FindChildComponent<Button>(spiritItem, "MinusButton", "-Button", "RemoveButton");
        TMP_Text countText = FindChildComponent<TMP_Text>(spiritItem, "CountText", "Count", "QuantityText");
        TMP_Text chanceText = FindChildComponent<TMP_Text>(spiritItem, "ChanceText", "Chance", "PercentText");
        TMP_Text nameText = FindChildComponent<TMP_Text>(spiritItem, "NameText", "Name", "LabelText");
        Image iconImage = FindChildComponent<Image>(spiritItem, "IconImage", "Icon", "SpriteImage");
        
        // Get currency data from CurrencyDatabase
        if (currencyDatabase != null)
        {
            CurrencyData currencyData = currencyDatabase.GetCurrency(spiritType);
            if (currencyData != null)
            {
                if (nameText != null)
                    nameText.text = currencyData.currencyName;
                if (iconImage != null && currencyData.currencySprite != null)
                    iconImage.sprite = currencyData.currencySprite;
            }
            else
            {
                Debug.LogWarning($"[SeerCardGenerationUI] CurrencyData not found for {spiritType} in CurrencyDatabase!");
            }
        }
        else
        {
            Debug.LogWarning("[SeerCardGenerationUI] CurrencyDatabase is null! Cannot populate spirit item data.");
        }
        
        // Setup buttons
        if (plusButton != null)
            plusButton.onClick.AddListener(() => AdjustSpirit(spiritType, 1));
        if (minusButton != null)
            minusButton.onClick.AddListener(() => AdjustSpirit(spiritType, -1));
        
        // Store references for updates
        SpiritItemData itemData = spiritItem.GetComponent<SpiritItemData>();
        if (itemData == null)
            itemData = spiritItem.AddComponent<SpiritItemData>();
        
        itemData.countText = countText;
        itemData.chanceText = chanceText;
        itemData.spiritType = spiritType;
    }
    
    /// <summary>
    /// Helper method to find a component in child objects, trying multiple name patterns
    /// First checks for a "content" child, then searches within that or the parent
    /// </summary>
    private T FindChildComponent<T>(GameObject parent, params string[] names) where T : Component
    {
        // First, check if there's a "content" child - if so, search within that
        Transform contentChild = parent.transform.Find("content");
        if (contentChild == null)
        {
            // Try capitalized version
            contentChild = parent.transform.Find("Content");
        }
        
        // Determine the root to search from
        Transform searchRoot = contentChild != null ? contentChild : parent.transform;
        
        foreach (string name in names)
        {
            Transform child = searchRoot.Find(name);
            if (child != null)
            {
                T component = child.GetComponent<T>();
                if (component != null)
                    return component;
            }
        }
        return null;
    }
    
    private void AdjustInfusionOrbs(int delta)
    {
        int current = selectedInfusionOrbs;
        int available = lootManager != null ? lootManager.GetCurrencyAmount(CurrencyType.OrbOfInfusion) : 0;
        int newValue = Mathf.Clamp(current + delta, 0, Mathf.Min(20, available));
        
        if (newValue != current)
        {
            selectedInfusionOrbs = newValue;
            RefreshUI();
        }
    }
    
    private void AdjustPerfectionOrbs(int delta)
    {
        // Validate: Perfection requires 100% Magic chance (20 Infusion orbs) - check BEFORE allowing increase
        if (delta > 0 && selectedInfusionOrbs < 20)
        {
            Debug.LogWarning("[SeerCardGenerationUI] Cannot use Perfection orbs without 100% Magic chance (need 20 Infusion orbs).");
            return;
        }
        
        int current = selectedPerfectionOrbs;
        int available = lootManager != null ? lootManager.GetCurrencyAmount(CurrencyType.OrbOfPerfection) : 0;
        int newValue = Mathf.Clamp(current + delta, 0, Mathf.Min(20, available));
        
        if (newValue != current)
        {
            selectedPerfectionOrbs = newValue;
            RefreshUI();
        }
    }
    
    private void AdjustSpirit(CurrencyType spiritType, int delta)
    {
        if (!selectedSpirits.ContainsKey(spiritType))
            selectedSpirits[spiritType] = 0;
        
        int current = selectedSpirits[spiritType];
        int available = lootManager != null ? lootManager.GetCurrencyAmount(spiritType) : 0;
        int newValue = Mathf.Clamp(current + delta, 0, Mathf.Min(20, available));
        
        if (newValue != current)
        {
            selectedSpirits[spiritType] = newValue;
            RefreshUI();
        }
    }
    
    private void RefreshUI()
    {
        // Update orb UI (prefer prefab-based if available)
        if (orbContainer != null)
        {
            UpdateOrbUI();
        }
        else
        {
            // Legacy text updates
            if (infusionOrbCountText != null)
                infusionOrbCountText.text = selectedInfusionOrbs.ToString();
            
            if (infusionOrbChanceText != null)
            {
                float chance = SeerCardGenerator.GetChancePercentage(selectedInfusionOrbs);
                infusionOrbChanceText.text = $"{chance:F0}% Magic";
            }
            
            if (perfectionOrbCountText != null)
                perfectionOrbCountText.text = selectedPerfectionOrbs.ToString();
            
            if (perfectionOrbChanceText != null)
            {
                float chance = SeerCardGenerator.GetChancePercentage(selectedPerfectionOrbs);
                perfectionOrbChanceText.text = $"{chance:F0}% Rare";
            }
        }
        
        // Update generation orb count (prefer prefab-based if available)
        if (generationOrbContainer != null)
        {
            UpdateGenerationOrbDisplay();
        }
        else if (generationOrbCountText != null && lootManager != null)
        {
            // Legacy text update
            int count = lootManager.GetCurrencyAmount(CurrencyType.OrbOfGeneration);
            generationOrbCountText.text = $"Orbs of Generation: {count}";
        }
        
        // Update spirit UI
        UpdateSpiritUI();
        
        // Update summary
        UpdateSummary();
        
        // Update button states
        UpdateButtonStates();
    }
    
    private void UpdateOrbUI()
    {
        if (orbContainer == null) return;
        
        foreach (Transform child in orbContainer)
        {
            OrbItemData itemData = child.GetComponent<OrbItemData>();
            if (itemData == null) continue;
            
            int count = 0;
            string chanceLabel = "";
            
            if (itemData.orbType == CurrencyType.OrbOfInfusion)
            {
                count = selectedInfusionOrbs;
                chanceLabel = "Magic";
            }
            else if (itemData.orbType == CurrencyType.OrbOfPerfection)
            {
                count = selectedPerfectionOrbs;
                chanceLabel = "Rare";
            }
            
            if (itemData.countText != null)
                itemData.countText.text = count.ToString();
            
            if (itemData.chanceText != null)
            {
                float chance = SeerCardGenerator.GetChancePercentage(count);
                itemData.chanceText.text = $"{chance:F0}% {chanceLabel}";
            }
        }
    }
    
    private void UpdateSpiritUI()
    {
        if (spiritContainer == null) return;
        
        foreach (Transform child in spiritContainer)
        {
            SpiritItemData itemData = child.GetComponent<SpiritItemData>();
            if (itemData == null) continue;
            
            int count = selectedSpirits.ContainsKey(itemData.spiritType) ? selectedSpirits[itemData.spiritType] : 0;
            
            if (itemData.countText != null)
                itemData.countText.text = count.ToString();
            
            if (itemData.chanceText != null)
            {
                float chance = SeerCardGenerator.GetChancePercentage(count, isSpirit: true);
                itemData.chanceText.text = $"{chance:F0}%";
            }
        }
    }
    
    private void UpdateSummary()
    {
        if (summaryText == null) return;
        
        // Build summary text - only show items that have been added (non-zero)
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        bool hasAnyContent = false;
        
        // Only show Infusion Orbs if > 0
        if (selectedInfusionOrbs > 0)
        {
            if (!hasAnyContent)
            {
                sb.AppendLine("Generation Summary:");
                hasAnyContent = true;
            }
            sb.AppendLine($"Infusion Orbs: {selectedInfusionOrbs} ({SeerCardGenerator.GetChancePercentage(selectedInfusionOrbs):F0}% Magic)");
        }
        
        // Only show Perfection Orbs if > 0
        if (selectedPerfectionOrbs > 0)
        {
            if (!hasAnyContent)
            {
                sb.AppendLine("Generation Summary:");
                hasAnyContent = true;
            }
            sb.AppendLine($"Perfection Orbs: {selectedPerfectionOrbs} ({SeerCardGenerator.GetChancePercentage(selectedPerfectionOrbs):F0}% Rare)");
        }
        
        // Only show Spirits if any are selected
        var activeSpirits = selectedSpirits.Where(kvp => kvp.Value > 0).ToList();
        if (activeSpirits.Count > 0)
        {
            if (!hasAnyContent)
            {
                sb.AppendLine("Generation Summary:");
                hasAnyContent = true;
            }
            sb.AppendLine("Spirits:");
            foreach (var kvp in activeSpirits)
            {
                CurrencyData currencyData = currencyDatabase?.GetCurrency(kvp.Key);
                string name = currencyData != null ? currencyData.currencyName : kvp.Key.ToString();
                float chance = SeerCardGenerator.GetChancePercentage(kvp.Value, isSpirit: true);
                sb.AppendLine($"  {name}: {kvp.Value} ({chance:F0}%)");
            }
        }
        
        // If nothing is selected, show empty or a message
        if (!hasAnyContent)
        {
            sb.AppendLine("Generation Summary:");
            sb.AppendLine("No ingredients selected. Random card will be generated.");
        }
        
        summaryText.text = sb.ToString();
    }
    
    private void UpdateButtonStates()
    {
        // Update orb button states (prefer prefab-based if available)
        if (orbContainer != null)
        {
            UpdateOrbButtonStates();
        }
        else
        {
            // Legacy button state updates
            if (infusionOrbPlusButton != null)
            {
                int available = lootManager != null ? lootManager.GetCurrencyAmount(CurrencyType.OrbOfInfusion) : 0;
                infusionOrbPlusButton.interactable = selectedInfusionOrbs < Mathf.Min(20, available);
            }
            
            if (infusionOrbMinusButton != null)
                infusionOrbMinusButton.interactable = selectedInfusionOrbs > 0;
            
            if (perfectionOrbPlusButton != null)
            {
                int available = lootManager != null ? lootManager.GetCurrencyAmount(CurrencyType.OrbOfPerfection) : 0;
                bool canUse = selectedInfusionOrbs >= 20; // Require 100% Magic
                perfectionOrbPlusButton.interactable = canUse && selectedPerfectionOrbs < Mathf.Min(20, available);
            }
            
            if (perfectionOrbMinusButton != null)
                perfectionOrbMinusButton.interactable = selectedPerfectionOrbs > 0;
        }
        
        // Update spirit button states
        UpdateSpiritButtonStates();
        
        // Scry button: requires at least 1 Orb of Generation
        if (scryButton != null)
        {
            int generationOrbs = lootManager != null ? lootManager.GetCurrencyAmount(CurrencyType.OrbOfGeneration) : 0;
            scryButton.interactable = generationOrbs >= 1;
        }
    }
    
    private void UpdateOrbButtonStates()
    {
        if (orbContainer == null) return;
        
        foreach (Transform child in orbContainer)
        {
            OrbItemData itemData = child.GetComponent<OrbItemData>();
            if (itemData == null) continue;
            
            // Check for "content" child first
            Transform contentChild = child.Find("content");
            if (contentChild == null)
                contentChild = child.Find("Content");
            Transform searchRoot = contentChild != null ? contentChild : child;
            
            Button plusButton = searchRoot.Find("PlusButton")?.GetComponent<Button>();
            Button minusButton = searchRoot.Find("MinusButton")?.GetComponent<Button>();
            
            if (itemData.orbType == CurrencyType.OrbOfInfusion)
            {
                if (plusButton != null)
                {
                    int available = lootManager != null ? lootManager.GetCurrencyAmount(CurrencyType.OrbOfInfusion) : 0;
                    plusButton.interactable = selectedInfusionOrbs < Mathf.Min(20, available);
                }
                if (minusButton != null)
                    minusButton.interactable = selectedInfusionOrbs > 0;
            }
            else if (itemData.orbType == CurrencyType.OrbOfPerfection)
            {
                if (plusButton != null)
                {
                    int available = lootManager != null ? lootManager.GetCurrencyAmount(CurrencyType.OrbOfPerfection) : 0;
                    bool canUse = selectedInfusionOrbs >= 20; // Require 100% Magic (20 Infusion orbs)
                    plusButton.interactable = canUse && selectedPerfectionOrbs < Mathf.Min(20, available);
                    
                    // Visual feedback: disable button if requirement not met
                    if (!canUse && selectedInfusionOrbs < 20)
                    {
                        // Button will be disabled, but we could add a tooltip or visual indicator here
                    }
                }
                if (minusButton != null)
                    minusButton.interactable = selectedPerfectionOrbs > 0;
            }
        }
    }
    
    private void UpdateSpiritButtonStates()
    {
        if (spiritContainer == null) return;
        
        foreach (Transform child in spiritContainer)
        {
            SpiritItemData itemData = child.GetComponent<SpiritItemData>();
            if (itemData == null) continue;
            
            // Check for "content" child first
            Transform contentChild = child.Find("content");
            if (contentChild == null)
                contentChild = child.Find("Content");
            Transform searchRoot = contentChild != null ? contentChild : child;
            
            Button plusButton = searchRoot.Find("PlusButton")?.GetComponent<Button>();
            Button minusButton = searchRoot.Find("MinusButton")?.GetComponent<Button>();
            
            if (plusButton != null)
            {
                int available = lootManager != null ? lootManager.GetCurrencyAmount(itemData.spiritType) : 0;
                int current = selectedSpirits.ContainsKey(itemData.spiritType) ? selectedSpirits[itemData.spiritType] : 0;
                plusButton.interactable = current < Mathf.Min(20, available);
            }
            
            if (minusButton != null)
            {
                int current = selectedSpirits.ContainsKey(itemData.spiritType) ? selectedSpirits[itemData.spiritType] : 0;
                minusButton.interactable = current > 0;
            }
        }
    }
    
    private void UpdateGenerationOrbDisplay()
    {
        if (generationOrbContainer == null || lootManager == null) return;
        
        int count = lootManager.GetCurrencyAmount(CurrencyType.OrbOfGeneration);
        
        foreach (Transform child in generationOrbContainer)
        {
            GenerationOrbDisplayData displayData = child.GetComponent<GenerationOrbDisplayData>();
            if (displayData != null && displayData.countText != null)
            {
                displayData.countText.text = count.ToString();
            }
        }
    }
    
    private void OnScryClicked()
    {
        // Check for Orb of Generation
        int generationOrbs = lootManager != null ? lootManager.GetCurrencyAmount(CurrencyType.OrbOfGeneration) : 0;
        if (generationOrbs < 1)
        {
            Debug.LogWarning("[SeerCardGenerationUI] No Orb of Generation available!");
            return;
        }
        
        // Generate card
        CardData generatedCard = SeerCardGenerator.GenerateCard(
            selectedInfusionOrbs,
            selectedPerfectionOrbs,
            selectedSpirits
        );
        
        if (generatedCard == null)
        {
            Debug.LogError("[SeerCardGenerationUI] Failed to generate card!");
            return;
        }
        
        // Consume currencies
        if (lootManager != null)
        {
            // Consume Orb of Generation (required)
            bool consumed = lootManager.RemoveCurrency(CurrencyType.OrbOfGeneration, 1);
            if (!consumed)
            {
                Debug.LogError("[SeerCardGenerationUI] Failed to consume Orb of Generation!");
                return;
            }
            
            // Consume Infusion Orbs (if any selected)
            if (selectedInfusionOrbs > 0)
            {
                bool infusionConsumed = lootManager.RemoveCurrency(CurrencyType.OrbOfInfusion, selectedInfusionOrbs);
                if (!infusionConsumed)
                {
                    Debug.LogError($"[SeerCardGenerationUI] Failed to consume {selectedInfusionOrbs} Orb(s) of Infusion!");
                    // Refund Generation orb since we failed
                    lootManager.AddCurrency(CurrencyType.OrbOfGeneration, 1);
                    return;
                }
            }
            
            // Consume Perfection Orbs (if any selected)
            if (selectedPerfectionOrbs > 0)
            {
                bool perfectionConsumed = lootManager.RemoveCurrency(CurrencyType.OrbOfPerfection, selectedPerfectionOrbs);
                if (!perfectionConsumed)
                {
                    Debug.LogError($"[SeerCardGenerationUI] Failed to consume {selectedPerfectionOrbs} Orb(s) of Perfection!");
                    // Refund Generation and Infusion orbs since we failed
                    lootManager.AddCurrency(CurrencyType.OrbOfGeneration, 1);
                    if (selectedInfusionOrbs > 0)
                        lootManager.AddCurrency(CurrencyType.OrbOfInfusion, selectedInfusionOrbs);
                    return;
                }
            }
            
            // Consume Spirits (if any selected)
            foreach (var kvp in selectedSpirits)
            {
                if (kvp.Value > 0)
                {
                    bool spiritConsumed = lootManager.RemoveCurrency(kvp.Key, kvp.Value);
                    if (!spiritConsumed)
                    {
                        Debug.LogError($"[SeerCardGenerationUI] Failed to consume {kvp.Value} {kvp.Key}!");
                        // Refund all consumed currencies
                        lootManager.AddCurrency(CurrencyType.OrbOfGeneration, 1);
                        if (selectedInfusionOrbs > 0)
                            lootManager.AddCurrency(CurrencyType.OrbOfInfusion, selectedInfusionOrbs);
                        if (selectedPerfectionOrbs > 0)
                            lootManager.AddCurrency(CurrencyType.OrbOfPerfection, selectedPerfectionOrbs);
                        // Refund already consumed spirits
                        foreach (var alreadyConsumed in selectedSpirits)
                        {
                            if (alreadyConsumed.Key != kvp.Key && alreadyConsumed.Value > 0)
                                lootManager.AddCurrency(alreadyConsumed.Key, alreadyConsumed.Value);
                        }
                        return;
                    }
                }
            }
        }
        
        // Unlock card for character
        if (characterManager != null && characterManager.GetCurrentCharacter() != null)
        {
            characterManager.UnlockCard(generatedCard.cardName);
        }
        
        // Display generated card
        DisplayGeneratedCard(generatedCard);
        
        // Reset selections
        selectedInfusionOrbs = 0;
        selectedPerfectionOrbs = 0;
        foreach (var key in selectedSpirits.Keys.ToList())
        {
            selectedSpirits[key] = 0;
        }
        
        // Refresh UI
        RefreshUI();
    }
    
    private void DisplayGeneratedCard(CardData card)
    {
        if (outputCardDisplay != null)
            outputCardDisplay.SetActive(true);
        
        if (outputCardNameText != null)
            outputCardNameText.text = card.cardName;
        
        if (outputCardRarityText != null)
            outputCardRarityText.text = $"Rarity: {card.rarity}";
        
        if (outputCardElementText != null)
            outputCardElementText.text = $"Element: {card.element}";
        
        if (outputCardImage != null && card.cardImage != null)
            outputCardImage.sprite = card.cardImage;
    }
    
    private void OnCloseClicked()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
        else
            gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Open the card generation UI
    /// </summary>
    public void Open()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);
        else
            gameObject.SetActive(true);
        
        RefreshUI();
    }
    
    /// <summary>
    /// Close the card generation UI
    /// </summary>
    public void Close()
    {
        OnCloseClicked();
    }
}

/// <summary>
/// Helper component to store references for spirit UI items
/// </summary>
public class SpiritItemData : MonoBehaviour
{
    public TMP_Text countText;
    public TMP_Text chanceText;
    public CurrencyType spiritType;
}

/// <summary>
/// Helper component to store references for orb UI items
/// </summary>
public class OrbItemData : MonoBehaviour
{
    public TMP_Text countText;
    public TMP_Text chanceText;
    public CurrencyType orbType;
}

/// <summary>
/// Helper component to store references for Generation Orb display (read-only, no buttons)
/// </summary>
public class GenerationOrbDisplayData : MonoBehaviour
{
    public TMP_Text countText;
}


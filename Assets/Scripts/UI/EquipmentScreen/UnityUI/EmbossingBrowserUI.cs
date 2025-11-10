using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace Dexiled.UI.EquipmentScreen
{
    /// <summary>
    /// Main controller for the embossing browser/storage UI
    /// Displays available embossings in a grid with filtering options
    /// </summary>
    public class EmbossingBrowserUI : MonoBehaviour
    {
        [Header("Grid Container")]
        [SerializeField] private Transform gridContainer;
        [SerializeField] private GameObject embossingSlotPrefab;
        [SerializeField] private GridLayoutGroup gridLayout;
        
        [Header("Filters - Dropdowns")]
        [SerializeField] private TMP_Dropdown categoryFilter;
        [SerializeField] private TMP_Dropdown rarityFilter;
        [SerializeField] private TMP_Dropdown elementFilter;
        
        [Header("Filters - Sliders")]
        [SerializeField] private Slider levelFilter;
        [SerializeField] private TextMeshProUGUI levelFilterText;
        
        [Header("Filters - Toggles")]
        [SerializeField] private Toggle showOnlyAffordableToggle;
        [SerializeField] private Toggle showOnlyMeetsRequirementsToggle;
        
        [Header("References")]
        [SerializeField] private CardCarouselUI cardCarousel;
        
        [Header("Tooltip & Confirmation")]
        [SerializeField] private EmbossingTooltip tooltipSystem;
        [SerializeField] private EmbossingConfirmationPanel confirmationPanel;
        
        [Header("Info Display")]
        [SerializeField] private TextMeshProUGUI embossingCountText;
        [SerializeField] private TextMeshProUGUI selectedEmbossingInfo;
        
        private List<EmbossingEffect> allEmbossings = new List<EmbossingEffect>();
        private List<EmbossingEffect> filteredEmbossings = new List<EmbossingEffect>();
        private List<GameObject> embossingSlots = new List<GameObject>();
        
        private EmbossingEffect selectedEmbossing;
        private Card selectedCard;
        
        // Filter state
        private EmbossingCategory? filterCategory = null;
        private EmbossingRarity? filterRarity = null;
        private DamageType? filterElement = null;
        private int filterMinLevel = 1;
        private bool filterAffordable = false;
        private bool filterMeetsRequirements = false;
        
        void Start()
        {
            InitializeFilters();
            LoadEmbossings();
            PopulateGrid();
            
            // Auto-find card carousel if not assigned
            if (cardCarousel == null)
            {
                cardCarousel = FindFirstObjectByType<CardCarouselUI>();
            }
            
            // Auto-find tooltip system if not assigned
            if (tooltipSystem == null)
            {
                tooltipSystem = FindFirstObjectByType<EmbossingTooltip>();
                if (tooltipSystem == null)
                {
                    Debug.LogWarning("[EmbossingBrowserUI] EmbossingTooltip not found in scene. Tooltips will not work.");
                }
            }
            
            // Auto-find confirmation panel if not assigned
            if (confirmationPanel == null)
            {
                confirmationPanel = FindFirstObjectByType<EmbossingConfirmationPanel>();
                if (confirmationPanel == null)
                {
                    Debug.LogWarning("[EmbossingBrowserUI] EmbossingConfirmationPanel not found in scene. Confirmation will not work.");
                }
            }
        }
        
        void Update()
        {
            // Update selected card
            if (cardCarousel != null)
            {
                Card newCard = cardCarousel.GetSelectedCard();
                if (newCard != selectedCard)
                {
                    selectedCard = newCard;
                    
                    // Refresh grid if filtering by requirements
                    if (filterMeetsRequirements)
                    {
                        ApplyFilters();
                    }
                }
            }
        }
        
        /// <summary>
        /// Initialize filter UI components
        /// </summary>
        void InitializeFilters()
        {
            // Category filter
            if (categoryFilter != null)
            {
                categoryFilter.ClearOptions();
                List<string> categories = new List<string> { "All" };
                categories.AddRange(System.Enum.GetNames(typeof(EmbossingCategory)));
                categoryFilter.AddOptions(categories);
                categoryFilter.onValueChanged.AddListener(OnCategoryFilterChanged);
            }
            
            // Rarity filter
            if (rarityFilter != null)
            {
                rarityFilter.ClearOptions();
                List<string> rarities = new List<string> { "All" };
                rarities.AddRange(System.Enum.GetNames(typeof(EmbossingRarity)));
                rarityFilter.AddOptions(rarities);
                rarityFilter.onValueChanged.AddListener(OnRarityFilterChanged);
            }
            
            // Element filter
            if (elementFilter != null)
            {
                elementFilter.ClearOptions();
                List<string> elements = new List<string> { "All" };
                elements.AddRange(System.Enum.GetNames(typeof(DamageType)));
                elementFilter.AddOptions(elements);
                elementFilter.onValueChanged.AddListener(OnElementFilterChanged);
            }
            
            // Level filter
            if (levelFilter != null)
            {
                levelFilter.minValue = 1;
                levelFilter.maxValue = 30;
                levelFilter.value = 1;
                levelFilter.onValueChanged.AddListener(OnLevelFilterChanged);
            }
            
            // Toggle filters
            if (showOnlyAffordableToggle != null)
            {
                showOnlyAffordableToggle.onValueChanged.AddListener(OnAffordableFilterChanged);
            }
            
            if (showOnlyMeetsRequirementsToggle != null)
            {
                showOnlyMeetsRequirementsToggle.onValueChanged.AddListener(OnRequirementsFilterChanged);
            }
            
            Debug.Log("[EmbossingBrowserUI] Filters initialized");
        }
        
        /// <summary>
        /// Load all embossings from database
        /// </summary>
        void LoadEmbossings()
        {
            if (EmbossingDatabase.Instance == null)
            {
                Debug.LogError("[EmbossingBrowserUI] EmbossingDatabase not found!");
                return;
            }
            
            allEmbossings = EmbossingDatabase.Instance.GetAllEmbossings();
            filteredEmbossings = new List<EmbossingEffect>(allEmbossings);
            
            Debug.Log($"[EmbossingBrowserUI] Loaded {allEmbossings.Count} embossings");
            
            UpdateEmbossingCount();
        }
        
        /// <summary>
        /// Populate grid with embossing slots
        /// </summary>
        void PopulateGrid()
        {
            // Clear existing slots
            ClearGrid();
            
            // Create slots for filtered embossings
            foreach (EmbossingEffect embossing in filteredEmbossings)
            {
                CreateEmbossingSlot(embossing);
            }
            
            UpdateEmbossingCount();
        }
        
        /// <summary>
        /// Create a single embossing slot in the grid
        /// </summary>
        void CreateEmbossingSlot(EmbossingEffect embossing)
        {
            if (gridContainer == null || embossingSlotPrefab == null)
            {
                Debug.LogWarning("[EmbossingBrowserUI] Grid container or prefab not assigned!");
                return;
            }
            
            GameObject slotObj = Instantiate(embossingSlotPrefab, gridContainer);
            embossingSlots.Add(slotObj);
            
            // Setup slot with embossing data
            EmbossingSlotUI slot = slotObj.GetComponent<EmbossingSlotUI>();
            if (slot != null)
            {
                slot.SetEmbossing(embossing);
                slot.OnSlotClicked = OnEmbossingSlotClicked; // Legacy callback
                slot.OnSlotClickedForConfirmation = OnEmbossingClickedForApplication; // New confirmation callback
            }
            else
            {
                // Fallback: Setup basic display
                SetupBasicSlot(slotObj, embossing);
            }
        }
        
        /// <summary>
        /// Setup basic slot display (fallback if EmbossingSlotUI doesn't exist)
        /// </summary>
        void SetupBasicSlot(GameObject slotObj, EmbossingEffect embossing)
        {
            // Set name
            TextMeshProUGUI nameText = slotObj.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = embossing.embossingName;
            }
            
            // Set icon
            Image iconImage = slotObj.GetComponentInChildren<Image>();
            if (iconImage != null && embossing.embossingIcon != null)
            {
                iconImage.sprite = embossing.embossingIcon;
            }
            
            // Add button listener
            Button button = slotObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnEmbossingSlotClicked(embossing));
            }
        }
        
        /// <summary>
        /// Clear all slots from grid
        /// </summary>
        void ClearGrid()
        {
            foreach (GameObject slot in embossingSlots)
            {
                Destroy(slot);
            }
            embossingSlots.Clear();
        }
        
        /// <summary>
        /// Apply all active filters
        /// </summary>
        void ApplyFilters()
        {
            filteredEmbossings = new List<EmbossingEffect>(allEmbossings);
            
            // Category filter
            if (filterCategory.HasValue)
            {
                filteredEmbossings = filteredEmbossings
                    .Where(e => e.category == filterCategory.Value)
                    .ToList();
            }
            
            // Rarity filter
            if (filterRarity.HasValue)
            {
                filteredEmbossings = filteredEmbossings
                    .Where(e => e.rarity == filterRarity.Value)
                    .ToList();
            }
            
            // Element filter
            if (filterElement.HasValue)
            {
                filteredEmbossings = filteredEmbossings
                    .Where(e => e.elementType == filterElement.Value)
                    .ToList();
            }
            
            // Level filter
            if (filterMinLevel > 1)
            {
                filteredEmbossings = filteredEmbossings
                    .Where(e => e.minimumLevel <= filterMinLevel)
                    .ToList();
            }
            
            // Requirements filter (needs character)
            if (filterMeetsRequirements && CharacterManager.Instance != null && CharacterManager.Instance.currentCharacter != null)
            {
                Character character = CharacterManager.Instance.currentCharacter;
                filteredEmbossings = filteredEmbossings
                    .Where(e => e.MeetsRequirements(character))
                    .ToList();
            }
            
            // Affordable filter (needs selected card)
            if (filterAffordable && selectedCard != null)
            {
                Character character = CharacterManager.Instance?.currentCharacter;
                if (character != null)
                {
                    filteredEmbossings = EmbossingDatabase.Instance
                        .GetEmbossingsForCard(selectedCard, character);
                }
            }
            
            // Refresh grid
            PopulateGrid();
            
            Debug.Log($"[EmbossingBrowserUI] Filtered to {filteredEmbossings.Count} embossings");
        }
        
        #region Filter Callbacks
        
        void OnCategoryFilterChanged(int index)
        {
            if (index == 0)
            {
                filterCategory = null;
            }
            else
            {
                filterCategory = (EmbossingCategory)(index - 1);
            }
            ApplyFilters();
        }
        
        void OnRarityFilterChanged(int index)
        {
            if (index == 0)
            {
                filterRarity = null;
            }
            else
            {
                filterRarity = (EmbossingRarity)(index - 1);
            }
            ApplyFilters();
        }
        
        void OnElementFilterChanged(int index)
        {
            if (index == 0)
            {
                filterElement = null;
            }
            else
            {
                filterElement = (DamageType)(index - 1);
            }
            ApplyFilters();
        }
        
        void OnLevelFilterChanged(float value)
        {
            filterMinLevel = Mathf.RoundToInt(value);
            
            if (levelFilterText != null)
            {
                levelFilterText.text = $"Level: {filterMinLevel}";
            }
            
            ApplyFilters();
        }
        
        void OnAffordableFilterChanged(bool value)
        {
            filterAffordable = value;
            ApplyFilters();
        }
        
        void OnRequirementsFilterChanged(bool value)
        {
            filterMeetsRequirements = value;
            ApplyFilters();
        }
        
        #endregion
        
        /// <summary>
        /// Handle embossing slot click (legacy callback)
        /// </summary>
        void OnEmbossingSlotClicked(EmbossingEffect embossing)
        {
            selectedEmbossing = embossing;
            UpdateSelectedEmbossingInfo();
            
            Debug.Log($"[EmbossingBrowserUI] Selected embossing: {embossing.embossingName}");
        }
        
        /// <summary>
        /// Handle embossing click for application (new confirmation system)
        /// </summary>
        void OnEmbossingClickedForApplication(EmbossingEffect embossing)
        {
            if (embossing == null)
            {
                Debug.LogWarning("[EmbossingBrowserUI] Embossing is null");
                return;
            }
            
            if (selectedCard == null)
            {
                Debug.LogWarning("[EmbossingBrowserUI] No card selected. Cannot apply embossing.");
                return;
            }
            
            if (confirmationPanel == null)
            {
                Debug.LogError("[EmbossingBrowserUI] Confirmation panel not found!");
                return;
            }
            
            // Determine next available slot
            int embossingCount = selectedCard.appliedEmbossings != null ? selectedCard.appliedEmbossings.Count : 0;
            int slotIndex = embossingCount;
            
            // Check if card has available slots
            if (slotIndex >= selectedCard.embossingSlots)
            {
                Debug.LogWarning($"[EmbossingBrowserUI] Card '{selectedCard.cardName}' has no available embossing slots ({embossingCount}/{selectedCard.embossingSlots})");
                
                // Show message to user (you can implement a toast/notification here)
                if (selectedEmbossingInfo != null)
                {
                    selectedEmbossingInfo.text = "<color=red>Card has no available embossing slots!</color>\n\nUse 'Add Embossing Slot' to add more slots.";
                }
                
                return;
            }
            
            // Show confirmation panel
            confirmationPanel.ShowConfirmation(embossing, selectedCard, slotIndex);
            
            Debug.Log($"[EmbossingBrowserUI] Showing confirmation for {embossing.embossingName} on {selectedCard.cardName} (slot {slotIndex + 1})");
        }
        
        /// <summary>
        /// Update embossing count display
        /// </summary>
        void UpdateEmbossingCount()
        {
            if (embossingCountText != null)
            {
                embossingCountText.text = $"{filteredEmbossings.Count} / {allEmbossings.Count} Embossings";
            }
        }
        
        /// <summary>
        /// Update selected embossing info display
        /// </summary>
        void UpdateSelectedEmbossingInfo()
        {
            if (selectedEmbossingInfo == null || selectedEmbossing == null) return;
            
            string info = $"<b>{selectedEmbossing.embossingName}</b>\n";
            info += $"<color=#888888>{selectedEmbossing.description}</color>\n\n";
            info += $"Category: {selectedEmbossing.category}\n";
            info += $"Rarity: {selectedEmbossing.rarity}\n";
            info += $"Mana Cost: +{(selectedEmbossing.manaCostMultiplier * 100):F0}%\n";
            
            if (selectedEmbossing.minimumLevel > 1 || 
                selectedEmbossing.minimumStrength > 0 || 
                selectedEmbossing.minimumDexterity > 0 || 
                selectedEmbossing.minimumIntelligence > 0)
            {
                info += $"\n<b>Requirements:</b>\n{selectedEmbossing.GetRequirementsText()}";
            }
            
            selectedEmbossingInfo.text = info;
        }
        
        /// <summary>
        /// Reset all filters
        /// </summary>
        public void ResetFilters()
        {
            if (categoryFilter != null) categoryFilter.value = 0;
            if (rarityFilter != null) rarityFilter.value = 0;
            if (elementFilter != null) elementFilter.value = 0;
            if (levelFilter != null) levelFilter.value = 1;
            if (showOnlyAffordableToggle != null) showOnlyAffordableToggle.isOn = false;
            if (showOnlyMeetsRequirementsToggle != null) showOnlyMeetsRequirementsToggle.isOn = false;
            
            filterCategory = null;
            filterRarity = null;
            filterElement = null;
            filterMinLevel = 1;
            filterAffordable = false;
            filterMeetsRequirements = false;
            
            ApplyFilters();
        }
        
        /// <summary>
        /// Get currently selected embossing
        /// </summary>
        public EmbossingEffect GetSelectedEmbossing()
        {
            return selectedEmbossing;
        }
        
        /// <summary>
        /// Refresh the grid (call after database changes)
        /// </summary>
        public void RefreshGrid()
        {
            LoadEmbossings();
            ApplyFilters();
        }
    }
}


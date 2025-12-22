using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace Dexiled.UI.EquipmentScreen
{
    /// <summary>
    /// Filter controller for EmbossingGridUI
    /// Works with existing EmbossingGridUI to filter displayed embossings
    /// </summary>
    public class EmbossingFilterController : MonoBehaviour
    {
        [Header("Grid Reference")]
        [SerializeField] private EmbossingGridUI embossingGrid;
        
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
        
        private EmbossingEffect selectedEmbossing;
        private Card selectedCard;
        
        // Filter state
        private EmbossingCategory? filterCategory = null;
        private EmbossingRarity? filterRarity = null;
        private DamageType? filterElement = null;
        private int filterMinLevel = 1;
        private bool filterAffordable = false;
        private bool filterMeetsRequirements = false;
        private bool filterByCardTags = true; // Default: filter by selected card's tags
        
        void Start()
        {
            // Auto-find embossing grid if not assigned
            if (embossingGrid == null)
            {
                embossingGrid = GetComponentInParent<EmbossingGridUI>();
                if (embossingGrid == null)
                {
                    embossingGrid = FindFirstObjectByType<EmbossingGridUI>();
                }
            }
            
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
                    Debug.LogWarning("[EmbossingFilterController] EmbossingTooltip not found in scene. Tooltips will not work.");
                }
            }
            
            // Auto-find confirmation panel if not assigned
            if (confirmationPanel == null)
            {
                confirmationPanel = FindFirstObjectByType<EmbossingConfirmationPanel>();
                if (confirmationPanel == null)
                {
                    Debug.LogWarning("[EmbossingFilterController] EmbossingConfirmationPanel not found in scene. Confirmation will not work.");
                }
            }
            
            InitializeFilters();
            LoadEmbossings();
            
            // Hook into grid's click callback
            if (embossingGrid != null)
            {
                embossingGrid.OnEmbossingClicked += OnEmbossingSlotClicked;
            }
            
            ApplyFilters();
        }
        
        void OnDestroy()
        {
            // Unsubscribe from callback
            if (embossingGrid != null)
            {
                embossingGrid.OnEmbossingClicked -= OnEmbossingSlotClicked;
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
                    
                    // Refresh grid if filtering by requirements or card tags
                    if (filterMeetsRequirements || filterAffordable || filterByCardTags)
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
                
                if (levelFilterText != null)
                {
                    levelFilterText.text = "Level: 1";
                }
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
            
            Debug.Log("[EmbossingFilterController] Filters initialized");
        }
        
        /// <summary>
        /// Load all embossings from database
        /// </summary>
        void LoadEmbossings()
        {
            if (EmbossingDatabase.Instance == null)
            {
                Debug.LogError("[EmbossingFilterController] EmbossingDatabase not found!");
                return;
            }
            
            allEmbossings = EmbossingDatabase.Instance.GetAllEmbossings();
            filteredEmbossings = new List<EmbossingEffect>(allEmbossings);
            
            Debug.Log($"[EmbossingFilterController] Loaded {allEmbossings.Count} embossings");
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
            if (filterAffordable && selectedCard != null && CharacterManager.Instance != null && CharacterManager.Instance.currentCharacter != null)
            {
                Character character = CharacterManager.Instance.currentCharacter;
                List<EmbossingEffect> affordable = EmbossingDatabase.Instance
                    .GetEmbossingsForCard(selectedCard, character);
                
                // Intersect with already filtered list
                filteredEmbossings = filteredEmbossings
                    .Where(e => affordable.Contains(e))
                    .ToList();
            }
            
            // Filter by card tags (based on embossing modifier requirements)
            if (filterByCardTags && selectedCard != null)
            {
                filteredEmbossings = filteredEmbossings
                    .Where(e => IsEmbossingCompatibleWithCard(e, selectedCard))
                    .ToList();
            }
            
            // Update grid with filtered embossings
            if (embossingGrid != null)
            {
                embossingGrid.SetEmbossings(filteredEmbossings);
            }
            
            UpdateEmbossingCount();
            
            Debug.Log($"[EmbossingFilterController] Filtered to {filteredEmbossings.Count} embossings");
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
        /// Handle embossing slot click (from EmbossingGridUI)
        /// </summary>
        public void OnEmbossingSlotClicked(EmbossingEffect embossing)
        {
            selectedEmbossing = embossing;
            UpdateSelectedEmbossingInfo();
            
            Debug.Log($"[EmbossingFilterController] Selected embossing: {embossing.embossingName}");
            
            // Show confirmation panel for applying embossing
            ShowConfirmationForEmbossing(embossing);
        }
        
        /// <summary>
        /// Show confirmation panel for applying embossing to selected card
        /// </summary>
        void ShowConfirmationForEmbossing(EmbossingEffect embossing)
        {
            if (embossing == null)
            {
                Debug.LogWarning("[EmbossingFilterController] Embossing is null");
                return;
            }
            
            if (selectedCard == null)
            {
                Debug.LogWarning("[EmbossingFilterController] No card selected. Cannot apply embossing.");
                return;
            }
            
            if (confirmationPanel == null)
            {
                Debug.LogError("[EmbossingFilterController] Confirmation panel not found!");
                return;
            }
            
            // Determine next available slot
            int embossingCount = selectedCard.appliedEmbossings != null ? selectedCard.appliedEmbossings.Count : 0;
            int slotIndex = embossingCount;
            
            // Check if card has available slots
            if (slotIndex >= selectedCard.embossingSlots)
            {
                Debug.LogWarning($"[EmbossingFilterController] Card '{selectedCard.cardName}' has no available embossing slots ({embossingCount}/{selectedCard.embossingSlots})");
                
                // Show message to user
                if (selectedEmbossingInfo != null)
                {
                    selectedEmbossingInfo.text = "<color=red>Card has no available embossing slots!</color>\n\nUse 'Add Embossing Slot' to add more slots.";
                }
                
                return;
            }
            
            // Show confirmation panel
            confirmationPanel.ShowConfirmation(embossing, selectedCard, slotIndex);
            
            Debug.Log($"[EmbossingFilterController] Showing confirmation for {embossing.embossingName} on {selectedCard.cardName} (slot {slotIndex + 1})");
        }
        
        /// <summary>
        /// Update embossing count display
        /// </summary>
        void UpdateEmbossingCount()
        {
            if (embossingCountText != null)
            {
                int total = allEmbossings.Count;
                int filtered = filteredEmbossings.Count;
                embossingCountText.text = $"{filtered} / {total} Embossings";
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
        /// Check if an embossing is compatible with a card based on the card's tags
        /// Uses the embossing's modifier definitions to check requiredCardTags
        /// </summary>
        private bool IsEmbossingCompatibleWithCard(EmbossingEffect embossing, Card card)
        {
            if (embossing == null || card == null)
                return false;
            
            // If embossing has no modifiers, it's compatible with all cards
            if (embossing.modifierIds == null || embossing.modifierIds.Count == 0)
                return true;
            
            // Get the embossing's modifiers
            var modifierRegistry = EmbossingModifierRegistry.Instance;
            if (modifierRegistry == null)
                return true; // If registry not available, assume compatible
            
            // Check if any modifier is compatible with the card
            // An embossing is compatible if at least one of its modifiers can apply to the card
            foreach (string modifierId in embossing.modifierIds)
            {
                var modifierDef = modifierRegistry.GetModifier(modifierId);
                if (modifierDef == null)
                    continue;
                
                // If modifier has no requiredCardTags, it applies to all cards
                if (modifierDef.requiredCardTags == null || modifierDef.requiredCardTags.Count == 0)
                    return true; // At least one modifier applies to all cards
                
                // Check if card has the required tags
                bool isCompatible = false;
                
                if (modifierDef.requireAllTags)
                {
                    // AND logic: card must have ALL required tags
                    isCompatible = true;
                    foreach (string requiredTag in modifierDef.requiredCardTags)
                    {
                        if (card.tags == null || !card.tags.Contains(requiredTag))
                        {
                            isCompatible = false;
                            break;
                        }
                    }
                }
                else
                {
                    // OR logic (default): card must have AT LEAST ONE required tag
                    isCompatible = false;
                    foreach (string requiredTag in modifierDef.requiredCardTags)
                    {
                        if (card.tags != null && card.tags.Contains(requiredTag))
                        {
                            isCompatible = true;
                            break;
                        }
                    }
                }
                
                // If this modifier is compatible, the embossing is compatible
                if (isCompatible)
                    return true;
            }
            
            // If no modifiers are compatible, the embossing is not compatible
            return false;
        }
        
        /// <summary>
        /// Refresh filters (call after database changes)
        /// </summary>
        public void RefreshFilters()
        {
            LoadEmbossings();
            ApplyFilters();
        }
    }
}


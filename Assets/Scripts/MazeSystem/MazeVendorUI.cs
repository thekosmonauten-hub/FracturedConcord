using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Dexiled.Data.Items;

namespace Dexiled.MazeSystem
{
    /// <summary>
    /// Manages the Maze Vendor UI - displays items for purchase with maze currency.
    /// Generates items around player level (±5 levels) with Magic or Rare rarity.
    /// </summary>
    public class MazeVendorUI : MonoBehaviour
    {
        [Header("Vendor Configuration")]
        [Tooltip("Number of items to generate in the vendor")]
        [Range(6, 24)]
        public int vendorItemCount = 12;
        
        [Tooltip("Level range around player level (±5 means level 34 → items level 29-39)")]
        [Range(1, 10)]
        public int levelRange = 5;
        
        [Tooltip("Chance for items to be Rare (vs Magic)")]
        [Range(0f, 1f)]
        public float rareChance = 0.3f;
        
        [Header("UI References")]
        [Tooltip("Container for vendor item slots (Grid Layout Group)")]
        public Transform itemGridContainer;
        
        [Tooltip("Prefab for vendor item slot")]
        public GameObject vendorItemSlotPrefab;
        
        
        [Tooltip("Text displaying current maze currency")]
        public TextMeshProUGUI currencyDisplayText;
        
        [Tooltip("Refresh button to regenerate vendor items")]
        public Button refreshButton;
        
        [Tooltip("Purchase button (outside of item slots)")]
        public Button purchaseButton;
        
        [Tooltip("Selected item display (shows selected item info)")]
        public TextMeshProUGUI selectedItemText;
        
        [Tooltip("Refresh cost in maze currency")]
        public int refreshCost = 10;
        
        [Tooltip("Currency type for refresh cost")]
        public CurrencyType refreshCurrencyType = CurrencyType.MandateFragment;
        
        [Header("Item Pricing")]
        [Tooltip("Base cost multiplier for items (cost = itemLevel * baseCostMultiplier)")]
        [Range(1, 20)]
        public int baseCostMultiplier = 2;
        
        [Tooltip("Rare item cost multiplier")]
        [Range(1f, 3f)]
        public float rareCostMultiplier = 1.5f;
        
        private List<BaseItem> vendorItems = new List<BaseItem>();
        private List<MazeVendorItemSlot> vendorSlots = new List<MazeVendorItemSlot>();
        private MazeVendorItemSlot selectedSlot = null;
        private CharacterManager characterManager;
        private MazeCurrencyManager currencyManager;
        private ItemDatabase itemDatabase;
        
        private void Awake()
        {
            characterManager = CharacterManager.Instance;
            currencyManager = MazeCurrencyManager.Instance;
            itemDatabase = ItemDatabase.Instance;
        }
        
        private void Start()
        {
            if (refreshButton != null)
            {
                refreshButton.onClick.AddListener(RefreshVendor);
            }
            
            if (purchaseButton != null)
            {
                purchaseButton.onClick.AddListener(OnPurchaseClicked);
                purchaseButton.interactable = false; // Disabled until item is selected
            }
        }
        
        private void OnEnable()
        {
            // Generate vendor items when panel opens
            GenerateVendorItems();
            UpdateCurrencyDisplay();
        }
        
        /// <summary>
        /// Generates vendor items based on player level.
        /// </summary>
        public void GenerateVendorItems()
        {
            // Get player level
            int playerLevel = GetPlayerLevel();
            if (playerLevel <= 0)
            {
                Debug.LogWarning("[MazeVendorUI] Cannot generate vendor items: No player character found!");
                return;
            }
            
            // Calculate level range
            int minLevel = Mathf.Max(1, playerLevel - levelRange);
            int maxLevel = playerLevel + levelRange;
            
            Debug.Log($"[MazeVendorUI] Generating {vendorItemCount} vendor items for player level {playerLevel} (items level {minLevel}-{maxLevel})");
            
            // Clear existing items
            ClearVendorItems();
            vendorItems.Clear();
            
            // Generate items
            for (int i = 0; i < vendorItemCount; i++)
            {
                BaseItem item = GenerateVendorItem(minLevel, maxLevel);
                if (item != null)
                {
                    vendorItems.Add(item);
                }
            }
            
            // Display items
            DisplayVendorItems();
        }
        
        /// <summary>
        /// Generates a single vendor item with random level in range and Magic/Rare rarity.
        /// </summary>
        private BaseItem GenerateVendorItem(int minLevel, int maxLevel)
        {
            if (itemDatabase == null)
            {
                Debug.LogError("[MazeVendorUI] ItemDatabase is null! Cannot generate vendor items.");
                return null;
            }
            
            // Get random item level in range
            int itemLevel = Random.Range(minLevel, maxLevel + 1);
            
            // Determine rarity
            ItemRarity targetRarity = Random.Range(0f, 1f) < rareChance ? ItemRarity.Rare : ItemRarity.Magic;
            
            // Get random base item from database
            List<BaseItem> allItems = itemDatabase.GetAllItems();
            if (allItems == null || allItems.Count == 0)
            {
                Debug.LogWarning("[MazeVendorUI] No items found in ItemDatabase!");
                return null;
            }
            
            // Filter items by level requirement (should be <= itemLevel)
            List<BaseItem> validItems = allItems.Where(item => item.requiredLevel <= itemLevel).ToList();
            if (validItems.Count == 0)
            {
                Debug.LogWarning($"[MazeVendorUI] No items found for level {itemLevel}!");
                return null;
            }
            
            // Pick random item
            BaseItem baseItem = validItems[Random.Range(0, validItems.Count)];
            
            // Create runtime copy
            BaseItem vendorItem = CreateItemCopy(baseItem);
            if (vendorItem == null)
            {
                Debug.LogError("[MazeVendorUI] Failed to create item copy!");
                return null;
            }
            
            // Set item level
            vendorItem.requiredLevel = itemLevel;
            
            // Generate affixes with forced rarity
            if (AffixDatabase_Modern.Instance != null)
            {
                AffixDatabase_Modern.Instance.GenerateRandomAffixes(vendorItem, itemLevel, targetRarity);
            }
            else
            {
                Debug.LogWarning("[MazeVendorUI] AffixDatabase_Modern.Instance is null! Item will have no affixes.");
            }
            
            Debug.Log($"[MazeVendorUI] Generated {targetRarity} {vendorItem.itemName} (Level {itemLevel})");
            
            return vendorItem;
        }
        
        /// <summary>
        /// Creates a runtime copy of a base item.
        /// </summary>
        private BaseItem CreateItemCopy(BaseItem original)
        {
            if (original == null) return null;
            
            // Create instance (runtime copy)
            BaseItem copy = ScriptableObject.Instantiate(original);
            
            // Clear affixes (will be regenerated)
            copy.prefixes.Clear();
            copy.suffixes.Clear();
            
            return copy;
        }
        
        /// <summary>
        /// Displays vendor items in the grid.
        /// </summary>
        private void DisplayVendorItems()
        {
            if (itemGridContainer == null || vendorItemSlotPrefab == null)
            {
                Debug.LogError("[MazeVendorUI] Item grid container or prefab not assigned!");
                return;
            }
            
            // Clear existing slots
            foreach (Transform child in itemGridContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Clear slot list
            vendorSlots.Clear();
            selectedSlot = null;
            
            // Create slots for each item
            foreach (BaseItem item in vendorItems)
            {
                if (item == null) continue;
                
                GameObject slotObj = Instantiate(vendorItemSlotPrefab, itemGridContainer);
                
                // Setup vendor item slot
                MazeVendorItemSlot slot = slotObj.GetComponent<MazeVendorItemSlot>();
                if (slot == null)
                {
                    slot = slotObj.AddComponent<MazeVendorItemSlot>();
                }
                
                // Calculate price
                int price = CalculateItemPrice(item);
                
                // Initialize slot with selection callback
                slot.Initialize(item, price, OnItemSelected);
                
                vendorSlots.Add(slot);
            }
            
            // Update purchase button state
            UpdatePurchaseButton();
        }
        
        /// <summary>
        /// Calculates the price for an item based on level and rarity.
        /// </summary>
        private int CalculateItemPrice(BaseItem item)
        {
            int basePrice = item.requiredLevel * baseCostMultiplier;
            
            if (item.GetCalculatedRarity() == ItemRarity.Rare)
            {
                basePrice = Mathf.RoundToInt(basePrice * rareCostMultiplier);
            }
            
            return basePrice;
        }
        
        /// <summary>
        /// Handles item selection.
        /// </summary>
        private void OnItemSelected(MazeVendorItemSlot slot)
        {
            if (slot == null || slot.IsSoldOut())
            {
                return;
            }
            
            // Deselect previous slot
            if (selectedSlot != null && selectedSlot != slot)
            {
                selectedSlot.SetSelected(false);
            }
            
            // Select new slot
            selectedSlot = slot;
            selectedSlot.SetSelected(true);
            
            // Update UI
            UpdateSelectedItemDisplay();
            UpdatePurchaseButton();
        }
        
        /// <summary>
        /// Handles purchase button click.
        /// </summary>
        private void OnPurchaseClicked()
        {
            if (selectedSlot == null || selectedSlot.IsSoldOut())
            {
                Debug.LogWarning("[MazeVendorUI] No item selected or item is sold out!");
                return;
            }
            
            BaseItem item = selectedSlot.GetItem();
            int price = selectedSlot.GetPrice();
            CurrencyType currencyType = selectedSlot.GetCurrencyType();
            
            if (item == null)
            {
                Debug.LogError("[MazeVendorUI] Cannot purchase null item!");
                return;
            }
            
            // Check currency
            if (currencyManager == null || !currencyManager.HasCurrency(currencyType, price))
            {
                Debug.LogWarning($"[MazeVendorUI] Insufficient currency! Need {price} {currencyType}");
                // TODO: Show error message to player
                return;
            }
            
            // Spend currency
            if (!currencyManager.SpendCurrency(currencyType, price))
            {
                Debug.LogWarning("[MazeVendorUI] Failed to spend currency!");
                return;
            }
            
            // Add item to character inventory
            if (characterManager != null && characterManager.HasCharacter())
            {
                characterManager.AddItem(item);
                Debug.Log($"[MazeVendorUI] Purchased {item.GetDisplayName()} for {price} {currencyType}");
            }
            else
            {
                Debug.LogWarning("[MazeVendorUI] Cannot add item to inventory: No character loaded!");
            }
            
            // Mark slot as sold out
            selectedSlot.MarkAsSoldOut();
            vendorItems.Remove(item);
            
            // Clear selection
            selectedSlot = null;
            UpdateSelectedItemDisplay();
            UpdatePurchaseButton();
            UpdateCurrencyDisplay();
        }
        
        /// <summary>
        /// Updates the selected item display.
        /// </summary>
        private void UpdateSelectedItemDisplay()
        {
            if (selectedItemText == null) return;
            
            if (selectedSlot != null && !selectedSlot.IsSoldOut())
            {
                BaseItem item = selectedSlot.GetItem();
                selectedItemText.text = $"Selected: {item.GetDisplayName()} - {selectedSlot.GetPrice()} {selectedSlot.GetCurrencyType()}";
            }
            else
            {
                selectedItemText.text = "No item selected";
            }
        }
        
        /// <summary>
        /// Updates the purchase button state.
        /// </summary>
        private void UpdatePurchaseButton()
        {
            if (purchaseButton == null) return;
            
            bool canPurchase = selectedSlot != null && 
                              !selectedSlot.IsSoldOut() && 
                              currencyManager != null &&
                              currencyManager.HasCurrency(selectedSlot.GetCurrencyType(), selectedSlot.GetPrice());
            
            purchaseButton.interactable = canPurchase;
        }
        
        /// <summary>
        /// Refreshes the vendor inventory (generates new items).
        /// </summary>
        public void RefreshVendor()
        {
            // Check refresh cost
            if (currencyManager == null || !currencyManager.HasCurrency(refreshCurrencyType, refreshCost))
            {
                Debug.LogWarning($"[MazeVendorUI] Cannot refresh: Need {refreshCost} {refreshCurrencyType}");
                // TODO: Show error message to player
                return;
            }
            
            // Spend refresh cost
            if (!currencyManager.SpendCurrency(refreshCurrencyType, refreshCost))
            {
                Debug.LogWarning("[MazeVendorUI] Failed to spend refresh cost!");
                return;
            }
            
            // Generate new items
            GenerateVendorItems();
            UpdateCurrencyDisplay();
            
            Debug.Log($"[MazeVendorUI] Vendor refreshed for {refreshCost} {refreshCurrencyType}");
        }
        
        /// <summary>
        /// Clears all vendor items from the grid.
        /// </summary>
        private void ClearVendorItems()
        {
            if (itemGridContainer == null) return;
            
            foreach (Transform child in itemGridContainer)
            {
                Destroy(child.gameObject);
            }
        }
        
        /// <summary>
        /// Gets the current player level.
        /// </summary>
        private int GetPlayerLevel()
        {
            if (characterManager != null && characterManager.HasCharacter())
            {
                return characterManager.GetCurrentCharacter().level;
            }
            return 0;
        }
        
        /// <summary>
        /// Updates the currency display.
        /// </summary>
        public void UpdateCurrencyDisplay()
        {
            if (currencyDisplayText == null || currencyManager == null)
                return;
            
            // Display primary maze currency (Mandate Fragment by default)
            int currencyAmount = currencyManager.GetCurrencyAmount(CurrencyType.MandateFragment);
            currencyDisplayText.text = $"Mandate Fragments: {currencyAmount}";
            
            // TODO: Display multiple currencies if needed
        }
    }
}


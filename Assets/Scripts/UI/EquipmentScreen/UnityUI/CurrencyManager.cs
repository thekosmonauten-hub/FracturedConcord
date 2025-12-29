using UnityEngine;
using Dexiled.Data.Items;

namespace Dexiled.UI.EquipmentScreen
{
    /// <summary>
    /// Main controller for the currency display system.
    /// Manages all three currency sections (Orbs, Spirits, Fragments) and 
    /// coordinates updates from the CurrencyDatabase.
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        [Header("Database Reference")]
        [SerializeField] private CurrencyDatabase currencyDatabase;

        [Header("Section Controllers")]
        [SerializeField] private CurrencySectionController orbsSection;
        [SerializeField] private CurrencySectionController spiritsSection;
        [SerializeField] private CurrencySectionController fragmentsSection;

        [Header("Settings")]
        [SerializeField] private bool initializeOnStart = true;

        private bool isInitialized = false;

        #region Unity Lifecycle

        private void Start()
        {
            if (initializeOnStart)
            {
                Initialize();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes all currency sections with data from the CurrencyDatabase.
        /// Call this manually if initializeOnStart is false.
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[CurrencyManager] Already initialized. Call Refresh() instead.");
                return;
            }

            if (!ValidateSetup())
            {
                return;
            }

            // Load database if it's in Resources (use preloader if available)
            if (currencyDatabase == null)
            {
                if (AssetPreloader.Instance != null)
                {
                    currencyDatabase = AssetPreloader.Instance.GetPreloadedAsset<CurrencyDatabase>("CurrencyDatabase");
                }
                if (currencyDatabase == null)
                {
                    currencyDatabase = Resources.Load<CurrencyDatabase>("CurrencyDatabase");
                }
                if (currencyDatabase == null)
                {
                    Debug.LogError("[CurrencyManager] Could not load CurrencyDatabase from Resources!");
                    return;
                }
            }

            // Initialize each section
            if (orbsSection != null)
            {
                orbsSection.InitializeSection(currencyDatabase);
            }

            if (spiritsSection != null)
            {
                spiritsSection.InitializeSection(currencyDatabase);
            }

            if (fragmentsSection != null)
            {
                fragmentsSection.InitializeSection(currencyDatabase);
            }

            isInitialized = true;
            Debug.Log("[CurrencyManager] Currency system initialized successfully!");
            
            // CRITICAL: Sync from LootManager AFTER initialization so display items exist
            SyncFromLootManager();
        }

        /// <summary>
        /// Refreshes all currency displays (call after quantities change in database).
        /// </summary>
        public void Refresh()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[CurrencyManager] Not initialized. Initializing now...");
                Initialize();
                // After initialization, sync from LootManager
                SyncFromLootManager();
                return;
            }
            
            // Sync from LootManager before refreshing display
            SyncFromLootManager();

            // UpdateAllDisplays is now redundant since SyncFromLootManager calls UpdateCurrency for each
            // But keeping it as a safety net
            orbsSection?.UpdateAllDisplays();
            spiritsSection?.UpdateAllDisplays();
            fragmentsSection?.UpdateAllDisplays();

            Debug.Log("[CurrencyManager] All currency displays refreshed");
        }
        
        /// <summary>
        /// Syncs currency quantities from LootManager to CurrencyDatabase AND updates all displays
        /// CRITICAL: LootManager stores the actual player currencies!
        /// </summary>
        public void SyncFromLootManager()
        {
            if (LootManager.Instance == null)
            {
                Debug.LogWarning("[CurrencyManager] LootManager.Instance is null - cannot sync currencies!");
                return;
            }
            
            if (currencyDatabase == null)
            {
                Debug.LogWarning("[CurrencyManager] CurrencyDatabase is null - cannot sync currencies!");
                return;
            }
            
            // Get all currencies from LootManager
            var lootManagerCurrencies = LootManager.Instance.GetAllCurrencies();
            
            // Update CurrencyDatabase with actual quantities AND update display items
            int updatedCount = 0;
            foreach (var kvp in lootManagerCurrencies)
            {
                CurrencyType type = kvp.Key;
                int quantity = kvp.Value;
                
                CurrencyData currency = currencyDatabase.GetCurrency(type);
                if (currency != null)
                {
                    currency.quantity = quantity;
                    
                    // Also update the spawned display items directly
                    UpdateCurrency(type, quantity);
                    updatedCount++;
                    
                    Debug.Log($"[CurrencyManager] Synced {type} = {quantity}");
                }
                else
                {
                    Debug.LogWarning($"[CurrencyManager] Currency {type} not found in CurrencyDatabase!");
                }
            }
            
            Debug.Log($"[CurrencyManager] Synced {updatedCount}/{lootManagerCurrencies.Count} currencies from LootManager");
        }

        /// <summary>
        /// Updates a specific currency's display quantity.
        /// More efficient than Refresh() when only one currency changes.
        /// </summary>
        /// <param name="currencyType">The type of currency to update</param>
        /// <param name="newQuantity">The new quantity value</param>
        public void UpdateCurrency(CurrencyType currencyType, int newQuantity)
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[CurrencyManager] Cannot update currency - not initialized!");
                return;
            }

            // Update in database
            CurrencyData currency = currencyDatabase.GetCurrency(currencyType);
            if (currency != null)
            {
                currency.quantity = newQuantity;
            }
            else
            {
                Debug.LogWarning($"[CurrencyManager] Currency {currencyType} not found in database!");
                return;
            }

            // Update in appropriate section
            int typeIndex = (int)currencyType;
            bool updated = false;

            if (typeIndex >= 0 && typeIndex <= 8)
            {
                if (orbsSection != null)
                {
                    orbsSection.UpdateCurrencyQuantity(currencyType, newQuantity);
                    updated = true;
                }
            }
            else if (typeIndex >= 9 && typeIndex <= 17)
            {
                if (spiritsSection != null)
                {
                    spiritsSection.UpdateCurrencyQuantity(currencyType, newQuantity);
                    updated = true;
                }
            }
            else if (typeIndex >= 18 && typeIndex <= 27)
            {
                if (fragmentsSection != null)
                {
                    fragmentsSection.UpdateCurrencyQuantity(currencyType, newQuantity);
                    updated = true;
                }
            }
            
            if (!updated)
            {
                Debug.LogWarning($"[CurrencyManager] Currency {currencyType} (index {typeIndex}) doesn't match any section!");
            }
        }

        /// <summary>
        /// Clears and re-initializes all sections (useful for complete resets).
        /// </summary>
        public void Reinitialize()
        {
            if (isInitialized)
            {
                orbsSection?.ClearSection();
                spiritsSection?.ClearSection();
                fragmentsSection?.ClearSection();
                isInitialized = false;
            }

            Initialize();
        }

        /// <summary>
        /// Gets the currently loaded CurrencyDatabase reference.
        /// </summary>
        public CurrencyDatabase GetDatabase()
        {
            return currencyDatabase;
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates that all required references are assigned.
        /// </summary>
        private bool ValidateSetup()
        {
            bool isValid = true;

            if (orbsSection == null)
            {
                Debug.LogError("[CurrencyManager] Orbs Section not assigned!");
                isValid = false;
            }

            if (spiritsSection == null)
            {
                Debug.LogError("[CurrencyManager] Spirits Section not assigned!");
                isValid = false;
            }

            if (fragmentsSection == null)
            {
                Debug.LogError("[CurrencyManager] Fragments Section not assigned!");
                isValid = false;
            }

            return isValid;
        }

        #endregion

        #region Editor Helpers
#if UNITY_EDITOR
        /// <summary>
        /// Attempts to auto-assign section controllers from children.
        /// </summary>
        [ContextMenu("Auto-Assign Sections")]
        private void AutoAssignSections()
        {
            if (orbsSection == null)
            {
                CurrencySectionController[] sections = GetComponentsInChildren<CurrencySectionController>(true);
                foreach (var section in sections)
                {
                    if (section.name.Contains("Orb"))
                    {
                        orbsSection = section;
                        Debug.Log("[CurrencyManager] Auto-assigned Orbs Section");
                        break;
                    }
                }
            }

            if (spiritsSection == null)
            {
                CurrencySectionController[] sections = GetComponentsInChildren<CurrencySectionController>(true);
                foreach (var section in sections)
                {
                    if (section.name.Contains("Spirit"))
                    {
                        spiritsSection = section;
                        Debug.Log("[CurrencyManager] Auto-assigned Spirits Section");
                        break;
                    }
                }
            }

            if (fragmentsSection == null)
            {
                CurrencySectionController[] sections = GetComponentsInChildren<CurrencySectionController>(true);
                foreach (var section in sections)
                {
                    if (section.name.Contains("Fragment"))
                    {
                        fragmentsSection = section;
                        Debug.Log("[CurrencyManager] Auto-assigned Fragments Section");
                        break;
                    }
                }
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Loads the CurrencyDatabase from Resources folder.
        /// </summary>
        [ContextMenu("Load Database from Resources")]
        private void LoadDatabaseFromResources()
        {
            currencyDatabase = Resources.Load<CurrencyDatabase>("CurrencyDatabase");
            
            if (currencyDatabase != null)
            {
                Debug.Log("[CurrencyManager] Successfully loaded CurrencyDatabase from Resources");
                UnityEditor.EditorUtility.SetDirty(this);
            }
            else
            {
                Debug.LogError("[CurrencyManager] Could not find CurrencyDatabase in Resources folder!");
            }
        }
#endif
        #endregion
    }
}


using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Dexiled.Data.Items;

namespace Dexiled.UI.EquipmentScreen
{
    /// <summary>
    /// Manages a single category section of currencies (Orbs, Spirits, or Fragments).
    /// Spawns currency prefabs dynamically and updates their displays.
    /// </summary>
    public class CurrencySectionController : MonoBehaviour
    {
        [Header("Section Configuration")]
        [SerializeField] private CurrencyCategory category;
        [SerializeField] private Transform currencyContainer;
        [SerializeField] private GameObject currencyPrefab;

        [Header("Optional: Hide if Empty")]
        [SerializeField] private bool hideWhenEmpty = false;
        [SerializeField] private GameObject sectionRootObject;

        private List<CurrencyDisplayItem> spawnedCurrencyItems = new List<CurrencyDisplayItem>();

        /// <summary>
        /// Initializes the section by spawning currency items based on the category.
        /// </summary>
        /// <param name="currencyDatabase">Reference to the CurrencyDatabase asset</param>
        public void InitializeSection(CurrencyDatabase currencyDatabase)
        {
            if (currencyDatabase == null)
            {
                Debug.LogError($"[CurrencySectionController] {category} section: CurrencyDatabase is null!");
                return;
            }

            if (currencyContainer == null)
            {
                Debug.LogError($"[CurrencySectionController] {category} section: Currency Container not assigned!");
                return;
            }

            if (currencyPrefab == null)
            {
                Debug.LogError($"[CurrencySectionController] {category} section: Currency Prefab not assigned!");
                return;
            }

            // Clear any existing items
            ClearSection();

            // Get currencies for this category
            List<CurrencyData> categoryCurrencies = GetCurrenciesForCategory(currencyDatabase);

            if (categoryCurrencies.Count == 0)
            {
                Debug.LogWarning($"[CurrencySectionController] No currencies found for category: {category}");
                
                if (hideWhenEmpty && sectionRootObject != null)
                {
                    sectionRootObject.SetActive(false);
                }
                return;
            }

            // Show section if it was hidden
            if (sectionRootObject != null)
            {
                sectionRootObject.SetActive(true);
            }

            // Spawn currency items
            foreach (CurrencyData currencyData in categoryCurrencies)
            {
                SpawnCurrencyItem(currencyData);
            }

            Debug.Log($"[CurrencySectionController] {category} section initialized with {spawnedCurrencyItems.Count} items");
        }

        /// <summary>
        /// Spawns a single currency item prefab and initializes it.
        /// </summary>
        private void SpawnCurrencyItem(CurrencyData currencyData)
        {
            GameObject itemObj = Instantiate(currencyPrefab, currencyContainer);
            CurrencyDisplayItem displayItem = itemObj.GetComponent<CurrencyDisplayItem>();

            if (displayItem == null)
            {
                Debug.LogError($"[CurrencySectionController] Spawned prefab is missing CurrencyDisplayItem component!");
                Destroy(itemObj);
                return;
            }

            displayItem.Initialize(currencyData);
            spawnedCurrencyItems.Add(displayItem);
        }

        /// <summary>
        /// Updates all currency displays in this section (call when quantities change).
        /// </summary>
        public void UpdateAllDisplays()
        {
            foreach (CurrencyDisplayItem item in spawnedCurrencyItems)
            {
                if (item != null)
                {
                    item.UpdateDisplay();
                }
            }
        }

        /// <summary>
        /// Updates a specific currency's quantity display.
        /// </summary>
        /// <param name="currencyType">The type of currency to update</param>
        /// <param name="newQuantity">The new quantity value</param>
        public void UpdateCurrencyQuantity(CurrencyType currencyType, int newQuantity)
        {
            CurrencyDisplayItem item = spawnedCurrencyItems.Find(c => 
                c.GetCurrencyData() != null && c.GetCurrencyData().currencyType == currencyType);

            if (item != null)
            {
                item.UpdateQuantity(newQuantity);
                Debug.Log($"[CurrencySectionController] Updated {category} {currencyType} to {newQuantity}");
            }
            else
            {
                // Debug: Log what currencies we have
                string availableTypes = string.Join(", ", spawnedCurrencyItems
                    .Where(c => c.GetCurrencyData() != null)
                    .Select(c => c.GetCurrencyData().currencyType.ToString()));
                Debug.LogWarning($"[CurrencySectionController] {category} section: Currency {currencyType} not found! Available: {availableTypes}");
            }
        }

        /// <summary>
        /// Clears all spawned currency items from the section.
        /// </summary>
        public void ClearSection()
        {
            foreach (CurrencyDisplayItem item in spawnedCurrencyItems)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }

            spawnedCurrencyItems.Clear();
        }

        /// <summary>
        /// Filters currencies from the database based on the section's category.
        /// </summary>
        private List<CurrencyData> GetCurrenciesForCategory(CurrencyDatabase database)
        {
            List<CurrencyData> result = new List<CurrencyData>();

            foreach (CurrencyData currency in database.currencies)
            {
                if (IsCurrencyInCategory(currency.currencyType))
                {
                    result.Add(currency);
                }
            }

            return result;
        }

        /// <summary>
        /// Determines if a currency type belongs to this section's category.
        /// </summary>
        private bool IsCurrencyInCategory(CurrencyType type)
        {
            int typeIndex = (int)type;

            switch (category)
            {
                case CurrencyCategory.Orbs:
                    // Orbs (0-8) = Primary crafting orbs
                    return typeIndex >= 0 && typeIndex <= 8;

                case CurrencyCategory.Spirits:
                    // Spirits (9-17) = Element/stat specific currencies
                    return typeIndex >= 9 && typeIndex <= 17;

                case CurrencyCategory.Fragments:
                    // Seals (18-24) + Fragments (25-27) = Special currencies and fragments
                    return typeIndex >= 18 && typeIndex <= 27;

                default:
                    return false;
            }
        }

        #region Editor Helpers
#if UNITY_EDITOR
        [ContextMenu("Auto-Assign Container")]
        private void AutoAssignContainer()
        {
            if (currencyContainer == null)
            {
                // Try to find a child named "Container" or "CurrencyContainer"
                Transform found = transform.Find("Container");
                if (found == null) found = transform.Find("CurrencyContainer");
                
                if (found != null)
                {
                    currencyContainer = found;
                    Debug.Log($"[CurrencySectionController] Auto-assigned container: {found.name}");
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
        }
#endif
        #endregion
    }

    /// <summary>
    /// Enum defining the three currency categories.
    /// </summary>
    public enum CurrencyCategory
    {
        Orbs,      // Crafting orbs (0-8)
        Spirits,   // Elemental/stat spirits (9-17)
        Fragments  // Seals (18-24) + Mysterious fragments (25-27)
    }
}


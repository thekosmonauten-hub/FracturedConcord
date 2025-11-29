using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Dexiled.Data.Items;

namespace Dexiled.UI.EquipmentScreen
{
    /// <summary>
    /// Controls a single currency display item (prefab instance).
    /// Updates the icon sprite and quantity text based on CurrencyData.
    /// </summary>
    public class CurrencyDisplayItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        [SerializeField] private Image currencyIcon;
        [SerializeField] private TextMeshProUGUI currencyCountText;
        [SerializeField] private Image backgroundImage;

        private CurrencyData currentCurrency;

        /// <summary>
        /// Initializes the currency display with data from CurrencyDatabase.
        /// </summary>
        /// <param name="currencyData">The currency data to display</param>
        public void Initialize(CurrencyData currencyData)
        {
            if (currencyData == null)
            {
                Debug.LogError("[CurrencyDisplayItem] Cannot initialize with null currency data!");
                return;
            }

            currentCurrency = currencyData;
            
            // Ensure the prefab has a proper size for the Layout Group
            UnityEngine.UI.LayoutElement layoutElement = GetComponent<UnityEngine.UI.LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            }
            layoutElement.preferredWidth = 60;
            layoutElement.preferredHeight = 60;
            
            UpdateDisplay();
        }

        /// <summary>
        /// Updates the visual display (icon, count, colors) based on current currency data.
        /// </summary>
        public void UpdateDisplay()
        {
            if (currentCurrency == null)
            {
                Debug.LogWarning("[CurrencyDisplayItem] No currency data to display!");
                return;
            }

            // Update icon sprite
            if (currencyIcon != null && currentCurrency.currencySprite != null)
            {
                currencyIcon.sprite = currentCurrency.currencySprite;
                currencyIcon.enabled = true;
            }
            else if (currencyIcon != null)
            {
                // Hide icon if no sprite available
                currencyIcon.enabled = false;
            }

            // Update count text
            if (currencyCountText != null)
            {
                currencyCountText.text = currentCurrency.quantity.ToString();
            }
        }

        /// <summary>
        /// Updates only the quantity text (for performance when only count changes).
        /// </summary>
        /// <param name="newQuantity">The new quantity to display</param>
        public void UpdateQuantity(int newQuantity)
        {
            if (currentCurrency != null)
            {
                currentCurrency.quantity = newQuantity;
            }

            if (currencyCountText != null)
            {
                currencyCountText.text = newQuantity.ToString();
            }
        }

        /// <summary>
        /// Gets the current currency data this item is displaying.
        /// </summary>
        public CurrencyData GetCurrencyData()
        {
            return currentCurrency;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentCurrency == null || ItemTooltipManager.Instance == null)
                return;

            ItemTooltipManager.Instance.ShowCurrencyTooltipForPointer(currentCurrency, eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (ItemTooltipManager.Instance != null)
            {
                ItemTooltipManager.Instance.HideTooltip();
            }
        }

        private void OnDisable()
        {
            if (ItemTooltipManager.Instance != null)
            {
                ItemTooltipManager.Instance.HideTooltip();
            }
        }

        #region Editor Setup Helper
#if UNITY_EDITOR
        /// <summary>
        /// Auto-assigns references in the Unity Editor (call from Inspector context menu).
        /// </summary>
        [ContextMenu("Auto-Assign References")]
        private void AutoAssignReferences()
        {
            if (currencyIcon == null)
            {
                Transform iconTransform = transform.Find("Image");
                if (iconTransform != null)
                {
                    currencyIcon = iconTransform.GetComponent<Image>();
                    Debug.Log("[CurrencyDisplayItem] Auto-assigned currencyIcon");
                }
            }

            if (currencyCountText == null)
            {
                Transform countTransform = transform.Find("CurrencyCount");
                if (countTransform != null)
                {
                    currencyCountText = countTransform.GetComponent<TextMeshProUGUI>();
                    Debug.Log("[CurrencyDisplayItem] Auto-assigned currencyCountText");
                }
            }

            if (backgroundImage == null)
            {
                Transform bgTransform = transform.Find("Background");
                if (bgTransform != null)
                {
                    backgroundImage = bgTransform.GetComponent<Image>();
                    Debug.Log("[CurrencyDisplayItem] Auto-assigned backgroundImage");
                }
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        #endregion
    }
}


using UnityEngine;
using TMPro;
using Dexiled.Data.Items;

namespace Dexiled.UI.EquipmentScreen
{
    /// <summary>
    /// Displays the player's Inscription Seal currency quantity
    /// Auto-updates when currency changes
    /// </summary>
    public class InscriptionSealDisplay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI quantityText;
        
        [Header("Auto-Find")]
        [Tooltip("If true, will search for TextMeshProUGUI in children")]
        [SerializeField] private bool autoFindText = true;
        
        [Header("Formatting")]
        [SerializeField] private string format = "{0}";
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color insufficientColor = Color.red;
        
        private int currentQuantity = 0;
        private int requiredQuantity = 0;
        
        void Awake()
        {
            // Auto-find text component if not assigned
            if (quantityText == null && autoFindText)
            {
                quantityText = GetComponentInChildren<TextMeshProUGUI>();
                
                if (quantityText != null)
                {
                    Debug.Log($"[InscriptionSealDisplay] Auto-assigned TextMeshProUGUI: {quantityText.name}");
                }
                else
                {
                    Debug.LogWarning($"[InscriptionSealDisplay] Could not find TextMeshProUGUI in {gameObject.name}");
                }
            }
        }
        
        void Start()
        {
            UpdateDisplay();
        }
        
        /// <summary>
        /// Update the display with current Inscription Seal quantity
        /// </summary>
        public void UpdateDisplay()
        {
            if (quantityText == null) return;
            
            // Get quantity from LootManager
            int quantity = GetInscriptionSealQuantity();
            currentQuantity = quantity;
            
            // Format and display
            quantityText.text = string.Format(format, quantity);
            
            // Update color based on requirement
            if (requiredQuantity > 0)
            {
                quantityText.color = quantity >= requiredQuantity ? normalColor : insufficientColor;
            }
            else
            {
                quantityText.color = normalColor;
            }
        }
        
        /// <summary>
        /// Update display with a required amount (for validation)
        /// </summary>
        public void UpdateDisplayWithRequirement(int required)
        {
            requiredQuantity = required;
            UpdateDisplay();
        }
        
        /// <summary>
        /// Get Inscription Seal quantity from LootManager
        /// </summary>
        private int GetInscriptionSealQuantity()
        {
            if (LootManager.Instance != null)
            {
                return LootManager.Instance.GetCurrencyAmount(CurrencyType.InscriptionSeal);
            }
            
            Debug.LogWarning("[InscriptionSealDisplay] LootManager not found, returning 0");
            return 0;
        }
        
        /// <summary>
        /// Check if player has enough Inscription Seals
        /// </summary>
        public bool HasEnoughCurrency(int amount)
        {
            return currentQuantity >= amount;
        }
        
        /// <summary>
        /// Get current displayed quantity
        /// </summary>
        public int GetCurrentQuantity()
        {
            return currentQuantity;
        }
        
        #region Editor Helpers
        
#if UNITY_EDITOR
        [ContextMenu("Update Display")]
        private void EditorUpdateDisplay()
        {
            UpdateDisplay();
        }
        
        [ContextMenu("Test with 10 Seals")]
        private void TestWith10Seals()
        {
            if (quantityText != null)
            {
                quantityText.text = string.Format(format, 10);
            }
        }
#endif
        
        #endregion
    }
}


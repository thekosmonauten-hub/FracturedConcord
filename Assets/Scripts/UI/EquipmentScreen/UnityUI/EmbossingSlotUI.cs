using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

namespace Dexiled.UI.EquipmentScreen
{
    /// <summary>
    /// Individual embossing slot in the grid
    /// Displays embossing icon, name, rarity, and handles click interaction
    /// </summary>
    public class EmbossingSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Elements")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image rarityBorderImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI manaCostText;
        [SerializeField] private Button button;
        
        [Header("Visual Settings")]
        [SerializeField] private bool autoSetup = true;
        [SerializeField] private Color selectedColor = new Color(1f, 1f, 0.5f, 1f);
        
        private EmbossingEffect embossing;
        private bool isSelected = false;
        private Color originalBackgroundColor;
        
        public Action<EmbossingEffect> OnSlotClicked;
        public Action<EmbossingEffect> OnSlotClickedForConfirmation;
        
        private EmbossingTooltip tooltipSystem;
        
        void Awake()
        {
            // Auto-setup references if enabled
            if (autoSetup)
            {
                AutoSetupReferences();
            }
            
            // Setup button listener
            if (button == null)
            {
                button = GetComponent<Button>();
            }
            
            if (button != null)
            {
                button.onClick.AddListener(OnClick);
            }
            
            if (backgroundImage != null)
            {
                originalBackgroundColor = backgroundImage.color;
            }
        }
        
        void Start()
        {
            // Find tooltip system
            tooltipSystem = FindFirstObjectByType<EmbossingTooltip>();
            
            if (tooltipSystem == null)
            {
                Debug.LogWarning("[EmbossingSlotUI] EmbossingTooltip system not found in scene!");
            }
        }
        
        /// <summary>
        /// Auto-find UI elements in children
        /// </summary>
        void AutoSetupReferences()
        {
            if (backgroundImage == null)
            {
                backgroundImage = GetComponent<Image>();
            }
            
            if (iconImage == null)
            {
                // Look for child named "Icon"
                Transform iconTransform = transform.Find("Icon");
                if (iconTransform != null)
                {
                    iconImage = iconTransform.GetComponent<Image>();
                }
            }
            
            if (rarityBorderImage == null)
            {
                // Look for child named "RarityBorder" or "Border"
                Transform borderTransform = transform.Find("RarityBorder");
                if (borderTransform == null)
                {
                    borderTransform = transform.Find("Border");
                }
                if (borderTransform != null)
                {
                    rarityBorderImage = borderTransform.GetComponent<Image>();
                }
            }
            
            if (nameText == null)
            {
                // Look for TMP text component
                nameText = GetComponentInChildren<TextMeshProUGUI>();
            }
            
            if (manaCostText == null)
            {
                // Look for second TMP text (if exists)
                TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 1)
                {
                    manaCostText = texts[1];
                }
            }
        }
        
        /// <summary>
        /// Set the embossing data for this slot
        /// </summary>
        public void SetEmbossing(EmbossingEffect newEmbossing)
        {
            embossing = newEmbossing;
            UpdateDisplay();
        }
        
        /// <summary>
        /// Update visual display based on embossing data
        /// </summary>
        void UpdateDisplay()
        {
            if (embossing == null) return;
            
            // Set name
            if (nameText != null)
            {
                nameText.text = embossing.embossingName;
                nameText.color = embossing.GetRarityColor();
            }
            
            // Set icon
            if (iconImage != null)
            {
                if (embossing.embossingIcon != null)
                {
                    iconImage.sprite = embossing.embossingIcon;
                    iconImage.enabled = true;
                }
                else
                {
                    iconImage.enabled = false;
                }
            }
            
            // Set background color based on category
            if (backgroundImage != null)
            {
                Color categoryColor = embossing.GetTypeColor();
                backgroundImage.color = categoryColor * 0.3f; // Darken for background
                originalBackgroundColor = backgroundImage.color;
            }
            
            // Set rarity border
            if (rarityBorderImage != null)
            {
                rarityBorderImage.color = embossing.GetRarityColor();
            }
            
            // Set mana cost
            if (manaCostText != null)
            {
                manaCostText.text = $"+{(embossing.manaCostMultiplier * 100):F0}%";
            }
            
            // Check requirements and adjust visuals
            UpdateRequirementDisplay();
        }
        
        /// <summary>
        /// Update display based on character requirements
        /// </summary>
        void UpdateRequirementDisplay()
        {
            if (embossing == null) return;
            
            // Check if character meets requirements
            bool meetsRequirements = true;
            
            if (CharacterManager.Instance != null && CharacterManager.Instance.currentCharacter != null)
            {
                Character character = CharacterManager.Instance.currentCharacter;
                meetsRequirements = embossing.MeetsRequirements(character);
            }
            
            // Grey out if requirements not met
            if (!meetsRequirements)
            {
                if (backgroundImage != null)
                {
                    Color greyedOut = backgroundImage.color;
                    greyedOut.a = 0.5f;
                    backgroundImage.color = greyedOut;
                }
                
                if (iconImage != null)
                {
                    Color iconColor = iconImage.color;
                    iconColor.a = 0.5f;
                    iconImage.color = iconColor;
                }
                
                if (nameText != null)
                {
                    Color textColor = nameText.color;
                    textColor.a = 0.5f;
                    nameText.color = textColor;
                }
            }
        }
        
        /// <summary>
        /// Handle pointer enter event (hover)
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (embossing != null && tooltipSystem != null)
            {
                tooltipSystem.ShowTooltip(embossing);
            }
        }
        
        /// <summary>
        /// Handle pointer exit event (stop hover)
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (tooltipSystem != null)
            {
                tooltipSystem.HideTooltip();
            }
        }
        
        /// <summary>
        /// Handle click event
        /// </summary>
        void OnClick()
        {
            if (embossing == null) return;
            
            // Hide tooltip when clicked
            if (tooltipSystem != null)
            {
                tooltipSystem.ForceHideTooltip();
            }
            
            SetSelected(true);
            
            // Trigger both callbacks (for backward compatibility and new confirmation system)
            OnSlotClicked?.Invoke(embossing);
            OnSlotClickedForConfirmation?.Invoke(embossing);
            
            Debug.Log($"[EmbossingSlotUI] Clicked: {embossing.embossingName}");
        }
        
        /// <summary>
        /// Set selection state
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            
            if (backgroundImage != null)
            {
                if (isSelected)
                {
                    backgroundImage.color = selectedColor;
                }
                else
                {
                    backgroundImage.color = originalBackgroundColor;
                }
            }
        }
        
        /// <summary>
        /// Get the embossing data for this slot
        /// </summary>
        public EmbossingEffect GetEmbossing()
        {
            return embossing;
        }
        
        /// <summary>
        /// Check if this slot is selected
        /// </summary>
        public bool IsSelected()
        {
            return isSelected;
        }
        
        #region Editor Helpers
        
#if UNITY_EDITOR
        [ContextMenu("Test Display")]
        void TestDisplay()
        {
            UpdateDisplay();
        }
#endif
        
        #endregion
    }
}


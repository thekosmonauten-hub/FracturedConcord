using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

namespace Dexiled.UI.EquipmentScreen
{
    /// <summary>
    /// Tooltip system for displaying embossing information on hover
    /// Shows detailed information about embossings including requirements, effects, and mana cost
    /// </summary>
    public class EmbossingTooltip : MonoBehaviour
    {
        [Header("Tooltip Settings")]
        [SerializeField] private GameObject tooltipPrefab;
        [SerializeField] private Canvas tooltipCanvas;
        [SerializeField] private bool followMouse = true;
        [SerializeField] private Vector2 mouseOffset = new Vector2(10, 10);
        [SerializeField] private float showDelay = 0.3f;
        [SerializeField] private float hideDelay = 0.1f;
        [SerializeField] private bool debugMode = false;

        [Header("Animation")]
        [SerializeField] private bool useAnimations = true;
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private float fadeOutDuration = 0.15f;
        [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Header("Positioning")]
        [SerializeField] private bool keepOnScreen = true;
        [SerializeField] private Vector2 screenMargin = new Vector2(20, 20);

        [Header("Tooltip Size & Layout")]
        [SerializeField] private Vector2 tooltipSize = new Vector2(350, 400);
        [SerializeField] private bool useTwoColumnLayout = true;
        [SerializeField] private bool autoHeight = true;
        [Tooltip("Maximum height when auto-height is enabled")]
        [SerializeField] private float maxHeight = 600f;
        
        [Header("Background Customization")]
        [SerializeField] private Sprite backgroundSprite;
        [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        [SerializeField] private Color borderColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        [SerializeField] private Vector2 borderSize = new Vector2(2, 2);
        
        [Header("Padding & Spacing")]
        [SerializeField] private int paddingLeft = 15;
        [SerializeField] private int paddingRight = 15;
        [SerializeField] private int paddingTop = 15;
        [SerializeField] private int paddingBottom = 15;
        [SerializeField] private float sectionSpacing = 8f;
        [SerializeField] private float columnSpacing = 20f;

        // Runtime components
        private GameObject currentTooltip;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI descriptionText;
        private TextMeshProUGUI categoryText;
        private TextMeshProUGUI rarityText;
        private TextMeshProUGUI elementText;
        private TextMeshProUGUI requirementsText;
        private TextMeshProUGUI effectText;
        private TextMeshProUGUI manaCostText;
        private Image iconImage;
        private Image backgroundImage;
        private CanvasGroup tooltipCanvasGroup;
        private RectTransform tooltipRectTransform;

        // State
        private EmbossingEffect currentEmbossing;
        private Coroutine showCoroutine;
        private Coroutine hideCoroutine;
        private bool isTooltipVisible = false;

        void Start()
        {
            SetupTooltipSystem();
        }

        void Update()
        {
            if (isTooltipVisible && followMouse && currentTooltip != null)
            {
                UpdateTooltipPosition();
            }
        }

        /// <summary>
        /// Set up the tooltip system
        /// </summary>
        private void SetupTooltipSystem()
        {
            Debug.Log("[EmbossingTooltip] Setting up tooltip system...");
            
            // Set up canvas if not assigned
            if (tooltipCanvas == null)
            {
                tooltipCanvas = FindFirstObjectByType<Canvas>();
                if (tooltipCanvas == null)
                {
                    Debug.LogWarning("[EmbossingTooltip] No Canvas found. Creating one...");
                    CreateTooltipCanvas();
                }
                else
                {
                    Debug.Log($"[EmbossingTooltip] Found canvas: {tooltipCanvas.name}");
                }
            }

            // Create tooltip from prefab or procedurally
            if (tooltipPrefab != null)
            {
                CreateTooltipFromPrefab();
            }
            else
            {
                Debug.Log("[EmbossingTooltip] No prefab assigned, creating procedurally");
                CreateTooltipProcedurally();
            }

            Debug.Log($"[EmbossingTooltip] Tooltip system setup complete. currentTooltip: {(currentTooltip != null ? currentTooltip.name : "NULL")}");
        }

        /// <summary>
        /// Create a tooltip canvas
        /// </summary>
        private void CreateTooltipCanvas()
        {
            GameObject canvasGO = new GameObject("EmbossingTooltipCanvas");
            tooltipCanvas = canvasGO.AddComponent<Canvas>();
            tooltipCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            tooltipCanvas.sortingOrder = 1000; // High sorting order to appear on top

            // Add CanvasScaler
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Add GraphicRaycaster
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        /// <summary>
        /// Create tooltip from prefab
        /// </summary>
        private void CreateTooltipFromPrefab()
        {
            if (tooltipPrefab == null) return;

            // Instantiate the tooltip prefab
            currentTooltip = Instantiate(tooltipPrefab, tooltipCanvas.transform);
            currentTooltip.name = "EmbossingTooltip";

            // Find components by name
            FindTooltipComponents();

            // Get RectTransform and CanvasGroup
            tooltipRectTransform = currentTooltip.GetComponent<RectTransform>();
            tooltipCanvasGroup = currentTooltip.GetComponent<CanvasGroup>();

            if (tooltipCanvasGroup == null)
            {
                tooltipCanvasGroup = currentTooltip.AddComponent<CanvasGroup>();
            }

            // Initially hide the tooltip
            currentTooltip.SetActive(false);

            Debug.Log("[EmbossingTooltip] Created tooltip from prefab");
        }

        /// <summary>
        /// Create tooltip procedurally if no prefab is assigned
        /// </summary>
        private void CreateTooltipProcedurally()
        {
            // Create tooltip root
            currentTooltip = new GameObject("EmbossingTooltip");
            currentTooltip.transform.SetParent(tooltipCanvas.transform, false);

            tooltipRectTransform = currentTooltip.AddComponent<RectTransform>();
            tooltipRectTransform.sizeDelta = tooltipSize;

            tooltipCanvasGroup = currentTooltip.AddComponent<CanvasGroup>();

            // Add background
            backgroundImage = currentTooltip.AddComponent<Image>();
            backgroundImage.color = backgroundColor;
            
            // Set background sprite if provided
            if (backgroundSprite != null)
            {
                backgroundImage.sprite = backgroundSprite;
                backgroundImage.type = Image.Type.Sliced; // Use sliced for 9-slice sprites
                Debug.Log($"[EmbossingTooltip] Applied background sprite: {backgroundSprite.name}");
            }
            
            // Add outline
            Outline outline = currentTooltip.AddComponent<Outline>();
            outline.effectColor = borderColor;
            outline.effectDistance = borderSize;

            // Create vertical layout
            VerticalLayoutGroup layout = currentTooltip.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(paddingLeft, paddingRight, paddingTop, paddingBottom);
            layout.spacing = sectionSpacing;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            
            // Add ContentSizeFitter if auto-height is enabled
            if (autoHeight)
            {
                ContentSizeFitter fitter = currentTooltip.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }

            // Create UI elements
            CreateHeaderSection();
            CreateDescriptionSection();
            
            if (useTwoColumnLayout)
            {
                CreateTwoColumnContent();
            }
            else
            {
                CreateInfoSection();
                CreateRequirementsSection();
                CreateEffectSection();
                CreateManaCostSection();
            }

            // Initially hide the tooltip
            currentTooltip.SetActive(false);

            Debug.Log("[EmbossingTooltip] Created tooltip procedurally");
        }

        /// <summary>
        /// Create header section with icon and title
        /// </summary>
        private void CreateHeaderSection()
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(currentTooltip.transform, false);

            HorizontalLayoutGroup headerLayout = header.AddComponent<HorizontalLayoutGroup>();
            headerLayout.spacing = 10;
            headerLayout.childControlWidth = false;
            headerLayout.childControlHeight = false;

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(header.transform, false);
            iconImage = iconObj.AddComponent<Image>();
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(40, 40);

            // Title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(header.transform, false);
            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.fontSize = 20;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
        }

        /// <summary>
        /// Create description section
        /// </summary>
        private void CreateDescriptionSection()
        {
            GameObject descObj = new GameObject("DescriptionText");
            descObj.transform.SetParent(currentTooltip.transform, false);
            descriptionText = descObj.AddComponent<TextMeshProUGUI>();
            descriptionText.fontSize = 14;
            descriptionText.color = new Color(0.8f, 0.8f, 0.8f);
            descriptionText.alignment = TextAlignmentOptions.Left;
        }

        /// <summary>
        /// Create info section (category, rarity, element)
        /// </summary>
        private void CreateInfoSection()
        {
            GameObject info = new GameObject("InfoSection");
            info.transform.SetParent(currentTooltip.transform, false);

            VerticalLayoutGroup infoLayout = info.AddComponent<VerticalLayoutGroup>();
            infoLayout.spacing = 4;

            categoryText = CreateInfoLabel(info.transform, "CategoryLabel");
            rarityText = CreateInfoLabel(info.transform, "RarityLabel");
            elementText = CreateInfoLabel(info.transform, "ElementLabel");
        }

        /// <summary>
        /// Create requirements section
        /// </summary>
        private void CreateRequirementsSection()
        {
            GameObject req = new GameObject("RequirementsSection");
            req.transform.SetParent(currentTooltip.transform, false);

            VerticalLayoutGroup reqLayout = req.AddComponent<VerticalLayoutGroup>();
            reqLayout.spacing = 4;

            TextMeshProUGUI reqTitle = CreateInfoLabel(req.transform, "RequirementsTitle");
            reqTitle.text = "Requirements:";
            reqTitle.fontStyle = FontStyles.Bold;

            requirementsText = CreateInfoLabel(req.transform, "RequirementsText");
        }

        /// <summary>
        /// Create effect section
        /// </summary>
        private void CreateEffectSection()
        {
            GameObject eff = new GameObject("EffectSection");
            eff.transform.SetParent(currentTooltip.transform, false);

            VerticalLayoutGroup effLayout = eff.AddComponent<VerticalLayoutGroup>();
            effLayout.spacing = 4;

            TextMeshProUGUI effTitle = CreateInfoLabel(eff.transform, "EffectTitle");
            effTitle.text = "Effect:";
            effTitle.fontStyle = FontStyles.Bold;

            effectText = CreateInfoLabel(eff.transform, "EffectText");
            effectText.color = new Color(0.5f, 1f, 0.5f); // Light green
        }

        /// <summary>
        /// Create mana cost section
        /// </summary>
        private void CreateManaCostSection()
        {
            GameObject mana = new GameObject("ManaCostSection");
            mana.transform.SetParent(currentTooltip.transform, false);

            VerticalLayoutGroup manaLayout = mana.AddComponent<VerticalLayoutGroup>();
            manaLayout.spacing = 4;

            TextMeshProUGUI manaTitle = CreateInfoLabel(mana.transform, "ManaCostTitle");
            manaTitle.text = "Mana Cost:";
            manaTitle.fontStyle = FontStyles.Bold;

            manaCostText = CreateInfoLabel(mana.transform, "ManaCostText");
            manaCostText.color = new Color(1f, 0.5f, 0.5f); // Light red
        }

        /// <summary>
        /// Helper to create info labels
        /// </summary>
        private TextMeshProUGUI CreateInfoLabel(Transform parent, string name)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
            text.fontSize = 14;
            text.alignment = TextAlignmentOptions.Left;
            return text;
        }

        /// <summary>
        /// Find tooltip components from prefab by name
        /// </summary>
        private void FindTooltipComponents()
        {
            if (currentTooltip == null) return;

            TextMeshProUGUI[] textComponents = currentTooltip.GetComponentsInChildren<TextMeshProUGUI>(true);

            foreach (TextMeshProUGUI text in textComponents)
            {
                string textName = text.name.ToLower();

                if (textName.Contains("title"))
                    titleText = text;
                else if (textName.Contains("description") || textName.Contains("desc"))
                    descriptionText = text;
                else if (textName.Contains("category"))
                    categoryText = text;
                else if (textName.Contains("rarity"))
                    rarityText = text;
                else if (textName.Contains("element"))
                    elementText = text;
                else if (textName.Contains("requirement"))
                    requirementsText = text;
                else if (textName.Contains("effect") && !textName.Contains("title"))
                    effectText = text;
                else if (textName.Contains("manacost") || textName.Contains("mana_cost"))
                    manaCostText = text;
            }

            Image[] images = currentTooltip.GetComponentsInChildren<Image>(true);
            foreach (Image img in images)
            {
                if (img.name.ToLower().Contains("icon") && iconImage == null)
                {
                    iconImage = img;
                }
                else if (img.gameObject == currentTooltip)
                {
                    backgroundImage = img;
                }
            }

            if (debugMode)
            {
                Debug.Log($"[EmbossingTooltip] Found components - Title: {titleText != null}, Desc: {descriptionText != null}, Icon: {iconImage != null}");
            }
        }

        /// <summary>
        /// Show tooltip for an embossing effect
        /// </summary>
        public void ShowTooltip(EmbossingEffect embossing)
        {
            Debug.Log($"[EmbossingTooltip] ShowTooltip called for: {(embossing != null ? embossing.embossingName : "NULL")}");
            Debug.Log($"[EmbossingTooltip] currentTooltip is null? {(currentTooltip == null)}");
            
            if (embossing == null || currentTooltip == null)
            {
                Debug.LogWarning($"[EmbossingTooltip] Cannot show tooltip - embossing null: {embossing == null}, currentTooltip null: {currentTooltip == null}");
                return;
            }

            // Stop any existing coroutines
            if (showCoroutine != null) StopCoroutine(showCoroutine);
            if (hideCoroutine != null) StopCoroutine(hideCoroutine);

            currentEmbossing = embossing;

            // Start show coroutine
            Debug.Log($"[EmbossingTooltip] Starting show coroutine with {showDelay}s delay");
            showCoroutine = StartCoroutine(ShowTooltipCoroutine(embossing));
        }

        /// <summary>
        /// Hide the tooltip
        /// </summary>
        public void HideTooltip()
        {
            if (currentTooltip == null) return;

            // Stop any existing coroutines
            if (showCoroutine != null) StopCoroutine(showCoroutine);
            if (hideCoroutine != null) StopCoroutine(hideCoroutine);

            // Start hide coroutine
            hideCoroutine = StartCoroutine(HideTooltipCoroutine());
        }

        /// <summary>
        /// Show tooltip coroutine with delay
        /// </summary>
        private IEnumerator ShowTooltipCoroutine(EmbossingEffect embossing)
        {
            Debug.Log($"[EmbossingTooltip] ShowTooltipCoroutine started, waiting {showDelay} seconds...");
            
            // Wait for show delay
            yield return new WaitForSeconds(showDelay);

            Debug.Log($"[EmbossingTooltip] Delay complete. Still showing same embossing? {currentEmbossing == embossing}");
            
            // Check if we're still showing the same embossing
            if (currentEmbossing != embossing)
            {
                Debug.Log("[EmbossingTooltip] Embossing changed during delay, aborting");
                yield break;
            }

            // Update tooltip content
            UpdateTooltipContent(embossing);

            // Show the tooltip
            Debug.Log($"[EmbossingTooltip] Activating tooltip GameObject: {currentTooltip.name}");
            currentTooltip.SetActive(true);
            isTooltipVisible = true;

            // Force layout rebuild for auto-height
            if (autoHeight)
            {
                Canvas.ForceUpdateCanvases();
                
                // Clamp height if needed
                if (tooltipRectTransform != null && tooltipRectTransform.sizeDelta.y > maxHeight)
                {
                    tooltipRectTransform.sizeDelta = new Vector2(tooltipRectTransform.sizeDelta.x, maxHeight);
                }
            }

            // Position the tooltip
            UpdateTooltipPosition();

            // Animate in
            if (useAnimations)
            {
                yield return StartCoroutine(AnimateTooltip(true));
            }

            Debug.Log($"[EmbossingTooltip] Tooltip now visible for {embossing.embossingName}");
        }

        /// <summary>
        /// Hide tooltip coroutine with delay
        /// </summary>
        private IEnumerator HideTooltipCoroutine()
        {
            // Wait for hide delay
            yield return new WaitForSeconds(hideDelay);

            // Animate out
            if (useAnimations && isTooltipVisible)
            {
                yield return StartCoroutine(AnimateTooltip(false));
            }

            // Hide the tooltip
            if (currentTooltip != null)
            {
                currentTooltip.SetActive(false);
            }

            isTooltipVisible = false;
            currentEmbossing = null;

            if (debugMode)
            {
                Debug.Log("[EmbossingTooltip] Hiding tooltip");
            }
        }

        /// <summary>
        /// Update tooltip content with embossing data
        /// </summary>
        private void UpdateTooltipContent(EmbossingEffect embossing)
        {
            if (embossing == null) return;

            Character character = CharacterManager.Instance?.currentCharacter;

            // Update title
            if (titleText != null)
            {
                titleText.text = embossing.embossingName;
                titleText.color = embossing.GetRarityColor();
            }

            // Update icon
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

            // Update description
            if (descriptionText != null)
            {
                descriptionText.text = embossing.description;
            }

            // Update category
            if (categoryText != null)
            {
                categoryText.text = $"<b>Category:</b> {embossing.category}";
                categoryText.color = embossing.GetTypeColor();
            }

            // Update rarity
            if (rarityText != null)
            {
                rarityText.text = $"<b>Rarity:</b> {embossing.rarity}";
                rarityText.color = embossing.GetRarityColor();
            }

            // Update element
            if (elementText != null)
            {
                elementText.text = $"<b>Element:</b> {embossing.elementType}";
            }

            // Update requirements
            if (requirementsText != null)
            {
                if (character != null)
                {
                    requirementsText.text = embossing.GetRequirementsTextColored(character);
                }
                else
                {
                    requirementsText.text = embossing.GetRequirementsText();
                }
            }

            // Update effect
            if (effectText != null)
            {
                effectText.text = embossing.GetEffectDescription();
            }

            // Update mana cost
            if (manaCostText != null)
            {
                string costText = $"+{(embossing.manaCostMultiplier * 100):F0}% mana cost";
                if (embossing.flatManaCostIncrease > 0)
                {
                    costText += $" +{embossing.flatManaCostIncrease} flat";
                }
                manaCostText.text = costText;
            }
        }

        /// <summary>
        /// Update tooltip position with smart positioning
        /// </summary>
        private void UpdateTooltipPosition()
        {
            if (currentTooltip == null || tooltipRectTransform == null) return;

            Vector2 position;
            Vector2 tooltipSize = tooltipRectTransform.sizeDelta;
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            if (followMouse)
            {
                // Follow mouse position using new Input System
                if (Mouse.current != null)
                {
                    Vector2 mousePos = Mouse.current.position.ReadValue();

                    // Smart positioning: try to place tooltip to the right of mouse
                    position = mousePos + new Vector2(mouseOffset.x, -tooltipSize.y / 2);

                    // If tooltip would go off screen at right, place it to the left
                    if (position.x + tooltipSize.x > screenSize.x - screenMargin.x)
                    {
                        position.x = mousePos.x - tooltipSize.x - mouseOffset.x;
                    }

                    // If tooltip would go off screen at bottom, adjust up
                    if (position.y < screenMargin.y)
                    {
                        position.y = screenMargin.y;
                    }

                    // If tooltip would go off screen at top, adjust down
                    if (position.y + tooltipSize.y > screenSize.y - screenMargin.y)
                    {
                        position.y = screenSize.y - tooltipSize.y - screenMargin.y;
                    }
                }
                else
                {
                    // Fallback to screen center if mouse is not available
                    position = new Vector2(Screen.width / 2f, Screen.height / 2f);
                    Debug.LogWarning("[EmbossingTooltip] Mouse.current is null, using screen center as fallback");
                }
            }
            else
            {
                return;
            }

            // Final screen boundary check
            if (keepOnScreen)
            {
                // Adjust X position
                if (position.x + tooltipSize.x > screenSize.x - screenMargin.x)
                {
                    position.x = screenSize.x - tooltipSize.x - screenMargin.x;
                }
                if (position.x < screenMargin.x)
                {
                    position.x = screenMargin.x;
                }

                // Adjust Y position
                if (position.y + tooltipSize.y > screenSize.y - screenMargin.y)
                {
                    position.y = screenSize.y - tooltipSize.y - screenMargin.y;
                }
                if (position.y < screenMargin.y)
                {
                    position.y = screenMargin.y;
                }
            }

            tooltipRectTransform.position = position;
        }

        /// <summary>
        /// Animate tooltip fade in/out
        /// </summary>
        private IEnumerator AnimateTooltip(bool fadeIn)
        {
            if (tooltipCanvasGroup == null) yield break;

            float duration = fadeIn ? fadeInDuration : fadeOutDuration;
            AnimationCurve curve = fadeIn ? fadeInCurve : fadeOutCurve;
            float startAlpha = fadeIn ? 0f : 1f;
            float endAlpha = fadeIn ? 1f : 0f;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float curveValue = curve.Evaluate(t);
                tooltipCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);
                yield return null;
            }

            tooltipCanvasGroup.alpha = endAlpha;
        }

        /// <summary>
        /// Create two-column layout for better space usage
        /// </summary>
        private void CreateTwoColumnContent()
        {
            // Create horizontal container for two columns
            GameObject columnsContainer = new GameObject("ColumnsContainer");
            columnsContainer.transform.SetParent(currentTooltip.transform, false);
            
            HorizontalLayoutGroup columnsLayout = columnsContainer.AddComponent<HorizontalLayoutGroup>();
            columnsLayout.spacing = columnSpacing;
            columnsLayout.childControlWidth = true;
            columnsLayout.childControlHeight = false;
            columnsLayout.childForceExpandWidth = true;
            
            // Left Column
            GameObject leftColumn = new GameObject("LeftColumn");
            leftColumn.transform.SetParent(columnsContainer.transform, false);
            
            VerticalLayoutGroup leftLayout = leftColumn.AddComponent<VerticalLayoutGroup>();
            leftLayout.spacing = 4;
            leftLayout.childControlWidth = true;
            leftLayout.childForceExpandWidth = true;
            
            // Right Column
            GameObject rightColumn = new GameObject("RightColumn");
            rightColumn.transform.SetParent(columnsContainer.transform, false);
            
            VerticalLayoutGroup rightLayout = rightColumn.AddComponent<VerticalLayoutGroup>();
            rightLayout.spacing = 4;
            rightLayout.childControlWidth = true;
            rightLayout.childForceExpandWidth = true;
            
            // Left column: Info (Category, Rarity, Element) + Effect
            categoryText = CreateInfoLabel(leftColumn.transform, "CategoryLabel");
            rarityText = CreateInfoLabel(leftColumn.transform, "RarityLabel");
            elementText = CreateInfoLabel(leftColumn.transform, "ElementLabel");
            
            // Add Effect to left column
            GameObject effSection = new GameObject("EffectSection");
            effSection.transform.SetParent(leftColumn.transform, false);
            
            VerticalLayoutGroup effLayout = effSection.AddComponent<VerticalLayoutGroup>();
            effLayout.spacing = 4;
            
            TextMeshProUGUI effTitle = CreateInfoLabel(effSection.transform, "EffectTitle");
            effTitle.text = "Effect:";
            effTitle.fontStyle = FontStyles.Bold;
            
            effectText = CreateInfoLabel(effSection.transform, "EffectText");
            effectText.color = new Color(0.5f, 1f, 0.5f); // Light green
            
            // Right column: Requirements + Mana Cost
            GameObject reqSection = new GameObject("RequirementsSection");
            reqSection.transform.SetParent(rightColumn.transform, false);
            
            VerticalLayoutGroup reqLayout = reqSection.AddComponent<VerticalLayoutGroup>();
            reqLayout.spacing = 4;
            
            TextMeshProUGUI reqTitle = CreateInfoLabel(reqSection.transform, "RequirementsTitle");
            reqTitle.text = "Requirements:";
            reqTitle.fontStyle = FontStyles.Bold;
            
            requirementsText = CreateInfoLabel(reqSection.transform, "RequirementsText");
            
            // Add Mana Cost to right column
            GameObject manaSection = new GameObject("ManaCostSection");
            manaSection.transform.SetParent(rightColumn.transform, false);
            
            VerticalLayoutGroup manaLayout = manaSection.AddComponent<VerticalLayoutGroup>();
            manaLayout.spacing = 4;
            
            TextMeshProUGUI manaTitle = CreateInfoLabel(manaSection.transform, "ManaCostTitle");
            manaTitle.text = "Mana Cost:";
            manaTitle.fontStyle = FontStyles.Bold;
            
            manaCostText = CreateInfoLabel(manaSection.transform, "ManaCostText");
            manaCostText.color = new Color(1f, 0.5f, 0.5f); // Light red
        }
        
        /// <summary>
        /// Force hide tooltip immediately
        /// </summary>
        public void ForceHideTooltip()
        {
            if (showCoroutine != null) StopCoroutine(showCoroutine);
            if (hideCoroutine != null) StopCoroutine(hideCoroutine);

            if (currentTooltip != null)
            {
                currentTooltip.SetActive(false);
            }

            isTooltipVisible = false;
            currentEmbossing = null;
        }
    }
}


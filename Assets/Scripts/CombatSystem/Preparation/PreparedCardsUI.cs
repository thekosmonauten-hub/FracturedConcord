using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// UI component for displaying prepared cards near the player character.
/// Shows cards with turn counter, glow effect, and click-to-unleash interaction.
/// </summary>
public class PreparedCardsUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Prefab for displaying a prepared card (same as hand card prefab)")]
    public GameObject preparedCardPrefab;
    
    [Tooltip("Parent transform where prepared cards will be spawned")]
    public Transform preparedCardsContainer;
    
    [Tooltip("Reference to player character for positioning")]
    public Transform playerCharacterTransform;
    
    [Header("Layout Settings")]
    [Tooltip("Horizontal spacing between prepared cards")]
    public float cardSpacing = 120f;
    
    [Tooltip("Vertical offset from player character")]
    public float verticalOffset = 200f;
    
    [Header("Visual Effects")]
    [Tooltip("Glow color for prepared cards")]
    public Color glowColor = new Color(0.3f, 0.7f, 1f, 1f); // Cyan glow
    
    [Tooltip("Glow intensity multiplier")]
    public float glowIntensity = 2f;
    
    [Tooltip("Pulse speed for glow effect")]
    public float pulseSpeed = 2f;
    
    // Tracking
    private Dictionary<PreparedCard, GameObject> cardVisuals = new Dictionary<PreparedCard, GameObject>();
    private List<PreparedCard> preparedCardsList = new List<PreparedCard>();
    
    private void Start()
    {
        // Auto-find references if not assigned
        if (preparedCardsContainer == null)
        {
            preparedCardsContainer = transform;
        }
        
        if (playerCharacterTransform == null)
        {
            // Try to find player display
            var playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
            if (playerDisplay != null)
            {
                playerCharacterTransform = playerDisplay.transform;
            }
        }
        
        Debug.Log($"<color=cyan>[PreparedCardsUI] Initialized at {transform.position}</color>");
    }
    
    /// <summary>
    /// Add a prepared card to the display
    /// </summary>
    public void AddPreparedCard(PreparedCard prepared)
    {
        if (prepared == null)
        {
            Debug.LogWarning("[PreparedCardsUI] Cannot add null prepared card!");
            return;
        }
        
        if (cardVisuals.ContainsKey(prepared))
        {
            Debug.LogWarning($"[PreparedCardsUI] Card {prepared.sourceCard.cardName} already displayed!");
            return;
        }
        
        // Create visual card object
        GameObject cardObj = CreatePreparedCardVisual(prepared);
        
        if (cardObj != null)
        {
            cardVisuals[prepared] = cardObj;
            preparedCardsList.Add(prepared);
            prepared.cardVisualObject = cardObj;
            
            // Update layout
            UpdateCardPositions();
            
            Debug.Log($"<color=green>[PreparedCardsUI] Added prepared card: {prepared.sourceCard.cardName}</color>");
        }
    }
    
    /// <summary>
    /// Remove a prepared card from the display
    /// </summary>
    public void RemovePreparedCard(PreparedCard prepared)
    {
        if (prepared == null || !cardVisuals.ContainsKey(prepared))
        {
            return;
        }
        
        GameObject cardObj = cardVisuals[prepared];
        cardVisuals.Remove(prepared);
        preparedCardsList.Remove(prepared);
        
        // Destroy visual
        if (cardObj != null)
        {
            Destroy(cardObj);
        }
        
        // Update layout
        UpdateCardPositions();
        
        Debug.Log($"<color=yellow>[PreparedCardsUI] Removed prepared card: {prepared.sourceCard.cardName}</color>");
    }
    
    /// <summary>
    /// Update visual state of a prepared card (turn counter, glow, damage/guard values, etc.)
    /// </summary>
    public void UpdatePreparedCard(PreparedCard prepared)
    {
        if (prepared == null || !cardVisuals.ContainsKey(prepared))
        {
            return;
        }
        
        GameObject cardObj = cardVisuals[prepared];
        
        // Update turn counter badge
        UpdateTurnCounterBadge(cardObj, prepared);
        
        // Update glow intensity based on charges
        UpdateGlowEffect(cardObj, prepared);
        
        // Update card damage/guard display with current prepared values
        UpdateCardValues(cardObj, prepared);
    }
    
    /// <summary>
    /// Update the displayed damage/guard values on the card to show current prepared values
    /// </summary>
    private void UpdateCardValues(GameObject cardObj, PreparedCard prepared)
    {
        if (cardObj == null || prepared == null || prepared.sourceCard == null) return;
        
        // Calculate current values
        int currentDamage = prepared.GetCurrentDamage();
        int currentGuard = prepared.GetCurrentGuard();
        int originalDamage = prepared.sourceCard.damage;
        int originalBlock = prepared.sourceCard.block;
        
        // Find all text elements and update damage/guard values
        TextMeshProUGUI[] allTexts = cardObj.GetComponentsInChildren<TextMeshProUGUI>(true);
        
        foreach (var text in allTexts)
        {
            if (text == null) continue;
            
            string textContent = text.text;
            string textName = text.name.ToLower();
            bool updated = false;
            
            // Update damage display
            if (currentDamage > 0 && originalDamage > 0)
            {
                // Check if this text element likely contains damage information
                bool isDamageText = textName.Contains("damage") || textName.Contains("attack") || 
                                   textContent.Contains("{damage}") ||
                                   (textContent.Contains(originalDamage.ToString()) && 
                                    (textContent.ToLower().Contains("deal") || textContent.ToLower().Contains("damage")));
                
                if (isDamageText)
                {
                    // Replace {damage} placeholder
                    if (textContent.Contains("{damage}"))
                    {
                        text.text = textContent.Replace("{damage}", currentDamage.ToString());
                        updated = true;
                    }
                    // Replace the original damage number
                    else if (textContent.Contains(originalDamage.ToString()))
                    {
                        text.text = System.Text.RegularExpressions.Regex.Replace(
                            textContent, 
                            @"\b" + originalDamage + @"\b", 
                            currentDamage.ToString()
                        );
                        updated = true;
                    }
                }
            }
            
            // Update guard/block display
            if (currentGuard > 0 && originalBlock > 0 && !updated)
            {
                // Check if this text element likely contains guard/block information
                bool isGuardText = textName.Contains("guard") || textName.Contains("block") || 
                                  textContent.Contains("{guard}") || textContent.Contains("{block}") ||
                                  (textContent.Contains(originalBlock.ToString()) && 
                                   (textContent.ToLower().Contains("gain") || textContent.ToLower().Contains("guard") || textContent.ToLower().Contains("block")));
                
                if (isGuardText)
                {
                    // Replace {guard} or {block} placeholder
                    if (textContent.Contains("{guard}"))
                    {
                        text.text = textContent.Replace("{guard}", currentGuard.ToString());
                    }
                    else if (textContent.Contains("{block}"))
                    {
                        text.text = textContent.Replace("{block}", currentGuard.ToString());
                    }
                    // Replace the original block number
                    else if (textContent.Contains(originalBlock.ToString()))
                    {
                        text.text = System.Text.RegularExpressions.Regex.Replace(
                            textContent, 
                            @"\b" + originalBlock + @"\b", 
                            currentGuard.ToString()
                        );
                    }
                }
            }
        }
        
        // Update the card description with current prepared values
        // The description may have {damage} or {guard} placeholders that need to show current values
        var cardUI = cardObj.GetComponent<DeckBuilderCardUI>();
        if (cardUI != null && prepared.sourceCard is CardDataExtended extendedCard)
        {
            // Find description text element (reuse allTexts from above)
            foreach (var text in allTexts)
            {
                if (text == null) continue;
                
                // Check if this is the description text (usually contains the card description)
                string textContent = text.text;
                string originalDesc = extendedCard.description;
                
                // If this text matches or contains the original description, it's likely the description text
                if (textContent.Contains(originalDesc) || textContent.Contains("{damage}") || textContent.Contains("{guard}"))
                {
                    // Get the dynamic description first (resolves other placeholders)
                    string dynamicDesc = extendedCard.GetDynamicDescription(prepared.owner);
                    
                    // Override {damage} and {guard} with current prepared values
                    if (currentDamage > 0)
                    {
                        // Replace any damage-related placeholders with current prepared damage
                        dynamicDesc = System.Text.RegularExpressions.Regex.Replace(
                            dynamicDesc, 
                            @"\{damage\}", 
                            currentDamage.ToString()
                        );
                        dynamicDesc = System.Text.RegularExpressions.Regex.Replace(
                            dynamicDesc, 
                            @"\{baseDamage\}", 
                            currentDamage.ToString()
                        );
                        // Also replace the calculated damage from GetDynamicDescription with our prepared value
                        if (originalDamage > 0 && dynamicDesc.Contains(originalDamage.ToString()))
                        {
                            // Replace the original damage number with current prepared damage
                            dynamicDesc = System.Text.RegularExpressions.Regex.Replace(
                                dynamicDesc, 
                                @"\b" + originalDamage + @"\b", 
                                currentDamage.ToString()
                            );
                        }
                    }
                    
                    if (currentGuard > 0)
                    {
                        // Replace guard-related placeholders
                        dynamicDesc = System.Text.RegularExpressions.Regex.Replace(
                            dynamicDesc, 
                            @"\{guard\}", 
                            currentGuard.ToString()
                        );
                        dynamicDesc = System.Text.RegularExpressions.Regex.Replace(
                            dynamicDesc, 
                            @"\{block\}", 
                            currentGuard.ToString()
                        );
                        dynamicDesc = System.Text.RegularExpressions.Regex.Replace(
                            dynamicDesc, 
                            @"\{baseGuard\}", 
                            currentGuard.ToString()
                        );
                        // Also replace the original block number with current prepared guard
                        if (originalBlock > 0 && dynamicDesc.Contains(originalBlock.ToString()))
                        {
                            dynamicDesc = System.Text.RegularExpressions.Regex.Replace(
                                dynamicDesc, 
                                @"\b" + originalBlock + @"\b", 
                                currentGuard.ToString()
                            );
                        }
                    }
                    
                    text.text = dynamicDesc;
                    break; // Found description text, no need to continue
                }
            }
        }
    }
    
    /// <summary>
    /// Clear all prepared card visuals
    /// </summary>
    public void ClearAll()
    {
        foreach (var kvp in cardVisuals)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value);
            }
        }
        
        cardVisuals.Clear();
        preparedCardsList.Clear();
        
        Debug.Log("<color=red>[PreparedCardsUI] Cleared all prepared cards</color>");
    }
    
    /// <summary>
    /// Create visual representation of a prepared card
    /// </summary>
    private GameObject CreatePreparedCardVisual(PreparedCard prepared)
    {
        if (preparedCardPrefab == null)
        {
            Debug.LogError("[PreparedCardsUI] preparedCardPrefab is not assigned!");
            return null;
        }
        
        // Instantiate card prefab
        GameObject cardObj = Instantiate(preparedCardPrefab, preparedCardsContainer);
        cardObj.name = $"PreparedCard_{prepared.sourceCard.cardName}";
        
        // Initialize card display using DeckBuilderCardUI or similar component
        var cardUI = cardObj.GetComponent<DeckBuilderCardUI>();
        if (cardUI != null)
        {
            cardUI.Initialize(prepared.sourceCard, null, prepared.owner);
            
            // Store reference to PreparedCard for dynamic value updates
            var preparedCardRef = cardObj.AddComponent<PreparedCardReference>();
            if (preparedCardRef != null)
            {
                preparedCardRef.preparedCard = prepared;
            }
        }
        else
        {
            Debug.LogWarning($"[PreparedCardsUI] Card prefab missing DeckBuilderCardUI component!");
        }
        
        // Update card values immediately after initialization
        UpdateCardValues(cardObj, prepared);
        
        // Add glow effect
        AddGlowEffect(cardObj);
        
        // Add turn counter badge
        AddTurnCounterBadge(cardObj, prepared);
        
        // Add click interaction
        AddClickInteraction(cardObj, prepared);
        
        return cardObj;
    }
    
    /// <summary>
    /// Add glow effect to prepared card
    /// </summary>
    private void AddGlowEffect(GameObject cardObj)
    {
        // Add a glow outline/image
        GameObject glowObj = new GameObject("GlowEffect");
        glowObj.transform.SetParent(cardObj.transform, false);
        glowObj.transform.SetAsFirstSibling(); // Behind card content
        
        Image glowImage = glowObj.AddComponent<Image>();
        glowImage.color = glowColor;
        
        // Use a simple sprite or shader for glow
        // For now, use a stretched white sprite with color
        glowImage.sprite = null; // TODO: Assign glow sprite
        
        RectTransform glowRT = glowObj.GetComponent<RectTransform>();
        glowRT.anchorMin = Vector2.zero;
        glowRT.anchorMax = Vector2.one;
        glowRT.sizeDelta = new Vector2(10f, 10f); // Slightly larger than card
        glowRT.anchoredPosition = Vector2.zero;
        
        // Add pulse animation
        var pulseAnim = glowObj.AddComponent<PreparedCardGlowPulse>();
        pulseAnim.baseColor = glowColor;
        pulseAnim.pulseSpeed = pulseSpeed;
        pulseAnim.intensity = glowIntensity;
    }
    
    /// <summary>
    /// Add turn counter badge to prepared card
    /// </summary>
    private void AddTurnCounterBadge(GameObject cardObj, PreparedCard prepared)
    {
        // Create badge UI
        GameObject badgeObj = new GameObject("TurnCounterBadge");
        badgeObj.transform.SetParent(cardObj.transform, false);
        
        // Background
        Image badgeBG = badgeObj.AddComponent<Image>();
        badgeBG.color = new Color(0f, 0f, 0f, 0.8f);
        
        RectTransform badgeRT = badgeObj.GetComponent<RectTransform>();
        badgeRT.anchorMin = new Vector2(1f, 1f); // Top-right corner
        badgeRT.anchorMax = new Vector2(1f, 1f);
        badgeRT.pivot = new Vector2(1f, 1f);
        badgeRT.anchoredPosition = new Vector2(-10f, -10f);
        badgeRT.sizeDelta = new Vector2(50f, 50f);
        
        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(badgeObj.transform, false);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = $"{prepared.turnsPrepared}/{prepared.maxTurns}";
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 24;
        text.color = Color.white;
        text.fontStyle = FontStyles.Bold;
        
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;
        textRT.anchoredPosition = Vector2.zero;
    }
    
    /// <summary>
    /// Update turn counter badge text
    /// </summary>
    private void UpdateTurnCounterBadge(GameObject cardObj, PreparedCard prepared)
    {
        // Find badge
        Transform badge = cardObj.transform.Find("TurnCounterBadge");
        if (badge != null)
        {
            Transform textTransform = badge.Find("Text");
            if (textTransform != null)
            {
                var text = textTransform.GetComponent<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = $"{prepared.turnsPrepared}/{prepared.maxTurns}";
                    
                    // Change color if overcharged
                    if (prepared.isOvercharged)
                    {
                        text.color = new Color(1f, 0.8f, 0f); // Gold for overcharge
                    }
                    else if (prepared.decayAmount > 0f)
                    {
                        text.color = new Color(1f, 0.3f, 0.3f); // Red for decay
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Update glow effect intensity
    /// </summary>
    private void UpdateGlowEffect(GameObject cardObj, PreparedCard prepared)
    {
        Transform glowTransform = cardObj.transform.Find("GlowEffect");
        if (glowTransform != null)
        {
            var pulseAnim = glowTransform.GetComponent<PreparedCardGlowPulse>();
            if (pulseAnim != null)
            {
                // Increase intensity with more charges
                float intensityBoost = prepared.turnsPrepared * 0.2f;
                pulseAnim.intensity = glowIntensity + intensityBoost;
                
                // Change color if overcharged or decaying
                if (prepared.isOvercharged)
                {
                    pulseAnim.baseColor = new Color(1f, 0.8f, 0f); // Gold
                }
                else if (prepared.decayAmount > 0f)
                {
                    pulseAnim.baseColor = new Color(1f, 0.3f, 0.3f); // Red
                }
            }
        }
    }
    
    /// <summary>
    /// Add click interaction to unleash card
    /// </summary>
    private void AddClickInteraction(GameObject cardObj, PreparedCard prepared)
    {
        // Add EventTrigger for click
        EventTrigger trigger = cardObj.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = cardObj.AddComponent<EventTrigger>();
        }
        
        // Click event
        EventTrigger.Entry clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerClick;
        clickEntry.callback.AddListener((data) => OnPreparedCardClicked(prepared));
        trigger.triggers.Add(clickEntry);
        
        // Hover highlight
        EventTrigger.Entry hoverEnter = new EventTrigger.Entry();
        hoverEnter.eventID = EventTriggerType.PointerEnter;
        hoverEnter.callback.AddListener((data) => OnCardHoverEnter(cardObj));
        trigger.triggers.Add(hoverEnter);
        
        EventTrigger.Entry hoverExit = new EventTrigger.Entry();
        hoverExit.eventID = EventTriggerType.PointerExit;
        hoverExit.callback.AddListener((data) => OnCardHoverExit(cardObj));
        trigger.triggers.Add(hoverExit);
    }
    
    /// <summary>
    /// Handle prepared card click (attempt manual unleash)
    /// </summary>
    private void OnPreparedCardClicked(PreparedCard prepared)
    {
        Debug.Log($"<color=yellow>[PreparedCardsUI] Clicked prepared card: {prepared.sourceCard.cardName}</color>");
        
        var prepManager = PreparationManager.Instance;
        if (prepManager != null)
        {
            bool success = prepManager.UnleashCardManually(prepared, prepared.owner);
            
            if (!success)
            {
                // Show feedback (not enough energy, etc.)
                Debug.LogWarning($"[PreparedCardsUI] Failed to unleash {prepared.sourceCard.cardName}");
                // TODO: Show UI feedback
            }
        }
    }
    
    /// <summary>
    /// Highlight card on hover
    /// </summary>
    private void OnCardHoverEnter(GameObject cardObj)
    {
        cardObj.transform.localScale = Vector3.one * 1.1f;
    }
    
    /// <summary>
    /// Remove highlight on hover exit
    /// </summary>
    private void OnCardHoverExit(GameObject cardObj)
    {
        cardObj.transform.localScale = Vector3.one;
    }
    
    /// <summary>
    /// Update positions of all prepared cards in a horizontal row
    /// </summary>
    private void UpdateCardPositions()
    {
        int count = preparedCardsList.Count;
        if (count == 0) return;
        
        // Calculate center position
        float totalWidth = (count - 1) * cardSpacing;
        float startX = -totalWidth / 2f;
        
        for (int i = 0; i < count; i++)
        {
            PreparedCard prepared = preparedCardsList[i];
            if (cardVisuals.TryGetValue(prepared, out GameObject cardObj))
            {
                RectTransform rt = cardObj.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(startX + (i * cardSpacing), 0f);
                }
            }
        }
    }
}

/// <summary>
/// Simple pulsing glow animation for prepared cards
/// </summary>
public class PreparedCardGlowPulse : MonoBehaviour
{
    public Color baseColor = Color.cyan;
    public float pulseSpeed = 2f;
    public float intensity = 2f;
    
    private Image glowImage;
    private float time = 0f;
    
    private void Awake()
    {
        glowImage = GetComponent<Image>();
    }
    
    private void Update()
    {
        if (glowImage == null) return;
        
        time += Time.deltaTime * pulseSpeed;
        float pulse = (Mathf.Sin(time) + 1f) / 2f; // 0 to 1
        
        Color pulseColor = baseColor * (1f + pulse * intensity);
        pulseColor.a = baseColor.a;
        
        glowImage.color = pulseColor;
    }
}












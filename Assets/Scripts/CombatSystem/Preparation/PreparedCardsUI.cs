using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
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
    
    [Header("Card Scaling")]
    [Tooltip("Scale for cards that cannot be unleashed (0/3 or not first in queue)")]
    public float disabledCardScale = 0.7f;
    [Tooltip("Scale for cards that can be unleashed (normal size)")]
    public float enabledCardScale = 1f;
    
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
            
            // Update scales for all cards (new card added, queue order may have changed)
            UpdateAllCardScales();
            
            Debug.Log($"<color=green>[PreparedCardsUI] Added prepared card: {prepared.sourceCard.cardName}</color>");
        }
    }
    
    /// <summary>
    /// Remove a prepared card from the display
    /// </summary>
    public void RemovePreparedCard(PreparedCard prepared)
    {
        if (prepared == null)
        {
            return;
        }
        
        // Check if we have a visual for this card
        if (!cardVisuals.ContainsKey(prepared))
        {
            // Card might have been removed already - check if there's a stale visual
            // ONLY match by exact instance reference, NOT by name (to avoid removing duplicate cards)
            GameObject staleVisual = null;
            PreparedCard stalePrepared = null;
            foreach (var kvp in cardVisuals.ToList())
            {
                // Only match by exact instance reference - never by name!
                if (kvp.Key == prepared)
                {
                    staleVisual = kvp.Value;
                    stalePrepared = kvp.Key;
                    break;
                }
            }
            
            if (staleVisual != null && stalePrepared != null)
            {
                cardVisuals.Remove(stalePrepared);
                preparedCardsList.Remove(stalePrepared);
                Destroy(staleVisual);
                UpdateCardPositions();
                UpdateAllCardScales();
                Debug.Log($"<color=yellow>[PreparedCardsUI] Removed stale prepared card visual: {prepared.sourceCard?.cardName ?? "Unknown"}</color>");
            }
            else
            {
                Debug.LogWarning($"[PreparedCardsUI] Attempted to remove prepared card '{prepared.sourceCard?.cardName ?? "Unknown"}' but it was not found in visuals. This may indicate a timing issue or the card was already removed.");
            }
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
        
        // Update scales for all cards (card removed, queue order changed)
        UpdateAllCardScales();
        
        Debug.Log($"<color=yellow>[PreparedCardsUI] Removed prepared card: {prepared.sourceCard?.cardName ?? "Unknown"}</color>");
    }
    
    /// <summary>
    /// Clean up any stale visuals that don't match current prepared cards
    /// Called periodically or when issues are detected
    /// </summary>
    public void CleanupStaleVisuals()
    {
        var prepManager = PreparationManager.Instance;
        if (prepManager == null) return;
        
        // Get current list of valid prepared cards
        var validPreparedCards = prepManager.GetPreparedCards();
        
        // Find visuals that don't match any valid prepared card
        List<PreparedCard> toRemove = new List<PreparedCard>();
        foreach (var kvp in cardVisuals.ToList()) // Use ToList() to avoid modification during iteration
        {
            if (kvp.Key == null || !validPreparedCards.Contains(kvp.Key))
            {
                toRemove.Add(kvp.Key);
            }
        }
        
        // Remove stale visuals
        foreach (var stale in toRemove)
        {
            Debug.LogWarning($"[PreparedCardsUI] Cleaning up stale visual for: {stale?.sourceCard?.cardName ?? "Unknown"}");
            RemovePreparedCard(stale);
        }
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
        
        // Update card scale based on whether it can be unleashed
        UpdateCardScale(cardObj, prepared);
    }
    
    /// <summary>
    /// Update scale of all prepared cards based on whether they can be unleashed
    /// </summary>
    public void UpdateAllCardScales()
    {
        var prepManager = PreparationManager.Instance;
        if (prepManager == null) return;
        
        var allPreparedCards = prepManager.GetPreparedCards();
        if (allPreparedCards == null || allPreparedCards.Count == 0) return;
        
        // Get the first card in queue (can be unleashed if it has charges)
        PreparedCard firstCard = allPreparedCards.Count > 0 ? allPreparedCards[0] : null;
        
        foreach (var kvp in cardVisuals.ToList())
        {
            if (kvp.Key != null && kvp.Value != null)
            {
                UpdateCardScale(kvp.Value, kvp.Key, firstCard);
            }
        }
    }
    
    /// <summary>
    /// Update scale of a single prepared card based on whether it can be unleashed
    /// </summary>
    private void UpdateCardScale(GameObject cardObj, PreparedCard prepared)
    {
        var prepManager = PreparationManager.Instance;
        if (prepManager == null)
        {
            // Default to disabled scale if manager not available
            cardObj.transform.localScale = Vector3.one * disabledCardScale;
            return;
        }
        
        var allPreparedCards = prepManager.GetPreparedCards();
        PreparedCard firstCard = allPreparedCards != null && allPreparedCards.Count > 0 ? allPreparedCards[0] : null;
        
        UpdateCardScale(cardObj, prepared, firstCard);
    }
    
    /// <summary>
    /// Update scale of a single prepared card based on whether it can be unleashed
    /// </summary>
    private void UpdateCardScale(GameObject cardObj, PreparedCard prepared, PreparedCard firstCard)
    {
        if (cardObj == null || prepared == null) return;
        
        // Check if card can be unleashed:
        // 1. Must have at least 1 charge (turnsPrepared >= 1)
        // 2. Must be first in queue
        bool canUnleash = prepared.turnsPrepared >= 1 && prepared == firstCard;
        
        float targetScale = canUnleash ? enabledCardScale : disabledCardScale;
        
        // Animate scale change
        LeanTween.cancel(cardObj);
        LeanTween.scale(cardObj, Vector3.one * targetScale, 0.3f)
            .setEase(LeanTweenType.easeOutQuad);
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
            // Get the dynamic description first (resolves other placeholders)
            string dynamicDesc = extendedCard.GetDynamicDescription(prepared.owner);
            
            // Override {damage} and {guard} with current prepared values
            if (currentDamage > 0)
            {
                // Replace any damage-related placeholders with current prepared damage
                dynamicDesc = System.Text.RegularExpressions.Regex.Replace(
                    dynamicDesc, 
                    @"\{damage\}", 
                    currentDamage.ToString(),
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                dynamicDesc = System.Text.RegularExpressions.Regex.Replace(
                    dynamicDesc, 
                    @"\{baseDamage\}", 
                    currentDamage.ToString(),
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                // Also replace {PrepareDamage} with the actual prepared card damage
                // This is important for cards like Twin Strike that say "deal {PrepareDamage} damage instead"
                dynamicDesc = System.Text.RegularExpressions.Regex.Replace(
                    dynamicDesc, 
                    @"\{PrepareDamage\}", 
                    currentDamage.ToString(),
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                
                // Replace any damage number in the description with the prepared damage value
                // GetDynamicDescription() replaces {damage} with a number, so we need to find and replace that number
                // Match patterns like "19 Physical damage", "Deal 19 damage", "19 damage", etc.
                
                // Pattern 1: "Deal X [Type] damage" -> "Deal {currentDamage} [Type] damage"
                dynamicDesc = System.Text.RegularExpressions.Regex.Replace(
                    dynamicDesc,
                    @"Deal\s+\d+(?:\s+(Physical|Chaos|Fire|Cold|Lightning|Poison|Bleed))?\s+damage",
                    match => {
                        string damageType = match.Groups[1].Success ? $" {match.Groups[1].Value}" : "";
                        return $"Deal {currentDamage}{damageType} damage";
                    },
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                
                // Pattern 2: "X [Type] damage" (standalone) -> "{currentDamage} [Type] damage"
                dynamicDesc = System.Text.RegularExpressions.Regex.Replace(
                    dynamicDesc,
                    @"\b\d+\s+((?:Physical|Chaos|Fire|Cold|Lightning|Poison|Bleed)\s+)?damage\b",
                    match => {
                        string damageType = match.Groups[1].Success ? match.Groups[1].Value : "";
                        return $"{currentDamage} {damageType}damage";
                    },
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                
                // Pattern 3: Any number followed by "damage" (catch-all for other formats)
                dynamicDesc = System.Text.RegularExpressions.Regex.Replace(
                    dynamicDesc,
                    @"\b\d+\s+damage\b",
                    $"{currentDamage} damage",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
            }
            
            if (currentGuard > 0)
            {
                // Replace guard-related placeholders
                dynamicDesc = System.Text.RegularExpressions.Regex.Replace(
                    dynamicDesc, 
                    @"\{guard\}", 
                    currentGuard.ToString(),
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                dynamicDesc = System.Text.RegularExpressions.Regex.Replace(
                    dynamicDesc, 
                    @"\{block\}", 
                    currentGuard.ToString(),
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                dynamicDesc = System.Text.RegularExpressions.Regex.Replace(
                    dynamicDesc, 
                    @"\{baseGuard\}", 
                    currentGuard.ToString(),
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                
                // Replace guard/block numbers in the description
                dynamicDesc = System.Text.RegularExpressions.Regex.Replace(
                    dynamicDesc,
                    @"\b\d+\s+(guard|block)\b",
                    $"{currentGuard} $1",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                
                // Replace "Gain X guard/block" patterns
                dynamicDesc = System.Text.RegularExpressions.Regex.Replace(
                    dynamicDesc,
                    @"Gain\s+\d+\s+(guard|block)",
                    $"Gain {currentGuard} $1",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
            }
            
            // Find description text element and update it
            foreach (var text in allTexts)
            {
                if (text == null) continue;
                
                string textContent = text.text;
                string textName = text.name.ToLower();
                string originalDesc = extendedCard.description;
                
                // If this text matches or contains the original description, it's likely the description text
                if (textName.Contains("description") || 
                    textContent.Contains(originalDesc) || 
                    textContent.Contains("{damage}") || 
                    textContent.Contains("{guard}"))
                {
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
        
        // Set initial scale based on whether card can be unleashed
        UpdateCardScale(cardObj, prepared);
        
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
        
        // Click event - use the GameObject to look up the PreparedCard reference dynamically
        // This prevents using stale references if the card was already removed
        EventTrigger.Entry clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerClick;
        clickEntry.callback.AddListener((data) => OnPreparedCardClicked(cardObj));
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
    /// Gets the PreparedCard from the GameObject's PreparedCardReference component
    /// </summary>
    private void OnPreparedCardClicked(GameObject cardObj)
    {
        if (cardObj == null)
        {
            Debug.LogWarning("[PreparedCardsUI] Clicked card GameObject is null!");
            return;
        }
        
        // Get PreparedCard from the GameObject's PreparedCardReference component
        // This ensures we're using the current reference, not a stale captured one
        PreparedCardReference cardRef = cardObj.GetComponent<PreparedCardReference>();
        if (cardRef == null || cardRef.preparedCard == null)
        {
            Debug.LogWarning($"[PreparedCardsUI] Clicked card {cardObj.name} has no valid PreparedCardReference! Removing stale visual...");
            
            // Try to find the PreparedCard from our dictionary
            PreparedCard foundPrepared = null;
            foreach (var kvp in cardVisuals)
            {
                if (kvp.Value == cardObj)
                {
                    foundPrepared = kvp.Key;
                    break;
                }
            }
            
            if (foundPrepared != null)
            {
                RemovePreparedCard(foundPrepared);
            }
            else
            {
                // Card visual exists but isn't in our tracking - destroy it
                Destroy(cardObj);
            }
            return;
        }
        
        PreparedCard prepared = cardRef.preparedCard;
        
        Debug.Log($"<color=yellow>[PreparedCardsUI] Clicked prepared card: {prepared.sourceCard?.cardName ?? "Unknown"}</color>");
        
        var prepManager = PreparationManager.Instance;
        if (prepManager == null)
        {
            Debug.LogWarning("[PreparedCardsUI] PreparationManager not found!");
            return;
        }
        
        // Validate that the prepared card is still in the manager's list
        // This prevents trying to unleash a card that was already removed
        if (!prepManager.IsPreparedCardValid(prepared))
        {
            Debug.LogWarning($"[PreparedCardsUI] Prepared card {prepared.sourceCard?.cardName ?? "Unknown"} is no longer valid (may have been already unleashed or removed). Removing from UI...");
            
            // Clean up the stale visual
            RemovePreparedCard(prepared);
            return;
        }
        
        bool success = prepManager.UnleashCardManually(prepared, prepared.owner);
        
        if (!success)
        {
            // Show feedback (not enough energy, etc.)
            Debug.LogWarning($"[PreparedCardsUI] Failed to unleash {prepared.sourceCard?.cardName ?? "Unknown"}");
            // TODO: Show UI feedback
        }
        else
        {
            // Card should be removed by PreparationManager, but ensure UI is updated
            // RemovePreparedCard will be called by PreparationManager.ExecuteUnleash
            // But we can also check here to ensure cleanup
            if (!prepManager.IsPreparedCardValid(prepared))
            {
                RemovePreparedCard(prepared);
            }
        }
    }
    
    /// <summary>
    /// Highlight card on hover (preserve base scale)
    /// </summary>
    private void OnCardHoverEnter(GameObject cardObj)
    {
        // Get the current base scale (enabled or disabled)
        var prepManager = PreparationManager.Instance;
        if (prepManager != null)
        {
            var preparedCardRef = cardObj.GetComponent<PreparedCardReference>();
            if (preparedCardRef != null && preparedCardRef.preparedCard != null)
            {
                var allPreparedCards = prepManager.GetPreparedCards();
                PreparedCard firstCard = allPreparedCards != null && allPreparedCards.Count > 0 ? allPreparedCards[0] : null;
                PreparedCard prepared = preparedCardRef.preparedCard;
                bool canUnleash = prepared.turnsPrepared >= 1 && prepared == firstCard;
                float baseScale = canUnleash ? enabledCardScale : disabledCardScale;
                
                LeanTween.cancel(cardObj);
                LeanTween.scale(cardObj, Vector3.one * baseScale * 1.1f, 0.2f)
                    .setEase(LeanTweenType.easeOutQuad);
                return;
            }
        }
        // Fallback
        LeanTween.cancel(cardObj);
        LeanTween.scale(cardObj, Vector3.one * 1.1f, 0.2f)
            .setEase(LeanTweenType.easeOutQuad);
    }
    
    /// <summary>
    /// Remove highlight on hover exit (restore base scale)
    /// </summary>
    private void OnCardHoverExit(GameObject cardObj)
    {
        // Restore the base scale based on whether card can be unleashed
        var prepManager = PreparationManager.Instance;
        if (prepManager != null)
        {
            var preparedCardRef = cardObj.GetComponent<PreparedCardReference>();
            if (preparedCardRef != null && preparedCardRef.preparedCard != null)
            {
                var allPreparedCards = prepManager.GetPreparedCards();
                PreparedCard firstCard = allPreparedCards != null && allPreparedCards.Count > 0 ? allPreparedCards[0] : null;
                PreparedCard prepared = preparedCardRef.preparedCard;
                bool canUnleash = prepared.turnsPrepared >= 1 && prepared == firstCard;
                float baseScale = canUnleash ? enabledCardScale : disabledCardScale;
                
                LeanTween.cancel(cardObj);
                LeanTween.scale(cardObj, Vector3.one * baseScale, 0.2f)
                    .setEase(LeanTweenType.easeOutQuad);
                return;
            }
        }
        // Fallback
        LeanTween.cancel(cardObj);
        LeanTween.scale(cardObj, Vector3.one, 0.2f)
            .setEase(LeanTweenType.easeOutQuad);
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












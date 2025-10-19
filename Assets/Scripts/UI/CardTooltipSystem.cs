using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

/// <summary>
/// Manages card tooltips that appear on hover
/// </summary>
public class CardTooltipSystem : MonoBehaviour
{
    public static CardTooltipSystem Instance { get; private set; }
    
    [Header("Tooltip Settings")]
    public UIDocument tooltipDocument;
    public float tooltipDelay = 0.5f;
    public float tooltipOffset = 10f;
    
    [Header("Tooltip Style")]
    public int tooltipWidth = 300;
    public int tooltipHeight = 200;
    
    private VisualElement tooltipRoot;
    private VisualElement tooltipContainer;
    private Label tooltipTitle;
    private Label tooltipDescription;
    private Label tooltipStats;
    private Label tooltipCombo;
    private Label tooltipRequirements;
    
    private Card currentTooltipCard;
    private Character currentCharacter;
    private bool isTooltipVisible = false;
    private float tooltipTimer = 0f;
    private Vector2 tooltipPosition;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeTooltip();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void InitializeTooltip()
    {
        if (tooltipDocument == null)
        {
            // Create tooltip document if not assigned
            GameObject tooltipObj = new GameObject("CardTooltipUI");
            tooltipObj.transform.SetParent(transform);
            tooltipDocument = tooltipObj.AddComponent<UIDocument>();
        }
        
        // Create tooltip UI structure
        CreateTooltipUI();
        
        // Hide tooltip initially
        HideTooltip();
    }
    
    private void CreateTooltipUI()
    {
        tooltipRoot = tooltipDocument.rootVisualElement;
        
        // Create tooltip container
        tooltipContainer = new VisualElement();
        tooltipContainer.name = "TooltipContainer";
        tooltipContainer.style.position = Position.Absolute;
        tooltipContainer.style.width = tooltipWidth;
        tooltipContainer.style.height = tooltipHeight;
        tooltipContainer.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        tooltipContainer.style.borderLeftColor = Color.yellow;
        tooltipContainer.style.borderRightColor = Color.yellow;
        tooltipContainer.style.borderTopColor = Color.yellow;
        tooltipContainer.style.borderBottomColor = Color.yellow;
        tooltipContainer.style.borderLeftWidth = 2;
        tooltipContainer.style.borderRightWidth = 2;
        tooltipContainer.style.borderTopWidth = 2;
        tooltipContainer.style.borderBottomWidth = 2;
        tooltipContainer.style.borderTopLeftRadius = 5;
        tooltipContainer.style.borderTopRightRadius = 5;
        tooltipContainer.style.borderBottomLeftRadius = 5;
        tooltipContainer.style.borderBottomRightRadius = 5;
        tooltipContainer.style.paddingLeft = 10;
        tooltipContainer.style.paddingRight = 10;
        tooltipContainer.style.paddingTop = 10;
        tooltipContainer.style.paddingBottom = 10;
        
        // Create title
        tooltipTitle = new Label();
        tooltipTitle.name = "TooltipTitle";
        tooltipTitle.style.color = Color.white;
        tooltipTitle.style.fontSize = 16;
        tooltipTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
        tooltipTitle.style.marginBottom = 5;
        
        // Create description
        tooltipDescription = new Label();
        tooltipDescription.name = "TooltipDescription";
        tooltipDescription.style.color = new Color(0.9f, 0.9f, 0.9f);
        tooltipDescription.style.fontSize = 12;
        tooltipDescription.style.whiteSpace = WhiteSpace.Normal;
        tooltipDescription.style.marginBottom = 5;
        
        // Create stats
        tooltipStats = new Label();
        tooltipStats.name = "TooltipStats";
        tooltipStats.style.color = Color.cyan;
        tooltipStats.style.fontSize = 11;
        tooltipStats.style.whiteSpace = WhiteSpace.Normal;
        tooltipStats.style.marginBottom = 5;
        
        // Create combo info
        tooltipCombo = new Label();
        tooltipCombo.name = "TooltipCombo";
        tooltipCombo.style.color = Color.yellow;
        tooltipCombo.style.fontSize = 11;
        tooltipCombo.style.whiteSpace = WhiteSpace.Normal;
        tooltipCombo.style.marginBottom = 5;
        
        // Create requirements
        tooltipRequirements = new Label();
        tooltipRequirements.name = "TooltipRequirements";
        tooltipRequirements.style.color = Color.red;
        tooltipRequirements.style.fontSize = 11;
        tooltipRequirements.style.whiteSpace = WhiteSpace.Normal;
        
        // Add elements to container
        tooltipContainer.Add(tooltipTitle);
        tooltipContainer.Add(tooltipDescription);
        tooltipContainer.Add(tooltipStats);
        tooltipContainer.Add(tooltipCombo);
        tooltipContainer.Add(tooltipRequirements);
        
        // Add container to root
        tooltipRoot.Add(tooltipContainer);
    }
    
    private void Update()
    {
        if (isTooltipVisible && currentTooltipCard != null)
        {
            // Update tooltip position to follow mouse
            UpdateTooltipPosition();
        }
        
        // Update tooltip timer
        if (tooltipTimer > 0f)
        {
            UpdateTooltipTimer();
        }
    }
    
    /// <summary>
    /// Show tooltip for a card
    /// </summary>
    public void ShowTooltip(Card card, Character character, Vector2 screenPosition)
    {
        if (card == null) return;
        
        currentTooltipCard = card;
        currentCharacter = character;
        tooltipPosition = screenPosition;
        
        // Update tooltip content
        UpdateTooltipContent();
        
        // Position tooltip
        PositionTooltip(screenPosition);
        
        // Show tooltip
        tooltipContainer.style.display = DisplayStyle.Flex;
        isTooltipVisible = true;
        
        Debug.Log($"<color=cyan>Tooltip shown for: {card.cardName}</color>");
    }
    
    /// <summary>
    /// Hide the tooltip
    /// </summary>
    public void HideTooltip()
    {
        tooltipContainer.style.display = DisplayStyle.None;
        isTooltipVisible = false;
        currentTooltipCard = null;
        currentCharacter = null;
    }
    
    /// <summary>
    /// Start showing tooltip after delay
    /// </summary>
    public void StartTooltipTimer(Card card, Character character, Vector2 screenPosition)
    {
        currentTooltipCard = card;
        currentCharacter = character;
        tooltipPosition = screenPosition;
        tooltipTimer = tooltipDelay;
    }
    
    /// <summary>
    /// Update tooltip timer
    /// </summary>
    public void UpdateTooltipTimer()
    {
        if (tooltipTimer > 0f)
        {
            tooltipTimer -= Time.deltaTime;
            if (tooltipTimer <= 0f)
            {
                ShowTooltip(currentTooltipCard, currentCharacter, tooltipPosition);
            }
        }
    }
    
    /// <summary>
    /// Cancel tooltip timer
    /// </summary>
    public void CancelTooltipTimer()
    {
        tooltipTimer = 0f;
        HideTooltip();
    }
    
    /// <summary>
    /// Update tooltip content based on current card
    /// </summary>
    private void UpdateTooltipContent()
    {
        if (currentTooltipCard == null) return;
        
        // Title
        tooltipTitle.text = currentTooltipCard.cardName;
        
        // Description
        string description = currentTooltipCard.description;
        if (currentCharacter != null)
        {
            // Use dynamic description if available
            description = currentTooltipCard.GetDynamicDescription(currentCharacter);
        }
        tooltipDescription.text = description;
        
        // Stats
        string stats = "";
        if (currentTooltipCard.baseDamage > 0)
        {
            float totalDamage = currentTooltipCard.baseDamage;
            if (currentCharacter != null)
            {
                totalDamage += currentTooltipCard.damageScaling.CalculateScalingBonus(currentCharacter);
            }
            stats += $"Damage: {totalDamage:F1}\n";
        }
        
        if (currentTooltipCard.baseGuard > 0)
        {
            float totalGuard = currentTooltipCard.baseGuard;
            if (currentCharacter != null)
            {
                totalGuard += currentTooltipCard.guardScaling.CalculateScalingBonus(currentCharacter);
            }
            stats += $"Guard: {totalGuard:F1}\n";
        }
        
        stats += $"Cost: {currentTooltipCard.manaCost} Mana\n";
        stats += $"Type: {currentTooltipCard.cardType}";
        
        tooltipStats.text = stats;
        
        // Combo info
        string comboInfo = "";
        if (!string.IsNullOrEmpty(currentTooltipCard.comboDescription))
        {
            comboInfo += $"Combo: {currentTooltipCard.comboDescription}";
        }
        tooltipCombo.text = comboInfo;
        
        // Requirements
        string requirements = "";
        if (currentCharacter != null)
        {
            var (canUse, reason) = currentTooltipCard.CheckCardUsage(currentCharacter);
            if (!canUse)
            {
                requirements = $"Cannot use: {reason}";
            }
        }
        tooltipRequirements.text = requirements;
    }
    
    /// <summary>
    /// Position tooltip at screen position
    /// </summary>
    private void PositionTooltip(Vector2 screenPosition)
    {
        // Convert screen position to UI position
        Vector2 uiPosition = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
        
        // Offset tooltip to avoid covering the cursor
        uiPosition.x += tooltipOffset;
        uiPosition.y -= tooltipOffset;
        
        // Ensure tooltip stays on screen
        if (uiPosition.x + tooltipWidth > Screen.width)
        {
            uiPosition.x = screenPosition.x - tooltipWidth - tooltipOffset;
        }
        
        if (uiPosition.y - tooltipHeight < 0)
        {
            uiPosition.y = screenPosition.y + tooltipOffset;
        }
        
        tooltipContainer.style.left = uiPosition.x;
        tooltipContainer.style.top = uiPosition.y;
    }
    
    /// <summary>
    /// Update tooltip position to follow mouse
    /// </summary>
    private void UpdateTooltipPosition()
    {
        Vector2 mousePosition = Input.mousePosition;
        PositionTooltip(mousePosition);
    }
    
    /// <summary>
    /// Get tooltip text for a card (for external use)
    /// </summary>
    public string GetTooltipText(Card card, Character character)
    {
        if (card == null) return "";
        
        string tooltip = $"<b>{card.cardName}</b>\n";
        tooltip += $"{card.description}\n\n";
        
        // Add stats
        if (card.baseDamage > 0)
        {
            float totalDamage = card.baseDamage;
            if (character != null)
            {
                totalDamage += card.damageScaling.CalculateScalingBonus(character);
            }
            tooltip += $"Damage: {totalDamage:F1}\n";
        }
        
        if (card.baseGuard > 0)
        {
            float totalGuard = card.baseGuard;
            if (character != null)
            {
                totalGuard += card.guardScaling.CalculateScalingBonus(character);
            }
            tooltip += $"Guard: {totalGuard:F1}\n";
        }
        
        tooltip += $"Cost: {card.manaCost} Mana\n";
        tooltip += $"Type: {card.cardType}\n";
        
        if (!string.IsNullOrEmpty(card.comboDescription))
        {
            tooltip += $"\nCombo: {card.comboDescription}";
        }
        
        return tooltip;
    }
}

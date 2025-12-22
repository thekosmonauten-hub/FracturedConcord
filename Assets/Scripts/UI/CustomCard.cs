using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class CustomCard : VisualElement
{
    // Card data
    public Card cardData { get; private set; }
    public bool isUsable { get; private set; }
    public bool isSelected { get; private set; }
    public bool isComboHighlighted { get; private set; }
    
    // UI Elements
    private VisualElement cardBackground;
    private Label cardName;
    private Label manaCost;
    private VisualElement cardTypeIcon;
    private Label valueLabel;
    private Label cardDescription;
    private VisualElement aoeIndicator;
    private Label cardTypeLabel;
    
    // Events
    public System.Action<CustomCard> OnCardClicked;
    
    public CustomCard()
    {
        // Load the card template
        var cardTemplate = Resources.Load<VisualTreeAsset>("UI/Combat/CardTemplate");
        if (cardTemplate != null)
        {
            cardTemplate.CloneTree(this);
            InitializeElements();
        }
        else
        {
            Debug.LogError("CardTemplate.uxml not found in Resources/UI/Combat/");
        }
    }
    
    private void InitializeElements()
    {
        // Get references to UI elements
        cardBackground = this.Q<VisualElement>("CardBackground");
        cardName = this.Q<Label>("CardName");
        manaCost = this.Q<Label>("ManaCost");
        cardTypeIcon = this.Q<VisualElement>("CardTypeIcon");
        valueLabel = this.Q<Label>("ValueLabel");
        cardDescription = this.Q<Label>("CardDescription");
        aoeIndicator = this.Q<VisualElement>("AoEIndicator");
        cardTypeLabel = this.Q<Label>("CardTypeLabel");
        
        // Set up click event
        cardBackground.RegisterCallback<ClickEvent>(OnCardClick);
        
        // Set up hover effects
        cardBackground.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
        cardBackground.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
    }
    
    public void SetCardData(Card card, Character character)
    {
        cardData = card;
        UpdateCardDisplay(character);
    }
    
    public void UpdateCardDisplay(Character character)
    {
        if (cardData == null) return;
        
        // Update card name
        cardName.text = cardData.cardName;
        
        // Update mana cost (using GetManaCostDisplay for Skill cards with percentage)
        manaCost.text = cardData.GetManaCostDisplay(character);
        
        // Update card type
        UpdateCardType();
        
        // Update value (damage or guard)
        UpdateValue(character);
        
        // Update description
        cardDescription.text = cardData.GetDynamicDescription(character);
        
        // Update AoE indicator
        UpdateAoEIndicator();
        
        // Update usability
        UpdateUsability(character);
    }
    
    private void UpdateCardType()
    {
        string typeText = cardData.cardType.ToString().ToUpper();
        cardTypeLabel.text = typeText;
        
        // Remove existing type classes
        cardBackground.RemoveFromClassList("attack");
        cardBackground.RemoveFromClassList("guard");
        cardBackground.RemoveFromClassList("skill");
        cardBackground.RemoveFromClassList("power");
        cardBackground.RemoveFromClassList("aura");
        
        // Add appropriate type class
        cardBackground.AddToClassList(cardData.cardType.ToString().ToLower());
    }
    
    private void UpdateValue(Character character)
    {
        if (cardData.cardType == CardType.Attack && cardData.baseDamage > 0)
        {
            float totalDamage = DamageCalculator.CalculateCardDamage(cardData, character, character.weapons.meleeWeapon);
            valueLabel.text = totalDamage.ToString("F0");
        }
        else if (cardData.cardType == CardType.Guard && cardData.baseGuard > 0)
        {
            float totalGuard = DamageCalculator.CalculateGuardAmount(cardData, character);
            valueLabel.text = totalGuard.ToString("F0");
        }
        else
        {
            valueLabel.text = "";
        }
    }
    
    private void UpdateAoEIndicator()
    {
        if (cardData.isAoE)
        {
            aoeIndicator.style.display = DisplayStyle.Flex;
        }
        else
        {
            aoeIndicator.style.display = DisplayStyle.None;
        }
    }
    
    private void UpdateUsability(Character character)
    {
        isUsable = cardData.CanUseCard(character);
        
        if (isUsable)
        {
            cardBackground.RemoveFromClassList("unusable");
            cardBackground.RemoveFromClassList("mana-insufficient");
        }
        else
        {
            cardBackground.AddToClassList("unusable");
            
            // Check if it's specifically a mana issue
            if (character != null)
            {
                int requiredMana = cardData.GetCurrentManaCost(character);
                if (character.mana < requiredMana)
            {
                cardBackground.AddToClassList("mana-insufficient");
                }
            }
        }
        
        // Update mana cost color based on affordability
        UpdateManaCostColor(character);
    }
    
    private void UpdateManaCostColor(Character character)
    {
        if (manaCost != null && character != null)
        {
            int requiredMana = cardData.GetCurrentManaCost(character);
            if (character.mana >= requiredMana)
            {
                // Can afford - normal color
                manaCost.style.color = new Color(1f, 1f, 1f); // White
            }
            else
            {
                // Cannot afford - red color
                manaCost.style.color = new Color(1f, 0.3f, 0.3f); // Red
            }
        }
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (selected)
        {
            cardBackground.AddToClassList("selected");
        }
        else
        {
            cardBackground.RemoveFromClassList("selected");
        }
    }
    
    public void SetComboHighlighted(bool highlighted)
    {
        isComboHighlighted = highlighted;
        
        if (highlighted)
        {
            cardBackground.AddToClassList("combo-highlighted");
        }
        else
        {
            cardBackground.RemoveFromClassList("combo-highlighted");
        }
    }
    
    private void OnCardClick(ClickEvent evt)
    {
        OnCardClicked?.Invoke(this);
    }
    
    private void OnMouseEnter(MouseEnterEvent evt)
    {
        // Lift card and bring to front on hover using Unity's style system
        this.AddToClassList("card-hovered");
        
        // Apply hover transform using individual style properties
        this.style.marginTop = -20;
        this.style.rotate = new StyleRotate(new Angle(0));
        this.style.scale = new StyleScale(new Vector2(1.1f, 1.1f));
        
        // Show tooltip
        if (CardTooltipSystem.Instance != null && cardData != null)
        {
            Vector2 mousePos = evt.localMousePosition;
            // Convert to screen position (this is approximate)
            Vector2 screenPos = new Vector2(mousePos.x + Screen.width/2, mousePos.y + Screen.height/2);
            CardTooltipSystem.Instance.StartTooltipTimer(cardData, GetCurrentCharacter(), screenPos);
        }
    }
    
    private void OnMouseLeave(MouseLeaveEvent evt)
    {
        // Remove hover class
        this.RemoveFromClassList("card-hovered");
        
        // Reset transform - the CombatUI will reapply the fanning transform
        // This will be handled by the UpdateHand method when it's called
        
        // Hide tooltip
        if (CardTooltipSystem.Instance != null)
        {
            CardTooltipSystem.Instance.CancelTooltipTimer();
        }
    }
    
    /// <summary>
    /// Get the current character for tooltip calculations
    /// </summary>
    private Character GetCurrentCharacter()
    {
        // Try to get character from CharacterManager
        CharacterManager characterManager = CharacterManager.Instance;
        if (characterManager != null && characterManager.HasCharacter())
        {
            return characterManager.GetCurrentCharacter();
        }
        
        // Try to get character from CombatDeckManager
        CombatDeckManager deckManager = CombatDeckManager.Instance;
        if (deckManager != null)
        {
            return deckManager.GetCurrentCharacter();
        }
        
        return null;
    }
}

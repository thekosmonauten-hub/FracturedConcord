using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages aura activation and deactivation
/// Handles reliance cost checking and updates character data
/// </summary>
public class AuraManagerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AuraStorageUI auraStorage;
    [SerializeField] private ActiveAurasDisplay activeAurasDisplay;
    [SerializeField] private RelianceDisplay relianceDisplay;
    
    private CharacterManager characterManager;
    
    void Start()
    {
        characterManager = CharacterManager.Instance;
        
        if (auraStorage != null)
        {
            auraStorage.SetAuraManager(this);
            auraStorage.OnAuraClicked += OnAuraClicked;
        }
        
        // Auto-find RelianceDisplay if not assigned
        if (relianceDisplay == null)
        {
            relianceDisplay = FindFirstObjectByType<RelianceDisplay>();
        }
        
        RefreshDisplays();
    }
    
    /// <summary>
    /// Toggle aura activation (activate if inactive, deactivate if active)
    /// </summary>
    public void ToggleAura(RelianceAura aura)
    {
        if (aura == null || characterManager == null || !characterManager.HasCharacter())
        {
            Debug.LogWarning("[AuraManagerUI] Cannot toggle aura - missing aura or character");
            return;
        }
        
        Character character = characterManager.GetCurrentCharacter();
        
        // Check if owned
        if (character.ownedRelianceAuras == null || !character.ownedRelianceAuras.Contains(aura.auraName))
        {
            Debug.LogWarning($"[AuraManagerUI] Cannot activate {aura.auraName} - not owned");
            return;
        }
        
        // Check if active
        bool isActive = character.activeRelianceAuras != null && character.activeRelianceAuras.Contains(aura.auraName);
        
        if (isActive)
        {
            // Deactivate
            DeactivateAura(aura, character);
        }
        else
        {
            // Activate (check reliance cost)
            ActivateAura(aura, character);
        }
        
        RefreshDisplays();
    }
    
    /// <summary>
    /// Activate an aura (check reliance cost)
    /// </summary>
    private void ActivateAura(RelianceAura aura, Character character)
    {
        if (character.activeRelianceAuras == null)
        {
            character.activeRelianceAuras = new List<string>();
        }
        
        // Calculate total reliance cost of all active auras + this one
        int currentCost = GetTotalRelianceCost(character);
        int newCost = currentCost + aura.relianceCost;
        
        // Check if player has enough available reliance
        int availableReliance = character.maxReliance - currentCost;
        if (availableReliance < aura.relianceCost)
        {
            Debug.LogWarning($"[AuraManagerUI] Cannot activate {aura.auraName} - insufficient reliance (Need {aura.relianceCost}, Have {availableReliance})");
            return;
        }
        
        if (!character.activeRelianceAuras.Contains(aura.auraName))
        {
            character.activeRelianceAuras.Add(aura.auraName);
            UpdateCharacterReliance(character);
            Debug.Log($"[AuraManagerUI] Activated {aura.auraName} (Reliance cost: {aura.relianceCost}, Total: {newCost})");
        }
    }
    
    /// <summary>
    /// Deactivate an aura
    /// </summary>
    private void DeactivateAura(RelianceAura aura, Character character)
    {
        if (character.activeRelianceAuras != null)
        {
            character.activeRelianceAuras.Remove(aura.auraName);
            UpdateCharacterReliance(character);
            Debug.Log($"[AuraManagerUI] Deactivated {aura.auraName}");
        }
    }
    
    /// <summary>
    /// Update character's current reliance based on active auras
    /// Current reliance = maxReliance - total cost of active auras
    /// </summary>
    private void UpdateCharacterReliance(Character character)
    {
        int totalCost = GetTotalRelianceCost(character);
        character.reliance = Mathf.Max(0, character.maxReliance - totalCost);
    }
    
    /// <summary>
    /// Get total reliance cost of all active auras
    /// </summary>
    private int GetTotalRelianceCost(Character character)
    {
        if (character.activeRelianceAuras == null || character.activeRelianceAuras.Count == 0)
            return 0;
        
        int totalCost = 0;
        RelianceAuraDatabase database = RelianceAuraDatabase.Instance;
        
        if (database != null)
        {
            foreach (string auraName in character.activeRelianceAuras)
            {
                RelianceAura aura = database.GetAura(auraName);
                if (aura != null)
                {
                    totalCost += aura.relianceCost;
                }
            }
        }
        
        return totalCost;
    }
    
    /// <summary>
    /// Refresh all displays
    /// </summary>
    public void RefreshDisplays()
    {
        if (auraStorage != null)
        {
            auraStorage.Refresh();
        }
        
        if (activeAurasDisplay != null)
        {
            activeAurasDisplay.Refresh();
        }
        
        // Update reliance display when auras change
        if (relianceDisplay != null)
        {
            relianceDisplay.Refresh();
        }
    }
    
    private void OnAuraClicked(RelianceAura aura)
    {
        // This is handled by ToggleAura
    }
}


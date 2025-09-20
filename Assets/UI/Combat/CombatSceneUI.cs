using UnityEngine;
using UnityEngine.UIElements;

public class CombatSceneUI : MonoBehaviour
{
    [Header("UI References")]
    public UIDocument uiDocument;
    public CombatManager combatManager;
    public CombatUI combatUI;
    public SimpleCombatUI cardManager;
    
    private void Start()
    {
        // Auto-find components if not set
        if (uiDocument == null)
            uiDocument = FindFirstObjectByType<UIDocument>();
        if (combatManager == null)
            combatManager = FindFirstObjectByType<CombatManager>();
        if (combatUI == null)
            combatUI = FindFirstObjectByType<CombatUI>();
        if (cardManager == null)
            cardManager = FindFirstObjectByType<SimpleCombatUI>();
        
        // Pass references to CombatUI
        if (combatUI != null)
        {
            combatUI.combatManager = combatManager;
        }
        
        // Initialize card manager
        if (cardManager != null)
        {
            // SimpleCombatUI doesn't need uiDocument reference
            Debug.Log("SimpleCombatUI found and initialized.");
        }
        else
        {
            Debug.LogWarning("SimpleCombatUI not found! Cards will not be displayed.");
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// UI panel that displays loot rewards after combat victory
/// </summary>
public class LootRewardsUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Main panel container")]
    public GameObject rewardsPanel;
    
    [Tooltip("Content area for reward items")]
    public Transform rewardsContent;
    
    [Tooltip("Prefab for individual reward items")]
    public GameObject rewardItemPrefab;
    
    [Header("Summary Text")]
    [Tooltip("Text showing total experience gained")]
    public TextMeshProUGUI experienceText;
    
    [Tooltip("Text showing victory message")]
    public TextMeshProUGUI victoryText;
    
    [Header("Buttons")]
    [Tooltip("Button to collect rewards and continue")]
    public Button collectButton;
    
    [Tooltip("Button to return to main game")]
    public Button returnButton;
    
    private LootDropResult currentLoot;
    private bool rewardsApplied = false;

    private void Start()
    {
        // Hide panel by default
        if (rewardsPanel != null)
        {
            rewardsPanel.SetActive(false);
        }
        
        // Setup button listeners
        if (collectButton != null)
        {
            collectButton.onClick.AddListener(OnCollectRewards);
        }
        
        if (returnButton != null)
        {
            returnButton.onClick.AddListener(OnReturnToMainGame);
        }
        
        // Listen for combat end
        CombatDisplayManager combatManager = FindFirstObjectByType<CombatDisplayManager>();
        if (combatManager != null)
        {
            combatManager.OnCombatStateChanged += OnCombatStateChanged;
        }
    }

    private void OnDestroy()
    {
        // Cleanup listeners
        if (collectButton != null)
        {
            collectButton.onClick.RemoveListener(OnCollectRewards);
        }
        
        if (returnButton != null)
        {
            returnButton.onClick.RemoveListener(OnReturnToMainGame);
        }
        
        CombatDisplayManager combatManager = FindFirstObjectByType<CombatDisplayManager>();
        if (combatManager != null)
        {
            combatManager.OnCombatStateChanged -= OnCombatStateChanged;
        }
    }

    private void OnCombatStateChanged(CombatDisplayManager.CombatState newState)
    {
        if (newState == CombatDisplayManager.CombatState.Victory)
        {
            // Show rewards after a short delay
            Invoke(nameof(ShowRewards), 1.5f);
        }
    }

    private void ShowRewards()
    {
        CombatDisplayManager combatManager = FindFirstObjectByType<CombatDisplayManager>();
        if (combatManager == null)
        {
            Debug.LogError("[LootRewardsUI] CombatDisplayManager not found!");
            return;
        }
        
        currentLoot = combatManager.GetPendingLoot();
        
        if (currentLoot == null || currentLoot.rewards.Count == 0)
        {
            Debug.LogWarning("[LootRewardsUI] No loot to display");
            // Still show victory panel with no rewards
            DisplayEmptyRewards();
            return;
        }
        
        // Display rewards
        DisplayRewards(currentLoot);
        
        // Show panel
        if (rewardsPanel != null)
        {
            rewardsPanel.SetActive(true);
        }
        
        Debug.Log($"[LootRewardsUI] Displaying {currentLoot.rewards.Count} rewards");
    }

    private void DisplayRewards(LootDropResult loot)
    {
        // Clear existing reward items
        ClearRewardItems();
        
        // Set victory text
        if (victoryText != null)
        {
            victoryText.text = "VICTORY!";
        }
        
        // Set experience text
        if (experienceText != null && loot.totalExperience > 0)
        {
            experienceText.text = $"+{loot.totalExperience} Experience";
        }
        else if (experienceText != null)
        {
            experienceText.text = "";
        }
        
        // Create reward item UI elements
        if (rewardsContent != null && rewardItemPrefab != null)
        {
            foreach (var reward in loot.rewards)
            {
                CreateRewardItem(reward);
            }
        }
        else
        {
            Debug.LogWarning("[LootRewardsUI] RewardsContent or RewardItemPrefab not assigned!");
        }
    }

    private void DisplayEmptyRewards()
    {
        ClearRewardItems();
        
        if (victoryText != null)
        {
            victoryText.text = "VICTORY!";
        }
        
        if (experienceText != null)
        {
            experienceText.text = "No rewards this time";
        }
        
        if (rewardsPanel != null)
        {
            rewardsPanel.SetActive(true);
        }
    }

    private void CreateRewardItem(LootReward reward)
    {
        GameObject rewardItem = Instantiate(rewardItemPrefab, rewardsContent);
        
        // Setup reward item display
        TextMeshProUGUI rewardText = rewardItem.GetComponentInChildren<TextMeshProUGUI>();
        if (rewardText != null)
        {
            rewardText.text = reward.GetDisplayName();
        }
        
        // Setup icon if available
        Image iconImage = rewardItem.GetComponent<Image>();
        if (iconImage != null)
        {
            Sprite icon = reward.GetIcon();
            if (icon != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }
        }
        
        // Color based on reward type
        if (rewardText != null)
        {
            switch (reward.rewardType)
            {
                case RewardType.Currency:
                    rewardText.color = new Color(1f, 0.84f, 0f); // Gold
                    break;
                case RewardType.Experience:
                    rewardText.color = new Color(0.5f, 1f, 0.5f); // Light green
                    break;
                case RewardType.Item:
                    rewardText.color = Color.white;
                    break;
                case RewardType.Card:
                    rewardText.color = new Color(0.7f, 0.7f, 1f); // Light blue
                    break;
            }
        }
    }

    private void ClearRewardItems()
    {
        if (rewardsContent != null)
        {
            foreach (Transform child in rewardsContent)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void OnCollectRewards()
    {
        if (!rewardsApplied)
        {
            // Apply rewards to character
            CombatDisplayManager combatManager = FindFirstObjectByType<CombatDisplayManager>();
            if (combatManager != null)
            {
                combatManager.ApplyPendingLoot();
                rewardsApplied = true;
                Debug.Log("[LootRewardsUI] Rewards collected and applied!");
            }
        }
        
        // Hide panel
        if (rewardsPanel != null)
        {
            rewardsPanel.SetActive(false);
        }
        
        // Return to main game
        OnReturnToMainGame();
    }

    private void OnReturnToMainGame()
    {
        // Return to main game UI
        Debug.Log("[LootRewardsUI] Returning to main game...");
        SceneManager.LoadScene("MainGameUI");
    }
    
    /// <summary>
    /// Public method to manually show rewards (for testing)
    /// </summary>
    public void ShowRewardsManually(LootDropResult loot)
    {
        currentLoot = loot;
        DisplayRewards(loot);
        
        if (rewardsPanel != null)
        {
            rewardsPanel.SetActive(true);
        }
    }
}


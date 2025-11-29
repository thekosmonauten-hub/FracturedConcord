using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Dexiled.MazeSystem
{
    /// <summary>
    /// Component for difficulty selection buttons in the Maze Hub.
    /// Displays difficulty information and handles visual feedback.
    /// </summary>
    public class MazeDifficultyButton : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI requirementsText;
        public TextMeshProUGUI costText;
        public TextMeshProUGUI firstClearBonusText;
        public Image iconImage;
        public Image backgroundImage;
        public GameObject recommendedBadge;
        
        [Header("First Clear Badges")]
        [Tooltip("Badge shown when first clear bonus is available (not cleared yet)")]
        public GameObject firstClearBadge; // Badge to show if first clear bonus is available
        
        [Tooltip("Badge shown when difficulty has been cleared (optional)")]
        public GameObject clearedBadge; // Badge to show if difficulty has been cleared
        
        private MazeDifficultyConfig difficultyConfig;
        
        /// <summary>
        /// Sets the difficulty configuration for this button.
        /// </summary>
        public void SetDifficulty(MazeDifficultyConfig difficulty)
        {
            difficultyConfig = difficulty;
            
            if (difficulty == null)
                return;
            
            // Update UI elements
            if (nameText != null)
                nameText.text = difficulty.difficultyName;
            
            if (descriptionText != null)
                descriptionText.text = difficulty.description;
            
            if (iconImage != null && difficulty.difficultyIcon != null)
                iconImage.sprite = difficulty.difficultyIcon;
            
            // Set background color
            if (backgroundImage != null)
                backgroundImage.color = difficulty.difficultyColor;
            
            // Show recommended badge
            if (recommendedBadge != null)
                recommendedBadge.SetActive(difficulty.isRecommended);
            
            // Update requirements text
            if (requirementsText != null)
            {
                string reqText = $"Level {difficulty.requiredLevel}";
                requirementsText.text = reqText;
            }
            
            // Update cost text
            if (costText != null)
            {
                if (difficulty.entryCost > 0)
                {
                    costText.text = $"{difficulty.entryCost} {difficulty.entryCurrencyType}";
                    costText.gameObject.SetActive(true);
                }
                else
                {
                    costText.gameObject.SetActive(false);
                }
            }
            
            // Update first clear bonus display
            UpdateFirstClearBonusDisplay(difficulty);
        }
        
        /// <summary>
        /// Updates the first clear bonus display based on whether this difficulty has been cleared.
        /// </summary>
        private void UpdateFirstClearBonusDisplay(MazeDifficultyConfig difficulty)
        {
            bool hasFirstClearBonus = HasFirstClearBonus(difficulty);
            bool hasBeenCleared = !hasFirstClearBonus;
            
            // Show/hide first clear badge (not cleared yet)
            if (firstClearBadge != null)
            {
                firstClearBadge.SetActive(hasFirstClearBonus);
            }
            
            // Show/hide cleared badge (already completed)
            if (clearedBadge != null)
            {
                clearedBadge.SetActive(hasBeenCleared);
            }
            
            // Update first clear bonus text
            if (firstClearBonusText != null)
            {
                if (hasFirstClearBonus)
                {
                    string bonusText = $"+{difficulty.firstClearAttunementPoints} Attunement";
                    if (difficulty.firstClearCurrencyBonus > 0)
                    {
                        bonusText += $"\n+{difficulty.firstClearCurrencyBonus} {difficulty.firstClearCurrencyType}";
                    }
                    firstClearBonusText.text = $"First Clear:\n{bonusText}";
                    firstClearBonusText.gameObject.SetActive(true);
                }
                else
                {
                    firstClearBonusText.gameObject.SetActive(false);
                }
            }
        }
        
        /// <summary>
        /// Checks if this difficulty has a first clear bonus available (hasn't been cleared yet).
        /// </summary>
        private bool HasFirstClearBonus(MazeDifficultyConfig difficulty)
        {
            if (difficulty == null) return false;
            
            // Check if this difficulty has been cleared before
            // Store in PlayerPrefs with key: "MazeDifficulty_Cleared_[DifficultyName]"
            string key = $"MazeDifficulty_Cleared_{difficulty.difficultyName}";
            bool hasBeenCleared = PlayerPrefs.GetInt(key, 0) == 1;
            
            return !hasBeenCleared;
        }
        
        /// <summary>
        /// Gets the difficulty configuration for this button.
        /// </summary>
        public MazeDifficultyConfig GetDifficulty()
        {
            return difficultyConfig;
        }
    }
}



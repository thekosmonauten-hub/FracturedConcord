using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PassiveTree.UI
{
    /// <summary>
    /// UI component for confirming board selection before placement
    /// </summary>
    public class BoardConfirmationUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject confirmationPanel;
        [SerializeField] private TextMeshProUGUI boardNameText;
        [SerializeField] private TextMeshProUGUI boardDescriptionText;
        [SerializeField] private Image boardPreviewImage;
        [SerializeField] private Button placeBoardButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button backButton;
        
        [Header("Board Summary")]
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI notablesText;
        [SerializeField] private ScrollRect statsScrollRect;
        [SerializeField] private ScrollRect notablesScrollRect;
        
        [Header("Styling")]
        [SerializeField] private Color confirmButtonColor = Color.green;
        [SerializeField] private Color cancelButtonColor = Color.red;
        [SerializeField] private Color backButtonColor = Color.gray;
        
        // Current board data
        private BoardData selectedBoardData;
        private System.Action<BoardData> onBoardConfirmed;
        private System.Action onBoardCancelled;
        
        private void Awake()
        {
            // Setup button events
            if (placeBoardButton != null)
            {
                placeBoardButton.onClick.AddListener(OnPlaceBoardClicked);
            }
            
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelClicked);
            }
            
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }
            
            // Hide panel initially
            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// Show board confirmation UI for the selected board
        /// </summary>
        public void ShowBoardConfirmation(BoardData boardData, System.Action<BoardData> onConfirmed, System.Action onCancelled)
        {
            if (boardData == null)
            {
                Debug.LogWarning("[BoardConfirmationUI] Cannot show confirmation for null board data");
                return;
            }
            
            selectedBoardData = boardData;
            onBoardConfirmed = onConfirmed;
            onBoardCancelled = onCancelled;
            
            // Update UI elements
            UpdateBoardInfo();
            UpdateBoardPreview();
            UpdateBoardSummary();
            
            // Show the panel
            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(true);
            }
            
            Debug.Log($"[BoardConfirmationUI] Showing confirmation for {boardData.BoardName}");
        }
        
        /// <summary>
        /// Hide the board confirmation UI
        /// </summary>
        public void HideBoardConfirmation()
        {
            selectedBoardData = null;
            onBoardConfirmed = null;
            onBoardCancelled = null;
            
            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(false);
            }
            
            Debug.Log("[BoardConfirmationUI] Hiding board confirmation");
        }
        
        /// <summary>
        /// Update basic board information
        /// </summary>
        private void UpdateBoardInfo()
        {
            if (selectedBoardData == null) return;
            
            if (boardNameText != null)
            {
                boardNameText.text = selectedBoardData.BoardName;
            }
            
            if (boardDescriptionText != null)
            {
                boardDescriptionText.text = selectedBoardData.BoardDescription;
            }
        }
        
        /// <summary>
        /// Update board preview image
        /// </summary>
        private void UpdateBoardPreview()
        {
            if (selectedBoardData == null || boardPreviewImage == null) return;
            
            if (selectedBoardData.BoardPreview != null)
            {
                boardPreviewImage.sprite = selectedBoardData.BoardPreview;
                boardPreviewImage.gameObject.SetActive(true);
            }
            else
            {
                boardPreviewImage.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Update board summary with stats and notables
        /// </summary>
        private void UpdateBoardSummary()
        {
            if (selectedBoardData == null) return;
            
            // Calculate and display stats
            if (statsText != null)
            {
                var totalStats = CalculateTotalStats(selectedBoardData);
                string statsDisplay = "Board Stats:\n";
                
                foreach (var stat in totalStats)
                {
                    if (stat.Value != 0)
                    {
                        string sign = stat.Value > 0 ? "+" : "";
                        statsDisplay += $"{stat.Key}: {sign}{stat.Value}%\n";
                    }
                }
                
                if (totalStats.Count == 0)
                {
                    statsDisplay += "No stat bonuses found.";
                }
                
                statsText.text = statsDisplay;
            }
            
            // Display notable effects
            if (notablesText != null)
            {
                var notables = ExtractNotableEffects(selectedBoardData);
                string notablesDisplay = "Notable Effects:\n";
                
                if (notables.Count > 0)
                {
                    foreach (var notable in notables)
                    {
                        notablesDisplay += $"• {notable}\n";
                    }
                }
                else
                {
                    notablesDisplay += "No notable effects found.";
                }
                
                notablesText.text = notablesDisplay;
            }
        }
        
        /// <summary>
        /// Handle place board button click
        /// </summary>
        private void OnPlaceBoardClicked()
        {
            if (selectedBoardData != null && onBoardConfirmed != null)
            {
                Debug.Log($"[BoardConfirmationUI] Board confirmed: {selectedBoardData.BoardName}");
                onBoardConfirmed.Invoke(selectedBoardData);
            }
            
            HideBoardConfirmation();
        }
        
        /// <summary>
        /// Handle cancel button click
        /// </summary>
        private void OnCancelClicked()
        {
            Debug.Log("[BoardConfirmationUI] Board selection cancelled");
            
            if (onBoardCancelled != null)
            {
                onBoardCancelled.Invoke();
            }
            
            HideBoardConfirmation();
        }
        
        /// <summary>
        /// Handle back button click (return to board selection)
        /// </summary>
        private void OnBackClicked()
        {
            Debug.Log("[BoardConfirmationUI] Returning to board selection");
            
            if (onBoardCancelled != null)
            {
                onBoardCancelled.Invoke();
            }
            
            HideBoardConfirmation();
        }
        
        /// <summary>
        /// Calculate total stats from board JSON data
        /// </summary>
        private System.Collections.Generic.Dictionary<string, float> CalculateTotalStats(BoardData boardData)
        {
            var totalStats = new System.Collections.Generic.Dictionary<string, float>();
            
            try
            {
                var boardJson = boardData.GetJsonDataText();
                if (string.IsNullOrEmpty(boardJson))
                {
                    return totalStats;
                }
                
                // Simplified stat parsing - you might need to adjust based on your JSON structure
                var statPatterns = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "Fire Damage", "fire_damage|fireDamage|fire_damage_percent" },
                    { "Intelligence", "intelligence|int|int_stat" },
                    { "Ignite Magnitude", "ignite_magnitude|igniteMagnitude|ignite_magnitude_percent" },
                    { "Health", "health|hp|health_percent" },
                    { "Mana", "mana|mp|mana_percent" },
                    { "Armor", "armor|armour|armor_percent" },
                    { "Resistance", "resistance|resist|resistance_percent" }
                };
                
                foreach (var statPattern in statPatterns)
                {
                    float totalValue = 0f;
                    string pattern = statPattern.Value;
                    
                    if (boardJson.ToLower().Contains(pattern.ToLower()))
                    {
                        var matches = System.Text.RegularExpressions.Regex.Matches(boardJson, @"(\d+(?:\.\d+)?)");
                        foreach (System.Text.RegularExpressions.Match match in matches)
                        {
                            if (float.TryParse(match.Value, out float value))
                            {
                                totalValue += value;
                            }
                        }
                    }
                    
                    if (totalValue != 0)
                    {
                        totalStats[statPattern.Key] = totalValue;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BoardConfirmationUI] Error calculating stats: {e.Message}");
            }
            
            return totalStats;
        }
        
        /// <summary>
        /// Extract notable effects from board data
        /// </summary>
        private System.Collections.Generic.List<string> ExtractNotableEffects(BoardData boardData)
        {
            var notables = new System.Collections.Generic.List<string>();
            
            try
            {
                var boardJson = boardData.GetJsonDataText();
                if (string.IsNullOrEmpty(boardJson))
                {
                    return notables;
                }
                
                var notableKeywords = new[] { "notable", "keystone", "special", "unique" };
                
                foreach (var keyword in notableKeywords)
                {
                    if (boardJson.ToLower().Contains(keyword))
                    {
                        notables.Add($"Special {keyword} effect");
                    }
                }
                
                if (notables.Count == 0)
                {
                    notables.Add("Enhanced abilities");
                    notables.Add("Improved resistance");
                    notables.Add("Increased damage");
                    notables.Add("Better efficiency");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BoardConfirmationUI] Error extracting notables: {e.Message}");
            }
            
            return notables;
        }
        
        /// <summary>
        /// Check if confirmation UI is currently visible
        /// </summary>
        public bool IsVisible => confirmationPanel != null && confirmationPanel.activeInHierarchy;
        
        /// <summary>
        /// Get the currently selected board data
        /// </summary>
        public BoardData SelectedBoardData => selectedBoardData;
    }
}














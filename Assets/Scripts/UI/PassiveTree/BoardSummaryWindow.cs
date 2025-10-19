using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace PassiveTree.UI
{
    /// <summary>
    /// UI component that displays a summary of board stats and notables when hovering over board selection buttons
    /// </summary>
    public class BoardSummaryWindow : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject summaryPanel;
        [SerializeField] private TextMeshProUGUI boardNameText;
        [SerializeField] private TextMeshProUGUI boardTierText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI notablesText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        
        [Header("Layout")]
        [SerializeField] private ScrollRect statsScrollRect;
        [SerializeField] private ScrollRect notablesScrollRect;
        
        [Header("Styling")]
        [SerializeField] private Color statColor = Color.white;
        [SerializeField] private Color notableColor = Color.yellow;
        [SerializeField] private Color descriptionColor = Color.gray;
        
        private bool isVisible = false;
        private BoardData currentBoardData;
        
        private void Awake()
        {
            if (summaryPanel != null)
            {
                summaryPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// Show the board summary for the specified board data
        /// </summary>
        public void ShowBoardSummary(BoardData boardData)
        {
            if (boardData == null)
            {
                Debug.LogWarning("[BoardSummaryWindow] Cannot show summary for null board data");
                return;
            }
            
            currentBoardData = boardData;
            isVisible = true;
            
            // Update UI elements
            UpdateBoardInfo();
            UpdateStatsSummary();
            UpdateNotablesSummary();
            UpdateDescription();
            
            // Show the panel
            if (summaryPanel != null)
            {
                summaryPanel.SetActive(true);
            }
            
            Debug.Log($"[BoardSummaryWindow] Showing summary for {boardData.BoardName}");
        }
        
        /// <summary>
        /// Hide the board summary
        /// </summary>
        public void HideBoardSummary()
        {
            isVisible = false;
            currentBoardData = null;
            
            if (summaryPanel != null)
            {
                summaryPanel.SetActive(false);
            }
            
            Debug.Log("[BoardSummaryWindow] Hiding board summary");
        }
        
        /// <summary>
        /// Update basic board information
        /// </summary>
        private void UpdateBoardInfo()
        {
            if (currentBoardData == null) return;
            
            if (boardNameText != null)
            {
                boardNameText.text = currentBoardData.BoardName;
                boardNameText.color = statColor;
            }
            
            if (boardTierText != null)
            {
                boardTierText.text = $"Tier {currentBoardData.RequiredLevel}";
                boardTierText.color = statColor;
            }
        }
        
        /// <summary>
        /// Calculate and display total stats from all nodes in the board
        /// </summary>
        private void UpdateStatsSummary()
        {
            if (currentBoardData == null || statsText == null) return;
            
            // Parse the board JSON to calculate total stats
            var totalStats = CalculateTotalStats(currentBoardData);
            
            // Format the stats text
            string statsDisplay = "Overall Stats:\n";
            
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
            statsText.color = statColor;
        }
        
        /// <summary>
        /// Display notable effects from the board
        /// </summary>
        private void UpdateNotablesSummary()
        {
            if (currentBoardData == null || notablesText == null) return;
            
            // Parse the board JSON to find notable effects
            var notables = ExtractNotableEffects(currentBoardData);
            
            // Format the notables text
            string notablesDisplay = "Notables:\n";
            
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
            notablesText.color = notableColor;
        }
        
        /// <summary>
        /// Update the board description
        /// </summary>
        private void UpdateDescription()
        {
            if (currentBoardData == null || descriptionText == null) return;
            
            string description = currentBoardData.BoardDescription ?? "No description available.";
            descriptionText.text = description;
            descriptionText.color = descriptionColor;
        }
        
        /// <summary>
        /// Calculate total stats from all nodes in the board
        /// </summary>
        private Dictionary<string, float> CalculateTotalStats(BoardData boardData)
        {
            var totalStats = new Dictionary<string, float>();
            
            try
            {
                // Parse the board JSON to extract node data
                var boardJson = boardData.GetJsonDataText();
                if (string.IsNullOrEmpty(boardJson))
                {
                    Debug.LogWarning($"[BoardSummaryWindow] No board JSON data for {boardData.BoardName}");
                    return totalStats;
                }
                
                // This is a simplified parser - you might need to adjust based on your JSON structure
                // For now, we'll look for common stat patterns in the JSON
                var statPatterns = new Dictionary<string, string>
                {
                    { "Fire Damage", "fire_damage|fireDamage|fire_damage_percent" },
                    { "Intelligence", "intelligence|int|int_stat" },
                    { "Ignite Magnitude", "ignite_magnitude|igniteMagnitude|ignite_magnitude_percent" },
                    { "Health", "health|hp|health_percent" },
                    { "Mana", "mana|mp|mana_percent" },
                    { "Armor", "armor|armour|armor_percent" },
                    { "Resistance", "resistance|resist|resistance_percent" }
                };
                
                // Search for stat values in the JSON
                foreach (var statPattern in statPatterns)
                {
                    float totalValue = 0f;
                    string pattern = statPattern.Value;
                    
                    // Look for numeric values associated with this stat
                    // This is a simplified approach - you might need more sophisticated JSON parsing
                    if (boardJson.ToLower().Contains(pattern.ToLower()))
                    {
                        // Extract numeric values (simplified - you might need a proper JSON parser)
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
                Debug.LogError($"[BoardSummaryWindow] Error calculating stats for {boardData.BoardName}: {e.Message}");
            }
            
            return totalStats;
        }
        
        /// <summary>
        /// Extract notable effects from the board
        /// </summary>
        private List<string> ExtractNotableEffects(BoardData boardData)
        {
            var notables = new List<string>();
            
            try
            {
                var boardJson = boardData.GetJsonDataText();
                if (string.IsNullOrEmpty(boardJson))
                {
                    return notables;
                }
                
                // Look for notable effects in the JSON
                // This is a simplified approach - you might need more sophisticated parsing
                var notableKeywords = new[] { "notable", "keystone", "special", "unique" };
                
                foreach (var keyword in notableKeywords)
                {
                    if (boardJson.ToLower().Contains(keyword))
                    {
                        // Extract notable descriptions (simplified)
                        notables.Add($"Special {keyword} effect");
                    }
                }
                
                // If no notables found, add some placeholder text
                if (notables.Count == 0)
                {
                    notables.Add("Enhanced fire-based abilities");
                    notables.Add("Improved elemental resistance");
                    notables.Add("Increased spell damage");
                    notables.Add("Better mana efficiency");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BoardSummaryWindow] Error extracting notables for {boardData.BoardName}: {e.Message}");
            }
            
            return notables;
        }
        
        /// <summary>
        /// Check if the summary window is currently visible
        /// </summary>
        public bool IsVisible => isVisible;
        
        /// <summary>
        /// Get the current board data being displayed
        /// </summary>
        public BoardData CurrentBoardData => currentBoardData;
    }
}

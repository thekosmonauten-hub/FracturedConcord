using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Test script for verifying CharacterStatsPanel integration
/// Add this to any GameObject in the scene for testing
/// </summary>
public class CharacterStatsPanelTest : MonoBehaviour
{
    [Header("Test Controls")]
    public Button testToggleButton;
    public Button testShowButton;
    public Button testHideButton;
    public Button testRefreshButton;
    
    [Header("Test Character")]
    public string testCharacterName = "TestCharacter";
    public string testCharacterClass = "Witch";
    
    private void Start()
    {
        SetupTestButtons();
        CreateTestCharacter();
    }
    
    private void SetupTestButtons()
    {
        // Create test toggle button if not assigned
        if (testToggleButton == null)
        {
            CreateTestButton("Test Toggle", testToggleButton, () => {
                if (CharacterStatsPanelManager.Instance != null)
                {
                    CharacterStatsPanelManager.Instance.TogglePanel();
                }
                else
                {
                    Debug.LogWarning("CharacterStatsPanelManager not found!");
                }
            });
        }
        else
        {
            testToggleButton.onClick.AddListener(() => {
                if (CharacterStatsPanelManager.Instance != null)
                {
                    CharacterStatsPanelManager.Instance.TogglePanel();
                }
            });
        }
        
        // Create test show button if not assigned
        if (testShowButton == null)
        {
            CreateTestButton("Test Show", testShowButton, () => {
                if (CharacterStatsPanelManager.Instance != null)
                {
                    CharacterStatsPanelManager.Instance.ShowPanel();
                }
            });
        }
        else
        {
            testShowButton.onClick.AddListener(() => {
                if (CharacterStatsPanelManager.Instance != null)
                {
                    CharacterStatsPanelManager.Instance.ShowPanel();
                }
            });
        }
        
        // Create test hide button if not assigned
        if (testHideButton == null)
        {
            CreateTestButton("Test Hide", testHideButton, () => {
                if (CharacterStatsPanelManager.Instance != null)
                {
                    CharacterStatsPanelManager.Instance.HidePanel();
                }
            });
        }
        else
        {
            testHideButton.onClick.AddListener(() => {
                if (CharacterStatsPanelManager.Instance != null)
                {
                    CharacterStatsPanelManager.Instance.HidePanel();
                }
            });
        }
        
        // Create test refresh button if not assigned
        if (testRefreshButton == null)
        {
            CreateTestButton("Test Refresh", testRefreshButton, () => {
                if (CharacterStatsPanelManager.Instance != null)
                {
                    CharacterStatsPanelManager.Instance.RefreshPanelData();
                }
            });
        }
        else
        {
            testRefreshButton.onClick.AddListener(() => {
                if (CharacterStatsPanelManager.Instance != null)
                {
                    CharacterStatsPanelManager.Instance.RefreshPanelData();
                }
            });
        }
    }
    
    private void CreateTestButton(string buttonName, Button buttonRef, System.Action onClick)
    {
        // Create a test button GameObject
        GameObject buttonObj = new GameObject(buttonName);
        buttonObj.transform.SetParent(transform);
        
        // Add required components
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        CanvasRenderer canvasRenderer = buttonObj.AddComponent<CanvasRenderer>();
        Image image = buttonObj.AddComponent<Image>();
        Button button = buttonObj.AddComponent<Button>();
        
        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        Text text = textObj.AddComponent<Text>();
        text.text = buttonName;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 14;
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleCenter;
        
        // Position text
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Position button
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(120, 30);
        rectTransform.anchoredPosition = new Vector2(10, 10);
        
        // Setup button
        button.onClick.AddListener(() => onClick());
        
        // Assign reference
        buttonRef = button;
        
        Debug.Log($"Created test button: {buttonName}");
    }
    
    private void CreateTestCharacter()
    {
        if (CharacterManager.Instance == null)
        {
            Debug.LogWarning("CharacterManager not found! Cannot create test character.");
            return;
        }
        
        // Check if character already exists
        if (CharacterManager.Instance.HasCharacter())
        {
            Debug.Log("Character already exists, skipping test character creation.");
            return;
        }
        
        // Create a test character using CharacterManager
        CharacterManager.Instance.CreateCharacter(testCharacterName, testCharacterClass);
        
        // Get the created character and modify its stats for testing
        Character testCharacter = CharacterManager.Instance.GetCurrentCharacter();
        if (testCharacter != null)
        {
            testCharacter.level = 5;
            testCharacter.experience = 1500;
            testCharacter.currentHealth = 80;
            testCharacter.mana = 2;
            testCharacter.reliance = 150;
            
            // Save the modified character
            CharacterManager.Instance.SaveCharacter();
            
            Debug.Log($"Created test character: {testCharacterName} ({testCharacterClass})");
        }
        else
        {
            Debug.LogError("Failed to create test character!");
        }
    }
    
    [ContextMenu("Test Panel Toggle")]
    public void TestPanelToggle()
    {
        if (CharacterStatsPanelManager.Instance != null)
        {
            CharacterStatsPanelManager.Instance.TogglePanel();
        }
        else
        {
            Debug.LogWarning("CharacterStatsPanelManager not found!");
        }
    }
    
    [ContextMenu("Test Panel Show")]
    public void TestPanelShow()
    {
        if (CharacterStatsPanelManager.Instance != null)
        {
            CharacterStatsPanelManager.Instance.ShowPanel();
        }
        else
        {
            Debug.LogWarning("CharacterStatsPanelManager not found!");
        }
    }
    
    [ContextMenu("Test Panel Hide")]
    public void TestPanelHide()
    {
        if (CharacterStatsPanelManager.Instance != null)
        {
            CharacterStatsPanelManager.Instance.HidePanel();
        }
        else
        {
            Debug.LogWarning("CharacterStatsPanelManager not found!");
        }
    }
    
    [ContextMenu("Test Panel Refresh")]
    public void TestPanelRefresh()
    {
        if (CharacterStatsPanelManager.Instance != null)
        {
            CharacterStatsPanelManager.Instance.RefreshPanelData();
        }
        else
        {
            Debug.LogWarning("CharacterStatsPanelManager not found!");
        }
    }
    
    [ContextMenu("Print Debug Info")]
    public void PrintDebugInfo()
    {
        Debug.Log("=== CharacterStatsPanel Debug Info ===");
        
        // Check CharacterStatsPanelManager
        if (CharacterStatsPanelManager.Instance != null)
        {
            Debug.Log("✓ CharacterStatsPanelManager found");
            Debug.Log($"  Panel visible: {CharacterStatsPanelManager.Instance.IsPanelVisible()}");
            Debug.Log($"  Panel assigned: {CharacterStatsPanelManager.Instance.characterStatsPanel != null}");
            Debug.Log($"  Controller assigned: {CharacterStatsPanelManager.Instance.statsController != null}");
        }
        else
        {
            Debug.LogWarning("✗ CharacterStatsPanelManager not found");
        }
        
        // Check CharacterManager
        if (CharacterManager.Instance != null)
        {
            Debug.Log("✓ CharacterManager found");
            Debug.Log($"  Has character: {CharacterManager.Instance.HasCharacter()}");
            
            if (CharacterManager.Instance.HasCharacter())
            {
                Character character = CharacterManager.Instance.GetCurrentCharacter();
                Debug.Log($"  Character name: {character.characterName}");
                Debug.Log($"  Character class: {character.characterClass}");
                Debug.Log($"  Character level: {character.level}");
            }
        }
        else
        {
            Debug.LogWarning("✗ CharacterManager not found");
        }
        
        // Check UIManager
        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            Debug.Log("✓ UIManager found");
            Debug.Log($"  CharacterStatsPanel assigned: {uiManager.CharacterStatsPanel != null}");
        }
        else
        {
            Debug.LogWarning("✗ UIManager not found");
        }
        
        Debug.Log("=== End Debug Info ===");
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays current and max Reliance with a filled image bar
/// Shows in DynamicArea/Auras header
/// </summary>
public class RelianceDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI relianceText; // "Reliance" label
    [SerializeField] private TextMeshProUGUI relianceCounterText; // "CurrentReliance/MaxReliance" counter
    [SerializeField] private Image relianceFillImage; // Filled image type bound to CurrentReliance
    
    [Header("Settings")]
    [SerializeField] private bool updateOnEnable = true;
    [SerializeField] private float updateInterval = 0.1f; // Update every 0.1 seconds
    
    private CharacterManager characterManager;
    private float lastUpdateTime = 0f;
    
    void Start()
    {
        characterManager = CharacterManager.Instance;
        
        // Ensure fill image is set to Filled type
        if (relianceFillImage != null)
        {
            relianceFillImage.type = Image.Type.Filled;
            relianceFillImage.fillMethod = Image.FillMethod.Horizontal; // Or Vertical, depending on design
        }
        
        UpdateDisplay();
    }
    
    void OnEnable()
    {
        if (updateOnEnable)
        {
            UpdateDisplay();
        }
    }
    
    void Update()
    {
        // Update periodically (not every frame for performance)
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateDisplay();
            lastUpdateTime = Time.time;
        }
    }
    
    /// <summary>
    /// Update the reliance display
    /// </summary>
    public void UpdateDisplay()
    {
        if (characterManager == null)
        {
            characterManager = CharacterManager.Instance;
        }
        
        if (characterManager == null || !characterManager.HasCharacter())
        {
            // Show default/empty values
            if (relianceCounterText != null)
            {
                relianceCounterText.text = "0/0";
            }
            
            if (relianceFillImage != null)
            {
                relianceFillImage.fillAmount = 0f;
            }
            return;
        }
        
        Character character = characterManager.GetCurrentCharacter();
        int currentReliance = character.reliance;
        int maxReliance = character.maxReliance;
        
        // Update counter text
        if (relianceCounterText != null)
        {
            relianceCounterText.text = $"{currentReliance}/{maxReliance}";
        }
        
        // Update fill image
        if (relianceFillImage != null && maxReliance > 0)
        {
            float fillAmount = (float)currentReliance / (float)maxReliance;
            relianceFillImage.fillAmount = Mathf.Clamp01(fillAmount);
        }
    }
    
    /// <summary>
    /// Force an immediate update
    /// </summary>
    public void Refresh()
    {
        UpdateDisplay();
    }
}


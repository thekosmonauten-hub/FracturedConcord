using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Component for floating damage number prefabs.
/// Attach this to a UI Text element to create reusable damage numbers.
/// </summary>
[RequireComponent(typeof(Text))]
[RequireComponent(typeof(RectTransform))]
public class FloatingDamageNumber : MonoBehaviour
{
    private Text damageText;
    private RectTransform rectTransform;
    private Outline outline;
    
    private void Awake()
    {
        damageText = GetComponent<Text>();
        rectTransform = GetComponent<RectTransform>();
        
        // Add outline if not present
        outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, 2);
        }
        
        // Set up default text properties
        if (damageText != null)
        {
            damageText.alignment = TextAnchor.MiddleCenter;
            damageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        
        // Set up rect transform
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(100, 50);
        }
    }
    
    /// <summary>
    /// Set the damage number value and appearance
    /// </summary>
    public void SetDamage(float damage, Color color, int fontSize)
    {
        if (damageText != null)
        {
            damageText.text = Mathf.RoundToInt(damage).ToString();
            damageText.color = color;
            damageText.fontSize = fontSize;
        }
    }
    
    /// <summary>
    /// Reset for object pooling
    /// </summary>
    public void ResetState()
    {
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        
        if (damageText != null)
        {
            Color color = damageText.color;
            color.a = 1f;
            damageText.color = color;
        }
    }
}


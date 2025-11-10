using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Fixes text clipping issues in rotated InputFields by adjusting viewport padding.
/// Attach this to any InputField (legacy or TMP) that is rotated.
/// </summary>
[ExecuteInEditMode]
public class RotatedInputFieldFixer : MonoBehaviour
{
    [Header("Padding Adjustment")]
    [Tooltip("Extra padding to prevent text clipping when rotated")]
    [SerializeField] private float extraPadding = 20f;
    
    [Header("Masking Options")]
    [Tooltip("Disable RectMask2D entirely (if you don't need text overflow masking)")]
    [SerializeField] private bool disableMasking = false;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    private RectTransform textArea;
    private RectMask2D rectMask;
    private bool hasAppliedFix = false;
    
    void Start()
    {
        ApplyFix();
    }
    
    void OnValidate()
    {
        // Apply fix in editor when values change
        if (Application.isPlaying) return;
        ApplyFix();
    }
    
    [ContextMenu("Apply Fix")]
    public void ApplyFix()
    {
        // Try to find TMP_InputField first
        TMP_InputField tmpInputField = GetComponent<TMP_InputField>();
        if (tmpInputField != null)
        {
            FixTMPInputField(tmpInputField);
            return;
        }
        
        // Try legacy InputField
        InputField legacyInputField = GetComponent<InputField>();
        if (legacyInputField != null)
        {
            FixLegacyInputField(legacyInputField);
            return;
        }
        
        Debug.LogError($"[RotatedInputFieldFixer] No InputField or TMP_InputField found on '{gameObject.name}'!");
    }
    
    void FixTMPInputField(TMP_InputField tmpInputField)
    {
        if (tmpInputField.textViewport == null)
        {
            Debug.LogError($"[RotatedInputFieldFixer] TMP_InputField '{gameObject.name}' has no textViewport assigned!");
            return;
        }
        
        textArea = tmpInputField.textViewport;
        
        // Adjust viewport padding
        textArea.offsetMin = new Vector2(-extraPadding, -extraPadding); // Left, Bottom
        textArea.offsetMax = new Vector2(extraPadding, extraPadding);   // Right, Top
        
        // Handle masking
        rectMask = textArea.GetComponent<RectMask2D>();
        if (rectMask != null && disableMasking)
        {
            rectMask.enabled = false;
            if (showDebugLogs)
            {
                Debug.Log($"[RotatedInputFieldFixer] Disabled RectMask2D on '{gameObject.name}'");
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[RotatedInputFieldFixer] Applied fix to TMP_InputField '{gameObject.name}' with padding: {extraPadding}");
        }
        
        hasAppliedFix = true;
    }
    
    void FixLegacyInputField(InputField legacyInputField)
    {
        // Find "Text" child (viewport)
        Transform textAreaTransform = transform.Find("Text");
        if (textAreaTransform == null)
        {
            Debug.LogError($"[RotatedInputFieldFixer] Legacy InputField '{gameObject.name}' has no 'Text' child!");
            return;
        }
        
        textArea = textAreaTransform.GetComponent<RectTransform>();
        
        // Adjust viewport padding
        textArea.offsetMin = new Vector2(-extraPadding, -extraPadding);
        textArea.offsetMax = new Vector2(extraPadding, extraPadding);
        
        // Handle masking
        rectMask = textArea.GetComponent<RectMask2D>();
        if (rectMask != null && disableMasking)
        {
            rectMask.enabled = false;
            if (showDebugLogs)
            {
                Debug.Log($"[RotatedInputFieldFixer] Disabled RectMask2D on '{gameObject.name}'");
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[RotatedInputFieldFixer] Applied fix to Legacy InputField '{gameObject.name}' with padding: {extraPadding}");
        }
        
        hasAppliedFix = true;
    }
    
    [ContextMenu("Reset Padding")]
    public void ResetPadding()
    {
        if (textArea != null)
        {
            textArea.offsetMin = Vector2.zero;
            textArea.offsetMax = Vector2.zero;
            
            if (showDebugLogs)
            {
                Debug.Log($"[RotatedInputFieldFixer] Reset padding on '{gameObject.name}'");
            }
        }
    }
    
    [ContextMenu("Toggle Masking")]
    public void ToggleMasking()
    {
        if (rectMask != null)
        {
            rectMask.enabled = !rectMask.enabled;
            disableMasking = !rectMask.enabled;
            
            if (showDebugLogs)
            {
                Debug.Log($"[RotatedInputFieldFixer] Masking {(rectMask.enabled ? "ENABLED" : "DISABLED")} on '{gameObject.name}'");
            }
        }
    }
}




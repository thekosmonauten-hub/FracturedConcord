using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Simple confirmation dialog for destructive actions (delete, overwrite, etc.)
/// Usage: SimpleConfirmationDialog.Show("Delete Deck?", "Are you sure?", onConfirm: () => DeleteDeck());
/// </summary>
public class SimpleConfirmationDialog : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI confirmButtonText;
    [SerializeField] private TextMeshProUGUI cancelButtonText;
    
    private static SimpleConfirmationDialog _instance;
    private Action onConfirmCallback;
    private Action onCancelCallback;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        
        // Setup button listeners
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }
        
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelClicked);
        }
        
        // Hide dialog initially
        Hide();
    }
    
    /// <summary>
    /// Show confirmation dialog with custom message.
    /// </summary>
    public static void Show(
        string title, 
        string message, 
        Action onConfirm = null, 
        Action onCancel = null,
        string confirmText = "Confirm",
        string cancelText = "Cancel")
    {
        if (_instance == null)
        {
            Debug.LogError("SimpleConfirmationDialog instance not found in scene!");
            return;
        }
        
        _instance.ShowDialog(title, message, onConfirm, onCancel, confirmText, cancelText);
    }
    
    private void ShowDialog(
        string title, 
        string message, 
        Action onConfirm, 
        Action onCancel,
        string confirmText,
        string cancelText)
    {
        // Set text
        if (titleText != null)
            titleText.text = title;
        
        if (messageText != null)
            messageText.text = message;
        
        if (confirmButtonText != null)
            confirmButtonText.text = confirmText;
        
        if (cancelButtonText != null)
            cancelButtonText.text = cancelText;
        
        // Store callbacks
        onConfirmCallback = onConfirm;
        onCancelCallback = onCancel;
        
        // Show dialog
        if (dialogPanel != null)
            dialogPanel.SetActive(true);
    }
    
    private void OnConfirmClicked()
    {
        Hide();
        onConfirmCallback?.Invoke();
    }
    
    private void OnCancelClicked()
    {
        Hide();
        onCancelCallback?.Invoke();
    }
    
    private void Hide()
    {
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
        
        // Clear callbacks
        onConfirmCallback = null;
        onCancelCallback = null;
    }
    
    /// <summary>
    /// Quick helper for delete confirmations.
    /// </summary>
    public static void ShowDeleteConfirmation(string itemName, Action onConfirm)
    {
        Show(
            title: "Delete Confirmation",
            message: $"Are you sure you want to permanently delete '{itemName}'?\n\nThis action cannot be undone.",
            onConfirm: onConfirm,
            confirmText: "Delete",
            cancelText: "Cancel"
        );
    }
}









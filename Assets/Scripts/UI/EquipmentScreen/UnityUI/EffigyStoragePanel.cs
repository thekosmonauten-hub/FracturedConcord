using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// Unity UI version of effigy storage panel with slide animation
/// Uses LeanTween for smooth sliding animation from the right
/// </summary>
public class EffigyStoragePanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform storageContent;
    [SerializeField] private GameObject effigySlotPrefab;
    
    [Header("Animation Settings")]
    [SerializeField] private float panelWidth = 400f;
    [SerializeField] private float animationDuration = 0.3f;
    
    private RectTransform panelRect;
    private bool isOpen = false;
    
    void Awake()
    {
        panelRect = GetComponent<RectTransform>();
        
        if (closeButton != null)
            closeButton.onClick.AddListener(() => SlideOut());
        
        // Start hidden off-screen
        gameObject.SetActive(false);
    }
    
    public void SlideIn()
    {
        if (isOpen) return;
        
        gameObject.SetActive(true);
        
        // Start position (off-screen right)
        panelRect.anchoredPosition = new Vector2(panelWidth, panelRect.anchoredPosition.y);
        
        // Animate to visible position
        LeanTween.cancel(gameObject);
        LeanTween.moveX(panelRect, 0, animationDuration)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() => {
                isOpen = true;
                Debug.Log("[EffigyStoragePanel] Slide in complete");
            });
        
        Debug.Log("[EffigyStoragePanel] Sliding in");
    }
    
    public void SlideOut(Action onComplete = null)
    {
        if (!isOpen && !gameObject.activeSelf) return;
        
        isOpen = false;
        
        // Animate off-screen
        LeanTween.cancel(gameObject);
        LeanTween.moveX(panelRect, panelWidth, animationDuration)
            .setEase(LeanTweenType.easeInQuad)
            .setOnComplete(() => {
                gameObject.SetActive(false);
                onComplete?.Invoke();
                Debug.Log("[EffigyStoragePanel] Slide out complete");
            });
        
        Debug.Log("[EffigyStoragePanel] Sliding out");
    }
    
    public void PopulateStorage(List<Effigy> effigies)
    {
        // Clear existing content
        foreach (Transform child in storageContent)
        {
            Destroy(child.gameObject);
        }
        
        // Create slots for each effigy
        foreach (Effigy effigy in effigies)
        {
            if (effigySlotPrefab != null)
            {
                GameObject slotObj = Instantiate(effigySlotPrefab, storageContent);
                
                // TODO: Set up effigy slot with icon, name, etc.
                // You'll want to create an EffigySlotUI component for this
                
                Debug.Log($"[EffigyStoragePanel] Added slot for {effigy.effigyName}");
            }
        }
    }
    
    public void Toggle()
    {
        if (isOpen || gameObject.activeSelf)
            SlideOut();
        else
            SlideIn();
    }
}


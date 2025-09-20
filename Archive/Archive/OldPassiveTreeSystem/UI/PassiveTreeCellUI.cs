using UnityEngine;
using UnityEngine.UI;
using PassiveTree;

/// <summary>
/// UI component for passive tree cells
/// Handles cell background sprites and positioning
/// </summary>
public class PassiveTreeCellUI : MonoBehaviour
{
    [Header("Cell References")]
    [SerializeField] private Image cellBackground;
    [SerializeField] private RectTransform cellRectTransform;
    
    [Header("Cell Settings")]
    [SerializeField] private bool useCustomCellSprites = true;
    [SerializeField] private Vector2Int gridPosition;
    [SerializeField] private BoardTheme cellTheme = BoardTheme.Utility;
    
    [Header("Debug")]
    [SerializeField] private bool logSpriteAssignments = false;
    
    private PassiveTreeSpriteManager spriteManager;
    private Sprite currentCellSprite;
    
    /// <summary>
    /// Initialize the cell with position and theme
    /// </summary>
    public void InitializeCell(Vector2Int position, BoardTheme theme, PassiveTreeSpriteManager manager = null)
    {
        gridPosition = position;
        cellTheme = theme;
        spriteManager = manager;
        
        if (logSpriteAssignments)
        {
            Debug.Log($"[PassiveTreeCellUI] Initialized cell at position {position} with theme {theme}");
        }
        
        UpdateCellSprite();
    }
    
    /// <summary>
    /// Update the cell sprite based on theme and sprite manager
    /// </summary>
    public void UpdateCellSprite()
    {
        if (!useCustomCellSprites || spriteManager == null)
        {
            if (logSpriteAssignments)
            {
                Debug.Log($"[PassiveTreeCellUI] Skipping cell sprite update - UseCustom: {useCustomCellSprites}, Manager: {(spriteManager != null ? "Valid" : "NULL")}");
            }
            return;
        }
        
        var newCellSprite = spriteManager.GetCellSprite(cellTheme);
        if (newCellSprite != null)
        {
            cellBackground.sprite = newCellSprite;
            currentCellSprite = newCellSprite;
            
            if (logSpriteAssignments)
            {
                Debug.Log($"[PassiveTreeCellUI] Applied cell sprite '{newCellSprite.name}' to cell at {gridPosition}");
                Debug.Log($"[PassiveTreeCellUI] Cell sprite properties - Size: {newCellSprite.rect.size}, PixelsPerUnit: {newCellSprite.pixelsPerUnit}");
                
                if (newCellSprite.texture != null)
                {
                    Debug.Log($"[PassiveTreeCellUI] Cell sprite texture - Format: {newCellSprite.texture.format}, Size: {newCellSprite.texture.width}x{newCellSprite.texture.height}");
                }
                
                Debug.Log($"[PassiveTreeCellUI] Cell background - Color: {cellBackground.color}, Material: {(cellBackground.material != null ? cellBackground.material.name : "None")}");
            }
        }
        else
        {
            if (logSpriteAssignments)
            {
                Debug.LogWarning($"[PassiveTreeCellUI] No cell sprite found for theme {cellTheme} at position {gridPosition}");
            }
        }
    }
    
    /// <summary>
    /// Set the sprite manager for this cell
    /// </summary>
    public void SetSpriteManager(PassiveTreeSpriteManager manager)
    {
        spriteManager = manager;
        UpdateCellSprite();
    }
    
    /// <summary>
    /// Set whether to use custom cell sprites
    /// </summary>
    public void SetUseCustomCellSprites(bool useCustom)
    {
        useCustomCellSprites = useCustom;
        UpdateCellSprite();
    }
    
    /// <summary>
    /// Get the current cell sprite
    /// </summary>
    public Sprite GetCurrentCellSprite() => currentCellSprite;
    
    /// <summary>
    /// Get the cell's grid position
    /// </summary>
    public Vector2Int GetGridPosition() => gridPosition;
    
    /// <summary>
    /// Get the cell's theme
    /// </summary>
    public BoardTheme GetCellTheme() => cellTheme;
    
    /// <summary>
    /// Get the cell background Image component
    /// </summary>
    public Image GetCellBackground() => cellBackground;
    
    [ContextMenu("Test Cell Sprite Rendering")]
    public void TestCellSpriteRendering()
    {
        if (cellBackground == null)
        {
            Debug.LogError("[PassiveTreeCellUI] Cell background is null!");
            return;
        }
        
        Debug.Log($"[PassiveTreeCellUI] === Cell Sprite Rendering Test ===");
        Debug.Log($"[PassiveTreeCellUI] Current sprite: {(cellBackground.sprite != null ? cellBackground.sprite.name : "NULL")}");
        Debug.Log($"[PassiveTreeCellUI] Image color: {cellBackground.color}");
        Debug.Log($"[PassiveTreeCellUI] Image alpha: {cellBackground.color.a}");
        Debug.Log($"[PassiveTreeCellUI] Image enabled: {cellBackground.enabled}");
        Debug.Log($"[PassiveTreeCellUI] Image type: {cellBackground.type}");
        
        // Test with a solid color to see if the image component works
        var originalColor = cellBackground.color;
        cellBackground.color = Color.blue;
        Debug.Log($"[PassiveTreeCellUI] Set cell background color to blue for testing");
        
        // Restore original color after 2 seconds
        StartCoroutine(RestoreColorAfterDelay(originalColor, 2f));
    }
    
    private System.Collections.IEnumerator RestoreColorAfterDelay(Color originalColor, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (cellBackground != null)
        {
            cellBackground.color = originalColor;
            Debug.Log($"[PassiveTreeCellUI] Restored original cell background color");
        }
    }
    
    [ContextMenu("Force Update Cell Sprite")]
    public void ForceUpdateCellSprite()
    {
        Debug.Log($"[PassiveTreeCellUI] Force updating cell sprite at position {gridPosition}");
        UpdateCellSprite();
    }
}

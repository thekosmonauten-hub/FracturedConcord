using UnityEngine;
using UnityEngine.UI;
using PassiveTree;

/// <summary>
/// Fixes cell sizes to match 128x128 pixel sprites for proper Canvas interaction
/// </summary>
public class CellSizeFixer : MonoBehaviour
{
    [Header("Cell Size Settings")]
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private bool useSpriteSize = true;
    [SerializeField] private Vector2 targetCellSize = new Vector2(128f, 128f); // 128x128 pixels to match sprite size
    [SerializeField] private float pixelsPerUnit = 100f; // Unity's default
    
    [Header("Fix Options")]
    [SerializeField] private bool fixTransformScale = false; // Disabled to preserve prefab scale
    [SerializeField] private bool fixColliderSize = true;
    [SerializeField] private bool fixButtonSize = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    void Start()
    {
        if (autoFixOnStart)
        {
            FixAllCellSizes();
        }
    }

    /// <summary>
    /// Fix all cell sizes in the scene
    /// </summary>
    [ContextMenu("Fix All Cell Sizes")]
    public void FixAllCellSizes()
    {
        Debug.Log("=== CELL SIZE FIX ===");
        Debug.Log($"Target cell size: {targetCellSize} pixels");
        Debug.Log($"Pixels per unit: {pixelsPerUnit}");

        CellController[] cellControllers = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        Debug.Log($"Found {cellControllers.Length} CellController components");

        int fixedCount = 0;

        foreach (CellController cell in cellControllers)
        {
            if (FixCellSize(cell))
            {
                fixedCount++;
            }
        }

        Debug.Log($"=== FIXED {fixedCount} CELL SIZES ===");
    }

    /// <summary>
    /// Fix a single cell's size
    /// </summary>
    private bool FixCellSize(CellController cell)
    {
        if (cell == null) return false;

        bool wasFixed = false;

        if (showDebugInfo)
        {
            Debug.Log($"\n🔧 Fixing cell at {cell.GetGridPosition()}:");
        }

        // Fix transform scale
        if (fixTransformScale)
        {
            wasFixed |= FixTransformScale(cell);
        }

        // Fix collider size
        if (fixColliderSize)
        {
            wasFixed |= FixColliderSize(cell);
        }

        // Fix button size (if it's a UI element)
        if (fixButtonSize)
        {
            wasFixed |= FixButtonSize(cell);
        }

        return wasFixed;
    }

    /// <summary>
    /// Fix the transform scale to match sprite size
    /// </summary>
    private bool FixTransformScale(CellController cell)
    {
        Vector3 currentScale = cell.transform.localScale;
        
        if (useSpriteSize)
        {
            // Get sprite size
            SpriteRenderer spriteRenderer = cell.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
                Vector2 spritePixels = new Vector2(
                    spriteRenderer.sprite.texture.width,
                    spriteRenderer.sprite.texture.height
                );
                
                // Calculate scale to make sprite appear as target size
                Vector2 targetScale = new Vector2(
                    targetCellSize.x / spritePixels.x,
                    targetCellSize.y / spritePixels.y
                );
                
                cell.transform.localScale = new Vector3(targetScale.x, targetScale.y, 1f);
                
                if (showDebugInfo)
                {
                    Debug.Log($"  📏 Transform Scale:");
                    Debug.Log($"    - Current: {currentScale}");
                    Debug.Log($"    - Sprite size: {spriteSize} (bounds) / {spritePixels} (pixels)");
                    Debug.Log($"    - Target: {targetCellSize} pixels");
                    Debug.Log($"    - New scale: {cell.transform.localScale}");
                }
                
                return true;
            }
            else
            {
                // No sprite, use default scale
                Vector2 defaultScale = new Vector2(
                    targetCellSize.x / pixelsPerUnit,
                    targetCellSize.y / pixelsPerUnit
                );
                cell.transform.localScale = new Vector3(defaultScale.x, defaultScale.y, 1f);
                
                if (showDebugInfo)
                {
                    Debug.Log($"  📏 Transform Scale (no sprite):");
                    Debug.Log($"    - Current: {currentScale}");
                    Debug.Log($"    - New scale: {cell.transform.localScale}");
                }
                
                return true;
            }
        }
        else
        {
            // Use target size directly
            Vector2 targetScale = new Vector2(
                targetCellSize.x / pixelsPerUnit,
                targetCellSize.y / pixelsPerUnit
            );
            cell.transform.localScale = new Vector3(targetScale.x, targetScale.y, 1f);
            
            if (showDebugInfo)
            {
                Debug.Log($"  📏 Transform Scale (direct):");
                Debug.Log($"    - Current: {currentScale}");
                Debug.Log($"    - New scale: {cell.transform.localScale}");
            }
            
            return true;
        }
    }

    /// <summary>
    /// Fix the collider size to match the cell size
    /// </summary>
    private bool FixColliderSize(CellController cell)
    {
        Collider2D collider = cell.GetComponent<Collider2D>();
        if (collider == null)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"  ⚠️ No Collider2D found on cell {cell.GetGridPosition()}");
            }
            return false;
        }

        // Calculate world size based on transform scale
        Vector3 scale = cell.transform.localScale;
        
        // Use a fixed collider size that works well with the 1.0 scale
        // This ensures consistent clickable area regardless of sprite size
        Vector2 worldSize = new Vector2(1.72f, 1.72f);

        if (collider is BoxCollider2D boxCollider)
        {
            boxCollider.size = worldSize;
            if (showDebugInfo)
            {
                Debug.Log($"  📦 BoxCollider2D size: {boxCollider.size}");
                Debug.Log($"    - Transform scale: {scale}");
                Debug.Log($"    - Target cell size: {targetCellSize}");
                Debug.Log($"    - Pixels per unit: {pixelsPerUnit}");
                Debug.Log($"    - Calculated world size: {worldSize}");
            }
        }
        else if (collider is CircleCollider2D circleCollider)
        {
            circleCollider.radius = Mathf.Max(worldSize.x, worldSize.y) * 0.5f;
            if (showDebugInfo)
            {
                Debug.Log($"  ⭕ CircleCollider2D radius: {circleCollider.radius}");
            }
        }

        return true;
    }

    /// <summary>
    /// Fix button size if it's a UI element
    /// </summary>
    private bool FixButtonSize(CellController cell)
    {
        Button button = cell.GetComponent<Button>();
        if (button == null) return false;

        RectTransform rectTransform = button.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = targetCellSize;
            if (showDebugInfo)
            {
                Debug.Log($"  🔘 Button size: {rectTransform.sizeDelta}");
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// Debug current cell sizes
    /// </summary>
    [ContextMenu("Debug Cell Sizes")]
    public void DebugCellSizes()
    {
        Debug.Log("=== CELL SIZE DEBUG ===");

        CellController[] cellControllers = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        
        foreach (CellController cell in cellControllers)
        {
            Debug.Log($"\n📊 Cell {cell.GetGridPosition()}:");
            
            // Transform scale
            Debug.Log($"  - Transform scale: {cell.transform.localScale}");
            
            // Sprite info
            SpriteRenderer spriteRenderer = cell.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                Vector2 spritePixels = new Vector2(
                    spriteRenderer.sprite.texture.width,
                    spriteRenderer.sprite.texture.height
                );
                Vector2 spriteBounds = spriteRenderer.sprite.bounds.size;
                Debug.Log($"  - Sprite pixels: {spritePixels}");
                Debug.Log($"  - Sprite bounds: {spriteBounds}");
            }
            
            // Collider info
            Collider2D collider = cell.GetComponent<Collider2D>();
            if (collider != null)
            {
                Bounds bounds = collider.bounds;
                Debug.Log($"  - Collider bounds: {bounds}");
                Debug.Log($"  - Collider extents: {bounds.extents}");
                
                if (collider is BoxCollider2D boxCollider)
                {
                    Debug.Log($"  - BoxCollider2D size: {boxCollider.size}");
                }
                else if (collider is CircleCollider2D circleCollider)
                {
                    Debug.Log($"  - CircleCollider2D radius: {circleCollider.radius}");
                }
            }
            
            // Button info
            Button button = cell.GetComponent<Button>();
            if (button != null)
            {
                RectTransform rectTransform = button.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    Debug.Log($"  - Button size: {rectTransform.sizeDelta}");
                }
            }
        }

        Debug.Log("=== END CELL SIZE DEBUG ===");
    }

    /// <summary>
    /// Set target cell size
    /// </summary>
    public void SetTargetCellSize(Vector2 size)
    {
        targetCellSize = size;
        Debug.Log($"Target cell size set to: {targetCellSize} pixels");
    }

    /// <summary>
    /// Set pixels per unit
    /// </summary>
    public void SetPixelsPerUnit(float ppu)
    {
        pixelsPerUnit = ppu;
        Debug.Log($"Pixels per unit set to: {pixelsPerUnit}");
    }
}

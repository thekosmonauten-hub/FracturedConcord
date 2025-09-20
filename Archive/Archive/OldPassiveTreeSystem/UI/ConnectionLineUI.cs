using UnityEngine;

/// <summary>
/// Simple component for connection lines between passive tree nodes
/// </summary>
public class ConnectionLineUI : MonoBehaviour
{
    [Header("Line Settings")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Color lineColor = Color.white;
    [SerializeField] private float lineWidth = 2f;
    
    private void Awake()
    {
        // Get or create LineRenderer
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
        
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        // Configure LineRenderer
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.sortingOrder = 1; // Ensure lines render behind nodes
    }
    
    /// <summary>
    /// Set the line positions
    /// </summary>
    public void SetPositions(Vector3 startPos, Vector3 endPos)
    {
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
        }
    }
    
    /// <summary>
    /// Set the line color
    /// </summary>
    public void SetColor(Color color)
    {
        if (lineRenderer != null)
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }
    }
    
    /// <summary>
    /// Set the line width
    /// </summary>
    public void SetWidth(float width)
    {
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
        }
    }
}

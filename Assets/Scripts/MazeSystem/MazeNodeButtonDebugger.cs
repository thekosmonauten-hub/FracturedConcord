using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Debug component to test if pointer events are being received on maze node buttons.
/// </summary>
public class MazeNodeButtonDebugger : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Vector2Int nodePosition;
    private MazeMinimapUI minimapUI;
    
    public void Initialize(Vector2Int position, MazeMinimapUI ui)
    {
        nodePosition = position;
        minimapUI = ui;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[MazeNodeButtonDebugger] OnPointerClick detected on node at {nodePosition}!");
        if (minimapUI != null)
        {
            minimapUI.OnNodeButtonClicked(nodePosition);
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"[MazeNodeButtonDebugger] OnPointerDown detected on node at {nodePosition}!");
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"[MazeNodeButtonDebugger] OnPointerEnter detected on node at {nodePosition}!");
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"[MazeNodeButtonDebugger] OnPointerExit detected on node at {nodePosition}!");
    }
}




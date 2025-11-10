using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Temporary debug tool to identify what UI element is blocking card clicks
/// Attach to a full-screen invisible Image with raycastTarget enabled
/// </summary>
public class ClickDebugger : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        if (results.Count > 0)
        {
            Debug.Log($"<color=yellow>━━━ CLICK AT {eventData.position} ━━━</color>");
            Debug.Log($"<color=cyan>Found {results.Count} UI elements under click (top 10):</color>");
            
            for (int i = 0; i < Mathf.Min(10, results.Count); i++)
            {
                var result = results[i];
                string canvasInfo = "";
                
                Canvas canvas = result.gameObject.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    canvasInfo = $" [Canvas: {canvas.name}, Sort: {canvas.sortingOrder}, Override: {canvas.overrideSorting}]";
                }
                
                string siblingInfo = $" [Sibling: {result.gameObject.transform.GetSiblingIndex()}]";
                
                // Check if it's raycast blocking
                Graphic graphic = result.gameObject.GetComponent<Graphic>();
                string raycastInfo = graphic != null ? $" [RaycastTarget: {graphic.raycastTarget}]" : "";
                
                Debug.Log($"  {i + 1}. <b>{result.gameObject.name}</b>{siblingInfo}{canvasInfo}{raycastInfo}");
            }
            
            Debug.Log($"<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        }
    }
}


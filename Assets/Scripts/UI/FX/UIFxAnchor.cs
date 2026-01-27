using UnityEngine;

/// <summary>
/// Optional anchor for FX Islands. Attach to a UI element to provide a stable
/// RectTransform and optional offset for world-space FX followers.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UIFxAnchor : MonoBehaviour
{
    [SerializeField] private Vector3 worldOffset = Vector3.zero;
    
    public RectTransform RectTransform => transform as RectTransform;
    public Vector3 WorldOffset => worldOffset;
}

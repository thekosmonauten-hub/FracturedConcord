using UnityEngine;

/// <summary>
/// Keeps a world-space FX object aligned with a UI RectTransform.
/// Intended for SpriteRenderer/ParticleSystem-based "FX Islands".
/// </summary>
public class UIFxFollower : MonoBehaviour
{
    [SerializeField] private RectTransform target;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private float worldDistance = 10f;
    [SerializeField] private Vector3 worldOffset = Vector3.zero;
    [SerializeField] private bool followRotation = false;
    [SerializeField] private bool followScale = false;
    [SerializeField] private bool snapOnEnable = true;
    
    public void SetTarget(RectTransform newTarget)
    {
        target = newTarget;
    }
    
    public void SetOffset(Vector3 offset)
    {
        worldOffset = offset;
    }
    
    public void SetUiCamera(Camera camera)
    {
        uiCamera = camera;
    }
    
    public void SetWorldCamera(Camera camera)
    {
        worldCamera = camera;
    }
    
    public void SetWorldDistance(float distance)
    {
        worldDistance = distance;
    }
    
    private void OnEnable()
    {
        if (snapOnEnable)
        {
            SnapToTarget();
        }
    }
    
    private void LateUpdate()
    {
        SnapToTarget();
    }
    
    public void SnapToTarget()
    {
        if (target == null)
        {
            return;
        }
        
        Camera resolvedUiCamera = uiCamera;
        if (resolvedUiCamera == null)
        {
            Canvas canvas = target.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                resolvedUiCamera = canvas.worldCamera;
            }
        }
        
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(resolvedUiCamera, target.position);
        Camera resolvedWorldCamera = worldCamera != null ? worldCamera : Camera.main;
        if (resolvedWorldCamera == null)
        {
            return;
        }
        
        float distance = Mathf.Max(0.01f, worldDistance);
        Vector3 worldPos = resolvedWorldCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distance));
        transform.position = worldPos + worldOffset;
        
        if (followRotation)
        {
            transform.rotation = target.rotation;
        }
        
        if (followScale)
        {
            transform.localScale = target.lossyScale;
        }
    }
}

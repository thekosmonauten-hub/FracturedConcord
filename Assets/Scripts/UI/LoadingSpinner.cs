using UnityEngine;

/// <summary>
/// Simple script to rotate a loading spinner icon continuously.
/// Attach to the loading spinner GameObject.
/// </summary>
public class LoadingSpinner : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Rotation speed in degrees per second")]
    [SerializeField] private float rotationSpeed = 180f;
    
    [Tooltip("Rotation direction")]
    [SerializeField] private bool clockwise = true;
    
    private void Update()
    {
        float speed = clockwise ? rotationSpeed : -rotationSpeed;
        transform.Rotate(0, 0, -speed * Time.deltaTime);
    }
}


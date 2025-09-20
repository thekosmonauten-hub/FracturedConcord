using UnityEngine;
using System.Collections;

/// <summary>
/// Simple memory management script to help reduce Unity's internal memory leaks
/// This addresses TLS Allocator issues that come from Unity's internal systems
/// </summary>
public class MemoryLeakFix : MonoBehaviour
{
    [Header("Memory Management Settings")]
    [SerializeField] private bool enableMemoryManagement = true;
    [SerializeField] private float garbageCollectionInterval = 30f; // Every 30 seconds
    [SerializeField] private bool logMemoryUsage = false;

    private float lastGCTime;

    private void Start()
    {
        if (enableMemoryManagement)
        {
            Debug.Log("[MemoryLeakFix] Memory management enabled");
            StartCoroutine(MemoryManagementRoutine());
        }
    }

    private IEnumerator MemoryManagementRoutine()
    {
        while (enableMemoryManagement)
        {
            yield return new WaitForSeconds(garbageCollectionInterval);
            
            // Force garbage collection to clean up temporary allocations
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();

            if (logMemoryUsage)
            {
                long memoryUsage = System.GC.GetTotalMemory(false);
                Debug.Log($"[MemoryLeakFix] Memory usage after GC: {memoryUsage / 1024 / 1024} MB");
            }
        }
    }

    /// <summary>
    /// Manual garbage collection - call this if you notice memory issues
    /// </summary>
    [ContextMenu("Force Garbage Collection")]
    public void ForceGarbageCollection()
    {
        Debug.Log("[MemoryLeakFix] Forcing garbage collection...");
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        System.GC.Collect();
        
        long memoryUsage = System.GC.GetTotalMemory(false);
        Debug.Log($"[MemoryLeakFix] Memory usage after forced GC: {memoryUsage / 1024 / 1024} MB");
    }

    /// <summary>
    /// Get current memory usage
    /// </summary>
    [ContextMenu("Log Memory Usage")]
    public void LogMemoryUsage()
    {
        long memoryUsage = System.GC.GetTotalMemory(false);
        Debug.Log($"[MemoryLeakFix] Current memory usage: {memoryUsage / 1024 / 1024} MB");
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && enableMemoryManagement)
        {
            // Force GC when app is paused (mobile)
            ForceGarbageCollection();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && enableMemoryManagement)
        {
            // Force GC when app loses focus
            ForceGarbageCollection();
        }
    }
}










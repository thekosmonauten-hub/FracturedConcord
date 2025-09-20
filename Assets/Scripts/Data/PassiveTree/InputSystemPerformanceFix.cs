using UnityEngine;
using UnityEngine.InputSystem;

namespace PassiveTree
{
    /// <summary>
    /// Fixes InputSystem performance issues by increasing event throughput budget
    /// </summary>
    public class InputSystemPerformanceFix : MonoBehaviour
    {
        [Header("Performance Settings")]
        [SerializeField] private bool fixOnStart = true;
        [SerializeField] private int maxEventBytesPerUpdate = 0; // 0 = unlimited
        
        void Start()
        {
            if (fixOnStart)
            {
                FixInputSystemPerformance();
            }
        }
        
        /// <summary>
        /// Fix InputSystem performance by increasing event throughput budget
        /// </summary>
        [ContextMenu("Fix InputSystem Performance")]
        public void FixInputSystemPerformance()
        {
            try
            {
                // Get current settings
                var settings = InputSystem.settings;
                
                Debug.Log($"[InputSystemPerformanceFix] Current maxEventBytesPerUpdate: {settings.maxEventBytesPerUpdate}");
                
                // Update settings
                settings.maxEventBytesPerUpdate = maxEventBytesPerUpdate;
                
                Debug.Log($"[InputSystemPerformanceFix] Updated maxEventBytesPerUpdate to: {maxEventBytesPerUpdate}");
                Debug.Log($"[InputSystemPerformanceFix] InputSystem performance fix applied successfully!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[InputSystemPerformanceFix] Failed to fix InputSystem performance: {e.Message}");
            }
        }
        
        /// <summary>
        /// Reset to default settings
        /// </summary>
        [ContextMenu("Reset to Default Settings")]
        public void ResetToDefaultSettings()
        {
            try
            {
                var settings = InputSystem.settings;
                settings.maxEventBytesPerUpdate = 1024 * 1024; // Default value
                
                Debug.Log("[InputSystemPerformanceFix] Reset to default settings");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[InputSystemPerformanceFix] Failed to reset settings: {e.Message}");
            }
        }
    }
}


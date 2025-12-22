using UnityEngine;

/// <summary>
/// Helper to validate effect setup and ensure ParticleAttractor works correctly
/// </summary>
public static class EffectValidationHelper
{
    /// <summary>
    /// Validate that an enemy has a proper "Default" effect point set up
    /// </summary>
    public static bool ValidateEnemyEffectPoint(Transform enemyTransform, out Transform defaultPoint, out string errorMessage)
    {
        defaultPoint = null;
        errorMessage = "";
        
        if (enemyTransform == null)
        {
            errorMessage = "Enemy transform is null";
            return false;
        }
        
        // Use GetComponentInChildren because EffectPointProvider may be a child GameObject
        EffectPointProvider provider = enemyTransform.GetComponentInChildren<EffectPointProvider>(includeInactive: false);
        if (provider == null)
        {
            errorMessage = $"Enemy {enemyTransform.name} has no EffectPointProvider component (searched in children too)!";
            return false;
        }
        
        defaultPoint = provider.effectPoint_Default;
        if (defaultPoint == null)
        {
            errorMessage = $"Enemy {enemyTransform.name} has EffectPointProvider but effectPoint_Default is not assigned!";
            return false;
        }
        
        RectTransform rect = defaultPoint.GetComponent<RectTransform>();
        if (rect == null)
        {
            errorMessage = $"Enemy {enemyTransform.name}'s Default effect point has no RectTransform!";
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Validate that ParticleAttractor is properly configured on a projectile
    /// </summary>
    public static bool ValidateParticleAttractor(GameObject projectile, RectTransform expectedTarget, out string errorMessage)
    {
        errorMessage = "";
        
        if (projectile == null)
        {
            errorMessage = "Projectile GameObject is null";
            return false;
        }
        
        // Try to find ParticleAttractor via reflection
        System.Type attractorType = System.Type.GetType("Coffee.UIExtensions.ParticleAttractor, Assembly-CSharp");
        if (attractorType == null)
        {
            attractorType = System.Type.GetType("Coffee.UIExtensions.ParticleAttractor");
        }
        
        if (attractorType == null)
        {
            errorMessage = "ParticleAttractor type not found (package may not be installed)";
            return false;
        }
        
        Component attractor = projectile.GetComponent(attractorType);
        if (attractor == null)
        {
            errorMessage = "ParticleAttractor component not found on projectile";
            return false;
        }
        
        // Check target
        var targetProperty = attractorType.GetProperty("target");
        var targetField = attractorType.GetField("target");
        
        if (targetProperty == null && targetField == null)
        {
            errorMessage = "Could not find 'target' property/field on ParticleAttractor";
            return false;
        }
        
        object currentTarget = targetProperty != null ? targetProperty.GetValue(attractor) : targetField.GetValue(attractor);
        if (currentTarget != expectedTarget)
        {
            errorMessage = $"ParticleAttractor target mismatch! Expected {expectedTarget?.name}, got {currentTarget}";
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Log validation results for debugging
    /// </summary>
    public static void LogValidationResults(Transform enemyTransform, GameObject projectile, RectTransform expectedTarget)
    {
        Debug.Log("=== Effect Validation ===");
        
        // Validate enemy
        if (ValidateEnemyEffectPoint(enemyTransform, out Transform defaultPoint, out string enemyError))
        {
            Debug.Log($"✓ Enemy {enemyTransform.name} has valid Default effect point: {defaultPoint.name}");
        }
        else
        {
            Debug.LogError($"✗ Enemy validation failed: {enemyError}");
        }
        
        // Validate ParticleAttractor
        if (projectile != null)
        {
            if (ValidateParticleAttractor(projectile, expectedTarget, out string attractorError))
            {
                Debug.Log($"✓ ParticleAttractor is properly configured on {projectile.name}");
            }
            else
            {
                Debug.LogWarning($"⚠ ParticleAttractor validation: {attractorError}");
            }
        }
        
        Debug.Log("=== End Validation ===");
    }
}


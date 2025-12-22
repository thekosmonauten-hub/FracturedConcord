using UnityEngine;
using UnityEditor;
using System.Linq;

/// <summary>
/// Custom editor for CraftingRecipe with auto-cost calculation
/// </summary>
[CustomEditor(typeof(CraftingRecipe))]
[CanEditMultipleObjects]
public class ForgeRecipeCostCalculatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CraftingRecipe recipe = (CraftingRecipe)target;
        
        // Draw default inspector
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Cost Calculator", EditorStyles.boldLabel);
        
        // Show current cost
        if (recipe.requiredMaterials != null && recipe.requiredMaterials.Count > 0)
        {
            EditorGUILayout.HelpBox("Current Material Requirements:", MessageType.Info);
            foreach (var req in recipe.requiredMaterials)
            {
                EditorGUILayout.LabelField($"  - {req.quantity} {req.materialType}");
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No material requirements set.", MessageType.Warning);
        }
        
        EditorGUILayout.Space();
        
        // Calculate and show recommended cost
        int recommendedCost = ForgeCostCalculator.CalculateCostForRecipe(recipe, recipe.costConfig);
        ForgeMaterialType materialType = GetMaterialTypeForRecipe(recipe);
        float itemsNeeded = ForgeCostCalculator.EstimateItemsToSalvage(recipe.minItemLevel, recipe.craftedRarity);
        
        EditorGUILayout.LabelField("Recommended Cost:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"  {recommendedCost} {materialType}");
        EditorGUILayout.LabelField($"  (~{itemsNeeded:F1} items to salvage)");
        
        EditorGUILayout.Space();
        
        // Auto-calculate button
        if (GUILayout.Button("Auto-Calculate and Apply Cost", GUILayout.Height(30)))
        {
            Undo.RecordObject(recipe, "Auto-calculate crafting costs");
            recipe.CalculateAndSetCosts();
            EditorUtility.SetDirty(recipe);
            
            // Update all selected recipes if multi-selecting
            if (Selection.objects.Length > 1)
            {
                foreach (var obj in Selection.objects)
                {
                    if (obj is CraftingRecipe r)
                    {
                        Undo.RecordObject(r, "Auto-calculate crafting costs");
                        r.CalculateAndSetCosts();
                        EditorUtility.SetDirty(r);
                    }
                }
            }
            
            Debug.Log($"[CraftingRecipe] Auto-calculated cost: {recommendedCost} {materialType} for recipe '{recipe.recipeName}'");
        }
        
        // Enable auto-calculate toggle
        EditorGUILayout.Space();
        bool useAuto = EditorGUILayout.Toggle("Enable Auto-Calculate on Load", recipe.useAutoCalculatedCosts);
        if (useAuto != recipe.useAutoCalculatedCosts)
        {
            Undo.RecordObject(recipe, "Toggle auto-calculate");
            recipe.useAutoCalculatedCosts = useAuto;
            EditorUtility.SetDirty(recipe);
            
            if (useAuto)
            {
                recipe.CalculateAndSetCosts();
            }
        }
        
        if (recipe.useAutoCalculatedCosts)
        {
            EditorGUILayout.HelpBox("Costs will be automatically calculated when the recipe is used.", MessageType.Info);
        }
        
        EditorGUILayout.Space();
        
        // Cost config preset selector
        EditorGUILayout.LabelField("Cost Preset:", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        
        int presetIndex = 0;
        if (recipe.costConfig.Equals(ForgeCostCalculator.CostConfig.Default))
            presetIndex = 0;
        else if (recipe.costConfig.Equals(ForgeCostCalculator.CostConfig.Alternative))
            presetIndex = 1;
        else if (recipe.costConfig.Equals(ForgeCostCalculator.CostConfig.Expensive))
            presetIndex = 2;
        else
            presetIndex = 3; // Custom
        
        string[] presetOptions = { "Default (Balanced)", "Alternative (Cheaper)", "Expensive", "Custom" };
        int newPresetIndex = EditorGUILayout.Popup(presetIndex, presetOptions);
        
        if (EditorGUI.EndChangeCheck() || presetIndex != newPresetIndex)
        {
            Undo.RecordObject(recipe, "Change cost preset");
            
            switch (newPresetIndex)
            {
                case 0:
                    recipe.costConfig = ForgeCostCalculator.CostConfig.Default;
                    break;
                case 1:
                    recipe.costConfig = ForgeCostCalculator.CostConfig.Alternative;
                    break;
                case 2:
                    recipe.costConfig = ForgeCostCalculator.CostConfig.Expensive;
                    break;
                // case 3: Custom - don't change
            }
            
            EditorUtility.SetDirty(recipe);
            
            if (recipe.useAutoCalculatedCosts)
            {
                recipe.CalculateAndSetCosts();
            }
        }
        
        // Show config details
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Current Config:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"  Base: {recipe.costConfig.baseCost}, Level Scaling: {recipe.costConfig.levelScaling}");
        EditorGUILayout.LabelField($"  Normal: {recipe.costConfig.normalMultiplier}x, Magic: {recipe.costConfig.magicMultiplier}x");
        EditorGUILayout.LabelField($"  Rare: {recipe.costConfig.rareMultiplier}x, Unique: {recipe.costConfig.uniqueMultiplier}x");
    }
    
    private ForgeMaterialType GetMaterialTypeForRecipe(CraftingRecipe recipe)
    {
        switch (recipe.itemType)
        {
            case ItemType.Weapon:
                return ForgeMaterialType.WeaponScraps;
            case ItemType.Armour:
            case ItemType.Accessory:
                return ForgeMaterialType.ArmourScraps;
            case ItemType.Effigy:
                return ForgeMaterialType.EffigySplinters;
            case ItemType.Warrant:
                return ForgeMaterialType.WarrantShards;
            default:
                return ForgeMaterialType.WeaponScraps;
        }
    }
}



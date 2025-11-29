using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

/// <summary>
/// Editor tool to extract requirement information from implicit modifiers
/// and apply them to equipment asset requirement fields.
/// Fixes issues at the source rather than hiding them in tooltips.
/// </summary>
public class ExtractRequirementsFromImplicits : EditorWindow
{
    private Vector2 scrollPosition;
    private List<EquipmentUpdateInfo> updateInfo = new List<EquipmentUpdateInfo>();
    private bool removeRequirementsFromImplicits = true;
    private bool previewMode = true;
    
    private class EquipmentUpdateInfo
    {
        public BaseItem item;
        public string assetPath;
        public string itemName;
        public bool hasChanges;
        public int newRequiredLevel;
        public int newRequiredStrength;
        public int newRequiredDexterity;
        public int newRequiredIntelligence;
        public List<string> removedImplicits = new List<string>();
        public string statusMessage;
    }
    
    [MenuItem("Tools/Dexiled/Extract Requirements from Implicits")]
    public static void ShowWindow()
    {
        GetWindow<ExtractRequirementsFromImplicits>("Extract Requirements");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Extract Requirements from Implicit Modifiers", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "This tool scans all equipment assets (Armour, OffHandEquipment, Jewellery) " +
            "and extracts requirement information from implicit modifiers.\n\n" +
            "Example: \"Requires Level 66, 159 Dex\" → requiredLevel=66, requiredDexterity=159\n\n" +
            "The requirements implicit will be removed from the implicit list after extraction.",
            MessageType.Info);
        
        EditorGUILayout.Space();
        
        removeRequirementsFromImplicits = EditorGUILayout.Toggle(
            "Remove Requirements from Implicits After Extraction", 
            removeRequirementsFromImplicits);
        
        previewMode = EditorGUILayout.Toggle(
            "Preview Mode (Don't Save Changes)", 
            previewMode);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Scan All Equipment Assets", GUILayout.Height(30)))
        {
            ScanAssets();
        }
        
        EditorGUILayout.Space();
        
        if (updateInfo.Count > 0)
        {
            EditorGUILayout.LabelField($"Found {updateInfo.Count} equipment items to process:", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            int itemsWithChanges = updateInfo.Count(info => info.hasChanges);
            EditorGUILayout.LabelField($"Items with changes: {itemsWithChanges}", EditorStyles.miniLabel);
            EditorGUILayout.Space();
            
            foreach (var info in updateInfo)
            {
                DrawEquipmentInfo(info);
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            
            if (!previewMode && itemsWithChanges > 0)
            {
                if (GUILayout.Button($"Apply Changes to {itemsWithChanges} Item(s)", GUILayout.Height(30)))
                {
                    ApplyChanges();
                }
            }
            else if (previewMode && itemsWithChanges > 0)
            {
                EditorGUILayout.HelpBox(
                    "Preview Mode is enabled. Disable it and click 'Apply Changes' to save modifications.",
                    MessageType.Warning);
            }
        }
    }
    
    private void DrawEquipmentInfo(EquipmentUpdateInfo info)
    {
        EditorGUILayout.BeginVertical("box");
        
        EditorGUILayout.LabelField(info.itemName, EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Path: {info.assetPath}", EditorStyles.miniLabel);
        
        if (info.hasChanges)
        {
            EditorGUILayout.LabelField("Changes:", EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            
            if (info.newRequiredLevel > 0)
            {
                EditorGUILayout.LabelField($"Required Level: {info.newRequiredLevel}");
            }
            
            if (info.newRequiredStrength > 0)
            {
                EditorGUILayout.LabelField($"Required Strength: {info.newRequiredStrength}");
            }
            
            if (info.newRequiredDexterity > 0)
            {
                EditorGUILayout.LabelField($"Required Dexterity: {info.newRequiredDexterity}");
            }
            
            if (info.newRequiredIntelligence > 0)
            {
                EditorGUILayout.LabelField($"Required Intelligence: {info.newRequiredIntelligence}");
            }
            
            if (info.removedImplicits.Count > 0)
            {
                EditorGUILayout.LabelField($"Implicits to remove: {info.removedImplicits.Count}");
                foreach (var implicitDesc in info.removedImplicits)
                {
                    EditorGUILayout.LabelField($"  • {implicitDesc}", EditorStyles.miniLabel);
                }
            }
            
            EditorGUI.indentLevel--;
        }
        else
        {
            EditorGUILayout.LabelField("No changes needed", EditorStyles.miniLabel);
        }
        
        if (!string.IsNullOrEmpty(info.statusMessage))
        {
            EditorGUILayout.HelpBox(info.statusMessage, MessageType.Info);
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }
    
    private void ScanAssets()
    {
        updateInfo.Clear();
        
        // Find all Armour assets
        string[] armourGuids = AssetDatabase.FindAssets("t:Armour");
        foreach (string guid in armourGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Armour armour = AssetDatabase.LoadAssetAtPath<Armour>(path);
            if (armour != null)
            {
                ProcessEquipment(armour, path);
            }
        }
        
        // Find all OffHandEquipment assets
        string[] offHandGuids = AssetDatabase.FindAssets("t:OffHandEquipment");
        foreach (string guid in offHandGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            OffHandEquipment offHand = AssetDatabase.LoadAssetAtPath<OffHandEquipment>(path);
            if (offHand != null)
            {
                ProcessEquipment(offHand, path);
            }
        }
        
        // Find all Jewellery assets
        string[] jewelleryGuids = AssetDatabase.FindAssets("t:Jewellery");
        foreach (string guid in jewelleryGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Jewellery jewellery = AssetDatabase.LoadAssetAtPath<Jewellery>(path);
            if (jewellery != null)
            {
                ProcessEquipment(jewellery, path);
            }
        }
        
        Debug.Log($"[ExtractRequirements] Scanned {updateInfo.Count} equipment items");
    }
    
    private void ProcessEquipment(BaseItem item, string assetPath)
    {
        var info = new EquipmentUpdateInfo
        {
            item = item,
            assetPath = assetPath,
            itemName = item.itemName ?? "Unnamed Item",
            hasChanges = false,
            newRequiredLevel = item.requiredLevel,
            newRequiredStrength = 0,
            newRequiredDexterity = 0,
            newRequiredIntelligence = 0
        };
        
        // Get current requirements based on equipment type
        if (item is Armour armour)
        {
            info.newRequiredStrength = armour.requiredStrength;
            info.newRequiredDexterity = armour.requiredDexterity;
            info.newRequiredIntelligence = armour.requiredIntelligence;
        }
        else if (item is OffHandEquipment offHand)
        {
            info.newRequiredStrength = offHand.requiredStrength;
            info.newRequiredDexterity = offHand.requiredDexterity;
            info.newRequiredIntelligence = offHand.requiredIntelligence;
        }
        else if (item is Jewellery jewellery)
        {
            info.newRequiredStrength = jewellery.requiredStrength;
            info.newRequiredDexterity = jewellery.requiredDexterity;
            info.newRequiredIntelligence = jewellery.requiredIntelligence;
        }
        
        // Check implicit modifiers for requirements
        if (item.implicitModifiers != null && item.implicitModifiers.Count > 0)
        {
            List<Affix> implicitsToRemove = new List<Affix>();
            
            foreach (var implicitModifier in item.implicitModifiers)
            {
                if (implicitModifier == null || string.IsNullOrEmpty(implicitModifier.description))
                    continue;
                
                string desc = implicitModifier.description;
                
                // Check if this is a requirements implicit
                if (IsRequirementsImplicit(desc))
                {
                    // Extract requirements
                    ExtractRequirements(desc, ref info);
                    
                    // Mark for removal if option is enabled
                    if (removeRequirementsFromImplicits)
                    {
                        implicitsToRemove.Add(implicitModifier);
                        info.removedImplicits.Add(desc);
                    }
                }
            }
            
            // Check if we have any changes
            bool hasLevelChange = info.newRequiredLevel != item.requiredLevel;
            bool hasAttributeChange = false;
            
            if (item is Armour a)
            {
                hasAttributeChange = info.newRequiredStrength != a.requiredStrength ||
                                     info.newRequiredDexterity != a.requiredDexterity ||
                                     info.newRequiredIntelligence != a.requiredIntelligence;
            }
            else if (item is OffHandEquipment oh)
            {
                hasAttributeChange = info.newRequiredStrength != oh.requiredStrength ||
                                     info.newRequiredDexterity != oh.requiredDexterity ||
                                     info.newRequiredIntelligence != oh.requiredIntelligence;
            }
            else if (item is Jewellery j)
            {
                hasAttributeChange = info.newRequiredStrength != j.requiredStrength ||
                                     info.newRequiredDexterity != j.requiredDexterity ||
                                     info.newRequiredIntelligence != j.requiredIntelligence;
            }
            
            // If we found requirements and extracted them, or need to remove implicits, mark as having changes
            if (info.removedImplicits.Count > 0 || hasLevelChange || hasAttributeChange)
            {
                info.hasChanges = true;
            }
        }
        
        updateInfo.Add(info);
    }
    
    private bool IsRequirementsImplicit(string description)
    {
        if (string.IsNullOrEmpty(description))
            return false;
        
        string desc = description.ToLower();
        return desc.Contains("requires") && 
               (desc.Contains("level") || desc.Contains("str") || desc.Contains("dex") || 
                desc.Contains("int") || desc.Contains("strength") || desc.Contains("dexterity") || 
                desc.Contains("intelligence"));
    }
    
    private void ExtractRequirements(string description, ref EquipmentUpdateInfo info)
    {
        if (string.IsNullOrEmpty(description))
            return;
        
        string desc = description.ToLower();
        
        // Extract level requirement
        var levelMatch = Regex.Match(desc, @"level\s+(\d+)", RegexOptions.IgnoreCase);
        if (levelMatch.Success && int.TryParse(levelMatch.Groups[1].Value, out int level))
        {
            info.newRequiredLevel = Mathf.Max(info.newRequiredLevel, level);
        }
        
        // Extract strength requirement
        var strMatch = Regex.Match(desc, @"(\d+)\s+(?:str|strength)", RegexOptions.IgnoreCase);
        if (strMatch.Success && int.TryParse(strMatch.Groups[1].Value, out int str))
        {
            info.newRequiredStrength = Mathf.Max(info.newRequiredStrength, str);
        }
        
        // Extract dexterity requirement
        var dexMatch = Regex.Match(desc, @"(\d+)\s+(?:dex|dexterity)", RegexOptions.IgnoreCase);
        if (dexMatch.Success && int.TryParse(dexMatch.Groups[1].Value, out int dex))
        {
            info.newRequiredDexterity = Mathf.Max(info.newRequiredDexterity, dex);
        }
        
        // Extract intelligence requirement
        var intMatch = Regex.Match(desc, @"(\d+)\s+(?:int|intelligence)", RegexOptions.IgnoreCase);
        if (intMatch.Success && int.TryParse(intMatch.Groups[1].Value, out int intelligence))
        {
            info.newRequiredIntelligence = Mathf.Max(info.newRequiredIntelligence, intelligence);
        }
    }
    
    private void ApplyChanges()
    {
        int updatedCount = 0;
        int skippedCount = 0;
        
        foreach (var info in updateInfo)
        {
            if (!info.hasChanges)
            {
                skippedCount++;
                continue;
            }
            
            try
            {
                bool wasModified = false;
                
                // Update requiredLevel (inherited from BaseItem)
                if (info.item.requiredLevel != info.newRequiredLevel)
                {
                    info.item.requiredLevel = info.newRequiredLevel;
                    wasModified = true;
                }
                
                // Update attribute requirements based on equipment type
                if (info.item is Armour armour)
                {
                    if (armour.requiredStrength != info.newRequiredStrength)
                    {
                        armour.requiredStrength = info.newRequiredStrength;
                        wasModified = true;
                    }
                    if (armour.requiredDexterity != info.newRequiredDexterity)
                    {
                        armour.requiredDexterity = info.newRequiredDexterity;
                        wasModified = true;
                    }
                    if (armour.requiredIntelligence != info.newRequiredIntelligence)
                    {
                        armour.requiredIntelligence = info.newRequiredIntelligence;
                        wasModified = true;
                    }
                }
                else if (info.item is OffHandEquipment offHand)
                {
                    if (offHand.requiredStrength != info.newRequiredStrength)
                    {
                        offHand.requiredStrength = info.newRequiredStrength;
                        wasModified = true;
                    }
                    if (offHand.requiredDexterity != info.newRequiredDexterity)
                    {
                        offHand.requiredDexterity = info.newRequiredDexterity;
                        wasModified = true;
                    }
                    if (offHand.requiredIntelligence != info.newRequiredIntelligence)
                    {
                        offHand.requiredIntelligence = info.newRequiredIntelligence;
                        wasModified = true;
                    }
                }
                else if (info.item is Jewellery jewellery)
                {
                    if (jewellery.requiredStrength != info.newRequiredStrength)
                    {
                        jewellery.requiredStrength = info.newRequiredStrength;
                        wasModified = true;
                    }
                    if (jewellery.requiredDexterity != info.newRequiredDexterity)
                    {
                        jewellery.requiredDexterity = info.newRequiredDexterity;
                        wasModified = true;
                    }
                    if (jewellery.requiredIntelligence != info.newRequiredIntelligence)
                    {
                        jewellery.requiredIntelligence = info.newRequiredIntelligence;
                        wasModified = true;
                    }
                }
                
                // Remove requirements implicits if enabled
                if (removeRequirementsFromImplicits && info.removedImplicits.Count > 0)
                {
                    List<Affix> newImplicits = new List<Affix>();
                    foreach (var implicitModifier in info.item.implicitModifiers)
                    {
                        if (implicitModifier == null)
                            continue;
                        
                        // Check if this implicit should be removed
                        bool shouldRemove = false;
                        foreach (var removedDesc in info.removedImplicits)
                        {
                            if (implicitModifier.description == removedDesc)
                            {
                                shouldRemove = true;
                                break;
                            }
                        }
                        
                        if (!shouldRemove)
                        {
                            newImplicits.Add(implicitModifier);
                        }
                    }
                    
                    info.item.implicitModifiers = newImplicits;
                    wasModified = true;
                }
                
                if (wasModified)
                {
                    EditorUtility.SetDirty(info.item);
                    updatedCount++;
                    info.statusMessage = "✓ Updated successfully";
                }
                else
                {
                    skippedCount++;
                }
            }
            catch (System.Exception e)
            {
                info.statusMessage = $"✗ Error: {e.Message}";
                Debug.LogError($"[ExtractRequirements] Error updating {info.assetPath}: {e}");
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"[ExtractRequirements] Updated {updatedCount} items, skipped {skippedCount} items");
        EditorUtility.DisplayDialog(
            "Update Complete",
            $"Successfully updated {updatedCount} equipment item(s).\nSkipped {skippedCount} item(s).",
            "OK");
        
        // Refresh the display
        ScanAssets();
    }
}


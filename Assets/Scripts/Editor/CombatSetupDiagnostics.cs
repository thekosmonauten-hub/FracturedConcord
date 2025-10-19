using UnityEngine;
using UnityEditor;

/// <summary>
/// Diagnostic tool to verify combat system setup.
/// Menu: Tools > Combat UI > Diagnose Combat Setup
/// </summary>
public class CombatSetupDiagnostics : EditorWindow
{
    private Vector2 scrollPosition;
    private string diagnosticResults = "";
    
    [MenuItem("Tools/Combat UI/Diagnose Combat Setup")]
    public static void ShowWindow()
    {
        var window = GetWindow<CombatSetupDiagnostics>("Combat Diagnostics");
        window.minSize = new Vector2(600, 600);
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Combat System Diagnostics", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "This tool checks if your combat system is properly configured.\n" +
            "Run diagnostics to find missing references or setup issues.",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Run Full Diagnostics", GUILayout.Height(40)))
        {
            RunDiagnostics();
        }
        
        EditorGUILayout.Space();
        
        // Results
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.TextArea(diagnosticResults, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }
    
    private void RunDiagnostics()
    {
        diagnosticResults = "=== COMBAT SYSTEM DIAGNOSTICS ===\n\n";
        
        bool allGood = true;
        
        // Check 1: CardRuntimeManager
        diagnosticResults += "1. CardRuntimeManager\n";
        CardRuntimeManager cardRuntime = GameObject.FindFirstObjectByType<CardRuntimeManager>();
        if (cardRuntime != null)
        {
            diagnosticResults += "   ✓ Found in scene\n";
            
            // Check prefab assignment
            SerializedObject so = new SerializedObject(cardRuntime);
            SerializedProperty prefabProp = so.FindProperty("cardPrefab");
            
            if (prefabProp.objectReferenceValue != null)
            {
                diagnosticResults += $"   ✓ Card Prefab assigned: {prefabProp.objectReferenceValue.name}\n";
            }
            else
            {
                diagnosticResults += "   ✗ Card Prefab NOT assigned!\n";
                diagnosticResults += "      FIX: Assign CardPrefab.prefab to Card Prefab field\n";
                allGood = false;
            }
            
            // Check hand parent
            SerializedProperty handParentProp = so.FindProperty("cardHandParent");
            if (handParentProp.objectReferenceValue != null)
            {
                diagnosticResults += $"   ✓ Card Hand Parent assigned: {handParentProp.objectReferenceValue.name}\n";
            }
            else
            {
                diagnosticResults += "   ✗ Card Hand Parent NOT assigned!\n";
                diagnosticResults += "      FIX: Create empty GameObject and assign to Card Hand Parent field\n";
                allGood = false;
            }
            
            // Check pool size
            SerializedProperty poolSizeProp = so.FindProperty("poolSize");
            diagnosticResults += $"   • Pool Size: {poolSizeProp.intValue}\n";
        }
        else
        {
            diagnosticResults += "   ✗ NOT FOUND in scene!\n";
            diagnosticResults += "      FIX: Create GameObject and add CardRuntimeManager component\n";
            allGood = false;
        }
        
        diagnosticResults += "\n";
        
        // Check 2: CombatDeckManager
        diagnosticResults += "2. CombatDeckManager\n";
        CombatDeckManager deckManager = GameObject.FindFirstObjectByType<CombatDeckManager>();
        if (deckManager != null)
        {
            diagnosticResults += "   ✓ Found in scene\n";
            
            // Check if it can find CardRuntimeManager
            if (cardRuntime != null)
            {
                diagnosticResults += "   ✓ Can access CardRuntimeManager\n";
            }
            else
            {
                diagnosticResults += "   ✗ Cannot find CardRuntimeManager!\n";
                allGood = false;
            }
        }
        else
        {
            diagnosticResults += "   ✗ NOT FOUND in scene!\n";
            diagnosticResults += "      FIX: Create GameObject and add CombatDeckManager component\n";
            allGood = false;
        }
        
        diagnosticResults += "\n";
        
        // Check 3: CombatAnimationManager
        diagnosticResults += "3. CombatAnimationManager\n";
        CombatAnimationManager animManager = GameObject.FindFirstObjectByType<CombatAnimationManager>();
        if (animManager != null)
        {
            diagnosticResults += "   ✓ Found in scene\n";
        }
        else
        {
            diagnosticResults += "   ⚠ NOT FOUND (optional but recommended)\n";
            diagnosticResults += "      FIX: Create GameObject and add CombatAnimationManager component\n";
        }
        
        diagnosticResults += "\n";
        
        // Check 4: Card Prefab
        diagnosticResults += "4. Card Prefab\n";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/CardArt/CardPrefab.prefab");
        if (prefab != null)
        {
            diagnosticResults += "   ✓ CardPrefab.prefab found\n";
            
            // Check for required components
            CombatCardAdapter adapter = prefab.GetComponent<CombatCardAdapter>();
            if (adapter != null)
            {
                diagnosticResults += "   ✓ Has CombatCardAdapter\n";
            }
            else
            {
                diagnosticResults += "   ✗ Missing CombatCardAdapter!\n";
                diagnosticResults += "      FIX: Open prefab and add CombatCardAdapter component\n";
                allGood = false;
            }
            
            CardHoverEffect hover = prefab.GetComponent<CardHoverEffect>();
            if (hover != null)
            {
                diagnosticResults += "   ✓ Has CardHoverEffect\n";
            }
            else
            {
                diagnosticResults += "   ⚠ Missing CardHoverEffect (optional)\n";
                diagnosticResults += "      FIX: Open prefab and add CardHoverEffect component\n";
            }
        }
        else
        {
            diagnosticResults += "   ✗ CardPrefab.prefab NOT FOUND!\n";
            diagnosticResults += "      FIX: Check if prefab exists at Assets/Art/CardArt/CardPrefab.prefab\n";
            allGood = false;
        }
        
        diagnosticResults += "\n";
        
        // Check 5: JSON Deck Files
        diagnosticResults += "5. JSON Deck Files\n";
        string[] classes = { "apostle", "brawler", "marauder", "ranger", "thief", "witch" };
        int foundDecks = 0;
        
        foreach (string className in classes)
        {
            TextAsset jsonFile = Resources.Load<TextAsset>($"Cards/starter_deck_{className}");
            if (jsonFile != null)
            {
                diagnosticResults += $"   ✓ {className} deck found\n";
                foundDecks++;
            }
            else
            {
                diagnosticResults += $"   ✗ {className} deck NOT FOUND\n";
                diagnosticResults += $"      Expected: Resources/Cards/starter_deck_{className}.json\n";
            }
        }
        
        diagnosticResults += $"   Found {foundDecks}/6 decks\n";
        
        diagnosticResults += "\n";
        
        // Check 6: CharacterManager
        diagnosticResults += "6. CharacterManager\n";
        CharacterManager charManager = CharacterManager.Instance;
        if (charManager != null)
        {
            diagnosticResults += "   ✓ CharacterManager exists\n";
            
            if (charManager.HasCharacter())
            {
                Character player = charManager.GetCurrentCharacter();
                diagnosticResults += $"   ✓ Character loaded: {player.characterName} ({player.characterClass})\n";
            }
            else
            {
                diagnosticResults += "   ⚠ No character loaded (create one to test)\n";
            }
        }
        else
        {
            diagnosticResults += "   ⚠ CharacterManager not found (will use test character)\n";
        }
        
        diagnosticResults += "\n";
        
        // Final Summary
        diagnosticResults += "=== SUMMARY ===\n";
        if (allGood && foundDecks == 6)
        {
            diagnosticResults += "✓ ALL CHECKS PASSED!\n";
            diagnosticResults += "\nYou're ready to test!\n";
            diagnosticResults += "Try: Right-click CombatDeckManager → Load Marauder Deck → Draw Initial Hand\n";
        }
        else
        {
            diagnosticResults += "✗ ISSUES FOUND - Fix the items marked with ✗ above\n";
        }
    }
}


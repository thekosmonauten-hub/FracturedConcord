using System.Linq;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool for testing warrant blueprint rolling.
/// </summary>
public class WarrantBlueprintTester : EditorWindow
{
    private WarrantDatabase warrantDatabase;
    private WarrantLockerGrid lockerGrid;
    private WarrantDefinition selectedBlueprint;
    private int minAffixes = 1;
    private int maxAffixes = 3;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Warrants/Test Blueprint Rolling")]
    public static void ShowWindow()
    {
        GetWindow<WarrantBlueprintTester>("Warrant Blueprint Tester");
    }

    private void OnEnable()
    {
        // Try to auto-find references
        if (warrantDatabase == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:WarrantDatabase");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                warrantDatabase = AssetDatabase.LoadAssetAtPath<WarrantDatabase>(path);
            }
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Warrant Blueprint Rolling Tester", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Database selection
        warrantDatabase = (WarrantDatabase)EditorGUILayout.ObjectField(
            "Warrant Database:",
            warrantDatabase,
            typeof(WarrantDatabase),
            false
        );

        if (warrantDatabase == null)
        {
            EditorGUILayout.HelpBox("Assign a WarrantDatabase to test blueprint rolling.", MessageType.Warning);
            EditorGUILayout.EndScrollView();
            return;
        }

        // Locker Grid selection (optional - will try to find in scene)
        lockerGrid = (WarrantLockerGrid)EditorGUILayout.ObjectField(
            "Warrant Locker Grid (Optional):",
            lockerGrid,
            typeof(WarrantLockerGrid),
            true
        );

        EditorGUILayout.Space();

        // Blueprint selection
        EditorGUILayout.LabelField("Blueprint Selection", EditorStyles.boldLabel);
        
        var allWarrants = warrantDatabase.Definitions;
        if (allWarrants == null || allWarrants.Count == 0)
        {
            EditorGUILayout.HelpBox("No warrants found in database.", MessageType.Info);
            EditorGUILayout.EndScrollView();
            return;
        }

        // Filter to blueprints only
        var blueprints = System.Linq.Enumerable.Where(allWarrants, w => w != null && w.isBlueprint).ToList();
        
        if (blueprints.Count == 0)
        {
            EditorGUILayout.HelpBox("No blueprints found in database. Mark a warrant as 'isBlueprint' to use it.", MessageType.Warning);
        }
        else
        {
            string[] blueprintNames = blueprints.Select(b => $"{b.displayName} ({b.warrantId})").ToArray();
            int currentIndex = selectedBlueprint != null ? blueprints.IndexOf(selectedBlueprint) : -1;
            if (currentIndex < 0) currentIndex = 0;

            int newIndex = EditorGUILayout.Popup("Select Blueprint:", currentIndex, blueprintNames);
            if (newIndex >= 0 && newIndex < blueprints.Count)
            {
                selectedBlueprint = blueprints[newIndex];
            }
        }

        EditorGUILayout.Space();

        // Affix count settings
        EditorGUILayout.LabelField("Rolling Settings", EditorStyles.boldLabel);
        minAffixes = EditorGUILayout.IntField("Min Affixes:", minAffixes);
        maxAffixes = EditorGUILayout.IntField("Max Affixes:", maxAffixes);
        minAffixes = Mathf.Max(0, minAffixes);
        maxAffixes = Mathf.Max(minAffixes, maxAffixes);

        EditorGUILayout.Space();
        
        // Rarity rules info
        EditorGUILayout.LabelField("Rarity Rules", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Rarity is automatically determined by affix count:\n" +
            "• 1 Notable + 1 affix = Common\n" +
            "• 1 Notable + 2 affixes = Magic\n" +
            "• 1 Notable + 3 affixes = Rare\n" +
            "• 1 Notable + 4 affixes = Rare",
            MessageType.Info);

        EditorGUILayout.Space();

        // Roll button
        EditorGUI.BeginDisabledGroup(selectedBlueprint == null || warrantDatabase == null);
        
        if (GUILayout.Button("Roll Warrant from Blueprint", GUILayout.Height(30)))
        {
            RollWarrant();
        }
        
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndScrollView();
    }

    private void RollWarrant()
    {
        if (selectedBlueprint == null)
        {
            EditorUtility.DisplayDialog("Error", "No blueprint selected.", "OK");
            return;
        }

        if (warrantDatabase == null)
        {
            EditorUtility.DisplayDialog("Error", "WarrantDatabase not assigned.", "OK");
            return;
        }

        // Find locker grid if not assigned
        if (lockerGrid == null)
        {
            lockerGrid = FindFirstObjectByType<WarrantLockerGrid>();
            if (lockerGrid == null)
            {
                EditorUtility.DisplayDialog("Error", 
                    "WarrantLockerGrid not found in scene. Please assign it manually or open the Warrant Tree scene.", 
                    "OK");
                return;
            }
        }

        // Roll the warrant (rarity is automatically calculated from affix count)
        WarrantDefinition rolledInstance = WarrantRollingUtility.RollAndAddToLocker(
            selectedBlueprint,
            warrantDatabase,
            lockerGrid,
            minAffixes,
            maxAffixes
        );

        if (rolledInstance != null)
        {
            EditorUtility.DisplayDialog("Success", 
                $"Successfully rolled warrant:\n\n" +
                $"Name: {rolledInstance.displayName}\n" +
                $"ID: {rolledInstance.warrantId}\n" +
                $"Rarity: {rolledInstance.rarity}\n" +
                $"Modifiers: {rolledInstance.modifiers?.Count ?? 0}\n" +
                $"Notable: {(string.IsNullOrEmpty(rolledInstance.notableId) ? "None" : rolledInstance.notableId)}",
                "OK");
            
            // Select the rolled instance in the project
            EditorUtility.FocusProjectWindow();
            // Note: Can't select runtime-created ScriptableObjects, but we can log it
            Debug.Log($"[WarrantBlueprintTester] Rolled warrant instance: {rolledInstance.warrantId}");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", 
                $"Failed to roll warrant from blueprint '{selectedBlueprint.warrantId}'. Check console for details.",
                "OK");
        }
    }
}


using UnityEngine;
using UnityEditor;
using System.Linq;

/// <summary>
/// Custom editor for WarrantDefinition to easily assign Notables from WarrantNotableDatabase.
/// </summary>
[CustomEditor(typeof(WarrantDefinition))]
public class WarrantDefinitionEditor : Editor
{
    private WarrantNotableDatabase notableDatabase;
    private int selectedNotableIndex = -1;
    private string[] notableNames;
    private string[] notableIds;

    private void OnEnable()
    {
        // Try to find the NotableDatabase automatically
        string[] guids = AssetDatabase.FindAssets("t:WarrantNotableDatabase");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            notableDatabase = AssetDatabase.LoadAssetAtPath<WarrantNotableDatabase>(path);
        }
    }

    public override void OnInspectorGUI()
    {
        WarrantDefinition warrant = (WarrantDefinition)target;

        // Draw default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Notable Assignment", EditorStyles.boldLabel);

        // Database selection
        notableDatabase = (WarrantNotableDatabase)EditorGUILayout.ObjectField(
            "Notable Database:",
            notableDatabase,
            typeof(WarrantNotableDatabase),
            false
        );

        if (notableDatabase == null)
        {
            EditorGUILayout.HelpBox(
                "Assign a WarrantNotableDatabase to enable Notable selection.",
                MessageType.Warning
            );
            return;
        }

        // Get all Notables
        var allNotables = notableDatabase.GetAll();
        if (allNotables == null || allNotables.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No Notables found in the database.",
                MessageType.Info
            );
            return;
        }

        // Build arrays for dropdown
        notableNames = allNotables.Select(n => $"{n.displayName} ({n.notableId})").ToArray();
        notableIds = allNotables.Select(n => n.notableId).ToArray();

        // Find current selection
        if (string.IsNullOrEmpty(warrant.notableId))
        {
            selectedNotableIndex = -1;
        }
        else
        {
            selectedNotableIndex = System.Array.IndexOf(notableIds, warrant.notableId);
            if (selectedNotableIndex < 0)
            {
                selectedNotableIndex = -1;
            }
        }

        // Dropdown selection
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Select Notable:", GUILayout.Width(120));
        
        int newIndex = EditorGUILayout.Popup(
            selectedNotableIndex < 0 ? 0 : selectedNotableIndex,
            notableNames
        );

        if (newIndex != selectedNotableIndex && newIndex >= 0 && newIndex < notableIds.Length)
        {
            warrant.notableId = notableIds[newIndex];
            selectedNotableIndex = newIndex;
            EditorUtility.SetDirty(warrant);
        }

        EditorGUILayout.EndHorizontal();

        // Show current Notable info
        if (!string.IsNullOrEmpty(warrant.notableId))
        {
            var notable = notableDatabase.GetById(warrant.notableId);
            if (notable != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Current Notable:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Name: {notable.displayName}");
                EditorGUILayout.LabelField($"ID: {notable.notableId}");
                EditorGUILayout.LabelField($"Modifiers: {notable.modifiers?.Count ?? 0}");
                
                if (notable.modifiers != null && notable.modifiers.Count > 0)
                {
                    EditorGUILayout.LabelField("Modifier List:", EditorStyles.miniLabel);
                    EditorGUI.indentLevel++;
                    foreach (var mod in notable.modifiers)
                    {
                        if (mod == null) continue;
                        string display = !string.IsNullOrWhiteSpace(mod.displayName) 
                            ? mod.displayName 
                            : $"{mod.value:+0.##;-0.##;0}% {mod.statKey}";
                        EditorGUILayout.LabelField($"• {display}", EditorStyles.miniLabel);
                    }
                    EditorGUI.indentLevel--;
                }

                // Button to clear Notable
                if (GUILayout.Button("Clear Notable Assignment", GUILayout.Height(25)))
                {
                    warrant.notableId = string.Empty;
                    warrant.notable = null; // Also clear legacy field
                    selectedNotableIndex = -1;
                    EditorUtility.SetDirty(warrant);
                }
            }
            else
            {
                EditorGUILayout.HelpBox(
                    $"Notable ID '{warrant.notableId}' not found in database.",
                    MessageType.Warning
                );
                
                if (GUILayout.Button("Clear Invalid Notable ID"))
                {
                    warrant.notableId = string.Empty;
                    EditorUtility.SetDirty(warrant);
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox(
                "No Notable assigned. Select one from the dropdown above.",
                MessageType.Info
            );
        }

        EditorGUILayout.Space();

        // Legacy Notable field info
        if (warrant.notable != null)
        {
            EditorGUILayout.HelpBox(
                "Legacy Notable ScriptableObject is assigned. " +
                "Consider using the Notable ID field above for the new system.",
                MessageType.Info
            );
        }
    }
}












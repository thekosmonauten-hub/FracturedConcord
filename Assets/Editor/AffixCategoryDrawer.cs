using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(AffixCategory))]
public class AffixCategoryDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        // Draw the main label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        
        // Get the category name
        SerializedProperty categoryNameProp = property.FindPropertyRelative("categoryName");
        string categoryName = categoryNameProp.stringValue;
        
        // Draw category name with bold style
        GUIStyle boldStyle = new GUIStyle(EditorStyles.label);
        boldStyle.fontStyle = FontStyle.Bold;
        boldStyle.fontSize = 12;
        
        EditorGUI.LabelField(position, categoryName, boldStyle);
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}

[CustomPropertyDrawer(typeof(AffixSubCategory))]
public class AffixSubCategoryDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        // Get the sub-category name
        SerializedProperty subCategoryNameProp = property.FindPropertyRelative("subCategoryName");
        string subCategoryName = subCategoryNameProp.stringValue;
        
        // Draw sub-category name with italic style
        GUIStyle italicStyle = new GUIStyle(EditorStyles.label);
        italicStyle.fontStyle = FontStyle.Italic;
        italicStyle.fontSize = 11;
        
        EditorGUI.LabelField(position, $"  {subCategoryName}", italicStyle);
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}

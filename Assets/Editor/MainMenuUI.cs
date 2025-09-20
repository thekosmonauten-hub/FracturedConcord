using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuUI : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/UI Toolkit/MainMenuUI")]
    public static void ShowExample()
    {
        MainMenuUI wnd = GetWindow<MainMenuUI>();
        wnd.titleContent = new GUIContent("MainMenuUI");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("Hello World! From C#");
        root.Add(label);

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);
    }
}

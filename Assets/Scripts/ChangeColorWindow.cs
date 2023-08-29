using UnityEngine;
using UnityEditor;
public class ChangeColorWindow : EditorWindow
{
    Color myColor;
    [MenuItem("Window/Change Selected Object Color")]
    public static void ShowExampleWindow()
    {
        GetWindow<ChangeColorWindow>("Example");
    }
    void OnGUI()
    {
        GUILayout.Label("Change Selected Objects Color", EditorStyles.boldLabel);
        myColor = EditorGUILayout.ColorField("Color", myColor);
        if (GUILayout.Button("Change Color"))
        {
            Renderer r = Selection.activeGameObject.GetComponent<Renderer>();
            if (r != null)
                r.sharedMaterial.color = myColor;
        }
    }
}

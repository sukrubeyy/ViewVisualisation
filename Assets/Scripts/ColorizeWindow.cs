using UnityEngine;
using UnityEditor;
public class ColorizeWindow : EditorWindow
{
    Color myColor;

    [MenuItem("Window/Example")]
    public static void ShowExampleWindow()
    {
        GetWindow<ColorizeWindow>("Example");
    }


    void OnGUI()
    {
        GUILayout.Label("Change Selected Objects Color", EditorStyles.boldLabel);
        myColor = EditorGUILayout.ColorField("Color", myColor);
        if (GUILayout.Button("If you wanna change selected objects color press me"))
        {
            Renderer r = Selection.activeGameObject.GetComponent<Renderer>();
            if (r != null)
                r.sharedMaterial.color = myColor;
        }
    }
}

using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(FieldOfView))]
public class FovEditor : Editor
{
    void OnSceneGUI()
    {
        FieldOfView fow = (FieldOfView)target;
        Handles.color = Color.yellow;
        Handles.DrawWireArc(fow.transform.position, Vector3.up, Vector3.forward, 360, fow.vRadius);

        Vector3 angleA = fow.DirectionFromAngle(-fow.vAngle / 2, false);
        Vector3 angleB = fow.DirectionFromAngle(fow.vAngle / 2, false);
        Handles.color = Color.green;
        Handles.DrawLine(fow.transform.position, fow.transform.position + angleA * fow.vRadius);
        Handles.color = Color.cyan;
        Handles.DrawLine(fow.transform.position, fow.transform.position + angleB * fow.vRadius);

        Handles.color = Color.blue;
        foreach (Transform item in fow.visibleTargets)
        {
            Handles.DrawLine(fow.transform.position,item.position);
        }
    }
}

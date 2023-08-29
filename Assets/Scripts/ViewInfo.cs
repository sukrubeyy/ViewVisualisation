using UnityEngine;

public struct ViewInfo
{   
    public Vector3 point;
    public bool hit;
    public float distance;
    public float angle;

    public ViewInfo(Vector3 _point, float _distance, float _angle, bool _hit)
    {
        point = _point;
        distance = _distance;
        angle = _angle;
        hit = _hit;
    }
}
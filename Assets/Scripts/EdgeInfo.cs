using UnityEngine;
public struct EdgeInfo
{
    public Vector3 pointA;
    public Vector3 pointB;
    public EdgeInfo(Vector3 _a, Vector3 _b)
    {
        pointA = _a;
        pointB = _b;
    }
}
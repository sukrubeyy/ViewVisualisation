using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [Header("Mathematical Variable")]
    public float vRadius;
    [Range(0, 360)]
    public float vAngle;
    public float meshResolution;
    public float edgeDstThreshold;
    public int edgeRIterations;
    public float maskCutAway = .1f;

    [Header("Masks")]
    public LayerMask obsMask;
    public LayerMask targetMask;
    [Header("Components")]
    public MeshFilter meshFilter;
    Mesh mesh;
    public List<Transform> visibleTargets = new List<Transform>();
    void Start()
    {
        mesh = new Mesh();
        mesh.name = "Sukonun Meshi";
        meshFilter.mesh = mesh;
        StartCoroutine(FindAllVisibleTargetWithTime(.2f));
    }
    void LateUpdate()
    {
        DrawLineMesh();
    }
    void DrawLineMesh()
    {
        int stepCount = Mathf.RoundToInt(vAngle * meshResolution);
        float stepAngle = vAngle / stepCount;
        List<Vector3> newPoints = new List<Vector3>();
        ViewInfo oldView = new ViewInfo();
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - vAngle / 2 + stepAngle * i;
            ViewInfo newView = ViewCast(angle);
            if (i > 0)
            {
                bool edgeDstThresholdExceeded = Mathf.Abs(oldView.distance - newView.distance) > edgeDstThreshold;
                if (oldView.hit != newView.hit || (oldView.hit && newView.hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edgeInfo = FindEdge(oldView, newView);
                    if (edgeInfo.pointA != Vector3.zero)
                    {
                        newPoints.Add(edgeInfo.pointA);
                    }
                    if (edgeInfo.pointB != Vector3.zero)
                    {
                        newPoints.Add(edgeInfo.pointB);
                    }
                }
            }
            newPoints.Add(newView.point);
            oldView = newView;
        }
        int vertexCount = newPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangle = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(newPoints[i]) + Vector3.forward * maskCutAway;
            if (i < vertexCount - 2)
            {
                triangle[i * 3] = 0;
                triangle[i * 3 + 1] = i + 1;
                triangle[i * 3 + 2] = i + 2;
            }
        }
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangle;
        mesh.RecalculateNormals();
    }

    ViewInfo ViewCast(float globalAngle)
    {
        Vector3 direction = DirectionFromAngle(globalAngle, true);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, vRadius, obsMask))
        {
            return new ViewInfo(hit.point, hit.distance, globalAngle, true);
        }
        else
            return new ViewInfo(transform.position + direction * vRadius, vRadius, globalAngle, false);
    }

    EdgeInfo FindEdge(ViewInfo minView, ViewInfo maxView)
    {
        float minAngle = minView.angle;
        float maxAngle = maxView.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;
        for (int i = 0; i < edgeRIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewInfo newView = ViewCast(angle);
            bool edgeDstThresholdExceeded = Mathf.Abs(minView.distance - newView.distance) > edgeDstThreshold;
            if (newView.hit == minView.hit)
            {
                minAngle = angle;
                minPoint = newView.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newView.point;
            }
        }
        return new EdgeInfo(minPoint, maxPoint);
    }

    public Vector3 DirectionFromAngle(float angleDegrees, bool globalAngle)
    {
        if (!globalAngle)
        {
            angleDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleDegrees * Mathf.Deg2Rad));
    }

    IEnumerator FindAllVisibleTargetWithTime(float time)
    {
        while (true)
        {
            yield return new WaitForSeconds(time);
            FindVisibleTargets();
        }
    }

    private void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsColliders = Physics.OverlapSphere(transform.position, vRadius, targetMask);
        for (int i = 0; i < targetsColliders.Length; i++)
        {
            Transform target = targetsColliders[i].transform;
            //Sadece direction aldığı için bize vector3'ün büyüklüğü yani magnitude lazım değil bu yüzden
            //normalized ediyoruz.
            Vector3 directionTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionTarget) < vAngle / 2)
            {
                float distanceTarget = Vector3.Distance(transform.position, target.position);
                //Target ile aramdaki mesafe kadar olan uzaklıkta herhangi bir obstacle olup olmadığını
                //Kontrol ediyoruz.
                if (!Physics.Raycast(transform.position, directionTarget, distanceTarget, obsMask))
                {
                    visibleTargets.Add(target);
                }
            }

        }
    }
}

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
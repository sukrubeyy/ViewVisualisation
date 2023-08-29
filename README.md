<h1>🌟 Field Of View Visualisation 🌟</h1>
<h2>🚀 Description</h2>
<ul>
  <li>Shaderlar</li>
    <ul>
      <li>Stencil  Mask</li>
       <ul>
         <li>We create a stencil mask surface shader so that objects that are not in the character's field of view are not rendered by the camera.</li>
         <li>
<pre> 
<code>
Shader "Custom/Stencil Mask"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-100" }
        ColorMask 0
        ZWrite Off
        LOD 200
        //Stencl Operation
        Stencil{
            Ref 1
            Pass replace
        }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        sampler2D _MainTex;
        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

</code>
</pre>
         </li>
         <li>Stencil Object</li>
         <ul>
           <li>We create this shader for the objects that the mask we created above will affect. That is for enemy and obstacle objects.</li>
           <li>
             <pre>
               <code>
Shader "Custom/Stencil Object"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        //Stencil Operation
        Stencil{
            Ref 1
            Comp equal
        }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        sampler2D _MainTex;
        struct Input
        {
            float2 uv_MainTex;
        };
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
               </code>
               </pre>
               </li>
         </ul>
       </ul>
    </ul>
    <li>Character Controller </li>
    <li>
<pre>
<code>
using UnityEngine;
public class Controller : MonoBehaviour
{
    Rigidbody rb;
    Camera viewCamera;
    Vector3 input;
    public float moveSpeed = 6;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        viewCamera = Camera.main;
    }
    void Update()
    {
        input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        Vector3 mousePos = viewCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, viewCamera.transform.position.y));
        transform.LookAt(mousePos + Vector3.up * transform.position.y);
    }
    void FixedUpdate()
    {
        rb.MovePosition(transform.position + input * Time.deltaTime * moveSpeed);
    }
}
</code>
</pre>
    </li>
          <li>Field Of View</li>
          <ul>
            <li>Structs</li>
            <ul>
              <li>EdgeInfo</li>
              <p>It was created to prevent edge breaks and distortion of the mesh to be drawn.</p>
<li>
<pre>
<code>
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
</code>
</pre>
</li>
              <li>ViewInfo</li>
              <p>It was created to contain the information of the rays drawn from the viewpoint and contacting the obstacle or enemy objects.</p>
<li>
<pre>
<code>
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
</code>
</pre>
</li>
</ul>
<li>
  <pre>
    <code>
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
        mesh.name = "MeshOfSukruBeyy";
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
    </code>
  </pre>
</li>
</ul>
<li>Change Selected Objects Color Editor</li>
<pre>
  <code>
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

  </code>
</pre>

<img src="https://github.com/sukrubeyy/ViewVisualisation/blob/main/Assets/Images/fOv.gif"/>
</ul>



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Perception))]
public class FieldOfView : MonoBehaviour
{
    #region Variables

    /// <summary>
    /// The radius (distance) the agent can see
    /// </summary>
    public float viewRadius = 15f;

    /// <summary>
    /// The angle (in degrees) the agent can see. Range of 0 to 360
    /// </summary>
    [Range(0, 360)]
    public float viewAngle = 70f;

    /// <summary>
    /// The layer of target objects (what we're trying to sense)
    /// </summary>
    public LayerMask targetLayer;

    /// <summary>
    /// The layer of obstacles (things that should block our line of sight)
    /// </summary>
    public LayerMask obstacleLayer;

    /// <summary>
    /// List of visible targets us updated at a set interval
    /// </summary>
    public List<Transform> visibleTargets = new List<Transform>();

    // Mesh Drawing
    public float meshResolution = 10;
    public int edgeResolveIterations = 1;
    public float edgeDstThreshold = 1;
    public MeshFilter viewMeshFilter;
    private Mesh viewMesh;
    public bool drawFOV = true;

    public int memoryDuration = 10;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        // Initialising the mesh drawing
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;

        // Check for visible targets every 0.2 seconds
        InvokeRepeating("FindVisibleTargets", 0.2f, 0.2f);

        //Perception percept = GetComponent<Perception>();
    }

    private void FindVisibleTargets()
    {
        // Clear current visible targets
        visibleTargets.Clear();

        // Do simple sphere collision check for nearby targets
        Collider[] targets = Physics.OverlapSphere(transform.position, viewRadius, targetLayer);

        foreach (Collider target in targets)
        {
            // Get direction and magnitude to target
            Vector3 toTarget = (target.transform.position - transform.position);

            // Normalize to get direction without magnitude
            Vector3 toTargetNormalized = toTarget.normalized;

            if (Vector3.Angle(transform.forward, toTargetNormalized) < viewAngle / 2 // Check if target is within FOV
                && !Physics.Raycast(transform.position, toTargetNormalized, toTarget.magnitude, obstacleLayer)) // do raycast to determine LoS
            {
                // Can be seen
                visibleTargets.Add(target.transform);
            }
        }

        // Add memory record to our perception system
        Perception percept = GetComponent<Perception>();

        percept.ClearFoV();

        foreach (Transform target in visibleTargets)
        {
            percept.AddMemory(target.gameObject);
        }
    }

    private void Update()
    {
        Perception percept = GetComponent<Perception>();

        if (percept.memoryMap.Count > 0)
        {
            MemoryRecord[] records = new MemoryRecord[percept.memoryMap.Values.Count];
            percept.memoryMap.Values.CopyTo(records, 0);

            for (int i = 0; i < records.Length; i++)
            {
                TimeSpan interval = DateTime.Now - records[i].timeLastSensed;
                //Debug.Log("Interval = " + interval);

                if (interval.TotalSeconds >= memoryDuration)
                {
                    records[i].timeLimitReached = true;
                }                
            }
        }

    }

    private void LateUpdate()
    {
        // Draw or hide our FoV
        if (drawFOV)
        {
            viewMeshFilter.gameObject.SetActive(true);
            DrawFieldOfView();

            foreach (Transform target in visibleTargets)
            {
                Debug.DrawLine(transform.position, target.position, Color.red);
            }
        }
        else
        {
            viewMeshFilter.gameObject.SetActive(false);
        }
    }

    // Draws FoV for debug purposes, from the internet
    private void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();

        for (int i = 0; i < stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0)
            {
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit  && newViewCast.hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;

        for (int i =0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();

        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    private EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    private ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, obstacleLayer))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }
}



//foreach (KeyValuePair<GameObject, MemoryRecord> memory in percept.memoryMap)
//{
//    TimeSpan interval = DateTime.Now - memory.Value.timeLastSensed;

//    Debug.Log("Running 2nd foreach loop");
//    Debug.Log("Interval = " + interval);

//    if (interval.TotalSeconds >= 10)
//    {
//        memory.Value.timeLimitReached = true;
//    }

//}


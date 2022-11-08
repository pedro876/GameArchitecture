using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class MeshScanner : MonoBehaviour
{
    [SerializeField] public Transform checkPoint;
    [SerializeField] public Mesh mesh;
    [SerializeField] public int scanResPerUnit = 32;
    [SerializeField] public int finalResolution;
    [SerializeField] public Vector3 startPos;
    [SerializeField] public Vector3 endPos;
    [SerializeField] public int lastHits;
    [SerializeField] public float lastMinDist;
    [SerializeField] public int triangleCheck;
    [SerializeField] public float step;
    [SerializeField] public float threshold;

    private Vector3[] vertices;
    private int[] triangles;

    private void OnEnable()
    {
        mesh = GetComponent<MeshFilter>().sharedMesh;
        Vector3 size = mesh.bounds.size;
        float maxSize = Mathf.Max(size.x, size.y, size.z);
        finalResolution = Mathf.CeilToInt(maxSize * scanResPerUnit);
        startPos = mesh.bounds.center - Vector3.one * maxSize*0.5f;
        endPos = mesh.bounds.center + Vector3.one * maxSize*0.5f;
        step = 1f / scanResPerUnit;
        threshold = step * 0.5f;
        vertices = mesh.vertices;
        triangles = mesh.triangles;
    }

    /*private void OnValidate()
    {
        Recalculate();
    }*/

    public void Recalculate()
    {
        Vector3 size = mesh.bounds.size;
        float maxSize = Mathf.Max(size.x, size.y, size.z);
        finalResolution = Mathf.CeilToInt(maxSize * scanResPerUnit);
        startPos = mesh.bounds.center - Vector3.one * maxSize * 0.5f;
        endPos = mesh.bounds.center + Vector3.one * maxSize * 0.5f;
        step = 1f / scanResPerUnit;
        threshold = step * 0.5f;
        vertices = mesh.vertices;
        triangles = mesh.triangles;
    }

    private void OnDrawGizmos()
    {
        Bounds bounds = mesh.bounds;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        Gizmos.DrawWireSphere(endPos, 0.1f);
        Gizmos.DrawLine(startPos, endPos);

        if(checkPoint != null)
        {
            bool insideMesh = PointInMesh(checkPoint.transform.position, out Vector3 firstHit);
            Gizmos.color = insideMesh ? Color.red : Color.blue;
            Gizmos.DrawWireSphere(checkPoint.transform.position, 0.1f);
            if (insideMesh)
            {
                Gizmos.DrawLine(checkPoint.transform.position, firstHit);
            }
            /*Vector3 p = checkPoint.transform.position;
            Vector3 dir = Vector3.right;
            Vector3 a = vertices[triangles[triangleCheck * 3]];
            Vector3 b = vertices[triangles[triangleCheck * 3 + 1]];
            Vector3 c = vertices[triangles[triangleCheck * 3 + 2]];
            Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
            Vector3 projP = ProjectLineOnPlane(p, dir, a, normal);
            bool isHit = PointInTriangle(projP, a, b, c);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(c, b);
            Gizmos.DrawLine(a, c);
            Gizmos.DrawLine(a, a + normal);
            Gizmos.color = isHit ? Color.red : Color.blue;
            Gizmos.DrawLine(p, projP);
            Gizmos.DrawWireSphere(p, 0.1f);
            Gizmos.DrawWireSphere(projP, 0.1f);*/
        }
    }

    public Vector3 ProjectLineOnPlane(Vector3 linePoint, Vector3 lineDir, Vector3 planePoint, Vector3 planeNormal, out bool parallel)
    {
        // x = lx + la * t
        // y = ly + lb * t
        // z = lz + lc * t
        float lx = linePoint.x;
        float ly = linePoint.y;
        float lz = linePoint.z;
        float la = lineDir.x;
        float lb = lineDir.y;
        float lc = lineDir.z;

        // pa(x-px)+pb(y-py)+pc(z-pz) = 0
        float px = planePoint.x;
        float py = planePoint.y;
        float pz = planePoint.z;
        float pa = planeNormal.x;
        float pb = planeNormal.y;
        float pc = planeNormal.z;


        /*
         * pa(lx + la * t -px)+pb(ly + lb * t-py)+pc(lz + lc * t-pz) = 0
         * pa*lx+pa*la*t-pa*px + pb*ly+pb*lb*t-pb*py + pc*lz+pc*lc*t-pc*pz = 0
         * pa*la*t + pb*lb*t + pc*lc*t = -pa*lx+pa*px -pb*ly+pb*py -pc*lz+pc*pz
         * t * (pa*la + pb*lb + pc*lc) = -pa*lx+pa*px -pb*ly+pb*py -pc*lz+pc*pz
         * t = (-pa*lx+pa*px -pb*ly+pb*py -pc*lz+pc*pz) / (pa*la + pb*lb + pc*lc)
         */

        float t = (-pa * lx + pa * px - pb * ly + pb * py - pc * lz + pc * pz) / (pa * la + pb * lb + pc * lc);
        float x = lx + la * t;
        float y = ly + lb * t;
        float z = lz + lc * t;
        parallel = Mathf.Abs(Vector3.Dot(lineDir, planeNormal)) < 0.0001f;
        return new Vector3(x, y, z);
    }

    //https://gdbooks.gitbooks.io/3dcollisions/content/Chapter4/point_in_triangle.html
    public bool PointInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        // Lets define some local variables, we can change these
        // without affecting the references passed in

        // Move the triangle so that the point becomes the 
        // triangles origin
        a -= p;
        b -= p;
        c -= p;

        // The point should be moved too, so they are both
        // relative, but because we don't use p in the
        // equation anymore, we don't need it!
        // p -= p;

        // Compute the normal vectors for triangles:
        // u = normal of PBC
        // v = normal of PCA
        // w = normal of PAB

        Vector3 u = Vector3.Cross(b, c);
        Vector3 v = Vector3.Cross(c, a);
        Vector3 w = Vector3.Cross(a, b);

        // Test to see if the normals are facing 
        // the same direction, return false if not
        if (Vector3.Dot(u, v) < 0f)
        {
            return false;
        }
        if (Vector3.Dot(u, w) < 0.0f)
        {
            return false;
        }

        // All normals facing the same way, return true
        return true;
    }

    public int RayCast(Vector3 p, Vector3 dir, out Vector3 firstHit)
    {
        int hits = 0;
        firstHit = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        for (int i = 0; i < triangles.Length / 3; i++)
        {
            Vector3 a = vertices[triangles[i*3]];
            Vector3 b = vertices[triangles[i*3+1]];
            Vector3 c = vertices[triangles[i*3+2]];
            Vector3 normal = Vector3.Cross(b-a, c-a).normalized;
            Vector3 projP = ProjectLineOnPlane(p, dir, a, normal, out bool parallel);
            Vector3 toProjP = projP - p;
            bool lookingAtPlane = Vector3.Dot(toProjP, dir) > 0;
            bool isHit = !parallel && lookingAtPlane && PointInTriangle(projP, a, b, c);

            if (isHit)
            {
                if(hits == 0)
                {
                    firstHit = projP;
                }
                hits++;
            }
        }
            
        return hits;
    }

    public float DistanceToMesh(Vector3 p, out Vector3 hit)
    {
        int hits = RayCast(p, Vector3.right, out hit);
        lastHits = hits;
        //if (hits == 0) return false;
        if (hits % 2 == 1) return 0f;
        float minDist = float.MaxValue;
        for (int i = 0; i < triangles.Length / 3; i++)
        {
            Vector3 a = vertices[triangles[i * 3]];
            Vector3 b = vertices[triangles[i * 3 + 1]];
            Vector3 c = vertices[triangles[i * 3 + 2]];
            Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
            Vector3 projP = ProjectLineOnPlane(p, -normal, a, normal, out bool parallel);
            if(PointInTriangle(projP, a, b, c))
            {
                float dist = Vector3.Distance(p, projP);
                if (dist < minDist)
                {
                    minDist = dist;
                    hit = projP;
                }
            }
            
        }
        return minDist;
    }

    public bool PointInMesh(Vector3 p, out Vector3 firstHit)
    {
        float distance = DistanceToMesh(p, out firstHit);
        lastMinDist = distance;
        return distance <= threshold;
        /*int hits = RayCast(p, Vector3.right, out firstHit);
        lastHits = hits;
        if (hits == 0) return false;
        if (hits % 2 == 1) return true;
        float distToFirstHit = Vector3.Distance(p, firstHit);
        return distToFirstHit <= threshold;*/
    }
}
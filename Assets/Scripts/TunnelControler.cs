using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathGenerator), typeof(MeshGenerator), typeof(PawnGenerator))]
public class TunnelControler : MonoBehaviour
{
    [Header("Tunnel Properties")]
    [SerializeField] private int initialNumberSegments = 5;
    [SerializeField] private float tunnelCurveRange = 7f;
    [SerializeField] private float distanceBetweenSegments = 150f;

    [SerializeField] private int segmentsToReload = 1;
    [SerializeField] private float transformDistance = 2000f;


    PathGenerator pathGenerator;
    MeshGenerator meshGenerator;
    PawnGenerator pawnGenerator;
    

    // Start is called before the first frame update
    void Awake()
    {
        pathGenerator = GetComponent<PathGenerator>();
        meshGenerator = GetComponent<MeshGenerator>();
        pawnGenerator = GetComponent<PawnGenerator>();

        pathGenerator.CreatePath(new Vector3[] { new Vector3(0f, 0f, -50f), new Vector3(0f, 0f, 300f), new Vector3(0f, 0f, 500f) });
        pathGenerator.AddToPath(GenerateRandomPoints(initialNumberSegments));

        meshGenerator.CreateMesh();
        pawnGenerator.SpawnPawns();
    }

    Vector3 GenerateRandomPoint(float zDistance)
    {
        float x = pathGenerator.LastAnchor.anchor.x + Random.Range(-tunnelCurveRange, tunnelCurveRange);
        float y = pathGenerator.LastAnchor.anchor.y + Random.Range(-tunnelCurveRange, tunnelCurveRange);
        float z = pathGenerator.LastAnchor.anchor.z + zDistance;
        return new Vector3(x, y, z);
    }

    Vector3[] GenerateRandomPoints(int amount)
    {
        Vector3[] points = new Vector3[amount];
        for (int i = 0; i < amount; i++)
        {
            float zDistance = distanceBetweenSegments * (i + 1);
            points[i] = GenerateRandomPoint(zDistance);
        }
        return points;
    }

    

    public void GenerateNewSegment()
    {
        pathGenerator.RemoveFromPath(segmentsToReload);
        pathGenerator.AddToPath(GenerateRandomPoints(segmentsToReload));

        meshGenerator.RemoveFromMesh(pathGenerator.LastRemovedAmount);
        meshGenerator.AddToMesh(segmentsToReload);

        pawnGenerator.OverrideDistance();
        pawnGenerator.SpawnPawns();
    }

    public void TransformTunnel()
    {
        float targetDistance = transformDistance * 2;
        pathGenerator.TransformPath(targetDistance);
        pawnGenerator.TransformPawns(targetDistance);
        meshGenerator.CreateMesh();
    }

    public float TransformDistance
    {
        get
        {
            return transformDistance;
        }
    }

    public float ReloadDistance
    {
        get
        {
            return pathGenerator.bezierPath[segmentsToReload + 1].anchor.z;
        }
    }

    public float LastRemovedDistance
    {
        get
        {
            return pathGenerator.LastRemovedLength;
        }
    }

    public float PathLength
    {
        get
        {
            return pathGenerator.path[pathGenerator.LastVertIndex].cumulativeLength;
        }
    }

    public float TunnelWidth
    {
        get
        {
            return meshGenerator.TunnelWidth;
        }
    }

    public float TunnelHeight
    {
        get
        {
            return meshGenerator.TunnelHeight;
        }
    }

    public Vector3 GetPointAtDistance(float distance)
    {
        return pathGenerator.GetPointAtDistance(distance);
    }

    public Quaternion GetRotationAtDistance(float distance)
    {
        return pathGenerator.GetRotationAtDistance(distance);
    }

    /// Gets right vector on path based on distance travelled.
    public Vector3 GetRightAtDistance(float distance)
    {
        return pathGenerator.GetRightAtDistance(distance);
    }

    /// Gets up vector on path based on distance travelled.
    public Vector3 GetUpAtDistance(float distance)
    {
        return pathGenerator.GetUpAtDistance(distance);
    }



    #region PathViewer

#if UNITY_EDITOR

    [Header("Path Viewer")]
    [SerializeField] bool showPath = false;

    [Space]
    [SerializeField] bool showAnchors = false;
    [SerializeField] float anchorSize = 1f;

    [Space]
    [SerializeField] bool showVerts = false;
    [SerializeField] bool showNormals = false;

    [Space]
    [SerializeField] bool showInfo = false;
    [SerializeField] int textSize = 12;
    [SerializeField] int textOffset = 16;

    //[Space]
    //[SerializeField] bool showMeshPath = false;
    //[SerializeField] bool showMeshPoints = false;

    void OnDrawGizmos()
    {
        if (pathGenerator != null)
        {
            if (showPath || showNormals || showVerts)
            {

                for (int i = 0; i < pathGenerator.path.Count; i++)
                {
                    if (showPath)
                    {
                        int nextI = i + 1;
                        if (nextI >= pathGenerator.path.Count)
                        {
                            break;
                        }

                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(pathGenerator.path[i].pos, pathGenerator.path[nextI].pos);
                    }
                    if (showNormals)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(pathGenerator.path[i].pos, pathGenerator.path[i].pos + pathGenerator.path[i].right);

                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(pathGenerator.path[i].pos, pathGenerator.path[i].pos + pathGenerator.path[i].up);
                    }
                    if (showVerts)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawSphere(pathGenerator.path[i].pos, anchorSize);

                        if (showInfo)
                        {
                            string text = string.Format("pos:{0}\nlength:{1}\ntime:{2}",
                                pathGenerator.path[i].pos,
                                pathGenerator.path[i].cumulativeLength,
                                pathGenerator.path[i].time
                                );

                            GizmosUtils.DrawText(GUI.skin, text, pathGenerator.path[i].pos, Color.white, textSize, textOffset);
                        }
                    }
                }
            }
            if (showAnchors)
            {
                for (int i = 0; i < pathGenerator.bezierPath.Count; i++)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(pathGenerator.bezierPath[i].anchor, anchorSize);

                    if (showInfo)
                    {
                        string text = string.Format("anch:{0}\nctrl1:{1}\nctrl2:{2}",
                            pathGenerator.bezierPath[i].anchor,
                            pathGenerator.bezierPath[i].ctrl1,
                            pathGenerator.bezierPath[i].ctrl2
                        );

                        GizmosUtils.DrawText(GUI.skin, text, pathGenerator.bezierPath[i].anchor, Color.white, textSize, textOffset);
                    }
                }
            }
            //if (showMeshPath || showMeshPoints)
            //{
            //    for (int i = 0; i < meshGenerator.debugPoints.Count; i++)
            //    {
            //        if (showMeshPoints)
            //        {
            //            Gizmos.color = Color.white;
            //            Gizmos.DrawSphere(meshGenerator.debugPoints[i], anchorSize);
            //        }
            //        if (showMeshPath)
            //        {
            //            int nextI = i + 1;
            //            if (nextI >= pathGenerator.path.Count)
            //            {
            //                break;
            //            }
            //
            //            Gizmos.color = Color.yellow;
            //            Gizmos.DrawLine(meshGenerator.debugPoints[i], meshGenerator.debugPoints[nextI]);
            //        }
            //    }
            //}
        }
    }

#endif

    #endregion
}

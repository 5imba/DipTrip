using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathGenerator : MonoBehaviour
{
    #region Fields

    // Default values and constants:
    [Header("Bezier Path")]
    [HideInInspector] public List<BezierPoint> bezierPath;
    [SerializeField] public float autoControlLength = .3f;

    [Header("Vertex Path")]
    [HideInInspector] public List<VertexPoint> path;
    [SerializeField] public float maxAngleError = .3f; // How much can the angle of the path change before a vertex is added. This allows fewer vertices to be generated in straighter sections.
    [SerializeField] public float minVertexDst = .01f; // Vertices won't be added closer together than this distance, regardless of angle error.
    [SerializeField] public float accuracy = 10; // A scalar for how many times bezier path is divided when determining vertex positions

    /// Total distance between the vertices of the polyline
    float length;

    List<int> anchorVertexMap;
    float lastRemovedLength = 0f;
    float newSegmentStartLength = 0f;
    int lastRemovedAmount;
    BezierPoint lastAnchor;
    Vector3 lastRotationAxis = Vector3.up;

    #endregion

    #region Constructors

    public void CreatePath(IEnumerable<Vector3> points)
    {
        bezierPath = new List<BezierPoint>();
        path = new List<VertexPoint>();

        CreateBezierPath(points);
        CreateVertexPath();
    }

    public void TransformPath(float distance)
    {
        for (int i = 0; i < bezierPath.Count; i++)
        {
            bezierPath[i].anchor.z -= distance;
            bezierPath[i].ctrl1.z -= distance;
            bezierPath[i].ctrl2.z -= distance;
        }

        lastAnchor.anchor.z -= distance;
        lastAnchor.ctrl1.z -= distance;
        lastAnchor.ctrl2.z -= distance;

        for (int i = 0; i < path.Count; i++)
        {
            path[i].pos.z -= distance;
        }
    }

    public void AddToPath(IEnumerable<Vector3> points)
    {
        int amount = points.Count();
        AddToBezierPath(points);
        AddToVertexPath(amount);
    }

    public void RemoveFromPath(int amount)
    {
        bezierPath.RemoveRange(0, amount);
        anchorVertexMap.RemoveRange(0, amount);

        int anchorToRemove = anchorVertexMap[0];
        path.RemoveRange(0, anchorToRemove);

        //RecalculateAnchorMap();

        for (int i = 0; i < anchorVertexMap.Count; i++)
        {
            anchorVertexMap[i] -= anchorToRemove;
        }

        float cumulativeLengthToRemove = path[0].cumulativeLength;
        for (int i = 0; i < path.Count; i++)
        {
            path[i].cumulativeLength -= cumulativeLengthToRemove;
        }

        length = path[LastVertIndex].cumulativeLength;
        lastRemovedLength = cumulativeLengthToRemove;
        lastRemovedAmount = anchorToRemove;
        newSegmentStartLength = path[LastVertIndex].cumulativeLength;
    }

    void AddToBezierPath(IEnumerable<Vector3> points)
    {
        Vector3[] p = points.ToArray();

        for (int i = 0; i < p.Length; i++)
        {
            AddToBezierPath(p[i]);
        }
    }

    void AddToBezierPath(Vector3 point)
    {
        Vector3 prevPoint = bezierPath[bezierPath.Count - 1].anchor;

        Vector3[] ctrl = Utils.GetAnchorControlPoints(prevPoint, lastAnchor.anchor, point, autoControlLength);
        bezierPath.Add(new BezierPoint(lastAnchor.anchor, ctrl[0], ctrl[1]));

        lastAnchor = new BezierPoint(point, Vector3.zero, Vector3.zero);
    }

    void CreateBezierPath(IEnumerable<Vector3> points)
    {
        Vector3[] p = points.ToArray();

        int penultIndex = p.Length - 1;
        for (int i = 0; i < p.Length; i++)
        {
            if (i == 0)
            {
                Vector3[] ctrl = Utils.GetAnchorControlPoints(p[i] - Vector3.forward, p[i], p[i + 1], autoControlLength);
                bezierPath.Add(new BezierPoint(p[i], Vector3.zero, ctrl[1]));
            }
            else if (i == penultIndex)
            {
                lastAnchor = new BezierPoint(p[i], Vector3.zero, Vector3.zero);
            }
            else
            {
                Vector3[] ctrl = Utils.GetAnchorControlPoints(p[i - 1], p[i], p[i + 1], autoControlLength);
                bezierPath.Add(new BezierPoint(p[i], ctrl[0], ctrl[1]));
            }
        }
    }

    void AddToVertexPath(int numSegments)
    {
        int segmentStartindex = bezierPath.Count - numSegments - 1;

        for (int segmentIndex = segmentStartindex; segmentIndex < bezierPath.Count - 1; segmentIndex++)
        {
            CalculateVertexPointsOnSegment(segmentIndex);
        }

        CalculateTimes();
    }

    void CreateVertexPath()
    {
        Vector3 tangent = Utils.EvaluateCurveDerivative(GetPathSegment(0), 0).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;
        Vector3 up = Vector3.up;
        path.Add(new VertexPoint(bezierPath[0].anchor, tangent, up, right, 0, 0));

        anchorVertexMap = new List<int>();
        anchorVertexMap.Add(0);

        for (int segmentIndex = 0; segmentIndex < bezierPath.Count - 1; segmentIndex++)
        {
            CalculateVertexPointsOnSegment(segmentIndex);
        }

        CalculateTimes();
    }

    void CalculateVertexPointsOnSegment(int segmentIndex)
    {
        Vector3 prevPointOnPath = path[LastVertIndex].pos;
        Vector3 lastAddedPoint = path[LastVertIndex].pos;
        Vector3 tangent;

        Vector3[] segmentPoints = GetPathSegment(segmentIndex);
        float estimatedSegmentLength = Utils.EstimateCurveLength(segmentPoints[0], segmentPoints[1], segmentPoints[2], segmentPoints[3]);
        int divisions = Mathf.CeilToInt(estimatedSegmentLength * accuracy);
        float increment = 1f / divisions;
        float dstSinceLastVertex = 0;

        for (float t = increment; t <= 1; t += increment)
        {
            bool isLastPointOnPath = (t + increment > 1 && segmentIndex == bezierPath.Count - 2);
            if (isLastPointOnPath) t = 1;

            Vector3 pointOnPath = Utils.EvaluateCurve(segmentPoints, t);
            Vector3 nextPointOnPath = Utils.EvaluateCurve(segmentPoints, t + increment);

            // angle at current point on path
            float localAngle = 180 - Utils.MinAngle(prevPointOnPath, pointOnPath, nextPointOnPath);
            // angle between the last added vertex, the current point on the path, and the next point on the path
            float angleFromPrevVertex = 180 - Utils.MinAngle(lastAddedPoint, pointOnPath, nextPointOnPath);
            float angleError = Mathf.Max(localAngle, angleFromPrevVertex);

            if ((angleError > maxAngleError && dstSinceLastVertex >= minVertexDst) || isLastPointOnPath)
            {
                length += (lastAddedPoint - pointOnPath).magnitude;
                dstSinceLastVertex = 0;
                lastAddedPoint = pointOnPath;

                // First reflection
                Vector3 offset = (pointOnPath - path[LastVertIndex].pos);
                float sqrDst = offset.sqrMagnitude;

                Vector3 r1 = lastRotationAxis - offset * 2 / sqrDst * Vector3.Dot(offset, lastRotationAxis);
                Vector3 r2 = path[LastVertIndex].forward - offset * 2 / sqrDst * Vector3.Dot(offset, path[LastVertIndex].forward);

                // Calculate current tangent
                tangent = Utils.EvaluateCurveDerivative(segmentPoints, t).normalized;

                // Second reflection
                Vector3 v2 = tangent - r2;
                float c2 = Vector3.Dot(v2, v2);

                // Calculate current normal
                Vector3 finalRot = r1 - v2 * 2 / c2 * Vector3.Dot(v2, r1);
                Vector3 normal = Vector3.Cross(finalRot, tangent).normalized;
                Vector3 cross = finalRot.normalized;

                lastRotationAxis = finalRot;

                path.Add(new VertexPoint(pointOnPath, tangent, cross, normal, length, 0));
            }
            else
            {
                dstSinceLastVertex += (pointOnPath - prevPointOnPath).magnitude;
            }
            prevPointOnPath = pointOnPath;
        }
        anchorVertexMap.Add(path.Count - 1);
    }

    #endregion

    #region Public methods and accessors

    public BezierPoint LastAnchor
    {
        get
        {
            return lastAnchor;
        }
    }

    public int LastVertIndex
    {
        get
        {
            return path.Count - 1;
        }
    }

    public float LastRemovedLength
    {
        get 
        { 
            return lastRemovedLength; 
        }
    }

    public float NewSegmentStartLength
    {
        get
        {
            return newSegmentStartLength;
        }
    }

    public int LastRemovedAmount
    {
        get 
        { 
            return lastRemovedAmount; 
        }
    }

    public int VertexMap(int i)
    {
        return anchorVertexMap[anchorVertexMap.Count - 1 - i];
    }

    public Vector3[] GetPathSegment(int segmentIndex)
    {
        return new Vector3[] { 
            bezierPath[segmentIndex].anchor, 
            bezierPath[segmentIndex].ctrl2, 
            bezierPath[segmentIndex + 1].ctrl1, 
            bezierPath[segmentIndex + 1].anchor 
        };
    }

    /// Gets point on path based on distance travelled.
    public Vector3 GetPointAtDistance(float dst)
    {
        float t = dst / length;
        return GetPointAtTime(t);
    }

    /// Gets up vector on path based on distance travelled.
    public Vector3 GetUpAtDistance(float dst)
    {
        float t = dst / length;
        return GetUp(t);
    }

    /// Gets up vector on path based on 'time' (where 0 is start, and 1 is end of path).
    public Vector3 GetUp(float t)
    {
        var data = CalculatePercentOnPathData(t);
        return Vector3.Lerp(path[data.previousIndex].up, path[data.nextIndex].up, data.percentBetweenIndices);
    }

    /// Gets right vector on path based on distance travelled.
    public Vector3 GetRightAtDistance(float dst)
    {
        float t = dst / length;
        return GetRight(t);
    }

    /// Gets right vector on path based on 'time' (where 0 is start, and 1 is end of path).
    public Vector3 GetRight(float t)
    {
        var data = CalculatePercentOnPathData(t);
        return Vector3.Lerp(path[data.previousIndex].right, path[data.nextIndex].right, data.percentBetweenIndices);
    }

    /// Gets point on path based on 'time' (where 0 is start, and 1 is end of path).
    public Vector3 GetPointAtTime(float t)
    {
        var data = CalculatePercentOnPathData(t);
        return Vector3.Lerp(path[data.previousIndex].pos, path[data.nextIndex].pos, data.percentBetweenIndices);
    }

    /// Gets a rotation that will orient an object in the direction of the path at this point, with local up point along the path's normal
    public Quaternion GetRotationAtDistance(float dst)
    {
        float t = dst / length;
        return GetRotation(t);
    }

    /// Gets a rotation that will orient an object in the direction of the path at this point, with local up point along the path's normal
    public Quaternion GetRotation(float t)
    {
        var data = CalculatePercentOnPathData(t);
        Vector3 direction = Vector3.Lerp(path[data.previousIndex].forward, path[data.nextIndex].forward, data.percentBetweenIndices);
        Vector3 up = Vector3.Lerp(path[data.previousIndex].up, path[data.nextIndex].up, data.percentBetweenIndices);
        return Quaternion.LookRotation(Utils.TransformDirection(direction, transform), Utils.TransformDirection(up, transform));
    }

    public Quaternion GetRotation(int index)
    {
        Vector3 direction = path[index].forward;
        Vector3 up = path[index].up;
        return Quaternion.LookRotation(Utils.TransformDirection(direction, transform), Utils.TransformDirection(up, transform));
    }


    /// Finds the closest point on the path from any point in the world
    public Vector3 GetClosestPointOnPath(Vector3 worldPoint)
    {
        TimeOnPathData data = CalculateClosestPointOnPathData(worldPoint);
        return Vector3.Lerp(path[data.previousIndex].pos, path[data.nextIndex].pos, data.percentBetweenIndices);
    }

    /// Finds the 'time' (0=start of path, 1=end of path) along the path that is closest to the given point
    public float GetClosestTimeOnPath(Vector3 worldPoint)
    {
        TimeOnPathData data = CalculateClosestPointOnPathData(worldPoint);
        return Mathf.Lerp(path[data.previousIndex].time, path[data.nextIndex].time, data.percentBetweenIndices);
    }

    #endregion

    #region Internal methods

    /// For a given value 't' between 0 and 1, calculate the indices of the two vertices before and after t. 
    /// Also calculate how far t is between those two vertices as a percentage between 0 and 1.
    TimeOnPathData CalculatePercentOnPathData(float t)
    {
        t = Mathf.Clamp01(t);

        int prevIndex = 0;
        int nextIndex = path.Count - 1;
        int i = Mathf.RoundToInt(t * (path.Count - 1)); // starting guess

        // Starts by looking at middle vertex and determines if t lies to the left or to the right of that vertex.
        // Continues dividing in half until closest surrounding vertices have been found.
        while (true)
        {
            // t lies to left
            if (t <= path[i].time)
            {
                nextIndex = i;
            }
            // t lies to right
            else
            {
                prevIndex = i;
            }
            i = (nextIndex + prevIndex) / 2;

            if (nextIndex - prevIndex <= 1)
            {
                break;
            }
        }

        float abPercent = Mathf.InverseLerp(path[prevIndex].time, path[nextIndex].time, t);
        return new TimeOnPathData(prevIndex, nextIndex, abPercent);
    }

    /// Calculate time data for closest point on the path from given world point
    TimeOnPathData CalculateClosestPointOnPathData(Vector3 worldPoint)
    {
        float minSqrDst = float.MaxValue;
        Vector3 closestPoint = Vector3.zero;
        int closestSegmentIndexA = 0;
        int closestSegmentIndexB = 0;

        for (int i = 0; i < path.Count; i++)
        {
            int nextI = i + 1;
            if (nextI >= path.Count)
            {
                break;
            }

            Vector3 closestPointOnSegment = Utils.ClosestPointOnLineSegment(worldPoint, path[i].pos, path[nextI].pos);
            float sqrDst = (worldPoint - closestPointOnSegment).sqrMagnitude;
            if (sqrDst < minSqrDst)
            {
                minSqrDst = sqrDst;
                closestPoint = closestPointOnSegment;
                closestSegmentIndexA = i;
                closestSegmentIndexB = nextI;
            }

        }
        float closestSegmentLength = (path[closestSegmentIndexA].pos - path[closestSegmentIndexB].pos).magnitude;
        float t = (closestPoint - path[closestSegmentIndexA].pos).magnitude / closestSegmentLength;
        return new TimeOnPathData(closestSegmentIndexA, closestSegmentIndexB, t);
    }
    float RecalculateLength()
    {
        float lengthToRemove = path[0].cumulativeLength;
        for (int i = 0; i < path.Count; i++)
        {
            path[i].cumulativeLength -= lengthToRemove;
        }
        return lengthToRemove;
    }
    void RecalculateAnchorMap()
    {
        int numToRemove = anchorVertexMap[0];
        for (int i = 0; i < anchorVertexMap.Count; i++)
        {
            anchorVertexMap[i] -= numToRemove;
        }
    }

    void CalculateTimes()
    {
        length = path[LastVertIndex].cumulativeLength;
        for (int i = 0; i < path.Count; i++)
        {
            path[i].time = path[i].cumulativeLength / length;
        }
    }

    #endregion

    #region Structs

    public class BezierPoint
    {
        public Vector3 anchor, ctrl1, ctrl2;

        public BezierPoint(Vector3 anchor, Vector3 ctrl1, Vector3 ctrl2)
        {
            this.anchor = anchor;
            this.ctrl1 = ctrl1;
            this.ctrl2 = ctrl2;
        }
    }

    public class VertexPoint
    {
        public Vector3 pos, forward, up, right;
        public float cumulativeLength, time; // Percentage along the path at each vertex (0 being start of path, and 1 being the end)

        public VertexPoint(Vector3 position, Vector3 forward, Vector3 up, Vector3 right, float cumulativeLength, float time)
        {
            this.pos = position;
            this.forward = forward;
            this.up = up;
            this.right = right;
            this.cumulativeLength = cumulativeLength;
            this.time = time;
        }
    }

    public struct TimeOnPathData
    {
        public readonly int previousIndex;
        public readonly int nextIndex;
        public readonly float percentBetweenIndices;

        public TimeOnPathData(int prev, int next, float percentBetweenIndices)
        {
            this.previousIndex = prev;
            this.nextIndex = next;
            this.percentBetweenIndices = percentBetweenIndices;
        }
    }

    #endregion


}



using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

public static class Utils
{
    /// Calculates good positions (to result in smooth path) for the controls around specified anchor
    public static Vector3[] GetAnchorControlPoints(Vector3 prevPoint, Vector3 currentPoint, Vector3 nextPoint, float autoControlLength)
    {
        // Calculate a vector that is perpendicular to the vector bisecting the angle between this anchor and its two immediate neighbours
        // The control points will be placed along that vector
        Vector3 offset;
        Vector3 dir = Vector3.zero;
        float[] neighbourDistances = new float[2];
        Vector3[] ctrl = new Vector3[2];

        offset = prevPoint - currentPoint;
        dir += offset.normalized;
        neighbourDistances[0] = offset.magnitude;


        offset = nextPoint - currentPoint;
        dir -= offset.normalized;
        neighbourDistances[1] = -offset.magnitude;

        dir.Normalize();

        // Set the control points along the calculated direction, with a distance proportional to the distance to the neighbouring control point
        for (int i = 0; i < 2; i++)
        {
            ctrl[i] = currentPoint + dir * neighbourDistances[i] * autoControlLength;
        }

        return ctrl;
    }

    /// returns the smallest angle between ABC. Never greater than 180
    public static float MinAngle(Vector3 a, Vector3 b, Vector3 c)
    {
        return Vector3.Angle((a - b), (c - b));
    }

    /// Returns point at time 't' (between 0 and 1)  along bezier curve defined by 4 points (anchor_1, control_1, control_2, anchor_2)
    public static Vector3 EvaluateCurve(Vector3 a1, Vector3 c1, Vector3 c2, Vector3 a2, float t)
    {
        t = Mathf.Clamp01(t);
        return (1 - t) * (1 - t) * (1 - t) * a1 + 3 * (1 - t) * (1 - t) * t * c1 + 3 * (1 - t) * t * t * c2 + t * t * t * a2;
    }

    /// Returns point at time 't' (between 0 and 1) along bezier curve defined by 4 points (anchor_1, control_1, control_2, anchor_2)
    public static Vector3 EvaluateCurve(Vector3[] points, float t)
    {
        return EvaluateCurve(points[0], points[1], points[2], points[3], t);
    }

    /// Calculates the derivative of the curve at time 't'
    /// This is the vector tangent to the curve at that point
    public static Vector3 EvaluateCurveDerivative(Vector3 a1, Vector3 c1, Vector3 c2, Vector3 a2, float t)
    {
        t = Mathf.Clamp01(t);
        return 3 * (1 - t) * (1 - t) * (c1 - a1) + 6 * (1 - t) * t * (c2 - c1) + 3 * t * t * (a2 - c2);
    }

    /// Returns a vector tangent to the point at time 't'
    /// This is the vector tangent to the curve at that point
    public static Vector3 EvaluateCurveDerivative(Vector3[] points, float t)
    {
        return EvaluateCurveDerivative(points[0], points[1], points[2], points[3], t);
    }

    // Crude, but fast estimation of curve length.
    public static float EstimateCurveLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float controlNetLength = (p0 - p1).magnitude + (p1 - p2).magnitude + (p2 - p3).magnitude;
        float estimatedCurveLength = (p0 - p3).magnitude + controlNetLength / 2f;
        return estimatedCurveLength;
    }

    public static bool IsInsidePolygon(Vector2[] polygon, Vector2 point)
    {
        int n = polygon.Length;
        // There must be at least 3 vertices in polygon[] 
        if (n < 3)
        {
            return false;
        }

        // Create a point for line segment from p to infinite 
        Vector2 extreme = new Vector2(Screen.width, point.y);

        // Count intersections of the above line 
        // with sides of polygon 
        int count = 0, i = 0;
        do
        {
            int next = (i + 1) % n;

            // Check if the line segment from 'p' to 
            // 'extreme' intersects with the line 
            // segment from 'polygon[i]' to 'polygon[next]' 
            if (doIntersect(polygon[i], polygon[next], point, extreme))
            {
                // If the point 'p' is colinear with line 
                // segment 'i-next', then check if it lies 
                // on segment. If it lies, return true, otherwise false 
                if (orientation(polygon[i], point, polygon[next]) == 0)
                {
                    return onSegment(polygon[i], point, polygon[next]);
                }
                count++;
            }
            i = next;
        } while (i != 0);

        // Return true if count is odd, false otherwise 
        return (count % 2 == 1); // Same as (count%2 == 1) 
    }

    static bool onSegment(Vector2 p, Vector2 q, Vector2 r)
    {
        if (q.x <= Mathf.Max(p.x, r.x) &&
            q.x >= Mathf.Min(p.x, r.x) &&
            q.y <= Mathf.Max(p.y, r.y) &&
            q.y >= Mathf.Min(p.y, r.y))
        {
            return true;
        }
        return false;
    }

    static int orientation(Vector2 p, Vector2 q, Vector2 r)
    {
        int val = (int)((q.y - p.y) * (r.x - q.x) -
                (q.x - p.x) * (r.y - q.y));

        if (val == 0)
        {
            return 0; // colinear 
        }
        return (val > 0) ? 1 : 2; // clock or counterclock wise 
    }

    static bool doIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
    {
        // Find the four orientations needed for 
        // general and special cases 
        int o1 = orientation(p1, q1, p2);
        int o2 = orientation(p1, q1, q2);
        int o3 = orientation(p2, q2, p1);
        int o4 = orientation(p2, q2, q1);

        // General case 
        if (o1 != o2 && o3 != o4)
        {
            return true;
        }

        // Special Cases 
        // p1, q1 and p2 are colinear and 
        // p2 lies on segment p1q1 
        if (o1 == 0 && onSegment(p1, p2, q1))
        {
            return true;
        }

        // p1, q1 and p2 are colinear and 
        // q2 lies on segment p1q1 
        if (o2 == 0 && onSegment(p1, q2, q1))
        {
            return true;
        }

        // p2, q2 and p1 are colinear and 
        // p1 lies on segment p2q2 
        if (o3 == 0 && onSegment(p2, p1, q2))
        {
            return true;
        }

        // p2, q2 and q1 are colinear and 
        // q1 lies on segment p2q2 
        if (o4 == 0 && onSegment(p2, q1, q2))
        {
            return true;
        }

        // Doesn't fall in any of the above cases 
        return false;
    }

    #region MathUtility

    public static Vector3 ClosestPointOnLineSegment(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 aB = b - a;
        Vector3 aP = p - a;
        float sqrLenAB = aB.sqrMagnitude;

        if (sqrLenAB == 0)
            return a;

        float t = Mathf.Clamp01(Vector3.Dot(aP, aB) / sqrLenAB);
        return a + aB * t;
    }
    public static Vector3 TransformDirection(Vector3 p, Transform t)
    {
        var original = LockTransformToSpace(t);
        Vector3 transformedPoint = t.TransformDirection(p);
        original.SetTransform(t);
        return transformedPoint;
    }
    static PosRotScale LockTransformToSpace(Transform t)
    {
        var original = new PosRotScale(t);

        //float maxScale = Mathf.Max (t.localScale.x * t.parent.localScale.x, t.localScale.y * t.parent.localScale.y, t.localScale.z * t.parent.localScale.z);
        float maxScale = Mathf.Max(t.lossyScale.x, t.lossyScale.y, t.lossyScale.z);

        t.localScale = Vector3.one * maxScale;

        return original;
    }
    class PosRotScale
    {
        public readonly Vector3 position;
        public readonly Quaternion rotation;
        public readonly Vector3 scale;

        public PosRotScale(Transform t)
        {
            this.position = t.position;
            this.rotation = t.rotation;
            this.scale = t.localScale;
        }

        public void SetTransform(Transform t)
        {
            t.position = position;
            t.rotation = rotation;
            t.localScale = scale;

        }
    }

    #endregion
}

public static class GizmosUtils
{
    public static void DrawText(GUISkin guiSkin, string text, Vector3 position, Color? color = null, int fontSize = 0, float yOffset = 0)
    {
#if UNITY_EDITOR
        var prevSkin = GUI.skin;
        if (guiSkin == null)
            Debug.LogWarning("editor warning: guiSkin parameter is null");
        else
            GUI.skin = guiSkin;

        GUIContent textContent = new GUIContent(text);

        GUIStyle style = (guiSkin != null) ? new GUIStyle(guiSkin.GetStyle("Label")) : new GUIStyle();
        if (color != null)
            style.normal.textColor = (Color)color;
        if (fontSize > 0)
            style.fontSize = fontSize;

        Vector2 textSize = style.CalcSize(textContent);
        Vector3 screenPoint = Camera.current.WorldToScreenPoint(position);

        if (screenPoint.z > 0) // checks necessary to the text is not visible when the camera is pointed in the opposite direction relative to the object
        {
            var worldPosition = Camera.current.ScreenToWorldPoint(new Vector3(screenPoint.x - textSize.x * 0.5f, screenPoint.y + textSize.y * 0.5f + yOffset, screenPoint.z));
            UnityEditor.Handles.Label(worldPosition, textContent, style);
        }
        GUI.skin = prevSkin;
#endif
    }
}

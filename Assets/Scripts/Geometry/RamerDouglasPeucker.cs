using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RamerDouglasPeucker
{
    //const float Epsilon = 1e-12f;
    /// <summary>
    /// Removes any repeated points (that is, one point extremely close to the previous one). The same point can
    /// appear multiple times just not right after one another. This does not modify the input list. If no repeats
    /// were found, it returns the input list; otherwise it creates a new list with the repeats removed.
    /// </summary>
    /// <param name="pts">Initial list of points.</param>
    /// <returns>Either pts (if no duplicates were found), or a new list containing pts with duplicates removed.</returns>
    public static List<Vector3> RemoveDuplicates(List<Vector3> pts)
    {
        if (pts.Count < 2)
            return pts;

        // Common case -- no duplicates, so just return the source list
        Vector3 prev = pts[0];
        int len = pts.Count;
        int nDup = 0;
        for (int i = 1; i < len; i++)
        {
            Vector3 cur = pts[i];
            if (prev == cur)
                nDup++;
            else
                prev = cur;
        }

        if (nDup == 0)
            return pts;
        else
        {
            //Debug.Log("[RDPReduce] Found duplicates!");
            // Create a copy without them
            List<Vector3> dst = new List<Vector3>(len - nDup);
            prev = pts[0];
            dst.Add(prev);
            for (int i = 1; i < len; i++)
            {
                Vector3 cur = pts[i];
                if (prev != cur)
                {
                    dst.Add(cur);
                    prev = cur;
                }
            }
            return dst;
        }
    }

    /// <summary>
    /// "Reduces" a set of line segments by removing points that are too far away. Does not modify the input list; returns
    /// a new list with the points removed.
    /// The image says it better than I could ever describe: http://upload.wikimedia.org/wikipedia/commons/3/30/Douglas-Peucker_animated.gif
    /// The wiki article: http://en.wikipedia.org/wiki/Ramer%E2%80%93Douglas%E2%80%93Peucker_algorithm
    /// Based on:  http://www.codeproject.com/Articles/18936/A-Csharp-Implementation-of-Douglas-Peucker-Line-Ap
    /// </summary>
    /// <param name="pts">Points to reduce</param>
    /// <param name="error">Maximum distance of a point to a line. Low values (~2-4) work well for mouse/touchscreen data.</param>
    /// <returns>A new list containing only the points needed to approximate the curve.</returns>
    public static List<Vector3> RDPReduce(List<Vector3> pts, float error, out List<int> keepIndex)
    {
        if (pts == null)
        {
            throw new ArgumentNullException("pts");
        }

        //pts = RemoveDuplicates(pts);

        if (pts.Count < 3)
        {
            keepIndex = new List<int> { 0, 1 };
            return new List<Vector3>(pts);
        }

        keepIndex = new List<int>(Math.Max(pts.Count / 2, 16))
        {
            0,
            pts.Count - 1
        };

        RDPRecursive(pts, error, 0, pts.Count - 1, keepIndex);
        keepIndex.Sort();
        List<Vector3> res = new List<Vector3>(keepIndex.Count);

        foreach (int idx in keepIndex)
            res.Add(pts[idx]);
        return res;
    }

    private static void RDPRecursive(List<Vector3> pts, float error, int first, int last, List<int> keepIndex)
    {
        int nPts = last - first + 1;
        if (nPts < 3)
            return;

        Vector3 a = pts[first];
        Vector3 b = pts[last];


        float maxDist = error;
        int split = 0;
        for (int i = first + 1; i < last; i++)
        {
            Vector3 p = pts[i];
            float pDist = PerpendicularDistance(a, b, p);
            if (pDist > maxDist)
            {
                maxDist = pDist;
                split = i;
            }
        }

        if (split != 0)
        {
            keepIndex.Add(split);
            RDPRecursive(pts, error, first, split, keepIndex);
            RDPRecursive(pts, error, split, last, keepIndex);
        }
    }

    /// <summary>
    /// Finds the shortest distance between a point and a line. See: http://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line
    /// </summary>
    /// <param name="a">First point of the line.</param>
    /// <param name="b">Last point of the line.</param>
    /// <param name="p">The point to test.</param>
    /// <returns>The perpendicular distance to the line.</returns>
    private static float PerpendicularDistance(Vector3 a, Vector3 b, Vector3 p)
    {
        return Mathf.Sqrt(Vector3.Cross(p - a, p - b).sqrMagnitude / (b - a).sqrMagnitude);
    }
}

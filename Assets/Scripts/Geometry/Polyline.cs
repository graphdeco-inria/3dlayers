using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Polyline
{
    private List<Vector3> points;
    private List<float> pressures;

    //private const int ENDPOINT_UNSAFE_POINTS_COUNT = 0;

    public Polyline()
    {
        this.points = new List<Vector3>(0);
        this.pressures = new List<float>(0);
    }

    public bool ShouldAddPoint(Vector3 candidatePos)
    {
        //float inputSamplingDistance = small / 2;
        return points.Count == 0 || Vector3.Distance(points[points.Count - 1], candidatePos) > 1e-5f;
    }

    public void AddPoint(Vector3 p, float pressure = 1f)
    {
        this.points.Add(p);
        this.pressures.Add(pressure);
    }

    public void SanitizeEndpoints(int unsafePtsCount = 2)
    {
        int newPtsCount = points.Count - 2 * unsafePtsCount;
        
        // Prevent sanitizing that yields degenerate strokes
        if (newPtsCount < 2)
            return;

        this.points.RemoveRange(0, unsafePtsCount);
        this.points.RemoveRange(this.points.Count - unsafePtsCount, unsafePtsCount);

        this.pressures.RemoveRange(0, unsafePtsCount);
        this.pressures.RemoveRange(this.points.Count - unsafePtsCount, unsafePtsCount);
    }

    public void Simplify(float error)
    {
        List<int> keptIndices = new List<int>();
        this.points = RamerDouglasPeucker.RDPReduce(this.points, error, out keptIndices);
        List<float> newPressures = new List<float>(keptIndices.Count);

        for (int i = 0; i < keptIndices.Count; i++)
            newPressures.Add(this.pressures[keptIndices[i]]);
        this.pressures = newPressures;
    }

    public int GetPointsCount() { 
        return Mathf.Max(0, points.Count); 
    }

    private int ConvertIndex(int inputIdx)
    {
        return inputIdx;
    }

    public Vector3 GetPosition(int idx)
    {
        return points[ConvertIndex(idx)];
    }

    public float GetPressure(int idx)
    {
        return pressures[ConvertIndex(idx)];
    }

    // Get a segment's tangent. There are N-1 segments, N being the number of points in the polyline.
    public Vector3 GetTangentAt(int segmentIdx)
    {
        Vector3 pA = this.points[ConvertIndex(segmentIdx)];
        Vector3 pB = this.points[ConvertIndex(segmentIdx + 1)];
        return pB - pA;
    }


    // Adapted from https://github.com/mattatz/unity-tubular
    public List<FrenetFrame> ComputeFrenetFrames(Vector3 N0)
    {
        int segments = GetPointsCount() - 1;

        var normal = new Vector3();

        var tangents = new Vector3[segments];
        var normals = new Vector3[segments];
        var binormals = new Vector3[segments];

        // compute the tangent vectors for each segment on the curve
        for (int i = 0; i < segments; i++)
        {
            tangents[i] = GetTangentAt(i).normalized;
            //Debug.Log(tangents[i]);
        }

        // A non null initial normal was given
        if (N0.magnitude > float.Epsilon && Vector3.Cross(tangents[0], N0.normalized).magnitude > float.Epsilon)
        {
            var vec = Vector3.Cross(tangents[0], N0.normalized).normalized;
            normals[0] = Vector3.Cross(tangents[0], vec);
        }
        else
        {
            // select an initial normal vector perpendicular to the first tangent vector,
            // and in the direction of the minimum tangent xyz component
            normals[0] = new Vector3();
            var min = float.MaxValue;
            var tx = Mathf.Abs(tangents[0].x);
            var ty = Mathf.Abs(tangents[0].y);
            var tz = Mathf.Abs(tangents[0].z);
            if (tx <= min)
            {
                min = tx;
                normal.Set(1, 0, 0);
            }
            if (ty <= min)
            {
                min = ty;
                normal.Set(0, 1, 0);
            }
            if (tz <= min)
            {
                normal.Set(0, 0, 1);
            }
            var vec = Vector3.Cross(tangents[0], normal).normalized;
            normals[0] = Vector3.Cross(tangents[0], vec);
            //Debug.Log($"normal at 0 = {normals[0]} magnitude = {normals[0].magnitude}");
        }

        binormals[0] = Vector3.Cross(tangents[0], normals[0]);
        //Debug.Log($"bi at 0 = {binormals[0]} magnitude = {binormals[0].magnitude}");

        // compute the slowly-varying normal and binormal vectors for each segment on the curve

        for (int i = 1; i < segments; i++)
        {
            // copy previous
            normals[i] = normals[i - 1];
            binormals[i] = binormals[i - 1];

            // Rotation axis
            var axis = Vector3.Cross(tangents[i - 1], tangents[i]);
            if (axis.magnitude > float.Epsilon)
            {
                axis.Normalize();

                float dot = Vector3.Dot(tangents[i - 1], tangents[i]);

                // clamp for floating pt errors
                float theta = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f));

                normals[i] = Quaternion.AngleAxis(theta * Mathf.Rad2Deg, axis) * normals[i];
                //binormals[i] = Vector3.Cross(tangents[i], normals[i]).normalized;
                //Debug.Log($"normal at {i} = {normals[i]} magnitude = {normals[i].magnitude}");
            }
            binormals[i] = Vector3.Cross(tangents[i], normals[i]).normalized;

            //else
            //{
            //    // Degenerate case: copy previous values
            //    tangents[i] = tangents[i - 1];
            //}

        }

        var frames = new List<FrenetFrame>();
        int n = tangents.Length;
        for (int i = 0; i < n; i++)
        {
            var frame = new FrenetFrame(tangents[i], normals[i], binormals[i]);
            frames.Add(frame);
            //Debug.Log($"frame at {i} = {tangents[i]} {normals[i]} {binormals[i]} magnitudes = {tangents[i].magnitude} {normals[i].magnitude} {binormals[i].magnitude}");

        }
        return frames;
    }


}
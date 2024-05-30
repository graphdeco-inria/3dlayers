using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Adapted from https://github.com/mattatz/unity-tubular

public static class Tube
{
    public static Mesh Build(
        Polyline curve, 
        float radius, 
        int radialSegments, 
        Color baseColor, 
        bool variableWidth = false, 
        bool variableOpacity = false)
    {
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var tangents = new List<Vector4>();
        List<Color> colors = new List<Color>();
        //var uvs = new List<Vector2>();
        var indices = new List<int>();

        var frames = curve.ComputeFrenetFrames(Vector3.zero);
        int tubularSegments = frames.Count;

        // start cap
        vertices.Add(curve.GetPosition(0));
        colors.Add(baseColor);
        //Debug.Log("start cap indices:");
        for (int i = 1; i <= radialSegments; i++)
        {
            indices.Add(0); indices.Add(i + 1); indices.Add(i); 
            //Debug.Log($"{0} - {i} - {i + 1}");
        }

        for (int i = 1; i < tubularSegments; i++)
        {
            //Debug.Log("generate segment " + i);
            GenerateSegment(curve, frames, i, radius, radialSegments, baseColor, variableWidth, variableOpacity, vertices, normals, tangents, colors);
        }



        //for (int i = 0; i < tubularSegments; i++)
        //{
        //    for (int j = 0; j <= radialSegments; j++)
        //    {
        //        float u = 1f * j / radialSegments;
        //        float v = 1f * i / tubularSegments;
        //        uvs.Add(new Vector2(u, v));
        //    }
        //}
        //Debug.Log("main body indices:");

        for (int j = 1; j < tubularSegments - 1; j++)
        {
            //Debug.Log("generate segment " + j);
            for (int i = 1; i <= radialSegments; i++)
            {
                int a = 1 + (radialSegments + 1) * (j - 1) + (i - 1);
                int b = 1 + (radialSegments + 1) * j + (i - 1);
                int c = 1 + (radialSegments + 1) * j + i;
                int d = 1 + (radialSegments + 1) * (j - 1) + i;

                // faces
                indices.Add(a); indices.Add(d); indices.Add(b);
                indices.Add(b); indices.Add(d); indices.Add(c);

                //Debug.Log($"{a} - {d} - {b}");
                //Debug.Log($"{b} - {d} - {c}");

            }
        }

        // end cap
        int endCapVertexIdx = vertices.Count;
        vertices.Add(curve.GetPosition(curve.GetPointsCount() - 1));
        colors.Add(baseColor);
        //Debug.Log("end cap indices:");
        for (int i = 1; i <= radialSegments; i++)
        {
            indices.Add(endCapVertexIdx); indices.Add(endCapVertexIdx - i - 1); indices.Add(endCapVertexIdx - i);
            //Debug.Log($"{endCapVertexIdx} - {endCapVertexIdx - i -1} - {endCapVertexIdx - i}");
        }

        //for (int i = 0; i < vertices.Count; i++)
        //{
        //    Debug.Log($"vertex {i} = {vertices[i]}");
        //}

        var mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        //mesh.normals = normals.ToArray();
        //mesh.tangents = tangents.ToArray();
        mesh.colors = colors.ToArray();
        //mesh.uv = uvs.ToArray();
        mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
        return mesh;
    }

    public static void Update(
        Mesh mesh,
        Vector3 newPoint,
        float newPointPressure,
        float radius,
        int radialSegments,
        Color baseColor,
        bool variableWidth = false,
        bool variableOpacity = false
        )
    {
        // TODO: update tube mesh with new point instead of recreating the whole tube each time
    }

    static void GenerateSegment(
        Polyline curve,
        List<FrenetFrame> frames,
        int segmentIdx,
        float maxRadius,
        int radialSegments,
        Color baseColor,
        bool variableWidth,
        bool variableOpacity,
        List<Vector3> vertices,
        List<Vector3> normals,
        List<Vector4> tangents,
        List<Color> colors
    )
    {
        var p = curve.GetPosition(segmentIdx);
        //Debug.Log($"p (idx = {segmentIdx}) = {p}");
        var fr = frames[segmentIdx];

        var N = fr.Normal;
        var B = fr.Binormal;

        float radius = maxRadius;
        float opacity = 1.0f;

        if (variableWidth)
            radius *= curve.GetPressure(segmentIdx);
        if (variableOpacity)
            opacity *= curve.GetPressure(segmentIdx);

        for (int j = 0; j <= radialSegments; j++)
        {
            float v = 1f * j / radialSegments * Mathf.PI * 2f;
            var sin = Mathf.Sin(v);
            var cos = Mathf.Cos(v);

            Vector3 normal = (cos * N + sin * B).normalized;
            //Debug.Log($"p = {p}, radius*normal ={radius * normal} radius = {radius} normal = {normal}");
            vertices.Add(p + radius * normal);
            //Debug.Log(p + radius * normal);

            normals.Add(normal);

            var tangent = fr.Tangent;
            tangents.Add(new Vector4(tangent.x, tangent.y, tangent.z, 0f));

            colors.Add(new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * opacity));
        }
    }
}

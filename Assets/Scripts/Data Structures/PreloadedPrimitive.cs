using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;


public class PreloadedPrimitive : Primitive
{

    public string sourcePath { get; private set; }
    public string meshName { get; private set; }

    public Color[] baseVertexColors { get; private set; }

    public void Create(int UID, string prefabPath, string meshName, Mesh loadedMesh)
    {
        base.Create(UID);
        sourcePath = prefabPath;
        this.meshName = meshName;
        Preload(loadedMesh);
    }

    public void Create(SerializablePreloadedPrimitive data)
    {
        sourcePath = data.sourcePath;
        meshName = data.meshName;

        Mesh[] meshesImported = Resources.LoadAll<Mesh>(sourcePath);

        Mesh subMesh = meshesImported.First(o => o.name == meshName);
        if (subMesh)
        {
            Preload(subMesh);
            base.Create(data);
        }


    }

    public override void ResetColor(Color initialColor, bool initialApplyColor)
    {
        if (!initialApplyColor)
            Recolor(baseVertexColors);
        else
            base.ResetColor(initialColor, initialApplyColor);
    }

    public override SerializablePrimitive Serialize()
    {
        return new SerializablePreloadedPrimitive(this);
    }

    private void Preload(Mesh loadedMesh)
    {
        Mesh transformedMesh = new Mesh();
        Vector3[] transformedVertices = new Vector3[loadedMesh.vertexCount];
        for (int i = 0; i < loadedMesh.vertexCount; i++)
        {
            //transformedVertices[i] = transform.TransformPoint(loadedMesh.vertices[i]);
            transformedVertices[i] = loadedMesh.vertices[i];
        }
        transformedMesh.vertices = transformedVertices;
        transformedMesh.colors = loadedMesh.colors;
        transformedMesh.triangles = loadedMesh.triangles;

        // Store the initial per-vertex colors
        baseVertexColors = loadedMesh.colors;

        base.UpdateMesh(transformedMesh);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    //public override Color GetNearestColor(Vector3 worldPosition)
    //{
    //    Vector3 localPosition = transform.InverseTransformPoint(worldPosition);

    //    Color[] vertexColors = GetMesh().colors;
    //    Vector3[] positions = GetMesh().vertices;

    //    Color c = vertexColors[0];
    //    float shortestDist = Vector3.Distance(positions[0], localPosition);

    //    for (int i = 1; i < positions.Length; i++)
    //    {
    //        float dist = Vector3.Distance(positions[i], localPosition);
    //        if (dist < shortestDist)
    //        {
    //            c = vertexColors[i];
    //            shortestDist = dist;
    //        }
    //    }

    //    return c;
    //}
}

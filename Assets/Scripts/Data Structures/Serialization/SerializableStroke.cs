using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SerializableStroke : SerializablePrimitive
{
    [SerializeField]
    public float[] vertices;
    [SerializeField]
    public float[] pressures;
    //[SerializeField]
    //public SerializableColor color;
    [SerializeField]
    public float baseRadius;

    public SerializableStroke(Stroke s) : base(s)
    {
        type = PrimitiveType.Stroke;
        Vector3[] strokeVertices = s.GetVertices();
        vertices = new float[strokeVertices.Length * 3]; // initialize vertices array.
        for (int i = 0; i < strokeVertices.Length; i++) // Serialization: Vector3's values are stored sequentially.
        {
            vertices[i * 3] = strokeVertices[i].x;
            vertices[i * 3 + 1] = strokeVertices[i].y;
            vertices[i * 3 + 2] = strokeVertices[i].z;
        }

        pressures = s.GetPressures();
        baseRadius = s.baseRadius;
        //color = s.strokeColor;
    }


    public Polyline GetPolyline()
    {
        Polyline polyline = new Polyline();
        //Debug.Log(vertices.Length / 3);
        for (int i = 0; i < vertices.Length / 3; i++)
        {
            //Debug.Log(i);
            polyline.AddPoint(new Vector3(
                    vertices[i * 3], vertices[i * 3 + 1], vertices[i * 3 + 2]
                ),
                    pressures[i]
                );
        }
        return polyline;
    }
}

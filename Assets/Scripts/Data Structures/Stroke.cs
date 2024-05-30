using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Stroke : Primitive
{
    //public int UID { get; private set; } = -1;

    //public Color strokeColor { get; private set; } = Color.yellow;
    public float baseRadius { get; private set; } = 0.01f;

    //public bool selected
    //{
    //    get;
    //    private set;
    //}

    private Polyline curve;
    //private MeshFilter mesh;

    private new void Awake()
    {
        base.Awake();
        curve = new Polyline();
        //mesh = gameObject.GetComponent<MeshFilter>();
    }

    public void Create(int UID, Color strokeColor, float baseRadius)
    {
        //this.UID = UID;
        base.Create(UID);
        this.baseRadius = baseRadius;
        this.PrimitiveColor = strokeColor;
    }

    public void Create(SerializableStroke s)
    {
        this.curve = s.GetPolyline();
        this.baseRadius = s.baseRadius;
        UpdateMesh(true);

        base.Create(s);
    }

    public void DrawPoint(Vector3 position, float pressure)
    {
        if (UID == -1)
        {
            Debug.LogError("Stroke not initialized.");
            return;
        }

        //this.strokeColor = strokeColor;

        //if (curve.ShouldAddPoint(position))
        //{
        curve.AddPoint(position, pressure);
        // Update mesh
        UpdateMesh(false);
        //}
        //else
        //    Debug.Log("Prevent adding pt");

    }

    //public void Sanitize()
    //{
    //    curve.SanitizeEndpoints();
    //    UpdateMesh();
    //}

    public void End(float simplifyThreshold)
    {
        //curve.SanitizeEndpoints();
        //curve.Simplify(simplifyThreshold);
        UpdateMesh(true);
    }


    private void UpdateMesh(bool updateCollider=true)
    {
        //Debug.Log(curve.GetPointsCount());
        if (curve.GetPointsCount() > 3)
        {
            int radialSegments = 8;
            radialSegments = (int)Mathf.Max(radialSegments, 2 * Mathf.Ceil(0.5f * Mathf.Log(baseRadius * 100, 2)) * radialSegments);
            //Debug.Log("radial segments = " + radialSegments);
            //Debug.Log("base radius = " + baseRadius);
            Mesh tubeMesh = Tube.Build(
                curve,
                baseRadius,
                radialSegments: radialSegments,
                baseColor: PrimitiveColor,
                variableWidth: true
            );

            UpdateMesh(tubeMesh, updateCollider);
        }

    }

    //public void ToggleSelect()
    //{
    //    selected = !selected;
    //}


    //public void Unselect()
    //{
    //    selected = false;
    //}

    //public void Hide()
    //{
    //    selected = false;
    //    gameObject.SetActive(false);
    //}

    //public void UnHide()
    //{
    //    gameObject.SetActive(true);
    //}

    //public Mesh GetMesh()
    //{
    //    return mesh.mesh;
    //}

    public Vector3[] GetVertices()
    {
        Vector3[] vs = new Vector3[curve.GetPointsCount()];
        for(int i = 0; i < curve.GetPointsCount(); i++)
        {
            vs[i] = curve.GetPosition(i);
        }
        return vs;
    }

    public float[] GetPressures()
    {
        float[] pressures = new float[curve.GetPointsCount()];
        for (int i = 0; i < curve.GetPointsCount(); i++)
        {
            pressures[i] = curve.GetPressure(i);
        }
        return pressures;
    }

    public override SerializablePrimitive Serialize()
    {
        return new SerializableStroke(this);
    }
}

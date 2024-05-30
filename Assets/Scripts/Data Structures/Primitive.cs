using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public abstract class Primitive : MonoBehaviour, IComparable
{
    public int UID { get; private set; } = -1;

    public bool selected
    {
        get;
        private set;
    }

    private Color _color = Color.clear;
    public Color PrimitiveColor { 
        get { return _color; }
        protected set
        {
            _applyColor = true;
            _color = value;

        }
    }

    protected bool _applyColor = false;
    public bool ApplyColor
    {
        get { return _applyColor; }
    }

    private MeshFilter mesh;
    private MeshCollider meshCollider;
    private Rigidbody rb;

    protected void Awake()
    {
        mesh = gameObject.GetComponent<MeshFilter>();
        meshCollider = gameObject.GetComponent<MeshCollider>();
    }

    protected void Create(int UID)
    {
        this.UID = UID;
    }

    protected void Create(SerializablePrimitive data)
    {
        gameObject.SetActive(data.active);
        UID = data.UID;
        transform.localPosition = data.position;
        transform.localRotation = data.rotation;
        transform.localScale = data.scale;
        _applyColor = data.applyColor;
        _color = data.color;

        if (_applyColor)
            Recolor(_color);
    }

    protected void UpdateMesh(Mesh mesh, bool updateCollider=true)
    {
        this.mesh.sharedMesh = mesh;
        //Debug.Log(this.mesh.sharedMesh);
        if (updateCollider)
            meshCollider.sharedMesh = mesh;
    }

    //public Mesh GetMesh() { return mesh.sharedMesh; }

    public void Unselect()
    {
        //Debug.Log("unselected" + name);
        selected = false;
    }

    public void Select()
    {
        selected = true;
    }

    public void Hide()
    {
        selected = false;
        gameObject.SetActive(false);
        //TriggerLayerGizmoUpdate();
    }

    public void UnHide()
    {
        gameObject.SetActive(true);
        //TriggerLayerGizmoUpdate();
    }

    //public void TriggerLayerGizmoUpdate()
    //{
    //    Layer parent = GetComponentInParent<Layer>();
    //    Debug.Log("todo: update layer gizmo position");
    //    //parent.UpdateGizmo();
    //}

    public void Recolor(Color[] vertexColors)
    {
        mesh.mesh.colors = vertexColors;

        // If all vertex colors are the same, set PrimitiveColor
        //IEnumerable<Color> distinctColors = vertexColors.Distinct<Color>();
        //if (distinctColors.Count() == 1)
        //{
        //    PrimitiveColor = distinctColors.First();
        //}
        //else
        //{
        _applyColor = false;
        _color = Color.clear;
        //}
    }

    public (Color, bool) Recolor(Color c)
    {
        int N = mesh.mesh.vertexCount;

        //Color[] initialColors = mesh.mesh.colors;

        Color initialColor = PrimitiveColor;
        bool initialApplyColor = ApplyColor;

        // Set primitive color
        PrimitiveColor = c;
        //Debug.Log("recoloring to " + c);

        Color[] colors = new Color[N];
        for (int i = 0; i < N; i++)
            colors[i] = c;

        mesh.mesh.colors = colors;

        return (initialColor, initialApplyColor);
    }

    public virtual void ResetColor(Color initialColor, bool initialApplyColor)
    {
        if (initialApplyColor)
        {
            PrimitiveColor = initialColor;
            int N = mesh.sharedMesh.vertexCount;
            Color[] colors = new Color[N];
            for (int i = 0; i < N; i++)
                colors[i] = initialColor;

            mesh.mesh.colors = colors;
        }
        // other cases are handled in overriding methods
    }

    public Mesh GetMesh(bool copy = false)
    {
        return copy ? mesh.mesh : mesh.sharedMesh;
    }

    public abstract SerializablePrimitive Serialize();

    public int CompareTo(object obj)
    {
        if (obj == null) return 1;

        Primitive otherPrimitive = obj as Primitive;
        if (otherPrimitive != null)
            return this.UID.CompareTo(otherPrimitive.UID);
        else
            throw new ArgumentException("Object is not a Primitive");
    }

    public virtual Color GetNearestColor(Vector3 worldPosition)
    {
        if (!_applyColor)
        {
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition);

            Color[] vertexColors = GetMesh().colors;
            Vector3[] positions = GetMesh().vertices;

            Color c = vertexColors[0];
            Debug.Log($"color = {ColorUtility.ToHtmlStringRGB(c)} gamma color = {ColorUtility.ToHtmlStringRGB(c.gamma)}");
            float shortestDist = Vector3.Distance(positions[0], localPosition);

            for (int i = 1; i < positions.Length; i++)
            {
                float dist = Vector3.Distance(positions[i], localPosition);
                if (dist < shortestDist)
                {
                    c = vertexColors[i];
                    shortestDist = dist;
                }
            }

            return c.gamma;
        }
        else
            return PrimitiveColor.gamma;
    }

    public Vector3 GetCenterOfMass()
    {
        return rb.centerOfMass;
    }

    //public SerializableStroke Serialize()
    //{
    //    return new SerializableStroke(this);
    //}
}

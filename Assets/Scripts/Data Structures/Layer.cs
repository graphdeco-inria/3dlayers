using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Layer : MonoBehaviour
{
    //public List<Stroke> Strokes { get; private set; }

    //public Material DebugOverrideMaterial;

    protected int _UID = -1;
    public virtual int UID { 
        get
        {
            return _UID;
        }
        protected set
        {
            _UID = value;
        }
    }

    public Material ForwardRenderMaterial { get; protected set; }

    public string LayerName { get; private set; }

    // "Visible" controls if the layer is hidden or not (but it will be shown in the layer UI)
    private bool _visible = true;
    public bool Visible
    {
        get { return _visible && gameObject.activeSelf; }
        private set
        {
            _visible = value;
            gameObject.SetActive(_visible);
        }
    }


    // "Enabled" controls if the layer is "deleted" or not (if !Enabled, the layer is not shown in the UI)
    public bool Enabled { get; private set; } = true;

    public int TriangleCount
    {
        get
        {
            int count = 0;
            MeshFilter[] meshes = GetComponentsInChildren<MeshFilter>();
            foreach(MeshFilter mesh in meshes)
            {
                count += mesh.sharedMesh.triangles.Length / 3;
            }
            return count;
        }
    }

    public int PrimitiveCount
    {
        get
        {
            Primitive[] primitives = GetComponentsInChildren<Primitive>();
            return primitives.Length;
        }
    }


    // "Active" controls if the layer is the currently selected layer in the workspace
    // This property is controlled directly by LayerManager, and should not be set by any other way (LayerManager is the source of truth)
    private bool _active;


    private void Start()
    {
        LayerManager.OnActiveLayerUpdate += HandleActiveLayerUpdate;
    }

    private void OnDestroy()
    {
        LayerManager.OnActiveLayerUpdate -= HandleActiveLayerUpdate;
    }

    private void OnEnable()
    {
        _visible = true;
    }

    private void OnDisable()
    {
        _visible = false;
    }


    public virtual void Init(string name, int UID)
    {
        this.LayerName = name;
        this.UID = UID;

        //UpdateGizmo();

        //if (this.RenderMaterial)
        //{
        //    // - Set shader property about the base group ID
        //    this.RenderMaterial.SetInt(Shader.PropertyToID("_GroupID"), UID);
        //}

        //OnInit?.Invoke();
    }

    public virtual void Init(SerializableLayer layerData)
    {
        LayerName = layerData.name;
        UID = layerData.UID;
        Visible = layerData.parameters.visible;
        Enabled = layerData.enabled;

        //UpdateGizmo();

        //layerGizmo.SetLayerName(LayerName);
    }

    public void ToggleVisibility()
    {
        Visible = !Visible;
    }

    public void Hide()
    {
        Enabled = false;
        ClearSelection();
    }

    public void UnHide()
    {
        Enabled = true;
    }

    public void Add(Primitive s)
    {
        // Preserve local pos/rot
        Vector3 pos = s.transform.localPosition;
        Quaternion rot = s.transform.localRotation;
        Vector3 scale = s.transform.localScale;
        s.transform.parent = transform;
        s.transform.localPosition = pos;
        s.transform.localRotation = rot;
        s.transform.localScale = scale;
        s.GetComponent<MeshRenderer>().sharedMaterial = this.ForwardRenderMaterial;
    }

    public virtual bool InStack(int baseUID)
    {
        return false;
    }

    public void ClearSelection()
    {
        //Debug.Log("clearing selection");
        Primitive[] primitives = GetComponentsInChildren<Primitive>(includeInactive: true);
        foreach(Primitive s in primitives)
        {
            s.Unselect();
        }
    }

    public Primitive[] GetSelection()
    {
        List<Primitive> selectedPrimitives = new List<Primitive>();
        Primitive[] strokes = GetComponentsInChildren<Primitive>(includeInactive: true);
        foreach (Primitive s in strokes)
        {
            if (s.selected)
                selectedPrimitives.Add(s);
        }

        return selectedPrimitives.ToArray();
    }

    public virtual SerializableLayer Serialize()
    {
        SerializableLayer data = new SerializableLayer();
        data.UID = UID;
        data.name = name;
        //data.visible = Visible;
        data.enabled = Enabled;

        // Primitives
        Primitive[] primitives = GetComponentsInChildren<Primitive>(includeInactive:true);
        int[] UIDs = new int[primitives.Length];
        for (int i = 0; i < primitives.Length; i++)
        {
            UIDs[i] = primitives[i].UID;
        }
        data.primitivesUID = UIDs;

        // Paramters
        LayerParameters parameters = new LayerParameters();
        parameters.visible = Visible;
        // ClippedLayers have more parameters (set in overriding method)
        data.parameters = parameters;

        return data;
    }


    public virtual LayerParameters GetParameters()
    {
        LayerParameters parameters = new LayerParameters();
        parameters.visible = Visible;
        return parameters;
    }

    public virtual void SetParameters(LayerParameters parameters)
    {
        Visible = parameters.visible;
    }

    private void HandleActiveLayerUpdate(Layer activeLayer)
    {
        if (activeLayer == this)
        {
            _active = true;
        }
        else
        {
            _active = false;
        }
        //layerGizmo.SetActive(_active);
    }


}

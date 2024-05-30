using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintCanvas : MonoBehaviour
{
    public GameObject strokePrefab;

    private LayerManager _layerManager;

    private LayerManager layerManager
    {
        get
        {
            if (_layerManager == null)
                _layerManager = GetComponentInChildren<LayerManager>();
            return _layerManager;
        }
    }

    private int _strokeNextUID = 0;
    private int strokeNextUID
    {
        get
        {
            while (assignedStrokeUIDs.Contains(_strokeNextUID))
            {
                _strokeNextUID++;
            }
            assignedStrokeUIDs.Add(_strokeNextUID);
            return _strokeNextUID;
        }
    }

    //private List<Stroke> paintStrokes = new List<Stroke>();
    private HashSet<int> assignedStrokeUIDs = new HashSet<int>();

    // Start is called before the first frame update
    void Start()
    {
        //SessionHistory.Instance.SetPaintCanvas(this);
        //layerManager = GetComponentInChildren<LayerManager>();
    }

    public void UpdateActiveLayer()
    {
        if (layerManager.ActiveLayer != null)
            layerManager.TriggerLayerUpdateEvent(layerManager.ActiveLayer.UID);
    }

    // Create a game object as a child of the canvas,
    // reset its own transform so that only the canvas transform impacts world positions
    public Stroke CreateStroke(Color strokeColor, float baseRadius)
    {
        if (layerManager.ActiveLayer == null)
            return null;

        //GameObject newObject = Instantiate(strokePrefab, Vector3.zero, Quaternion.identity);

        //Stroke s = newObject.AddComponent<Stroke>();
        Stroke s = NewStroke();
        s.Create(strokeNextUID, strokeColor, baseRadius);
        layerManager.AddToActive(s);
        //paintStrokes.Add(s);
        return s;
    }

    public PrimitiveCopy Duplicate(Primitive src)
    {
        if (layerManager.ActiveLayer == null)
            return null;

        //GameObject newObject = Instantiate(strokePrefab, Vector3.zero, Quaternion.identity);
        //PrimitiveCopy s = newObject.AddComponent<PrimitiveCopy>();
        PrimitiveCopy s = NewPrimitiveCopy();

        s.Create(strokeNextUID, src);

        //layerManager.ActiveLayer.Add(s);
        layerManager.AddToActive(s);

        //paintStrokes.Add(s);
        return s;
    }

    public PreloadedPrimitive Preload(string path, string meshName, Mesh mesh)
    {
        if (layerManager.ActiveLayer == null)
            return null;

        PreloadedPrimitive s = NewPrimitivePreload();

        //s.Create(strokeNextUID, path);
        s.Create(strokeNextUID, path, meshName, mesh);

        //layerManager.ActiveLayer.Add(s);
        layerManager.AddToActive(s);

        return s;
    }

    public bool ToggleSelect(Primitive s, bool selected)
    {
        if (layerManager.ActiveLayer == null)
            return false;

        

        if (s.GetComponentInParent<Layer>().UID == layerManager.ActiveLayer.UID)
        {
            bool emptyBefore = layerManager.CurrentSelection.Length == 0;

            if (selected)
                s.Select();
            else
                s.Unselect();

            bool emptyAfter = layerManager.CurrentSelection.Length == 0;

            if (emptyBefore != emptyAfter)
            {
                // Trigger change of layer panel UI
                layerManager.TriggerLayerUpdateEvent(layerManager.ActiveLayer.UID);
            }

            return true;
        }
        return false;
    }

    public void UnselectAll()
    {
        if (layerManager.ActiveLayer)
        {
            layerManager.ActiveLayer.ClearSelection();
            layerManager.TriggerLayerUpdateEvent(layerManager.ActiveLayer.UID);
        }
    }

    public Primitive[] Selected()
    {
        return layerManager.CurrentSelection;
    }

    //public bool SwitchToLayer(SwitchLayerDirection direction, Primitive[] grabbedObjects)
    //{
    //    Layer targetLayer = null;
    //    switch(direction)
    //    {
    //        case SwitchLayerDirection.Up:
    //            targetLayer = layerManager.AboveActiveLayer;
    //            break;
    //        case SwitchLayerDirection.Down:
    //            targetLayer = layerManager.BelowActiveLayer;
    //            break;
    //    }


    //    if (targetLayer != null)
    //    {
    //        return SwitchToLayer(targetLayer, grabbedObjects);
    //    }
    //    else
    //        return false;
    //}

    //public bool SwitchToLayer(int targetLayerUID, Primitive[] grabbedObjects)
    //{
    //    Layer target = layerManager.GetLayer(targetLayerUID);
    //    if (target != null)
    //    {
    //        return SwitchToLayer(target, grabbedObjects);
    //    }
    //    else
    //        return false;
    //}

    //private bool SwitchToLayer(Layer targetLayer, Primitive[] grabbedObjects)
    //{
    //    Layer prevLayer = layerManager.ActiveLayer;
    //    // Switch active layer (this also updates gizmos)

    //    foreach (Primitive obj in grabbedObjects)
    //    {
    //        targetLayer.Add(obj);
    //        //obj.Select();
    //    }

    //    layerManager.SetActive(targetLayer.UID);

    //    foreach (Primitive obj in grabbedObjects)
    //    {
    //        obj.Select();
    //    }

    //    // Register in history
    //    SessionHistory.Instance.RegisterAction(
    //        new SwitchLayerAction(grabbedObjects, prevLayer, targetLayer)
    //    );

    //    //layerManager.TriggerLayerUpdateEvent(targetLayer.UID);


    //    return true;
    //}

    // TODO: this method needs to be adapted to work with the new session history
    //public void DebugAddStroke(string prefabPath)
    //{
    //    Transform parent = layerManager.ActiveLayer.transform;
    //    GameObject newObject = layerManager.ActiveLayer.Preload(prefabPath); // TODO: replace with: NewPrimitivePreload() etc.


    //    newObject.transform.localPosition += Random.insideUnitSphere * 0.01f;
    //    newObject.transform.localRotation = Quaternion.Euler(Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f)); ;
    //    newObject.transform.localScale *= Random.Range(0.8f, 1.2f);
    //    //newObject.transform.localScale *= 10f;

    //    // Set random color
    //    Mesh mesh = newObject.GetComponent<MeshFilter>().mesh;
    //    Vector3[] vertices = mesh.vertices;

    //    // create new colors array where the colors will be created.
    //    Color[] colors = new Color[vertices.Length];

    //    Color c = Random.ColorHSV();
    //    //Color c = Color.red;
    //    c.a = 1f;
    //    for (int i = 0; i < vertices.Length; i++)
    //        colors[i] = c;

    //    mesh.colors = colors;
    //    //Stroke s = newObject.GetComponent<Stroke>();
    //    //s.Create(strokeNextUID, c, 1f); // values for color and baseRadius are not used here, since we instead directly set the stroke mesh
    //    //paintStrokes.Add(s);
    //    //assignedStrokeUIDs.Add(s.UID);

    //}


    public Stroke LoadSerializedStroke(SerializableStroke strokeData)
    {
        Stroke s = NewStroke();
        s.Create(strokeData);

        return s;
    }

    public PrimitiveCopy LoadSerializedCopy(SerializablePrimitiveCopy copyData, Primitive src)
    {
        PrimitiveCopy s = NewPrimitiveCopy();
        s.Create(copyData, src);

        return s;

    }

    public PreloadedPrimitive LoadSerializedPreloaded(SerializablePreloadedPrimitive preloadedData)
    {
        PreloadedPrimitive s = NewPrimitivePreload();
        s.Create(preloadedData);

        return s;
    }

    public Dictionary<int, Primitive> LoadPrimitives(SerializableSessionState sessionData)
    {
        Dictionary<int, Primitive> allPrimitives = new Dictionary<int, Primitive>();

        // First instantiate all primitives
        foreach(SerializablePrimitive primitive in sessionData.primitives)
        {
            //Debug.Log("primitive UID: " + primitive.UID);
            Primitive o = null;
            //Debug.Log(primitive.type);
            //Debug.Log(primitive);
            switch (primitive.type)
            {
                case PrimitiveType.Stroke:
                    o = LoadSerializedStroke((SerializableStroke)primitive);
                    break;
                case PrimitiveType.Copy:
                    int srcUID = ((SerializablePrimitiveCopy)primitive).sourceUID;
                    if (!allPrimitives.ContainsKey(srcUID))
                    {
                        Debug.LogError("Cannot load primitive with source primitive: " + srcUID + ". Source doesn't exist.");
                        throw new System.Exception();
                    }    
                    Primitive srcPrimitive = allPrimitives[srcUID];
                    o = LoadSerializedCopy((SerializablePrimitiveCopy)primitive, srcPrimitive);
                    break;
                case PrimitiveType.PreloadedMesh:
                    o = LoadSerializedPreloaded((SerializablePreloadedPrimitive)primitive);
                    //Debug.Log(o);
                    break;
            }

            if (o == null)
            {
                Debug.LogError("Primitive " + primitive.UID + " of type " + primitive.type + " does not have a compatible loader.");
            }
            else
            {
                assignedStrokeUIDs.Add(primitive.UID);
                allPrimitives.Add(primitive.UID, o);
            }
        }

        // Add primitives to the right layer
        foreach(SerializableLayer layerData in sessionData.layers)
        {
            Layer container = layerManager.GetLayer(layerData.UID);
            int[] strokesInLayer = layerData.primitivesUID;
            foreach(int strokeUID in strokesInLayer)
            {
                if (!allPrimitives.ContainsKey(strokeUID))
                {
                    Debug.LogError("Primitive wasn't loaded. UID: " + strokeUID);
                }
                container.Add(allPrimitives[strokeUID]);
                layerManager.TriggerLayerUpdateEvent(container.UID);

            }
        }

        return allPrimitives;
    }

    //public void TryDeleteStroke(Stroke s)
    //{
    //    // Todo: check that the stroke is part of an active layer
    //    if (s.GetComponentInParent<Layer>().UID == layerManager.ActiveLayer.UID)
    //    {
    //        Debug.Log("erasing stroke");
    //        //Debug.Log(s);
    //        //Destroy(s.gameObject);
    //        s.Hide();

    //        SessionHistory.Instance.RegisterAction(
    //            new DeleteAction(s)
    //        );
    //    }
            
    //}

    public void SetGradient(Vector3 A, Vector3 B, GradientMaskType type, int gradientDirection)
    {
        if (layerManager.IsClippedLayer(layerManager.ActiveLayer))
        {
            ClippedLayer currentLayer = (ClippedLayer)layerManager.ActiveLayer;
            LayerParameters paramsBefore = currentLayer.GetParameters();
            currentLayer.AddGradientMask(A, B, type, gradientDirection);
            LayerParameters paramsAfter = currentLayer.GetParameters();
            SessionHistory.Instance.RegisterAction(
                new EditLayerAction(currentLayer, paramsBefore, paramsAfter)
            );
        }
    }

    private Stroke NewStroke()
    {
        GameObject newObject = Instantiate(strokePrefab, Vector3.zero, Quaternion.identity);
        return newObject.AddComponent<Stroke>();
    }

    private PrimitiveCopy NewPrimitiveCopy()
    {
        GameObject newObject = Instantiate(strokePrefab, Vector3.zero, Quaternion.identity);
        return newObject.AddComponent<PrimitiveCopy>();
    }

    private PreloadedPrimitive NewPrimitivePreload()
    {
        GameObject newObject = Instantiate(strokePrefab, Vector3.zero, Quaternion.identity);
        return newObject.AddComponent<PreloadedPrimitive>();
    }



}

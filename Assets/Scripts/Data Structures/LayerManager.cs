using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

public enum InputFileType
{
    FBX, Session, Latest, QuillBaseline
}

public class LayerManager : MonoBehaviour
{

    private const int MAX_BASE_LAYERS_COUNT = 64;
    //public bool LoadDebugStrokes = false;

    public InputFileType fileType;
    public string InputFileName = "";
    //public string ExportSessionFilePath = "";

    //public static event Action OnBaseLayersUpdate;
    //public static event Action OnStackedLayersUpdate;
    public static event Action<Layer> OnLayerUpdate;
    public static event Action<Layer> OnActiveLayerUpdate;

    private Dictionary<int, BaseLayer> _baseLayers;
    private Dictionary<int, List<ClippedLayer>> _layerStacks;
    //private Dictionary<int, ClippedLayer> _allLayers;
    private List<Layer> _allLayersList;

    public int[] BaseLayersUID
    {
        get
        {
            return _layerStacks.Keys.ToArray();
        }
    }

    private Layer _activeLayer;
    public Layer ActiveLayer
    {
        get { return _activeLayer; }
        private set
        {
            Layer prevActive = _activeLayer;
            _activeLayer = value;
            OnActiveLayerUpdate?.Invoke(_activeLayer);
            // Notify of change on old active layer too
            if (prevActive != null && prevActive != _activeLayer)
            {
                OnLayerUpdate?.Invoke(prevActive);
                prevActive.ClearSelection();
            }
            OnLayerUpdate?.Invoke(_activeLayer);
        }
    }

    public Layer BelowActiveLayer
    {
        get
        {
            if (ActiveLayer == null)
                return null;

            if (IsClippedLayer(ActiveLayer))
            {
                int baseUID = ((ClippedLayer)ActiveLayer).clippingBaseUID;
                List<ClippedLayer> stack = _layerStacks[baseUID];
                int inStackZ = stack.FindIndex((layer) => ActiveLayer.UID == layer.UID);
                if (inStackZ <= 0)
                    return _baseLayers[baseUID];
                else
                    return stack[inStackZ - 1];
            }
            else
                return null; // No below layer for a base layer
        }
    }

    public Layer AboveActiveLayer
    {
        get
        {
            if (ActiveLayer == null)
                return null;

            //Debug.Log("active layer is " + ActiveLayer.LayerName);
            if (IsClippedLayer(ActiveLayer))
            {
                //Debug.Log("is clipped layer");
                int baseUID = ((ClippedLayer)ActiveLayer).clippingBaseUID;
                List<ClippedLayer> stack = _layerStacks[baseUID];
                int inStackZ = stack.FindIndex((layer) => ActiveLayer.UID == layer.UID);
                //Debug.Log(inStackZ);
                if (inStackZ == stack.Count - 1)
                    return null;
                else
                    return stack[inStackZ + 1];
            }
            else
            {
                //Debug.Log(_layerStacks[ActiveLayer.UID]);
                //Debug.Log(ActiveLayer.UID);
                // Is the stack not empty?
                if (!_layerStacks.ContainsKey(ActiveLayer.UID) || _layerStacks[ActiveLayer.UID].Count == 0)
                    return null;
                else
                    return _layerStacks[ActiveLayer.UID][0];
            }
        }
    }

    private Layer _hoveredLayer;
    public Layer HoveredLayer
    {
        get { return _hoveredLayer; }
        private set { _hoveredLayer = value; }
    }

    public Primitive[] CurrentSelection
    {
        get
        {
            if (ActiveLayer == null)
                return new Primitive[0];
            else
            {
                return ActiveLayer.GetSelection();
            }
        }
    }

    public int StacksCount
    {
        get
        {
            return _layerStacks.Count;
        }
    }

    public int AppearanceLayersCount
    {
        get
        {
            int count = 0;
            foreach(Layer layer in _allLayersList)
            {
                if (IsClippedLayer(layer))
                {
                    if (layer.Enabled && layer.Visible)
                        count++;
                }
            }
            return count;
        }
    }

    public (int, int) GetPrimitivesCount(bool appearanceLayers)
    {
        int primitivesCount = 0;
        int trianglesCount = 0;
        foreach(Layer l in _allLayersList)
        {
            if ((appearanceLayers == IsClippedLayer(l)) && l.Enabled && l.Visible)
            {
                primitivesCount += l.PrimitiveCount;
                trianglesCount += l.TriangleCount;
            }
        }

        return (primitivesCount, trianglesCount);
    }


    private int _layerNextUID = 0; // Start layer UIDs at 1 so that all base layers have a UID > 0
    private int layerNextUID
    {
        get
        {
            while (assignedLayerUIDs.Contains(_layerNextUID))
            {
                _layerNextUID++;
            }
            assignedLayerUIDs.Add(_layerNextUID);
            return _layerNextUID;
        }
    }
    private HashSet<int> assignedLayerUIDs = new HashSet<int>();

    // Start is called before the first frame update
    void OnEnable()
    {
        // Make sure all things are cleared before we start
        _layerNextUID = 0;
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        SessionHistory.Instance.SetManager(this);
        PaintCanvas canvas = GetComponentInParent<PaintCanvas>();
        SessionHistory.Instance.SetPaintCanvas(canvas);
        Debug.Log("reset layer manager");
        //Debug.Log(canvas);

        assignedLayerUIDs = new HashSet<int>();
        _baseLayers = new Dictionary<int, BaseLayer>();
        _layerStacks = new Dictionary<int, List<ClippedLayer>>();
        //_allLayers = new Dictionary<int, ClippedLayer>();
        _allLayersList = new List<Layer>();

        Selector.OnLayerActivate += SetActive;

        if (InputFileName == null || InputFileName.Length == 0)
        {
            Debug.Log("No input file specified, doing nothing.");
            return;
        }

        if (fileType == InputFileType.FBX)
        {

            LoadFBXScene();

        }
        else if (fileType == InputFileType.Session)
        {
            SessionHistory.Instance.Load(Path.Combine(Application.dataPath, "SessionData~", $"{InputFileName}.dat"));
        }
        else if (fileType == InputFileType.Latest)
        {
            // Check if there is any file in SessionData~/scene_name
            string savedSessionsPath = Path.Combine(Application.dataPath, "SessionData~", InputFileName);
            if (!Directory.Exists(savedSessionsPath))
            {
                Debug.Log($"No saved log found, loading from FBX: {InputFileName}");
                LoadFBXScene();
            }
            else
            {
                DirectoryInfo info = new DirectoryInfo(savedSessionsPath);
                FileInfo[] files = info.GetFiles();
                if (files.Length == 0)
                {
                    Debug.Log($"No saved log found, loading from FBX: {InputFileName}");
                    LoadFBXScene();
                }
                else
                {
                    // Find latest saved session
                    // Filter only dat files
                    FileInfo[] datSessionFiles = Array.FindAll(files, (f) => f.Extension == ".dat");
                    if (datSessionFiles.Length == 0)
                    {
                        Debug.Log($"No saved log found, loading from FBX: {InputFileName}");
                        LoadFBXScene();
                    }
                    else
                    {
                        // Sort by creation-time descending 
                        Array.Sort(datSessionFiles, (FileInfo f1, FileInfo f2) => f2.CreationTime.CompareTo(f1.CreationTime) );
                        FileInfo latestLog = datSessionFiles[0];
                        Debug.Log($"Loading latest saved log: {latestLog.FullName}");
                        SessionHistory.Instance.Load(Path.Combine(Application.dataPath, "SessionData~", latestLog.FullName));
                    }


                }
            }

        }
        else if (fileType == InputFileType.QuillBaseline)
        {
            LoadFBXScene(Path.Combine("Preload", "Study-baseline-results"));
            
            // Hide ref image
            foreach(Layer l in _allLayersList)
            {
                Debug.Log(l.LayerName);
                if (l.LayerName == "Reference")
                {
                    l.ToggleVisibility();
                }
            }
        }


    }

    private void LoadFBXScene(string folderPath= "Preload")
    {
        PaintCanvas canvas = GetComponentInParent<PaintCanvas>();

        // Scene-wide stack:
        // Todo: enforce explicitly that UID == 0 for the scene-wide layer
        BaseLayer scenWideLayer = CreateBaseLayer("Scene", registerInHistory: false);

        string preloadedFBXPath = Path.Combine(folderPath, InputFileName);
        Mesh[] loadedObjects = Resources.LoadAll<Mesh>(preloadedFBXPath);

        // Is there an accompagnying json file?
        //string preloadedJSONPath = Path.Combine("Preload", InputFileName + ".json");
        TextAsset targetFile = Resources.Load<TextAsset>(preloadedFBXPath);
        if (targetFile)
        {
            Debug.Log(targetFile.text);
            Dictionary<string, string[]> layerHierarchy = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(targetFile.text);
            foreach (string key in layerHierarchy.Keys)
            {
                Debug.Log($"Creating base layer for: {key}");
                BaseLayer l = CreateBaseLayer(key, registerInHistory: false);
                Debug.Log($"Base Layer UID = {l.UID}");
                ActiveLayer = l;
                foreach (string meshName in layerHierarchy[key])
                {
                    Mesh subMesh = loadedObjects.First(o => o.name == meshName);
                    PreloadedPrimitive p = canvas.Preload(preloadedFBXPath, meshName, subMesh);
                }

            }
        }
        else
        {
            for (int i = 0; i < loadedObjects.Length; i++)
            {
                Mesh subMesh = loadedObjects[i];
                Debug.Log(string.Format("mesh found: {0}", subMesh.name));
                // Create layer
                BaseLayer l = CreateBaseLayer($"{subMesh.name}", registerInHistory: false);
                Debug.Log($"Created base layer {l.UID} for Quill layer {subMesh.name}.");
                ActiveLayer = l;
                PreloadedPrimitive p = canvas.Preload(preloadedFBXPath, subMesh.name, subMesh);
            }
        }
    }

    private void OnDisable()
    {
        Selector.OnLayerActivate -= SetActive;
    }

    private void Update()
    {
        // Debug: switch layers blend mode on key press
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (ActiveLayer is ClippedLayer)
            {
                ((ClippedLayer)ActiveLayer).DebugIncrementBlendMode(-1);
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (ActiveLayer is ClippedLayer)
            {
                ((ClippedLayer)ActiveLayer).DebugIncrementBlendMode(1);

            }
        }

    }

    public bool IsActive(Layer l )
    {
        return l == ActiveLayer;
    }

    public bool IsActive(int uid)
    {
        return ActiveLayer != null && uid == ActiveLayer.UID;
    }

    public bool IsClippedLayer(Layer l)
    {
        return l != null && (l is ClippedLayer);
    }

    public string GetBaseLayerName(int uid)
    {
        if (uid == 0)
        {
            return "Scene";
        }
        if (!_baseLayers.ContainsKey(uid))
        {
            Debug.LogError($"Base Layer with uid {uid} does not exist");
            return "";
        }
        return _baseLayers[uid].LayerName;
    }

    public bool GetBaseLayerVisibility(int uid)
    {
        if (uid == 0)
            return true;
        if (!_baseLayers.ContainsKey(uid))
        {
            Debug.LogError($"Base Layer with uid {uid} does not exist");
            return false;
        }
        return _baseLayers[uid].Visible;
    }

    public List<ClippedLayer> GetStack(int uid)
    {
        if (!_layerStacks.ContainsKey(uid))
        {
            Debug.LogError($"Layer stack for base with uid {uid} does not exist");
            return null;
        }
        return _layerStacks[uid];
    }

    public Dictionary<int, List<ClippedLayer>> GetStacks()
    {
        return _layerStacks;
    }

    public int GetIndexInStack(ClippedLayer layer)
    {
        int baseUID = layer.clippingBaseUID;
        List<ClippedLayer> stack = _layerStacks[baseUID];
        int inStackZ = stack.FindIndex((l) => layer.UID == l.UID);
        return inStackZ;
    }

    public bool MoveLayerUpTheStack(ClippedLayer clippedLayer)
    {
        List<ClippedLayer> stack = _layerStacks[clippedLayer.clippingBaseUID];
        int stackIdx = GetIndexInStack(clippedLayer);
        if (stackIdx >= stack.Count - 1)
            return false;
        // Exchange this layer with the next up
        (stack[stackIdx + 1], stack[stackIdx]) = (stack[stackIdx], stack[stackIdx + 1]);
        SessionHistory.Instance.RegisterAction(
            new ReorderStackAction(clippedLayer.clippingBaseUID, stack, stackIdx, stackIdx + 1)
        );
        return true;
    }

    public bool MoveLayerDownTheStack(ClippedLayer clippedLayer)
    {
        List<ClippedLayer> stack = _layerStacks[clippedLayer.clippingBaseUID];
        int stackIdx = GetIndexInStack(clippedLayer);
        if (stackIdx <= 0)
            return false;
        // Exchange this layer with the next down
        (stack[stackIdx - 1], stack[stackIdx]) = (stack[stackIdx], stack[stackIdx - 1]);
        SessionHistory.Instance.RegisterAction(
            new ReorderStackAction(clippedLayer.clippingBaseUID, stack, stackIdx, stackIdx - 1)
        );
        return true;
    }

    public ClippedLayer GetClippedLayer(int baseUID, int clippedUID)
    {
        List<ClippedLayer> stack = GetStack(baseUID);
        // TODO: change data structure for layers maybe?
        return stack.Find((l) => l.UID == clippedUID);
    }

    public Primitive[] GetAllPrimitives()
    {
        Primitive[] primitives = GetComponentsInChildren<Primitive>(includeInactive: true);

        // Ensure they're sorted by UID
        Array.Sort(primitives);

        return primitives;
    }

    public Layer GetLayer(int uid)
    {
        if (uid >= 0 && uid < _allLayersList.Count)
        {
            return _allLayersList[uid];
        }
        else
        {
            Debug.LogError($"Layer with uid {uid} not found.");
            return null;
        }
    }

    public void SetActive(int uid)
    {
        // TODO: how to differentiate between base and clipped.. maybe we shouldn't have a concrete difference?
        //Debug.Log("clicked on layer " + uid);
        //ActiveLayer = ((ClippedLayer)GetLayer(uid)) ?? _activeLayer;
        ActiveLayer = GetLayer(uid) ?? _activeLayer;
        //Debug.Log(ActiveLayer.LayerName);
        //OnLayerUpdate?.Invoke(ActiveLayer);
    }

    public void SetHovered(int uid)
    {
        HoveredLayer = GetLayer(uid) ?? _hoveredLayer;
    }

    public void ClearHovered(int layerUID)
    {
        if (HoveredLayer && HoveredLayer.UID == layerUID)
            HoveredLayer = null;
    }

    public void ToggleVisibility(int uid)
    {
        Layer l = GetLayer(uid);
        if (l != null)
        {
            LayerParameters paramsBefore = l.GetParameters();
            l.ToggleVisibility();
            LayerParameters paramsAfter = l.GetParameters();
            SessionHistory.Instance.RegisterAction(
                new EditLayerAction(l, paramsBefore, paramsAfter)
            );

            OnLayerUpdate?.Invoke(l);
        }
    }

    public bool SwitchSelectedToLayer(int uid)
    {
        Layer targetLayer = GetLayer(uid);
        if (targetLayer == null)
            return false;

        return SwitchToLayer(targetLayer, CurrentSelection);
    }

    private bool SwitchToLayer(Layer targetLayer, Primitive[] grabbedObjects)
    {
        Layer prevLayer = ActiveLayer;
        // Switch active layer (this also updates gizmos)

        foreach (Primitive obj in grabbedObjects)
        {
            targetLayer.Add(obj);
        }

        SetActive(targetLayer.UID);

        foreach (Primitive obj in grabbedObjects)
        {
            Debug.Log("selecting " + obj.gameObject.name);
            obj.Select();
        }

        // Register in history
        SessionHistory.Instance.RegisterAction(
            new SwitchLayerAction(grabbedObjects, prevLayer, targetLayer)
        );

        //layerManager.TriggerLayerUpdateEvent(targetLayer.UID);


        return true;
    }


    public BaseLayer CreateBaseLayer(string name=null, bool registerInHistory=true)
    {
        if (_layerNextUID >= MAX_BASE_LAYERS_COUNT)
        {
            Debug.LogError("[Layer Manager] Max nb of base layers reached.");
            return null;
        }

        int layerID = layerNextUID;
        string layerName = name ?? $"Substrate {layerID}";
        GameObject o = new GameObject(layerName);
        o.AddComponent<BaseLayer>();
        o.transform.parent = transform;
        InitTransform(o);
        BaseLayer layer = o.GetComponent<BaseLayer>();
        layer.Init(layerName, layerID);

        // Add to data structures
        _baseLayers.Add(layerID, layer);
        //_allLayers.Add(layerID, layer);
        _layerStacks.Add(layerID, new List<ClippedLayer>());
        _allLayersList.Add(layer);

        // Notify change
        //OnBaseLayersUpdate?.Invoke();
        OnLayerUpdate?.Invoke(layer);

        // Register action in history
        if (registerInHistory)
        {
            SessionHistory.Instance.RegisterAction(
                new CreateLayerAction(layer)
            );
        }


        return layer;
    }


    // The new layer gets added to the top of the stack (composited last)
    public ClippedLayer CreateClippedLayer(int baseLayerUID, string name = null, bool registerInHistory = true, bool moveCurrentSelection=false)
    {
        if (!_layerStacks.ContainsKey(baseLayerUID))
        {
            Debug.LogError($"Base Layer with ID {baseLayerUID} is not registered in LayerManager.");
        }
        BaseLayer baseLayer = _baseLayers[baseLayerUID];
        int clippedLayerIDInStack = _layerStacks[baseLayerUID].Count + 1;
        string layerName = name ?? $"Layer {clippedLayerIDInStack} (over {baseLayer.LayerName})";
        GameObject o = new GameObject(layerName);
        o.AddComponent<ClippedLayer>();
        o.transform.parent = transform;
        InitTransform(o);
        ClippedLayer layer = o.GetComponent<ClippedLayer>();
        int layerID = layerNextUID;
        
        layer.Init(layerName, layerID, baseLayer.UID);
        _layerStacks[baseLayerUID].Add(layer);
        _allLayersList.Add(layer);

        // Register action in history
        if (registerInHistory)
        {
            SessionHistory.Instance.RegisterAction(
                new CreateLayerAction(layer)
            );
        }

        // Move current selection?
        if (moveCurrentSelection)
        {
            SwitchToLayer(layer, CurrentSelection);
        }

        // Set new layer as active by default
        ActiveLayer = layer;

        // Notify change
        //OnStackedLayersUpdate?.Invoke();
        OnLayerUpdate?.Invoke(layer);

        Debug.Log("creating clipped layer " + name);




        return layer;
    }

    public void AddToActive(Primitive p)
    {
        if (ActiveLayer != null)
        {
            ActiveLayer.Add(p);
            OnLayerUpdate?.Invoke(ActiveLayer);
        }

    }

    public void TriggerLayerUpdateEvent(int uid)
    {
        Layer updatedLayer = GetLayer(uid);
        if (updatedLayer != null)
        {
            OnLayerUpdate?.Invoke(updatedLayer);
        }
    }

    public Dictionary<int, Layer> LoadLayers(SerializableSessionState state)
    {
        // Reinit state
        _allLayersList.Clear();
        _layerStacks.Clear();
        _baseLayers.Clear();

        _layerNextUID = 0;

        Dictionary<int, Layer> loadedLayers = new Dictionary<int, Layer>();

        // Recreate the layers list
        foreach(SerializableLayer layerData in state.layers)
        {
            // Create layer gameobject
            GameObject layerObj = new GameObject(layerData.name);
            layerObj.transform.parent = transform;

            if (layerData.clippingBaseUID == -1)
            {
                layerObj.AddComponent<BaseLayer>();
                BaseLayer layer = layerObj.GetComponent<BaseLayer>();
                layer.Init(layerData);

                // Add to data structures
                _allLayersList.Add(layer);
                _baseLayers.Add(layerData.UID, layer);
                //_layerStacks.Add(layerData.UID, new List<ClippedLayer>());

                loadedLayers.Add(layerData.UID, layer);

                OnLayerUpdate?.Invoke(layer);
            }
            else
            {
                layerObj.AddComponent<ClippedLayer>();
                ClippedLayer layer = layerObj.GetComponent<ClippedLayer>();

                layer.Init(layerData);
                //_layerStacks[layerData.clippingBaseUID].Add(layer);
                _allLayersList.Add(layer);
                loadedLayers.Add(layerData.UID, layer);

                //OnLayerUpdate?.Invoke(layer);
            }

            // Register layer UID
            assignedLayerUIDs.Add(layerData.UID);

        }

        // Recreate the stack
        foreach (SerializableStack stackData in state.layerStacks)
        {
            int baseUID = stackData.baseLayerUID;
            List<ClippedLayer> stack = new List<ClippedLayer>();
            //Debug.Log("stack base = " + _baseLayers[baseUID].LayerName);

            // Then recreate the stacked layers
            foreach (int stackedLayerUID in stackData.stackedLayersUID)
            {
                stack.Add((ClippedLayer)_allLayersList[stackedLayerUID]);
                //Debug.Log("appearance layer = " + _allLayersList[stackedLayerUID].LayerName);
            }

            // Add stack
            _layerStacks.Add(baseUID, stack);
        }

        return loadedLayers;
    }

    //public void SetTextureMask(Vector3 direction, float min=0, float max=1)
    //{
    //        float baseScale = 30f;
    //        float maxScale = 500f;
    //        Vector3 scale = Vector3.one * baseScale + direction * maxScale;

    //        ActiveLayer.AddTextureMask(scale, min, max);
    //        Debug.Log("texture mask");
    //}

    public SerializableLayer[] SerializeLayers()
    {
        int layerCount = _allLayersList.Count;
        SerializableLayer[] layersData = new SerializableLayer[layerCount];

        //for (int i = 0; i < BaseLayersUID.Length; i++)
        //{
        //    layersData[i] = _baseLayers[BaseLayersUID[i]].Serialize();
        //}

        //int[] clippedLayersIDS = _allLayers.Keys.ToArray();

        //for (int i = 0; i < clippedLayersIDS.Length; i++)
        //{
        //    layersData[i + BaseLayersUID.Length] = _allLayers[clippedLayersIDS[i]].Serialize();
        //}

        for (int i = 0; i < layerCount; i++)
        {
            layersData[i] = _allLayersList[i].Serialize();
        }

        return layersData;
    }

    public SerializableStack[] SerializeStacks()
    {
        SerializableStack[] stacks = new SerializableStack[_layerStacks.Count];
        int i = 0;
        foreach(var stack in _layerStacks)
        {
            stacks[i] = new SerializableStack();
            stacks[i].baseLayerUID = stack.Key;
            stacks[i].stackedLayersUID = stack.Value.ConvertAll(l => l.UID).ToArray();
            i++;
        }
        return stacks;
    }

    private void InitTransform(GameObject newObject)
    {
        newObject.transform.localPosition = Vector3.zero;
        newObject.transform.localRotation = Quaternion.identity;
        newObject.transform.localScale = Vector3.one;
    }

}

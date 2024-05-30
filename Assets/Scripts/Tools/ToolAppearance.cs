using HSVPicker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolAppearance : MonoBehaviour
{
    [Header("Refs")]
    public ColorPicker picker;
    //public LayerManager layerManager;

    [Header("Materials Per Tool")]
    public Material Painter;
    public Material Default;
    public Material Eraser;
    public Material UIPointer;

    [Header("Pointers Per Tool")]
    public GameObject UIPointerObject;
    public GameObject GradientPointerObject;

    private MeshRenderer controllerRenderer;

    private Layer activeLayer;

    // Start is called before the first frame update
    //void Start()
    //{
    //    controllerRenderer = GetComponent<MeshRenderer>();
    //    TogglePointer(false); // Init pointer as the normal (non-UI) pointer
    //    //toolsManager = GetComponent<ToolsManager>();
    //}

    void OnEnable()
    {
        //ToolsManager.OnUIZoneEnter += HandleUIZoneEnter;
        //ToolsManager.OnUIZoneExit += HandleUIZoneExit;
        ToolsManager.OnToolChange += HandleToolChange;
        ToolsManager.OnToolRadiusChange += HandleRadiusChange;

        LayerManager.OnActiveLayerUpdate += HandleActiveLayerChange;
        //LayerManager.OnLayerUpdate += HandleLayerUpdate;

        picker.onValueChanged.AddListener(HandleColorChange);
        HandleColorChange(picker.CurrentColor);

        controllerRenderer = GetComponent<MeshRenderer>();
        TogglePointer(false); // Init pointer as the normal (non-UI) pointer
    }

    void OnDisable()
    {
        //ToolsManager.OnUIZoneEnter -= HandleUIZoneEnter;
        //ToolsManager.OnUIZoneExit -= HandleUIZoneExit;
        ToolsManager.OnToolChange -= HandleToolChange;
        ToolsManager.OnToolRadiusChange -= HandleRadiusChange;
        LayerManager.OnActiveLayerUpdate -= HandleActiveLayerChange;
    }

    void OnWillRenderObject()
    {
        if (activeLayer as ClippedLayer != null)
        {
            Painter.SetFloat("_DepthThreshold", ((ClippedLayer)activeLayer).WorldSpaceDepthThreshold);
        }
    }


    private void HandleToolChange(Tool toolType)
    {
        //Debug.Log("switching tool to " + toolType);
        HideAllPointers();
        GameObject selectedPointer = this.gameObject;
        switch (toolType)
        {
            case Tool.UIPointer:
                //SetControllerAppearance(UIPointer);
                selectedPointer = UIPointerObject;
                break;
            case Tool.Eraser:
                selectedPointer = this.gameObject;
                SetControllerAppearance(Eraser);
                break;
            case Tool.Painter:
                selectedPointer = this.gameObject;
                SetControllerAppearance(Painter);
                break;
            case Tool.Gradient:
                selectedPointer = GradientPointerObject;
                break;
            default:
                SetControllerAppearance(Default);
                break;

        }
        selectedPointer.GetComponent<MeshRenderer>().enabled = true;
    }

    private void HandleRadiusChange(float toolRadius)
    {
        transform.localScale = new Vector3(toolRadius, toolRadius, toolRadius);
    }

    private void SetControllerAppearance(Material mat)
    {
        controllerRenderer.material = mat;
    }

    private void HideAllPointers()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        UIPointerObject.GetComponent<MeshRenderer>().enabled = false;
        GradientPointerObject.GetComponent<MeshRenderer>().enabled = false;
    }

    private void TogglePointer(bool UIMode)
    {
        gameObject.GetComponent<MeshRenderer>().enabled = !UIMode;
        UIPointerObject.GetComponent<MeshRenderer>().enabled = UIMode;
    }

    private void HandleActiveLayerChange(Layer activeLayer)
    {
        this.activeLayer = activeLayer;
        if (activeLayer as ClippedLayer != null)
        {
            Painter.SetInt("_GroupIDIntersect", ((ClippedLayer)activeLayer).IntersectStencilRef);
            Painter.SetInt("_GroupIDPermissive", ((ClippedLayer)activeLayer).PermissiveStencilRef);
            //Painter.SetFloat("_DepthThreshold", ((ClippedLayer)activeLayer).DepthThreshold);

            Painter.SetFloat("_BaseOpacity", 0.5f);

            // Base ID == 0 => scene-wide stack
            Painter.SetInt(Shader.PropertyToID("_StencilComparison"), ((ClippedLayer)activeLayer).StencilComparison); // Less

            //Debug.Log("Switched to a clipped layer");
        }
        else
        {
            // TODO: render the brush as a solid color sphere: we are painting on a base layer
            Painter.SetFloat("_BaseOpacity", 1f);
        }
    }

    //private void HandleLayerUpdate(Layer updatedLayer)
    //{
    //    if (updatedLayer as ClippedLayer != null)
    //    {
    //        Painter.SetFloat("_DepthThreshold", ((ClippedLayer)updatedLayer).DepthThreshold);

    //        Debug.Log(updatedLayer.gameObject.transform.lossyScale);
    //    }
    //}

    private void HandleColorChange(Color c)
    {
        Painter.SetColor("_PaintColor", c.linear);
    }
}

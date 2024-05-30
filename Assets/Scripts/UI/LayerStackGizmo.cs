using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LayerStackGizmo : MonoBehaviour
{
    public TextMeshProUGUI layerStackName;
    public RectTransform layerStackPanel;
    public RectTransform topStackPanelContainer;

    private InSituUIManager uiManager;
    private Material material;

    private Dictionary<int, LayerButton> buttonsPerClippedLayer;

    public int BaseLayerUID
    {
        get; private set;
    }

    private bool highlight = false;
    private bool active = false;

    private Color GizmoColor
    {
        get
        {
            return highlight ? UIConstants.HIGHLIGHT_LAYER_GIZMO : (active ? UIConstants.ACTIVE_LAYER_GIZMO : UIConstants.DEFAULT_LAYER_GIZMO);
        }
    }

    private void Awake()
    {
        ToolsManager.OnToolChange += HandleModeChange;
        material = new Material(Shader.Find("VRPaint/LayerGizmoShader"));
        GetComponentInChildren<MeshRenderer>().material = material; // There is only 1 mesh renderer in children => the sphere mesh renderer
        material.SetColor(Shader.PropertyToID("_BaseColor"), GizmoColor);

        buttonsPerClippedLayer = new Dictionary<int, LayerButton>();

        // Hiding panel because it's not working properly yet
        //ShowPanel(false);

        topStackPanelContainer.gameObject.SetActive(false);

    }

    private void OnDestroy()
    {
        ToolsManager.OnToolChange -= HandleModeChange;

    }

    public void Init(InSituUIManager manager, BaseLayer baseLayer)
    {
        uiManager = manager;
        BaseLayerUID = baseLayer.UID;
        SetLayerName(baseLayer.name);
    }

    public void HandleCollide()
    {
        Highlight(true);
    }

    public void HandleExit()
    {
        Highlight(false);
    }


    public void SetLayerName(string name)
    {
        layerStackName.text = name;
    }

    public void SetSelected(bool active)
    {
        layerStackName.color = active ? UIConstants.ACTIVE_COLOR : UIConstants.DEFAULT_TEXT_COLOR;
        this.active = active;
        material.SetColor(Shader.PropertyToID("_BaseColor"), GizmoColor);

        // Hiding panel because it's not working properly yet
        //ShowPanel(active);
    }

    public void ShowPanel(bool show)
    {
        layerStackPanel.gameObject.SetActive(show);
    }

    private void HandleModeChange(Tool selectedTool)
    {
        gameObject.SetActive(selectedTool == Tool.Selector || selectedTool == Tool.UIPointer);
    }

    public void Highlight(bool state)
    {
        highlight = state;
        material.SetColor(Shader.PropertyToID("_BaseColor"), GizmoColor);
    }

    public void UpdateClippedLayer(ClippedLayer layer, bool isSelected, int order)
    {
        if (!buttonsPerClippedLayer.ContainsKey(layer.UID))
        {
            // Create button
            RectTransform container = topStackPanelContainer.GetComponentInChildren<DragZone>().itemsContainer;
            GameObject layerButtonObj = Instantiate(Resources.Load<GameObject>("Layer Button (small font)"), container);
            LayerButton newLayerButton = layerButtonObj.GetComponent<LayerButton>();
            buttonsPerClippedLayer[layer.UID] = newLayerButton;

            newLayerButton.Init(layer.UID);

            // Update name
            newLayerButton.SetName(layer.LayerName);

            // Add callback
            newLayerButton.MainButton.OnButtonClick.AddListener((e) => { uiManager.SetActiveLayer(layer.UID); });
            newLayerButton.VisibilityButton.OnButtonClick.AddListener((e) => { uiManager.ToggleVisibility(layer.UID); });
            //layerButton.MoreButton.OnButtonClick.AddListener((e) => { uiManager.SwitchToDetailView(layer.UID); });
        }

        LayerButton layerButton = buttonsPerClippedLayer[layer.UID];
        // Is active?
        layerButton.SetColor(isSelected ? UIConstants.ACTIVE_COLOR : UIConstants.DEFAULT_TEXT_COLOR);

        // Order in stack?
        //Debug.Log($"Layer {layer.LayerName} is at sibling index {order}");
        layerButton.transform.SetSiblingIndex(order);

        // Update visibility icon
        layerButton.SetVisibility(layer.Visible);

        topStackPanelContainer.GetComponentInChildren<DragZone>().AlignTop();
    }

    public void OnNewLayerButtonClick()
    {
        uiManager.CreateLayer(BaseLayerUID);
    }

    //public int GetLayerUID()
    //{
    //    return GetComponentInParent<Layer>().UID;
    //}

}

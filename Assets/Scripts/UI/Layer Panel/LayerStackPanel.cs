using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LayerStackPanel : LayerSubPanel
{
    //public Color ActiveLayerButtonColor;
    //public Color DefaultLayerButtonColor;

    public GameObject layerButtonPrefab;

    public TextMeshProUGUI layerNameBox;
    public GameObject main;

    public PokableSpriteButton reorderButton;
    public PokableSpriteButton createLayerAndMoveSelectionButton;

    private Dictionary<int, LayerButton> layerButtons = new Dictionary<int, LayerButton>();

    private bool _reorderingLayers = false;
    private bool ReorderingLayers
    {
        get
        {
            return _reorderingLayers;
        }
        set
        {
            _reorderingLayers = value;
            reorderButton.SetSprite((_reorderingLayers ? 1 : 0));
            reorderButton.SetColor(_reorderingLayers ? UIConstants.ACTIVE_COLOR : Color.white);
            foreach (KeyValuePair<int, LayerButton> buttonEl in layerButtons)
            {
                // Do not display move option for base layer
                if (buttonEl.Key == ElementUID)
                    continue;
                buttonEl.Value.ToggleLayerReorder(_reorderingLayers);
            }
        }
    }

    //private void Awake()
    //{
    //    layerButtons = new Dictionary<int, LayerButton>();
    //}

    private void OnEnable()
    {
        //LayerManager.OnActiveLayerUpdate += UpdateView;
        //LayerManager.OnStackedLayersUpdate += UpdateView;
        LayerManager.OnLayerUpdate += UpdateLayerButton;

    }


    public override void UpdateView()
    {
        //Clear(main);

        layerNameBox.text = PanelManager.layerManager.GetBaseLayerName(ElementUID);

        // Add base layer button first
        LayerButton baseLayerButton;
        if (!layerButtons.ContainsKey(ElementUID))
        {
            //Debug.Log("instantiate button for base " + PanelManager.layerManager.GetBaseLayerName(ElementUID));
            GameObject layerButtonObj = Instantiate(layerButtonPrefab, main.transform);

            baseLayerButton = layerButtonObj.GetComponent<LayerButton>();
            baseLayerButton.Init(ElementUID, visibilityToggle: false, moreButton: false);
            layerButtons[ElementUID] = baseLayerButton;

            // Add callbacks
            // - if this is the "Scene" stack, we make the layer non-active-able
            if (ElementUID == 0)
            {
                baseLayerButton.MainButton.Active = false;
            }
            else
            {
                baseLayerButton.MainButton.OnButtonClick.AddListener((e) => { PanelManager.SetActiveLayer(ElementUID); });
                baseLayerButton.MainButton.OnButtonEnter.AddListener((e) => { PanelManager.SetHoveredLayer(ElementUID); });
                baseLayerButton.MainButton.OnButtonExit.AddListener((e) => { PanelManager.ClearHoveredLayer(ElementUID); });
            }

            baseLayerButton.VisibilityButton.OnButtonClick.AddListener((e) => { PanelManager.ToggleVisibility(ElementUID); });
            baseLayerButton.MoreButton.OnButtonClick.AddListener((e) => { PanelManager.SwitchToStackView(ElementUID); });

            baseLayerButton.MoveToLayerButton.OnButtonClick.AddListener((e) => { PanelManager.layerManager.SwitchSelectedToLayer(ElementUID); });
        }
        else
        {
            baseLayerButton = layerButtons[ElementUID];
        }

        // Update name
        //Debug.Log(layerButton);
        baseLayerButton.SetName(PanelManager.layerManager.GetBaseLayerName(ElementUID));
        baseLayerButton.SetEmphasize(true);
        baseLayerButton.SetVisibility(PanelManager.layerManager.GetBaseLayerVisibility(ElementUID));
        baseLayerButton.SetColor(PanelManager.layerManager.IsActive(ElementUID) ? UIConstants.ACTIVE_COLOR : UIConstants.DEFAULT_TEXT_COLOR);

        // Move to layer button => activate if selection is not-empty AND layer is not active
        baseLayerButton.ToggleMoveButton(PanelManager.layerManager.CurrentSelection.Length > 0 && !PanelManager.layerManager.IsActive(ElementUID));

        baseLayerButton.transform.SetSiblingIndex(0);

        // Get stack
        List<ClippedLayer> stack = PanelManager.layerManager.GetStack(ElementUID);


        // Reorder button? Visible if n_layers > 1
        reorderButton.gameObject.SetActive(stack.Count > 1);

        foreach (ClippedLayer l in stack)
        {
            //if (!l.Enabled)
            //    continue;

            LayerButton layerButton;
            //Debug.Log(l.LayerName);
            if (!layerButtons.ContainsKey(l.UID))
            {
                //Debug.Log("instantiate button for layer " + l.LayerName);
                GameObject layerButtonObj = Instantiate(layerButtonPrefab, main.transform);

                layerButton = layerButtonObj.GetComponent<LayerButton>();
                layerButton.Init(l.UID);
                layerButtons[l.UID] = layerButton;

                // Add callback
                layerButton.MainButton.OnButtonClick.AddListener((e) => { PanelManager.SetActiveLayer(l.UID); });
                layerButton.MainButton.OnButtonEnter.AddListener((e) => { PanelManager.SetHoveredLayer(l.UID); });
                layerButton.MainButton.OnButtonExit.AddListener((e) => { PanelManager.ClearHoveredLayer(l.UID); });
                layerButton.VisibilityButton.OnButtonClick.AddListener((e) => { PanelManager.ToggleVisibility(l.UID); });
                layerButton.MoreButton.OnButtonClick.AddListener((e) => { PanelManager.SwitchToDetailView(l.UID); });
                layerButton.MoveToLayerButton.OnButtonClick.AddListener((e) => { PanelManager.layerManager.SwitchSelectedToLayer(l.UID); });

                layerButton.MoveUpButton.OnButtonClick.AddListener((e) => { 
                    bool success = PanelManager.layerManager.MoveLayerUpTheStack(l);
                    if (success)
                    {
                        UpdateView();
                    }
                });
                layerButton.MoveDownButton.OnButtonClick.AddListener((e) => {
                    bool success = PanelManager.layerManager.MoveLayerDownTheStack(l);
                    if (success)
                    {
                        UpdateView();
                    }
                });
            }
            else
            {
                layerButton = layerButtons[l.UID];
            }

            // If layer was deleted, hide button
            layerButton.gameObject.SetActive(l.Enabled);

            // Update name
            layerButton.SetName(l.LayerName);

            // Update visibility icon
            layerButton.SetVisibility(l.Visible);

            // Update order
            layerButton.transform.SetSiblingIndex(1 + PanelManager.layerManager.GetIndexInStack(l));
            //Debug.Log("sibling index of Layer " + l.LayerName + " , " + PanelManager.layerManager.GetIndexInStack(l));


            // Is active?
            if (PanelManager.layerManager.IsActive(l))
            {
                //layerButtonObj.GetComponentInChildren<TextMeshProUGUI>().color = ActiveLayerButtonColor;
                layerButton.SetColor(UIConstants.ACTIVE_COLOR);
            }
            else
            {
                layerButton.SetColor(UIConstants.DEFAULT_TEXT_COLOR);
            }

            // Move to layer button => activate if selection is not-empty AND layer is not active
            layerButton.ToggleMoveButton(PanelManager.layerManager.CurrentSelection.Length > 0 && !PanelManager.layerManager.IsActive(l));
        }

        // Update scroll view
        if (GetComponentInChildren<DragZone>() != null)
        {
            DragZone dragZone = GetComponentInChildren<DragZone>();
            Layer activeLayer = PanelManager.layerManager.ActiveLayer;
            if (PanelManager.layerManager.IsClippedLayer(activeLayer))
            {
                // Scroll so that this layer is in view
                int layerStackIdx = PanelManager.layerManager.GetIndexInStack((ClippedLayer)activeLayer);
                float y = ((float)layerStackIdx) / stack.Count;
                //Debug.Log("aligning to y = " + y);
                //dragZone.AlignTo(y);
                if (gameObject.activeSelf)
                    StartCoroutine(ScrollTo(y));
                else
                    dragZone.AlignTo(y);
            }
            else
            {
                //Debug.Log("scroll to bottom");
                if (gameObject.activeSelf)
                    StartCoroutine(ScrollTo(0f));
                else
                    dragZone.AlignBottom();
                //dragZone.AlignBottom();
            }

        }

        // Update footer buttons
        // Create and move to layer button => activate if selection is not-empty
        createLayerAndMoveSelectionButton.gameObject.SetActive(PanelManager.layerManager.CurrentSelection.Length > 0);
    }

    private IEnumerator ScrollTo(float y)
    {
        if (y > 0f)
            yield return new WaitForSeconds(0.2f);
        DragZone dragZone = GetComponentInChildren<DragZone>();
        dragZone.AlignTo(y);
    }


    public void UpdateLayerButton(Layer layer)
    {
        if (layerButtons.ContainsKey(layer.UID))
            UpdateView();
    }


    //public void UpdateLayerButton(Layer layer)
    //{
    //    // Make sure this layer does belong in this stack
    //    if (!layer.InStack(ElementUID))
    //    {
    //        return;
    //    }
    //    LayerButton[] buttons = main.GetComponentsInChildren<LayerButton>();
    //    LayerButton matchingButton = buttons.ToList().Find((b) => b.LayerUID == layer.UID);

    //    if (matchingButton != null && !layer.Enabled)
    //    {
    //        Destroy(matchingButton.gameObject);
    //        return;
    //    }
    //    else if (matchingButton == null)
    //    {
    //        if (!layer.Enabled)
    //            return;

    //        GameObject layerButtonObj = Instantiate(layerButtonPrefab, main.transform);

    //        matchingButton = layerButtonObj.GetComponent<LayerButton>();
    //        matchingButton.Init(layer.UID);

    //        // Add callback
    //        matchingButton.MainButton.OnButtonClick.AddListener((e) => { PanelManager.SetActiveLayer(layer.UID); });
    //        matchingButton.VisibilityButton.OnButtonClick.AddListener((e) => { PanelManager.ToggleVisibility(layer.UID); });
    //        matchingButton.MoreButton.OnButtonClick.AddListener((e) => { PanelManager.SwitchToDetailView(layer.UID); });
    //    }

    //    // Update name
    //    matchingButton.SetName(layer.LayerName);

    //    // Update visibility icon and color
    //    matchingButton.SetVisibility(layer.Visible);
    //    matchingButton.SetColor(PanelManager.layerManager.IsActive(layer) ? UIConstants.ACTIVE_COLOR : UIConstants.DEFAULT_TEXT_COLOR);

    //}

    public void OnNewLayerButtonClick()
    {
        PanelManager.layerManager.CreateClippedLayer(ElementUID);
        UpdateView();

        // Disable layer reordering
        ReorderingLayers = false;
    }

    public void OnNewLayerWithSelectionButtonClick()
    {
        PanelManager.layerManager.CreateClippedLayer(ElementUID, moveCurrentSelection: true);
        UpdateView();

        // Disable layer reordering
        ReorderingLayers = false;
    }

    public void OnReorderButtonClick()
    {
        ReorderingLayers = !ReorderingLayers;
        //reorderButton.SetSprite((ReorderingLayers ? 1 : 0));
        //reorderButton.SetColor(ReorderingLayers ? UIConstants.ACTIVE_COLOR : Color.white);
        //foreach (KeyValuePair<int,LayerButton> buttonEl in layerButtons)
        //{
        //    // Do not display move option for base layer
        //    if (buttonEl.Key == ElementUID)
        //        continue;
        //    buttonEl.Value.ToggleLayerReorder(ReorderingLayers);
        //}
    }

    private void OnDisable()
    {
        Clear();
    }

    public override void Clear()
    {
        ReorderingLayers = false;
        layerButtons.Clear();
        // Destroy all children (layer buttons)
        foreach (Transform o in main.transform)
        {
            Destroy(o.gameObject);
        }
    }
}

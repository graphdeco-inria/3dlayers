using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// TODO: does this need to inherit from LayerSubPanel?
public class LayerBasesPanel : LayerSubPanel
{
    public GameObject layerButtonPrefab;

    private Dictionary<int, LayerButton> baseLayerButtons = new Dictionary<int, LayerButton>();
    private void Start()
    {
        //LayerManager.OnActiveLayerUpdate += UpdateView;
        //LayerManager.OnStackedLayersUpdate += UpdateView;
        LayerManager.OnLayerUpdate += UpdateLayerButton;
    }

    private void OnDestroy()
    {
        LayerManager.OnLayerUpdate -= UpdateLayerButton;
    }

    public override void UpdateView()
    {
        //Debug.Log("update layers UI");
        int[] currentBaseLayers = PanelManager.layerManager.BaseLayersUID;
        foreach (int uid in currentBaseLayers)
        {
            LayerButton layerButton;
            if (!baseLayerButtons.ContainsKey(uid))
            {
                //Debug.Log("instantiate button");
                GameObject layerButtonObj = Instantiate(layerButtonPrefab, transform);

                layerButton = layerButtonObj.GetComponent<LayerButton>();
                // - if this is the "Scene" stack, we make the layer non-hide-able
                layerButton.Init(uid, visibilityToggle: uid > 0, moreButton: true);
                baseLayerButtons[uid] = layerButton;

                // Add callbacks
                // - if this is the "Scene" stack, we make the layer non-active-able
                if (uid == 0)
                {
                    layerButton.MainButton.Active = false;
                }
                else
                {
                    layerButton.MainButton.OnButtonClick.AddListener((e) => { PanelManager.SetActiveLayer(uid); });
                    layerButton.MainButton.OnButtonEnter.AddListener((e) => { PanelManager.SetHoveredLayer(uid); });
                    layerButton.MainButton.OnButtonExit.AddListener((e) => { PanelManager.ClearHoveredLayer(uid); });
                    layerButton.VisibilityButton.OnButtonClick.AddListener((e) => { PanelManager.ToggleVisibility(uid); });
                }


                layerButton.MoreButton.OnButtonClick.AddListener((e) => { PanelManager.SwitchToStackView(uid); });

                layerButton.MoveToLayerButton.OnButtonClick.AddListener((e) => { PanelManager.layerManager.SwitchSelectedToLayer(uid); });
            }
            else
            {
                layerButton = baseLayerButtons[uid];
            }

            // Update name
            //Debug.Log(layerButton);
            layerButton.SetName(PanelManager.layerManager.GetBaseLayerName(uid));
            layerButton.SetVisibility(PanelManager.layerManager.GetBaseLayerVisibility(uid));
            layerButton.SetColor(PanelManager.layerManager.IsActive(uid) ? UIConstants.ACTIVE_COLOR : UIConstants.DEFAULT_TEXT_COLOR);

            // Move to layer button => activate if selection is not-empty AND layer is not active
            layerButton.ToggleMoveButton(PanelManager.layerManager.CurrentSelection.Length > 0 && !PanelManager.layerManager.IsActive(uid));
        }
    }

    public void UpdateLayerButton(Layer layer)
    {
        UpdateView();
        //LayerButton[] buttons = GetComponentsInChildren<LayerButton>();
        //foreach (var button in buttons)
        //{
        //    if (button.LayerUID == layer.UID)
        //    {
        //        button.SetColor(PanelManager.layerManager.IsActive(layer) ? UIConstants.ACTIVE_COLOR : UIConstants.DEFAULT_TEXT_COLOR);
        //        button.SetVisibility(layer.Visible);
        //    }
        //}
    }

    // Start is called before the first frame update
    //void Awake()
    //{
    //    Debug.Log("awake" + name);
    //    baseLayerButtons

    //}

    public override void Clear()
    {
        baseLayerButtons.Clear();
        // Destroy all children (layer buttons)
        foreach (Transform o in transform)
        {
            Destroy(o.gameObject);
        }
    }
}

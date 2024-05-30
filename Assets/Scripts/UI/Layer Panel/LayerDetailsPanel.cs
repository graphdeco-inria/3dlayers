using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class LayerDetailsPanel : LayerSubPanel
{

    public PokableSelect BlendModeSelect;
    //public PokableSelect CompositingModeSelect;
    public PokableSelect MaskModeSelect;
    public PokableSlider OpacitySlider;
    public PokableSlider DepthThresholdSlider;

    public TextMeshProUGUI LayerNameBox;

    private void OnEnable()
    {
        LayerManager.OnLayerUpdate += (l) => { if (l.UID == ElementUID) UpdateView(); };
    }

    public override void UpdateView()
    {

        if (!PanelManager || !PanelManager.layerManager.IsClippedLayer(PanelManager.layerManager.GetLayer(ElementUID)))
        {
            return;
        }
        ClippedLayer currentLayer = (ClippedLayer)PanelManager.layerManager.GetLayer(ElementUID);
        if (currentLayer == null)
            Debug.LogError("Attempting to open a panel for a clipped layer that doesn't exist.");
        else
        {
            // Based on active layer, update all properties
            BlendModeSelect.InitValue((int)currentLayer.Blend);
            //CompositingModeSelect.InitValue((int)currentLayer.Compositing);
            OpacitySlider.InitValue(currentLayer.Opacity);
            MaskModeSelect.InitValue((int)currentLayer.Masking);

            //if (currentLayer.Compositing == CompositingMode.Over)
            //{
            DepthThresholdSlider.InitValue(currentLayer.DepthThreshold);
            DepthThresholdSlider.transform.parent.gameObject.SetActive(true);
            //}
            //else
            //{
            //    DepthThresholdSlider.transform.parent.gameObject.SetActive(false);
            //}

            LayerNameBox.text = PanelManager.layerManager.ActiveLayer.LayerName;
        }

    }

    public void UpdateLayerOpacity(float newValue)
    {
        ClippedLayer currentLayer = (ClippedLayer)PanelManager.layerManager.GetLayer(ElementUID);
        if (currentLayer == null)
            Debug.LogError("Attempting to set opacity for a clipped layer that doesn't exist.");
        else
        {
            if (currentLayer.Opacity != newValue)
            {
                LayerParameters paramsBefore = currentLayer.GetParameters();
                currentLayer.Opacity = newValue;
                LayerParameters paramsAfter = currentLayer.GetParameters();
                SessionHistory.Instance.RegisterAction(
                    new EditLayerAction(currentLayer, paramsBefore, paramsAfter)
                );
            }
        }
    }

    public void UpdateLayerDepthThreshold(float newValue)
    {
        ClippedLayer currentLayer = (ClippedLayer)PanelManager.layerManager.GetLayer(ElementUID);
        if (currentLayer == null)
            Debug.LogError("Attempting to set depth threshold for a clipped layer that doesn't exist.");
        else
        {
            //if (currentLayer.DepthThreshold != newValue)
            //{
                LayerParameters paramsBefore = currentLayer.GetParameters();
                currentLayer.DepthThreshold = newValue;
                PanelManager.layerManager.TriggerLayerUpdateEvent(ElementUID);
                LayerParameters paramsAfter = currentLayer.GetParameters();
                SessionHistory.Instance.RegisterAction(
                    new EditLayerAction(currentLayer, paramsBefore, paramsAfter)
                );
            //}
        }
    }

    public void UpdateLayerBlendMode(int blendMode)
    {
        ClippedLayer currentLayer = (ClippedLayer)PanelManager.layerManager.GetLayer(ElementUID);
        if (currentLayer == null)
            Debug.LogError("Attempting to set blend mode for a clipped layer that doesn't exist.");
        else
        {
            if (currentLayer.Blend != (ColorBlendMode)blendMode)
            {
                LayerParameters paramsBefore = currentLayer.GetParameters();
                currentLayer.Blend = (ColorBlendMode)blendMode;
                LayerParameters paramsAfter = currentLayer.GetParameters();
                SessionHistory.Instance.RegisterAction(
                    new EditLayerAction(currentLayer, paramsBefore, paramsAfter)
                );
            }

        }
    }

    //public void UpdateLayerCompositingMode(int compositingMode)
    //{
    //    ClippedLayer currentLayer = (ClippedLayer)PanelManager.layerManager.GetLayer(ElementUID);
    //    if (currentLayer == null)
    //        Debug.LogError("Attempting to set compositing mode for a clipped layer that doesn't exist.");
    //    else
    //    {
    //        if (currentLayer.Compositing != (CompositingMode)compositingMode)
    //        {
    //            LayerParameters paramsBefore = currentLayer.GetParameters();
    //            currentLayer.Compositing = (CompositingMode)compositingMode;
    //            LayerParameters paramsAfter = currentLayer.GetParameters();
    //            SessionHistory.Instance.RegisterAction(
    //                new EditLayerAction(currentLayer, paramsBefore, paramsAfter)
    //            );
    //        }
    //    }
    //}

    public void UpdateLayerMaskMode(int maskMode)
    {
        ClippedLayer currentLayer = (ClippedLayer)PanelManager.layerManager.GetLayer(ElementUID);
        if (currentLayer == null)
            Debug.LogError("Attempting to set layer mask mode for a clipped layer that doesn't exist.");
        else
        {
            if (currentLayer.Masking != (MaskMode)maskMode)
            {
                LayerParameters paramsBefore = currentLayer.GetParameters();
                currentLayer.Masking = (MaskMode)maskMode;
                LayerParameters paramsAfter = currentLayer.GetParameters();
                SessionHistory.Instance.RegisterAction(
                    new EditLayerAction(currentLayer, paramsBefore, paramsAfter)
                );
            }

        }
    }

    public override void Clear()
    {
        return;
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum LayerPanelView
{
    Bases, Stack, Layer
}


public class LayerPanel : MonoBehaviour
{
    public LayerManager layerManager;
    //public GameObject LayerButtonPrefab;


    private LayerPanelView _activeView;
    private LayerPanelView ActiveView
    {
        get { return _activeView; }
        set
        {
            if (availablePanels.ContainsKey(value))
            {
                if (availablePanels.ContainsKey(_activeView))
                    StartCoroutine(SwitchOffSubPanel(availablePanels[_activeView]));

                _activeView = value;

                StartCoroutine(SwitchOnSubPanel());
            }
            else
            {
                Debug.LogError($"Panel {value} is not available.");
            }

        }
    }

    private IEnumerator SwitchOffSubPanel(LayerSubPanel panel)
    {
        yield return new WaitForSeconds(0.1f);

        panel.gameObject.SetActive(false);
        panel.Clear();
    }

    private IEnumerator SwitchOnSubPanel()
    {
        yield return new WaitForSeconds(0.1f);

        availablePanels[ActiveView].gameObject.SetActive(true);
        availablePanels[ActiveView].UpdateView();

    }

    // Available sub panel views
    private Dictionary<LayerPanelView, LayerSubPanel> availablePanels;

    //private int activeLayer = -1;


    // Start is called before the first frame update
    void Awake()
    {
        availablePanels = new Dictionary<LayerPanelView, LayerSubPanel>()
        {
            { LayerPanelView.Bases,  GetComponentInChildren<LayerBasesPanel>() },
            { LayerPanelView.Stack,  GetComponentInChildren<LayerStackPanel>() },
            { LayerPanelView.Layer,  GetComponentInChildren<LayerDetailsPanel>() },
        };
        
        foreach(LayerSubPanel panel in availablePanels.Values)
        {
            panel.PanelManager = this;
            // Hide
            panel.gameObject.SetActive(false);
        }

        // Initialize view
        ActiveView = LayerPanelView.Bases;

        LayerManager.OnLayerUpdate += UpdateActivePanel;

    }

    public void UpdateActivePanel(Layer layer)
    {
        availablePanels[ActiveView].UpdateView();
    }

    public void SwitchToStackView(int baseUID)
    {
        ActiveView = LayerPanelView.Stack;
        availablePanels[LayerPanelView.Stack].ElementUID = baseUID;
    }

    public void SwitchToDetailView(int clippedUID)
    {
        SetActiveLayer(clippedUID);
        ActiveView = LayerPanelView.Layer;
        availablePanels[LayerPanelView.Layer].ElementUID = clippedUID;
        //Debug.Log("display detail view for layer: " + clippedUID);
    }

    public void SetActiveLayer(int layerUID)
    {
        layerManager.SetActive(layerUID);
    }

    public void SetHoveredLayer(int layerUID)
    {
        layerManager.SetHovered(layerUID);
    }

    public void ClearHoveredLayer(int layerUID)
    {
        layerManager.ClearHovered(layerUID);
    }

    public void ToggleVisibility(int layerUID)
    {
        layerManager.ToggleVisibility(layerUID);
    }

    public void HandleBackNav()
    {
        // Go back one level
        ActiveView = (LayerPanelView)((int)Mathf.Max(0, ((int)ActiveView) - 1));
    }
}

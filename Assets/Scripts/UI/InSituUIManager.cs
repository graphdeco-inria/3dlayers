using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InSituUIManager : MonoBehaviour
{

    private LayerManager layerManager;

    private GameObject UIElementsContainer;

    private Dictionary<int, LayerStackGizmo> gizmosPerStack;

    // Use this for initialization
    void Awake()
    {
        layerManager = GetComponent<LayerManager>();

        UIElementsContainer = new GameObject("UI Elements Container");
        UIElementsContainer.transform.parent = transform;
        UIElementsContainer.transform.localPosition = Vector3.zero;
        UIElementsContainer.transform.localRotation = Quaternion.identity;

        LayerManager.OnLayerUpdate += HandleLayerUpdate;

        gizmosPerStack = new Dictionary<int, LayerStackGizmo>();
    }


    private void OnDestroy()
    {
        LayerManager.OnLayerUpdate -= HandleLayerUpdate;
    }


    private void HandleLayerUpdate(Layer obj)
    {
        int layerUID = obj.UID;
        if (obj as BaseLayer != null)
        {
            BaseLayer l = (BaseLayer)obj;
            if (!gizmosPerStack.ContainsKey(layerUID))
            {
                // Create gizmo for this stack
                GameObject layerGizmoObj = Instantiate(Resources.Load<GameObject>("Layer Stack Gizmo"), UIElementsContainer.transform);
                //layerGizmoObj.transform.parent = UIElementsContainer.transform;
                LayerStackGizmo newLayerGizmo = layerGizmoObj.GetComponent<LayerStackGizmo>();
                newLayerGizmo.Init(this, l);
                gizmosPerStack[layerUID] = newLayerGizmo;
            }

            LayerStackGizmo layerGizmo = gizmosPerStack[layerUID];
            // Update all properties of this gizmo
            // - active?
            layerGizmo.SetSelected(layerManager.IsActive(obj));

            // - position?
            layerGizmo.transform.localPosition = UIElementsContainer.transform.InverseTransformPoint(l.GetGizmoPosition());
            //Debug.Log(l.GetGizmoPosition());
            layerGizmo.transform.localRotation = Quaternion.identity;
        }
        else
        {
            ClippedLayer l = (ClippedLayer)obj;
            bool isActive = layerManager.IsActive(obj);
            int baseLayerUID = l.clippingBaseUID;
            if (gizmosPerStack.ContainsKey(baseLayerUID))
            {
                gizmosPerStack[baseLayerUID].UpdateClippedLayer(l, isSelected: isActive, order: layerManager.GetIndexInStack(l));

                // TODO: a more consistent check would be to check at the layerManager level whether the ActiveLayer is in the same stack
                //gizmosPerStack[baseLayerUID].ShowPanel(isActive);
            }
            else
            {
                Debug.LogError("Cannot update clipped layer UI because the gizmo for the whole stack is missing.");
            }
        }
        return;
    }

    public void SetActiveLayer(int layerUID)
    {
        layerManager.SetActive(layerUID);
    }

    public void ToggleVisibility(int layerUID)
    {
        layerManager.ToggleVisibility(layerUID);
    }

    public void CreateLayer(int baseUID)
    {
        layerManager.CreateClippedLayer(baseUID);
    }
}
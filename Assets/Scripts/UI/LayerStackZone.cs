using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LayerStackZone : MonoBehaviour
{
    private LayerStackGizmo parentGizmo;

    // Start is called before the first frame update
    void Start()
    {
        parentGizmo = GetComponentInParent<LayerStackGizmo>();    
    }

    public void OnCollide()
    {
        parentGizmo.HandleCollide();
    }

    public void OnExit()
    {
        parentGizmo.HandleExit();
    }

    public int GetStackBaseUID()
    {
        return parentGizmo.BaseLayerUID;
    }
}

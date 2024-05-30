using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerMask
{
    public Material BlendingMaterial 
    { 
        get { return _blendingMat; }
    }
    protected Material _blendingMat;

    protected void UpdateMatProp(string name, float value)
    {
        if (_blendingMat)
        {
            _blendingMat.SetFloat(name, value);
        }
    }

    protected void UpdateMatProp(string name, Vector3 value)
    {
        if (_blendingMat)
        {
            _blendingMat.SetVector(name, value);
            //Debug.Log("setting prop " + name + " to " + value);
        }
    }

    public LayerMask(Material mat)
    {
        _blendingMat = mat;
    }
}

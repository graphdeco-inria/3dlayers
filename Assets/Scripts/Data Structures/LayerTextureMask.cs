using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LayerTextureMask : LayerMask
{
    //private Material gradientBlendingMat;

    private Vector3 _scale;
    private float _minOpacity;
    private float _maxOpacity;

    public Vector3 Scale
    {
        get { return _scale; }
        set
        {
            _scale = value;

            UpdateMatProp("_TextureSamplingScale", _scale);
        }
    }

    public float MinOpacity
    {
        get { return _minOpacity; }
        set
        {
            _minOpacity = value;
            UpdateMatProp("_MinOpacity", _minOpacity);
        }
    }

    public float MaxOpacity
    {
        get { return _maxOpacity; }
        set
        {
            _maxOpacity = value;
            UpdateMatProp("_MaxOpacity", _maxOpacity);
        }
    }

    // Start is called before the first frame update
    public LayerTextureMask(Material mat, Vector3 scale, float min, float max) : base(mat)
    {
        this.Scale = scale;
        this.MinOpacity = min;
        this.MaxOpacity = max;
    }

}

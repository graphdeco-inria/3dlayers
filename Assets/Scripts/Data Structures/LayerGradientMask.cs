using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GradientMaskType
{
    Linear, Sphere
}

public class LayerGradientMask : LayerMask
{
    //private Material gradientBlendingMat;

    private Vector3 _a;
    private Vector3 _b;

    private float _valueA;
    private float _valueB;

    private GradientMaskType _type;

    public Vector3 A
    {
        get { return _a; }
        set
        {
            _a = value;

            UpdateMatProp("_GradientA", _a);
        }
    }

    public Vector3 B
    {
        get { return _b; }
        set
        {
            _b = value;

            UpdateMatProp("_GradientB", _b);
        }
    }

    public float ValueA
    {
        get { return _valueA; }
        set
        {
            _valueA = value;

            UpdateMatProp("_ValueA", _valueA);
        }
    }

    public float ValueB
    {
        get { return _valueB; }
        set
        {
            _valueB = value;

            UpdateMatProp("_ValueB", _valueB);
        }
    }

    public GradientMaskType Type
    {
        get { return _type; }
        set
        {
            _type = value;

            if (_blendingMat)
            {
                //_blendingMat.SetInt("_GradientType", (int)_type);
                UpdateMatProp("_GradientType", (float)_type);
            }
        }
    }


    // Start is called before the first frame update
    public LayerGradientMask(Material mat, Vector3 A, Vector3 B, GradientMaskType type, float ValueA, float ValueB) : base(mat)
    {

        this.A = A;
        this.B = B;

        this.ValueA = ValueA;
        this.ValueB = ValueB;

        this.Type = type;
    }

    public LayerGradientMask(Material mat, SerializableGradientMask maskData) : this(mat, maskData.A, maskData.B, maskData.type, maskData.valueA, maskData.valueB) { }


}

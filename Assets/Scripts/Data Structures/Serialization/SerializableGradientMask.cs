using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SerializableGradientMask
{
    [SerializeField]
    public SerializableVector3 A;
    [SerializeField]
    public SerializableVector3 B;
    [SerializeField]
    public float valueA;
    [SerializeField]
    public float valueB;
    [SerializeField]
    public GradientMaskType type;

    public SerializableGradientMask(LayerGradientMask mask)
    {
        this.A = mask.A;
        this.B = mask.B;
        this.valueA = mask.ValueA;
        this.valueB = mask.ValueB;
        this.type = mask.Type;
    }

    public static implicit operator SerializableGradientMask(LayerGradientMask rValue)
    {
        return new SerializableGradientMask(rValue);
    }
}

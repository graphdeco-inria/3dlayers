using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public struct SerializableLayer
{
    public int UID;
    public string name;
    public int clippingBaseUID;
    //public bool visible;
    public bool enabled;
    public int[] primitivesUID;

    public LayerParameters parameters;
}

[System.Serializable]
public struct LayerParameters
{
    public bool visible;
    public float opacity;
    public float depthThreshold;
    public ColorBlendMode blendMode;
    //public CompositingMode compositingMode;
    public MaskMode maskMode;
    public SerializableGradientMask gradientMask;
}
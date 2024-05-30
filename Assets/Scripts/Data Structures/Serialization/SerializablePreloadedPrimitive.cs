using System.Collections;
using UnityEngine;

[System.Serializable]
public class SerializablePreloadedPrimitive : SerializablePrimitive
{
    [SerializeField]
    public string sourcePath;

    [SerializeField]
    public string meshName;

    public SerializablePreloadedPrimitive(PreloadedPrimitive s) : base(s)
    {
        type = PrimitiveType.PreloadedMesh;
        this.sourcePath = s.sourcePath;
        this.meshName = s.meshName;
    }
}
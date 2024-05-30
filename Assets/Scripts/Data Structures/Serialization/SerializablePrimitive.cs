using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum PrimitiveType
{
    Stroke,
    Copy,
    PreloadedMesh
}

[System.Serializable]
public class SerializablePrimitive
{
    [SerializeField]
    public PrimitiveType type;
    [SerializeField]
    public int UID;
    [SerializeField]
    public bool active;
    [SerializeField]
    public SerializableVector3 position;
    [SerializeField]
    public SerializableQuaternion rotation;
    [SerializeField]
    public SerializableVector3 scale;
    [SerializeField]
    public SerializableColor color;
    [SerializeField]
    public bool applyColor;

    public SerializablePrimitive(Primitive o)
    {
        active = o.gameObject.activeSelf;
        UID = o.UID;
        position = o.transform.localPosition;
        rotation = o.transform.localRotation;
        scale = o.transform.localScale;
        color = o.PrimitiveColor;
        applyColor = o.ApplyColor;
    }

}

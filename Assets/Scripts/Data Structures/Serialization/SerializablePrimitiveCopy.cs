using System.Collections;
using UnityEngine;

[System.Serializable]
public class SerializablePrimitiveCopy : SerializablePrimitive
{
    [SerializeField]
    public int sourceUID;

    public SerializablePrimitiveCopy(PrimitiveCopy s) : base(s)
    {
        type = PrimitiveType.Copy;
        sourceUID = s.Source.UID;
    }
}
using System.Collections;
using UnityEngine;


public class PrimitiveCopy : Primitive
{

    public Primitive Source { get; private set; }

    public void Create(int UID, Primitive src)
    {
        base.Create(UID);
        if (src is PrimitiveCopy && ((PrimitiveCopy)src).Source is PreloadedPrimitive)
            Source = ((PrimitiveCopy)src).Source;
        else
            Source = src;
        SetMesh();

        // Copy color data if any
        PrimitiveColor = src.PrimitiveColor;
        _applyColor = src.ApplyColor;
        if (src.ApplyColor)
            Recolor(src.PrimitiveColor);


        this.transform.localPosition = src.transform.localPosition;
        this.transform.localRotation = src.transform.localRotation;
    }

    public void Create(SerializablePrimitiveCopy s, Primitive src)
    {
        Source = src;
        SetMesh();

        // If we are meant to NOT apply any global color and the source primitive can have per-vertex colors => apply those
        if (!ApplyColor && src is PreloadedPrimitive)
        {
            Recolor(((PreloadedPrimitive)src).baseVertexColors);
        }

        base.Create(s);
    }

    public override void ResetColor(Color initialColor, bool initialApplyColor)
    {
        //Debug.Log($"reset color for copy {initialColor} {initialApplyColor}");
        if (initialApplyColor)
            Recolor(initialColor);
        else
        {
            if (Source as PreloadedPrimitive != null)
            {
                PreloadedPrimitive src = (PreloadedPrimitive)Source;
                Recolor(src.baseVertexColors);
            }
        }
    }

    private void SetMesh()
    {
        base.UpdateMesh(Source.GetMesh(copy:true));
    }

    public override SerializablePrimitive Serialize()
    {
        return new SerializablePrimitiveCopy(this);
    }
}

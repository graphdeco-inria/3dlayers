

using System;
using System.Runtime.Serialization;
using UnityEngine;

public class SwitchLayerAction : AppAction
{
    private Primitive[] primitives;

    private Layer srcLayer;
    private Layer tgtLayer;


    public SwitchLayerAction(Primitive[] primitives, Layer src, Layer tgt) : base()
    {
        this.primitives = primitives;
        type = AppActionType.SwitchLayer;
        this.srcLayer = src;
        this.tgtLayer = tgt;
    }

    public SwitchLayerAction(SerializableAction actionData, Primitive[] primitives, Layer src, Layer tgt) : base(actionData)
    {
        this.primitives = primitives;
        this.srcLayer = src;
        this.tgtLayer = tgt;
    }

    public override void Undo()
    {
        foreach(Primitive s in primitives)
        {
            srcLayer.Add(s);
        }
    }

    public override void Redo()
    {
        foreach (Primitive s in primitives)
        {
            tgtLayer.Add(s);
        }
    }

    public override SerializableAction Serialize()
    {
        SerializableAction actionData = base.Serialize();
        actionData.objectsUID = new int[primitives.Length];
        for (int i = 0; i < primitives.Length; i++)
        {
            actionData.objectsUID[i] = primitives[i].UID;
        }
        actionData.parameters = new ActionParameters();
        actionData.parameters.Add("SourceLayer", srcLayer.UID);
        actionData.parameters.Add("TargetLayer", tgtLayer.UID);
        return actionData;
    }

    //public override int GetElementUID()
    //{
    //    return deletedStroke.UID;
    //}
}
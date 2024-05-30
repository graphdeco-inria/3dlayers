

using System;
using System.Runtime.Serialization;
using UnityEngine;

public class TransformAction : AppAction, IMergeableAction
{
    private Primitive[] transformed;

    //private Vector3 translation;
    //private Quaternion rotation;

    private GrabInfo grabInfo;

    public TransformAction(Primitive[] primitives, GrabInfo grabInfo) : base()
    {
        transformed = primitives;
        type = AppActionType.Transform;
        this.grabInfo = grabInfo;
    }

    public TransformAction(SerializableAction actionData, Primitive[] primitives) : base(actionData)
    {
        transformed = primitives;
        grabInfo = (GrabInfo)actionData.parameters.TryGet("GrabInfo");
    }

    public override void Undo()
    {
        foreach(Primitive s in transformed)
        {
            grabInfo.InverseTransform(s.transform);
        }
    }

    public override void Redo()
    {
        foreach (Primitive s in transformed)
        {
            grabInfo.ForwardTransform(s.transform);
        }
    }

    public override SerializableAction Serialize()
    {
        SerializableAction actionData = base.Serialize();
        actionData.objectsUID = new int[transformed.Length];
        for (int i = 0; i < transformed.Length; i++)
        {
            actionData.objectsUID[i] = transformed[i].UID;
        }
        actionData.parameters = new ActionParameters();
        actionData.parameters.Add("GrabInfo", grabInfo);
        return actionData;
    }

    // If the transform action is preceded by a duplicate, they can be merged into one action on the stack
    public (bool, AppAction) TryMergeWith(AppAction prevAction)
    {
        if (prevAction as DuplicateAction == null)
            return (false, this);

        DuplicateAction prev = (DuplicateAction)prevAction;
        if (timePerformed - prev.timePerformed > MERGE_ACTION_TIME)
            return (false, this);

        // Do these concern the exact same set of primitives?
        if (!prev.IsOnSameSelection(transformed))
            return (false, this);

        // All primitives match, we merge...
        DuplicateAction mergedAction = prev;

        return (true, mergedAction);
    }

    //public override int GetElementUID()
    //{
    //    return deletedStroke.UID;
    //}
}
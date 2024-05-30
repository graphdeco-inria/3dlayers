

using System;
using System.Runtime.Serialization;
using UnityEngine;

public class DeleteAction : AppAction
{
    private Primitive[] deleted;

    public DeleteAction(Primitive[] strokes) : base()
    {
        deleted = strokes;
        type = AppActionType.Delete;
    }

    public DeleteAction(SerializableAction actionData, Primitive[] strokes) : base(actionData)
    {
        deleted = strokes;
    }

    public override void Undo()
    {
        foreach(Primitive s in deleted)
            s.UnHide();
    }

    public override void Redo()
    {
        foreach (Primitive s in deleted)
            s.Hide();
    }

    public override SerializableAction Serialize()
    {
        SerializableAction actionData = base.Serialize();
        actionData.objectsUID = new int[deleted.Length];
        for (int i = 0; i < deleted.Length; i++)
        {
            actionData.objectsUID[i] = deleted[i].UID;
        }
        return actionData;
    }

    //public override int GetElementUID()
    //{
    //    return deletedStroke.UID;
    //}
}
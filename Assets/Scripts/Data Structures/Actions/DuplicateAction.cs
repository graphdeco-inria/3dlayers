

using System;
using System.Runtime.Serialization;
using UnityEngine;

public class DuplicateAction : AppAction
{
    private Primitive[] duplicates;


    public DuplicateAction(Primitive[] duplicates) : base()
    {
        this.duplicates = duplicates;
        type = AppActionType.Duplicate;
    }

    public DuplicateAction(SerializableAction actionData, Primitive[] primitives) : base(actionData)
    {
        duplicates = primitives;
    }

    public override void Undo()
    {
        foreach(Primitive s in duplicates)
        {
            s.Hide();
        }
    }

    public override void Redo()
    {
        foreach (Primitive s in duplicates)
        {
            s.UnHide();
        }
    }

    public override SerializableAction Serialize()
    {
        SerializableAction actionData = base.Serialize();
        actionData.objectsUID = new int[duplicates.Length];
        for (int i = 0; i < duplicates.Length; i++)
        {
            actionData.objectsUID[i] = duplicates[i].UID;
        }
        return actionData;
    }

    public bool IsOnSameSelection(Primitive[] selection)
    {
        // Do these concern the exact same set of primitives?
        if (selection.Length != duplicates.Length)
            return false;

        for (int i = 0; i < duplicates.Length; i++)
        {
            if (duplicates[i].UID != selection[i].UID)
                return false;
        }

        return true;
    }

    //public override int GetElementUID()
    //{
    //    return deletedStroke.UID;
    //}
}


using System;
using System.Runtime.Serialization;
using UnityEngine;



public class EditLayerAction : LayerAction, IMergeableAction
{

    private Layer editedLayer;
    private LayerParameters paramsBefore;
    private LayerParameters paramsAfter;

    public EditLayerAction(Layer l, LayerParameters paramsBefore, LayerParameters paramsAfter) : base()
    {
        editedLayer = l;
        type = AppActionType.LayerEdit;
        this.paramsBefore = paramsBefore;
        this.paramsAfter = paramsAfter;
    }

    public EditLayerAction(SerializableAction actionData, Layer editedLayer) : base(actionData)
    {
        this.editedLayer = editedLayer;
        //Debug.Log(actionData.parameters.ToString());
        this.paramsBefore = (LayerParameters)actionData.parameters.TryGet("paramsBefore");
        this.paramsAfter = (LayerParameters)actionData.parameters.TryGet("paramsAfter");
    }

    public override void Undo()
    {
        editedLayer.SetParameters(paramsBefore);
    }

    public override void Redo()
    {
        editedLayer.SetParameters(paramsAfter);
    }

    public override SerializableAction Serialize()
    {
        SerializableAction actionData = base.Serialize();
        actionData.objectsUID = new int[] { editedLayer.UID };
        actionData.parameters.Add("paramsBefore", paramsBefore);
        actionData.parameters.Add("paramsAfter", paramsAfter);
        return actionData;
    }

    public override int GetLayerUID()
    {
        return editedLayer.UID;
    }

    public static (bool, EditLayerAction) TryMergeEdits(EditLayerAction prevAction, EditLayerAction currAction)
    {
        if (prevAction.GetLayerUID() != currAction.GetLayerUID())
            return (false, currAction);

        if (currAction.timePerformed  - prevAction.timePerformed < MERGE_ACTION_TIME)
        {
            EditLayerAction mergedAction = new EditLayerAction(prevAction.editedLayer, prevAction.paramsBefore, currAction.paramsAfter);
            return (true, mergedAction);
        }
        else
            return (false, currAction);
    }

    public (bool, AppAction) TryMergeWith(AppAction prevAction)
    {
        if (prevAction as EditLayerAction != null)
        {
            return TryMergeEdits((EditLayerAction)prevAction, this);
        }
        else
            return (false, this);
    }
}
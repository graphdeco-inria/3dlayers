

using System;
using System.Runtime.Serialization;


public class DeleteLayerAction : LayerAction
{
    private Layer deletedLayer;

    public DeleteLayerAction(Layer l) : base()
    {
        deletedLayer = l;
        type = AppActionType.LayerDelete;
    }

    public override void Undo()
    {
        deletedLayer.UnHide();
    }

    public override void Redo()
    {
        deletedLayer.Hide();
    }

    public override SerializableAction Serialize()
    {
        SerializableAction actionData = base.Serialize();
        actionData.objectsUID = new int[] { deletedLayer.UID };
        return actionData;
    }

    public override int GetLayerUID()
    {
        return deletedLayer.UID;
    }
}
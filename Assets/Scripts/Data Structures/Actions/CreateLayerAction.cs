

using System;
using System.Runtime.Serialization;


public class CreateLayerAction : LayerAction
{
    private Layer createdLayer;

    public CreateLayerAction(Layer l) : base()
    {
        createdLayer = l;
        type = AppActionType.LayerCreate;
    }

    public CreateLayerAction(SerializableAction actionData, Layer l) : base(actionData)
    {
        createdLayer = l;
    }

    public override void Undo()
    {
        createdLayer.Hide();
    }

    public override void Redo()
    {
        createdLayer.UnHide();
    }

    public override SerializableAction Serialize()
    {
        SerializableAction actionData = base.Serialize();
        actionData.objectsUID = new int[] { createdLayer.UID };
        return actionData;
    }

    public override int GetLayerUID()
    {
        return createdLayer.UID;
    }
}
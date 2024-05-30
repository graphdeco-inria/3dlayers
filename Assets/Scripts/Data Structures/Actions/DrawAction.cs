using UnityEngine;

public class DrawAction : AppAction
{
    private Stroke createdStroke;
    private Color currentDrawingColor;

    public DrawAction(Stroke s) : base()
    {
        createdStroke = s;
        type = AppActionType.Draw;
        currentDrawingColor = s.PrimitiveColor;
    }

    public DrawAction(SerializableAction actionData, Stroke s) : base(actionData)
    {
        createdStroke = s;
        currentDrawingColor = (SerializableColor)actionData.parameters.TryGet("drawColor");
    }

    public override void Undo()
    {
        createdStroke.Hide();
    }

    public override void Redo()
    {
        createdStroke.UnHide();
    }

    public override SerializableAction Serialize()
    {
        SerializableAction actionData = base.Serialize();
        actionData.objectsUID = new int[] { createdStroke.UID };
        actionData.parameters.Add("drawColor", currentDrawingColor);
        return actionData;
    }

    //public override int GetElementUID()
    //{
    //    return createdStroke.UID;
    //}
}
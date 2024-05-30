using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ReorderStackAction : LayerAction
{
    private int stackBaseUID;
    private List<ClippedLayer> stack;
    private int sourceZIndex;
    private int targetZIndex;

    public ReorderStackAction(int stackBaseUID, List<ClippedLayer> stack, int srcIdx, int tgtIdx) : base()
    {
        this.stackBaseUID = stackBaseUID;
        this.stack = stack;
        sourceZIndex = srcIdx;
        targetZIndex = tgtIdx;
        type = AppActionType.StackReorder;
    }

    public ReorderStackAction(SerializableAction actionData, List<ClippedLayer> stack) : base(actionData)
    {
        this.stack = stack;
        this.sourceZIndex = (int)(float)actionData.parameters.TryGet("sourceZIndex");
        this.targetZIndex = (int)(float)actionData.parameters.TryGet("targetZIndex");
    }

    public override void Redo()
    {
        (stack[sourceZIndex], stack[targetZIndex]) = (stack[targetZIndex], stack[sourceZIndex]);
    }

    public override void Undo()
    {
        Debug.Log("undoing reorder");
        (stack[sourceZIndex], stack[targetZIndex]) = (stack[targetZIndex], stack[sourceZIndex]);
    }
    public override SerializableAction Serialize()
    {
        SerializableAction actionData = base.Serialize();
        actionData.objectsUID = new int[] { stackBaseUID };
        actionData.parameters.Add("sourceZIndex", sourceZIndex);
        actionData.parameters.Add("targetZIndex", targetZIndex);
        return actionData;
    }

    public override int GetLayerUID()
    {
        return stackBaseUID;
    }
}
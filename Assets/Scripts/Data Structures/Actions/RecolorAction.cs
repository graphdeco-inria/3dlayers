

using System;
using System.Runtime.Serialization;
using UnityEngine;

public class RecolorAction : AppAction, IMergeableAction
{
    private Primitive[] primitives;

    // TODO: replace this as it can get costly in memory
    //private Color[][] initialColors; // an array containing one array of colors per primitive, that correspond to the per-vertex colors (ugly but necessary if we want to support quill primitives with per-vertex colors)
    private Color[] initialColors;
    private bool[] initialApplyColors;
    private Color newColor;


    //public RecolorAction(Primitive[] primitives, Color[][] initialColors, Color targetColor) : base()
    public RecolorAction(Primitive[] primitives, Color targetColor, Color[] initialColors, bool[] initialApplyColors) : base()
    {
        this.primitives = primitives;
        type = AppActionType.Recolor;
        this.newColor = targetColor;

        this.initialApplyColors = initialApplyColors;
        this.initialColors = initialColors;

        //this.initialColors = initialColors;
    }

    public RecolorAction(SerializableAction actionData, Primitive[] recoloredPrimitives) : base(actionData)
    {
        this.primitives = recoloredPrimitives;
        this.newColor = (SerializableColor)actionData.parameters.TryGet("Color");
        SerializableColor[] serializedInitcolors = (SerializableColor[])actionData.parameters.TryGet("Initial Colors");
        this.initialColors = new Color[serializedInitcolors.Length];
        for (int i = 0; i < serializedInitcolors.Length; i++)
        {
            this.initialColors[i] = serializedInitcolors[i];
        }
        this.initialApplyColors = (bool[])actionData.parameters.TryGet("Initial Apply Colors");
    }

    public override void Undo()
    {
        for (int i = 0; i < primitives.Length; i++)
        {
            primitives[i].ResetColor(initialColors[i], initialApplyColors[i]);
        }
    }

    public override void Redo()
    {
        foreach (Primitive s in primitives)
        {
            s.Recolor(newColor);
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
        actionData.parameters.Add("Color", newColor);
        actionData.parameters.Add("Initial Colors", initialColors);
        actionData.parameters.Add("Initial Apply Colors", initialApplyColors);
        return actionData;
    }

    public (bool, AppAction) TryMergeWith(AppAction prevAction)
    {
        if (prevAction as RecolorAction == null)
            return (false, this);

        
        RecolorAction prev = (RecolorAction)prevAction;
        if (timePerformed - prev.timePerformed > MERGE_ACTION_TIME)
            return (false, this);

        // Do these concern the exact same set of primitives?
        if (prev.primitives.Length != primitives.Length)
            return (false, this);

        for (int i = 0; i < primitives.Length; i++)
        {
            if (primitives[i].UID != prev.primitives[i].UID)
                return (false, this);
        }

        // All primitives match, we merge...
        //Debug.Log("merging recolor actions");
        RecolorAction mergedAction = new RecolorAction(primitives, this.newColor, prev.initialColors, prev.initialApplyColors);

        return (true, mergedAction);
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public enum AppActionType
{
    Draw,
    Delete,
    Transform,
    Duplicate,
    Recolor,
    SwitchLayer,
    LayerCreate,
    LayerEdit,
    LayerDelete,
    StackReorder
}

[Serializable]
public class ActionParameters : ISerializable
{
    private Dictionary<string, object> values;

    public ActionParameters()
    {
        values = new Dictionary<string, object>();
    }

    public ActionParameters(SerializationInfo info, StreamingContext context)
    {
        values = new Dictionary<string, object>();
        string[] keys = info.GetValue("keys", typeof(string[])) as string[];
        //Debug.Log(keys);

        foreach (string key in keys)
        {
            values.Add(key, info.GetValue(key, typeof(object)));
        }
    }

    public void Add(string key, float value)
    {
        values.Add(key, value);
    }

    public void Add(string key, UnityEngine.Color c)
    {
        values.Add(key, (SerializableColor)c);
    }

    public void Add(string key, UnityEngine.Color[] colors)
    {
        SerializableColor[] cs = new SerializableColor[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            cs[i] = colors[i];
        }
        values.Add(key, cs);
    }

    public void Add(string key, object serializable)
    {
        values.Add(key, serializable);
    }

    public object TryGet(string key)
    {
        if (values.ContainsKey(key))
            return values[key];
        else
        {
            Debug.LogError("dict=" + ToString());
            throw new FieldAccessException("ActionParameters does not contain key: " + key);
        }
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        string[] keys = new string[values.Count];
        int i = 0;
        foreach(var v in values)
        {
            info.AddValue(v.Key, v.Value);
            keys[i] = v.Key;
            i++;
        }
        info.AddValue("keys", keys);
    }

    public override string ToString()
    {
        string s = "";

        foreach(string key in values.Keys)
        {
            s += $"k = {key}, v = {values[key]} ;";
        }

        return s;
    }
}

[Serializable]
public struct SerializableAction
{
    public AppActionType type;
    public int[] objectsUID;
    public float performedTime;
    public ActionParameters parameters;
}

public static class ActionDeserializer
{
    private static bool TryGetStroke(SerializableAction actionData, Dictionary<int, Primitive> primitives, out Stroke s)
    {
        if (actionData.objectsUID.Length == 1 && primitives.ContainsKey(actionData.objectsUID[0]) && primitives[actionData.objectsUID[0]] as Stroke != null)
        {
            s = (Stroke)primitives[actionData.objectsUID[0]];
            return true;
        }
        else
        {
            s = null;
            return false;
        }
    }

    private static bool TryGetLayer(SerializableAction actionData, Dictionary<int, Layer> layers, out Layer l)
    {
        if (actionData.objectsUID.Length == 1 && layers.ContainsKey(actionData.objectsUID[0]))
        {
            l = layers[actionData.objectsUID[0]];
            return true;
        }
        else
        {
            l = null;
            return false;
        }
    }

    private static bool TryGetPrimitives(SerializableAction actionData, Dictionary<int, Primitive> primitives, out Primitive[] actionPrimitives)
    {
        actionPrimitives = new Primitive[actionData.objectsUID.Length];

        for (int i = 0; i < actionData.objectsUID.Length; i++)
        {
            if (!primitives.ContainsKey(actionData.objectsUID[i]))
                return false;
            actionPrimitives[i] = primitives[actionData.objectsUID[i]];
        }

        return true;
    }

    private static bool TryGetStack(SerializableAction actionData, Dictionary<int, List<ClippedLayer>> stacks, out List<ClippedLayer> stack)
    {
        if (actionData.objectsUID.Length == 1 && stacks.ContainsKey(actionData.objectsUID[0]))
        {
            stack = stacks[actionData.objectsUID[0]];
            return true;
        }
        else
        {
            stack = null;
            return false;
        }
    }

    public static AppAction[] Deserialize(SerializableAction[] actionLog, Dictionary<int, Layer> layers, Dictionary<int, Primitive> primitives, Dictionary<int, List<ClippedLayer>> stacks)
    {
        AppAction[] actions = new AppAction[actionLog.Length];

        for (int i = 0; i < actionLog.Length; i++)
        {
            Debug.Log("Deserializing action at t = " + actionLog[i].performedTime);
            actions[i] = Deserialize(actionLog[i], layers, primitives, stacks);
        }

        return actions;
    }

    public static AppAction Deserialize(SerializableAction actionData, Dictionary<int, Layer> layers, Dictionary<int, Primitive> primitives, Dictionary<int, List<ClippedLayer>> stacks)
    {
        AppAction action = null;

        switch(actionData.type)
        {
            case AppActionType.Draw:
                Stroke s;
                if (TryGetStroke(actionData, primitives, out s))
                {
                    action = new DrawAction(actionData, s);
                }
                else
                {
                    throw new Exception("Cannot load log action (type=draw).");
                }
                break;
            case AppActionType.Delete:
                Primitive[] deletedPrimitives;
                if (TryGetPrimitives(actionData, primitives, out deletedPrimitives))
                {
                    action = new DeleteAction(actionData, deletedPrimitives);
                }
                else
                {
                    throw new Exception("Cannot load log action (type=delete).");
                }
                break;
            case AppActionType.Duplicate:
                Primitive[] duplicatedPrimitives;
                if (TryGetPrimitives(actionData, primitives, out duplicatedPrimitives))
                {
                    action = new DuplicateAction(actionData, duplicatedPrimitives);
                }
                else
                {
                    throw new Exception("Cannot load log action (type=duplicate).");
                }
                break;
            case AppActionType.Recolor:
                Primitive[] recoloredPrimitives;
                if (TryGetPrimitives(actionData, primitives, out recoloredPrimitives))
                {
                    action = new RecolorAction(actionData, recoloredPrimitives);
                }
                else
                {
                    throw new Exception("Cannot load log action (type=recolor).");
                }
                break;
            case AppActionType.Transform:
                Primitive[] transformedPrimitives;
                if (TryGetPrimitives(actionData, primitives, out transformedPrimitives))
                {
                    action = new TransformAction(actionData, transformedPrimitives);
                }
                else
                {
                    throw new Exception("Cannot load log action (type=transform).");
                }
                break;
            case AppActionType.SwitchLayer:
                Primitive[] transportedPrimitives;
                if (TryGetPrimitives(actionData, primitives, out transportedPrimitives))
                {
                    int srcLayerUID = (int)((float)actionData.parameters.TryGet("SourceLayer"));
                    int tgtLayerUID = (int)((float)actionData.parameters.TryGet("TargetLayer"));
                    if (layers.ContainsKey(srcLayerUID) && layers.ContainsKey(tgtLayerUID))
                    {
                        Layer srcLayer = layers[srcLayerUID];
                        Layer tgtLayer = layers[tgtLayerUID];
                        action = new SwitchLayerAction(actionData, transportedPrimitives, srcLayer, tgtLayer);
                    }
                }
                else
                {
                    throw new Exception("Cannot load log action (type=switch).");
                }
                break;
            case AppActionType.LayerCreate:
                Layer createdLayer;
                if (TryGetLayer(actionData, layers, out createdLayer))
                {
                    action = new CreateLayerAction(actionData, createdLayer);
                }
                else
                {
                    throw new Exception("Cannot load log action (type=layercreate).");
                }
                break;
            case AppActionType.LayerEdit:
                Layer editedLayer;
                if (TryGetLayer(actionData, layers, out editedLayer))
                {
                    action = new EditLayerAction(actionData, editedLayer);
                }
                else
                {
                    throw new Exception("Cannot load log action (type=layercreate).");
                }
                break;
            //case AppActionType.LayerDelete:
            //    Layer deletedLayer;
            //    if (TryGetLayer(actionData, layers, out deletedLayer))
            //    {
            //        action = new DeleteLayerAction(actionData, (ClippedLayer)deletedLayer);
            //    }
            //    else
            //    {
            //        throw new Exception("Cannot load log action (type=layercreate).");
            //    }
            //    break;
            case AppActionType.StackReorder:
                List<ClippedLayer> stack;
                if (TryGetStack(actionData, stacks, out stack))
                    action = new ReorderStackAction(actionData, stack);
                else
                    throw new Exception("Cannot load log action (type=stackreorder).");
                break;
        }

        return action;
    }


}
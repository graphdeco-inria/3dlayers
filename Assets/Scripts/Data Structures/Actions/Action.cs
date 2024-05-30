using System.Runtime.Serialization;
using UnityEngine;

public abstract class LayerAction : AppAction
{
    public abstract int GetLayerUID();

    public LayerAction() : base() { }

    public LayerAction(SerializableAction actionData) : base(actionData) { }
}

public interface IMergeableAction
{
    public (bool, AppAction) TryMergeWith(AppAction prevAction);
}

public abstract class AppAction
{

    protected const float MERGE_ACTION_TIME = 1.0f;

    public float timePerformed { get; private set; }
    public AppActionType type { get; protected set; }

    public AppAction()
    {
        timePerformed = Time.time;
    }

    public AppAction(SerializableAction actionData)
    {
        type = actionData.type;
        timePerformed = actionData.performedTime;
    }

    public bool ImpactsLayerUI()
    {
        return type == AppActionType.LayerCreate || type == AppActionType.LayerDelete || type == AppActionType.LayerEdit;
    }

    //public abstract int GetElementUID();

    public abstract void Undo();

    public abstract void Redo();

    //public void GetObjectData(SerializationInfo info, StreamingContext context)
    //{
    //    info.AddValue("performedTime", timePerformed);
    //    info.AddValue("type", type);
    //}

    public virtual SerializableAction Serialize()
    {
        SerializableAction actionData = new SerializableAction();
        actionData.performedTime = timePerformed;
        actionData.type = type;
        actionData.parameters = new ActionParameters();
        return actionData;
    }

    public override string ToString()
    {
        return $"{type}-{timePerformed}s";
    }
}
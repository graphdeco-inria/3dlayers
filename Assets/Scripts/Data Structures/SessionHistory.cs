using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;


[Serializable]
public struct SerializableSessionState
{
    public SerializablePrimitive[] primitives;
    public SerializableLayer[] layers;
    public SerializableStack[] layerStacks;

    public SerializableAction[] actionHistory;
}


public class SessionHistory
{
    private static SessionHistory instance = null;

    private SessionHistory()
    {
        actionsDone = new Stack<AppAction>();
        actionsUndone = new Stack<AppAction>();
    }

    public static SessionHistory Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new SessionHistory();
            }
            return instance;
        }
    }

    private LayerManager layerManager;
    private PaintCanvas paintCanvas;

    private Stack<AppAction> actionsDone;
    private Stack<AppAction> actionsUndone;

    public string PrevAction
    {
        get
        {
            AppAction actionToPrint = actionsDone.Peek();
            return actionToPrint.ToString();
        }
    }

    public string NextAction
    {
        get
        {
            AppAction actionToPrint = actionsUndone.Peek();
            return actionToPrint.ToString();
        }
    }

    public int ActionsCount
    {
        get
        {
            return actionsDone.Count + actionsUndone.Count;
        }
    }

    public bool CanUndo
    {
        get
        {
            return actionsDone.Count > 1;
        }
    }


    public bool CanRedo
    {
        get
        {
            return actionsUndone.Count > 1;
        }
    }

    public void SetManager(LayerManager m)
    {
        layerManager = m;
    }

    public void SetPaintCanvas(PaintCanvas p)
    {
        paintCanvas = p;
    }

    public void RegisterAction(AppAction action)
    {
        //if (action.type == AppActionType.LayerEdit && actionsDone.Count > 1 && actionsDone.Peek().type == AppActionType.LayerEdit)
        if (actionsDone.Count > 1 && action is IMergeableAction)
            {
            IMergeableAction currAction = (IMergeableAction)action;
            //(bool canMerge, EditLayerAction merged) = EditLayerAction.TryMergeEdits((EditLayerAction)actionsDone.Peek(), (EditLayerAction)action);
            (bool canMerge, AppAction merged) = currAction.TryMergeWith(actionsDone.Peek());
            if (canMerge)
            {
                // Replace last action by the merged one
                actionsDone.Pop();
                action = merged;
            }
        }
        actionsDone.Push(action);

        // When a new action is performed, we have to clear the undo stack
        actionsUndone.Clear();
    }

    public void ResetSession()
    {
        while(actionsDone.Count > 0)
        {
            UndoAction();
        }
    }

    public bool UndoAction()
    {
        if (actionsDone.Count > 0)
        {
            AppAction actionToUndo = actionsDone.Pop();
            actionToUndo.Undo();
            actionsUndone.Push(actionToUndo);

            // Should update UI views?
            if (actionToUndo is LayerAction)
            {
                layerManager.TriggerLayerUpdateEvent(((LayerAction)actionToUndo).GetLayerUID());
            }
            return true;
        }
        return false;

    }

    public bool RedoAction()
    {
        if (actionsUndone.Count > 0)
        {
            AppAction actionToRedo = actionsUndone.Pop();
            actionToRedo.Redo();
            actionsDone.Push(actionToRedo);

            // Should update UI views?
            if (actionToRedo is LayerAction)
            {
                layerManager.TriggerLayerUpdateEvent(((LayerAction)actionToRedo).GetLayerUID());
            }
            return true;
        }
        return false;
    }

    public void GoToAction(int idx)
    {
        // Undo all
        ResetSession();

        // Do "idx" actions
        for (int i = 0; i < idx; i++)
        {
            RedoAction();
        }
    }

    public void PrintPrevAction()
    {
        if (actionsDone.Count > 0)
        {
            AppAction actionToPrint = actionsDone.Peek();
            Debug.Log($"Action {actionsDone.Count}: " + actionToPrint.ToString());
        }
    }

    public void PrintNextAction()
    {
        if (actionsUndone.Count > 0)
        {
            AppAction actionToPrint = actionsUndone.Peek();
            Debug.Log($"Action {actionsDone.Count}: " + actionToPrint.ToString());
        }
    }


    public void Write()
    {
        string path = Path.Combine(Application.dataPath, "SessionData~");

        // If in latest mode: create a sub-directory for the scene we're working on now
        if (layerManager.fileType == InputFileType.Latest || layerManager.fileType == InputFileType.FBX)
        {
            path = Path.Combine(path, layerManager.InputFileName);
        }

        // Try to create the directory
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
        }

        // File name
        string fileName = (DateTime.Now).ToString("yyyy-MM-dd-HH-mm-ss") + "_session";

        // Export current state
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        FileStream fs = new System.IO.FileStream(Path.Combine(path, fileName + ".dat"), System.IO.FileMode.Create);

        SerializableSessionState state = new SerializableSessionState();

        // Primitives (sorted by UID => very important to make sure that copied primitives will be instantiated after their source)
        Primitive[] currentPrimitives = layerManager.GetAllPrimitives();
        SerializablePrimitive[] serializedPrimitives = new SerializablePrimitive[currentPrimitives.Length];
        for (int i = 0; i < currentPrimitives.Length; i++)
        {
            serializedPrimitives[i] = currentPrimitives[i].Serialize();
        }

        state.primitives = serializedPrimitives;

        // Layers
        state.layerStacks = layerManager.SerializeStacks();
        state.layers = layerManager.SerializeLayers();

        // Actions
        state.actionHistory = SerializeActions();

        // Write to file stream
        bf.Serialize(fs, state);
        fs.Close();

        // Debug: write also a json
        File.WriteAllText(Path.Combine(path, fileName + ".json"), JsonConvert.SerializeObject(state, new JsonSerializerSettings
        {
            Culture = new System.Globalization.CultureInfo("en-US")
        }));

        Debug.Log("wrote files to " + Application.dataPath);

    }

    public void Load(string filePath)
    {
        // Load scene from exported file

        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        System.IO.FileStream fs = new FileStream(filePath, FileMode.Open);
        SerializableSessionState state = (SerializableSessionState)bf.Deserialize(fs);
        fs.Close();
        //Debug.Log(filePath);
        //SerializableSessionState state = JsonConvert.DeserializeObject<SerializableSessionState>(File.ReadAllText(filePath));
        //Debug.Log(state);

        Dictionary<int, Layer> layers = layerManager.LoadLayers(state);
        Dictionary<int, Primitive> primitives = paintCanvas.LoadPrimitives(state);

        Dictionary<int, List<ClippedLayer>> stacks = layerManager.GetStacks();

        // Build action history
        AppAction[] actions = ActionDeserializer.Deserialize(state.actionHistory, layers, primitives, stacks);

        for(int i = actions.Length - 1; i >=0; i--)
        {
            AppAction action = actions[i];
            //Debug.Log(action.timePerformed);
            //Debug.Log(action.type);
            actionsDone.Push(action);
        }
    }

    private SerializableAction[] SerializeActions()
    {
        SerializableAction[] actionsData = new SerializableAction[actionsDone.Count];
        AppAction[] actionHistory = actionsDone.ToArray();
        for (int i = 0; i < actionsDone.Count; i++)
        {
            actionsData[i] = actionHistory[i].Serialize();
        }

        return actionsData;
    }
}

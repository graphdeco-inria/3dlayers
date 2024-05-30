using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(LayerManager))]
public class SessionVisualizer : MonoBehaviour
{

    public int ActionIndex;

    public bool RenderFrames = false;
    public bool RenderWithLabels = false;
    public string FolderName = "path";

    private LayerManager manager;
    private LayerRenderer layerRenderer;

    private bool playing = false;
    private int frameIdx = 0;



    // Start is called before the first frame update
    void Start()
    {
        manager = GetComponent<LayerManager>();
        layerRenderer = GetComponent<LayerRenderer>();

        //// Load session log of actions
        //if (manager.fileType == InputFileType.Session)
        //{
        //    //string filePath = Path.Combine(Application.dataPath, "SessionData~", $"{manager.InputFileName}.dat");
        //    //System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        //    //System.IO.FileStream fs = new FileStream(filePath, FileMode.Open);
        //    //SerializableSessionState state = (SerializableSessionState)bf.Deserialize(fs);

        //    //actionsLog = state.actionHistory;

        //}
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Debug.Log("prev action");
            SessionHistory.Instance.PrintPrevAction();
            SessionHistory.Instance.UndoAction();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Debug.Log($"next action: {SessionHistory.Instance.NextAction}");
            SessionHistory.Instance.PrintNextAction();
            SessionHistory.Instance.RedoAction();
        }

        playing = playing && SessionHistory.Instance.CanRedo;

        if (playing)
        {
            //SessionHistory.Instance.PrintNextAction();
            string nextActionName = SessionHistory.Instance.NextAction;
            Debug.Log("playing next action: " + nextActionName);
            SessionHistory.Instance.RedoAction();

            if (RenderFrames && layerRenderer)
            {
                if (RenderWithLabels)
                    layerRenderer.RenderAnimationFrame(FolderName, $"{frameIdx.ToString("D4")}-{nextActionName}");
                else
                    layerRenderer.RenderAnimationFrame(FolderName, frameIdx);
            }
            frameIdx++;
        }
    }

    public void ResetSession()
    {
        SessionHistory.Instance.ResetSession();
    }

    public void PlaySession()
    {
        ResetSession();
        frameIdx = 0;
        playing = true;
    }

    public void GoToSelectedAction()
    {
        if (ActionIndex >= 0 && ActionIndex < SessionHistory.Instance.ActionsCount)
            SessionHistory.Instance.GoToAction(ActionIndex);
    }
}

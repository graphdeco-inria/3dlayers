using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Eraser : MonoBehaviour
{

    [Header("XR Input Actions")]
    public InputActionProperty eraseAction;

    public Selector selector;

    //public static event Action OnEraseSelection;

    // Start is called before the first frame update
    private void Start()
    {
        eraseAction.action.started += TriggerSelectionDelete;
    }

    private void OnDisable()
    {
        eraseAction.action.started -= TriggerSelectionDelete;
    }

    private void TriggerSelectionDelete(InputAction.CallbackContext ctx)
    {
        //OnEraseSelection?.Invoke();
        Primitive[] selection = selector.CurrentSelection();

        foreach(Primitive s in selection)
        {
            s.Hide();
        }

        // TODO: put back this thing once we can serialize arbitrary primitives
        if (selection.Length > 0)
        {
            SessionHistory.Instance.RegisterAction(
                new DeleteAction(selection)
            );
        }

    }

    // Update is called once per frame
    //void Update()
    //{

    //}

    //private void HandleStrokeCollide(Stroke s)
    //{
    //    if (doingAction)
    //    {
    //        paintCanvas.TryDeleteStroke(s);
    //    }
    //}

    //private void StartErasing()
    //{
    //    if (manager.CanUseTool(this))
    //    {
    //        erasing = true;
    //        manager.MarkBusy();
    //    }
    //}

    //private void StopErasing()
    //{
    //    erasing = false;
    //    manager.MarkIdle();
    //}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class UIPointer : MonoBehaviour, ITool
{

    //public Material NormalControllerMat;
    //public Material UIControllerMat;
    public InputActionProperty rightHandPosition;
    public InputActionProperty rightGrabAction;

    //private MeshRenderer controllerRenderer;
    private ToolsManager toolsManager;

    private ActionBasedController controller;

    private DragZone currentDragZone;
    private bool isDragging = false;

    public bool Busy
    {
        get { return isDragging; }
    }

    private void Start()
    {
        controller = GetComponentInParent<ActionBasedController>();
        rightGrabAction.action.started += ctx => HandleUIGrabStart();
        rightGrabAction.action.canceled += ctx => HandleUIGrabEnd();
    }

    private void Update()
    {
        if (isDragging && currentDragZone != null)
        {
            currentDragZone.DragUpdate(rightHandPosition.action.ReadValue<Vector3>());
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PokeZone"))
        {
            if (toolsManager.CanUseTool())
            {
                //toolsManager.MarkBusy();
                toolsManager.ToggleUIMode(true);
            }
        }

        if (other.CompareTag("DragZone"))
        {
            currentDragZone = other.GetComponent<DragZone>();
            //Debug.Log("can use drag ui? " + toolsManager.CanUseUITool());


        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.Log("exited " + other.name);
        if (other.CompareTag("PokeZone"))
        {
            toolsManager.MarkIdle();
            toolsManager.ToggleUIMode(false);
        }

        //if (!isDragging && currentDragZone != null && other.GetComponent<DragZone>() == currentDragZone)
        //    currentDragZone = null;
    }

    private void HandleUIGrabStart()
    {
        //Debug.Log("dragging");
        if (currentDragZone == null)
            return;

        if (toolsManager.CanUseUITool() && currentDragZone.Draggable())
        {
            toolsManager.MarkBusy();
            //toolsManager.ToggleUIMode(true);
            //Debug.Log("entered drag zone");
            //Debug.Log(currentDragZone);
            currentDragZone.DragStart(rightHandPosition.action.ReadValue<Vector3>());
            isDragging = true;
        }


    }

    private void HandleUIGrabEnd()
    {
        isDragging = false;
        toolsManager.MarkIdle();
    }

    public void OnPoke()
    {
        //Debug.Log("send haptic");
        controller.SendHapticImpulse(0.5f, 0.1f);
    }


    public void SetManager(ToolsManager manager)
    {
        this.toolsManager = manager;
    }

    public Tool GetToolType()
    {
        return Tool.UIPointer;
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ToolsManager : MonoBehaviour
{
    //public UIPointer UITool;

    //public static event Action OnUIZoneEnter;
    //public static event Action OnUIZoneExit;

    public InputActionProperty toolSwitchAction;

    public ToolSettingsPanel toolSettingsPanel;

    public static event Action<Tool> OnToolChange;
    public static event Action<float> OnToolRadiusChange;

    //private ITool activeTool;
    //private ITool UISelectedTool;

    private Dictionary<Tool, SphereTool> availableTools;

    private bool toolIsInUse = false;
    private bool UIMode = false;
    private SphereTool SelectedTriggerTool;

    private SphereTool secondaryTriggerTool;

    private void Start()
    {
        availableTools = new Dictionary<Tool, SphereTool>();
        foreach(var tool in GetComponentsInChildren<ITool>())
        {
            tool.SetManager(this);

            Debug.Log(tool);
            if (tool as SphereTool != null)
                availableTools.Add(tool.GetToolType(), (SphereTool)tool);
        }

        secondaryTriggerTool = availableTools[Tool.Selector];

        UITriggerToolSwitch((int)Tool.Painter);

        toolSwitchAction.action.started += HandleToolSwitch;
    }

    private void OnDisable()
    {
        toolSwitchAction.action.started -= HandleToolSwitch;
    }

    // Switch tool
    public void UITriggerToolSwitch(int toolID)
    {
        //toolIsInUse = true;
        foreach (SphereTool tool in GetComponentsInChildren<SphereTool>())
        {
            if (tool.GetToolType() == (Tool)toolID)
            {
                SelectedTriggerTool = tool;
                // Set secondary
                if (SelectedTriggerTool.GetToolType() == Tool.Selector)
                    secondaryTriggerTool = availableTools[Tool.Painter];
                else
                    secondaryTriggerTool = availableTools[Tool.Selector];

                OnToolRadiusChange?.Invoke(tool.SphereToolRadius);
                if (!UIMode)
                    OnToolChange?.Invoke(tool.GetToolType());
                toolSettingsPanel.SwitchTool(tool.GetToolType());
                return;
            }
        }
        Debug.Log($"Tool {(Tool)toolID} is not available for ToolManager.");
    }

    public void SwitchOffTool(int toolID)
    {
        if (SelectedTriggerTool && SelectedTriggerTool.GetToolType() == (Tool)toolID)
        {
            UITriggerToolSwitch((int)Tool.Selector); // Switch back to selector by default
        }
    }

    public void ToggleUIMode(bool inUIMode)
    {
        UIMode = inUIMode;
        if (inUIMode)
            OnToolChange?.Invoke(Tool.UIPointer);
        else
            OnToolChange?.Invoke(SelectedTriggerTool.GetToolType());
    }

    public bool CanUseTool()
    {
        return !toolIsInUse && !UIMode;
    }

    public bool CanUseUITool()
    {
        return !toolIsInUse && UIMode;
    }

    public bool CanUseTool(SphereTool tool)
    {
        return !UIMode && !toolIsInUse && SelectedTriggerTool == tool;
    }

    public void ChangeToolRadius(float newRadius)
    {
        OnToolRadiusChange?.Invoke(newRadius);
    }

    public void MarkBusy()
    {
        toolIsInUse = true;
    }

    public void MarkIdle()
    {
        toolIsInUse = false;
    }

    //private void SwitchToTool(Tool toolType)
    //{

    //}


    private void HandleToolSwitch(InputAction.CallbackContext ctx)
    {
        SphereTool temp = secondaryTriggerTool;
        secondaryTriggerTool = SelectedTriggerTool;
        SelectedTriggerTool = temp;
        OnToolRadiusChange?.Invoke(SelectedTriggerTool.SphereToolRadius);
        OnToolChange?.Invoke(SelectedTriggerTool.GetToolType());

    }

}

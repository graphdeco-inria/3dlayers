using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolsPanel : MonoBehaviour
{
    public PokableButton[] toolButtons;

    // Start is called before the first frame update
    void Start()
    {
        ToolsManager.OnToolChange += HandleToolChange;
        //toolButtons = GetComponentsInChildren<PokableButton>();
        //toolButtons = new Dictionary<Tool, PokableButton>();
    }

    private void HandleToolChange(Tool activeTool)
    {
        if (activeTool == Tool.UIPointer)
            return;

        //Debug.Log("switch active tool (tools panel) " + activeTool);
        // Reset all buttons
        foreach (var button in toolButtons)
        {
            if (button != null)
            {
                button.SetColor(Color.white);
            }
        }
        if (toolButtons[(int)activeTool] != null)
        {
            toolButtons[(int)activeTool].SetColor(UIConstants.ACTIVE_COLOR);
        }
    }
}

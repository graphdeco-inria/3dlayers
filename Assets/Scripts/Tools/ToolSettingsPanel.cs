using System.Collections;
using UnityEngine;


public class ToolSettingsPanel : MonoBehaviour
{

    public GameObject BrushToolPanel;
    public GameObject GradientToolPanel;

    
    public void SwitchTool(Tool tool)
    {
        if (tool == Tool.Gradient)
        {
            BrushToolPanel.SetActive(false);
            GradientToolPanel.SetActive(true);
        }
        else if (tool == Tool.Painter || tool == Tool.Selector)
        {
            BrushToolPanel.SetActive(true);
            GradientToolPanel.SetActive(false);
        }
    }
}

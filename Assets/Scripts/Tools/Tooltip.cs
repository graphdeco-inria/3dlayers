using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    [Header("Highlight current active interaction")]
    public InputActionProperty highlightCurrentActiveAction;

    private TextMeshProUGUI tooltipText;

    private string currentActiveLayerText = "";
    private string currentActiveTool = "";

    // Use this for initialization
    void Start()
    {
        highlightCurrentActiveAction.action.started += ToggleTooltip;
        highlightCurrentActiveAction.action.canceled += ToggleTooltip;

        LayerManager.OnActiveLayerUpdate += ToggleTooltip;
        ToolsManager.OnToolChange += ToggleTooltip;

        tooltipText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnDestroy()
    {
        highlightCurrentActiveAction.action.started -= ToggleTooltip;
        highlightCurrentActiveAction.action.canceled -= ToggleTooltip;

        LayerManager.OnActiveLayerUpdate -= ToggleTooltip;
        ToolsManager.OnToolChange -= ToggleTooltip;
    }

    private void ToggleTooltip(Layer l)
    {
        currentActiveLayerText = $"{l.LayerName}";
        UpdateTooltip();
    }

    private void ToggleTooltip(Tool active)
    {
        currentActiveTool = $"{active}";
        UpdateTooltip();

    }

    private void ToggleTooltip(InputAction.CallbackContext ctx)
    {
        UpdateTooltip();

    }

    private void ToggleVisibility(bool visible)
    {
        GetComponent<Image>().enabled = visible;
        tooltipText.enabled = visible;
    }

    private void UpdateTooltip()
    {
        if (highlightCurrentActiveAction.action.IsPressed())
        {
            ToggleVisibility(true);
        }
        else
        {
            ToggleVisibility(false);
        }
        tooltipText.text = $"Active layer: {currentActiveLayerText} <br> Active tool: {currentActiveTool}";
    }
}
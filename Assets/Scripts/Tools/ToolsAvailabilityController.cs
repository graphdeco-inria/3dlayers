using System.Collections;
using UnityEngine;


public class ToolsAvailabilityController : MonoBehaviour
{

    public GameObject gradientButton;
    public GameObject alphaSlider;

    public ToolsManager toolsManager;

    private void OnEnable()
    {
        LayerManager.OnActiveLayerUpdate += HandleLayerActivate;
    }

    private void OnDisable()
    {
        LayerManager.OnActiveLayerUpdate -= HandleLayerActivate;
    }

    private void HandleLayerActivate(Layer l)
    {
        if (l != null && (l is ClippedLayer))
        {
            gradientButton.gameObject.SetActive(true);
            alphaSlider.gameObject.SetActive(true);
        }
        else
        {
            gradientButton.gameObject.SetActive(false);
            alphaSlider.gameObject.SetActive(false);

            // Switch tool if gradient tool was selected
            toolsManager.SwitchOffTool((int)Tool.Gradient);
        }
    }
}
using System.Collections;
using UnityEngine;


public class GradientToolSettings : MonoBehaviour
{

    public GradientTool tool;

    public void UpdateGradientShape(int shape)
    {
        tool.GradientShape = (GradientMaskType)shape;
    }

    public void UpdateGradientDirection(int dir)
    {
        tool.GradientDirection = dir;
    }
}
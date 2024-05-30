using HSVPicker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(LineRenderer))]
public class GradientTool : SphereTool
{

    public GradientMaskType GradientShape = GradientMaskType.Linear;
    public int GradientDirection = 0;

    private Vector3 currentStartPoint;
    private bool isDragging = false;
    private float basePressureValue = 0.5f;

    private LineRenderer lineRenderer;

    protected override Tool ToolType { get { return Tool.Gradient; } }

    protected override void Start()
    {
        base.Start();
        //action.action.started += ctx => { StartGradient(); };
        //action.action.canceled += ctx => { EndGradient(); };

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.black, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1.0f) }
        );
        lineRenderer.colorGradient = gradient;
    }

    protected override void Update()
    {
        base.Update();

        if (doingAction)
            UpdateGizmo();
    }

    protected override void StartAction(InputAction.CallbackContext ctx)
    {
        base.StartAction(ctx);
        if (doingAction)
            StartGradient();
    }

    protected override void StopAction(InputAction.CallbackContext ctx)
    {
        if (doingAction)
            EndGradient();
        base.StopAction(ctx);
    }

    void StartGradient()
    {
        //if (!manager.CanUseTool(this))
        //    return;

        //manager.MarkBusy();

        //Debug.Log("start gradient");

        currentStartPoint = handPosition.action.ReadValue<Vector3>();
        lineRenderer.SetPosition(0, currentStartPoint);
        lineRenderer.SetPosition(1, currentStartPoint);
        lineRenderer.enabled = true;

        isDragging = true;
    }

    void UpdateGizmo()
    {
        Vector3 position = handPosition.action.ReadValue<Vector3>();

        lineRenderer.SetPosition(1, position);

    }

    void EndGradient()
    {
        //if (!isDragging)
        //    return;

        ////manager.MarkIdle();
        //isDragging = false;

        lineRenderer.enabled = false;

        Vector3 ACanvasSpace = paintCanvas.transform.InverseTransformPoint(currentStartPoint);
        Vector3 BCanvasSpace = paintCanvas.transform.InverseTransformPoint(handPosition.action.ReadValue<Vector3>());

        //if (GradientDirection == 1)
        //{
        //    Debug.Log("revert gradient direction");
        //    (ACanvasSpace, BCanvasSpace) = (BCanvasSpace, ACanvasSpace);
        //}

        Debug.Log("gradient type = " + GradientShape);
        // Notify layer manager to set the current layer's gradient mask
        paintCanvas.SetGradient(ACanvasSpace, BCanvasSpace, GradientShape, GradientDirection);

        //isDragging = false;
    }

    //public override bool TrySwitchOff()
    //{
    //    if (currentStroke == null)
    //    {
    //        return true;
    //    }
    //    else
    //        return false;
    //}

}
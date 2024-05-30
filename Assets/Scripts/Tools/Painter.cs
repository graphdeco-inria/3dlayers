using HSVPicker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Painter : SphereTool
{

    //[Header("XR Input Actions")]
    //public InputActionProperty drawingHandPosition;

    public InputActionProperty colorPickAction;

    public Pointer3D pointer;
    //public InputActionProperty drawActionValue;

    //[Header("Canvas")]
    //public PaintCanvas paintCanvas;

    //[Header("Painter propeties")]
    //public Color _PaintColor = Color.yellow;
    public ColorPicker picker;
    //public float _BaseRadius = 0.01f;

    private Stroke currentStroke;
    private float basePressureValue = 0.5f;

    protected override Tool ToolType { get { return Tool.Painter; } }

    protected override void Start()
    {
        base.Start();

        colorPickAction.action.started += HandleColorPick;

    }

    protected override void OnDisable()
    {
        base.OnDisable();

        colorPickAction.action.started -= HandleColorPick;
    }

    protected override void Update()
    {
        base.Update();

        if (currentStroke)
            UpdateStroke();
    }

    protected override void StartAction(InputAction.CallbackContext ctx)
    {
        base.StartAction(ctx);
        if (doingAction)
            CreateStroke();
    }

    protected override void StopAction(InputAction.CallbackContext ctx)
    {
        base.StopAction(ctx);
        EndStroke();
    }

    void CreateStroke()
    {
        float radius = WorldRadius() * 1f / paintCanvas.transform.localScale.x;
        Color strokeColor = picker.CurrentColor.linear;

        currentStroke = paintCanvas.CreateStroke(strokeColor, radius);
        basePressureValue = actionValue.action.ReadValue<float>();
    }

    void UpdateStroke()
    {
        Vector3 position = handPosition.action.ReadValue<Vector3>();
        float pressure = actionValue.action.ReadValue<float>();

        float relPressure = pressure;
        if (basePressureValue < 0.9f)
            relPressure = Mathf.Max(0f, (pressure - basePressureValue)/ (1f - basePressureValue));

        // To Canvas Space
        position = paintCanvas.transform.InverseTransformPoint(position);


        currentStroke.DrawPoint(position, relPressure);

    }

    void EndStroke()
    {
        if (currentStroke != null)
        {

            //currentStroke.Sanitize();
            float simplifyThreshold = 5e-4f * 1f / paintCanvas.transform.localScale.x;
            currentStroke.End(simplifyThreshold);

            SessionHistory.Instance.RegisterAction(
                new DrawAction(currentStroke)
            );

            //manager.MarkIdle();
            currentStroke = null;
        }

    }

    private void HandleColorPick(InputAction.CallbackContext ctx)
    {
        Collider[] overlapped = pointer.QueryOverlap();

        // TODO: maybe we should only select colors from the active layer?
        foreach(Collider o in overlapped)
        {
            Primitive p = o.GetComponent<Primitive>();
            if (p != null)
            {
                picker.CurrentColor = p.GetNearestColor(pointer.transform.position);
                //Debug.Log(picker.CurrentColor);
            }
        }
    }


}

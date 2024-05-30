using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class SphereTool : MonoBehaviour, ITool
{

    private const float MIN_RADIUS = 0.1f;
    private const float MAX_RADIUS = 20f;
    private const float INITIAL_RADIUS = 1f;
    private const float RADIUS_INCREMENT = 0.05f;

    [Header("XR Input Actions")]
    public InputActionProperty handPosition;

    public InputActionProperty action;
    public InputActionProperty actionValue;

    public InputActionProperty scaleSphereActionValue;

    [Header("Canvas")]
    public PaintCanvas paintCanvas;

    public float SphereToolRadius
    {
        get; protected set;
    } = INITIAL_RADIUS;

    protected bool doingAction = false;


    protected ToolsManager manager;
    protected abstract Tool ToolType { get; }

    protected virtual void Start()
    {
        action.action.started += StartAction;
        action.action.canceled += StopAction;
    }

    protected virtual void OnDisable()
    {
        action.action.started -= StartAction;
        action.action.canceled -= StopAction;
    }

    public Tool GetToolType()
    {
        return ToolType;
    }

    public void SetManager(ToolsManager manager)
    {
        this.manager = manager;
    }

    protected virtual void Update()
    {
        Vector2 thumbstick = scaleSphereActionValue.action.ReadValue<Vector2>();
        if (this.manager && this.manager.CanUseTool(this) && Mathf.Abs(thumbstick.y) > 0.9f)
        {
            if (thumbstick.y < 0)
            {
                SphereToolRadius -= RADIUS_INCREMENT;
            }
            else
            {
                SphereToolRadius += RADIUS_INCREMENT;
            }

            SphereToolRadius = Mathf.Clamp(SphereToolRadius,MIN_RADIUS, MAX_RADIUS);

            manager.ChangeToolRadius(SphereToolRadius);
        }
    }

    protected float WorldRadius()
    {
        return transform.lossyScale.x * 0.5f * SphereToolRadius;
    }

    protected virtual void StartAction(InputAction.CallbackContext ctx)
    {
        if (manager.CanUseTool(this))
        {
            doingAction = true;
            manager.MarkBusy();
        }
    }

    protected virtual void StopAction(InputAction.CallbackContext ctx)
    {
        doingAction = false;
        manager.MarkIdle();
    }


}

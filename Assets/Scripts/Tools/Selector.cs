using HSVPicker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Selector : SphereTool
{

    public InputActionProperty altButtonAction;

    public static event Action<int> OnLayerActivate;

    public ColorPicker picker;
    public Pointer3D pointer;

    protected override Tool ToolType { get { return Tool.Selector; } }

    private bool HadEffect = false;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // Listen for stroke/hand collision events
        Pointer3D.OnPrimitiveCollide += HandlePrimitiveCollide;

        // Listen for color picker change: if there is a selection, we recolor the selected strokes
        picker.onValueChanged.AddListener(HandleRecolor);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        picker.onValueChanged.RemoveListener(HandleRecolor);
    }

    protected override void StartAction(InputAction.CallbackContext ctx)
    {
        base.StartAction(ctx);

        HadEffect = false;

        if (doingAction && pointer)
        {
            Collider[] collided = pointer.QueryOverlap();

            foreach (Collider c in collided)
            {
                // Select any primitives that were overlapping

                Primitive p = c.GetComponent<Primitive>();
                if (p != null)
                {
                    bool stateChanged = paintCanvas.ToggleSelect(p, !altButtonAction.action.IsPressed());
                    HadEffect = HadEffect || stateChanged;
                }
            }
        }
    }

    protected override void StopAction(InputAction.CallbackContext ctx)
    {
        base.StopAction(ctx);

        // Did we collide with any primitive?
        //Debug.Log(HadEffect);
        if (!HadEffect)
            paintCanvas.UnselectAll();

    }

    private void HandlePrimitiveCollide(Primitive s)
    {
        if (doingAction)
        {
            //Debug.Log("(un)selecting primitive " + s.UID);
            bool stateChanged = paintCanvas.ToggleSelect(s, !altButtonAction.action.IsPressed());
            HadEffect = HadEffect || stateChanged;
            //HadEffect = true;
        }
    }

    private void HandleRecolor(Color pickedColor)
    {
        Color newColor = pickedColor.linear;
        Primitive[] selection = CurrentSelection();

        if (selection.Length == 0)
            return;

        //Color[][] initialColors = new Color[selection.Length][];
        Color[] initialColors = new Color[selection.Length];
        bool[] initialApplyColors = new bool[selection.Length];

        for (int i = 0; i < selection.Length; i++)
        {
            //Debug.Log(selection[i].name);
            (initialColors[i], initialApplyColors[i]) = selection[i].Recolor(newColor);
        }

        SessionHistory.Instance.RegisterAction(
            new RecolorAction(selection, newColor, initialColors, initialApplyColors)
        );
    }

    public Primitive[] CurrentSelection()
    {
        return paintCanvas.Selected();
    }

    

}

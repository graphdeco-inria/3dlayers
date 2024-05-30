using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class SessionManager : MonoBehaviour
{
    public InputActionProperty joystickActionValue;
    public float undoCoolDownPeriod = 0.2f;

    private float coolDownTimer = 0f;
    private ActionBasedController controller;

    private void Start()
    {
        controller = GetComponentInParent<ActionBasedController>();
    }

    void Update()
    {
        Vector2 joystick = joystickActionValue.action.ReadValue<Vector2>();
        if (Mathf.Abs(joystick.x) > 0.9f && coolDownTimer == 0f)
        {
            coolDownTimer = undoCoolDownPeriod;
            if (joystick.x < 0)
            {
                Undo();
                controller.SendHapticImpulse(0.5f, 0.1f);
            }
            else
            {
                Redo();
                controller.SendHapticImpulse(0.5f, 0.1f);
            }
        }
        coolDownTimer = Mathf.Max(0f, coolDownTimer - Time.deltaTime);
    }

    //public void ExportSession()
    //{
    //    SessionHistory.Instance.Write();
    //}

    public void Undo()
    {
        SessionHistory.Instance.UndoAction();
    }

    public void Redo()
    {
        SessionHistory.Instance.RedoAction();
    }
}

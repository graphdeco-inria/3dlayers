using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotatingMenu : MonoBehaviour
{
    //public InputActionProperty joystickAction;
    public InputActionProperty joystickActionValue;
    public float coolDownPeriod = 1f;

    private Animator animator;
    private float coolDownTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        Vector2 joystick = joystickActionValue.action.ReadValue<Vector2>();
        if (joystick.magnitude > 0.9f && coolDownTimer == 0f)
        {
            coolDownTimer = coolDownPeriod;
            if (joystick.x < 0)
            {
                animator.SetTrigger("TrRotateDec");
            }
            else
            {
                //transform.Rotate(Vector3.up, -120);
                animator.SetTrigger("TrRotateInc");
            }
        }
        coolDownTimer = Mathf.Max(0f, coolDownTimer - Time.deltaTime);
        //Debug.Log(joystickActionValue.action.ReadValue<Vector2>());
    }
}

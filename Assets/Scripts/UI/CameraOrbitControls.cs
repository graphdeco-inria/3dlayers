//  A simple Unity C# script for orbital movement around a target gameobject
//  Author: Ashkan Ashtiani
//  Gist on Github: https://gist.github.com/3dln/c16d000b174f7ccf6df9a1cb0cef7f80

using System;
using UnityEngine;


public class CameraOrbitControls : MonoBehaviour
{
    public Vector3 target = Vector3.zero;
    public float distance = 10.0f;

    public float minDistance = 0.01f;

    public float xSpeed = 250.0f;
    public float ySpeed = 120.0f;

    public float yMinLimit = -20;
    public float yMaxLimit = 80;

    float x = 0.0f;
    float y = 0.0f;

    void Start()
    {
        var angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    float prevDistance;

    void LateUpdate()
    {
        distance -= Input.GetAxis("Mouse ScrollWheel") * 0.25f;
        if (distance < minDistance) distance = minDistance;
        //Vector3 orbitCenterPos = target;

        if (Input.GetMouseButton(0))
        {
            var pos = Input.mousePosition;
            var dpiScale = 1f;
            if (Screen.dpi < 1) dpiScale = 1;
            if (Screen.dpi < 200) dpiScale = 1;
            else dpiScale = Screen.dpi / 200f;

            if (pos.x < 380 * dpiScale && Screen.height - pos.y < 250 * dpiScale) return;

            // comment out these two lines if you don't want to hide mouse curser or you have a UI button 
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            y = ClampAngle(y, yMinLimit, yMaxLimit);
            var rotation = Quaternion.Euler(y, x, 0);
            var position = rotation * new Vector3(0.0f, 0.0f, -distance) + target;
            transform.rotation = rotation;
            transform.position = position;

        }
        else if (Input.GetMouseButton(1))
        {
            Vector3 pos = Input.mousePosition;
            var dpiScale = 1f;
            if (Screen.dpi < 1) dpiScale = 1;
            if (Screen.dpi < 200) dpiScale = 1;
            else dpiScale = Screen.dpi / 200f;

            if (pos.x < 380 * dpiScale && Screen.height - pos.y < 250 * dpiScale) return;

            // comment out these two lines if you don't want to hide mouse curser or you have a UI button 
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // Update target
            Vector3 displacement = new Vector3(-Input.GetAxis("Mouse X") * xSpeed * 0.002f, -Input.GetAxis("Mouse Y") * ySpeed * 0.002f, 0f);
            displacement = transform.TransformVector(displacement);
            target += displacement;

            var rotation = Quaternion.Euler(y, x, 0);
            var position = rotation * new Vector3(0.0f, 0.0f, -distance) + target;
            transform.rotation = rotation;
            transform.position = position;
        }
        else
        {
            // comment out these two lines if you don't want to hide mouse curser or you have a UI button 
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (Math.Abs(prevDistance - distance) > 0.001f)
        {
            prevDistance = distance;
            var rot = Quaternion.Euler(y, x, 0);
            var po = rot * new Vector3(0.0f, 0.0f, -distance) + target;
            transform.rotation = rot;
            transform.position = po;
        }
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}

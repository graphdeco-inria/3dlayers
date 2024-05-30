
#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



[CustomEditor(typeof(CameraPathAnimator))]
public class CameraPathAnimatorEditor : Editor
{

    CameraPathAnimator camPath;


    private void OnEnable()
    {
        MonoBehaviour o = (MonoBehaviour)target;
        camPath = o.GetComponent<CameraPathAnimator>();
        //keyframes = new List<CameraKeyframe>();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //serializedObject.Update();
        //EditorGUILayout.PropertyField(serializedObject.FindProperty("keyframes"));
        //serializedObject.ApplyModifiedProperties();

        if (camPath && GUILayout.Button("Set Start"))
        {
            camPath.SetStart();
        }

        if (camPath && GUILayout.Button("Set End"))
        {
            camPath.SetEnd();
        }

        if (camPath && GUILayout.Button("Animate"))
        {
            camPath.Play();
        }
    }
}
#endif
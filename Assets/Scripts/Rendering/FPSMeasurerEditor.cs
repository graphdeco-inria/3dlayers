#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

[CustomEditor(typeof(FPSMeasurer))]
public class FPSMeasurerEditor : Editor
{

    FPSMeasurer fpsMeasurer;


    private void OnEnable()
    {
        MonoBehaviour o = (MonoBehaviour)target;
        fpsMeasurer = o.GetComponent<FPSMeasurer>();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (fpsMeasurer && GUILayout.Button("Start Measure"))
        {
            fpsMeasurer.StartMeasure();
        }
    }
}
#endif
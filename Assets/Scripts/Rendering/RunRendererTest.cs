#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LayerRendererTester))]
public class RunRendererTest : Editor
{

    LayerRendererTester tester;


    private void OnEnable()
    {
        MonoBehaviour o = (MonoBehaviour)target;
        tester = o.GetComponent<LayerRendererTester>();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (tester && GUILayout.Button("Set Test Conditions"))
        {
            tester.SetTestConditions();
        }
        if (tester && GUILayout.Button("Log FPS"))
        {
            tester.LogCurrentFPS();
        }
        if (tester && GUILayout.Button("Save test"))
        {
            tester.SaveTestData();
        }
    }
}
#endif
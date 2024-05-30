#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SessionVisualizer))]
public class SessionVisualizerEditor : Editor
{

    SessionVisualizer visualizer;


    private void OnEnable()
    {
        MonoBehaviour o = (MonoBehaviour)target;
        visualizer = o.GetComponent<SessionVisualizer>();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (visualizer && GUILayout.Button("Reset Session"))
        {
            visualizer.ResetSession();
        }

        if (visualizer && GUILayout.Button("Play Session"))
        {
            visualizer.PlaySession();
        }

        if (visualizer && GUILayout.Button("Go To Action"))
        {
            visualizer.GoToSelectedAction();
        }
    }
}
#endif
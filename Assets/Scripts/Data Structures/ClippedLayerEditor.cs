#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ClippedLayer))]
public class ClippedLayerEditor : Editor
{

    ClippedLayer clippedLayer;


    private void OnEnable()
    {
        MonoBehaviour o = (MonoBehaviour)target;
        clippedLayer = o.GetComponent<ClippedLayer>();
    }


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (clippedLayer)
        {
            clippedLayer.DepthThreshold = EditorGUILayout.Slider(clippedLayer.DepthThreshold, 0, 10);
        }
    }
}
#endif
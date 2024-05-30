#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;



[CustomEditor(typeof(LayerRenderer))]
public class LayerRendererEditor : Editor
{

    LayerRenderer layerRenderer;




    private void OnEnable()
    {
        MonoBehaviour o = (MonoBehaviour)target;
        layerRenderer = o.GetComponent<LayerRenderer>();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (layerRenderer && GUILayout.Button("Render current frame"))
        {
            layerRenderer.RenderFrame();
        }
    }
}
#endif
#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;



[CustomEditor(typeof(LayerAnimationRenderer))]
public class LayerAnimationRendererEditor : Editor
{

    LayerAnimationRenderer animRenderer;




    private void OnEnable()
    {
        MonoBehaviour o = (MonoBehaviour)target;
        animRenderer = o.GetComponent<LayerAnimationRenderer>();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (animRenderer && GUILayout.Button("Set Start"))
        {
            animRenderer.SetStart();
        }

        if (animRenderer && GUILayout.Button("Set End"))
        {
            animRenderer.SetEnd();
        }

        if (animRenderer && GUILayout.Button("Animate"))
        {
            animRenderer.Play();
        }

        if (animRenderer && GUILayout.Button("Go To Frame"))
        {
            animRenderer.GoToSelectedFrame();
        }
    }
}
#endif
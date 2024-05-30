#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PokableRect))]
public class PokableRectEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
        serializedObject.Update();

        if (GUILayout.Button("Update collider"))
        {
            ((PokableRect)target).UpdateCollider();
        }
    }
}
#endif
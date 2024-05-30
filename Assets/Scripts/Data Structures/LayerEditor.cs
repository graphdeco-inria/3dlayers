//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(Layer))]
//public class LayerEditor : Editor
//{
//    Layer layer;

//    private void OnEnable()
//    {
//        MonoBehaviour o = (MonoBehaviour)target;
//        layer = o.GetComponent<Layer>();
//    }

//    public override void OnInspectorGUI()
//    {
//        DrawDefaultInspector();

//        if (layer && GUILayout.Button("Hide"))
//        {
//            layer.gameObject.SetActive(false);
//        }

//        if (layer && layer as ClippedLayer != null && GUILayout.Button("Debug Highlight"))
//        {
//            ((ClippedLayer)layer).Highlight();
//        }
//    }
//}

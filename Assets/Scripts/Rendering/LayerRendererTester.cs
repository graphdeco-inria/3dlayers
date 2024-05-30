using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public struct TestState
{
    public int seed;

    public int NbBaseLayers;
    public int NbAppearanceLayersPerStack;

    public int NbBasePrimitives;
    public int NbAppearancePrimitivesPerLayer;
}

[System.Serializable]
public struct TestResult
{
    public TestState state;
    public float fps;
}

[RequireComponent(typeof(LayerManager))]
public class LayerRendererTester : MonoBehaviour
{
    public GameObject TestMeshesPrefab;

    public TestState state;
    //public int seed = 0;

    //public int NbBaseLayers = 1;
    //public int NbAppearanceLayersPerStack = 1;
 
    //public int NbBasePrimitives = 10;
    //public int NbAppearancePrimitivesPerLayer = 10;

    public float SpreadRadius = 5f;

    private LayerManager _manager;
    private PaintCanvas _canvas;

    private List<TestResult> testData;

    // Start is called before the first frame update
    void Start()
    {
        _manager = GetComponent<LayerManager>();
        _canvas = GetComponentInParent<PaintCanvas>();

        SetTestConditions();

        testData = new List<TestResult>();
    }

    public void SetTestConditions()
    {
        // Hard reset layer manager
        _manager.enabled = false;
        _manager.enabled = true;

        System.Random random = new System.Random(state.seed);
        UnityEngine.Random.InitState(state.seed);


        // Scene-wide stack:
        // Todo: enforce explicitly that UID == 0 for the scene-wide layer
        BaseLayer scenWideLayer = _manager.CreateBaseLayer("Scene", registerInHistory: false);

        // Generate primitives and layers
        // - Base Layers
        for (int i = 0; i < state.NbBaseLayers; i++)
        {
            _manager.CreateBaseLayer($"Base {i + 1}");
        }
        // - Appearance Layers
        int[] baseUIDs = _manager.BaseLayersUID;

        foreach (int baseUID in baseUIDs)
        {
            // Do not add primitives to scene layer
            if (baseUID > 0)
            {
                for (int j = 0; j < state.NbAppearanceLayersPerStack; j++)
                {
                    ClippedLayer cl = _manager.CreateClippedLayer(baseUID, $"Clipped {j + 1} (over {_manager.GetBaseLayerName(baseUID)})");
                    //cl.DepthThreshold = 1f;
                    //cl.AddGradientMask(Vector3.zero, Vector3.forward, GradientMaskType.Linear, 0);
                }
            }
        }



        // - Primitives
        // Create preloaded primitives by loading the test meshes
        //int randomNumber = random.Next();
        MeshFilter[] testMeshes = TestMeshesPrefab.GetComponentsInChildren<MeshFilter>();
        Debug.Log($"Sampling primitive mesh on prefab {TestMeshesPrefab.name} among {testMeshes.Length} meshes");
        foreach (int baseUID in baseUIDs)
        {
            _manager.SetActive(baseUID);
            // Do not add primitives to scene layer
            if (baseUID > 0)
            {
                for (int primitiveIdx = 0; primitiveIdx < state.NbBasePrimitives; primitiveIdx++)
                {
                    MeshFilter sampledMesh = testMeshes[random.Next(testMeshes.Length)];
                    PreloadedPrimitive primitive = _canvas.Preload(TestMeshesPrefab.name, sampledMesh.gameObject.name, sampledMesh.sharedMesh);

                    // Random color
                    primitive.Recolor(UnityEngine.Random.ColorHSV());
                    //primitive.Recolor(Color.black);

                    // Random position
                    primitive.transform.SetPositionAndRotation(UnityEngine.Random.insideUnitSphere * SpreadRadius, UnityEngine.Random.rotationUniform);
                }
            }


            // Add to stacked layers
            foreach (ClippedLayer layer in _manager.GetStack(baseUID))
            {
                Color layerColor = UnityEngine.Random.ColorHSV();
                _manager.SetActive(layer.UID);
                for (int primitiveIdx = 0; primitiveIdx < state.NbAppearancePrimitivesPerLayer; primitiveIdx++)
                {
                    MeshFilter sampledMesh = testMeshes[random.Next(testMeshes.Length)];
                    PreloadedPrimitive primitive = _canvas.Preload(TestMeshesPrefab.name, sampledMesh.gameObject.name, sampledMesh.sharedMesh);

                    // Random color
                    primitive.Recolor(layerColor);
                    //primitive.Recolor(Color.black);

                    // Random position
                    primitive.transform.SetPositionAndRotation(UnityEngine.Random.insideUnitSphere * SpreadRadius, UnityEngine.Random.rotationUniform);
                }
            }
        }
    }

    public void LogCurrentFPS()
    {
        float frameRate = 1f / Time.smoothDeltaTime;
        Debug.Log("current fps = " + frameRate);
        TestResult result = new TestResult();
        result.state = state;
        result.fps = frameRate;
        testData.Add(result);
    }

    public void SaveTestData()
    {
        Debug.Log(JsonConvert.SerializeObject(testData));

        string fileName = (DateTime.Now).ToString("yyyy-MM-dd-HH-mm-ss") + "_performance_test";
        File.WriteAllText(Path.Combine(Application.dataPath, "PerformanceLogs~", fileName + ".json"), JsonConvert.SerializeObject(testData, new JsonSerializerSettings
        {
            Culture = new System.Globalization.CultureInfo("en-US")
        }));
    }
}

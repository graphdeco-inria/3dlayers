
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public struct SceneMetrics
{
    public string scene;
    public int substrateTrianglesCount;
    public int appearanceTrianglesCount;
    public int substratePrimitivesCount;
    public int appearancePrimitivesCount;
    public int stacksCount;
    public int appearanceLayersCount;
}

public struct FPSDataPoint
{
    public SceneMetrics scene;
    public float fps;
}

[RequireComponent(typeof(LayerManager))]
public class FPSMeasurer : MonoBehaviour
{
    public int NSamples = 20;
    public string folderName;

    LayerManager _manager;

    SceneMetrics currentMeasureData;
    float[] fpsSamples;
    int sampleCounter = 0;
    bool capturingSamples = false;

    void Start()
    {
        _manager = GetComponent<LayerManager>();
    }

    private void Update()
    {
        if (capturingSamples)
        {
            float frameRate = 1f / Time.smoothDeltaTime;
            fpsSamples[sampleCounter] = frameRate;
            sampleCounter++;

            if (sampleCounter == NSamples)
            {
                capturingSamples = false;

                // Compute mean fps
                float mean = 0f;
                foreach(float fps in fpsSamples)
                {
                    Debug.Log(fps);
                    mean += fps;
                }

                float meanFPS = mean / NSamples;

                Debug.Log($"mean = {meanFPS}");
                SaveTestData(meanFPS);
            }
        }
    }

    public void StartMeasure()
    {
        // First measure scene data
        currentMeasureData = new SceneMetrics();
        currentMeasureData.scene = _manager.InputFileName;
        // - layer counts
        currentMeasureData.stacksCount = _manager.StacksCount;
        currentMeasureData.appearanceLayersCount = _manager.AppearanceLayersCount;
        (currentMeasureData.appearancePrimitivesCount, currentMeasureData.appearanceTrianglesCount) = _manager.GetPrimitivesCount(true);
        (currentMeasureData.substratePrimitivesCount, currentMeasureData.substrateTrianglesCount) = _manager.GetPrimitivesCount(false);

        print(JsonConvert.SerializeObject(currentMeasureData));

        sampleCounter = 0;
        fpsSamples = new float[NSamples];
        capturingSamples = true;
    }


    public void SaveTestData(float fps)
    {
        FPSDataPoint data = new FPSDataPoint();
        data.scene = currentMeasureData;
        data.fps = fps;

        string path = Path.Combine(Application.dataPath, "PerformanceLogs~", folderName);
        // Try to create the directory
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
        }

        string fileName = (DateTime.Now).ToString("yyyy-MM-dd-HH-mm-ss") + "_performance_test";
        File.WriteAllText(Path.Combine(path, fileName + ".json"), JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            Culture = new System.Globalization.CultureInfo("en-US")
        }));
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ControlMode
{
    Camera, Primitives
}

[Serializable]
public struct SceneKeyframe
{
    public SerializableVector3 position;
    public SerializableQuaternion rotation;

    public SceneKeyframe(Vector3 position, Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }
}


[RequireComponent(typeof(LayerRenderer))]
public class LayerAnimationRenderer : MonoBehaviour
{
    private LayerRenderer layerRenderer;

    public ControlMode controlMode;

    public int StartFrame = 0;
    public int EndFrame = 100;
    public int NumberOfFrames = 100;

    public int SelectedFrame = 0;

    public bool RenderFrames = false;
    public string FolderName = "path";

    public SceneKeyframe start;
    public SceneKeyframe end;

    private List<SceneKeyframe> primitivesStart;
    private List<SceneKeyframe> primitivesEnd;

    private bool playing = false;
    private int frameIdx = 0;

    // Use this for initialization
    void Start()
    {
        layerRenderer = GetComponent<LayerRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playing)
        {
            if (frameIdx > EndFrame + 1)
            {
                playing = false;
            }
            else
            {
                if (frameIdx <= EndFrame)
                    GoToFrame(frameIdx);

                if (RenderFrames && layerRenderer && frameIdx > 0)
                {
                    layerRenderer.RenderAnimationFrame(FolderName, frameIdx - 1);
                }
                frameIdx++;
            }
        }
    }

    public void SetStart()
    {
        start = new SceneKeyframe(transform.position, transform.rotation);

        if (controlMode == ControlMode.Primitives)
        {
            primitivesStart = new List<SceneKeyframe>();

            Primitive[] childrenPrimitives = GetComponentsInChildren<Primitive>();

            foreach(Primitive primitive in childrenPrimitives)
            {
                Transform original = primitive.GetComponent<Transform>();
                SceneKeyframe kf = new SceneKeyframe();
                kf.position = original.position;
                kf.rotation = original.rotation;
                primitivesStart.Add(kf);
            }
        }
    }
    public void SetEnd()
    {
        end = new SceneKeyframe(transform.position, transform.rotation);

        if (controlMode == ControlMode.Primitives)
        {
            primitivesEnd = new List<SceneKeyframe>();

            Primitive[] childrenPrimitives = GetComponentsInChildren<Primitive>();

            foreach (Primitive primitive in childrenPrimitives)
            {
                Transform original = primitive.GetComponent<Transform>();
                SceneKeyframe kf = new SceneKeyframe();
                kf.position = original.position;
                kf.rotation = original.rotation;
                primitivesEnd.Add(kf);
            }
        }
    }

    public void Play()
    {
        playing = true;
        frameIdx = StartFrame;

        if (EndFrame >= NumberOfFrames)
            EndFrame = NumberOfFrames - 1;
    }

    public void Pause()
    {
        playing = false;
    }

    public void GoToSelectedFrame()
    {
        GoToFrame(SelectedFrame);
    }

    private void GoToFrame(int selectedFrame)
    {
        float u = ((float) selectedFrame) / NumberOfFrames;
        transform.position = Vector3.Lerp(start.position, end.position, u);
        transform.rotation = Quaternion.Lerp(start.rotation, end.rotation, u);

        if (controlMode == ControlMode.Primitives)
        {
            Primitive[] childrenPrimitives = GetComponentsInChildren<Primitive>();

            if (primitivesEnd.Count == primitivesStart.Count && childrenPrimitives.Length == primitivesStart.Count)
            {
                for (int i = 0; i < childrenPrimitives.Length; i++)
                {
                    SceneKeyframe primitiveStart = primitivesStart[i];
                    SceneKeyframe primitiveEnd = primitivesEnd[i];
                    childrenPrimitives[i].transform.position = Vector3.Lerp(primitiveStart.position, primitiveEnd.position, u);
                    childrenPrimitives[i].transform.rotation = Quaternion.Lerp(primitiveStart.rotation, primitiveEnd.rotation, u);
                }
            }


        }
    }
}

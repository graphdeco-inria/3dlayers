using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct CameraKeyframe
{
    public SerializableVector3 position;
    public SerializableQuaternion rotation;

    public CameraKeyframe(Vector3 position, Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }
}

public class CameraPathAnimator : MonoBehaviour
{
    public LayerRenderer layerRenderer;

    public int NumberOfFrames = 100;

    public bool RenderFrames = false;
    public string FolderName = "path";

    public CameraKeyframe start;
    public CameraKeyframe end;

    private bool playing = false;
    private int frameIdx = 0;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (playing)
        {
            if (frameIdx > NumberOfFrames)
            {
                playing = false;
            }
            else
            {

                float u = ((float)frameIdx) / NumberOfFrames;
                transform.position = Vector3.Lerp(start.position, end.position, u);
                transform.rotation = Quaternion.Lerp(start.rotation, end.rotation, u);

                if (RenderFrames && layerRenderer)
                {
                    layerRenderer.RenderAnimationFrame(FolderName, frameIdx);
                }
                frameIdx++;
            }
        }
    }

    public void SetStart()
    {
        start = new CameraKeyframe(transform.position, transform.rotation);
    }
    public void SetEnd()
    {
        end = new CameraKeyframe(transform.position, transform.rotation);
    }

    public void Play()
    {
        playing = true;
        frameIdx = 0;
    }

    public void Pause()
    {
        playing = false;
    }
}
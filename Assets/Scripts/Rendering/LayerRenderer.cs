using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.XR;

//[ExecuteInEditMode]
public class LayerRenderer : MonoBehaviour
{
    [Header("Highlight current active interaction")]
    public InputActionProperty highlightCurrentActiveAction;

    [Header("Frame export parameters")]
    public string FrameExportFolder;
    public string FrameExportName;

    private Camera[] cameras;

    private CommandBuffer commandBufferClipped;
    private CommandBuffer commandBufferUI;

    private Material selectedStrokeMaterial;
    private Material hoveredLayerMaterial;

    private LayerManager layerManager;

    private bool needsToRenderFrame = false;

    private string overrideFrameExportFolder;
    private string overrideFileName;


    private void OnEnable()
    {

        cameras = new Camera[Camera.allCamerasCount];
        Camera.GetAllCameras(cameras);

        foreach (var cam in cameras)
        {
            //Debug.Log(cam.name);
            cam.depthTextureMode = DepthTextureMode.Depth;
        }
    }

    private void Start()
    {
        layerManager = GetComponent<LayerManager>();

        selectedStrokeMaterial = new Material(Shader.Find("VRPaint/GridHighlight"));
        selectedStrokeMaterial.SetFloat("_TimeFreq", 0f);
        hoveredLayerMaterial = new Material(Shader.Find("VRPaint/GridHighlight"));
        hoveredLayerMaterial.SetFloat("_TimeFreq", 4f);

    }

    public void OnDisable()
    {
        foreach(var camera in cameras)
        {
            if (commandBufferClipped != null && camera)
                camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, commandBufferClipped);
            if (commandBufferUI != null && camera)
            {
                camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, commandBufferUI);
            }
        }



    }

    public void OnWillRenderObject()
    {


        if (needsToRenderFrame)
        {
            // File name
            string fileName = overrideFileName ?? (DateTime.Now).ToString("yyyy-MM-dd-HH-mm-ss") + (FrameExportName ?? "_frame") + ".png";
            string path = Path.Combine(Application.dataPath, overrideFrameExportFolder ?? FrameExportFolder);
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
                Debug.Log(ex.Message);
            }
            string fullPath = Path.Combine(path, fileName);
            ExportBufferToPNG(fullPath);

            needsToRenderFrame = false;
            overrideFrameExportFolder = null;
            overrideFileName = null;
        }


        //Debug.Log("rendering...");
        var act = gameObject.activeInHierarchy && enabled;
        if (!act)
        {
            OnDisable();
            return;
        }


        if (commandBufferClipped != null)
        {
            commandBufferClipped.Clear();
            commandBufferUI.Clear();
        }
        else
        {
            commandBufferClipped = new CommandBuffer();
            commandBufferClipped.name = "Clipped Layers";
            commandBufferUI = new CommandBuffer();
            commandBufferUI.name = "UI";
            foreach(var camera in cameras)
            {
                camera.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, commandBufferClipped);
                camera.AddCommandBuffer(CameraEvent.AfterImageEffectsOpaque, commandBufferUI);
            }

            //fixedCamera.AddCommandBuffer(CameraEvent.AfterImageEffectsOpaque, commandBufferUI);

        }


        int[] baseUIDs = layerManager.BaseLayersUID;

        if (baseUIDs.Length > 1)
        {
            // Hack to render the scene-wide layer stack always last: exchange first and last stacks (first stack is the scene-wide stack)
            (baseUIDs[0], baseUIDs[baseUIDs.Length - 1]) = (baseUIDs[baseUIDs.Length - 1], baseUIDs[0]);
        }


        commandBufferClipped.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);

        // Temp RT used to Blit the main render target to, so that we can use it as a texture
        int RT_main_copy_id = Shader.PropertyToID("_RT_main_copy");
        commandBufferClipped.GetTemporaryRT(RT_main_copy_id, -1, -1, 0);
        RenderTargetIdentifier RT_main_identifier = new RenderTargetIdentifier(RT_main_copy_id);

        // Render all layer' s strokes first then composite (goal: better layer-wise opacity control, layer opacity masks, etc)
        // - Create a temporary RT that shares the same depth map and same stencil buffer than CameraTarget
        // - For each clipped layer
        //  - render all strokes to temporary RT
        //  - Blit that temporary RT to CameraTarget with a special shader

        int RT_per_layer_id = Shader.PropertyToID("_RT_per_layer");
        commandBufferClipped.GetTemporaryRT(RT_per_layer_id, -1, -1, 16);
        RenderTargetIdentifier RT_per_layer_identifier = new RenderTargetIdentifier(RT_per_layer_id);

        // TODO: render layer only if the gameobject is active (convenient for debug vis)

        foreach (int uid in baseUIDs)
        {
            List<ClippedLayer> stack = layerManager.GetStack(uid);
            foreach (ClippedLayer topLayer in stack)
            {
                if (!topLayer.Visible || !topLayer.Enabled)
                    continue;

                //Debug.Log("rendering layer " + topLayer.LayerName);

                commandBufferClipped.SetRenderTarget(RT_per_layer_identifier, BuiltinRenderTextureType.CameraTarget);
                commandBufferClipped.ClearRenderTarget(false, true, Color.clear);

                // - Select children strokes
                //MeshFilter[] topLayerMeshes = topLayer.GetComponentsInChildren<MeshFilter>();
                Primitive[] topLayerMeshes = topLayer.GetComponentsInChildren<Primitive>();

                // - Render meshes to RT_base
                foreach (Primitive primitive in topLayerMeshes)
                {
                    Mesh mesh = primitive.GetMesh();
                    // If mesh is not initialized yet, don't attempt to render
                    if (mesh != null)
                    {
                        //Debug.Log("rendering top layer mesh...");
                        Matrix4x4 objectMatrix = primitive.transform.localToWorldMatrix;

                        // Set depth threshold, since it can change depending on canvas scale...
                        topLayer.CustomRenderMaterial.SetFloat(Shader.PropertyToID("_DepthThreshold"), topLayer.WorldSpaceDepthThreshold);

                        // Render final stroke
                        commandBufferClipped.DrawMesh(mesh, objectMatrix, topLayer.CustomRenderMaterial);

                    }
                }


                // TODO: a mask layer containing strokes could be rendered as a greyscale image with the same process as above in a dedicated RT (RT_mask_per_layer)
                // then blited with RT_per_layer with a shader that sets the alpha channel of the result based on the value in RT_mask_per_layer


                //// Apply per-layer texture/effects
                //if (topLayer.EffectMaterial != null)
                //{
                //    // Create temp RT to blit to

                //    commandBufferClipped.Blit(RT_per_layer_identifier, RT_per_layer_effects_identifier, topLayer.EffectMaterial);

                //}

                // Render mask layer primitives
                if (topLayer.Mask != null)
                {
                    // - Select children strokes
                    Primitive[] mastLayerMeshes = topLayer.Mask.GetComponentsInChildren<Primitive>();

                    // - Render meshes to RT_base
                    foreach (Primitive primitive in topLayerMeshes)
                    {
                        Mesh mesh = primitive.GetMesh();
                        // If mesh is not initialized yet, don't attempt to render
                        if (mesh != null)
                        {
                            Matrix4x4 objectMatrix = primitive.transform.localToWorldMatrix;
                            commandBufferClipped.DrawMesh(mesh, objectMatrix, topLayer.Mask.CustomRenderMaterial);

                        }
                    }
                }

                // Copy main render target (everything rendered before this layer) to the temporary RT
                commandBufferClipped.Blit(BuiltinRenderTextureType.CameraTarget, RT_main_identifier);
                commandBufferClipped.SetGlobalTexture("_BaseLayerColorTex", RT_main_identifier);
                //commandBufferClipped.SetGlobalTexture("_LayerContentTex", RT_per_layer_identifier);

                if (topLayer.CompositingMaterial)
                {
                    topLayer.CompositingMaterial.SetMatrix("_CanvasObjectToWorldMatrix", topLayer.gameObject.transform.localToWorldMatrix);
                    topLayer.CompositingMaterial.SetMatrix("_CanvasWorldToObjectMatrix", topLayer.gameObject.transform.worldToLocalMatrix);
                    topLayer.CompositingMaterial.SetFloat("_CanvasScale", topLayer.gameObject.transform.lossyScale.x);
                }

                commandBufferClipped.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);

                if (topLayer.debugFocus)
                {
                    commandBufferClipped.ClearRenderTarget(false, true, Color.clear);
                }

                commandBufferClipped.Blit(RT_per_layer_identifier, BuiltinRenderTextureType.CameraTarget, topLayer.CompositingMaterial);

                if (topLayer.debugHighlight)
                {

                    foreach (Primitive s in topLayerMeshes)
                    {
                        MeshFilter mesh = s.GetComponent<MeshFilter>();
                        Matrix4x4 objectMatrix = mesh.transform.localToWorldMatrix;
                        commandBufferUI.DrawMesh(mesh.sharedMesh, objectMatrix, selectedStrokeMaterial);
                    }
                }

                //if (layerManager.IsActive(topLayer))
                //{
                //    commandBufferUI.Blit(RT_per_layer_identifier, BuiltinRenderTextureType.CameraTarget, UIHiglightMaterial);
                //}

            }

        }

        // UI visuals
        commandBufferUI.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
        // TODO: only render selection if "selector" tool is active
        if (layerManager.ActiveLayer != null)
        {
            Primitive[] selection = layerManager.ActiveLayer.GetSelection();

            foreach (Primitive s in selection)
            {
                MeshFilter mesh = s.GetComponent<MeshFilter>();
                Matrix4x4 objectMatrix = mesh.transform.localToWorldMatrix;
                commandBufferUI.DrawMesh(mesh.sharedMesh, objectMatrix, selectedStrokeMaterial);
            }
        }

        Layer layerToHighlight = null;

        if (layerManager.HoveredLayer != null)
            layerToHighlight = layerManager.HoveredLayer;

        // Highlight button (Y button)
        if (highlightCurrentActiveAction.action.IsPressed() && layerManager.ActiveLayer != null)
            layerToHighlight = layerManager.ActiveLayer;

        if (layerToHighlight != null)
        {
            Primitive[] selection = layerToHighlight.GetComponentsInChildren<Primitive>();
            //Debug.Log("hovering layer " + layerManager.HoveredLayer);

            foreach (Primitive s in selection)
            {
                MeshFilter mesh = s.GetComponent<MeshFilter>();
                Matrix4x4 objectMatrix = mesh.transform.localToWorldMatrix;
                commandBufferUI.DrawMesh(mesh.sharedMesh, objectMatrix, hoveredLayerMaterial);
            }
        }

        //if (needsToRenderFrame)
        //    frameToRenderWasDrawn = true;


    }

    private void ExportBufferToPNG(string filePath)
    {

        Debug.Log("rendering frame at: " + filePath);

        // Create a texture the size of the screen, RGB24 format
        int width = Screen.width;
        int height = Screen.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        //Texture2D tex = new Texture2D(width, height);

        // Read screen contents into the texture
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        // Encode texture into PNG
        byte[] bytes = ImageConversion.EncodeToPNG(tex);
        UnityEngine.Object.Destroy(tex);

        File.WriteAllBytes(filePath, bytes);

        //frameToRenderWasDrawn = false;
    }


    public void RenderFrame()
    {

        needsToRenderFrame = true;
        //frameToRenderWasDrawn = false;

    }

    public void RenderAnimationFrame(string folderName, int frameIndex)
    {
        overrideFileName = $"{frameIndex.ToString("D4")}.png";
        overrideFrameExportFolder = Path.Combine("Frames~", folderName);
        RenderFrame();
    }

    public void RenderAnimationFrame(string folderName, string frameName)
    {
        overrideFileName = $"{frameName}.png";
        overrideFrameExportFolder = Path.Combine("Frames~", folderName);
        RenderFrame();
    }
}

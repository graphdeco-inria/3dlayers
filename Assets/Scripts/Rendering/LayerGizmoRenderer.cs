using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public class LayerGizmoRenderer : MonoBehaviour
{
    public Camera fixedCamera;
    private LayerManager layerManager;

    private CommandBuffer gizmoCommandBuffer;
    private Material highlightActiveMaterial;
    private Material blendingMaterial;

    // Start is called before the first frame update
    void Start()
    {
        layerManager = GetComponent<LayerManager>();
        highlightActiveMaterial = new Material(Shader.Find("VRPaint/Highlight"));
        blendingMaterial = new Material(Shader.Find("VRPaint/BlendingDefault"));

        blendingMaterial.SetFloat(Shader.PropertyToID("_LayerOpacity"), 0.5f);
        blendingMaterial.SetInt(Shader.PropertyToID("_ColorMixMode"), (int)ColorBlendMode.Normal);
    }

    public void OnDisable()
    {

        if (gizmoCommandBuffer != null && fixedCamera)
            fixedCamera.RemoveCommandBuffer(CameraEvent.AfterImageEffectsOpaque, gizmoCommandBuffer);

    }

    public void OnWillRenderObject()
    {
        //Debug.Log("rendering...");
        var act = gameObject.activeInHierarchy && enabled;
        if (!act)
        {
            OnDisable();
            return;
        }


        if (gizmoCommandBuffer != null)
        {
            gizmoCommandBuffer.Clear();
        }
        else
        {
            gizmoCommandBuffer = new CommandBuffer();
            gizmoCommandBuffer.name = "Layers Gizmo";

            fixedCamera.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, gizmoCommandBuffer);

        }

        if (layerManager.ActiveLayer != null)
        {
            gizmoCommandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);

            int RT_active_layer = Shader.PropertyToID("_RT_active_layer");
            gizmoCommandBuffer.GetTemporaryRT(RT_active_layer, -1, -1, 16);
            RenderTargetIdentifier RT_active_layer_identifier = new RenderTargetIdentifier(RT_active_layer);

            gizmoCommandBuffer.SetRenderTarget(RT_active_layer_identifier, BuiltinRenderTextureType.CameraTarget);
            gizmoCommandBuffer.ClearRenderTarget(false, true, Color.clear);

            // Render content of the active layer in transparent white, over everything
            // - Select children meshes
            MeshFilter[] topLayerMeshes = layerManager.ActiveLayer.GetComponentsInChildren<MeshFilter>();

            // - Render meshes to RT_base
            foreach (MeshFilter mesh in topLayerMeshes)
            {
                // If mesh is not initialized yet, don't attempt to render
                if (mesh.sharedMesh != null)
                {
                    //Debug.Log("rendering top layer mesh...");
                    Matrix4x4 objectMatrix = mesh.transform.localToWorldMatrix;

                    // Render final stroke
                    gizmoCommandBuffer.DrawMesh(mesh.sharedMesh, objectMatrix, highlightActiveMaterial);

                }
            }

            gizmoCommandBuffer.Blit(RT_active_layer_identifier, BuiltinRenderTextureType.CameraTarget, blendingMaterial);


        }


    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorBlendMode
{
    Normal,
    Multiply,
    Screen,
    Overlay
}

//public enum CompositingMode
//{
//    Intersect,
//    Over,
//}

public enum MaskMode
{
    Off,
    Gradient,
}

public class ClippedLayer : Layer
{

    private const int STENCIL_TEST_INCREMENT_INTERSECT = 192;
    private const int STENCIL_TEST_INCREMENT_PERMISSIVE = 64;


    public bool debugHighlight;

    public bool debugFocus;

    private Material _customRenderMaterial;
    private Material _customRenderMaterialPermissive;
    public Material CustomRenderMaterial { 
        get
        {
            if (debugHighlight)
                return debugOverrideMaterial;
            else
                return DepthThreshold == 0 ? _customRenderMaterial : _customRenderMaterialPermissive;
        }
    }

    private Material _compositingMaterial;
    public Material CompositingMaterial 
    { 
        get 
        { 
            return (debugHighlight || debugFocus) ? null : _compositingMaterial; 
        }
        private set
        {
            _compositingMaterial = value;
            _compositingMaterial.SetInt(Shader.PropertyToID("_ColorMixMode"), (int)Blend);
            _compositingMaterial.SetFloat(Shader.PropertyToID("_LayerOpacity"), Opacity);
        }
    }

    private Material blendingDefaultMat;
    private Material blendingGradientMat;
    private Material blendingTextureMat;

    private Material debugOverrideMaterial;

    private int _clippingBaseUID = -1;
    public int clippingBaseUID
    {
        get { return _clippingBaseUID; }
        set
        {
            _clippingBaseUID = value;
            // - Set shader property about the base group ID
            if (this._clippingBaseUID != -1)
            {
                //Debug.Log("setting clipping base UID " + _clippingBaseUID);
                this._customRenderMaterial.SetInt(Shader.PropertyToID("_GroupIDIntersect"), IntersectStencilRef);
                this._customRenderMaterial.SetInt(Shader.PropertyToID("_GroupIDPermissive"), PermissiveStencilRef);
                this._customRenderMaterial.SetInt(Shader.PropertyToID("_StencilComparison"), StencilComparison);

                this._customRenderMaterialPermissive.SetInt(Shader.PropertyToID("_GroupIDIntersect"), IntersectStencilRef);
                this._customRenderMaterialPermissive.SetInt(Shader.PropertyToID("_GroupIDPermissive"), PermissiveStencilRef);
                this._customRenderMaterialPermissive.SetInt(Shader.PropertyToID("_StencilComparison"), StencilComparison);

            }
        }
    }

    public ColorBlendMode Blend
    {
        get 
        {
            return _blendMode; 
        }
        set
        {
            _blendMode = value;
            this.CompositingMaterial.SetInt(Shader.PropertyToID("_ColorMixMode"), (int)_blendMode);
        }
    }
    private ColorBlendMode _blendMode;

    private float _depthThreshold = 0f;
    public float DepthThreshold
    {
        get { return _depthThreshold; }
        set
        {
            _depthThreshold = value;
            this._customRenderMaterialPermissive.SetFloat(Shader.PropertyToID("_DepthThreshold"), WorldSpaceDepthThreshold);
        }
    }

    public float WorldSpaceDepthThreshold
    {
        get { return _depthThreshold * gameObject.transform.lossyScale.x; }
    }

    public int StencilComparison
    {
        get
        {
            // Base ID == 0 => scene-wide stack => Less, else Equal
            return this._clippingBaseUID == 0 ? 2 : 3;
        }
    }

    public MaskMode Masking
    {
        get { return _maskingMode; }
        set
        {
            _maskingMode = value;
            switch(_maskingMode)
            {
                case MaskMode.Off:
                    this.CompositingMaterial = blendingDefaultMat;
                    break;
                case MaskMode.Gradient:
                    this.CompositingMaterial = gradientMask != null ? gradientMask.BlendingMaterial : blendingDefaultMat;
                    break;
                default:
                    this.CompositingMaterial = blendingDefaultMat;
                    break;
            }
        }
    }
    private MaskMode _maskingMode;

    public ClippedLayer Mask;

    private LayerGradientMask gradientMask;
    private LayerTextureMask textureMask;

    private float _opacity = 0f;
    public float Opacity
    {
        get { return _opacity; }
        set { 
            _opacity = value;
            this.CompositingMaterial.SetFloat(Shader.PropertyToID("_LayerOpacity"), _opacity);
        }
    }

    public int IntersectStencilRef
    {
        get { return _clippingBaseUID + STENCIL_TEST_INCREMENT_INTERSECT; }
    }

    public int PermissiveStencilRef
    {
        get { return _clippingBaseUID + STENCIL_TEST_INCREMENT_PERMISSIVE; }
    }

    private void Awake()
    {
        blendingDefaultMat = new Material(Shader.Find("VRPaint/BlendingDefault"));
        blendingGradientMat = new Material(Shader.Find("VRPaint/BlendingGradient"));
        blendingTextureMat = new Material(Shader.Find("VRPaint/BlendingTexture"));
        this.CompositingMaterial = blendingDefaultMat;

        this.ForwardRenderMaterial = new Material(Shader.Find("VRPaint/DoNotRenderInForwardPass"));
        this._customRenderMaterial = new Material(Shader.Find("VRPaint/ClippedLayerShader"));
        this._customRenderMaterialPermissive = new Material(Shader.Find("VRPaint/ClippedLayerPermissiveShader"));

        this.debugOverrideMaterial = new Material(Shader.Find("VRPaint/DebugHighlight"));

        this.Opacity = 1f;
        this.Blend = ColorBlendMode.Normal;
    }

    public void Init(string name, int UID, int baseLayerUID)
    {
        base.Init(name, UID);
        clippingBaseUID = baseLayerUID;
    }

    public override void Init(SerializableLayer layerData)
    {
        base.Init(layerData);
        clippingBaseUID = layerData.clippingBaseUID;
        SetParameters(layerData.parameters);
    }

    public override LayerParameters GetParameters()
    {
        LayerParameters parameters = base.GetParameters();
        parameters.blendMode = Blend;
        parameters.opacity = Opacity;
        if (gradientMask != null)
            parameters.gradientMask = new SerializableGradientMask(gradientMask);
        parameters.maskMode = Masking;
        parameters.depthThreshold = DepthThreshold;
        return parameters;
    }

    public override void SetParameters(LayerParameters parameters)
    {
        base.SetParameters(parameters);
        Blend = parameters.blendMode;
        Opacity = parameters.opacity;

        if (parameters.gradientMask != null)
        {
            gradientMask = new LayerGradientMask(blendingGradientMat, parameters.gradientMask);
        }
        Masking = parameters.maskMode;

        DepthThreshold = parameters.depthThreshold;
    }

    public void DebugIncrementBlendMode(int sign)
    {
        ColorBlendMode newMode = (ColorBlendMode)(((int)this.Blend + sign * 1) % 4);
        Blend = newMode;
    }


    public void AddGradientMask(Vector3 A, Vector3 B, GradientMaskType type, int gradientDirection)
    {
        // We assume that the user wants to turn the gradient mask on
        this.gradientMask = new LayerGradientMask(blendingGradientMat, A, B, type, gradientDirection == 0 ? 1f : 0f, gradientDirection == 0 ? 0f : 1f);
        this.Masking = MaskMode.Gradient;
    }

    public void AddTextureMask(Vector3 scale, float min, float max)
    {
        this.textureMask = new LayerTextureMask(blendingTextureMat, scale, min, max);
    }

    public override bool InStack(int baseUID)
    {
        return baseUID == clippingBaseUID;
    }

    public override SerializableLayer Serialize()
    {
        SerializableLayer data = base.Serialize();
        data.clippingBaseUID = clippingBaseUID;
        data.parameters.blendMode = Blend;
        data.parameters.maskMode = Masking;
        data.parameters.opacity = Opacity;
        data.parameters.depthThreshold = DepthThreshold;

        if (gradientMask != null)
            data.parameters.gradientMask = new SerializableGradientMask(gradientMask);

        return data;
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

public class BaseLayer : Layer
{

    public bool HideColor;

    public override int UID
    {
        get
        {
            return _UID;
        }
        protected set
        {
            _UID = value;
            //Debug.Log("called BaseLayer set");
            if (this.ForwardRenderMaterial)
            {
                //Debug.Log("set uniform to " + _UID);
                // - Set shader property about the base group ID
                this.ForwardRenderMaterial.SetInt(Shader.PropertyToID("_GroupID"), _UID);
            }

        }
    }

    // Convention: Scene stack => UID == 0 => base layer is non-editable
    public bool Editable
    {
        get { return UID > 0; }
    }

    //private LayerGizmo layerGizmo;


    private void Awake()
    {
        this.ForwardRenderMaterial = new Material(Shader.Find("VRPaint/BaseLayerShader"));
    }


    public override SerializableLayer Serialize()
    {
        SerializableLayer data = base.Serialize();
        data.clippingBaseUID = -1; // no clipping base

        return data;
    }


    public Vector3 GetGizmoPosition()
    {
        MeshCollider[] children = gameObject.GetComponentsInChildren<MeshCollider>();
        if (children.Length > 0)
        {
            var bounds = children[0].bounds;
            for (var i = 1; i < children.Length; ++i)
                bounds.Encapsulate(children[i].bounds);

            //Vector3 gizmoPos = transform.InverseTransformPoint(bounds.max);

            return bounds.center; // Position in world space
        }
        else
            return Vector3.zero;
    }


}

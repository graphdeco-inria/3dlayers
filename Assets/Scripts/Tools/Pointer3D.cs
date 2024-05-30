using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Pointer3D : MonoBehaviour
{
    public static event Action<Primitive> OnPrimitiveCollide;
    //public static event Action<LayerGizmo> OnLayerGizmoCollide;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {

        Primitive primitive = other.GetComponent<Primitive>();
        if (primitive != null)
        {
            //Debug.Log("pointer 3D collided with " + other.name);
            OnPrimitiveCollide?.Invoke(primitive);
        }

        LayerStackZone gizmo = other.GetComponent<LayerStackZone>();
        if (gizmo != null)
        {
            //Debug.Log("pointer 3D collided with " + other.name);
            gizmo.OnCollide();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        LayerStackZone gizmo = other.GetComponent<LayerStackZone>();
        if (gizmo != null)
        {
            gizmo.OnExit();
        }
    }

    public Collider[] QueryOverlap()
    {
        // Return a list of colliders that fall in the pointer's collider sphere
        Vector3 center = transform.position;
        float radius = transform.lossyScale.x * 0.5f;
        Collider[] hitColliders = Physics.OverlapSphere(center, radius);
        //foreach (var hitCollider in hitColliders)
        //{
        //    if (hitCollider.GetComponent<Stroke>() != null)
        //    {
        //        Debug.Log("overlaps stroke " + hitCollider.GetComponent<Stroke>().UID);
        //    }
        //}
        return hitColliders;
    }


    public bool OverlapsASelection()
    {
        foreach (Collider c in QueryOverlap())
        {
            if (c.GetComponent<Primitive>() != null && c.GetComponent<Primitive>().selected)
                return true;
        }

        return false;
    }

}

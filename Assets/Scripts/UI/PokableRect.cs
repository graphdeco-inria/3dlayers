using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class PokableRect : MonoBehaviour
{

    public float PokableZoneDepth = 0.8f;
    public bool IsPokeZone = false;
    public float OvershootMargin = 5f;


    public void UpdateCollider()
    {


        // Force update layout
        Canvas.ForceUpdateCanvases();

        //if (name == "BoxSlider")
        //    Debug.Log("on validate " + name);
        if (IsPokeZone)
        {
            PokableZoneDepth = 30f;
        }

        // Create a BoxCollider component with the same dimensions as the Rect
        BoxCollider collider = GetComponent<BoxCollider>();
        if (collider == null)
            collider = gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;

        // Create a Rigidbody component
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        UpdateColliderDimensions();

    }

    private void Start()
    {
        if (IsPokeZone)
        {
            tag = "PokeZone";
        }
        else if (tag == "PokeZone")
        {
            tag = "Untagged";
        }
        //UpdateColliderDimensions();
    }

    private void UpdateColliderDimensions()
    {
        BoxCollider collider = GetComponent<BoxCollider>();
        RectTransform rect = GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        rect.GetLocalCorners(corners);
        //if (name == "BoxSlider")
        //{
        //    Debug.Log("corners " + name);
        //    foreach (var c in corners)
        //    {
        //        Debug.Log(c);
        //    }
        //}


        // Set BoxCollider dimensions
        collider.size = new Vector3(Mathf.Abs(corners[2].x - corners[0].x), Mathf.Abs(corners[2].y - corners[0].y), PokableZoneDepth + OvershootMargin);
        collider.center = new Vector3(0.5f * (corners[2].x + corners[0].x), 0.5f * (corners[2].y + corners[0].y), -PokableZoneDepth * 0.5f + OvershootMargin * 0.5f);

    }


}

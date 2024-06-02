using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum Handedness {
    LeftHanded,
    RightHanded
}

public class HandednessManager : MonoBehaviour
{
    public Handedness handedness;

    public InputActionAsset inputActionMapping;

    public GameObject rightHandedSetup;
    public GameObject leftHandedSetup;

    public LayerRenderer layerRenderer;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("set handedness");
        if (handedness == Handedness.LeftHanded)
        {
            rightHandedSetup.SetActive(false);
            leftHandedSetup.SetActive(true);

            layerRenderer.highlightCurrentActiveAction = inputActionMapping.FindActionMap("XRI RightHand Interaction").FindAction("Show Current State"); 
        }
        else
        {
            rightHandedSetup.SetActive(true);
            leftHandedSetup.SetActive(false);
            layerRenderer.highlightCurrentActiveAction = inputActionMapping.FindActionMap("XRI LeftHand Interaction").FindAction("Show Current State");

        }
    }


}

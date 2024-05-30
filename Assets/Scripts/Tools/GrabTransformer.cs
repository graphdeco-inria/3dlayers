using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GrabFocus
{
    Canvas,
    Selection
}

public enum SwitchLayerDirection
{
    Down, Up
    //Right, Left
}

// Bug in earlier versions: grabinfo transforms were defined in world space and not in canvas space, so all logs saved before Dec 11 basically have wrong transform data for grab actions (since canvas transform is not identity in the general case and it is not logged anywhere)
[Serializable]
public struct GrabInfo
{
    public SerializableVector3 startPos;
    public SerializableVector3 endPos;
    public SerializableQuaternion startRot;
    public SerializableQuaternion endRot;

    public void InverseTransform(Transform transform)
    {
        //transform.rotation = Quaternion.Inverse(endRot * Quaternion.Inverse(startRot)) * transform.rotation;
        //transform.position = startPos + Quaternion.Inverse(endRot * Quaternion.Inverse(startRot)) * (transform.position - endPos);
        transform.localRotation = Quaternion.Inverse(endRot * Quaternion.Inverse(startRot)) * transform.localRotation;
        transform.localPosition = startPos + Quaternion.Inverse(endRot * Quaternion.Inverse(startRot)) * (transform.localPosition - endPos);
    }

    public void ForwardTransform(Transform transform)
    {
        //transform.rotation = endRot * Quaternion.Inverse(startRot) * transform.rotation;
        //Vector3 correctedOffset = endRot * Quaternion.Inverse(startRot) * (transform.position - startPos);
        //transform.position = endPos + correctedOffset;
        transform.localRotation = endRot * Quaternion.Inverse(startRot) * transform.localRotation;
        Vector3 correctedOffset = endRot * Quaternion.Inverse(startRot) * (transform.localPosition - startPos);
        transform.localPosition = endPos + correctedOffset;
    }
}

public struct GrabbedObject
{
    public GameObject obj;
    Vector3 initialPosition;
    Quaternion initialRotation;

    Vector3 initialGrabPosition;
    Quaternion initialGrabRotation;

    public GrabbedObject(GameObject obj, Vector3 p, Quaternion q, Vector3 handPos, Quaternion handRot)
    {
        this.obj = obj;
        initialPosition = p;
        initialRotation = q;
        initialGrabPosition = handPos;
        initialGrabRotation = handRot;
    }

    public void GrabTo(Vector3 handPos, Quaternion handRot)
    {
        obj.transform.rotation = handRot * Quaternion.Inverse(initialGrabRotation) * initialRotation;
        Vector3 correctedOffset = handRot * Quaternion.Inverse(initialGrabRotation) * (initialPosition - initialGrabPosition);
        obj.transform.position = handPos + correctedOffset;
    }

}

public class GrabTransformer : MonoBehaviour, ITool
{

    [Header("XR Input Actions")]
    public InputActionProperty leftHandPosition;
    public InputActionProperty leftHandRotation;

    public InputActionProperty rightHandPosition;
    public InputActionProperty rightHandRotation;

    public InputActionProperty leftGrabAction;
    public InputActionProperty rightGrabAction;

    public InputActionProperty altButtonAction;

    public InputActionProperty layerSwitchAction;

    [Header("Canvas")]
    public PaintCanvas paintCanvas;

    public Selector selector;

    public Pointer3D pointer;

    private GrabFocus focus = GrabFocus.Canvas;

    private bool isGrabbing = false;
    private InputAction grabbingHandPositionAction;
    private InputAction grabbingHandRotationAction;

    private bool isScaling = false;

    private GrabbedObject[] grabbedObjects;

    // Grabbing persistent data
    private Vector3 startGrabPosition;
    private Quaternion startGrabRotation;
    //private Vector3 positionOffset;
    private Quaternion q0;
    //private Quaternion q0, qObj;

    // Scaling persistent data
    private float startHandsDistance;
    private float startScale;

    private float layerSwitchActionLastPerformed = 0f;
    private const float LAYER_SWITCH_COOLDOWN = 1f;

    private ToolsManager manager;

    public Tool GetToolType()
    {
        return Tool.GrabTransformer;
    }

    public void SetManager(ToolsManager manager)
    {
        this.manager = manager;
    }

    // Start is called before the first frame update
    void Start()
    {
        leftGrabAction.action.started += ctx => { InitiateGrab(); };
        leftGrabAction.action.canceled += ctx => { ReleaseGrab(); };

        rightGrabAction.action.started += ctx => { InitiateGrab(); };
        rightGrabAction.action.canceled += ctx => { ReleaseGrab(); };

    }

    private void InitiateGrab()
    {

        // Test which hand(s) grab(s): if 2 hands grab => scale ; else => move/rotate
        if (leftGrabAction.action.IsPressed() && rightGrabAction.action.IsPressed())
        {
            InitiateScale();
        }
        else
        {
            // Actually starting a grab action
            if (!manager.CanUseTool())
                return;

            manager.MarkBusy();

            grabbingHandPositionAction = leftGrabAction.action.IsPressed() ? leftHandPosition.action : rightHandPosition.action;
            grabbingHandRotationAction = leftGrabAction.action.IsPressed() ? leftHandRotation.action : rightHandRotation.action;
            startGrabPosition = grabbingHandPositionAction.ReadValue<Vector3>();
            startGrabRotation = grabbingHandRotationAction.ReadValue<Quaternion>();

            // Figure out if we're grabbing a selection
            Primitive[] grabbedStrokes = new Primitive[0];

            if (pointer.OverlapsASelection())
                grabbedStrokes = selector.CurrentSelection();

            if (grabbedStrokes.Length > 0)
            { 
                // Figure out if we're pressing Alt => duplicate selection
                if (altButtonAction.action.IsPressed())
                {
                    Debug.Log("duplicate selection");
                    PrimitiveCopy[] copies = new PrimitiveCopy[grabbedStrokes.Length];
                    for(int i = 0; i < grabbedStrokes.Length; i++)
                    {
                        copies[i] = paintCanvas.Duplicate(grabbedStrokes[i]);
                        grabbedStrokes[i].Unselect();
                        copies[i].Select();
                    }
                    grabbedStrokes = copies;

                    // Register Duplicate Action in session history
                    SessionHistory.Instance.RegisterAction(
                        new DuplicateAction(copies)
                    );

                }
                Debug.Log("transform selection...");
                grabbedObjects = new GrabbedObject[grabbedStrokes.Length];
                for (int i = 0; i <grabbedStrokes.Length; i++)
                {
                    grabbedObjects[i] = new GrabbedObject(grabbedStrokes[i].gameObject, grabbedStrokes[i].transform.position, grabbedStrokes[i].transform.rotation, startGrabPosition, startGrabRotation);
                }

                focus = GrabFocus.Selection;
            }
            else
            {
                grabbedObjects = new GrabbedObject[] { new GrabbedObject(paintCanvas.gameObject, paintCanvas.transform.position, paintCanvas.transform.rotation, startGrabPosition, startGrabRotation) };
                focus = GrabFocus.Canvas;
            }



            //positionOffset = paintCanvas.transform.position - startGrabPosition;
            q0 = Quaternion.Inverse(startGrabRotation);
            //qObj = paintCanvas.transform.rotation;

            isGrabbing = true;
        }
    }

    private void InitiateScale()
    {
        if (!isGrabbing && !manager.CanUseTool())
            return;

        manager.MarkBusy();

        isGrabbing = false;
        isScaling = true;

        float handsDistance = Vector3.Distance(rightHandPosition.action.ReadValue<Vector3>(), leftHandPosition.action.ReadValue<Vector3>());
        startHandsDistance = handsDistance;
        startScale = paintCanvas.transform.localScale.x;
    }

    private void ReleaseGrab()
    {
        manager.MarkIdle();

        // If there is still a grabbing hand, go back to grab
        if (leftGrabAction.action.IsPressed() || rightGrabAction.action.IsPressed())
        {

            isScaling = false;
            InitiateGrab();
            return;
        }
        else
        {
            isGrabbing = false;
            isScaling = false;

            // If we transformed strokes, then register this action in action stack
            Primitive[] grabbed;
            if (grabbedObjects != null && GetGrabbedPrimitives(out grabbed))
            {
                GrabInfo grabInfo = new GrabInfo();
                //grabInfo.startPos = startGrabPosition;
                //grabInfo.startRot = startGrabRotation;
                //grabInfo.endPos = grabbingHandPositionAction.ReadValue<Vector3>();
                //grabInfo.endRot = grabbingHandRotationAction.ReadValue<Quaternion>();

                grabInfo.startPos = paintCanvas.transform.InverseTransformPoint(startGrabPosition);
                grabInfo.startRot = Quaternion.Inverse(paintCanvas.transform.rotation) * startGrabRotation;
                grabInfo.endPos = paintCanvas.transform.InverseTransformPoint(grabbingHandPositionAction.ReadValue<Vector3>());
                grabInfo.endRot = Quaternion.Inverse(paintCanvas.transform.rotation) * grabbingHandRotationAction.ReadValue<Quaternion>();



                // Also trigger update of layer gizmo => all primitives are supposed to belong to the same layer, so one call of the update method is enough
                //foreach(Primitive p in grabbed)
                //{
                //grabbed[0].TriggerLayerGizmoUpdate();
                //}
                paintCanvas.UpdateActiveLayer();

                SessionHistory.Instance.RegisterAction(
                    new TransformAction(grabbed, grabInfo)
                );


                // Should we switch primitives to another layer?
                //Collider[] collided = pointer.QueryOverlap();

                //foreach (Collider c in collided)
                //{
                //    LayerStackZone layerGizmo = c.GetComponent<LayerStackZone>();
                //    if (layerGizmo != null)
                //    {
                //        // Switch to that layer
                //        bool switchSuccess = paintCanvas.SwitchToLayer(layerGizmo.GetStackBaseUID(), grabbed);
                //    }
                //}
            }
        }
    }

    private bool GetGrabbedPrimitives(out Primitive[] primitives)
    {

        primitives = new Primitive[grabbedObjects.Length];
        if (focus == GrabFocus.Selection && grabbedObjects.Length > 0)
        {
            for (int i = 0; i < grabbedObjects.Length; i++)
            {
                primitives[i] = grabbedObjects[i].obj.GetComponent<Primitive>();
            }
            return true;
        }
        else
            return false;
    }

    private void GrabUpdate(Vector3 handPos, Quaternion handRot)
    {
        if (Vector3.Distance(handPos, startGrabPosition) > 0.01f || Quaternion.Angle(startGrabRotation, handRot) > 0.1f)
        {
            //Vector3 endPosition = handPos;
            //Quaternion q1 = handRot;

            foreach(GrabbedObject obj in grabbedObjects)
            {
                //obj.transform.position = endPosition;
                //paintCanvas.transform.rotation = q1 * q0 * qObj;
                //Vector3 correctedOffset = q1 * q0 * positionOffset;
                //paintCanvas.transform.position += correctedOffset;
                obj.GrabTo(handPos, handRot);
            }

        }
    }

    private void ScaleUpdate(float handsDistance, Vector3 handsCenter)
    {
        float newScale = startScale * handsDistance / startHandsDistance;
        Vector3 originalPos = paintCanvas.transform.localPosition;
        Vector3 translation = originalPos - handsCenter;

        float RS = newScale / paintCanvas.transform.localScale.x; // relative scale factor

        // calc final position post-scale
        Vector3 FP = handsCenter + translation * RS;

        // finally, actually perform the scale/translation
        paintCanvas.transform.localScale = new Vector3(newScale, newScale, newScale);
        paintCanvas.transform.localPosition = FP;
    }

    // Update is called once per frame
    void Update()
    {
        if (isGrabbing)
        {
            GrabUpdate(grabbingHandPositionAction.ReadValue<Vector3>(), grabbingHandRotationAction.ReadValue<Quaternion>());

            // Listen for layer change actions
            //if (focus == GrabFocus.Selection && grabbedObjects.Length > 0 && Time.time - layerSwitchActionLastPerformed > LAYER_SWITCH_COOLDOWN)
            //{
            //    Vector2 thumbstick = layerSwitchAction.action.ReadValue<Vector2>();
            //    if (Mathf.Abs(thumbstick.y) > 0.9f)
            //    {
            //        SwitchLayerDirection dir = SwitchLayerDirection.Down;
            //        if (thumbstick.y < -0.9f)
            //        {
            //            Debug.Log("switch to below layer");
            //            dir = SwitchLayerDirection.Down;
            //        }
            //        else if (thumbstick.y > 0.9f)
            //        {
            //            dir = SwitchLayerDirection.Up;

            //        }

            //        Primitive[] grabbed;
            //        if (GetGrabbedPrimitives(out grabbed))
            //        {
            //            bool switchSuccess = paintCanvas.SwitchToLayer(dir, grabbed);
            //            if (switchSuccess)
            //                layerSwitchActionLastPerformed = Time.time;
            //        }

            //    }
            //}
        }

        if (isScaling)
        {
            float dist = Vector3.Distance(rightHandPosition.action.ReadValue<Vector3>(), leftHandPosition.action.ReadValue<Vector3>());
            Vector3 center = 0.5f * (rightHandPosition.action.ReadValue<Vector3>() + leftHandPosition.action.ReadValue<Vector3>());
            ScaleUpdate(dist, center);
        }
    }
}

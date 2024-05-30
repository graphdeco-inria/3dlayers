using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public enum ButtonState
{
    Idle = 0, Hover = 1, Clicking = 2, Clicked = 3
}


[RequireComponent(typeof(PokableRect))]
public class PokableButton : MonoBehaviour
{
    public string ButtonName;
    //public Material BaseButtonMaterial;

    public Color BaseColor = UIConstants.DEFAULT_COLOR;
    public Color HoverColor = Color.white;
    public Color ClickColor = UIConstants.ACTIVE_COLOR;

    public float IdleDepth = 1f;
    public float ClickedDepth = 0.1f;

    public float RestTime = 1f;

    private bool _active = true;
    public bool Active
    {
        get { return _active; }
        set
        {
            _active = value;
            if (!_active)
                State = ButtonState.Idle;
        }
    }

    public EventTrigger.TriggerEvent OnButtonClick;
    public EventTrigger.TriggerEvent OnButtonEnter;
    public EventTrigger.TriggerEvent OnButtonExit;

    private ButtonState _state = ButtonState.Idle;
    private UIPointer hoveringPointer = null;
    //private Vector3 triggerStartPos;
    private float lastClickedTime = 0f;

    private Animator animator;



    private ButtonState State
    {
        get { return _state; }
        set
        {
            _state = value;
            //Debug.Log(gameObject.name + " state = " + (int)_state);


            if (animator)
                animator.SetInteger("State", (int)_state);
            else
            {
                Background.color = (_state == ButtonState.Clicking) ? ClickColor : (_state == ButtonState.Hover) ? HoverColor : BaseColor;

                if (_state == ButtonState.Idle)
                {
                    float z = -IdleDepth;
                    transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, z);
                }

            }
            //animator.SetTrigger((_state == ButtonState.Click) ? "TrClick" : (_state == ButtonState.Hover) ? "TrHover" : "TrReset");
        }
    }

    private Image _background;
    private Image Background
    {
        get
        {
            if (_background == null)
                _background = GetComponent<Image>();
            return _background;
        }
    }

    // Start is called before the first frame update
    protected void OnEnable()
    {
        //image = GetComponent<Image>();
        animator = GetComponent<Animator>();
        //buttonLabel = 
        if (animator != null && !animator.enabled)
            animator = null;
        //Debug.Log(animator);
        //Debug.Log(buttonLabel);

        State = ButtonState.Idle;

    }

    // Update is called once per frame
    protected void Update()
    {
        //Debug.Log(hoveringPointer);
        //if (hoveringPointer == null)
        //    return;

        //float pressDistance = Vector3.Dot(hoveringPointer.transform.position - transform.position, transform.TransformDirection(Vector3.back));
        // If we are in hover mode, check for pokes
        if (Active && hoveringPointer && (State == ButtonState.Hover || State == ButtonState.Clicking ))
        {
            Vector3 localPointerPos = transform.parent.InverseTransformPoint(hoveringPointer.transform.position);
            //Debug.Log(-localPointerPos.z);

            if (-localPointerPos.z < IdleDepth )
            {
                // Are we clicking or already have reached the click?
                if (-localPointerPos.z <= ClickedDepth && Time.time - lastClickedTime > RestTime)
                {
                    //Debug.Log("click");
                    //hoveringPointer.OnButtonClick(this);

                    State = ButtonState.Clicked;
                    lastClickedTime = Time.time;
                    //Debug.Log("clicked");
                    ResetButton();

                    // Trigger vibration on UI pointer controller
                    hoveringPointer.OnPoke();

                    // Call callback
                    BaseEventData eventData = new BaseEventData(EventSystem.current);
                    eventData.selectedObject = gameObject;
                    OnButtonClick.Invoke(eventData);
                }
                else
                {
                    //Debug.Log("clicking");
                    State = ButtonState.Clicking;
                    float z = -localPointerPos.z;
                    transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, z);
                }


            }

        }

    }






    public void SetColor(Color color)
    {
        // Set Background color
        Background.color = color;
        BaseColor = color;
    }

    public void TriggerColliderUpdate()
    {
        GetComponent<PokableRect>().UpdateCollider();
    }

    private void ResetButton()
    {
        if (animator)
        {
            //Debug.Log(animator.IsInTransition(0));
            // Change state based on animator state
            if (!animator.IsInTransition(0))
            {
                State = ButtonState.Idle;
            }
        }
        else
        {
            State = ButtonState.Idle;
            //triggerStartPos =

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.gameObject.GetComponent<UIPointer>());
        UIPointer pointer = other.gameObject.GetComponent<UIPointer>();
        if (pointer != null && !pointer.Busy && Active)
        {
            // Make sure we're poking it from the front
            if (Vector3.Dot(pointer.transform.forward, transform.forward) > 0)
            {
                //Debug.Log(other.name + " entered button collider " + name);
                // Trigger button hover state
                hoveringPointer = pointer;
                //image.color = HoverColor;
                State = ButtonState.Hover;
                BaseEventData eventData = new BaseEventData(EventSystem.current);
                OnButtonEnter.Invoke(eventData);
                //Debug.Log("start hover");

                //triggerStartPos = transform.parent.InverseTransformPoint(hoveringPointer.transform.position);
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (Active && hoveringPointer != null && other.gameObject.GetComponent<UIPointer>() == hoveringPointer)
        {
            //Debug.Log("exited button collider " + name);
            ResetButton();
            // Trigger button hover state
            //image.color = BaseColor;
            hoveringPointer = null;
            BaseEventData eventData = new BaseEventData(EventSystem.current);
            OnButtonExit.Invoke(eventData);

            //transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -IdleDepth);
        }
    }

    private void OnDisable()
    {
        State = ButtonState.Idle;
    }


}

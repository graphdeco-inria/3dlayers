using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;




[RequireComponent(typeof(PokableRect))]
public class PokableBoxSlider : MonoBehaviour
{

    [Serializable]
    public class BoxSliderEvent : UnityEvent<float, float> { }

    [SerializeField]
    private RectTransform m_HandleRect;
    public RectTransform handleRect { get { return m_HandleRect; } set { if (SetClass(ref m_HandleRect, value)) { UpdateCachedReferences(); UpdateVisuals(); } } }

    [Space(6)]

    [SerializeField]
    private float m_MinValue = 0;
    public float minValue { get { return m_MinValue; } set { if (SetStruct(ref m_MinValue, value)) { Set(m_Value); SetY(m_ValueY); UpdateVisuals(); } } }

    [SerializeField]
    private float m_MaxValue = 1;
    public float maxValue { get { return m_MaxValue; } set { if (SetStruct(ref m_MaxValue, value)) { Set(m_Value); SetY(m_ValueY); UpdateVisuals(); } } }

    [SerializeField]
    private bool m_WholeNumbers = false;
    public bool wholeNumbers { get { return m_WholeNumbers; } set { if (SetStruct(ref m_WholeNumbers, value)) { Set(m_Value); SetY(m_ValueY); UpdateVisuals(); } } }

    [SerializeField]
    private float m_Value = 1f;
    public float value
    {
        get
        {
            if (wholeNumbers)
                return Mathf.Round(m_Value);
            return m_Value;
        }
        set
        {
            Set(value);
        }
    }

    public float normalizedValue
    {
        get
        {
            if (Mathf.Approximately(minValue, maxValue))
                return 0;
            return Mathf.InverseLerp(minValue, maxValue, value);
        }
        set
        {
            this.value = Mathf.Lerp(minValue, maxValue, value);
        }
    }

    [SerializeField]
    private float m_ValueY = 1f;
    public float valueY
    {
        get
        {
            if (wholeNumbers)
                return Mathf.Round(m_ValueY);
            return m_ValueY;
        }
        set
        {
            SetY(value);
        }
    }

    public float normalizedValueY
    {
        get
        {
            if (Mathf.Approximately(minValue, maxValue))
                return 0;
            return Mathf.InverseLerp(minValue, maxValue, valueY);
        }
        set
        {
            this.valueY = Mathf.Lerp(minValue, maxValue, value);
        }
    }

    [Space(6)]

    // Allow for delegate-based subscriptions for faster events than 'eventReceiver', and allowing for multiple receivers.
    [SerializeField]
    private BoxSliderEvent m_OnValueChanged = new BoxSliderEvent();
    public BoxSliderEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

    // Private fields

    //private Image m_FillImage;
    //private Transform m_FillTransform;
    //private RectTransform m_FillContainerRect;
    private Transform m_HandleTransform;
    private RectTransform m_HandleContainerRect;

    // The offset from handle position to mouse down position
    private Vector2 m_Offset = Vector2.zero;

    private DrivenRectTransformTracker m_Tracker;

    // Size of each step.
    float stepSize { get { return wholeNumbers ? 1 : (maxValue - minValue) * 0.1f; } }


    // Dragging
    private Collider draggingCursor = null;

    private PokableBoxSlider()
    { }

    public void LayoutComplete()
    {

    }

    public void GraphicUpdateComplete()
    {

    }

    public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
    {
        if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
            return false;

        currentValue = newValue;
        return true;
    }

    public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
    {
        if (currentValue.Equals(newValue))
            return false;

        currentValue = newValue;
        return true;
    }

    private void OnEnable()
    {
        UpdateCachedReferences();
        Set(m_Value, false);
        SetY(m_ValueY, false);
        // Update rects since they need to be initialized correctly.
        UpdateVisuals();
    }

    private void OnDisable()
    {
        m_Tracker.Clear();
        //base.OnDisable();
    }

    void UpdateCachedReferences()
    {

        if (m_HandleRect)
        {
            m_HandleTransform = m_HandleRect.transform;
            if (m_HandleTransform.parent != null)
                m_HandleContainerRect = m_HandleTransform.parent.GetComponent<RectTransform>();
        }
        else
        {
            m_HandleContainerRect = null;
        }
    }

    // Set the valueUpdate the visible Image.
    void Set(float input)
    {
        Set(input, true);
    }

    void Set(float input, bool sendCallback)
    {
        // Clamp the input
        float newValue = Mathf.Clamp(input, minValue, maxValue);
        if (wholeNumbers)
            newValue = Mathf.Round(newValue);

        // If the stepped value doesn't match the last one, it's time to update
        if (m_Value.Equals(newValue))
            return;

        m_Value = newValue;
        UpdateVisuals();
        if (sendCallback)
            m_OnValueChanged.Invoke(newValue, valueY);
    }

    void SetY(float input)
    {
        SetY(input, true);
    }

    void SetY(float input, bool sendCallback)
    {
        // Clamp the input
        float newValue = Mathf.Clamp(input, minValue, maxValue);
        if (wholeNumbers)
            newValue = Mathf.Round(newValue);

        // If the stepped value doesn't match the last one, it's time to update
        if (m_ValueY.Equals(newValue))
            return;

        m_ValueY = newValue;
        UpdateVisuals();
        if (sendCallback)
            m_OnValueChanged.Invoke(value, newValue);
    }



    // Force-update the slider. Useful if you've changed the properties and want it to update visually.
    private void UpdateVisuals()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            UpdateCachedReferences();
#endif

        m_Tracker.Clear();


        //to business!
        if (m_HandleContainerRect != null)
        {
            m_Tracker.Add(this, m_HandleRect, DrivenTransformProperties.Anchors);
            Vector2 anchorMin = Vector2.zero;
            Vector2 anchorMax = Vector2.one;
            anchorMin[0] = anchorMax[0] = (normalizedValue);
            anchorMin[1] = anchorMax[1] = (normalizedValueY);

            m_HandleRect.anchorMin = anchorMin;
            m_HandleRect.anchorMax = anchorMax;
        }
    }

    // Update the slider's position based on the mouse.
    void UpdateDrag(Vector3 cursorWorldPos)
    {
        RectTransform clickRect = m_HandleContainerRect;
        Vector3 localPos = clickRect.InverseTransformPoint(cursorWorldPos);
        //m_Offset = localPos;
        //Debug.Log(localPos);

        //TODO
        if (clickRect != null && clickRect.rect.size[0] > 0)
        {
            Vector2 localCursor = localPos;
            //if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(clickRect, eventData.position, cam, out localCursor))
            //    return;
            localCursor -= clickRect.rect.position;

            float val = Mathf.Clamp01((localCursor)[0] / clickRect.rect.size[0]);
            normalizedValue = (val);

            float valY = Mathf.Clamp01((localCursor)[1] / clickRect.rect.size[1]);
            normalizedValueY = (valY);

            //m_OnValueChanged.Invoke(value, valueY);

        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("UICursor"))
        {
            UpdateDrag(other.transform.position);
            draggingCursor = other;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == draggingCursor)
        {
            draggingCursor = null;
        }
    }

    private void Update()
    {
        if (draggingCursor != null)
        {
            UpdateDrag(draggingCursor.transform.position);
        }
    }

}


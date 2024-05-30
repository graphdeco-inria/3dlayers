using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum SliderAxis
{
    x, y
}

[RequireComponent(typeof(PokableRect))]
public class PokableSlider : MonoBehaviour
{
    public RectTransform Handle;
    public SliderAxis axis = SliderAxis.x;

    public UnityEvent<float> onValueChanged;

    //private UnityEvent<float> m_OnValueChanged = new UnityEvent<float>();
    //public UnityEvent<float> onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

    public float minValue = 0f;
    public float maxValue = 1f;

    public float normalizedValue;
    private Collider draggingCursor;
    private RectTransform m_HandleContainerRect;

    public void InitValue(float value)
    {
        normalizedValue = (value - minValue) / (maxValue - minValue);
        UpdateVisuals();
    }

    private void Start()
    {
        m_HandleContainerRect = Handle.parent.GetComponent<RectTransform>();
        UpdateVisuals();
    }

    // Force-update the slider. Useful if you've changed the properties and want it to update visually.
    private void UpdateVisuals()
    {
        //to business!
        if (Handle != null)
        {
            Vector2 anchorMin = Vector2.zero;
            Vector2 anchorMax = Vector2.one;
            anchorMin[(int)axis] = anchorMax[(int)axis] = (normalizedValue);

            Handle.anchorMin = anchorMin;
            Handle.anchorMax = anchorMax;
        }
    }

    // Update the slider's position based on the mouse.
    void UpdateDrag(Vector3 cursorWorldPos)
    {
        RectTransform clickRect = m_HandleContainerRect;
        Vector3 localPos = clickRect.InverseTransformPoint(cursorWorldPos);
        //m_Offset = localPos;

        //TODO
        if (clickRect != null && clickRect.rect.size[0] > 0)
        {
            Vector2 localCursor = localPos;
            //if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(clickRect, eventData.position, cam, out localCursor))
            //    return;
            localCursor -= clickRect.rect.position;

            float val = Mathf.Clamp01((localCursor)[(int)axis] / clickRect.rect.size[(int)axis]);
            normalizedValue = (val);

            //m_OnValueChanged.Invoke(val);
            float realValue = normalizedValue * (maxValue - minValue) + minValue;
            onValueChanged.Invoke(realValue);
            UpdateVisuals();
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

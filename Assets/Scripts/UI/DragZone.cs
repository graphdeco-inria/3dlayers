using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;

[ExecuteInEditMode]
public class DragZone : MonoBehaviour
{
    public RectTransform itemsContainer;

    private LayoutElement layout;

    private float worldToLocalScaleFactor;
    private Vector3 dragStartPos;
    private Vector2 dragZoneDim;

    private void OnEnable()
    {
        layout = itemsContainer.GetComponent<LayoutElement>();

        Vector3[] corners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(corners);
        Vector2 dragZoneDimWorld = new Vector2(Mathf.Abs(corners[2].x - corners[0].x), Mathf.Abs(corners[2].y - corners[0].y));

        Vector3[] localCorners = new Vector3[4];
        GetComponent<RectTransform>().GetLocalCorners(localCorners);
        dragZoneDim = new Vector2(Mathf.Abs(localCorners[2].x - localCorners[0].x), Mathf.Abs(localCorners[2].y - localCorners[0].y));

        worldToLocalScaleFactor = dragZoneDim.y / dragZoneDimWorld.y;

        AlignBottom();

        //DragStart(Vector3.zero);
    }

    private void Start()
    {

    }

    private float GetItemsHeight()
    {
        Vector3[] corners = new Vector3[4];
        itemsContainer.GetLocalCorners(corners);
        // Min height is one layer button height
        return Mathf.Max(16f, Mathf.Abs(corners[2].y - corners[0].y));
        //return 16f + Mathf.Abs(corners[2].y - corners[0].y);
    }

    // Must call this whenever we add a new layer (scroll to the top layer)
    public void AlignTop()
    {
        float topPos = -GetItemsHeight() * 0.5f;
        itemsContainer.anchoredPosition = new Vector2(itemsContainer.anchoredPosition.x, topPos);
    }

    // Must call this whenever we open the layer panel (reset view)
    public void AlignBottom()
    {
        //Canvas.ForceUpdateCanvases();

        float bottomPos = -dragZoneDim.y + GetItemsHeight() * 0.5f;
        itemsContainer.anchoredPosition = new Vector2(itemsContainer.anchoredPosition.x, bottomPos);
    }

    public void AlignTo(float normalizedY)
    {
        // Only run it if there is overflowing content
        if (GetItemsHeight() < dragZoneDim.y)
        {
            AlignBottom();
            return;
        }
        float topPos = -GetItemsHeight() * 0.5f;
        float bottomPos = -dragZoneDim.y + GetItemsHeight() * 0.5f;
        float newContainerY = bottomPos + (topPos - bottomPos) * normalizedY;
        //Debug.Log($"top = {topPos} bottom = {bottomPos} newY = {newContainerY}");
        itemsContainer.anchoredPosition = new Vector2(itemsContainer.anchoredPosition.x, newContainerY);
    }

    public bool Draggable()
    {
        return GetItemsHeight() > dragZoneDim.y;
    }

    public void DragStart(Vector3 pos)
    {
        // Disable layout control
        layout.ignoreLayout = true;
        dragStartPos = pos;
    }

    public void DragUpdate(Vector3 dragPos)
    {
        // Compute the corresponding y offset
        float offset = Vector3.Dot(dragPos - dragStartPos, itemsContainer.up);

        Vector2 containerPos = itemsContainer.anchoredPosition;

        // Offset pos y
        float minY = -GetItemsHeight() * 0.5f;
        float maxY = - dragZoneDim.y + GetItemsHeight() * 0.5f;
        //Debug.Log($"value = {containerPos.y + offset * worldToLocalScaleFactor} min = {minY} max = {maxY}");
        float newPosY = Mathf.Clamp(containerPos.y + offset * worldToLocalScaleFactor, minY, maxY);
        itemsContainer.anchoredPosition = new Vector2(containerPos.x, newPosY);
        dragStartPos = dragPos;
    }
}

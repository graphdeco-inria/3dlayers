using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR.Haptics;

public class LayerButton : MonoBehaviour
{
    //private TextMeshProUGUI layerNameTextField;
    //private TextMeshProUGUI layerVisibilityTextField;

    public PokableTextButton MainButton { get; private set; }
    public PokableSpriteButton VisibilityButton { get; private set; }
    public PokableButton MoreButton { get; private set; }

    public PokableButton MoveToLayerButton { get; private set; }

    public PokableButton MoveUpButton { get; private set; }
    public PokableButton MoveDownButton { get; private set; }

    public int LayerUID { get; private set; }

    public void Init(int uid, bool visibilityToggle=true, bool moreButton=true, bool moveButton= false, bool moveUpButton=false, bool moveDownButton=false)
    {
        LayerUID = uid;

        PokableButton[] buttons = GetComponentsInChildren<PokableButton>(includeInactive:true);

        foreach(var b in buttons)
        {
            if (b.name == "VisibilityButton")
                VisibilityButton = (PokableSpriteButton)b;
            if (b.name == "LayerNameButton")
                MainButton = (PokableTextButton)b;
            if (b.name == "DetailsButton")
                MoreButton = b;
            if (b.name == "MoveSelectionToLayerButton")
                MoveToLayerButton = b;
            if (b.name == "MoveUpButton")
                MoveUpButton = b;
            if (b.name == "MoveDownButton")
                MoveDownButton = b;

        }


        VisibilityButton.gameObject.SetActive(visibilityToggle);
        MoreButton.gameObject.SetActive(moreButton);
        MoveToLayerButton.gameObject.SetActive(moveButton);
        MoveDownButton.gameObject.SetActive(moveDownButton);
        MoveUpButton.gameObject.SetActive(moveUpButton);
    }


    public void SetName(string name)
    {
        MainButton.SetLabel(name);
    }

    public void SetEmphasize(bool bold)
    {
        MainButton.SetEmphasize(bold);
    }

    public void SetVisibility(bool visible)
    {
        //VisibilityButton.SetLabel(visible ? "o" : "x");
        VisibilityButton.SetSprite(visible ? 0 : 1);
    }

    public void SetColor(Color c)
    {

        MainButton.SetTextColor(c);
    }

    public void ToggleMoveButton(bool active)
    {
        MoveToLayerButton.gameObject.SetActive(active);
        //if (active && MoveDownButton && MoveDownButton.gameObject.activeSelf)
        //    ToggleLayerReorder(false);

        // Button colliders must be updated
        MainButton.TriggerColliderUpdate();
    }

    public void ToggleLayerReorder(bool canReorder)
    {
        MoveDownButton.gameObject.SetActive(canReorder);
        MoveUpButton.gameObject.SetActive(canReorder);
        VisibilityButton.gameObject.SetActive(!canReorder);

        MainButton.TriggerColliderUpdate();
    }

}

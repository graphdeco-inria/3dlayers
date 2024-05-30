using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LayerSubPanel : MonoBehaviour
{
    [HideInInspector]
    public LayerPanel PanelManager;

    private int _elementUID;
    public int ElementUID
    {
        get { return _elementUID; }
        set
        {
            _elementUID = value;
            UpdateView();
        }
    }

    public abstract void UpdateView();

    public abstract void Clear();

}

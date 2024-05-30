using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CheatsheetViewer : MonoBehaviour
{

    public InputActionProperty ToggleViewAction;

    private void Start()
    {
        ToggleViewAction.action.started += ToggleView;
    }

    private void OnDisable()
    {
        ToggleViewAction.action.started -= ToggleView;
    }

    private void ToggleView(InputAction.CallbackContext ctx)
    {
        Debug.Log("toggle view");
        gameObject.GetComponent<MeshRenderer>().enabled = !gameObject.GetComponent<MeshRenderer>().enabled;
    }
}
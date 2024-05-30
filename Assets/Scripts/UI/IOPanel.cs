using System.Collections;
using UnityEngine;

public class IOPanel : MonoBehaviour
{

    public PokableButton confirmButton;
    public PokableButton cancelButton;

    public float ConfirmWaitDelay = 5f;

    private bool waitingForConfirmation = false;
    private float startConfirmWaitTime = 0f;

    // Use this for initialization
    void Start()
    {
        confirmButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);

        cancelButton.OnButtonClick.AddListener((e) => HandleConfirmation(false));
        confirmButton.OnButtonClick.AddListener((e) => HandleConfirmation(true));
    }

    private void Update()
    {
        //if (waitingForConfirmation)
        //    Debug.Log(Time.time - startConfirmWaitTime);
        if (waitingForConfirmation && Time.time - startConfirmWaitTime > ConfirmWaitDelay)
        {
            waitingForConfirmation = false;
            confirmButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
        }
    }

    public void ShowExportConfirmation()
    {
        confirmButton.gameObject.SetActive(true);
        cancelButton.gameObject.SetActive(true);
        startConfirmWaitTime = Time.time;
        waitingForConfirmation = true;
    }

    public void HandleConfirmation(bool confirmed)
    {
        if (confirmed)
        {
            SessionHistory.Instance.Write();
        }
        waitingForConfirmation = false;
        confirmButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);
    }
}
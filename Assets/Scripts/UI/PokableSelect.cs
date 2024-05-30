using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PokableSelect : PokableButton
{
    public List<string> options;
    public UnityEvent<int> onValueChanged;

    private int selectedOptionIdx = 0;

    private void Start()
    {
        base.OnButtonClick.AddListener(ToggleOption);
        GetComponentInChildren<TextMeshProUGUI>().text = options[selectedOptionIdx];
    }

    void ToggleOption(BaseEventData d)
    {
        selectedOptionIdx = (selectedOptionIdx + 1) % options.Count;
        //Debug.Log("selected " + selectedOptionIdx);
        GetComponentInChildren<TextMeshProUGUI>().text = options[selectedOptionIdx];
        onValueChanged.Invoke(selectedOptionIdx);
    }

    public void InitValue(int option)
    {
        selectedOptionIdx = option;
        GetComponentInChildren<TextMeshProUGUI>().text = options[selectedOptionIdx];
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }
}

using System.Collections;
using TMPro;
using UnityEngine;


public class PokableTextButton : PokableButton
{

    public string ButtonLabelText;

    private TextMeshProUGUI _buttonLabel;
    private TextMeshProUGUI ButtonLabel
    {
        get
        {
            if (_buttonLabel == null)
            {
                InitLabel();
            }
            return _buttonLabel;
        }
    }

    private void InitLabel()
    {
        if (_buttonLabel == null)
        {
            GameObject labelObj = new GameObject("Button Label");
            labelObj.transform.parent = transform;
            labelObj.transform.localPosition = Vector3.zero;
            labelObj.transform.localRotation = Quaternion.identity;
            labelObj.transform.localScale = Vector3.one;
            _buttonLabel = labelObj.AddComponent<TextMeshProUGUI>();
            _buttonLabel.color = Color.black;
            _buttonLabel.horizontalAlignment = HorizontalAlignmentOptions.Left;
            _buttonLabel.verticalAlignment = VerticalAlignmentOptions.Middle;
            _buttonLabel.text = ButtonLabelText;
            _buttonLabel.fontSize = 6;
            _buttonLabel.margin = new Vector4(2, 0, 2, 0);
            _buttonLabel.enableWordWrapping = false;
            _buttonLabel.overflowMode = TextOverflowModes.Ellipsis;

            //TextMeshPro textmeshPro = GetComponent<TextMeshPro>();
            _buttonLabel.faceColor = Color.white;
        }

    }

    void Start()
    {
        InitLabel();
    }

    public void SetLabel(string text)
    {
        ButtonLabelText = text;
        ButtonLabel.text = text;
    }

    public void SetEmphasize(bool important)
    {
        ButtonLabel.fontStyle = important ? FontStyles.Underline | FontStyles.Bold : FontStyles.Normal;
    }

    public void SetTextColor(Color color)
    {
        ButtonLabel.color = color;
    }
}
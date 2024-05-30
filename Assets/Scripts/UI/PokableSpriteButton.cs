
using UnityEngine;
using UnityEngine.UI;

public class PokableSpriteButton : PokableButton
{

    public Sprite[] sprites;
    private Image _buttonImage;
    private Image ButtonImage
    {
        get
        {
            if (_buttonImage == null)
            {
                InitSprite();
            }
            return _buttonImage;
        }
    }

    private void Start()
    {
        InitSprite();
    }

    private void InitSprite()
    {
        if (_buttonImage == null)
        {
            GameObject imageObj = new GameObject("Button Image");
            imageObj.transform.parent = transform;
            imageObj.transform.localPosition = Vector3.zero;
            imageObj.transform.localRotation = Quaternion.identity;
            imageObj.transform.localScale = Vector3.one;
            _buttonImage = imageObj.AddComponent<Image>();
            if (sprites.Length > 0)
                _buttonImage.sprite = sprites[0];

            AspectRatioFitter fitter = imageObj.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        }

    }

    public void SetSprite(int spriteIdx)
    {
        if (spriteIdx >= 0 && spriteIdx < sprites.Length && ButtonImage != null)
            ButtonImage.sprite = sprites[spriteIdx];
    }



}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UIIconItem : MonoBehaviour
{
    public Image mainImage;
    public Image secondImage;
    public Text mainText;
    private void Start()
    {
        
    }
    private void Update()
    {
        
    }
    public void SetMainIcon(string iconName,string text)
    {
        this.mainImage.overrideSprite = Resloader.Load<Sprite>(iconName);
        this.mainText.text = text;
    }

}

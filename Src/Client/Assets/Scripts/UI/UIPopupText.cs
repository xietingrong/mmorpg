using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UIPopupText: MonoBehaviour
{
    public Text normalDamageText;
    public Text critDamageText;
    public Text healText;
    public float floatTime = 0.5f;
    internal void InitPopup(PopupType type,float number,bool isCrit)
    {
        string text = number.ToString("0");
        normalDamageText.text = text;
        critDamageText.text = text;
        healText.text = text;

        normalDamageText.enabled = !isCrit && number<0;
        critDamageText.enabled = isCrit && number < 0;
        healText.enabled = number>0;
        float time = Random.Range(0f, 0.5f) + floatTime;
        float height = Random.Range(-0.5f, 0.5f);
        float disperse = Random.Range(-0.5f, 0.5f);
        disperse += Mathf.Sign(disperse) * 0.3f;

        LeanTween.moveX(this.gameObject, this.transform.position.x + disperse, time);
        LeanTween.moveZ(this.gameObject, this.transform.position.z + disperse, time);
        LeanTween.moveY(this.gameObject, this.transform.position.y + disperse, time).setEaseOutBack().setDestroyOnComplete(true);
    }
}


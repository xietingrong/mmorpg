using Battle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UIBuffItem: MonoBehaviour
{
    public Image icon;
    public Image overlay;
    public Text laber;
    Buff buff;
     void Start()
    {
        
    }
     void Update()
    {
        if (this.buff == null) return;
        if(this.buff.time >0)
        {
            if (!overlay.enabled) overlay.enabled = true;
            if (!laber.enabled) laber.enabled = true;
            overlay.fillAmount = this.buff.time / this.buff.Define.Duraction;
            this.laber.text = ((int)Math.Ceiling(this.buff.Define.Duraction - this.buff.time)).ToString();
        }
        else
        {
            if (overlay.enabled) overlay.enabled = false;
            if (laber.enabled) laber.enabled = false;
        }
       
    }
    internal void SetItem(Buff buff)
    {
        this.buff = buff;
        if(this.icon!= null)
        {
            this.icon.overrideSprite = Resloader.Load<Sprite>(this.buff.Define.Icon);
            this.icon.SetAllDirty();
        }
    }
}

